using System;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;

namespace Asteroids.Collisions.Tests
{
    [TestFixture]
    public sealed class CollisionSystemTests
    {
        private const float Tolerance = 1e-5f;

        private CollisionSystem _system;
        private List<Notification> _journal;

        [SetUp]
        public void SetUp()
        {
            _system = new CollisionSystem();
            _journal = new List<Notification>();
        }

        private FakeCollider CreateCollider(string name, bool overlaps = false)
        {
            var collider = new FakeCollider(name, _journal) { Overlaps = overlaps };
            return collider;
        }

        private static int Count(FakeCollider collider, NotificationKind kind)
        {
            var count = 0;
            foreach (var notification in collider.Notifications)
                if (notification.Kind == kind) count++;
            return count;
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
            a.OnNotified = _ => _system.Tick(0f);
            _system.Register(a);
            _system.Register(b);

            Assert.Throws<InvalidOperationException>(() => _system.Tick(0f));
        }

        [Test]
        public void Tick_AfterHandlerThrew_SystemIsUsableAgain()
        {
            var a = CreateCollider("a", overlaps: true);
            var b = CreateCollider("b");
            a.OnNotified = _ => throw new InvalidOperationException("boom");
            _system.Register(a);
            _system.Register(b);
            Assert.Throws<InvalidOperationException>(() => _system.Tick(0f));

            a.OnNotified = null;

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

            Assert.That(a.Notifications.Count, Is.EqualTo(1));
            Assert.That(a.Notifications[0].Kind, Is.EqualTo(NotificationKind.Entered));
            Assert.That(a.Notifications[0].Other, Is.SameAs(b));
            Assert.That(b.Notifications.Count, Is.EqualTo(1));
            Assert.That(b.Notifications[0].Kind, Is.EqualTo(NotificationKind.Entered));
            Assert.That(b.Notifications[0].Other, Is.SameAs(a));
        }

        [Test]
        public void Tick_FirstOverlap_FiresEnteredFirstThenSecondInRegistrationOrder()
        {
            var a = CreateCollider("a", overlaps: true);
            var b = CreateCollider("b");
            _system.Register(a);
            _system.Register(b);

            _system.Tick(0f);

            Assert.That(_journal.Count, Is.EqualTo(2));
            Assert.That(_journal[0].Self, Is.SameAs(a));
            Assert.That(_journal[0].Other, Is.SameAs(b));
            Assert.That(_journal[1].Self, Is.SameAs(b));
            Assert.That(_journal[1].Other, Is.SameAs(a));
        }

        [Test]
        public void Tick_ContinuedOverlap_FiresStayedNotEntered()
        {
            var a = CreateCollider("a", overlaps: true);
            var b = CreateCollider("b");
            _system.Register(a);
            _system.Register(b);
            _system.Tick(0f);
            _journal.Clear();

            _system.Tick(0f);

            Assert.That(_journal.Count, Is.EqualTo(2));
            Assert.That(_journal[0].Kind, Is.EqualTo(NotificationKind.Stayed));
            Assert.That(_journal[0].Self, Is.SameAs(a));
            Assert.That(_journal[1].Kind, Is.EqualTo(NotificationKind.Stayed));
            Assert.That(_journal[1].Self, Is.SameAs(b));
            Assert.That(Count(a, NotificationKind.Entered), Is.EqualTo(1));
            Assert.That(Count(b, NotificationKind.Entered), Is.EqualTo(1));
        }

        [Test]
        public void Tick_OverlapEnds_FiresExitedOnBothSides()
        {
            var a = CreateCollider("a", overlaps: true);
            var b = CreateCollider("b");
            _system.Register(a);
            _system.Register(b);
            _system.Tick(0f);
            _journal.Clear();
            a.Overlaps = false;

            _system.Tick(0f);

            Assert.That(_journal.Count, Is.EqualTo(2));
            Assert.That(_journal[0].Kind, Is.EqualTo(NotificationKind.Exited));
            Assert.That(_journal[0].Self, Is.SameAs(a));
            Assert.That(_journal[0].Other, Is.SameAs(b));
            Assert.That(_journal[1].Kind, Is.EqualTo(NotificationKind.Exited));
            Assert.That(_journal[1].Self, Is.SameAs(b));
            Assert.That(_journal[1].Other, Is.SameAs(a));
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
            _journal.Clear();

            _system.Tick(0f);

            Assert.That(_journal, Is.Empty);
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

            Assert.That(_journal, Is.Empty);
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

            var first = a.Notifications[0].Contact;
            var second = b.Notifications[0].Contact;
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

            Assert.That(a.Notifications, Is.Empty);
            Assert.That(b.Notifications.Count, Is.EqualTo(1));
            Assert.That(b.Notifications[0].Other, Is.SameAs(c));
            Assert.That(c.Notifications.Count, Is.EqualTo(1));
            Assert.That(c.Notifications[0].Other, Is.SameAs(b));
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
            Assert.That(_journal, Is.Empty);
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
            Assert.That(_journal, Is.Empty);
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
            a.OnNotified = _ =>
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
            a.OnNotified = _ =>
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
            a.OnNotified = _ => _system.Unregister(b);
            _system.Register(a);
            _system.Register(b);
            _system.Tick(0f);
            a.ContactQueries.Clear();

            _system.Tick(0f);

            Assert.That(a.ContactQueries, Is.Empty);
        }

        // ---- Unregister of an active pair ----
        
        [Test]
        public void Unregister_ActivePair_PartnerGetsExited_UnregisteredDoesNot()
        {
            var a = CreateCollider("a", overlaps: true);
            var b = CreateCollider("b");
            _system.Register(a);
            _system.Register(b);
            _system.Tick(0f);
            _journal.Clear();

            _system.Unregister(a);

            Assert.That(Count(b, NotificationKind.Exited), Is.EqualTo(1));
            Assert.That(b.Notifications[^1].Other, Is.SameAs(a));
            Assert.That(Count(a, NotificationKind.Exited), Is.EqualTo(0));
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
            _journal.Clear();

            _system.Tick(0f);

            Assert.That(_journal, Is.Empty);
        }
    }
}
