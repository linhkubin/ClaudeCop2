using NUnit.Framework;
using UnityEngine;
using ClaudeCop.Enemy;

namespace ClaudeCop.Enemy.Tests
{
    public class DeathStyleTests
    {
        [Test]
        public void LowHit_Crumples()
        {
            Assert.AreEqual(EnemyActor.DeathStyle.Crumple, EnemyActor.PickDeathStyle(new Vector3(0f, 0.5f, 0f), Vector3.right, false));
        }

        [Test]
        public void SideHit_Spins_CenterHit_FallsBack()
        {
            Assert.AreEqual(EnemyActor.DeathStyle.Spin, EnemyActor.PickDeathStyle(new Vector3(0.3f, 1.4f, 0f), Vector3.right, false));
            Assert.AreEqual(EnemyActor.DeathStyle.FallBack, EnemyActor.PickDeathStyle(new Vector3(0.05f, 1.4f, 0f), Vector3.right, false));
        }

        [Test]
        public void Blast_AlwaysFallsBack()
        {
            Assert.AreEqual(EnemyActor.DeathStyle.FallBack, EnemyActor.PickDeathStyle(new Vector3(0f, 0.2f, 0f), Vector3.right, true));
        }
    }
}
