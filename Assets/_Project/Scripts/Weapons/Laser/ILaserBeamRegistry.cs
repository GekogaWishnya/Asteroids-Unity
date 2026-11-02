using System;
using Asteroids.Core;

namespace Asteroids.Weapons.Laser
{
    public interface ILaserBeamRegistry
    {
        /// <exception cref="ArgumentNullException">An argument is null.</exception>
        /// <exception cref="InvalidOperationException">The entity is already registered.</exception>
        IDisposable Register(IEntity beam, ILaserBeamContacts contacts);
    }
}
