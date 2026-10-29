using System;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;

namespace Asteroids.Enemies.Tests
{
    [TestFixture]
    public sealed class CompositeDeathHandlerTests
    {
        private sealed class NamedHandler : IEnemyDeathHandler
        {
            private readonly string _name;
            private readonly List<string> _log;
            private readonly bool _throws;

            public NamedHandler(string name, List<string> log, bool throws = false)
            {
                _name = name;
                _log = log;
                _throws = throws;
            }

            public void Died(Vector2 position)
            {
                _log.Add(_name);
                if (_throws) throw new InvalidOperationException("boom");
            }
        }

        [Test]
        public void Constructor_NullHandlers_Throws()
        {
            Assert.Throws<ArgumentNullException>(() => new CompositeDeathHandler(null));
        }

        [Test]
        public void Constructor_NullHandler_Throws()
        {
            Assert.Throws<ArgumentException>(() => new CompositeDeathHandler(new IEnemyDeathHandler[] { null }));
        }

        [Test]
        public void Died_RunsTheHandlersInOrder()
        {
            var log = new List<string>();
            var composite = new CompositeDeathHandler(new IEnemyDeathHandler[]
            {
                new NamedHandler("first", log), new NamedHandler("second", log), new NamedHandler("third", log)
            });

            composite.Died(Vector2.zero);

            Assert.That(log, Is.EqualTo(new[] { "first", "second", "third" }));
        }

        [Test]
        public void Died_PassesThePositionToEveryHandler()
        {
            var first = new RecordingDeathHandler();
            var second = new RecordingDeathHandler();
            var composite = new CompositeDeathHandler(new IEnemyDeathHandler[] { first, second });

            composite.Died(new Vector2(3f, 4f));

            Assert.That(first.Deaths, Is.EqualTo(new[] { new Vector2(3f, 4f) }));
            Assert.That(second.Deaths, Is.EqualTo(new[] { new Vector2(3f, 4f) }));
        }

        [Test]
        public void Died_AHandlerThrows_StopsThereAndRethrows()
        {
            var log = new List<string>();
            var composite = new CompositeDeathHandler(new IEnemyDeathHandler[]
            {
                new NamedHandler("first", log), new NamedHandler("second", log, throws: true),
                new NamedHandler("third", log)
            });

            Assert.Throws<InvalidOperationException>(() => composite.Died(Vector2.zero));

            Assert.That(log, Is.EqualTo(new[] { "first", "second" }));
        }

        [Test]
        public void Died_NoHandlers_DoesNothing()
        {
            var composite = new CompositeDeathHandler(Array.Empty<IEnemyDeathHandler>());

            Assert.DoesNotThrow(() => composite.Died(Vector2.zero));
        }
    }
}
