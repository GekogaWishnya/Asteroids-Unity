using System;
using NUnit.Framework;

namespace Asteroids.Common.Tests
{
    [TestFixture]
    public sealed class RegistrationMapTests
    {
        private sealed class Key { }
        private sealed class Value { }

        private RegistrationMap<Key, Value> _map;

        [SetUp]
        public void SetUp() => _map = new RegistrationMap<Key, Value>("thing");

        [Test]
        public void Constructor_NoRole_Throws()
        {
            Assert.Throws<ArgumentException>(() => new RegistrationMap<Key, Value>(null));
            Assert.Throws<ArgumentException>(() => new RegistrationMap<Key, Value>(""));
        }

        [Test]
        public void Add_ThenTryGet_FindsTheValue()
        {
            var key = new Key();
            var value = new Value();

            _map.Add(key, value);

            Assert.That(_map.TryGet(key, out var found), Is.True);
            Assert.That(found, Is.SameAs(value));
            Assert.That(_map.Count, Is.EqualTo(1));
        }

        [Test]
        public void Add_NullArguments_ThrowWithTheGivenNames()
        {
            var keyFailure = Assert.Throws<ArgumentNullException>(() => _map.Add(null, new Value(), "entity", "body"));
            var valueFailure = Assert.Throws<ArgumentNullException>(() => _map.Add(new Key(), null, "entity", "body"));

            Assert.That(keyFailure.ParamName, Is.EqualTo("entity"));
            Assert.That(valueFailure.ParamName, Is.EqualTo("body"));
        }

        [Test]
        public void Add_SameKeyTwice_ThrowsAndKeepsTheFirst()
        {
            var key = new Key();
            var first = new Value();
            _map.Add(key, first);

            var failure = Assert.Throws<InvalidOperationException>(() => _map.Add(key, new Value()));

            Assert.That(failure.Message, Does.Contain("thing"));
            Assert.That(_map.TryGet(key, out var found), Is.True);
            Assert.That(found, Is.SameAs(first));
        }

        [Test]
        public void Dispose_RemovesTheEntry()
        {
            var key = new Key();
            var handle = _map.Add(key, new Value());

            handle.Dispose();

            Assert.That(_map.TryGet(key, out _), Is.False);
            Assert.That(_map.Count, Is.Zero);
        }

        [Test]
        public void Dispose_OldHandleAfterReRegistration_KeepsTheNewEntry()
        {
            var key = new Key();
            var old = _map.Add(key, new Value());
            old.Dispose();
            var current = new Value();
            _map.Add(key, current);

            old.Dispose();

            Assert.That(_map.TryGet(key, out var found), Is.True);
            Assert.That(found, Is.SameAs(current));
        }

        [Test]
        public void TryGet_NullKey_ReturnsFalse()
        {
            Assert.That(_map.TryGet(null, out var found), Is.False);
            Assert.That(found, Is.Null);
        }
    }
}
