using System.Collections.Generic;
using ClaudeCop.Core;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;

namespace ClaudeCop.Combat.Tests
{
    public class ComboTrackerTests
    {
        [Test]
        public void HitsIncreaseMultiplier_CappedAtFive()
        {
            var c = new ComboTracker(5, 1);
            Assert.AreEqual(1f, c.Multiplier);
            float[] expected = { 1, 2, 3, 4, 5, 5, 5 };
            for (int i = 0; i < expected.Length; i++)
            {
                c.Apply(i % 2 == 0 ? TapOutcome.Kill : TapOutcome.JusticeKill);
                Assert.AreEqual(expected[i], c.Multiplier, "hit " + (i + 1));
            }
        }

        [Test]
        public void MissHostageAndPlainEnvironmentReset()
        {
            foreach (var o in new[] { TapOutcome.Miss, TapOutcome.HostageHit, TapOutcome.Environment })
            {
                var c = new ComboTracker();
                c.Apply(TapOutcome.Kill); c.Apply(TapOutcome.Kill);
                Assert.IsTrue(c.Apply(o), o.ToString());
                Assert.AreEqual(0, c.Streak);
                Assert.AreEqual(1f, c.Multiplier);
            }
        }

        [Test]
        public void ShootableEnvironmentAndPickupKeepCombo()
        {
            var c = new ComboTracker();
            c.Apply(TapOutcome.Kill); c.Apply(TapOutcome.Kill);
            Assert.IsFalse(c.Apply(TapOutcome.Environment, true));
            Assert.IsFalse(c.Apply(TapOutcome.PickupCollected));
            Assert.AreEqual(2, c.Streak);
        }

        [Test]
        public void HitsPerStepSlowsGrowth()
        {
            var c = new ComboTracker(5, 2);
            c.Apply(TapOutcome.Kill); Assert.AreEqual(1f, c.Multiplier);
            c.Apply(TapOutcome.Kill); Assert.AreEqual(1f, c.Multiplier);
            c.Apply(TapOutcome.Kill); Assert.AreEqual(2f, c.Multiplier);
        }
    }

    public class ComboSystemTests
    {
        GameObject go;
        [TearDown] public void TearDown() { if (go != null) Object.DestroyImmediate(go); CombatEvents.RaiseComboChanged(0, 1f); }

        [Test]
        public void RegisterShot_RaisesComboChanged_AndPlayerDamagedResets()
        {
            go = new GameObject("Combo");
            var cs = go.AddComponent<ComboSystem>();
            int lastStreak = -1; float lastMult = 0f; int raised = 0;
            System.Action<int, float> h = (s, m) => { lastStreak = s; lastMult = m; raised++; };
            CombatEvents.ComboChanged += h;
            try
            {
                Assert.AreEqual(1f, cs.RegisterShot(TapOutcome.Kill));
                Assert.AreEqual(2f, cs.RegisterShot(TapOutcome.Kill));
                Assert.AreEqual(2, lastStreak); Assert.AreEqual(2f, lastMult);
                Assert.AreEqual(2f, CombatEvents.Current.ComboMultiplier);
                cs.ResetCombo(); // OnEnable khong chay o EditMode: PlayerDamaged goi OnPlayerDamaged -> ResetCombo
                Assert.AreEqual(0, lastStreak); Assert.AreEqual(1f, lastMult);
                Assert.AreEqual(1f, cs.RegisterShot(TapOutcome.Miss));
            }
            finally { CombatEvents.ComboChanged -= h; }
        }
    }

    public class WeaponPickupTests
    {
        readonly List<GameObject> spawned = new List<GameObject>();
        [TearDown]
        public void TearDown()
        {
            foreach (var g in spawned) if (g != null) Object.DestroyImmediate(g);
            spawned.Clear();
            for (int i = TargetRegistry.Targets.Count - 1; i >= 0; i--) TargetRegistry.Unregister(TargetRegistry.Targets[i]);
        }

        static bool InRegistry(ITapTarget t)
        {
            for (int i = 0; i < TargetRegistry.Targets.Count; i++) if (TargetRegistry.Targets[i] == t) return true;
            return false;
        }

        static WeaponData W(WeaponKind k, int mag, bool hold = false)
        {
            var w = ScriptableObject.CreateInstance<WeaponData>();
            w.Configure(k, mag, 90f, 1, 10f, 0.5f, 1f, hold);
            return w;
        }

        WeaponPickup MakePickup(WeaponData w, Vector3 pos)
        {
            var go = new GameObject("Pickup"); spawned.Add(go);
            go.transform.position = pos;
            var p = go.AddComponent<WeaponPickup>();
            p.Setup(w);
            return p;
        }

        [Test]
        public void ShowRegisters_TapCollects_AndUnregisters()
        {
            var p = MakePickup(W(WeaponKind.Shotgun, 6), Vector3.zero);
            Assert.IsFalse(p.IsTargetable);
            p.Show();
            Assert.IsTrue(InRegistry(p));
            Assert.AreEqual(TargetKind.Pickup, p.Kind);
            Assert.IsFalse(p.ShowsReticle);
            var o = p.OnTapHit(default, false);
            Assert.AreEqual(TapOutcome.PickupCollected, o);
            Assert.IsFalse(InRegistry(p));
            Assert.IsFalse(p.gameObject.activeSelf);
            Assert.AreEqual(TapOutcome.Miss, p.OnTapHit(default, false));
        }

        [Test]
        public void PausedPickupIsNotTargetable()
        {
            var p = MakePickup(W(WeaponKind.Shotgun, 6), Vector3.zero);
            p.Show();
            CombatPauseSignal.Push("t");
            try { Assert.IsFalse(p.IsTargetable); }
            finally { CombatPauseSignal.Pop("t"); }
        }

        // ---- Chon muc tieu: Pickup thap hon Enemy (F-107) ----
        sealed class Fake : ITapTarget
        {
            public int Id { get; set; }
            public TargetKind Kind { get; set; }
            public bool IsTargetable => true;
            public Vector3 AimPoint { get; set; }
            public bool HasJusticePoint => false;
            public Vector3 JusticePoint => AimPoint;
            public bool ShowsReticle => false;
            public float ReticleProgress => 0f;
            public float ExposedTime => 0f;
            public TapOutcome OnTapHit(ShotInfo s, bool j) => TapOutcome.Kill;
        }
        static System.Func<Vector3, Vector2?> Proj => w => new Vector2(w.x, w.y);
        static Fake T(TargetKind k, float x, int id) => new Fake { Id = id, Kind = k, AimPoint = new Vector3(x, 0, 1) };

        [Test]
        public void EnemyBeatsCloserPickup()
        {
            var res = new List<TargetHit>();
            var list = new List<ITapTarget> { T(TargetKind.Pickup, 5, 1), T(TargetKind.Enemy, 80, 2) };
            Assert.AreEqual(1, TargetSelector.Select(list, Vector2.zero, 90f, 35f, 1, Proj, res));
            Assert.AreEqual(TargetKind.Enemy, res[0].Target.Kind);
        }

        [Test]
        public void ShotgunDoesNotCollectPickupWhenEnemyInRange()
        {
            var res = new List<TargetHit>();
            var list = new List<ITapTarget> { T(TargetKind.Pickup, 5, 1), T(TargetKind.Enemy, 80, 2) };
            Assert.AreEqual(1, TargetSelector.Select(list, Vector2.zero, 180f, 35f, 5, Proj, res));
            Assert.AreEqual(TargetKind.Enemy, res[0].Target.Kind);
        }

        [Test]
        public void PickupSelectedWhenNoEnemy_AndBeatsHostage()
        {
            var res = new List<TargetHit>();
            var list = new List<ITapTarget> { T(TargetKind.Hostage, 3, 1), T(TargetKind.Pickup, 40, 2) };
            Assert.AreEqual(1, TargetSelector.Select(list, Vector2.zero, 90f, 35f, 1, Proj, res));
            Assert.AreEqual(TargetKind.Pickup, res[0].Target.Kind);
        }

        // ---- TapShooter: nhat thung khong ton dan, equip, het dan ve Pistol ----
        TapShooter MakeShooter(WeaponData start, out Camera cam)
        {
            var camGo = new GameObject("TestCam"); spawned.Add(camGo);
            camGo.tag = "MainCamera";
            cam = camGo.AddComponent<Camera>();
            camGo.transform.position = Vector3.zero;
            camGo.transform.rotation = Quaternion.identity;

            var go = new GameObject("Shooter"); spawned.Add(go);
            var s = go.AddComponent<TapShooter>();
            var so = new SerializedObject(s);
            so.FindProperty("startingWeapon").objectReferenceValue = start;
            so.ApplyModifiedProperties();
            s.Equip(start, true);
            return s;
        }

        Vector2 ScreenOf(Camera cam, Vector3 world) { var p = cam.WorldToScreenPoint(world); return new Vector2(p.x, p.y); }

        [Test]
        public void PickupTap_RefundsAmmo_EquipsWeapon_Refilled()
        {
            var pistol = W(WeaponKind.Pistol, 6);
            var shotgun = W(WeaponKind.Shotgun, 6);
            var shooter = MakeShooter(pistol, out var cam);
            var pos = new Vector3(0, 0, 8);
            var p = MakePickup(shotgun, pos - Vector3.up * 0.5f);
            p.Show();

            WeaponKind? changed = null;
            System.Action<WeaponKind> h = k => changed = k;
            CombatEvents.WeaponChanged += h;
            try
            {
                shooter.FireAt(ScreenOf(cam, p.AimPoint));
            }
            finally { CombatEvents.WeaponChanged -= h; }

            Assert.AreEqual(WeaponKind.Shotgun, shooter.CurrentWeapon.Kind);
            Assert.AreEqual(6, shooter.Ammo, "dan day bang vu khi moi");
            Assert.AreEqual(WeaponKind.Shotgun, changed);
        }

        [Test]
        public void SpecialWeaponEmpty_RevertsToStartingWeaponFull()
        {
            var pistol = W(WeaponKind.Pistol, 6);
            var shotgun = W(WeaponKind.Shotgun, 2);
            var shooter = MakeShooter(pistol, out var cam);
            shooter.Equip(shotgun, true);
            Assert.AreEqual(2, shooter.Ammo);
            var miss = new Vector2(10, 10);
            shooter.FireAt(miss);
            Assert.AreEqual(WeaponKind.Shotgun, shooter.CurrentWeapon.Kind);
            Assert.AreEqual(1, shooter.Ammo);
            shooter.FireAt(miss);
            Assert.AreEqual(WeaponKind.Pistol, shooter.CurrentWeapon.Kind);
            Assert.AreEqual(6, shooter.Ammo);
        }

        [Test]
        public void ComboMultiplierInShotResultIsAfterShot()
        {
            var pistol = W(WeaponKind.Pistol, 6);
            var shooter = MakeShooter(pistol, out var cam);
            var comboGo = new GameObject("Combo"); spawned.Add(comboGo);
            var combo = comboGo.AddComponent<ComboSystem>();
            var so = new SerializedObject(shooter);
            so.FindProperty("combo").objectReferenceValue = combo;
            so.ApplyModifiedProperties();

            var t = new KillTarget { Pos = new Vector3(0, 0, 8) };
            TargetRegistry.Register(t);
            var results = new List<ShotResult>();
            System.Action<ShotResult> h = r => results.Add(r);
            CombatEvents.ShotResolved += h;
            try
            {
                var sp = ScreenOf(cam, t.Pos);
                shooter.FireAt(sp);
                shooter.FireAt(sp);
                shooter.FireAt(new Vector2(5, 5)); // miss -> reset
            }
            finally { CombatEvents.ShotResolved -= h; TargetRegistry.Unregister(t); }

            Assert.AreEqual(3, results.Count);
            Assert.AreEqual(1f, results[0].ComboMultiplier);
            Assert.AreEqual(2f, results[1].ComboMultiplier);
            Assert.AreEqual(1f, results[2].ComboMultiplier);
            Assert.AreEqual(TapOutcome.Kill, results[0].Outcome);
        }

        sealed class KillTarget : ITapTarget
        {
            public Vector3 Pos;
            public int Id => 4242;
            public TargetKind Kind => TargetKind.Enemy;
            public bool IsTargetable => true;
            public Vector3 AimPoint => Pos;
            public bool HasJusticePoint => false;
            public Vector3 JusticePoint => Pos;
            public bool ShowsReticle => true;
            public float ReticleProgress => 0.2f;
            public float ExposedTime => 0.5f;
            public TapOutcome OnTapHit(ShotInfo s, bool j) => TapOutcome.Kill;
        }
    }
}
