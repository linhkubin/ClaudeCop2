using System.Collections.Generic;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using ClaudeCop.Core;
using ClaudeCop.RankScore;

namespace ClaudeCop.UI.Tests
{
    /// <summary>EditMode: bang debug RankScore (T-411).</summary>
    public class RankScoreDebugPanelTests
    {
        GameObject go;
        RankScoreDebugPanelView view;
        RankScoreDebugPanelPresenter pr;

        static void Call(GameObject g, string method)
        {
            foreach (var mb in g.GetComponentsInChildren<MonoBehaviour>(true))
            {
                if (mb == null || mb.GetType().Namespace != "ClaudeCop.UI") continue;
                var m = mb.GetType().GetMethod(method, System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
                if (m != null) m.Invoke(mb, null);
            }
        }

        void Spawn()
        {
            var p = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/_Game/Prefabs/UI/RankScoreDebugPanel.prefab");
            Assert.IsNotNull(p, "Thieu RankScoreDebugPanel.prefab");
            go = (GameObject)PrefabUtility.InstantiatePrefab(p);
            Call(go, "Awake"); Call(go, "OnEnable");
            view = go.GetComponent<RankScoreDebugPanelView>();
            pr = go.GetComponent<RankScoreDebugPanelPresenter>();
        }

        [SetUp] public void SetUp() { RankScoreDecisionLog.Clear(); }

        [TearDown]
        public void TearDown()
        {
            if (go != null) { Call(go, "OnDisable"); Object.DestroyImmediate(go); }
            RankScoreDecisionLog.Clear();
        }

        static RankScoreDecision Make(string choice, float reticle, RankScoreDecisionSource src, float conf = 0.8f)
        {
            return new RankScoreDecision
            {
                Question = "reticle_time",
                Choice = choice,
                ReticleTime = reticle,
                Source = src,
                Confidence = conf,
                Timestamp = 12.5f,
                Probabilities = new Dictionary<string, float> { { "short", 0.1f }, { "normal", 0.2f }, { "long", 0.7f } },
                Stats = new PlayerStats { Shots = 10, Hits = 7, Misses = 3, Accuracy = 0.7f, AvgReactionTime = 0.45f, ReactionSamples = 6, DamageTaken = 1 }
            };
        }

        [Test]
        public void Hidden_By_Default_And_Button_Shown_In_Editor()
        {
            Spawn();
            Assert.IsFalse(view.IsVisible); Assert.IsFalse(pr.IsVisible);
            Assert.IsTrue(view.ToggleButtonVisible);
        }

        [Test]
        public void Toggle_Command_Shows_And_Hides()
        {
            Spawn();
            GameCommands.RequestDebugOverlayToggle();
            Assert.IsTrue(view.IsVisible);
            GameCommands.RequestDebugOverlayToggle();
            Assert.IsFalse(view.IsVisible);
        }

        [Test]
        public void Button_Click_Toggles()
        {
            Spawn();
            view.ToggleButton.onClick.Invoke();
            Assert.IsTrue(view.IsVisible);
            view.ToggleButton.onClick.Invoke();
            Assert.IsFalse(view.IsVisible);
        }

        [Test]
        public void Unbind_On_Disable_Ignores_Events()
        {
            Spawn();
            Call(go, "OnDisable");
            GameCommands.RequestDebugOverlayToggle();
            Assert.IsFalse(view.IsVisible);
            RankScoreDecisionLog.Record(Make("long", 3f, RankScoreDecisionSource.Offline));
            Assert.AreEqual(0, pr.HistoryCount);
            Call(go, "OnEnable"); // OnEnable lai de TearDown khong loi
        }

        [Test]
        public void Shows_Offline_Decision()
        {
            Spawn();
            RankScoreDecisionLog.Record(Make("long", 3f, RankScoreDecisionSource.Offline));
            StringAssert.Contains("long", view.ChoiceString);
            StringAssert.Contains("3", view.ChoiceString);
            StringAssert.Contains("Offline", view.SourceString);
            StringAssert.Contains("Accuracy 70%", view.StatsString);
            var offlineColor = view.SourceColor;

            RankScoreDecisionLog.Record(Make("normal", 2.5f, RankScoreDecisionSource.Default, 0.2f));
            StringAssert.Contains("Mặc định", view.SourceString);
            Assert.AreNotEqual(offlineColor, view.SourceColor);
        }

        [Test]
        public void OnEnable_Reads_Last_Record()
        {
            RankScoreDecisionLog.Record(Make("short", 2f, RankScoreDecisionSource.Default));
            Spawn();
            Assert.AreEqual(1, pr.HistoryCount);
            StringAssert.Contains("short", view.ChoiceString);
            StringAssert.Contains("Mặc định", view.SourceString);
        }

        [Test]
        public void History_Is_Ring_Buffer_Newest_First()
        {
            Spawn();
            for (int i = 0; i < RankScoreDebugPanelPresenter.HistoryCapacity + 3; i++)
                RankScoreDecisionLog.Record(Make("normal", 2f + i, RankScoreDecisionSource.Offline));
            Assert.AreEqual(RankScoreDebugPanelPresenter.HistoryCapacity, pr.HistoryCount);
            string h = view.HistoryString;
            // moi nhat (reticle 9.0s) dung dau, cu nhat con lai la reticle 5s
            Assert.Less(h.IndexOf(" 9.0s"), h.IndexOf(" 5.0s"));
            StringAssert.DoesNotContain(" 4.0s  c=", h);
        }
    }
}
