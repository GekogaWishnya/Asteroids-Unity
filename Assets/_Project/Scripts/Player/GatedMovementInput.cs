using System;
using Asteroids.Core;

namespace Asteroids.Player
{
    /// <summary>Movement input that takes into account ship's lock state.</summary>
    public sealed class GatedMovementInput : IMovementInput
    {
        private readonly IMovementInput _inner;
        private readonly IShipControlLock _lock;

        /// <exception cref="ArgumentNullException">Any argument is null.</exception>
        public GatedMovementInput(IMovementInput inner, IShipControlLock controlLock)
        {
            _inner = inner ?? throw new ArgumentNullException(nameof(inner));
            _lock = controlLock ?? throw new ArgumentNullException(nameof(controlLock));
        }

        public float Turn => _lock.IsLocked ? 0f : _inner.Turn;
        public float Thrust => _lock.IsLocked ? 0f : _inner.Thrust;
    }
}
