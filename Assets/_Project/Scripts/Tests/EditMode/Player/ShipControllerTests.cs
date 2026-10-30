using System;
using System.Collections.Generic;
using Asteroids.Physics;
using NUnit.Framework;

namespace Asteroids.Player.Tests
{
    [TestFixture]
    public sealed class ShipControllerTests
    {
        private List<string> _log;
        private FakeShipPart _first;
        private FakeShipPart _second;
        private Entity _entity;
        private PlayerHealth _health;
        private RecordingShipEvents _events;

        [SetUp]
        public void SetUp()
        {
            _log = new List<string>();
            _first = new FakeShipPart("first", _log);
            _second = new FakeShipPart("second", _log);
            _entity = new Entity(new BodyPose());
            _health = new PlayerHealth(new TestHealthConfig { MaxHealth = 3 });
            _events = new RecordingShipEvents();
        }

        private ShipController CreateController() => new(_entity, new[] { _first, _second }, _health, _events);

        [Test]
        public void Construct_AttachesNothing()
        {
            CreateController();

            Assert.That(_log, Is.Empty);
        }

        [Test]
        public void Enable_AttachesEveryPartToTheEntityInOrder()
        {
            CreateController().Enable();

            Assert.That(_log, Is.EqualTo(new[] { "+first", "+second" }));
            Assert.That(_first.Ships, Is.EqualTo(new[] { _entity }));
            Assert.That(_second.Ships, Is.EqualTo(new[] { _entity }));
        }

        [Test]
        public void Disable_DetachesThePartsInReverseOrder()
        {
            var controller = CreateController();
            controller.Enable();
            _log.Clear();

            controller.Disable();

            Assert.That(_log, Is.EqualTo(new[] { "-second", "-first" }));
        }

        [Test]
        public void Enable_Twice_AttachesOnce()
        {
            var controller = CreateController();

            controller.Enable();
            controller.Enable();

            Assert.That(_log, Is.EqualTo(new[] { "+first", "+second" }));
        }

        [Test]
        public void Disable_WithoutEnable_DoesNothing()
        {
            CreateController().Disable();

            Assert.That(_log, Is.Empty);
        }

        [Test]
        public void EnableDisableEnable_AttachesAgain()
        {
            var controller = CreateController();

            controller.Enable();
            controller.Disable();
            controller.Enable();

            Assert.That(_first.LiveCount, Is.EqualTo(1));
            Assert.That(_second.LiveCount, Is.EqualTo(1));
            Assert.That(_first.Ships.Count, Is.EqualTo(2));
        }

        [Test]
        public void Enable_WhenAPartFails_DetachesTheOnesBeforeItAndRethrows()
        {
            _second.FailOnAttach = true;
            var controller = CreateController();

            Assert.Throws<InvalidOperationException>(() => controller.Enable());
            Assert.That(_first.LiveCount, Is.EqualTo(0));
        }

        [Test]
        public void Damage_NotLethal_DoesNotDestroyTheShip()
        {
            CreateController().Enable();

            _health.TakeDamage(2);

            Assert.That(_events.DestroyedEntities, Is.Empty);
            Assert.That(_first.LiveCount, Is.EqualTo(1));
        }

        [Test]
        public void Damage_Lethal_ReportsDestroyedOnceWithTheEntity()
        {
            CreateController().Enable();

            _health.TakeDamage(3);
            _health.TakeDamage(1);

            Assert.That(_events.DestroyedEntities, Is.EqualTo(new[] { _entity }));
        }

        [Test]
        public void Damage_Lethal_DetachesEveryPartBeforeReportingIt()
        {
            var liveWhenReported = -1;
            _events.OnDestroyed = () => liveWhenReported = _first.LiveCount + _second.LiveCount;
            CreateController().Enable();

            _health.TakeDamage(3);

            Assert.That(liveWhenReported, Is.EqualTo(0));
        }

        [Test]
        public void Enable_WhenHealthIsDepleted_DoesNothing()
        {
            _health.TakeDamage(3);

            CreateController().Enable();

            Assert.That(_log, Is.Empty);
        }

        [Test]
        public void Enable_AfterHealthWasReset_WorksAgain()
        {
            var controller = CreateController();
            controller.Enable();
            _health.TakeDamage(3);
            _health.Reset();

            controller.Enable();

            Assert.That(_first.LiveCount, Is.EqualTo(1));
            Assert.That(_second.LiveCount, Is.EqualTo(1));
        }

        [Test]
        public void Damage_AfterDisable_DoesNotReportDestroyed()
        {
            var controller = CreateController();
            controller.Enable();
            controller.Disable();

            _health.TakeDamage(3);

            Assert.That(_events.DestroyedEntities, Is.Empty);
        }

        [Test]
        public void Damage_AfterReEnable_ReportsDestroyedOnce()
        {
            var controller = CreateController();
            controller.Enable();
            controller.Disable();
            controller.Enable();

            _health.TakeDamage(3);

            Assert.That(_events.DestroyedEntities.Count, Is.EqualTo(1));
        }

        [Test]
        public void Construct_NoParts_StillWatchesHealth()
        {
            new ShipController(_entity, Array.Empty<IShipPart>(), _health, _events).Enable();

            _health.TakeDamage(3);

            Assert.That(_events.DestroyedEntities.Count, Is.EqualTo(1));
        }

        [Test]
        public void Construct_NullArguments_Throw()
        {
            var parts = new IShipPart[] { _first };

            Assert.Throws<ArgumentNullException>(() => new ShipController(null, parts, _health, _events));
            Assert.Throws<ArgumentNullException>(() => new ShipController(_entity, null, _health, _events));
            Assert.Throws<ArgumentNullException>(() => new ShipController(_entity, parts, null, _events));
            Assert.Throws<ArgumentNullException>(() => new ShipController(_entity, parts, _health, null));
        }

        [Test]
        public void Construct_NullPart_Throws()
        {
            Assert.Throws<ArgumentException>(() =>
                new ShipController(_entity, new IShipPart[] { _first, null }, _health, _events));
        }
    }
}
