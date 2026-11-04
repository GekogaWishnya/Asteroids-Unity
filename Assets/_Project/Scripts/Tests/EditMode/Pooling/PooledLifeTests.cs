using System;
using System.Collections.Generic;
using Asteroids.Core;
using Asteroids.Physics;
using NUnit.Framework;
using UnityEngine;

namespace Asteroids.Pooling.Tests
{
    [TestFixture]
    public sealed class PooledLifeTests
    {
        private sealed class Handle : IDisposable
        {
            private readonly List<string> _log;
            private readonly string _name;

            public Handle(List<string> log, string name)
            {
                _log = log;
                _name = name;
                log.Add("+" + name);
            }

            public void Dispose() => _log.Add("-" + _name);
        }

        private sealed class View : IPooledView
        {
            private readonly List<string> _log;

            public View(List<string> log) => _log = log;

            public IEntity Entity { get; } = new Entity(new BodyPose());
            public bool FailOnShow { get; set; }

            public void Show(Vector2 position, float angle)
            {
                if (FailOnShow) throw new InvalidOperationException("show failed");
                _log.Add("show");
            }

            public void Hide() => _log.Add("hide");
            public IDisposable RegisterColliders() => new Handle(_log, "colliders");
            public void Dispose() => _log.Add("dispose");
        }

        private readonly List<string> _log = new();
        private View _view;
        private PooledLife _life;
        private int _finished;

        [SetUp]
        public void SetUp()
        {
            _log.Clear();
            _finished = 0;
            _view = new View(_log);
            _life = new PooledLife("thing", _view, new Func<IDisposable>[] { () => new Handle(_log, "motion") });
            _life.Finished += () =>
            {
                _finished++;
                _log.Add("finished");
            };
        }

        [Test]
        public void Enter_RegistersCollidersFirstThenTheRestThenShowsThenRunsAfterShown()
        {
            _life.Enter(Vector2.zero, 0f, () => _log.Add("after"));

            Assert.That(_log, Is.EqualTo(new[] { "+colliders", "+motion", "show", "after" }));
            Assert.That(_life.IsInPlay, Is.True);
        }

        [Test]
        public void Retire_TakesEverythingDownInReverseThenHidesThenFinishes()
        {
            _life.Enter(Vector2.zero, 0f);
            _log.Clear();

            _life.Retire();

            Assert.That(_log, Is.EqualTo(new[] { "-motion", "-colliders", "hide", "finished" }));
            Assert.That(_life.IsInPlay, Is.False);
        }

        [Test]
        public void Retire_NotInPlay_DoesNothing()
        {
            _life.Retire();

            Assert.That(_log, Is.Empty);
            Assert.That(_finished, Is.Zero);
        }

        [Test]
        public void Enter_ShowFails_LeavesPlayWithoutFinishingAndRethrows()
        {
            _view.FailOnShow = true;

            Assert.Throws<InvalidOperationException>(() => _life.Enter(Vector2.zero, 0f));

            Assert.That(_life.IsInPlay, Is.False);
            Assert.That(_finished, Is.Zero);
            Assert.That(_log, Is.EqualTo(new[] { "+colliders", "+motion", "-motion", "-colliders", "hide" }));
        }

        [Test]
        public void Enter_WhileInPlay_ThrowsWithTheName()
        {
            _life.Enter(Vector2.zero, 0f);

            var error = Assert.Throws<InvalidOperationException>(() => _life.Enter(Vector2.zero, 0f));
            Assert.That(error.Message, Does.Contain("thing"));
        }

        [Test]
        public void Dispose_TakesDownAndDestroysWithoutFinishing()
        {
            _life.Enter(Vector2.zero, 0f);
            _log.Clear();

            _life.Dispose();

            Assert.That(_log, Is.EqualTo(new[] { "-motion", "-colliders", "dispose" }));
            Assert.That(_finished, Is.Zero);
        }

        [Test]
        public void Constructor_NullArguments_Throw()
        {
            var none = Array.Empty<Func<IDisposable>>();

            Assert.Throws<ArgumentNullException>(() => new PooledLife(null, _view, none));
            Assert.Throws<ArgumentNullException>(() => new PooledLife("x", null, none));
            Assert.Throws<ArgumentNullException>(() => new PooledLife("x", _view, null));
        }
    }
}
