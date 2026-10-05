using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using ClaudeCop.Enemy;
using ClaudeCop.Game;

namespace ClaudeCop.Tests.EditMode
{
    /// <summary>Tich hop W6: GrenadeFlight o bien, asset GameConfig, prefab enemy moi.</summary>
    [Category("Wave6")]
    public class W6EnemyGameTests
    {
        [Test, Category("T604")]
        public void GameConfigAsset_ExplosionPoints_Are100And50()
        {
            var c = AssetDatabase.LoadAssetAtPath<GameConfig>("Assets/_Game/Prefabs/Game/Data/GameConfig.asset");
            Assert.IsNotNull(c, "Thieu GameConfig.asset");
            Assert.AreEqual(100, c.ExplosionKillPoints, "ExplosionKillPoints mong doi 100");
            Assert.AreEqual(50, c.GrenadeShotPoints, "GrenadeShotPoints mong doi 50");
        }

        [Test, Category("T602")]
        public void GrenadeFlight_ShootAfterExplode_Fails()
        {
            var f = new GrenadeFlight(Vector3.zero, Vector3.forward * 10f, 1f, 2f);
            Assert.IsTrue(f.Tick(2f), "Tick vuot thoi luong phai no");
            Assert.AreEqual(GrenadeState.Exploded, f.State);
            Assert.IsFalse(f.Shoot(), "Ban sau khi no phai that bai");
            Assert.IsFalse(f.Tick(1f), "Tick lan hai khong duoc no lai");
        }

        [Test, Category("T602")]
        public void GrenadeFlight_ShotDown_StopsTicking()
        {
            var f = new GrenadeFlight(Vector3.zero, Vector3.forward * 10f, 1f, 2f);
            Assert.IsTrue(f.Shoot());
            Assert.IsFalse(f.Tick(5f), "Da bi ban thi khong no");
            Assert.AreEqual(GrenadeState.ShotDown, f.State);
        }

        [Test, Category("T602")]
        public void GrenadeFlight_EndPosition_EqualsTarget()
        {
            var end = new Vector3(1, 2, 8);
            var f = new GrenadeFlight(Vector3.zero, end, 1f, 3f);
            Assert.AreEqual(0f, Vector3.Distance(f.PositionAt(1f), end), 1e-4f, "Diem cuoi phai la end");
        }

        [Test, Category("T602")]
        public void EnemyPrefabs_GrenadierAndShield_Exist()
        {
            Assert.IsNotNull(AssetDatabase.LoadAssetAtPath<GameObject>("Assets/_Game/Prefabs/Enemies/Enemy_Grenadier.prefab"));
            Assert.IsNotNull(AssetDatabase.LoadAssetAtPath<GameObject>("Assets/_Game/Prefabs/Enemies/Enemy_HumanShield.prefab"));
            var g = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/_Game/Prefabs/Enemies/Grenade.prefab");
            Assert.IsNotNull(g, "Thieu Grenade.prefab");
            Assert.IsNotNull(g.GetComponent<Grenade>(), "Grenade.prefab thieu component Grenade");
        }
    }
}
