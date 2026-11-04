using System;
using System.Collections.Generic;
using Asteroids.Core;

namespace Asteroids.Weapons
{
    public sealed class WeaponFactories
    {
        private readonly Dictionary<Type, IWeaponFactory> _byDefinitionType = new();

        /// <exception cref="ArgumentNullException"><paramref name="factories"/> is null.</exception>
        /// <exception cref="ArgumentException">
        /// A factory is null or has no definition type, or two factories build the same definition type.
        /// </exception>
        public WeaponFactories(IEnumerable<IWeaponFactory> factories)
        {
            if (factories == null) throw new ArgumentNullException(nameof(factories));

            foreach (var factory in factories)
            {
                if (factory == null) throw new ArgumentException("A factory must not be null.", nameof(factories));

                var type = factory.DefinitionType ??
                           throw new ArgumentException(
                               $"{factory.GetType().Name} has no definition type.", nameof(factories));

                if (!_byDefinitionType.TryAdd(type, factory))
                    throw new ArgumentException(
                        $"{_byDefinitionType[type].GetType().Name} and {factory.GetType().Name} both build " +
                        $"{type.Name}.", nameof(factories));
            }
        }

        /// <summary>Builds a new, independent weapon from <paramref name="definition"/>.</summary>
        /// <exception cref="ArgumentNullException"><paramref name="definition"/> is null.</exception>
        /// <exception cref="InvalidOperationException">
        /// No factory builds the definition's type, or the definition is invalid.
        /// </exception>
        public IWeapon Create(IWeaponDefinition definition)
        {
            if (definition == null) throw new ArgumentNullException(nameof(definition));

            var type = definition.GetType();
            if (!_byDefinitionType.TryGetValue(type, out var factory))
                throw new InvalidOperationException(
                    $"No weapon factory builds {type.Name} (weapon '{definition.Name}'). " +
                    $"Bind an {nameof(IWeaponFactory)} for it.");

            return factory.Create(definition);
        }
    }
}