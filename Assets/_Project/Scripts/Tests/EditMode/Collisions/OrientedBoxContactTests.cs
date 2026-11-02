using System;
using NUnit.Framework;
using UnityEngine;

namespace Asteroids.Collisions.Tests
{
    /// <summary>Contact math of <see cref="OrientedBoxShape"/> against every other shape.</summary>
    [TestFixture]
    public sealed class OrientedBoxContactTests
    {
        private const float Tolerance = 1e-4f;

        private ShapeContactTable _table;

        [SetUp]
        public void SetUp() => _table = ShapeContacts.CreateTable();

        private static OrientedBoxShape Box(
            Vector2 position, Vector2 size, float angle = 0f, Vector2 offset = default) =>
            new(new TestPose(position, angle), offset, size);

        private static CircleShape Circle(Vector2 position, float radius) =>
            new(new TestPose(position), Vector2.zero, radius);

        private static void AssertVector(Vector2 actual, Vector2 expected)
        {
            Assert.That(actual.x, Is.EqualTo(expected.x).Within(Tolerance), "x");
            Assert.That(actual.y, Is.EqualTo(expected.y).Within(Tolerance), "y");
        }

        // ---- shape ----

        [Test]
        public void Construct_NegativeOrNonFiniteSide_Throws()
        {
            var pose = new TestPose(Vector2.zero);

            Assert.Throws<ArgumentOutOfRangeException>(() => new OrientedBoxShape(pose, Vector2.zero, new Vector2(-1f, 1f)));
            Assert.Throws<ArgumentOutOfRangeException>(() => new OrientedBoxShape(pose, Vector2.zero, new Vector2(1f, float.NaN)));
        }

        [Test]
        public void Construct_NullPose_Throws() =>
            Assert.Throws<ArgumentNullException>(() => new OrientedBoxShape(null, Vector2.zero, Vector2.one));

        [Test]
        public void Center_FollowsThePoseAndTurnsTheOffset()
        {
            var pose = new TestPose(new Vector2(1f, 1f), 90f); // Up is -X.
            var box = new OrientedBoxShape(pose, new Vector2(0f, 5f), new Vector2(1f, 10f));

            AssertVector(box.Center, new Vector2(-4f, 1f));
        }

        // ---- box vs circle ----

        [Test]
        public void BoxWithCircle_CircleOverlapsASide_ContactPointsFromTheCircleToTheBox()
        {
            var box = Box(Vector2.zero, new Vector2(2f, 10f));
            var circle = Circle(new Vector2(1.4f, 0f), 0.5f);

            Assert.That(_table.TryGetContact(box, circle, out var contact), Is.True);
            AssertVector(contact.Normal, new Vector2(-1f, 0f));
            Assert.That(contact.Depth, Is.EqualTo(0.1f).Within(Tolerance));
        }

        [Test]
        public void BoxWithCircle_CircleOverlapsACorner_ContactPointsAlongTheDiagonal()
        {
            var box = Box(Vector2.zero, new Vector2(2f, 2f));
            var circle = Circle(new Vector2(1.3f, 1.3f), 0.5f);

            Assert.That(_table.TryGetContact(box, circle, out var contact), Is.True);
            var diagonal = -new Vector2(1f, 1f).normalized;
            AssertVector(contact.Normal, diagonal);
            Assert.That(contact.Depth, Is.EqualTo(0.5f - Mathf.Sqrt(0.18f)).Within(Tolerance));
        }

        [Test]
        public void BoxWithCircle_CircleBeyondACorner_NoContact()
        {
            // Inside the box's bounds on both axes plus the radius, but out of reach of the rounded corner.
            var box = Box(Vector2.zero, new Vector2(2f, 2f));
            var circle = Circle(new Vector2(1.4f, 1.4f), 0.5f);

            Assert.That(_table.TryGetContact(box, circle, out _), Is.False);
        }

        [Test]
        public void BoxWithCircle_CircleOutOfReach_NoContact()
        {
            var box = Box(Vector2.zero, new Vector2(2f, 10f));
            var circle = Circle(new Vector2(1.6f, 0f), 0.5f);

            Assert.That(_table.TryGetContact(box, circle, out _), Is.False);
        }

        [Test]
        public void BoxWithCircle_CircleCenterInside_PushesOutThroughTheNearestSide()
        {
            var box = Box(Vector2.zero, new Vector2(2f, 10f));
            var circle = Circle(new Vector2(0.5f, 0f), 0.5f);

            Assert.That(_table.TryGetContact(box, circle, out var contact), Is.True);
            AssertVector(contact.Normal, new Vector2(-1f, 0f));
            Assert.That(contact.Depth, Is.EqualTo(1f).Within(Tolerance));
        }

        [Test]
        public void BoxWithCircle_TurnedBox_TestsAlongItsOwnAxes()
        {
            // Turned by 90 degrees the long side lies along -X.
            var box = Box(Vector2.zero, new Vector2(2f, 10f), angle: 90f);

            Assert.That(_table.TryGetContact(box, Circle(new Vector2(-4.8f, 0f), 0.5f), out var contact), Is.True);
            AssertVector(contact.Normal, new Vector2(1f, 0f));
            Assert.That(contact.Depth, Is.EqualTo(0.7f).Within(Tolerance));

            Assert.That(_table.TryGetContact(box, Circle(new Vector2(0f, 1.6f), 0.5f), out _), Is.False);
        }

        [Test]
        public void BoxWithCircle_OffsetBox_ReachesForwardFromThePose()
        {
            // A beam: the pose is its start and it reaches 10 units along Up.
            var forward = Box(Vector2.zero, new Vector2(1f, 10f), offset: new Vector2(0f, 5f));
            var backward = Box(Vector2.zero, new Vector2(1f, 10f), angle: 180f, offset: new Vector2(0f, 5f));

            Assert.That(_table.TryGetContact(forward, Circle(new Vector2(0f, 9f), 0.1f), out _), Is.True);
            Assert.That(_table.TryGetContact(forward, Circle(new Vector2(0f, -1f), 0.1f), out _), Is.False);
            Assert.That(_table.TryGetContact(backward, Circle(new Vector2(0f, -9f), 0.1f), out _), Is.True);
        }

        [Test]
        public void CircleWithBox_ReversedOrder_InvertsTheNormal()
        {
            var box = Box(Vector2.zero, new Vector2(2f, 10f));
            var circle = Circle(new Vector2(1.4f, 0f), 0.5f);

            Assert.That(_table.TryGetContact(circle, box, out var contact), Is.True);
            AssertVector(contact.Normal, new Vector2(1f, 0f));
        }

        // ---- box vs boundary ----

        [Test]
        public void BoxWithBoundary_CornerBeyondTheEdge_ContactPointsInward()
        {
            var box = Box(new Vector2(9.5f, 0f), new Vector2(2f, 2f));
            var wall = new BoundaryShape(HalfPlane.MaxX(10f));

            Assert.That(_table.TryGetContact(box, wall, out var contact), Is.True);
            AssertVector(contact.Normal, new Vector2(-1f, 0f));
            Assert.That(contact.Depth, Is.EqualTo(0.5f).Within(Tolerance));
        }

        [Test]
        public void BoxWithBoundary_TurnedBox_UsesItsDeepestCorner()
        {
            // A 2x2 box turned by 45 degrees reaches sqrt(2) from its center.
            var box = Box(new Vector2(9f, 0f), new Vector2(2f, 2f), angle: 45f);
            var wall = new BoundaryShape(HalfPlane.MaxX(10f));

            Assert.That(_table.TryGetContact(box, wall, out var contact), Is.True);
            Assert.That(contact.Depth, Is.EqualTo(Mathf.Sqrt(2f) - 1f).Within(Tolerance));
        }

        [Test]
        public void BoxWithBoundary_Inside_NoContact()
        {
            var box = Box(new Vector2(8f, 0f), new Vector2(2f, 2f));
            var wall = new BoundaryShape(HalfPlane.MaxX(10f));

            Assert.That(_table.TryGetContact(box, wall, out _), Is.False);
        }

        // ---- box vs box ----

        [Test]
        public void BoxWithBox_Overlapping_ContactPointsFromSecondToFirstAlongTheShallowestAxis()
        {
            var first = Box(Vector2.zero, new Vector2(2f, 2f));
            var second = Box(new Vector2(1.5f, 0f), new Vector2(2f, 2f));

            Assert.That(_table.TryGetContact(first, second, out var contact), Is.True);
            AssertVector(contact.Normal, new Vector2(-1f, 0f));
            Assert.That(contact.Depth, Is.EqualTo(0.5f).Within(Tolerance));
        }

        [Test]
        public void BoxWithBox_Separated_NoContact()
        {
            var first = Box(Vector2.zero, new Vector2(2f, 2f));
            var second = Box(new Vector2(2.5f, 0f), new Vector2(2f, 2f));

            Assert.That(_table.TryGetContact(first, second, out _), Is.False);
        }

        [Test]
        public void BoxWithBox_TurnedSecond_OverlapsByItsCorner()
        {
            var first = Box(Vector2.zero, new Vector2(2f, 2f));
            var close = Box(new Vector2(2.3f, 0f), new Vector2(2f, 2f), angle: 45f);
            var far = Box(new Vector2(2.5f, 0f), new Vector2(2f, 2f), angle: 45f);

            Assert.That(_table.TryGetContact(first, close, out var contact), Is.True);
            Assert.That(contact.Depth, Is.EqualTo(1f + Mathf.Sqrt(2f) - 2.3f).Within(Tolerance));
            Assert.That(_table.TryGetContact(first, far, out _), Is.False);
        }

        // ---- table ----

        [Test]
        public void Supports_EveryPairWithABox_InEitherOrder()
        {
            Assert.That(_table.Supports(typeof(OrientedBoxShape), typeof(OrientedBoxShape)), Is.True);
            Assert.That(_table.Supports(typeof(OrientedBoxShape), typeof(CircleShape)), Is.True);
            Assert.That(_table.Supports(typeof(CircleShape), typeof(OrientedBoxShape)), Is.True);
            Assert.That(_table.Supports(typeof(OrientedBoxShape), typeof(BoundaryShape)), Is.True);
            Assert.That(_table.Supports(typeof(BoundaryShape), typeof(OrientedBoxShape)), Is.True);
        }
    }
}
