using System;
using System.Collections.Generic;
using Asteroids.Health;
using NUnit.Framework;
using UnityEngine;

namespace Asteroids.Enemies.Tests
{
    [TestFixture]
    public sealed class EnemyLifeTests
    {
        private sealed class Rig
        {
            public readonly List<string> Log = new();
            public readonly FakeEnemyView View = new();
            public readonly HealthModel Health = new(2);
            public readonly RecordingDeathHandler Death;
            public readonly EnemyLife Life;
            public int FinishedCount;

            public Rig()
            {
                Death = new RecordingDeathHandler(Log);
                Life = new EnemyLife("thing", View, Health, Death, new Func<IDisposable>[]
                {
                    () => new Registration(Log, "a"),
                    () => new Registration(Log, "b")
                });
                Life.Finished += () =>
                {
                    FinishedCount++;
                    Log.Add("finished");
                };
            }
        }

        private sealed class Registration : IDisposable
        {
            private readonly List<string> _log;
            private readonly string _name;

            public Registration(List<string> log, string name)
            {
                _log = log;
                _name = name;
                log.Add("+" + name);
            }

            public void Dispose() => _log.Add("-" + _name);
        }

        [Test]
        public void Constructor_NullArguments_Throw()
        {
            var view = new FakeEnemyView();
            var health = new HealthModel(1);
            var death = new RecordingDeathHandler();
            var none = Array.Empty<Func<IDisposable>>();

            Assert.Throws<ArgumentNullException>(() => new EnemyLife(null, view, health, death, none));
            Assert.Throws<ArgumentNullException>(() => new EnemyLife("x", null, health, death, none));
            Assert.Throws<ArgumentNullException>(() => new EnemyLife("x", view, null, death, none));
            Assert.Throws<ArgumentNullException>(() => new EnemyLife("x", view, health, null, none));
            Assert.Throws<ArgumentNullException>(() => new EnemyLife("x", view, health, death, null));
        }

        [Test]
        public void Enter_RegistersInOrderShowsThenRunsAfterShown()
        {
            var rig = new Rig();
            var shownWhenCalled = false;

            rig.Life.Enter(new Vector2(1f, 2f), 30f, () => shownWhenCalled = rig.View.IsShown);

            Assert.That(rig.Life.IsInPlay, Is.True);
            Assert.That(rig.Log, Is.EqualTo(new[] { "+a", "+b" }));
            Assert.That(shownWhenCalled, Is.True);
            Assert.That(rig.View.Entity.Pose.Position, Is.EqualTo(new Vector2(1f, 2f)));
        }

        [Test]
        public void Enter_WhileInPlay_ThrowsWithTheName()
        {
            var rig = new Rig();
            rig.Life.Enter(Vector2.zero, 0f);

            var error = Assert.Throws<InvalidOperationException>(() => rig.Life.Enter(Vector2.zero, 0f));
            Assert.That(error.Message, Is.EqualTo("The thing is already in play."));
        }

        [Test]
        public void Enter_AfterShownThrows_LeavesPlayAndRethrows()
        {
            var rig = new Rig();

            Assert.Throws<InvalidOperationException>(
                () => rig.Life.Enter(Vector2.zero, 0f, () => throw new InvalidOperationException("boom")));
            Assert.That(rig.Life.IsInPlay, Is.False);
            Assert.That(rig.View.IsShown, Is.False);
            Assert.That(rig.Log, Is.EqualTo(new[] { "+a", "+b", "-b", "-a" }));
            Assert.That(rig.FinishedCount, Is.EqualTo(0));
        }

        [Test]
        public void Enter_RefillsTheHealth()
        {
            var rig = new Rig();
            rig.Life.Enter(Vector2.zero, 0f);
            rig.Health.TakeDamage(2);

            rig.Life.Enter(Vector2.zero, 0f);

            Assert.That(rig.Health.Current.Value, Is.EqualTo(2));
        }

        [Test]
        public void Depleted_ReportsDeathThenLeavesThenFinishes()
        {
            var rig = new Rig();
            rig.Life.Enter(new Vector2(3f, 4f), 0f);

            rig.Health.TakeDamage(2);

            Assert.That(rig.Log, Is.EqualTo(new[] { "+a", "+b", "died", "-b", "-a", "finished" }));
            Assert.That(rig.Death.Deaths, Is.EqualTo(new[] { new Vector2(3f, 4f) }));
            Assert.That(rig.View.IsShown, Is.False);
        }

        [Test]
        public void Depleted_WhileOutOfPlay_DoesNothing()
        {
            var rig = new Rig();
            rig.Life.Enter(Vector2.zero, 0f);
            rig.Health.TakeDamage(2);
            rig.Health.Reset();

            rig.Health.TakeDamage(2);

            Assert.That(rig.Death.Deaths, Has.Count.EqualTo(1));
            Assert.That(rig.FinishedCount, Is.EqualTo(1));
        }

        [Test]
        public void Dispose_RemovesRegistrationsAndDestroysTheViewWithoutFinishing()
        {
            var rig = new Rig();
            rig.Life.Enter(Vector2.zero, 0f);

            rig.Life.Dispose();

            Assert.That(rig.Life.IsInPlay, Is.False);
            Assert.That(rig.View.IsDisposed, Is.True);
            Assert.That(rig.FinishedCount, Is.EqualTo(0));
        }
    }
}
