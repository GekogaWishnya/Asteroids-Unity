using System;

namespace Asteroids.Core
{
    /// <summary>
    /// A collider whose lifetime is decided by the owner.
    /// Used by pooled objects, whose lifecycle registers everything they need while in play.
    /// </summary>
    public interface IManagedCollider
    {
        /// <summary>
        /// Stops the collider registering itself when its object is enabled or disabled. From then on it is in
        /// collision detection only while a handle from <see cref="Register"/> is held.
        /// </summary>
        void TakeOverRegistration();

        /// <summary>Registers the collider until the handle is disposed.</summary>
        /// <exception cref="InvalidOperationException">
        /// <see cref="TakeOverRegistration"/> wasn't called, or the collider isn't injected yet.
        /// </exception>
        IDisposable Register();
    }
}