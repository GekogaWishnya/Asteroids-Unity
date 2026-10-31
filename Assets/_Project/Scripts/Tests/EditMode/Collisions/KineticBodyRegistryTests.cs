using System;
using Asteroids.Collisions.Response;
using NUnit.Framework;
using UnityEngine;

namespace Asteroids.Collisions.Tests
{
    [TestFixture]
    public sealed class KineticBodyRegistryTests
    {
        private KineticBodyRegistry _registry;
        private TestEntity _entity;
        private FakeKineticBody _body;

        [SetUp]
        public void SetUp()
        {
            _registry = new KineticBodyRegistry();
            _entity = new TestEntity(Vector2.zero);
            _body = new FakeKineticBody(Vector2.zero);
        }

        [Test]
        public void TryGet_Registered_ReturnsTheBody()
        {
            _registry.Register(_entity, _body);

            Assert.That(_registry.TryGet(_entity, out var found), Is.True);
            Assert.That(found, Is.SameAs(_body));
        }

        [Test]
        public void TryGet_NotRegisteredOrNull_ReturnsFalse()
        {
            Assert.That(_registry.TryGet(_entity, out _), Is.False);
            Assert.That(_registry.TryGet(null, out _), Is.False);
        }

        [Test]
        public void Dispose_RemovesTheRegistration()
        {
            _registry.Register(_entity, _body).Dispose();

            Assert.That(_registry.TryGet(_entity, out _), Is.False);
        }

        [Test]
        public void Dispose_OfAnOldRegistration_KeepsTheNewOne()
        {
            var old = _registry.Register(_entity, _body);
            old.Dispose();
            var newer = new FakeKineticBody(Vector2.one);
            _registry.Register(_entity, newer);

            old.Dispose();

            Assert.That(_registry.TryGet(_entity, out var found), Is.True);
            Assert.That(found, Is.SameAs(newer));
        }

        [Test]
        public void Register_Twice_Throws()
        {
            _registry.Register(_entity, _body);

            Assert.Throws<InvalidOperationException>(() => _registry.Register(_entity, _body));
        }

        [Test]
        public void Register_NullArguments_Throw()
        {
            Assert.Throws<ArgumentNullException>(() => _registry.Register(null, _body));
            Assert.Throws<ArgumentNullException>(() => _registry.Register(_entity, null));
        }
    }
}
