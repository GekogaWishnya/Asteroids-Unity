using System;
using Asteroids.Core;
using UnityEngine;

namespace Asteroids.Physics
{
    /// <summary>A kinetic body that stuns its owner whenever a collision knocks it back.</summary>
    public sealed class StunOnKnockback : IKineticBody
    {
        private readonly IKineticBody _body;
        private readonly IStunnable _owner;
        private readonly float _seconds;

        /// <exception cref="ArgumentNullException"><paramref name="body"/> or <paramref name="owner"/> is null.</exception>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="seconds"/> is negative or not finite.</exception>
        public StunOnKnockback(IKineticBody body, IStunnable owner, float seconds)
        {
            _body = body ?? throw new ArgumentNullException(nameof(body));
            _owner = owner ?? throw new ArgumentNullException(nameof(owner));
            if (!float.IsFinite(seconds) || seconds < 0f)
                throw new ArgumentOutOfRangeException(
                    nameof(seconds), "Duration must be a finite number that is not negative.");

            _seconds = seconds;
        }

        public Vector2 Velocity => _body.Velocity;
        public float MinKnockbackSpeed => _body.MinKnockbackSpeed;

        public void AddVelocity(Vector2 delta)
        {
            _body.AddVelocity(delta);
            _owner.Stun(_seconds);
        }
    }
}
