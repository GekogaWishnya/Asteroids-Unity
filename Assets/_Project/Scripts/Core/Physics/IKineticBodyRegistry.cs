using System;

namespace Asteroids.Core
{
    /// <summary>Tells the collision response which entities bounce.</summary>
    public interface IKineticBodyRegistry
    {
        /// <exception cref="ArgumentNullException">An argument is null.</exception>
        /// <exception cref="InvalidOperationException">The entity is already registered.</exception>
        IDisposable Register(IEntity entity, IKineticBody body);
    }
}
