using System.Collections.Generic;
using System.Reflection;
using ClaudeCop.Core;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;

namespace ClaudeCop.Enemy.Tests
{
    public class HostageBrainTests
    {
        static HostageBrain Make() => new HostageBrain(0.3f, 4f, 0.3f);

        [Test]
        public void StaysHiddenUntilActivated()
        {
            var b = Make(); b.Tick(10f);
            Assert.AreEqual(HostageState.Hidden, b.State);
        }

        [Test]
        public void PeekExposeRetreatLeave_NoReticle_NeverReturns()
        {
            var b = Make(); int ex = 0, ended = 0;
            b.ExposeStarted = () => ex++; b.ExposeEnded = () => ended++;
            b.Activate(); b.Tick(0.31f);
            Assert.AreEqual(HostageState.Exposed, b.State);
            Assert.IsTrue(b.IsTargetable);
            b.Tick(3.9f);
            Assert.IsTrue(b.IsTargetable);
            b.Tick(0.2f);
            Assert.AreEqual(HostageState.Retreating, b.State);
            Assert.AreEqual(1, ended);
            b.Tick(0.31f);
            Assert.AreEqual(HostageState.Left, b.State);
            b.Tick(20f);
            Assert.AreEqual(HostageState.Left, b.State);
            Assert.AreEqual(1, ex);
        }

        [Test]
        public void ShootOnlyWhenExposed_ThenShotAndFinished()
        {
            var b = Make();
            Assert.IsFalse(b.Shoot());
            b.Activate(); b.Tick(0.1f);
            Assert.IsFalse(b.Shoot(), "dang Peeking");
            b.Tick(0.3f);
            Assert.IsTrue(b.Shoot());
            Assert.AreEqual(HostageState.Shot, b.State);
            Assert.IsTrue(b.IsFinished);
        }
    }

    public class HostageActorTests
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

        static void Call(object o, string method) =>
            o.GetType().GetMethod(method, BindingFlags.NonPublic | BindingFlags.Instance).Invoke(o, null);

        HostageActor MakeHostage()
        {
            var go = new GameObject("TestHostage"); spawned.Add(go);
            return go.AddComponent<HostageActor>();
        }

        [Test]
        public void ExposedRegisters_NoReticle_ShotReturnsHostageHit_AndDisappears()
        {
            var h = MakeHostage();
            h.Activate();
            Assert.IsFalse(InRegistry(h));
            h.Tick(0.31f);
            Assert.AreEqual(HostageState.Exposed, h.State);
            Assert.IsTrue(InRegistry(h));
            Assert.AreEqual(TargetKind.Hostage, h.Kind);
            Assert.IsFalse(h.ShowsReticle);
            Assert.IsFalse(h.HasJusticePoint);
            Assert.AreEqual(TapOutcome.HostageHit, h.OnTapHit(default, false));
            Assert.IsFalse(InRegistry(h));
            Assert.IsFalse(h.gameObject.activeSelf);
            Assert.AreEqual(TapOutcome.Miss, h.OnTapHit(default, false));
        }

        [Test]
        public void LeavesByItselfAfterConfigTime_AndUnregisters()
        {
            var h = MakeHostage();
            int left = 0; h.Left += _ => left++;
            h.Activate(); h.Tick(0.31f);
            Assert.IsTrue(InRegistry(h));
            h.Tick(4.1f);
            Assert.IsFalse(InRegistry(h));
            h.Tick(0.31f);
            Assert.AreEqual(HostageState.Left, h.State);
            Assert.AreEqual(1, left);
            Assert.IsFalse(h.gameObject.activeSelf);
        }

        [Test]
        public void ReEnableWhileExposed_ReRegisters_F103()
        {
            var h = MakeHostage();
            h.Activate(); h.Tick(0.31f);
            Call(h, "OnDisable");
            Assert.IsFalse(InRegistry(h));
            Call(h, "OnEnable");
            Assert.IsTrue(InRegistry(h));
            Call(h, "OnDisable");
        }

        [Test]
        public void PausedHostageIsNotTargetable()
        {
            var h = MakeHostage();
            h.Activate(); h.Tick(0.31f);
            CombatPauseSignal.Push("t");
            try { Assert.IsFalse(h.IsTargetable); }
            finally { CombatPauseSignal.Pop("t"); }
            h.Dismiss();
            Assert.IsFalse(InRegistry(h));
        }
    }

    public class EncounterWaveHostageTests
    {
        readonly List<GameObject> spawned = new List<GameObject>();

        [TearDown]
        public void TearDown()
        {
            foreach (var g in spawned) if (g != null) Object.DestroyImmediate(g);
            spawned.Clear();
            for (int i = TargetRegistry.Targets.Count - 1; i >= 0; i--) TargetRegistry.Unregister(TargetRegistry.Targets[i]);
        }

        T Make<T>(string n) where T : Component
        {
            var go = new GameObject(n); spawned.Add(go);
            return go.AddComponent<T>();
        }

        static void SetList<T>(Object target, string field, params T[] items) where T : Object
        {
            var so = new SerializedObject(target);
            var arr = so.FindProperty(field); arr.arraySize = items.Length;
            for (int i = 0; i < items.Length; i++) arr.GetArrayElementAtIndex(i).objectReferenceValue = items[i];
            so.ApplyModifiedProperties();
        }

        EncounterWave MakeWave(EnemyActor[] enemies, HostageActor[] hostages)
        {
            var w = Make<EncounterWave>("Wave");
            SetList(w, "sceneEnemies", enemies);
            SetList(w, "sceneHostages", hostages);
            return w;
        }

        static void Kill(EnemyActor e)
        {
            e.Tick(0.31f);
            Assert.AreEqual(EnemyState.Aiming, e.State);
            e.OnTapHit(new ShotInfo { Direction = Vector3.forward }, false);
        }

        [Test]
        public void HostageIsInterleavedBetweenEnemies()
        {
            var e1 = Make<EnemyActor>("e1"); var e2 = Make<EnemyActor>("e2"); var e3 = Make<EnemyActor>("e3");
            var h = Make<HostageActor>("h");
            var w = MakeWave(new[] { e1, e2, e3 }, new[] { h });
            w.Begin();
            Assert.AreEqual(EnemyState.Peeking, e1.State);
            Assert.IsFalse(h.IsActivated);
            w.Tick(2f);   // e2
            Assert.AreEqual(EnemyState.Peeking, e2.State);
            Assert.IsFalse(h.IsActivated);
            w.Tick(2f);   // hostage (giua e2 va e3)
            Assert.IsTrue(h.IsActivated);
            Assert.AreEqual(EnemyState.Hidden, e3.State);
            w.Tick(2f);
            Assert.AreEqual(EnemyState.Peeking, e3.State);
        }

        [Test]
        public void HostageDoesNotCountForCleared_AndIsDismissedOnClear()
        {
            var e = Make<EnemyActor>("e"); var h = Make<HostageActor>("h");
            var w = MakeWave(new[] { e }, new[] { h });
            int cleared = 0; w.Cleared += (_, __) => cleared++;
            w.Begin();
            w.Tick(2f);             // enemy (hostage da lo tu Begin)
            Assert.IsTrue(h.IsActivated);
            h.Tick(0.31f);
            Assert.AreEqual(1, w.ActiveHostageCount);
            Kill(e);
            w.FlushClear();
            Assert.AreEqual(1, cleared, "con tin dang lo khong chan Cleared");
            Assert.AreEqual(0, w.ActiveHostageCount);
            Assert.IsTrue(h.gameObject.activeSelf, "con tin khong tat dot ngot");
            Assert.IsFalse(h.IsTargetable, "het dot: khong ban duoc nua");
            // L4-HOSTFIX (GDD vong 11): con tin khong dung im - tu cui xuong roi tat
            h.Tick(1f);
            Assert.IsFalse(h.gameObject.activeSelf, "con tin dang lo khi het dot phai tu cui xuong, khong dung im");
        }

        [Test]
        public void ShotHostageDoesNotBlockOrBreakWave()
        {
            var e1 = Make<EnemyActor>("e1"); var e2 = Make<EnemyActor>("e2"); var h = Make<HostageActor>("h");
            var w = MakeWave(new[] { e1, e2 }, new[] { h });
            int cleared = 0; w.Cleared += (_, __) => cleared++;
            w.Begin();
            w.Tick(2f);   // hostage
            w.Tick(2f);   // e2
            h.Tick(0.31f);
            Assert.AreEqual(TapOutcome.HostageHit, h.OnTapHit(default, false));
            Kill(e1); Kill(e2);
            w.FlushClear();
            Assert.AreEqual(1, cleared);
        }

        [Test]
        public void ApplyReticleTime_AppliesToEnemiesBeforeActivation()
        {
            var e = Make<EnemyActor>("e");
            var w = MakeWave(new[] { e }, new HostageActor[0]);
            w.ApplyReticleTime(2f);
            w.Begin();
            e.Tick(0.31f);
            e.Tick(1f);
            Assert.AreEqual(0.5f, e.ReticleProgress, 0.02f);
        }

        [Test]
        public void JusticeAssignedToAboutOneThird()
        {
            var es = new EnemyActor[6];
            for (int i = 0; i < es.Length; i++) es[i] = Make<EnemyActor>("e" + i);
            var w = MakeWave(es, new HostageActor[0]);
            var cfg = ScriptableObject.CreateInstance<EnemyConfig>();
            cfg.justiceEnabled = true; cfg.justiceFraction = 0.34f;
            w.Configure(cfg);
            w.Begin();
            int n = 0;
            foreach (var e in es) if (e.JusticeEnabled) n++;
            Assert.AreEqual(2, n);
            Object.DestroyImmediate(cfg);
        }

        [Test]
        public void PresetAppliesTimingOverrides_AndCanDisableHostages()
        {
            var e = Make<EnemyActor>("e"); var h = Make<HostageActor>("h");
            var w = MakeWave(new[] { e }, new[] { h });
            var p = ScriptableObject.CreateInstance<EnemyPreset>();
            p.reticleTime = 3f; p.useHostages = false; p.justiceEnabled = false;
            w.ApplyPreset(p);
            w.Begin();
            Assert.AreEqual(0, w.Hostages.Count);
            e.Tick(0.31f); e.Tick(1.5f);
            Assert.AreEqual(0.5f, e.ReticleProgress, 0.02f);
            Object.DestroyImmediate(p);
        }
    }

    public class EnemyJusticeTests
    {
        GameObject go;
        [TearDown] public void TearDown() { if (go != null) Object.DestroyImmediate(go); for (int i = TargetRegistry.Targets.Count - 1; i >= 0; i--) TargetRegistry.Unregister(TargetRegistry.Targets[i]); }

        EnemyActor Make(bool justice)
        {
            go = new GameObject("E");
            var e = go.AddComponent<EnemyActor>();
            e.SetJustice(justice);
            return e;
        }

        [Test]
        public void JusticeHit_OnJusticeEnemy_IsJusticeKill_AndSurrenders()
        {
            var e = Make(true);
            e.Activate(); e.Tick(0.31f);
            Assert.IsTrue(e.HasJusticePoint);
            var o = e.OnTapHit(new ShotInfo { Direction = Vector3.forward }, true);
            Assert.AreEqual(TapOutcome.JusticeKill, o);
            Assert.IsTrue(e.IsDead);
            Assert.IsTrue(e.IsSurrendered);
            Assert.IsFalse(e.HasJusticePoint);
        }

        [Test]
        public void JusticeFlagOnNonJusticeEnemy_IsPlainKill()
        {
            var e = Make(false);
            e.Activate(); e.Tick(0.31f);
            Assert.IsFalse(e.HasJusticePoint);
            Assert.AreEqual(TapOutcome.Kill, e.OnTapHit(new ShotInfo { Direction = Vector3.forward }, true));
            Assert.IsFalse(e.IsSurrendered);
        }
    }
}
