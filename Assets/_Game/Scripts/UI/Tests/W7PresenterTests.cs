using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using ClaudeCop.Core;
using ClaudeCop.RankScore;

namespace ClaudeCop.UI.Tests
{
    /// <summary>EditMode: man Win rank (hai thu tu su kien), map rank-mau, restart xoa rank, chu bay NO! (T-711).</summary>
    public class W7PresenterTests
    {
        const string Dir = "Assets/_Game/Prefabs/UI/";
        GameObject go, camGo;
        bool prevReduce;

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
            Call(go, "Awake"); Call(go, "OnEnable"); Call(go, "Start");
            return go;
        }

        [SetUp]
        public void SetUp()
        {
            prevReduce = UserSettings.ReduceMotion;
            UserSettings.ReduceMotion = true; // hien thang, khong dong dau
            ScoreRankBoard.Clear();
            GameEvents.RaiseGameStateChanged(GameState.Playing);
            camGo = new GameObject("TestCam", typeof(Camera)) { tag = "MainCamera" };
        }

        [TearDown]
        public void TearDown()
        {
            if (go != null) { Call(go, "OnDisable"); Object.DestroyImmediate(go); }
            if (camGo != null) Object.DestroyImmediate(camGo);
            UserSettings.ReduceMotion = prevReduce;
            ScoreRankBoard.Clear();
            GameEvents.RaiseGameStateChanged(GameState.Title);
        }

        static ScoreRankResult Make(ScoreRank r, ScoreWeakness w) => new ScoreRankResult
        {
            Rank = r, Weakness = w, WeaknessText = ScoreRankEvaluator.TextFor(w),
            Accuracy = 0.8f, AvgReactionTime = 1.5f, DamageTaken = 2, HostageHits = 1, RevivesUsed = 0, BlastKills = 4
        };

        [Test]
        public void Win_RankBeforeState_ShowsRank()
        {
            Spawn("GameplayUI");
            var win = go.GetComponentInChildren<WinView>(true);
            ScoreRankBoard.Publish(Make(ScoreRank.S, ScoreWeakness.None));
            Assert.IsFalse(win.RankVisible);
            GameEvents.RaiseGameStateChanged(GameState.Win);
            Assert.IsTrue(win.RankVisible);
            Assert.AreEqual("S", win.RankString);
            Assert.AreEqual(UIConfig.Fallback.rankColorS, win.RankColor);
            StringAssert.Contains("Hoàn hảo", win.WeaknessString);
            StringAssert.Contains("80%", win.StatsString);
        }

        [Test]
        public void Win_RankAfterState_ShowsRankWhenEventArrives()
        {
            Spawn("GameplayUI");
            var win = go.GetComponentInChildren<WinView>(true);
            GameEvents.RaiseGameStateChanged(GameState.Win);
            Assert.IsTrue(win.IsVisible);
            Assert.IsFalse(win.RankVisible);
            ScoreRankBoard.Publish(Make(ScoreRank.B, ScoreWeakness.Accuracy));
            Assert.IsTrue(win.RankVisible);
            Assert.AreEqual("B", win.RankString);
            StringAssert.Contains("Bắn trượt", win.WeaknessString);
        }

        [Test]
        public void Win_NoRank_StillShowsScoreAndHidesRankBlock()
        {
            Spawn("GameplayUI");
            var win = go.GetComponentInChildren<WinView>(true);
            GameEvents.RaiseGameStateChanged(GameState.Win);
            var pr = go.GetComponentInChildren<EndScreenPresenter>(true);
            Assert.IsTrue(win.IsVisible);
            Assert.IsFalse(win.RankVisible);
            Assert.IsTrue(pr.WaitingRank);
        }

        [Test]
        public void Restart_ClearsRank()
        {
            Spawn("GameplayUI");
            var win = go.GetComponentInChildren<WinView>(true);
            ScoreRankBoard.Publish(Make(ScoreRank.A, ScoreWeakness.Reaction));
            GameEvents.RaiseGameStateChanged(GameState.Win);
            Assert.IsTrue(win.RankVisible);
            ScoreRankBoard.Clear();
            GameEvents.RaiseGameStateChanged(GameState.Playing);
            Assert.IsFalse(win.RankVisible);
            Assert.IsFalse(win.IsVisible);
            GameEvents.RaiseGameStateChanged(GameState.Win); // luot moi chua co rank
            Assert.IsFalse(win.RankVisible);
        }

        [Test]
        public void Rank_Maps_To_Letter_And_Color()
        {
            var c = UIConfig.Fallback;
            Assert.AreEqual("S", UIConfig.RankLetter(ScoreRank.S));
            Assert.AreEqual("A", UIConfig.RankLetter(ScoreRank.A));
            Assert.AreEqual("B", UIConfig.RankLetter(ScoreRank.B));
            Assert.AreEqual("C", UIConfig.RankLetter(ScoreRank.C));
            Assert.AreEqual(c.rankColorS, c.RankColor(ScoreRank.S));
            Assert.AreEqual(c.rankColorA, c.RankColor(ScoreRank.A));
            Assert.AreEqual(c.rankColorB, c.RankColor(ScoreRank.B));
            Assert.AreEqual(c.rankColorC, c.RankColor(ScoreRank.C));
            Assert.AreNotEqual(c.rankColorS, c.rankColorA);
            Assert.AreNotEqual(c.rankColorB, c.rankColorC);
        }

        [Test]
        public void Blast_SpawnsFloatingText_WithKillCount()
        {
            Spawn("FloatingScore");
            var v = go.GetComponent<FloatingScoreView>();
            BlastEvents.Raise(new BlastReport { Center = new Vector3(0, 0, 10), Radius = 3f, EnemiesKilled = 3 });
            Assert.AreEqual(1, v.ActiveCount);
            StringAssert.Contains("NỔ", v.LastSpawned.Text);
            StringAssert.Contains("x3", v.LastSpawned.Text);
        }

        [Test]
        public void DebugPanel_TracksPresetDropRank()
        {
            RankScoreDecisionLog.Clear();
            Spawn("RankScoreDebugPanel");
            var view = go.GetComponent<RankScoreDebugPanelView>();
            RankScoreDecisionLog.Record(new RankScoreDecision { Question = "wave_preset", Choice = "heavy", Confidence = 0.9f });
            RankScoreDecisionLog.Record(new RankScoreDecision { Question = "weapon_drop", Choice = "shotgun", Confidence = 0.9f });
            RankScoreDecisionLog.Record(new RankScoreDecision { Question = "rank", Choice = "A", Confidence = 0.9f });
            StringAssert.Contains("preset: heavy", view.HistoryString);
            StringAssert.Contains("drop: shotgun", view.HistoryString);
            StringAssert.Contains("rank: A", view.HistoryString);
            RankScoreDecisionLog.Clear();
        }
    }
}
