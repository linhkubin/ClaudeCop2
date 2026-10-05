using System.Reflection;
using ClaudeCop.Combat;
using ClaudeCop.Core;
using NUnit.Framework;
using UnityEngine;

namespace ClaudeCop.Tests.EditMode
{
    /// <summary>Kiem tra tich hop cho T-600 (BlastEvents, ngu nghia Environment/Miss + combo).</summary>
    [Category("Wave6"), Category("T600")]
    public class W6T600Tests
    {
        [Test]
        public void BlastEvents_ResetOnPlay_ClearsSubscribers()
        {
            int calls = 0;
            System.Action<BlastReport> h = _ => calls++;
            BlastEvents.Blasted += h;
            try
            {
                var reset = typeof(BlastEvents).GetMethod("ResetStatics", BindingFlags.NonPublic | BindingFlags.Static);
                Assert.IsNotNull(reset, "BlastEvents phai co ResetStatics (RuntimeInitializeOnLoadMethod) de tu reset khi vao Play");
                reset.Invoke(null, null);
                BlastEvents.Raise(new BlastReport { Radius = 3f });
                Assert.AreEqual(0, calls, "Sau reset khong con subscriber nao nhan su kien; thuc te nhan " + calls);
            }
            finally { BlastEvents.Blasted -= h; }
        }

        [Test]
        public void Combo_ShootableThenWallMiss_KeepsThenResets()
        {
            var c = new ComboTracker();
            c.Apply(TapOutcome.Kill); c.Apply(TapOutcome.Kill); c.Apply(TapOutcome.Kill);
            var env = ShotClassifier.ClassifyEnvironment(true, true);
            c.Apply(env, ShotClassifier.KeepsCombo(env));
            Assert.AreEqual(3, c.Streak, "Ban thung/hop (IShootable) phai giu combo 3; thuc te " + c.Streak);
            var wall = ShotClassifier.ClassifyEnvironment(true, false);
            c.Apply(wall, ShotClassifier.KeepsCombo(wall));
            Assert.AreEqual(0, c.Streak, "Ban tuong tro la Miss, combo ve 0; thuc te " + c.Streak);
        }
    }
}
