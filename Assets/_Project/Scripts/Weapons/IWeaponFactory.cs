using System;
using Asteroids.Core;

namespace Asteroids.Weapons
{
    public interface IWeaponFactory
    {
        /// <summary>The exact definition type this factory builds from.</summary>
        Type DefinitionType { get; }

        /// <summary>Builds a new, independent weapon, with its own pool.</summary>
        /// <exception cref="ArgumentNullException"><paramref name="definition"/> is null.</exception>
        /// <exception cref="ArgumentException"><paramref name="definition"/> is not of <see cref="DefinitionType"/>.</exception>
        /// <exception cref="InvalidOperationException">The definition is invalid.</exception>
        IWeapon Create(IWeaponDefinition definition);
    }
}