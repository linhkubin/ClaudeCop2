using System.Collections.Generic;
using ClaudeCop.Core;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;

namespace ClaudeCop.Enemy.Tests
{
    public class EncounterWaveTests
    {
        readonly List<GameObject> spawned = new List<GameObject>();

        [TearDown]
        public void TearDown()
        {
            foreach (var g in spawned) if (g != null) Object.DestroyImmediate(g);
            spawned.Clear();
            TargetRegistry.Unregister(null);
        }

        EnemyActor MakeEnemy()
        {
            var go = new GameObject("TestEnemy"); spawned.Add(go);
            return go.AddComponent<EnemyActor>();
        }

        EncounterWave MakeWave(params EnemyActor[] enemies)
        {
            var go = new GameObject("TestWave"); spawned.Add(go);
            var w = go.AddComponent<EncounterWave>();
            var so = new SerializedObject(w);
            var arr = so.FindProperty("sceneEnemies"); arr.arraySize = enemies.Length;
            for (int i = 0; i < enemies.Length; i++) arr.GetArrayElementAtIndex(i).objectReferenceValue = enemies[i];
            so.ApplyModifiedProperties();
            return w;
        }

        [Test]
        public void Cleared_IsDeferredUntilFlush_AfterLastKill()
        {
            var e = MakeEnemy(); var w = MakeWave(e);
            int cleared = 0; w.Cleared += (_, __) => cleared++;
            w.Begin();
            e.Tick(0.31f);
            Assert.AreEqual(EnemyState.Aiming, e.State);
            var outcome = e.OnTapHit(new ShotInfo { Direction = Vector3.forward }, false);
            Assert.AreEqual(TapOutcome.Kill, outcome);
            Assert.AreEqual(0, cleared, "Cleared khong duoc phat dong bo trong OnTapHit");
            w.FlushClear();
            Assert.AreEqual(1, cleared);
            Assert.IsTrue(w.IsCleared);
            w.FlushClear();
            Assert.AreEqual(1, cleared, "chi phat mot lan");
        }

        [Test]
        public void PreSpawn_CreatesInactiveEnemies_BeginOnlyActivates()
        {
            var tmpl = MakeEnemy(); tmpl.gameObject.SetActive(false);
            var sp = new GameObject("EnemySpawn_P1_W1_01"); spawned.Add(sp);
            var wgo = new GameObject("SpawnWave"); spawned.Add(wgo);
            var w = wgo.AddComponent<EncounterWave>();
            var so = new SerializedObject(w);
            so.FindProperty("enemyPrefab").objectReferenceValue = tmpl;
            var pts = so.FindProperty("spawnPoints"); pts.arraySize = 1;
            pts.GetArrayElementAtIndex(0).objectReferenceValue = sp.transform;
            so.ApplyModifiedProperties();

            w.PreSpawn();
            Assert.AreEqual(2, wgo.transform.childCount + 1, "enemy da duoc tao truoc Begin");
            var child = wgo.transform.GetChild(0).gameObject;
            Assert.IsFalse(child.activeSelf, "pre-spawn phai tat san");
            w.PreSpawn();
            Assert.AreEqual(1, wgo.transform.childCount, "PreSpawn idempotent");

            w.Begin();
            Assert.AreEqual(1, wgo.transform.childCount, "Begin khong Instantiate them");
            Assert.IsTrue(child.activeSelf);
            Assert.AreEqual(1, w.Enemies.Count);
        }

        [Test]
        public void EmptyWave_ClearsOnFlush_NotInBegin()
        {
            var w = MakeWave();
            int cleared = 0; w.Cleared += (_, __) => cleared++;
            w.Begin();
            Assert.AreEqual(0, cleared);
            w.FlushClear();
            Assert.AreEqual(1, cleared);
        }

        [Test]
        public void DestroyedEnemy_DoesNotBlockWave()
        {
            var a = MakeEnemy(); var b = MakeEnemy(); var w = MakeWave(a, b);
            int cleared = 0; w.Cleared += (_, __) => cleared++;
            w.Begin();
            a.Tick(0.31f);
            Object.DestroyImmediate(b.gameObject);   // b chua kich hoat, bi huy
            a.OnTapHit(new ShotInfo { Direction = Vector3.forward }, false);
            w.FlushClear();
            Assert.AreEqual(1, cleared);
        }

        [Test]
        public void NotCleared_WhileEnemyStillNotActivated()
        {
            var a = MakeEnemy(); var b = MakeEnemy(); var w = MakeWave(a, b);
            int cleared = 0; w.Cleared += (_, __) => cleared++;
            w.Begin();
            a.Tick(0.31f);
            a.OnTapHit(new ShotInfo { Direction = Vector3.forward }, false);
            w.FlushClear();
            Assert.AreEqual(0, cleared);
            w.Tick(2f);   // kich hoat b
            Assert.AreEqual(EnemyState.Peeking, b.State);
        }
    }
}
