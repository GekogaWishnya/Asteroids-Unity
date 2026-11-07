using System;
using System.Collections.Generic;
using Asteroids.Core;
using NUnit.Framework;

namespace Asteroids.Controls.Tests
{
    /// <summary>
    /// <see cref="ShipInput"/> maps Input Manager axes onto the ship's input contracts. These tests drive it with a
    /// fake <see cref="IAxisReader"/>, so no Unity input is involved; the real bindings are checked by
    /// <see cref="InputManagerAxesTests"/>.
    /// </summary>
    [TestFixture]
    public sealed class ShipInputTests
    {
        /// <summary>
        /// Axes not set explicitly read 0. Axes in <see cref="Missing"/> throw like an undefined Unity axis.
        /// Every name asked for lands in <see cref="Requested"/>.
        /// </summary>
        private sealed class FakeAxes : IAxisReader
        {
            private readonly Dictionary<string, float> _values = new();

            public HashSet<string> Missing { get; } = new();
            public HashSet<string> Requested { get; } = new();

            public float this[string axis] { set => _values[axis] = value; }

            public float Read(string axis)
            {
                Requested.Add(axis);
                if (Missing.Contains(axis)) throw new InvalidOperationException($"'{axis}' is not defined.");
                return _values.TryGetValue(axis, out var value) ? value : 0f;
            }
        }

        private FakeAxes _axes;
        private ShipInput _input;

        [SetUp]
        public void SetUp()
        {
            _axes = new FakeAxes();
            _input = new ShipInput(_axes);
        }

        [Test]
        public void Constructor_NullReader_Throws() =>
            Assert.Throws<ArgumentNullException>(() => _ = new ShipInput(null));

        [TestCase(-1f, -1f)]
        [TestCase(0.4f, 0.4f)]
        [TestCase(1f, 1f)]
        [TestCase(-3f, -1f)] // a misconfigured axis (sensitivity, stacked entries) can never over-steer
        [TestCase(3f, 1f)]
        public void Turn_PassesAxisThroughClampedToUnitRange(float raw, float expected)
        {
            _axes[ShipInputAxes.Turn] = raw;

            Assert.That(_input.Turn, Is.EqualTo(expected));
        }

        [TestCase(1f, 1f)]
        [TestCase(0.25f, 0.25f)]
        [TestCase(-1f, 0f)] // stick pulled back or S pressed: no reverse thrust, as before the migration
        [TestCase(2f, 1f)]
        public void Thrust_IsForwardOnly(float raw, float expected)
        {
            _axes[ShipInputAxes.Thrust] = raw;

            Assert.That(_input.Thrust, Is.EqualTo(expected));
        }

        [TestCase(1f, true)]                            // key or mouse button held
        [TestCase(ShipInput.TriggerThreshold, true)]    // trigger exactly at the threshold
        [TestCase(0.49f, false)]                        // trigger resting a little pressed
        [TestCase(0f, false)]
        [TestCase(-1f, false)]                          // trigger axes that rest at -1 on some drivers
        public void Fire_IsHeldFromTheThresholdUp(float raw, bool expected)
        {
            _axes[ShipInputAxes.Fire] = raw;

            Assert.That(_input.For(WeaponSlot.Primary).IsFiring, Is.EqualTo(expected));
        }

        [Test]
        public void EachSlot_ReadsItsOwnAxis()
        {
            _axes[ShipInputAxes.AltFire] = 1f;

            Assert.That(_input.For(WeaponSlot.Alternate).IsFiring, Is.True);
            Assert.That(_input.For(WeaponSlot.Primary).IsFiring, Is.False);
        }

        [Test]
        public void Triggers_AreLive_NotSnapshotted()
        {
            var fire = _input.For(WeaponSlot.Primary);

            _axes[ShipInputAxes.Fire] = 1f;
            Assert.That(fire.IsFiring, Is.True);

            _axes[ShipInputAxes.Fire] = 0f;
            Assert.That(fire.IsFiring, Is.False);
        }

        [Test]
        public void For_UnknownSlot_Throws() =>
            Assert.Throws<ArgumentOutOfRangeException>(() => _input.For((WeaponSlot)999));

        [Test]
        public void Initialize_AllAxesDefined_DoesNotThrow() =>
            Assert.DoesNotThrow(_input.Initialize);

        /// <summary>
        /// The startup check must cover exactly the axes gameplay reads. This is what replaces the old
        /// hand-maintained axis list: add an axis to <see cref="ShipInput"/> and this keeps passing on its own,
        /// with no second place to update.
        /// </summary>
        [Test]
        public void Initialize_ChecksEveryAxisGameplayReads()
        {
            var gameplay = AxesReadDuringGameplay();

            var startup = new FakeAxes();
            new ShipInput(startup).Initialize();

            Assert.That(startup.Requested, Is.EquivalentTo(gameplay));
        }

        [Test]
        public void Initialize_MissingAxis_FailsAtStartup([ValueSource(nameof(AxesReadDuringGameplay))] string axis)
        {
            _axes.Missing.Add(axis);

            Assert.Throws<InvalidOperationException>(_input.Initialize);
        }

        /// <summary>Every axis name touched by reading all of <see cref="ShipInput"/>'s public inputs once.</summary>
        private static IEnumerable<string> AxesReadDuringGameplay()
        {
            var axes = new FakeAxes();
            var input = new ShipInput(axes);

            _ = input.Turn;
            _ = input.Thrust;
            foreach (WeaponSlot slot in Enum.GetValues(typeof(WeaponSlot)))
                _ = input.For(slot).IsFiring;

            return axes.Requested;
        }
    }
}
