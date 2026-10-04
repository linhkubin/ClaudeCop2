using System.Linq;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using ClaudeCop.Core;
using ClaudeCop.Combat;

namespace ClaudeCop.UI.Tests
{
    public class UIPresenterTests
    {
        const string PrefabPath = "Assets/_Game/Prefabs/UI/GameplayUI.prefab";

        class FakeTarget : ITapTarget
        {
            public Vector3 Aim = new Vector3(0, 0, 10);
            public bool Targetable = true, Reticle = true;
            public float Progress;
            public int Id => 1; public TargetKind Kind => TargetKind.Enemy;
            public bool IsTargetable => Targetable; public Vector3 AimPoint => Aim;
            public bool HasJusticePoint => false; public Vector3 JusticePoint => Aim;
            public bool ShowsReticle => Reticle; public float ReticleProgress => Progress; public float ExposedTime => 0;
            public TapOutcome OnTapHit(ShotInfo s, bool j) => TapOutcome.Kill;
        }

        GameObject ui, camGo;
        // EditMode khong tu goi Awake/OnEnable/Start cho MonoBehaviour thuong: mo phong thu cong (cung thu tu Unity).
        static void Call(GameObject go, string method)
        {
            foreach (var mb in go.GetComponentsInChildren<MonoBehaviour>(true))
            {
                if (mb == null || mb.GetType().Namespace != "ClaudeCop.UI") continue;
                var m = mb.GetType().GetMethod(method, System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
                if (m != null) m.Invoke(mb, null);
            }
        }


        [SetUp]
        public void SetUp()
        {
            ResetState();
            camGo = new GameObject("TestCam", typeof(Camera)) { tag = "MainCamera" };
            camGo.transform.position = Vector3.zero;
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(PrefabPath);
            Assert.IsNotNull(prefab, "Thieu " + PrefabPath);
            ui = (GameObject)PrefabUtility.InstantiatePrefab(prefab);
            Call(ui, "Awake"); Call(ui, "OnEnable"); Call(ui, "Start");
        }

        [TearDown]
        public void TearDown()
        {
            if (ui != null) { Call(ui, "OnDisable"); Object.DestroyImmediate(ui); }
            if (camGo != null) Object.DestroyImmediate(camGo);
            ResetState();
        }

        static void ResetState()
        {
            GameEvents.RaiseScoreChanged(0);
            GameEvents.RaiseLivesChanged(0, 0);
            GameEvents.RaiseGameStateChanged(GameState.Title);
            CombatEvents.RaiseAmmoChanged(0, 0, WeaponKind.Pistol);
            CombatEvents.RaiseReloadStateChanged(false);
            TapShooter.PointerBlocker = null;
        }

        TMPro.TMP_Text TextNamed(string objName, string underRoot = null)
        {
            var root = underRoot == null ? ui.transform : ui.transform.Find(underRoot);
            return root.GetComponentsInChildren<TMPro.TMP_Text>(true).First(t => t.name == objName);
        }

        [Test]
        public void Hud_BindsScoreAmmoLivesAndReload()
        {
            GameEvents.RaiseScoreChanged(1500);
            Assert.AreEqual((1500).ToString("N0"), TextNamed("ScoreText", "HUD").text);

            CombatEvents.RaiseAmmoChanged(3, 6, WeaponKind.Pistol);
            Assert.AreEqual("3 / 6", TextNamed("AmmoText").text);

            CombatEvents.RaiseWeaponChanged(WeaponKind.Shotgun);
            Assert.AreEqual("Shotgun", TextNamed("WeaponLabel").text);

            GameEvents.RaiseLivesChanged(1, 3);
            var hearts = ui.transform.Find("HUD/SafeArea/Hearts").GetComponentsInChildren<Image>(true);
            var cfg = AssetDatabase.LoadAssetAtPath<UIConfig>("Assets/_Game/UI/UIConfig.asset");
            Assert.AreEqual(cfg.heartFull, hearts[0].color);
            Assert.AreEqual(cfg.heartEmpty, hearts[1].color);

            CombatEvents.RaiseReloadStateChanged(true);
            Assert.AreEqual("...", TextNamed("Label", "HUD/SafeArea/ReloadButton").text);
            CombatEvents.RaiseReloadStateChanged(false);
            Assert.AreEqual("RELOAD", TextNamed("Label", "HUD/SafeArea/ReloadButton").text);
        }

        [Test]
        public void PointerBlocker_SetOnEnable_ClearedOnDisable()
        {
            Assert.IsNotNull(TapShooter.PointerBlocker);
            Assert.DoesNotThrow(() => TapShooter.PointerBlocker(new Vector2(10, 10)));
            Call(ui, "OnDisable"); Object.DestroyImmediate(ui); ui = null;
            Assert.IsNull(TapShooter.PointerBlocker);
        }

        [Test]
        public void EndScreen_WinAndGameOver()
        {
            var win = ui.GetComponentInChildren<WinView>(true);
            var over = ui.GetComponentInChildren<GameOverView>(true);
            GameEvents.RaiseScoreChanged(900);
            GameEvents.RaiseGameStateChanged(GameState.Win);
            Assert.IsTrue(win.IsVisible); Assert.IsFalse(over.IsVisible);
            GameEvents.RaiseGameStateChanged(GameState.Playing);
            Assert.IsFalse(win.IsVisible);
            GameEvents.RaiseGameStateChanged(GameState.GameOver);
            Assert.IsTrue(over.IsVisible); Assert.IsFalse(win.IsVisible);
            Assert.AreEqual("SCORE  900", TextNamed("ScoreText", "GameOverPanel").text);
        }

        [Test]
        public void Reticle_FollowsTarget_HidesWhenBehindOrNotShown()
        {
            var presenter = ui.GetComponent<TargetReticlePresenter>();
            var ft = new FakeTarget { Progress = 0.9f };
            TargetRegistry.Register(ft);
            try
            {
                Assert.AreEqual(1, presenter.ActiveCount);
                var root = ui.GetComponentInChildren<HudView>(true).ReticleRoot;
                var v = root.GetComponentInChildren<TargetReticleView>(true);
                Invoke(presenter, "LateUpdate");
                Assert.IsTrue(v.gameObject.activeSelf);
                var img = v.GetComponentInChildren<Image>(true);
                var cfg = AssetDatabase.LoadAssetAtPath<UIConfig>("Assets/_Game/UI/UIConfig.asset");
                Assert.AreEqual(cfg.reticleRed, img.color);

                ft.Aim = new Vector3(0, 0, -10); // sau camera
                Invoke(presenter, "LateUpdate");
                Assert.IsFalse(v.gameObject.activeSelf);

                ft.Aim = new Vector3(0, 0, 10); ft.Reticle = false;
                Invoke(presenter, "LateUpdate");
                Assert.IsFalse(v.gameObject.activeSelf);
            }
            finally { TargetRegistry.Unregister(ft); }
            Assert.AreEqual(0, presenter.ActiveCount);
        }

        static void Invoke(object o, string method)
        {
            o.GetType().GetMethod(method, System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic).Invoke(o, null);
        }
    }
}
