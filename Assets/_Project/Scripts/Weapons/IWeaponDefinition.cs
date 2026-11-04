using System;
using System.Collections.Generic;

namespace Asteroids.Weapons
{
    public interface IWeaponDefinition
    {
        string Name { get; }

        /// <summary>
        /// The contracts the built weapon implements for others to read, such as the laser's status for the HUD.
        /// </summary>
        IReadOnlyList<Type> StatusContracts { get; }
    }
}