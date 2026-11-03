using System;
using NUnit.Framework;

namespace Asteroids.Collisions.Tests
{
    [TestFixture]
    public sealed class CollisionResponsesTests
    {
        private sealed class Response : ICollisionResponse
        {
            public Response(CollisionResponseOrder order) => Order = order;

            public CollisionResponseOrder Order { get; }
            public void Entered(ICollider first, ICollider second, in Contact contact) { }
            public void Stayed(ICollider first, ICollider second, in Contact contact) { }
            public void Exited(ICollider first, ICollider second) { }
        }

        [Test]
        public void InOrder_SortsByOrder()
        {
            var laser = new Response(CollisionResponseOrder.LaserContact);
            var bounce = new Response(CollisionResponseOrder.Bounce);
            var impact = new Response(CollisionResponseOrder.ProjectileImpact);
            var damage = new Response(CollisionResponseOrder.Damage);

            var ordered = CollisionResponses.InOrder(new ICollisionResponse[] { laser, impact, bounce, damage });

            Assert.That(ordered, Is.EqualTo(new ICollisionResponse[] { bounce, damage, impact, laser }));
        }

        [Test]
        public void InOrder_Empty_ReturnsEmpty() =>
            Assert.That(CollisionResponses.InOrder(Array.Empty<ICollisionResponse>()), Is.Empty);

        [Test]
        public void InOrder_TwoResponsesShareAnOrder_Throws()
        {
            var responses = new ICollisionResponse[]
            {
                new Response(CollisionResponseOrder.Damage), new Response(CollisionResponseOrder.Damage)
            };

            Assert.Throws<ArgumentException>(() => CollisionResponses.InOrder(responses));
        }

        [Test]
        public void InOrder_Null_Throws()
        {
            Assert.Throws<ArgumentNullException>(() => CollisionResponses.InOrder(null));
            Assert.Throws<ArgumentException>(() => CollisionResponses.InOrder(new ICollisionResponse[] { null }));
        }

        [Test]
        public void Order_PhysicsThenDamageThenImpact()
        {
            // Gameplay must see a contact whose bounce already happened, and a projectile must deal its damage
            // before its impact retires it. Changing this order changes gameplay.
            Assert.That(CollisionResponseOrder.Bounce, Is.LessThan(CollisionResponseOrder.Damage));
            Assert.That(CollisionResponseOrder.Damage, Is.LessThan(CollisionResponseOrder.ProjectileImpact));
        }
    }
}
