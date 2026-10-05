using System.Linq;
using System.Collections.Generic;
using ClaudeCop.Core;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;

namespace ClaudeCop.Enemy.Tests
{
    public class GrenadeTests
    {
        sealed class Recorder : IPlayerDamageReceiver
        {
            public int hits; public DamageSource last;
            public void Damage(DamageSource s, Vector3 p) { hits++; last = s; }
        }

        readonly List<GameObject> spawned = new List<GameObject>();
        Recorder rec;

        [SetUp]
        public void SetUp()
        {
            rec = new Recorder();
            PlayerDamageService.Register(rec);
        }

        [TearDown]
        public void TearDown()
        {
            PlayerDamageService.Unregister(rec);
            while (CombatPauseSignal.IsPaused) CombatPauseSignal.Pop("t");
            foreach (var g in spawned) if (g != null) Object.DestroyImmediate(g);
            spawned.Clear();
        }

        Grenade Make()
        {
            var go = new GameObject("Grenade"); spawned.Add(go);
            var g = go.AddComponent<Grenade>();
            g.Launch(Vector3.zero, new Vector3(0, 0, 10), EnemyConfig.Fallback);
            return g;
        }

        [Test]
        public void Flight_ProgressAndArc()
        {
            var f = new GrenadeFlight(Vector3.zero, new Vector3(0, 0, 10), 1.5f, 1.5f);
            Assert.AreEqual(0f, f.Progress);
            f.Tick(0.75f);
            Assert.AreEqual(0.5f, f.Progress, 1e-4f);
            Assert.AreEqual(5f, f.Position.z, 1e-3f);
            Assert.AreEqual(1.5f, f.Position.y, 1e-3f, "dinh cung = arcHeight");
            Assert.IsFalse(f.Tick(0.1f) && false);
            Assert.IsTrue(f.Tick(1f));
            Assert.AreEqual(1f, f.Progress);
            Assert.AreEqual(GrenadeState.Exploded, f.State);
            Assert.IsFalse(f.Tick(1f), "chi bao den noi mot lan");
        }

        [Test]
        public void Grenade_DefaultsComeFromConfig()
        {
            var c = EnemyConfig.Fallback;
            Assert.AreEqual(1.5f, c.grenadeFlightTime);
            Assert.AreEqual(1.5f, c.grenadeArcHeight);
            Assert.AreEqual(1.5f, c.grenadeLandDistance);
        }

        [Test]
        public void Grenade_RegistersAsGrenadeTarget_WithProgress()
        {
            var g = Make();
            Assert.IsTrue(TargetRegistry.Targets.Contains(g));
            Assert.AreEqual(TargetKind.Grenade, g.Kind);
            Assert.IsTrue(g.ShowsReticle);
            Assert.IsFalse(g.HasJusticePoint);
            g.Tick(0.75f);
            Assert.AreEqual(0.5f, g.ReticleProgress, 1e-4f);
        }

        [Test]
        public void Grenade_TimeoutDamagesPlayerExactlyOnce()
        {
            var g = Make();
            int resolved = 0; bool shot = true;
            g.Resolved += (_, s) => { resolved++; shot = s; };
            g.Tick(1.4f);
            Assert.AreEqual(0, rec.hits);
            g.Tick(0.2f);
            g.Tick(1f);
            Assert.AreEqual(1, rec.hits);
            Assert.AreEqual(DamageSource.Explosion, rec.last);
            Assert.AreEqual(1, resolved);
            Assert.IsFalse(shot);
            Assert.IsFalse(TargetRegistry.Targets.Contains(g));
        }

        [Test]
        public void Grenade_DisabledWhileFlying_ResolvesWithoutDamage()
        {
            var g = Make();
            int resolved = 0;
            g.Resolved += (_, __) => resolved++;
            g.Tick(0.5f);
            g.Abort();   // OnDisable goi Abort(); OnDisable khong chay o EditMode
            Assert.AreEqual(1, resolved);
            Assert.IsTrue(g.IsResolved);
            Assert.IsFalse(TargetRegistry.Targets.Contains(g));
            g.Abort();
            g.Tick(5f);
            Assert.AreEqual(1, resolved, "khong resolve lan 2");
            Assert.AreEqual(0, rec.hits, "khong gay sat thuong");
            Assert.IsFalse(TargetRegistry.Targets.Contains(g));
        }

        [Test]
        public void Grenade_ShotDown_NoDamage_Kill()
        {
            var g = Make();
            bool shot = false;
            g.Resolved += (_, s) => shot = s;
            g.Tick(0.5f);
            var o = g.OnTapHit(new ShotInfo { Direction = Vector3.forward }, false);
            Assert.AreEqual(TapOutcome.Kill, o);
            Assert.IsTrue(shot);
            g.Tick(5f);
            Assert.AreEqual(0, rec.hits);
            Assert.IsFalse(TargetRegistry.Targets.Contains(g));
            Assert.AreEqual(TapOutcome.Miss, g.OnTapHit(new ShotInfo(), false), "khong ban 2 lan");
        }

        [Test]
        public void Grenade_StopsWhilePaused()
        {
            var g = Make();
            g.Tick(0.3f);
            float p = g.Progress;
            CombatPauseSignal.Push("t");
            g.Tick(5f);
            Assert.AreEqual(p, g.Progress, 1e-6f);
            Assert.AreEqual(0, rec.hits);
            Assert.IsFalse(g.IsTargetable);
            CombatPauseSignal.Pop("t");
            g.Tick(0.3f);
            Assert.Greater(g.Progress, p);
        }

        [Test]
        public void Target_InFrontOfCamera_OrFallback()
        {
            var c = EnemyConfig.Fallback;
            var t = EnemyActor.ComputeGrenadeTarget(Vector3.zero, Vector3.forward, true, new Vector3(0, 1.6f, 0), Vector3.back, c);
            Assert.AreEqual(-1.5f, t.z, 1e-4f);
            Assert.AreEqual(1.6f + c.grenadeLandHeightOffset, t.y, 1e-4f);
            var f = EnemyActor.ComputeGrenadeTarget(Vector3.zero, Vector3.forward, false, Vector3.zero, Vector3.forward, c);
            Assert.AreEqual(c.grenadeFallbackDistance, f.z, 1e-4f);
        }

        // ---------- Wave ----------

        [Test]
        public void Wave_NotCleared_UntilGrenadeResolved()
        {
            var gp = new GameObject("GrenadePrefab"); spawned.Add(gp);
            gp.SetActive(false);
            var prefab = gp.AddComponent<Grenade>();

            var ego = new GameObject("Grenadier"); spawned.Add(ego);
            var e = ego.AddComponent<EnemyActor>();
            e.ConfigureGrenadier(prefab);

            var wgo = new GameObject("Wave"); spawned.Add(wgo);
            var w = wgo.AddComponent<EncounterWave>();
            var so = new SerializedObject(w);
            var arr = so.FindProperty("sceneEnemies"); arr.arraySize = 1;
            arr.GetArrayElementAtIndex(0).objectReferenceValue = e;
            so.ApplyModifiedProperties();

            int cleared = 0; w.Cleared += (_, __) => cleared++;
            w.Begin();
            e.Tick(0.31f);                       // Aiming
            Assert.AreEqual(EnemyState.Aiming, e.State);
            e.Tick(EnemyConfig.Fallback.reticleTime + 0.1f);   // vong het -> nem
            Assert.AreEqual(1, w.PendingGrenadeCount);
            Assert.AreEqual(0, rec.hits, "grenadier nem, khong ban");
            Grenade thrown = null;
            foreach (var t in TargetRegistry.Targets) if (t is Grenade gr) thrown = gr;
            Assert.IsNotNull(thrown);
            spawned.Add(thrown.gameObject);

            // Enemy lui, nap, lo lai (ngan hon thoi gian bay) roi bi ban trong luc lua dan dang bay.
            var c = EnemyConfig.Fallback;
            e.Tick(c.retreatDuration + 0.01f);
            e.Tick(c.hideTime + 0.01f);
            e.Tick(c.peekDuration + 0.01f);
            Assert.AreEqual(EnemyState.Aiming, e.State);
            thrown.Tick(c.retreatDuration + c.hideTime + c.peekDuration);
            Assert.IsTrue(thrown.IsFlying);
            e.OnTapHit(new ShotInfo { Direction = Vector3.forward }, false);
            w.FlushClear();
            Assert.AreEqual(0, cleared, "con luu dan dang bay");

            thrown.OnTapHit(new ShotInfo(), false);
            Assert.AreEqual(0, w.PendingGrenadeCount);
            w.FlushClear();
            Assert.AreEqual(1, cleared);
        }
    }
}
