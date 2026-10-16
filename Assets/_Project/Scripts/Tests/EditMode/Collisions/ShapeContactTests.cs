using System;
using NUnit.Framework;
using UnityEngine;

namespace Asteroids.Collisions.Tests
{
    /// <summary>Contact math of shapes.</summary>
    [TestFixture]
    public sealed class ShapeContactTests
    {
        private const float Tolerance = 1e-4f;
        private const float Radius = 0.5f;

        public enum Wall
        {
            MinX,
            MaxX,
            MinY,
            MaxY
        }

        private ShapeContactTable _table;

        [SetUp]
        public void SetUp() => _table = ShapeContacts.CreateTable();

        private static CircleShape Circle(Vector2 position, float radius, Vector2 offset = default, float angle = 0f) =>
            new(new TestPose(position, angle), offset, radius);

        private static BoundaryShape Boundary(HalfPlane region) => new(region);

        private static void AssertVector(Vector2 actual, Vector2 expected)
        {
            Assert.That(actual.x, Is.EqualTo(expected.x).Within(Tolerance));
            Assert.That(actual.y, Is.EqualTo(expected.y).Within(Tolerance));
        }

        // ---- circle vs circle ----

        [Test]
        public void CircleWithCircle_Overlapping_ContactPointsFromSecondToFirst()
        {
            var first = Circle(new Vector2(0f, 0f), 0.75f);
            var second = Circle(new Vector2(1f, 0f), 0.75f);

            var found = _table.TryGetContact(first, second, out var contact);

            Assert.That(found, Is.True);
            AssertVector(contact.Normal, new Vector2(-1f, 0f));
            Assert.That(contact.Depth, Is.EqualTo(0.5f).Within(Tolerance));
        }

        [Test]
        public void CircleWithCircle_DistanceEqualsSumOfRadii_NoContact()
        {
            var first = Circle(new Vector2(0f, 0f), 1f);
            var second = Circle(new Vector2(2f, 0f), 1f);

            Assert.That(_table.TryGetContact(first, second, out _), Is.False);
        }

        [Test]
        public void CircleWithCircle_Separated_NoContact()
        {
            var first = Circle(new Vector2(0f, 0f), 1f);
            var second = Circle(new Vector2(5f, 0f), 1f);

            Assert.That(_table.TryGetContact(first, second, out _), Is.False);
        }

        [Test]
        public void CircleWithCircle_CoincidentCenters_UsesUpNormalAndFullDepth()
        {
            var first = Circle(new Vector2(3f, 3f), 1f);
            var second = Circle(new Vector2(3f, 3f), 0.5f);

            var found = _table.TryGetContact(first, second, out var contact);

            Assert.That(found, Is.True);
            AssertVector(contact.Normal, Vector2.up);
            Assert.That(contact.Depth, Is.EqualTo(1.5f).Within(Tolerance));
        }

        [Test]
        public void CircleWithCircle_OffsetInRotatedPose_CenterFollowsPoseAxes()
        {
            // Angle 90: Right = (0, 1), so local offset (1, 0) lands one unit above the pose position.
            var first = Circle(new Vector2(0f, 0f), 0.6f, new Vector2(1f, 0f), angle: 90f);
            var second = Circle(new Vector2(0f, 2f), 0.6f);

            var found = _table.TryGetContact(first, second, out var contact);

            Assert.That(found, Is.True);
            AssertVector(contact.Normal, new Vector2(0f, -1f));
            Assert.That(contact.Depth, Is.EqualTo(0.2f).Within(Tolerance));
        }

        [Test]
        public void CircleWithCircle_OffsetAlongLocalUpInRotatedPose_CenterFollowsPoseAxes()
        {
            // Angle 90: Up = (-1, 0), so local offset (0, 1) lands one unit to the left of the pose position.
            var first = Circle(new Vector2(0f, 0f), 0.6f, new Vector2(0f, 1f), angle: 90f);
            var second = Circle(new Vector2(-2f, 0f), 0.6f);

            var found = _table.TryGetContact(first, second, out var contact);

            Assert.That(found, Is.True);
            AssertVector(contact.Normal, new Vector2(1f, 0f));
            Assert.That(contact.Depth, Is.EqualTo(0.2f).Within(Tolerance));
        }

        [Test]
        public void CircleWithCircle_PoseMovesAfterConstruction_CenterIsReadAtTestTime()
        {
            var pose = new TestPose(new Vector2(10f, 0f));
            var first = new CircleShape(pose, Vector2.zero, 1f);
            var second = Circle(new Vector2(0f, 0f), 1f);
            Assert.That(_table.TryGetContact(first, second, out _), Is.False);

            pose.Position = new Vector2(1f, 0f);

            Assert.That(_table.TryGetContact(first, second, out _), Is.True);
        }

        [Test]
        public void CircleWithCircle_SecondOrder_ContactIsInvertedWithEqualDepth()
        {
            var first = Circle(new Vector2(0f, 0f), 0.75f);
            var second = Circle(new Vector2(1f, 0f), 0.75f);
            _table.TryGetContact(first, second, out var forward);

            var found = _table.TryGetContact(second, first, out var backward);

            Assert.That(found, Is.True);
            AssertVector(backward.Normal, -forward.Normal);
            Assert.That(backward.Depth, Is.EqualTo(forward.Depth).Within(Tolerance));
        }

        // ---- circle vs boundary ----
        // Every wall passes through coordinate 3 on its axis; "inward" is the direction into the allowed area.

        private static HalfPlane Region(Wall wall) => wall switch
        {
            Wall.MinX => HalfPlane.MinX(3f),
            Wall.MaxX => HalfPlane.MaxX(3f),
            Wall.MinY => HalfPlane.MinY(3f),
            Wall.MaxY => HalfPlane.MaxY(3f),
            _ => throw new ArgumentOutOfRangeException(nameof(wall))
        };

        private static Vector2 Inward(Wall wall) => wall switch
        {
            Wall.MinX => Vector2.right,
            Wall.MaxX => Vector2.left,
            Wall.MinY => Vector2.up,
            Wall.MaxY => Vector2.down,
            _ => throw new ArgumentOutOfRangeException(nameof(wall))
        };

        private static Vector2 EdgePoint(Wall wall) =>
            wall == Wall.MinX || wall == Wall.MaxX ? new Vector2(3f, 7f) : new Vector2(7f, 3f);

        /// <summary>Circle whose center is <paramref name="distance"/> inside the wall (negative: beyond it).</summary>
        private static CircleShape CircleAt(Wall wall, float distance) =>
            Circle(EdgePoint(wall) + Inward(wall) * distance, Radius);

        [TestCase(Wall.MinX)]
        [TestCase(Wall.MaxX)]
        [TestCase(Wall.MinY)]
        [TestCase(Wall.MaxY)]
        public void CircleWithBoundary_FarInside_NoContact(Wall wall)
        {
            Assert.That(_table.TryGetContact(CircleAt(wall, 2f), Boundary(Region(wall)), out _), Is.False);
        }

        [TestCase(Wall.MinX)]
        [TestCase(Wall.MaxX)]
        [TestCase(Wall.MinY)]
        [TestCase(Wall.MaxY)]
        public void CircleWithBoundary_TouchingEdgeFromInside_NoContact(Wall wall)
        {
            Assert.That(_table.TryGetContact(CircleAt(wall, Radius), Boundary(Region(wall)), out _), Is.False);
        }

        [TestCase(Wall.MinX)]
        [TestCase(Wall.MaxX)]
        [TestCase(Wall.MinY)]
        [TestCase(Wall.MaxY)]
        public void CircleWithBoundary_PartiallyPenetrating_ContactWithInwardNormalAndDepth(Wall wall)
        {
            var found = _table.TryGetContact(CircleAt(wall, 0.2f), Boundary(Region(wall)), out var contact);

            Assert.That(found, Is.True);
            AssertVector(contact.Normal, Inward(wall));
            Assert.That(contact.Depth, Is.EqualTo(0.3f).Within(Tolerance));
        }

        [TestCase(Wall.MinX)]
        [TestCase(Wall.MaxX)]
        [TestCase(Wall.MinY)]
        [TestCase(Wall.MaxY)]
        public void CircleWithBoundary_CenterOnEdge_DepthIsRadius(Wall wall)
        {
            var found = _table.TryGetContact(CircleAt(wall, 0f), Boundary(Region(wall)), out var contact);

            Assert.That(found, Is.True);
            Assert.That(contact.Depth, Is.EqualTo(Radius).Within(Tolerance));
        }

        [TestCase(Wall.MinX)]
        [TestCase(Wall.MaxX)]
        [TestCase(Wall.MinY)]
        [TestCase(Wall.MaxY)]
        public void CircleWithBoundary_FullyBeyondEdge_StillContactWithDepthGrowing(Wall wall)
        {
            var found = _table.TryGetContact(CircleAt(wall, -2f), Boundary(Region(wall)), out var contact);

            Assert.That(found, Is.True);
            AssertVector(contact.Normal, Inward(wall));
            Assert.That(contact.Depth, Is.EqualTo(2.5f).Within(Tolerance));
        }

        [TestCase(Wall.MinX)]
        [TestCase(Wall.MaxX)]
        [TestCase(Wall.MinY)]
        [TestCase(Wall.MaxY)]
        public void BoundaryWithCircle_PartiallyPenetrating_ContactIsInvertedWithSameDepth(Wall wall)
        {
            var found = _table.TryGetContact(Boundary(Region(wall)), CircleAt(wall, 0.2f), out var contact);

            Assert.That(found, Is.True);
            AssertVector(contact.Normal, -Inward(wall));
            Assert.That(contact.Depth, Is.EqualTo(0.3f).Within(Tolerance));
        }

        [Test]
        public void BoundaryWithCircle_FarInside_NoContact()
        {
            Assert.That(_table.TryGetContact(Boundary(HalfPlane.MinX(3f)), CircleAt(Wall.MinX, 2f), out _), Is.False);
        }

        // ---- boundary vs boundary ----

        [Test]
        public void BoundaryWithBoundary_AnyWalls_NoContact()
        {
            var left = Boundary(HalfPlane.MinX(0f));
            var right = Boundary(HalfPlane.MaxX(0f));

            Assert.That(_table.TryGetContact(left, right, out _), Is.False);
            Assert.That(_table.TryGetContact(left, left, out _), Is.False);
        }

        // ---- shape construction ----

        [Test]
        public void BoundaryShape_DefaultRegion_Throws()
        {
            Assert.Throws<ArgumentException>(() => new BoundaryShape(default));
        }

        [Test]
        public void CircleShape_InvalidRadius_Throws()
        {
            var pose = new TestPose(Vector2.zero);

            Assert.Throws<ArgumentOutOfRangeException>(() => new CircleShape(pose, Vector2.zero, -1f));
            Assert.Throws<ArgumentOutOfRangeException>(() => new CircleShape(pose, Vector2.zero, float.NaN));
        }

        // ---- unknown pairs and table rules ----

        [Test]
        public void Supports_BuiltInPairsInEitherOrder_True()
        {
            Assert.That(_table.Supports(typeof(CircleShape), typeof(CircleShape)), Is.True);
            Assert.That(_table.Supports(typeof(CircleShape), typeof(BoundaryShape)), Is.True);
            Assert.That(_table.Supports(typeof(BoundaryShape), typeof(CircleShape)), Is.True);
            Assert.That(_table.Supports(typeof(BoundaryShape), typeof(BoundaryShape)), Is.True);
        }

        [Test]
        public void Supports_UnknownShape_False()
        {
            Assert.That(_table.Supports(typeof(UnknownShape), typeof(CircleShape)), Is.False);
            Assert.That(_table.Supports(typeof(CircleShape), typeof(UnknownShape)), Is.False);
        }

        [Test]
        public void TryGetContact_UnknownPair_ThrowsNotSupported()
        {
            Assert.Throws<NotSupportedException>(
                () => _table.TryGetContact(new UnknownShape(), Circle(Vector2.zero, 1f), out _));
        }

        [Test]
        public void Add_RuleForAlreadyCoveredPair_ThrowsInvalidOperation()
        {
            Assert.Throws<InvalidOperationException>(() =>
                _table.Add<BoundaryShape, CircleShape>((BoundaryShape _, CircleShape __, out Contact contact) =>
                {
                    contact = default;
                    return false;
                }));
        }

        [Test]
        public void Add_NullRule_ThrowsArgumentNull()
        {
            Assert.Throws<ArgumentNullException>(() => new ShapeContactTable().Add<CircleShape, CircleShape>(null));
        }

        [Test]
        public void Add_NewShapePair_ServesBothOrdersWithoutTouchingBuiltIns()
        {
            var table = ShapeContacts.CreateTable()
                .Add<UnknownShape, CircleShape>((UnknownShape _, CircleShape __, out Contact contact) =>
                {
                    contact = new Contact(Vector2.right, 2f);
                    return true;
                });
            var unknown = new UnknownShape();
            var circle = Circle(Vector2.zero, 1f);

            var forward = table.TryGetContact(unknown, circle, out var direct);
            var backward = table.TryGetContact(circle, unknown, out var reversed);

            Assert.That(forward, Is.True);
            Assert.That(backward, Is.True);
            AssertVector(direct.Normal, Vector2.right);
            AssertVector(reversed.Normal, Vector2.left);
            Assert.That(reversed.Depth, Is.EqualTo(direct.Depth).Within(Tolerance));
        }
    }
}
