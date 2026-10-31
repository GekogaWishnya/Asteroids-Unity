using System;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;

namespace Asteroids.Collisions.Tests
{
    [TestFixture]
    public sealed class CompositeCollisionEventSinkTests
    {
        private sealed class LoggingSink : ICollisionEventSink
        {
            private readonly string _name;
            private readonly List<string> _log;

            public LoggingSink(string name, List<string> log)
            {
                _name = name;
                _log = log;
            }

            public void Entered(ICollider self, ICollider other, in Contact contact) => _log.Add($"{_name} entered");
            public void Stayed(ICollider self, ICollider other, in Contact contact) => _log.Add($"{_name} stayed");
            public void Exited(ICollider self, ICollider other) => _log.Add($"{_name} exited");
        }

        [Test]
        public void EveryEvent_GoesToEverySinkInOrder()
        {
            var log = new List<string>();
            var sink = new CompositeCollisionEventSink(new LoggingSink("a", log), new LoggingSink("b", log));
            var first = new FakeCollider("first");
            var second = new FakeCollider("second");
            var contact = new Contact(Vector2.up, 1f);

            sink.Entered(first, second, contact);
            sink.Stayed(first, second, contact);
            sink.Exited(first, second);

            Assert.That(log, Is.EqualTo(new[]
            {
                "a entered", "b entered", "a stayed", "b stayed", "a exited", "b exited"
            }));
        }

        [Test]
        public void Construct_NullOrNullSink_Throws()
        {
            Assert.Throws<ArgumentNullException>(() => new CompositeCollisionEventSink(null));
            Assert.Throws<ArgumentException>(() => new CompositeCollisionEventSink(new RecordingSink(), null));
        }
    }
}
