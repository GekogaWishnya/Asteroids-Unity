using System;
using System.Collections.Generic;
using UnityEngine;

namespace Asteroids.Weapons.Components
{
    /// <summary>The data of one weapon.</summary>
    public abstract class WeaponDefinition : ScriptableObject, IWeaponDefinition
    {
        public string Name => name;

        public virtual IReadOnlyList<Type> StatusContracts => Array.Empty<Type>();

        /// <summary>Everything wrong with the data. Empty when valid.</summary>
        public abstract IReadOnlyList<string> FindProblems();
    }
}