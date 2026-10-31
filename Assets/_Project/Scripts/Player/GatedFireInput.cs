using System;
using Asteroids.Core;

namespace Asteroids.Player
{
    /// <summary>Fire input that takes into account ship's lock state.</summary>
    public sealed class GatedFireInput : IFireInput
    {
        private readonly IFireInput _inner;
        private readonly IShipControlLock _lock;

        /// <exception cref="ArgumentNullException">Any argument is null.</exception>
        public GatedFireInput(IFireInput inner, IShipControlLock controlLock)
        {
            _inner = inner ?? throw new ArgumentNullException(nameof(inner));
            _lock = controlLock ?? throw new ArgumentNullException(nameof(controlLock));
        }

        public bool IsFiring => !_lock.IsLocked && _inner.IsFiring;
    }
}
