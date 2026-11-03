using System;
using Asteroids.Core;
using NUnit.Framework;
using UnityEngine;

namespace Asteroids.World.Tests
{
    [TestFixture]
    public sealed class WorldWrapTests
    {
        private const float Tolerance = 1e-4f;

        private sealed class Pose : IPose
        {
            public Vector2 Position { get; set; }
            public float Angle { get; set; }
            public Vector2 Up => Vector2.up;
            public Vector2 Right => Vector2.right;
        }

        private WorldWrap _wrap;

        // 256 x 256 px at 16 PPU: the area is [-8, 8) on both axes.
        [SetUp]
        public void SetUp() => _wrap = new WorldWrap(new WorldBounds(new TestWorldLayout(256, 256, 16)));

        private static void AssertPosition(IPose pose, float x, float y)
        {
            Assert.That(pose.Position.x, Is.EqualTo(x).Within(Tolerance), "x");
            Assert.That(pose.Position.y, Is.EqualTo(y).Within(Tolerance), "y");
        }

        [Test]
        public void Constructor_NullBounds_Throws() =>
            Assert.Throws<ArgumentNullException>(() => new WorldWrap(null));

        [Test]
        public void Tick_RegisteredPoseOutside_IsMovedToTheOppositeSide()
        {
            var pose = new Pose { Position = new Vector2(8.5f, -9f) };
            _wrap.Register(pose);

            _wrap.Tick(0.02f);

            AssertPosition(pose, -7.5f, 7f);
        }

        [Test]
        public void Tick_RegisteredPoseInside_IsLeftAlone()
        {
            var pose = new Pose { Position = new Vector2(3f, -2f) };
            _wrap.Register(pose);

            _wrap.Tick(0.02f);

            AssertPosition(pose, 3f, -2f);
        }

        [Test]
        public void Tick_UnregisteredPose_IsNotWrapped()
        {
            var pose = new Pose { Position = new Vector2(20f, 0f) };

            _wrap.Tick(0.02f);

            AssertPosition(pose, 20f, 0f);
        }

        [Test]
        public void Dispose_StopsWrappingAndTwiceDoesNothing()
        {
            var pose = new Pose();
            var handle = _wrap.Register(pose);

            handle.Dispose();
            handle.Dispose();
            pose.Position = new Vector2(20f, 0f);
            _wrap.Tick(0.02f);

            AssertPosition(pose, 20f, 0f);
            Assert.That(_wrap.Count, Is.Zero);
        }

        [Test]
        public void Register_NullOrTwice_Throws()
        {
            var pose = new Pose();
            _wrap.Register(pose);

            Assert.Throws<ArgumentNullException>(() => _wrap.Register(null));
            Assert.Throws<InvalidOperationException>(() => _wrap.Register(pose));
        }

        [Test]
        public void Register_AfterDispose_WrapsAgain()
        {
            var pose = new Pose();
            _wrap.Register(pose).Dispose();
            _wrap.Register(pose);
            pose.Position = new Vector2(9f, 0f);

            _wrap.Tick(0.02f);

            AssertPosition(pose, -7f, 0f);
        }
    }
}
