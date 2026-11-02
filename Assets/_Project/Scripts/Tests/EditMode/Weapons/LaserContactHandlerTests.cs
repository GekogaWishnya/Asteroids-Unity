using System;
using Asteroids.Collisions;
using Asteroids.Weapons.Laser;
using NUnit.Framework;

namespace Asteroids.Weapons.Tests
{
    [TestFixture]
    public sealed class LaserContactHandlerTests
    {
        private LaserBeamRegistry _registry;
        private LaserContactHandler _handler;
        private TestEntity _beam;
        private RecordingBeamContacts _contacts;

        [SetUp]
        public void SetUp()
        {
            _registry = new LaserBeamRegistry();
            _handler = new LaserContactHandler(_registry);
            _beam = new TestEntity();
            _contacts = new RecordingBeamContacts();
            _registry.Register(_beam, _contacts);
        }

        private static CollisionEntered Entered(object self, object other) =>
            new(new FakeCollider(self), new FakeCollider(other), default);

        private static CollisionExited Exited(object self, object other) =>
            new(new FakeCollider(self), new FakeCollider(other));

        [Test]
        public void Entered_FromTheBeamsSide_TellsTheBeam()
        {
            var target = new TestEntity();

            _handler.OnCollisionEntered(Entered(_beam, target));

            Assert.That(_contacts.EnteredTargets, Is.EqualTo(new[] { target }));
        }

        [Test]
        public void Entered_FromTheTargetsSide_IsIgnored()
        {
            _handler.OnCollisionEntered(Entered(new TestEntity(), _beam));

            Assert.That(_contacts.EnteredTargets, Is.Empty);
        }

        [Test]
        public void Exited_FromTheBeamsSide_TellsTheBeam()
        {
            var target = new TestEntity();

            _handler.OnCollisionExited(Exited(_beam, target));

            Assert.That(_contacts.ExitedTargets, Is.EqualTo(new[] { target }));
        }

        [Test]
        public void Contact_WithSomethingThatIsNotAnEntity_IsIgnored()
        {
            _handler.OnCollisionEntered(Entered(_beam, new object()));
            _handler.OnCollisionExited(Exited(_beam, new object()));

            Assert.That(_contacts.EnteredTargets, Is.Empty);
            Assert.That(_contacts.ExitedTargets, Is.Empty);
        }

        [Test]
        public void Contact_OfAnEntityThatIsNotABeam_IsIgnored()
        {
            Assert.DoesNotThrow(() => _handler.OnCollisionEntered(Entered(new TestEntity(), new TestEntity())));
        }

        [Test]
        public void Construct_NullLookup_Throws() =>
            Assert.Throws<ArgumentNullException>(() => new LaserContactHandler(null));
    }

    [TestFixture]
    public sealed class LaserBeamRegistryTests
    {
        [Test]
        public void Register_ThenDispose_RemovesTheBeam()
        {
            var registry = new LaserBeamRegistry();
            var beam = new TestEntity();

            registry.Register(beam, new RecordingBeamContacts()).Dispose();

            Assert.That(registry.TryGet(beam, out _), Is.False);
        }

        [Test]
        public void Register_Twice_Throws()
        {
            var registry = new LaserBeamRegistry();
            var beam = new TestEntity();
            registry.Register(beam, new RecordingBeamContacts());

            Assert.Throws<InvalidOperationException>(() => registry.Register(beam, new RecordingBeamContacts()));
        }

        [Test]
        public void Dispose_StaleHandle_KeepsTheNewRegistration()
        {
            var registry = new LaserBeamRegistry();
            var beam = new TestEntity();
            var stale = registry.Register(beam, new RecordingBeamContacts());
            stale.Dispose();
            var current = new RecordingBeamContacts();
            registry.Register(beam, current);

            stale.Dispose();

            Assert.That(registry.TryGet(beam, out var found), Is.True);
            Assert.That(found, Is.SameAs(current));
        }

        [Test]
        public void TryGet_Null_IsFalse() => Assert.That(new LaserBeamRegistry().TryGet(null, out _), Is.False);

        [Test]
        public void Register_NullArguments_Throw()
        {
            var registry = new LaserBeamRegistry();

            Assert.Throws<ArgumentNullException>(() => registry.Register(null, new RecordingBeamContacts()));
            Assert.Throws<ArgumentNullException>(() => registry.Register(new TestEntity(), null));
        }
    }
}
