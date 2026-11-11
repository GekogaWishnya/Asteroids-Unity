using System;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;

namespace Asteroids.Physics.Tests
{
    /// <summary>
    /// <see cref="PoseViewSync"/> took over from the <c>LateUpdate</c> that every EntityComponent used to have.
    /// It must show the latest pose on every registered view each frame, stop at once for a view that left, and not
    /// let one broken view freeze every other object on screen.
    /// </summary>
    [TestFixture]
    public sealed class PoseViewSyncTests
    {
        private sealed class RecordingView : IPoseView
        {
            public List<(Vector2 Position, float Angle)> Shown { get; } = new();
            public Action OnShow { get; set; }

            public (Vector2 Position, float Angle) Last => Shown[Shown.Count - 1];

            public void Show(Vector2 position, float angle)
            {
                Shown.Add((position, angle));
                OnShow?.Invoke();
            }
        }

        private PoseViewSync _sync;

        [SetUp]
        public void SetUp() => _sync = new PoseViewSync();

        private static BodyPose Pose(float x, float y, float angle) =>
            new() { Position = new Vector2(x, y), Angle = angle };

        [Test]
        public void Register_NullArgument_Throws()
        {
            Assert.Throws<ArgumentNullException>(() => _sync.Register(null, new RecordingView()));
            Assert.Throws<ArgumentNullException>(() => _sync.Register(Pose(0f, 0f, 0f), null));
        }

        [Test]
        public void Tick_ShowsEachPoseOnItsOwnView()
        {
            var first = new RecordingView();
            var second = new RecordingView();
            _sync.Register(Pose(1f, 2f, 30f), first);
            _sync.Register(Pose(-3f, 4f, 90f), second);

            _sync.Tick();

            Assert.That(first.Shown, Is.EqualTo(new[] { (new Vector2(1f, 2f), 30f) }));
            Assert.That(second.Shown, Is.EqualTo(new[] { (new Vector2(-3f, 4f), 90f) }));
        }

        /// <summary>The pose is read on every tick, not captured once at registration.</summary>
        [Test]
        public void Tick_ShowsThePoseAsItIsNow()
        {
            var view = new RecordingView();
            var pose = Pose(0f, 0f, 0f);
            _sync.Register(pose, view);
            _sync.Tick();

            pose.Position = new Vector2(5f, 6f);
            pose.Angle = 45f;
            _sync.Tick();

            Assert.That(view.Shown, Has.Count.EqualTo(2));
            Assert.That(view.Last.Position, Is.EqualTo(new Vector2(5f, 6f)));
            Assert.That(view.Last.Angle, Is.EqualTo(45f));
        }

        [Test]
        public void DisposedRegistration_IsNotShownAgain()
        {
            var view = new RecordingView();
            var registration = _sync.Register(Pose(0f, 0f, 0f), view);
            _sync.Tick();

            registration.Dispose();
            _sync.Tick();

            Assert.That(view.Shown, Has.Count.EqualTo(1));
            Assert.That(_sync.Count, Is.Zero);
        }

        [Test]
        public void Dispose_Twice_RemovesOnlyOnce()
        {
            var registration = _sync.Register(Pose(0f, 0f, 0f), new RecordingView());
            _sync.Register(Pose(0f, 0f, 0f), new RecordingView());

            registration.Dispose();
            registration.Dispose();

            Assert.That(_sync.Count, Is.EqualTo(1));
        }

        /// <summary>
        /// One pose shown on two views is two registrations; disposing one leaves the other. This is what lets a
        /// pooled object be released and taken again in the same frame.
        /// </summary>
        [Test]
        public void SamePoseTwice_EachRegistrationIsIndependent()
        {
            var pose = Pose(0f, 0f, 0f);
            var view = new RecordingView();
            var first = _sync.Register(pose, view);
            _sync.Register(pose, view);

            first.Dispose();
            _sync.Tick();

            Assert.That(view.Shown, Has.Count.EqualTo(1));
        }

        /// <summary>A view enabled while views are being shown (a spawn from a view callback) starts next frame.</summary>
        [Test]
        public void RegisterDuringTick_IsShownFromTheNextTick()
        {
            var late = new RecordingView();
            var trigger = new RecordingView();
            trigger.OnShow = () =>
            {
                trigger.OnShow = null;
                _sync.Register(Pose(0f, 0f, 0f), late);
            };
            _sync.Register(Pose(0f, 0f, 0f), trigger);

            _sync.Tick();
            Assert.That(late.Shown, Is.Empty);

            _sync.Tick();
            Assert.That(late.Shown, Has.Count.EqualTo(1));
        }

        [Test]
        public void DisposeDuringTick_TheRestOfThisTickStillRuns_AndItIsGoneNextTick()
        {
            var second = new RecordingView();
            IDisposable secondRegistration = null;
            var first = new RecordingView { OnShow = () => secondRegistration.Dispose() };
            _sync.Register(Pose(0f, 0f, 0f), first);
            secondRegistration = _sync.Register(Pose(0f, 0f, 0f), second);

            _sync.Tick();
            Assert.That(second.Shown, Has.Count.EqualTo(1));

            _sync.Tick();
            Assert.That(second.Shown, Has.Count.EqualTo(1));
        }

        /// <summary>A destroyed transform throws; every other object must still move this frame.</summary>
        [Test]
        public void ThrowingView_OthersAreStillShown_ThenTheFailureIsRethrown()
        {
            var failure = new InvalidOperationException("destroyed");
            var broken = new RecordingView { OnShow = () => throw failure };
            var healthy = new RecordingView();
            _sync.Register(Pose(0f, 0f, 0f), broken);
            _sync.Register(Pose(1f, 1f, 0f), healthy);

            var thrown = Assert.Throws<InvalidOperationException>(() => _sync.Tick());

            Assert.That(thrown, Is.SameAs(failure));
            Assert.That(healthy.Shown, Has.Count.EqualTo(1));
        }

        /// <summary>The list must be unlocked after a failure, or no registration would ever apply again.</summary>
        [Test]
        public void ThrowingView_RegistrationsStillApplyAfterwards()
        {
            var broken = new RecordingView { OnShow = () => throw new InvalidOperationException() };
            _sync.Register(Pose(0f, 0f, 0f), broken);
            Assert.Throws<InvalidOperationException>(() => _sync.Tick());

            var view = new RecordingView();
            _sync.Register(Pose(0f, 0f, 0f), view);

            Assert.That(_sync.Count, Is.EqualTo(2));
        }
    }
}
