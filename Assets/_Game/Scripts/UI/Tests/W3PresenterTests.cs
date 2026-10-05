using System;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using ClaudeCop.Core;
using ClaudeCop.Ads;

namespace ClaudeCop.UI.Tests
{
    /// <summary>EditMode: RevivePopup, PhaseTransition, FloatingScore, Title (T-311/T-312). Mo phong Awake/OnEnable bang reflection.</summary>
    public class W3PresenterTests
    {
        const string Dir = "Assets/_Game/Prefabs/UI/";

        class StubAd : IRewardedAd
        {
            public bool Ready = true; public int ShowCount; public Action Rewarded, Failed;
            public bool IsReady => Ready;
            public void Show(Action onRewarded, Action onFailed) { ShowCount++; Rewarded = onRewarded; Failed = onFailed; }
        }

        GameObject go, camGo;
        bool prevReduce; int declines, revives, starts;

        static void Call(GameObject g, string method)
        {
            foreach (var mb in g.GetComponentsInChildren<MonoBehaviour>(true))
            {
                if (mb == null || mb.GetType().Namespace != "ClaudeCop.UI") continue;
                var m = mb.GetType().GetMethod(method, System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
                if (m != null) m.Invoke(mb, null);
            }
        }

        GameObject Spawn(string prefab)
        {
            var p = AssetDatabase.LoadAssetAtPath<GameObject>(Dir + prefab + ".prefab");
            Assert.IsNotNull(p, "Thieu " + prefab);
            go = (GameObject)PrefabUtility.InstantiatePrefab(p);
            Call(go, "Awake"); Call(go, "OnEnable");
            return go;
        }

        void OnDecline() => declines++;
        void OnRevive() => revives++;
        void OnStart() => starts++;

        [SetUp]
        public void SetUp()
        {
            Time.timeScale = 1f;
            prevReduce = UserSettings.ReduceMotion;
            declines = revives = starts = 0;
            GameEvents.RaiseGameStateChanged(GameState.Playing);
            GameEvents.RaiseReviveAvailabilityChanged(1);
            GameCommands.ReviveDeclined += OnDecline;
            GameCommands.ReviveRequested += OnRevive;
            GameCommands.StartGameRequested += OnStart;
            camGo = new GameObject("TestCam", typeof(Camera)) { tag = "MainCamera" };
        }

        [TearDown]
        public void TearDown()
        {
            GameCommands.ReviveDeclined -= OnDecline;
            GameCommands.ReviveRequested -= OnRevive;
            GameCommands.StartGameRequested -= OnStart;
            if (go != null) { Call(go, "OnDisable"); UnityEngine.Object.DestroyImmediate(go); }
            if (camGo != null) UnityEngine.Object.DestroyImmediate(camGo);
            GameEvents.RaiseGameStateChanged(GameState.Title);
            GameEvents.RaiseReviveAvailabilityChanged(0);
            UserSettings.ReduceMotion = prevReduce;
            Time.timeScale = 1f;
        }

        // ---------- RevivePopup ----------
        RevivePopupPresenter OpenRevive(out RevivePopupView view, out StubAd ad)
        {
            Spawn("RevivePopup");
            var pr = go.GetComponent<RevivePopupPresenter>();
            view = go.GetComponent<RevivePopupView>();
            ad = new StubAd();
            pr.SetAd(ad);
            GameEvents.RaiseGameStateChanged(GameState.RevivePrompt);
            return pr;
        }

        [Test]
        public void Revive_Opens_On_RevivePrompt_And_Freezes_Time()
        {
            var pr = OpenRevive(out var view, out _);
            Assert.IsTrue(pr.IsOpen); Assert.IsTrue(view.IsVisible);
            Assert.AreEqual(10, view.ShownSeconds);
            Assert.AreEqual(0f, Time.timeScale);
            Assert.IsTrue(view.WatchInteractable);
            GameEvents.RaiseGameStateChanged(GameState.Playing);
            Assert.IsFalse(pr.IsOpen); Assert.IsFalse(view.IsVisible);
            Assert.AreEqual(1f, Time.timeScale);
        }

        [Test]
        public void Revive_Countdown_Timeout_Declines()
        {
            var pr = OpenRevive(out var view, out _);
            pr.Tick(4.2f);
            Assert.AreEqual(6, view.ShownSeconds);
            Assert.AreEqual(0, declines);
            pr.Tick(6f);
            Assert.AreEqual(1, declines); Assert.IsFalse(pr.IsOpen); Assert.AreEqual(1f, Time.timeScale);
        }

        [Test]
        public void Revive_Watch_Reward_Requests_Revive_And_Pauses_Countdown_During_Ad()
        {
            var pr = OpenRevive(out var view, out var ad);
            view.WatchButton.onClick.Invoke();
            Assert.AreEqual(1, ad.ShowCount); Assert.IsTrue(pr.IsAdRunning);
            float r = pr.Remaining;
            pr.Tick(5f);
            Assert.AreEqual(r, pr.Remaining, 1e-4f);
            Assert.IsFalse(view.SkipInteractable);
            ad.Rewarded();
            Assert.AreEqual(1, revives); Assert.AreEqual(0, declines); Assert.IsFalse(pr.IsOpen);
        }

        [Test]
        public void Revive_Ad_Failed_Keeps_Popup_Open()
        {
            var pr = OpenRevive(out var view, out var ad);
            view.WatchButton.onClick.Invoke();
            ad.Failed();
            Assert.IsTrue(pr.IsOpen); Assert.IsFalse(pr.IsAdRunning);
            Assert.IsTrue(view.WatchInteractable); Assert.IsTrue(view.SkipInteractable);
            Assert.AreEqual(0, revives); Assert.AreEqual(0, declines);
        }

        [Test]
        public void Revive_Skip_Declines()
        {
            var pr = OpenRevive(out var view, out _);
            view.SkipButton.onClick.Invoke();
            Assert.AreEqual(1, declines); Assert.IsFalse(pr.IsOpen);
        }

        [Test]
        public void Revive_Watch_Disabled_When_No_Revives_Left()
        {
            Spawn("RevivePopup");
            var pr = go.GetComponent<RevivePopupPresenter>(); var view = go.GetComponent<RevivePopupView>();
            pr.SetAd(new StubAd());
            GameEvents.RaiseReviveAvailabilityChanged(0);
            GameEvents.RaiseGameStateChanged(GameState.RevivePrompt);
            Assert.IsFalse(view.WatchInteractable); Assert.IsTrue(view.SkipInteractable);
        }

        // ---------- PhaseTransition ----------
        [Test]
        public void Phase_Transition_Fades_Black_And_Shows_Title()
        {
            Spawn("PhaseFade");
            var v = go.GetComponent<PhaseTransitionView>();
            RailEvents.RaisePhaseTransition("STAGE 1-2", 0.4f, 1f, 0.4f);
            Assert.IsTrue(v.IsPlaying); Assert.AreEqual("STAGE 1-2", v.TitleString);
            v.Tick(0.4f); Assert.AreEqual(1f, v.BlackAlpha, 0.01f);
            v.Tick(0.5f); Assert.AreEqual(1f, v.TitleAlpha, 0.01f);
            v.Tick(0.6f); Assert.AreEqual(0.75f, v.BlackAlpha, 0.05f);
            v.Tick(0.5f); Assert.IsFalse(v.IsPlaying); Assert.AreEqual(0f, v.BlackAlpha);
        }

        [Test]
        public void Phase_Started_Shows_Banner_Unless_Transition_Follows()
        {
            Spawn("PhaseFade");
            var v = go.GetComponent<PhaseTransitionView>(); var pr = go.GetComponent<PhaseTransitionPresenter>();
            RailEvents.RaisePhaseStarted(0, "STAGE 1-1");
            pr.FlushPending();
            Assert.IsTrue(v.IsPlaying); Assert.AreEqual(0f, v.BlackAlpha); Assert.AreEqual("STAGE 1-1", v.TitleString);
            v.Stop();
            RailEvents.RaisePhaseStarted(1, "STAGE 1-2");
            RailEvents.RaisePhaseTransition("STAGE 1-2", 0.4f, 1f, 0.4f);
            pr.FlushPending();
            v.Tick(0.4f);
            Assert.AreEqual(1f, v.BlackAlpha, 0.01f);   // van la fade den, khong bi banner de
        }

        [Test]
        public void Phase_Banner_Seamless_Shows_Title_Without_Black()
        {
            Spawn("PhaseFade");
            var v = go.GetComponent<PhaseTransitionView>(); var pr = go.GetComponent<PhaseTransitionPresenter>();
            RailEvents.RaisePhaseStarted(1, "STAGE 1-2");   // Phase sau: khong tu hien banner, cho PhaseBanner
            pr.FlushPending();
            Assert.IsFalse(v.IsPlaying);
            RailEvents.RaisePhaseBanner("STAGE 1-2");
            Assert.IsTrue(v.IsPlaying); Assert.AreEqual("STAGE 1-2", v.TitleString);
            for (int i = 0; i < 40; i++) { v.Tick(0.05f); Assert.AreEqual(0f, v.BlackAlpha, "khong bao gio co lop den"); }
            Assert.IsFalse(v.IsPlaying);   // tu an sau ~2 s
        }

        // ---------- FloatingScore ----------
        [Test]
        public void FloatingScore_Spawn_Pool_Is_Bounded_And_Recycles()
        {
            Spawn("FloatingScore");
            var v = go.GetComponent<FloatingScoreView>();
            GameEvents.RaiseScoreAwarded(100, new Vector3(0, 0, 10), false, 1f);
            Assert.AreEqual(1, v.ActiveCount);
            Assert.AreEqual("+100", v.LastSpawned.Text);
            for (int i = 0; i < 40; i++) GameEvents.RaiseScoreAwarded(100, new Vector3(0, 0, 10), false, 1f);
            Assert.LessOrEqual(v.PoolCount, 12);
            Assert.AreEqual(v.PoolCount, v.ActiveCount);
            v.Tick(5f);
            Assert.AreEqual(0, v.ActiveCount);
            GameEvents.RaiseScoreAwarded(100, new Vector3(0, 0, 10), false, 1f);
            Assert.AreEqual(1, v.ActiveCount);
            Assert.LessOrEqual(v.PoolCount, 12);
        }

        [Test]
        public void FloatingScore_Justice_And_Multiplier_Formatting()
        {
            Spawn("FloatingScore");
            var v = go.GetComponent<FloatingScoreView>();
            GameEvents.RaiseScoreAwarded(300, new Vector3(0, 0, 10), true, 3f);
            var it = v.LastSpawned;
            StringAssert.Contains("JUSTICE", it.Text);
            StringAssert.Contains("x3", it.Text);
            GameEvents.RaiseScoreAwarded(100, new Vector3(0, 0, 10), false, 1f);
            Assert.Greater(it.Label.fontSize, v.LastSpawned.Label.fontSize);
            Assert.AreNotEqual(it.Label.color, v.LastSpawned.Label.color);
        }

        // ---------- Title ----------
        [Test]
        public void Title_Toggle_Writes_ReduceMotion_And_Syncs_Back()
        {
            Spawn("TitlePanel");
            var v = go.GetComponent<TitleView>();
            UserSettings.ReduceMotion = false;
            Assert.IsFalse(v.ReduceMotionOn);
            v.ReduceMotionToggle.isOn = true;
            Assert.IsTrue(UserSettings.ReduceMotion);
            UserSettings.ReduceMotion = false;
            Assert.IsFalse(v.ReduceMotionOn);
        }

        [Test]
        public void Title_Reads_Snapshot_On_Enable()
        {
            UserSettings.ReduceMotion = true;
            Spawn("TitlePanel");
            Assert.IsTrue(go.GetComponent<TitleView>().ReduceMotionOn);
        }

        [Test]
        public void Title_Start_Requests_Start_Game()
        {
            Spawn("TitlePanel");
            go.GetComponent<TitleView>().StartButton.onClick.Invoke();
            Assert.AreEqual(1, starts);
        }

        // ---------- chung ----------
        [TestCase("RevivePopup")]
        [TestCase("PhaseFade")]
        [TestCase("FloatingScore")]
        [TestCase("TitlePanel")]
        public void Decorative_Graphics_Do_Not_Raycast(string prefab)
        {
            Spawn(prefab);
            foreach (var t in go.GetComponentsInChildren<TMPro.TMP_Text>(true)) Assert.IsFalse(t.raycastTarget, t.name);
            if (prefab == "PhaseFade" || prefab == "FloatingScore")
                foreach (var g in go.GetComponentsInChildren<UnityEngine.UI.Graphic>(true)) Assert.IsFalse(g.raycastTarget, g.name);
        }
    }
}
