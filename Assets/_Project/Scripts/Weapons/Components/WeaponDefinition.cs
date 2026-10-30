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
    }
}
