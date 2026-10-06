using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using ClaudeCop.Game.Validation;

namespace ClaudeCop.Game.Tests
{
    /// <summary>Logic thuan cua LevelValidator (khong can scene).</summary>
    public class LevelValidatorTests
    {
        static List<Vector2> V(params float[] xy)
        {
            var l = new List<Vector2>();
            for (int i = 0; i < xy.Length; i += 2) l.Add(new Vector2(xy[i], xy[i + 1]));
            return l;
        }

        [Test]
        public void Route_Straight_Passes()
        {
            var poly = V(0, 0, 0, 10, 1, 20, 0, 30, 0, 40);
            Assert.IsFalse(LevelGeometry.FindSelfIntersection(poly, out _, out _));
            var yaws = new List<float> { 0f, 5f, -3f, 0f };
            Assert.Less(Mathf.Abs(LevelGeometry.NetYaw(yaws)), 150f);
        }

        [Test]
        public void Route_LoopBack_Fails()
        {
            // di len, re phai, xuong, re trai, lai cat ngang doan dau
            var poly = V(0, 0, 0, 20, 20, 20, 20, 10, -10, 10);
            Assert.IsTrue(LevelGeometry.FindSelfIntersection(poly, out int a, out int b));
            Assert.AreEqual(0, a); Assert.AreEqual(3, b);
            var yaws = new List<float> { 0f, 90f, 180f, 270f };
            Assert.Greater(Mathf.Abs(LevelGeometry.NetYaw(yaws)), 150f);
        }

        [Test]
        public void Route_TouchingEndpointsAndTinySegments_DoNotCount()
        {
            Assert.IsFalse(LevelGeometry.SegmentsIntersect(new Vector2(0, 0), new Vector2(1, 1), new Vector2(1, 1), new Vector2(2, 0)));
            var simp = LevelGeometry.Simplify(V(0, 0, 0.1f, 0.1f, 0.2f, 0, 5, 5), 0.5f);
            Assert.AreEqual(2, simp.Count);
        }

        [Test]
        public void Yaw_WrapAroundAndReversals()
        {
            // 350 -> 10 la +20, khong phai -340
            Assert.AreEqual(20f, LevelGeometry.NetYaw(new List<float> { 350f, 10f }), 1e-3f);
            Assert.AreEqual(1, LevelGeometry.CountTurnReversals(new List<float> { 0f, 30f, 60f, 20f, -20f }, 15f));
            Assert.AreEqual(0, LevelGeometry.CountTurnReversals(new List<float> { 0f, 30f, 60f, 90f }, 15f));
            Assert.AreEqual(0, LevelGeometry.CountTurnReversals(new List<float> { 0f, 5f, 0f, 5f, 0f }, 15f));
        }

        [Test]
        public void HorizontalOffset_AndClusterWidth()
        {
            var pos = Vector3.zero; var fwd = Vector3.forward;
            float h = LevelGeometry.HorizontalOffset(pos, fwd, new Vector3(Mathf.Tan(5f * Mathf.Deg2Rad) * 15f, 1f, 15f));
            Assert.AreEqual(5f, h, 0.01f); // ben phai (+X khi nhin +Z)
            var t = new List<Vector3> { new Vector3(-1.5f, 1f, 15f), new Vector3(2f, 1f, 15f) };
            LevelGeometry.ClusterSpread(pos, fwd, t, out float width, out float maxAbs);
            Assert.AreEqual(Mathf.Atan(2f / 15f) * Mathf.Rad2Deg + Mathf.Atan(1.5f / 15f) * Mathf.Rad2Deg, width, 0.01f);
            Assert.AreEqual(Mathf.Atan(2f / 15f) * Mathf.Rad2Deg, maxAbs, 0.01f);
        }

        [Test]
        public void RequiredFov_WiderAspectNeedsMoreVertical_AndBehindCameraIs180()
        {
            var tg = new List<Vector3> { new Vector3(-2f, 1.5f, 15f), new Vector3(2f, 1.5f, 15f) };
            float portrait = LevelGeometry.RequiredVerticalFov(Vector3.zero, Quaternion.identity, tg, 9f / 16f, 5f);
            float tall = LevelGeometry.RequiredVerticalFov(Vector3.zero, Quaternion.identity, tg, 9f / 19.5f, 5f);
            Assert.Greater(tall, portrait);
            Assert.AreEqual(180f, LevelGeometry.RequiredVerticalFov(Vector3.zero, Quaternion.identity, new List<Vector3> { new Vector3(0, 0, -1) }, 0.5f, 5f));
        }

        [Test]
        public void MoveSeconds_BoostedUpToMaxRailSpeed_AndDwellAdded()
        {
            // 12 m @3.5 m/s + ease 0.4 = 3.8 s (khong tang toc)
            Assert.AreEqual(3.8286f, LevelGeometry.MoveSeconds(12f, 3.5f, 0f, 0.4f, 0.4f, 5.8f, 4.5f, 0f), 0.01f);
            // 30 m: tang toc toi 4.5 -> 0.4 + 30/4.5
            Assert.AreEqual(0.4f + 30f / 4.5f, LevelGeometry.MoveSeconds(30f, 3.5f, 0f, 0.4f, 0.4f, 5.8f, 4.5f, 0f), 0.01f);
            Assert.AreEqual(2f, LevelGeometry.MoveSeconds(10f, 3.5f, 10f, 0f, 0f, 5.8f, 4.5f, 1f), 0.01f);
        }

        [Test]
        public void Chain_LevelKey_FromRootOrWavePrefix()
        {
            Assert.AreEqual("Level_02", LevelValidationRules.LevelKeyOf("L3_Wave_P1_W2", "Level_02"));
            Assert.AreEqual("Level_03", LevelValidationRules.LevelKeyOf("L3_Wave_P1_W2", "Encounters"));
            Assert.AreEqual("Level_01", LevelValidationRules.LevelKeyOf("Wave_P1_W2", null));
        }

        [Test]
        public void Chain_Limits_ScaleWithLevelCount_AndUseOwnLevelRule()
        {
            var rules = ScriptableObject.CreateInstance<LevelValidationRules>();
            var chain = new LevelRule { sceneNameContains = "Level_Chain", maxEnemiesPerLevel = 32 };
            rules.levelRules = new List<LevelRule> { new LevelRule { sceneNameContains = "Level_02", maxEnemiesTotal = 28 }, chain };
            Assert.AreSame(chain, rules.RuleFor("Level_Chain"));
            Assert.AreEqual(160, LevelValidationRules.ChainTotalLimit(chain, 5));
            Assert.AreEqual(28, rules.PerLevelLimit(chain, "Level_02"));
            Assert.AreEqual(32, rules.PerLevelLimit(chain, "Level_03"));   // khong co rule rieng -> theo chuoi
            Object.DestroyImmediate(rules);
        }

        [Test]
        public void RulesAsset_HasOwnRuleForLevel01To04_AndChain()
        {
            var rules = UnityEditor.AssetDatabase.LoadAssetAtPath<LevelValidationRules>("Assets/_Game/Settings/LevelValidationRules.asset");
            Assert.IsNotNull(rules);
            foreach (var key in new[] { "Level_01", "Level_02", "Level_03", "Level_04", "Level_Chain" })
                Assert.AreEqual(key, rules.RuleFor(key).sceneNameContains, key);
            var chain = rules.RuleFor("Level_Chain");
            Assert.AreEqual(20, rules.PerLevelLimit(chain, "Level_03"));   // trong chuoi: Level_03 dung rule rieng
            Assert.AreEqual(27, rules.PerLevelLimit(chain, "Level_04"));   // L4 kho tien: 27 enemy (GDD vong 11)
            var l4 = rules.RuleFor("Level_04");
            Assert.IsFalse(l4.forbidHumanShield, "L4: luat moi khien nguoi");
            Assert.IsFalse(l4.forbidHostage);
            Assert.GreaterOrEqual(l4.maxEnemiesPerWave, 5, "L4: 'Don dap loi vang' 5 enemy");
            Assert.GreaterOrEqual(chain.maxEnemiesPerWave, l4.maxEnemiesPerWave, "chuoi khong chat hon level con");
            Assert.GreaterOrEqual(chain.maxConcurrentCap, l4.maxConcurrentCap);
        }

        [Test]
        public void RulesAsset_MaxTargetDistance_Is35m()
        {
            var rules = UnityEditor.AssetDatabase.LoadAssetAtPath<LevelValidationRules>("Assets/_Game/Settings/LevelValidationRules.asset");
            Assert.AreEqual(35f, rules.maxTargetDistance, 1e-3f);   // GDD vong 11: xa thu toi 35 m
            Assert.Less(rules.minTargetDistance, rules.maxTargetDistance);
        }
    }
}
