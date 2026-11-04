using System;
using Asteroids.Core;

namespace Asteroids.Weapons
{
    /// <summary>An <see cref="IWeaponFactory"/> typed by its definition.</summary>
    public abstract class WeaponFactory<TDefinition> : IWeaponFactory where TDefinition : class, IWeaponDefinition
    {
        public Type DefinitionType => typeof(TDefinition);

        public IWeapon Create(IWeaponDefinition definition)
        {
            if (definition == null) throw new ArgumentNullException(nameof(definition));
            if (definition is not TDefinition typed)
                throw new ArgumentException(
                    $"Weapon '{definition.Name}' is a {definition.GetType().Name}, not a {typeof(TDefinition).Name}.",
                    nameof(definition));

            return Create(typed);
        }

        /// <inheritdoc cref="IWeaponFactory.Create"/>
        protected abstract IWeapon Create(TDefinition definition);
    }
}