using System;
using System.Collections.Generic;
using Asteroids.Core.IConfigs;
using UnityEngine;

namespace Asteroids.Enemies
{
    /// <summary>Finds the population of a type, so an asteroid can split into any other type.</summary>
    public sealed class AsteroidLauncher : IAsteroidLauncher
    {
        private readonly Dictionary<EnemyTypeId, AsteroidPopulation> _populations = new();

        /// <exception cref="ArgumentNullException"><paramref name="population"/> is null.</exception>
        /// <exception cref="InvalidOperationException">The type already has a population.</exception>
        public void Register(AsteroidPopulation population)
        {
            if (population == null) throw new ArgumentNullException(nameof(population));
            if (!_populations.TryAdd(population.Type, population))
                throw new InvalidOperationException(
                    $"Asteroid type '{population.Type}' is already registered.");
        }

        /// <exception cref="InvalidOperationException">The type is not registered.</exception>
        public void Launch(EnemyTypeId type, Vector2 position)
        {
            if (!type.IsValid || !_populations.TryGetValue(type, out var population))
                throw new InvalidOperationException($"Asteroid type '{type}' is not registered.");

            population.Launch(position);
        }
    }
}
