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

        private void Entered(object first, object second) =>
            _handler.Entered(new FakeCollider(first), new FakeCollider(second), default);

        private void Exited(object first, object second) =>
            _handler.Exited(new FakeCollider(first), new FakeCollider(second));

        [Test]
        public void Entered_FromTheBeamsSide_TellsTheBeam()
        {
            var target = new TestEntity();

            Entered(_beam, target);

            Assert.That(_contacts.EnteredTargets, Is.EqualTo(new[] { target }));
        }

        [Test]
        public void Entered_BeamIsTheSecondCollider_TellsTheBeam()
        {
            var target = new TestEntity();

            Entered(target, _beam);

            Assert.That(_contacts.EnteredTargets, Is.EqualTo(new[] { target }));
        }

        [Test]
        public void Exited_BeamIsTheSecondCollider_TellsTheBeam()
        {
            var target = new TestEntity();

            Exited(target, _beam);

            Assert.That(_contacts.ExitedTargets, Is.EqualTo(new[] { target }));
        }

        [Test]
        public void Entered_TwoBeams_TellsEachAboutTheOther()
        {
            var otherBeam = new TestEntity();
            var otherContacts = new RecordingBeamContacts();
            _registry.Register(otherBeam, otherContacts);

            Entered(_beam, otherBeam);

            Assert.That(_contacts.EnteredTargets, Is.EqualTo(new[] { otherBeam }));
            Assert.That(otherContacts.EnteredTargets, Is.EqualTo(new[] { _beam }));
        }

        [Test]
        public void Order_IsLaserContact() =>
            Assert.That(_handler.Order, Is.EqualTo(CollisionResponseOrder.LaserContact));

        [Test]
        public void Exited_FromTheBeamsSide_TellsTheBeam()
        {
            var target = new TestEntity();

            Exited(_beam, target);

            Assert.That(_contacts.ExitedTargets, Is.EqualTo(new[] { target }));
        }

        [Test]
        public void Contact_WithSomethingThatIsNotAnEntity_IsIgnored()
        {
            Entered(_beam, new object());
            Exited(_beam, new object());

            Assert.That(_contacts.EnteredTargets, Is.Empty);
            Assert.That(_contacts.ExitedTargets, Is.Empty);
        }

        [Test]
        public void Contact_OfAnEntityThatIsNotABeam_IsIgnored()
        {
            Assert.DoesNotThrow(() => Entered(new TestEntity(), new TestEntity()));
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
