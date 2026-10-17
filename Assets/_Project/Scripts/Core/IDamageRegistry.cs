using System;

namespace Asteroids.Core
{
    /// <summary>
    /// Tells the damage system which entities can be hurt and which entities hurt others.
    /// An entity can be registered once as a damageable and once as a source.
    /// Dispose the returned handle to remove the registration.
    /// </summary>
    public interface IDamageRegistry
    {
        /// <exception cref="ArgumentNullException">An argument is null.</exception>
        /// <exception cref="InvalidOperationException">The entity is already registered as a damageable.</exception>
        IDisposable RegisterDamageable(IEntity entity, IDamageable damageable);

        /// <exception cref="ArgumentNullException">An argument is null.</exception>
        /// <exception cref="InvalidOperationException">The entity is already registered as a source.</exception>
        IDisposable RegisterSource(IEntity entity, IDamageSource source);
    }
}