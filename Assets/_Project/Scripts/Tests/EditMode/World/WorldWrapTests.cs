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
        public void SetUp() => _wrap = new WorldWrap(Bounds(), new TestSpawnEntryConfig(1f, MaxEntering));

        private const float MaxEntering = 1f;

        private static WorldBounds Bounds() => new(new TestWorldLayout(256, 256, 16));

        private static void AssertPosition(IPose pose, float x, float y)
        {
            Assert.That(pose.Position.x, Is.EqualTo(x).Within(Tolerance), "x");
            Assert.That(pose.Position.y, Is.EqualTo(y).Within(Tolerance), "y");
        }

        [Test]
        public void Constructor_NullBounds_Throws() =>
            Assert.Throws<ArgumentNullException>(() => new WorldWrap(null, new TestSpawnEntryConfig(1f, 1f)));

        [Test]
        public void Constructor_NullEntryConfig_Throws() =>
            Assert.Throws<ArgumentNullException>(() => new WorldWrap(Bounds(), null));

        [Test]
        public void Constructor_InvalidEntryConfig_Throws() =>
            Assert.Throws<InvalidOperationException>(() => new WorldWrap(Bounds(), new TestSpawnEntryConfig(1f, 0f)));

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
        [Test]
        public void Tick_EnteringPoseOutside_IsLeftAlone()
        {
            var pose = new Pose { Position = new Vector2(-9.5f, 2f) };
            _wrap.RegisterEntering(pose);

            _wrap.Tick(0.1f);

            AssertPosition(pose, -9.5f, 2f);
            Assert.That(_wrap.EnteringCount, Is.EqualTo(1));
        }

        [Test]
        public void Tick_EnteringPoseThatWasInside_WrapsFromThenOn()
        {
            var pose = new Pose { Position = new Vector2(-9.5f, 2f) };
            _wrap.RegisterEntering(pose);
            _wrap.Tick(0.1f);

            pose.Position = new Vector2(-7.5f, 2f);
            _wrap.Tick(0.1f);
            Assert.That(_wrap.EnteringCount, Is.Zero);

            pose.Position = new Vector2(-8.5f, 2f);
            _wrap.Tick(0.1f);

            AssertPosition(pose, 7.5f, 2f);
        }

        [Test]
        public void Tick_EnteringPoseRegisteredInside_WrapsAfterItsFirstTick()
        {
            var pose = new Pose { Position = new Vector2(0f, 0f) };
            _wrap.RegisterEntering(pose);
            _wrap.Tick(0.1f);

            pose.Position = new Vector2(8.5f, 0f);
            _wrap.Tick(0.1f);

            AssertPosition(pose, -7.5f, 0f);
        }

        [Test]
        public void Tick_EnteringPoseOutsideTooLong_IsMovedOntoTheNearestEdge()
        {
            var pose = new Pose { Position = new Vector2(-13f, 20f) };
            _wrap.RegisterEntering(pose);

            _wrap.Tick(MaxEntering * 0.6f);
            AssertPosition(pose, -13f, 20f);

            _wrap.Tick(MaxEntering * 0.6f);

            AssertPosition(pose, -8f, 7.999f);
            Assert.That(_wrap.EnteringCount, Is.Zero);
        }

        [Test]
        public void Tick_AfterTheTimeout_WrapsLikeAnyOtherPose()
        {
            var pose = new Pose { Position = new Vector2(9.5f, 0f) };
            _wrap.RegisterEntering(pose);
            _wrap.Tick(MaxEntering * 2f);

            pose.Position = new Vector2(8.5f, 0f);
            _wrap.Tick(0.02f);

            AssertPosition(pose, -7.5f, 0f);
        }

        [Test]
        public void Dispose_EnteringPose_ForgetsIt()
        {
            var pose = new Pose { Position = new Vector2(9.5f, 0f) };
            var handle = _wrap.RegisterEntering(pose);

            handle.Dispose();
            _wrap.Tick(MaxEntering * 2f);

            AssertPosition(pose, 9.5f, 0f);
            Assert.That(_wrap.EnteringCount, Is.Zero);
            Assert.That(_wrap.Count, Is.Zero);
        }

        [Test]
        public void Register_AfterEnteringDispose_WrapsAtOnce()
        {
            var pose = new Pose { Position = new Vector2(9.5f, 0f) };
            _wrap.RegisterEntering(pose).Dispose();
            _wrap.Register(pose);

            _wrap.Tick(0.02f);

            AssertPosition(pose, -6.5f, 0f);
        }

        [Test]
        public void RegisterEntering_NullOrTwice_Throws()
        {
            var pose = new Pose();
            _wrap.Register(pose);

            Assert.Throws<ArgumentNullException>(() => _wrap.RegisterEntering(null));
            Assert.Throws<InvalidOperationException>(() => _wrap.RegisterEntering(pose));
        }
    }
}
