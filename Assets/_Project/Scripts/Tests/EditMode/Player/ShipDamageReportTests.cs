using System;
using Asteroids.Physics;
using NUnit.Framework;
using Asteroids.Core;

namespace Asteroids.Player.Tests
{
    [TestFixture]
    public sealed class ShipDamageReportTests
    {
        private PlayerHealth _health;
        private RecordingShipEvents _events;
        private ShipDamageReport _report;
        private Entity _ship;

        [SetUp]
        public void SetUp()
        {
            _health = new PlayerHealth(new TestHealthConfig { MaxHealth = 2 });
            _events = new RecordingShipEvents();
            _report = new ShipDamageReport(_health, _events);
            _ship = new Entity(new BodyPose());
        }

        [Test]
        public void Constructor_NullArguments_Throw()
        {
            Assert.Throws<ArgumentNullException>(() => new ShipDamageReport(null, _events));
            Assert.Throws<ArgumentNullException>(() => new ShipDamageReport(_health, null));
        }

        [Test]
        public void Attach_Null_Throws()
        {
            Assert.Throws<ArgumentNullException>(() => _report.Attach(null));
        }

        [Test]
        public void Attach_Twice_Throws()
        {
            _report.Attach(_ship);

            Assert.Throws<InvalidOperationException>(() => _report.Attach(_ship));
        }

        [Test]
        public void Attached_ReportsEveryLossWithTheShipAndWhetherItKilled()
        {
            _report.Attach(_ship);

            _health.TakeDamage(1);
            _health.TakeDamage(1);

            Assert.That(_events.Damaged, Is.EqualTo(new (IEntity, bool)[] { (_ship, false), (_ship, true) }));
        }

        [Test]
        public void FatalLoss_IsReportedBeforeDepletion()
        {
            var reportedFirst = false;
            _health.Depleted += () => reportedFirst = _events.Damaged.Count == 1;
            _report.Attach(_ship);

            _health.TakeDamage(2);

            Assert.That(reportedFirst, Is.True);
        }

        [Test]
        public void Detached_ReportsNothing()
        {
            _report.Attach(_ship).Dispose();

            _health.TakeDamage(1);

            Assert.That(_events.Damaged, Is.Empty);
        }

        [Test]
        public void Reset_IsNotReported()
        {
            _report.Attach(_ship);
            _health.TakeDamage(1);

            _health.Reset();

            Assert.That(_events.Damaged, Has.Count.EqualTo(1));
        }
    }
}
