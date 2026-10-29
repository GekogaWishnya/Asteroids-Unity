using Asteroids.Core.IConfigs;
using NUnit.Framework;

namespace Asteroids.Collisions.Tests
{
    /// <summary>The system reads masks on every pass, so changing one at runtime switches a collider's pairs.</summary>
    [TestFixture]
    public sealed class CollisionSystemMaskTests
    {
        private const CollisionLayer ShipMask = CollisionLayer.Enemy | CollisionLayer.Boundary;

        private CollisionSystem _system;
        private RecordingSink _sink;
        private FakeCollider _ship;
        private FakeCollider _rock;

        [SetUp]
        public void SetUp()
        {
            _sink = new RecordingSink();
            _system = new CollisionSystem(_sink, FakeShape.CreateContacts());
            _ship = new FakeCollider("ship")
            {
                Layer = CollisionLayer.Player, Mask = ShipMask, Overlaps = true
            };
            _rock = new FakeCollider("rock")
            {
                Layer = CollisionLayer.Enemy, Mask = CollisionLayer.Player, Overlaps = true
            };
            _system.Register(_ship);
            _system.Register(_rock);
        }

        private int Count(FakeCollider self, NotificationKind kind)
        {
            var count = 0;
            foreach (var notification in _sink.For(self))
                if (notification.Kind == kind) count++;
            return count;
        }

        [Test]
        public void Mask_WithoutTheOtherLayer_ChangedBetweenTicks_EndsTheContactOnTheNextTick()
        {
            _system.Tick(0f);
            Assert.That(Count(_ship, NotificationKind.Entered), Is.EqualTo(1));

            _ship.Mask = CollisionLayer.Boundary;
            _system.Tick(0f);

            Assert.That(Count(_ship, NotificationKind.Stayed), Is.EqualTo(0));
            Assert.That(Count(_ship, NotificationKind.Exited), Is.EqualTo(1));
            Assert.That(Count(_rock, NotificationKind.Exited), Is.EqualTo(1));
        }

        [Test]
        public void Mask_RestoredWhileStillOverlapping_StartsANewContact()
        {
            _system.Tick(0f);
            _ship.Mask = CollisionLayer.Boundary;
            _system.Tick(0f);

            _ship.Mask = ShipMask;
            _system.Tick(0f);

            Assert.That(Count(_ship, NotificationKind.Entered), Is.EqualTo(2));
            Assert.That(Count(_rock, NotificationKind.Entered), Is.EqualTo(2));
        }

        [Test]
        public void Mask_ChangedWhileEventsAreDispatched_ContactIsStillReportedToBothSides()
        {
            _sink[_ship] = _ => _ship.Mask = CollisionLayer.Boundary;

            _system.Tick(0f);

            Assert.That(Count(_ship, NotificationKind.Entered), Is.EqualTo(1));
            Assert.That(Count(_rock, NotificationKind.Entered), Is.EqualTo(1));

            _system.Tick(0f);

            Assert.That(Count(_ship, NotificationKind.Stayed), Is.EqualTo(0));
            Assert.That(Count(_ship, NotificationKind.Exited), Is.EqualTo(1));
        }
    }
}
