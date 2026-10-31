using System;
using Asteroids.Core;
using UnityEngine;

namespace Asteroids.Collisions.Response
{
    /// <summary>
    /// Bounces two kinetic bodies off each other like balls. Only entities
    /// registered in <see cref="IKineticBodyRegistry"/> on both sides of a contact bounce.
    /// </summary>
    public sealed class BounceResolver : ICollisionEventSink
    {
        private readonly IKineticBodyLookup _bodies;

        /// <exception cref="ArgumentNullException"><paramref name="bodies"/> is null.</exception>
        public BounceResolver(IKineticBodyLookup bodies) =>
            _bodies = bodies ?? throw new ArgumentNullException(nameof(bodies));

        public void Entered(ICollider self, ICollider other, in Contact contact) => Resolve(self, other, contact);

        public void Stayed(ICollider self, ICollider other, in Contact contact) => Resolve(self, other, contact);

        public void Exited(ICollider self, ICollider other) { }

        private void Resolve(ICollider self, ICollider other, in Contact contact)
        {
            if (self.Owner is not IEntity first || other.Owner is not IEntity second) return;
            if (ReferenceEquals(first, second)) return;
            if (!_bodies.TryGet(first, out var firstBody) || !_bodies.TryGet(second, out var secondBody)) return;
            
            var normal = contact.Normal;
            var closing = Vector2.Dot(firstBody.Velocity - secondBody.Velocity, normal);
            if (closing >= 0f) return;

            var halfDepth = normal * (contact.Depth * 0.5f);
            first.Pose.Position += halfDepth;
            second.Pose.Position -= halfDepth;
            
            KnockAway(firstBody, normal);
            KnockAway(secondBody, -normal);
        }

        /// <param name="away">Unit vector pointing from what the body hit towards the body.</param>
        private static void KnockAway(IKineticBody body, Vector2 away)
        {
            var along = Vector2.Dot(body.Velocity, away);
            
            var after = along < 0f ? -along : along;
            var target = Mathf.Max(after, body.MinKnockbackSpeed);

            if (!Mathf.Approximately(target, along)) body.AddVelocity(away * (target - along));
        }
    }
}
