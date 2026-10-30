using System;
using NUnit.Framework;

namespace Asteroids.Weapons.Tests
{
    [TestFixture]
    public sealed class FireCooldownTests
    {
        private const float Interval = 0.25f;

        [Test]
        public void Tick_HeldFromTheStart_FiresAtOnce()
        {
            var cooldown = new FireCooldown();

            Assert.That(cooldown.Tick(0.01f, true, Interval), Is.True);
        }

        [Test]
        public void Tick_NotHeld_NeverFires()
        {
            var cooldown = new FireCooldown();

            for (var i = 0; i < 100; i++) Assert.That(cooldown.Tick(0.1f, false, Interval), Is.False);
        }

        [Test]
        public void Tick_Held_FiresOncePerInterval()
        {
            var cooldown = new FireCooldown();
            var shots = 0;

            // 1 s at 100 ticks per second: the shot at t = 0.01 and then one every 0.25 s.
            for (var i = 0; i < 100; i++)
                if (cooldown.Tick(0.01f, true, Interval)) shots++;

            Assert.That(shots, Is.EqualTo(4));
        }

        [Test]
        public void Tick_RateDoesNotDependOnTheTickLength()
        {
            var fast = new FireCooldown();
            var slow = new FireCooldown();
            var fastShots = 0;
            var slowShots = 0;

            for (var i = 0; i < 600; i++)
                if (fast.Tick(1f / 60f, true, Interval)) fastShots++;
            for (var i = 0; i < 210; i++)
                if (slow.Tick(1f / 21f, true, Interval)) slowShots++;

            Assert.That(fastShots, Is.EqualTo(40).Within(1));
            Assert.That(slowShots, Is.EqualTo(40).Within(1));
        }

        [Test]
        public void Tick_ReleasedAndPressedAgain_WaitsForTheRestOfTheInterval()
        {
            var cooldown = new FireCooldown();
            cooldown.Tick(0.01f, true, Interval);

            cooldown.Tick(0.1f, false, Interval);

            Assert.That(cooldown.Tick(0.01f, true, Interval), Is.False);
            Assert.That(cooldown.Tick(0.15f, true, Interval), Is.True);
        }

        [Test]
        public void Tick_ReleasedLongerThanTheInterval_FiresAtOnceOnThePress()
        {
            var cooldown = new FireCooldown();
            cooldown.Tick(0.01f, true, Interval);

            cooldown.Tick(1f, false, Interval);

            Assert.That(cooldown.IsReady, Is.True);
            Assert.That(cooldown.Tick(0.01f, true, Interval), Is.True);
        }

        [Test]
        public void Tick_OneVeryLongTick_FiresOnlyOnce()
        {
            var cooldown = new FireCooldown();
            cooldown.Tick(0.01f, true, Interval);

            Assert.That(cooldown.Tick(10f, true, Interval), Is.True);
            Assert.That(cooldown.Tick(0.01f, true, Interval), Is.True);
            Assert.That(cooldown.Tick(0.01f, true, Interval), Is.False);
        }

        [TestCase(0f)]
        [TestCase(-1f)]
        [TestCase(float.NaN)]
        [TestCase(float.PositiveInfinity)]
        public void Tick_InvalidInterval_Throws(float interval)
        {
            var cooldown = new FireCooldown();

            Assert.Throws<ArgumentOutOfRangeException>(() => cooldown.Tick(0.1f, true, interval));
        }
    }
}
