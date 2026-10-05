using System.Linq;
using System.Collections.Generic;
using ClaudeCop.Core;
using NUnit.Framework;
using UnityEngine;

namespace ClaudeCop.Enemy.Tests
{
    public class HumanShieldTests
    {
        readonly List<GameObject> spawned = new List<GameObject>();

        [TearDown]
        public void TearDown()
        {
            foreach (var g in spawned) if (g != null) Object.DestroyImmediate(g);
            spawned.Clear();
        }

        [Test]
        public void Classify_JusticeFirst_ThenHead_ThenBody()
        {
            var head = new Vector2(500, 900);
            Assert.AreEqual(ShieldTap.Justice, HumanShieldRules.Classify(true, new Vector2(0, 0), head, 45f));
            Assert.AreEqual(ShieldTap.Head, HumanShieldRules.Classify(false, new Vector2(530, 900), head, 45f));
            Assert.AreEqual(ShieldTap.Head, HumanShieldRules.Classify(false, new Vector2(545, 900), head, 45f), "bien = trung");
            Assert.AreEqual(ShieldTap.Body, HumanShieldRules.Classify(false, new Vector2(546, 900), head, 45f));
            Assert.AreEqual(ShieldTap.Head, HumanShieldRules.Classify(false, Vector2.zero, null, 45f), "khong chieu duoc -> dau");
        }

        [Test]
        public void ScaleRadius_ByScreenWidth()
        {
            Assert.AreEqual(45f, HumanShieldRules.ScaleRadius(45f, 1080f, 1080f), 1e-4f);
            Assert.AreEqual(22.5f, HumanShieldRules.ScaleRadius(45f, 540f, 1080f), 1e-4f);
            Assert.AreEqual(EnemyConfig.Fallback.shieldHeadRadiusPx, 45f);
        }

        [Test]
        public void Outcomes()
        {
            Assert.AreEqual(TapOutcome.JusticeKill, HumanShieldRules.ToOutcome(ShieldTap.Justice));
            Assert.AreEqual(TapOutcome.Kill, HumanShieldRules.ToOutcome(ShieldTap.Head));
            Assert.AreEqual(TapOutcome.HostageHit, HumanShieldRules.ToOutcome(ShieldTap.Body));
        }

        HumanShieldEnemy Make(bool withBody = false)
        {
            var go = new GameObject("Shield"); spawned.Add(go);
            var e = go.AddComponent<HumanShieldEnemy>();
            if (withBody)
            {
                var body = new GameObject("Body"); spawned.Add(body);
                body.transform.SetParent(go.transform);
                typeof(HumanShieldEnemy).GetField("hostageBody", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                    .SetValue(e, body.transform);
            }
            e.Projector = _ => new Vector2(500, 900);   // dau o (500,900)
            e.ScreenWidthOverride = 1080f;
            e.Activate();
            e.Tick(EnemyConfig.Fallback.peekDuration + 0.01f);
            Assert.AreEqual(EnemyState.Aiming, e.State);
            return e;
        }

        [Test]
        public void Tap_Body_IsHostageHit_EnemyAlive()
        {
            var e = Make();
            var o = e.OnTapHit(new ShotInfo { ScreenPosition = new Vector2(700, 900), Direction = Vector3.forward }, false);
            Assert.AreEqual(TapOutcome.HostageHit, o);
            Assert.IsFalse(e.IsDead);
            Assert.IsTrue(e.IsHostageWounded);
            Assert.AreEqual(EnemyState.Aiming, e.State);
        }

        [Test]
        public void Blast_KillsShield_ProxyIgnored()
        {
            var e = Make(true);
            ITapTarget proxy = null;
            foreach (var t in TargetRegistry.Targets) if (t.Kind == TargetKind.Hostage) proxy = t;
            var blast = new ShotInfo { ScreenPosition = new Vector2(float.NaN, float.NaN), Direction = Vector3.up };
            Assert.AreEqual(TapOutcome.Miss, proxy.OnTapHit(blast, false), "proxy bo qua khi no");
            Assert.IsFalse(e.IsHostageWounded);
            Assert.AreEqual(TapOutcome.Kill, e.OnTapHit(blast, false));
            Assert.IsTrue(e.IsDead);
            Assert.IsFalse(e.IsHostageWounded);
        }

        [Test]
        public void Tap_Head_IsKill()
        {
            var e = Make();
            var o = e.OnTapHit(new ShotInfo { ScreenPosition = new Vector2(510, 910), Direction = Vector3.forward }, false);
            Assert.AreEqual(TapOutcome.Kill, o);
            Assert.IsTrue(e.IsDead);
        }

        [Test]
        public void Tap_Justice_IsJusticeKill_EvenWhenWaveTurnedJusticeOff()
        {
            var e = Make();
            e.SetJustice(false);
            Assert.IsTrue(e.HasJusticePoint);
            var o = e.OnTapHit(new ShotInfo { ScreenPosition = new Vector2(100, 100), Direction = Vector3.forward }, true);
            Assert.AreEqual(TapOutcome.JusticeKill, o);
            Assert.IsTrue(e.IsSurrendered);
        }

        [Test]
        public void BodyProxy_RegisteredAsHostage_WhileAiming_AndPenalizes()
        {
            var e = Make(true);
            ITapTarget proxy = null;
            foreach (var t in TargetRegistry.Targets) if (t.Kind == TargetKind.Hostage) proxy = t;
            Assert.IsNotNull(proxy);
            Assert.IsFalse(proxy.ShowsReticle);
            Assert.AreEqual(TapOutcome.HostageHit, proxy.OnTapHit(new ShotInfo(), false));
            e.OnTapHit(new ShotInfo { ScreenPosition = new Vector2(510, 900), Direction = Vector3.forward }, false);
            Assert.IsFalse(TargetRegistry.Targets.Contains(proxy), "enemy chet -> go proxy");
            Assert.IsTrue(e.IsHostageReleased);
        }

        [Test]
        public void ShieldEnemy_FiresAtPlayer_WhenReticleEnds()
        {
            var e = Make();
            int fired = 0; e.Fired += _ => fired++;
            e.Tick(EnemyConfig.Fallback.reticleTime + 0.1f);
            Assert.AreEqual(1, fired);
        }
    }
}
