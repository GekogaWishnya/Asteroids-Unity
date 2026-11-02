using System;
using Asteroids.Core;
using UnityEngine;
using Zenject;

namespace Asteroids.Weapons.Components
{
    public abstract class WeaponDefinition : ScriptableObject
    {
        /// <summary>
        /// Builds a new, independent weapon. Every call returns a weapon of its own, with its own pool.
        /// </summary>
        /// <param name="container">Supplies the shared services (simulation loop, damage registry, ...).</param>
        /// <exception cref="System.ArgumentNullException"><paramref name="container"/> is null.</exception>
        /// <exception cref="System.InvalidOperationException">The definition is invalid.</exception>
        public abstract IWeapon Create(DiContainer container);

        /// <summary>
        /// Binds what others (the HUD) may read about the weapon this definition builds for a loadout, such as
        /// <see cref="ILaserStatus"/>.
        /// </summary>
        /// <param name="container">The container to bind in.</param>
        /// <param name="weapon">Returns the built weapon of the slot. Call it lazily, from the binding.</param>
        /// <exception cref="ArgumentNullException">An argument is null.</exception>
        public virtual void BindStatus(DiContainer container, Func<DiContainer, IWeapon> weapon)
        {
            if (container == null) throw new ArgumentNullException(nameof(container));
            if (weapon == null) throw new ArgumentNullException(nameof(weapon));
        }
    }
}
