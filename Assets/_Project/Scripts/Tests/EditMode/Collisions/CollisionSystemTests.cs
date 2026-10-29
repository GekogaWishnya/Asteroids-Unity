using System;
using Asteroids.Core.IConfigs;
using NUnit.Framework;
using UnityEngine;

namespace Asteroids.Collisions.Tests
{
    [TestFixture]
    public sealed class CollisionSystemTests
    {
        private const float Tolerance = 1e-5f;

        private CollisionSystem _system;
        private RecordingSink _sink;

        [SetUp]
        public void SetUp()
        {
            _sink = new RecordingSink();
            _system = new CollisionSystem(_sink, FakeShape.CreateContacts());
        }

        private static FakeCollider CreateCollider(string name, bool overlaps = false)
        {
            var collider = new FakeCollider(name) { Overlaps = overlaps };
            return collider;
        }

        private int Count(ICollider collider, NotificationKind kind)
        {
            var count = 0;
            foreach (var notification in _sink.For(collider))
                if (notification.Kind == kind) count++;
            return count;
        }

        // ---- Unsupported shape pairs ----

        [Test]
        public void Register_ShapeWithNoRuleAgainstRegisteredShape_ThrowsNotSupported()
        {
            // UnknownShape knows how to meet itself, but not FakeShape.
            var contacts = FakeShape.CreateContacts()
                .Add<UnknownShape, UnknownShape>((UnknownShape _, UnknownShape __, out Contact contact) =>
                {
                    contact = default;
                    return false;
                });
            var system = new CollisionSystem(_sink, contacts);
            system.Register(CreateCollider("a"));
            var unknown = new ShapeCollider("unknown", CollisionLayer.All, CollisionLayer.All, new UnknownShape());

            Assert.Throws<NotSupportedException>(() => system.Register(unknown));
        }

        [Test]
        public void Register_ShapeWithNoRuleAgainstItself_ThrowsNotSupported()
        {
            var unknown = new ShapeCollider("unknown", CollisionLayer.All, CollisionLayer.All, new UnknownShape());

            Assert.Throws<NotSupportedException>(() => _system.Register(unknown));
        }

        [Test]
        public void Register_UnsupportedShape_LeavesSystemUntouched()
        {
            var a = CreateCollider("a", overlaps: true);
            var b = CreateCollider("b");
            _system.Register(a);
            _system.Register(b);
            var unknown = new ShapeCollider("unknown", CollisionLayer.All, CollisionLayer.All, new UnknownShape());
            Assert.Throws<NotSupportedException>(() => _system.Register(unknown));

            Assert.That(_system.Unregister(unknown), Is.False);
            Assert.DoesNotThrow(() => _system.Tick(0f));
            Assert.That(Count(a, NotificationKind.Entered), Is.EqualTo(1));
        }

        [Test]
        public void Register_ColliderWithoutShape_ThrowsArgument()
        {
            Assert.Throws<ArgumentException>(() => _system.Register(new NoShapeCollider()));
        }

        private sealed class NoShapeCollider : ICollider
        {
            public object Owner => this;
            public CollisionLayer Layer => CollisionLayer.All;
            public CollisionLayer Mask => CollisionLayer.All;
            public IShape Shape => null;
        }

        // ---- Registration ----

        [Test]
        public void Register_SameColliderTwice_Throws()
        {
            var a = CreateCollider("a");
            _system.Register(a);

            Assert.Throws<InvalidOperationException>(() => _system.Register(a));
        }

        [Test]
        public void Register_Null_ThrowsArgumentNull()
        {
            Assert.Throws<ArgumentNullException>(() => _system.Register(null));
        }

        [Test]
        public void Unregister_Null_ThrowsArgumentNull()
        {
            Assert.Throws<ArgumentNullException>(() => _system.Unregister(null));
        }

        [Test]
        public void Unregister_NotRegistered_ReturnsFalse()
        {
            Assert.That(_system.Unregister(CreateCollider("a")), Is.False);
        }

        [Test]
        public void Unregister_Registered_ReturnsTrueOnceThenFalse()
        {
            var a = CreateCollider("a");
            _system.Register(a);

            Assert.That(_system.Unregister(a), Is.True);
            Assert.That(_system.Unregister(a), Is.False);
        }

        [Test]
        public void Tick_CalledFromInsideAHandler_ThrowsInvalidOperation()
        {
            var a = CreateCollider("a", overlaps: true);
            var b = CreateCollider("b");
            _sink[a] = _ => _system.Tick(0f);
            _system.Register(a);
            _system.Register(b);

            Assert.Throws<InvalidOperationException>(() => _system.Tick(0f));
        }

        [Test]
        public void Tick_AfterHandlerThrew_SystemIsUsableAgain()
        {
            var a = CreateCollider("a", overlaps: true);
            var b = CreateCollider("b");
            _sink[a] = _ => throw new InvalidOperationException("boom");
            _system.Register(a);
            _system.Register(b);
            Assert.Throws<InvalidOperationException>(() => _system.Tick(0f));

            _sink[a] = null;

            Assert.DoesNotThrow(() => _system.Tick(0f));
        }

        // ---- Lifecycle ----

        [Test]
        public void Tick_FirstOverlap_FiresEnteredOnBothSides()
        {
            var a = CreateCollider("a", overlaps: true);
            var b = CreateCollider("b");
            _system.Register(a);
            _system.Register(b);

            _system.Tick(0f);

            Assert.That(_sink.For(a).Count, Is.EqualTo(1));
            Assert.That(_sink.For(a)[0].Kind, Is.EqualTo(NotificationKind.Entered));
            Assert.That(_sink.For(a)[0].Other, Is.SameAs(b));
            Assert.That(_sink.For(b).Count, Is.EqualTo(1));
            Assert.That(_sink.For(b)[0].Kind, Is.EqualTo(NotificationKind.Entered));
            Assert.That(_sink.For(b)[0].Other, Is.SameAs(a));
        }

        [Test]
        public void Tick_FirstOverlap_FiresEnteredFirstThenSecondInRegistrationOrder()
        {
            var a = CreateCollider("a", overlaps: true);
            var b = CreateCollider("b");
            _system.Register(a);
            _system.Register(b);

            _system.Tick(0f);

            Assert.That(_sink.Notifications.Count, Is.EqualTo(2));
            Assert.That(_sink.Notifications[0].Self, Is.SameAs(a));
            Assert.That(_sink.Notifications[0].Other, Is.SameAs(b));
            Assert.That(_sink.Notifications[1].Self, Is.SameAs(b));
            Assert.That(_sink.Notifications[1].Other, Is.SameAs(a));
        }

        [Test]
        public void Tick_ContinuedOverlap_FiresStayedNotEntered()
        {
            var a = CreateCollider("a", overlaps: true);
            var b = CreateCollider("b");
            _system.Register(a);
            _system.Register(b);
            _system.Tick(0f);
            _sink.Notifications.Clear();

            _system.Tick(0f);

            Assert.That(_sink.Notifications.Count, Is.EqualTo(2));
            Assert.That(_sink.Notifications[0].Kind, Is.EqualTo(NotificationKind.Stayed));
            Assert.That(_sink.Notifications[0].Self, Is.SameAs(a));
            Assert.That(_sink.Notifications[1].Kind, Is.EqualTo(NotificationKind.Stayed));
            Assert.That(_sink.Notifications[1].Self, Is.SameAs(b));
            Assert.That(Count(a, NotificationKind.Entered), Is.EqualTo(0));
            Assert.That(Count(b, NotificationKind.Entered), Is.EqualTo(0));
        }

        [Test]
        public void Tick_OverlapEnds_FiresExitedOnBothSides()
        {
            var a = CreateCollider("a", overlaps: true);
            var b = CreateCollider("b");
            _system.Register(a);
            _system.Register(b);
            _system.Tick(0f);
            _sink.Notifications.Clear();
            a.Overlaps = false;

            _system.Tick(0f);

            Assert.That(_sink.Notifications.Count, Is.EqualTo(2));
            Assert.That(_sink.Notifications[0].Kind, Is.EqualTo(NotificationKind.Exited));
            Assert.That(_sink.Notifications[0].Self, Is.SameAs(a));
            Assert.That(_sink.Notifications[0].Other, Is.SameAs(b));
            Assert.That(_sink.Notifications[1].Kind, Is.EqualTo(NotificationKind.Exited));
            Assert.That(_sink.Notifications[1].Self, Is.SameAs(b));
            Assert.That(_sink.Notifications[1].Other, Is.SameAs(a));
        }

        [Test]
        public void Tick_OverlapEnds_ExitedFiresOnlyOnce()
        {
            var a = CreateCollider("a", overlaps: true);
            var b = CreateCollider("b");
            _system.Register(a);
            _system.Register(b);
            _system.Tick(0f);
            a.Overlaps = false;
            _system.Tick(0f);
            _sink.Notifications.Clear();

            _system.Tick(0f);

            Assert.That(_sink.Notifications, Is.Empty);
        }

        [Test]
        public void Tick_PairNeverOverlapped_FiresNothing()
        {
            var a = CreateCollider("a");
            var b = CreateCollider("b");
            _system.Register(a);
            _system.Register(b);

            _system.Tick(0f);
            _system.Tick(0f);

            Assert.That(_sink.Notifications, Is.Empty);
        }

        [Test]
        public void Tick_SecondSideReceivesInvertedContact()
        {
            var a = CreateCollider("a", overlaps: true);
            var b = CreateCollider("b");
            a.ContactToReturn = new Contact(new Vector2(1f, 0f), 0.25f);
            _system.Register(a);
            _system.Register(b);

            _system.Tick(0f);

            var first = _sink.For(a)[0].Contact;
            var second = _sink.For(b)[0].Contact;
            Assert.That(first.Normal.x, Is.EqualTo(1f).Within(Tolerance));
            Assert.That(first.Normal.y, Is.EqualTo(0f).Within(Tolerance));
            Assert.That(second.Normal.x, Is.EqualTo(-1f).Within(Tolerance));
            Assert.That(second.Normal.y, Is.EqualTo(0f).Within(Tolerance));
            Assert.That(second.Depth, Is.EqualTo(first.Depth).Within(Tolerance));
            Assert.That(first.Depth, Is.EqualTo(0.25f).Within(Tolerance));
        }

        [Test]
        public void Tick_ThreeColliders_OnlyOverlappingPairsAreNotified()
        {
            var a = CreateCollider("a");
            var b = CreateCollider("b", overlaps: true);
            var c = CreateCollider("c");
            _system.Register(a);
            _system.Register(b);
            _system.Register(c);

            _system.Tick(0f);

            Assert.That(_sink.For(a), Is.Empty);
            Assert.That(_sink.For(b).Count, Is.EqualTo(1));
            Assert.That(_sink.For(b)[0].Other, Is.SameAs(c));
            Assert.That(_sink.For(c).Count, Is.EqualTo(1));
            Assert.That(_sink.For(c)[0].Other, Is.SameAs(b));
        }

        // ---- Filtering ----

        [Test]
        public void Tick_MasksDoNotAcceptEachOther_NoContactTested()
        {
            var a = CreateCollider("a", overlaps: true);
            var b = CreateCollider("b", overlaps: true);
            a.Layer = CollisionLayer.Player;
            a.Mask = CollisionLayer.Player;
            b.Layer = CollisionLayer.Enemy;
            b.Mask = CollisionLayer.Enemy;
            _system.Register(a);
            _system.Register(b);

            _system.Tick(0f);

            Assert.That(a.ContactQueries, Is.Empty);
            Assert.That(b.ContactQueries, Is.Empty);
            Assert.That(_sink.Notifications, Is.Empty);
        }

        [Test]
        public void Tick_OneMaskAcceptsOtherDoesNot_NoContact()
        {
            var a = CreateCollider("a", overlaps: true);
            var b = CreateCollider("b", overlaps: true);
            a.Layer = CollisionLayer.Player;
            a.Mask = CollisionLayer.Enemy;
            b.Layer = CollisionLayer.Enemy;
            b.Mask = CollisionLayer.Enemy;
            _system.Register(a);
            _system.Register(b);

            _system.Tick(0f);

            Assert.That(a.ContactQueries, Is.Empty);
            Assert.That(_sink.Notifications, Is.Empty);
        }

        [Test]
        public void Tick_BothMasksAccept_Contact()
        {
            var a = CreateCollider("a", overlaps: true);
            var b = CreateCollider("b");
            a.Layer = CollisionLayer.Player;
            a.Mask = CollisionLayer.Enemy;
            b.Layer = CollisionLayer.Enemy;
            b.Mask = CollisionLayer.Player;
            _system.Register(a);
            _system.Register(b);

            _system.Tick(0f);

            Assert.That(a.ContactQueries, Is.EqualTo(new[] { b }));
            Assert.That(Count(a, NotificationKind.Entered), Is.EqualTo(1));
            Assert.That(Count(b, NotificationKind.Entered), Is.EqualTo(1));
        }

        // ---- Deferred registration ----

        [Test]
        public void Register_DuringTick_AppliesAfterTick()
        {
            var a = CreateCollider("a", overlaps: true);
            var b = CreateCollider("b");
            var c = CreateCollider("c");
            var registered = false;
            _sink[a] = _ =>
            {
                if (registered) return;
                registered = true;
                _system.Register(c);
            };
            _system.Register(a);
            _system.Register(b);

            _system.Tick(0f);

            Assert.That(a.ContactQueries, Is.EqualTo(new[] { b }), "c must not be tested in the tick that registered it.");

            _system.Tick(0f);

            Assert.That(a.ContactQueries, Contains.Item(c), "c must be tested in the next tick.");
            Assert.That(Count(c, NotificationKind.Entered), Is.EqualTo(1));
        }

        [Test]
        public void Unregister_DuringTick_ColliderStillGetsThisTicksEvents()
        {
            var a = CreateCollider("a", overlaps: true);
            var b = CreateCollider("b");
            var unregistered = false;
            _sink[a] = _ =>
            {
                if (unregistered) return;
                unregistered = true;
                Assert.That(_system.Unregister(b), Is.True);
            };
            _system.Register(a);
            _system.Register(b);

            _system.Tick(0f);

            Assert.That(Count(b, NotificationKind.Entered), Is.EqualTo(1));
        }

        [Test]
        public void Unregister_DuringTick_ColliderIsNotTestedNextTick()
        {
            var a = CreateCollider("a", overlaps: true);
            var b = CreateCollider("b");
            _sink[a] = _ => _system.Unregister(b);
            _system.Register(a);
            _system.Register(b);
            _system.Tick(0f);
            a.ContactQueries.Clear();

            _system.Tick(0f);

            Assert.That(a.ContactQueries, Is.Empty);
        }

        // ---- Unregister of an active pair ----

        [Test]
        public void Unregister_ActivePair_BothSidesGetExited()
        {
            var a = CreateCollider("a", overlaps: true);
            var b = CreateCollider("b");
            _system.Register(a);
            _system.Register(b);
            _system.Tick(0f);
            _sink.Notifications.Clear();

            _system.Unregister(a);

            Assert.That(_sink.Notifications.Count, Is.EqualTo(2));
            Assert.That(_sink.Notifications[0].Kind, Is.EqualTo(NotificationKind.Exited));
            Assert.That(_sink.Notifications[0].Self, Is.SameAs(a));
            Assert.That(_sink.Notifications[0].Other, Is.SameAs(b));
            Assert.That(_sink.Notifications[1].Kind, Is.EqualTo(NotificationKind.Exited));
            Assert.That(_sink.Notifications[1].Self, Is.SameAs(b));
            Assert.That(_sink.Notifications[1].Other, Is.SameAs(a));
        }

        [Test]
        public void Unregister_SecondColliderOfActivePair_ExitedOrderFollowsPairOrder()
        {
            var a = CreateCollider("a", overlaps: true);
            var b = CreateCollider("b");
            _system.Register(a);
            _system.Register(b);
            _system.Tick(0f);
            _sink.Notifications.Clear();

            _system.Unregister(b);

            Assert.That(_sink.Notifications.Count, Is.EqualTo(2));
            Assert.That(_sink.Notifications[0].Self, Is.SameAs(a));
            Assert.That(_sink.Notifications[1].Self, Is.SameAs(b));
        }

        [Test]
        public void Unregister_ColliderInManyActivePairs_EndsEveryPairExactlyOnce()
        {
            var hub = CreateCollider("hub", overlaps: true);
            var others = new[] { CreateCollider("o1"), CreateCollider("o2"), CreateCollider("o3") };
            _system.Register(hub);
            foreach (var other in others) _system.Register(other);
            _system.Tick(0f);
            _sink.Notifications.Clear();

            _system.Unregister(hub);

            Assert.That(Count(hub, NotificationKind.Exited), Is.EqualTo(others.Length));
            foreach (var other in others)
            {
                Assert.That(Count(other, NotificationKind.Exited), Is.EqualTo(1), other.Name);
                Assert.That(_sink.For(other)[_sink.For(other).Count - 1].Other, Is.SameAs(hub), other.Name);
            }

            _sink.Notifications.Clear();
            _system.Tick(0f);
            Assert.That(_sink.Notifications, Is.Empty);
        }

        [Test]
        public void Unregister_DuringTickOfActivePair_BothSidesGetExitedAfterTick()
        {
            var a = CreateCollider("a", overlaps: true);
            var b = CreateCollider("b");
            _system.Register(a);
            _system.Register(b);
            _system.Tick(0f);
            _sink.Notifications.Clear();
            _sink[a] = n =>
            {
                if (n.Kind == NotificationKind.Stayed) _system.Unregister(b);
            };

            _system.Tick(0f);

            Assert.That(Count(a, NotificationKind.Exited), Is.EqualTo(1));
            Assert.That(Count(b, NotificationKind.Exited), Is.EqualTo(1));
            Assert.That(_sink.Notifications[_sink.Notifications.Count - 1].Kind, Is.EqualTo(NotificationKind.Exited));
        }

        [Test]
        public void Unregister_FromExitedHandlerOfAnotherUnregister_EachPairEndsOnce()
        {
            var a = CreateCollider("a", overlaps: true);
            var b = CreateCollider("b");
            var c = CreateCollider("c");
            _system.Register(a);
            _system.Register(b);
            _system.Register(c);
            _system.Tick(0f);
            _sink[b] = n =>
            {
                if (n.Kind == NotificationKind.Exited) _system.Unregister(c);
            };

            _system.Unregister(a);

            Assert.That(Count(a, NotificationKind.Exited), Is.EqualTo(2));
            Assert.That(Count(b, NotificationKind.Exited), Is.EqualTo(1));
            Assert.That(Count(c, NotificationKind.Exited), Is.EqualTo(1));
        }

        [Test]
        public void Tick_AfterUnregisterOfActivePair_FiresNoFurtherEvents()
        {
            var a = CreateCollider("a", overlaps: true);
            var b = CreateCollider("b");
            _system.Register(a);
            _system.Register(b);
            _system.Tick(0f);
            _system.Unregister(a);
            _sink.Notifications.Clear();

            _system.Tick(0f);

            Assert.That(_sink.Notifications, Is.Empty);
        }
    }
}
