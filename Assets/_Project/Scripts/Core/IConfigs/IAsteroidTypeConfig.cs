namespace Asteroids.Core.IConfigs
{
    /// <summary>Everything that distinguishes one asteroid type (for example "big") from another.</summary>
    public interface IAsteroidTypeConfig
    {
        EnemyTypeId Type { get; }

        int MaxHealth { get; }

        /// <summary>Damage dealt to whatever it touches and may hurt.</summary>
        int ContactDamage { get; }

        /// <summary>Points awarded when the asteroid is destroyed.</summary>
        int ScorePoints { get; }

        /// <summary>Range of the speed, in units per second. Chosen randomly on spawn.</summary>
        float MinSpeed { get; }
        float MaxSpeed { get; }

        /// <summary>
        /// Range of the rotation speed, in degrees per second. Chosen randomly on spawn.
        /// Sperate from movement direction.
        /// </summary>
        float MinSpinSpeed { get; }
        float MaxSpinSpeed { get; }

        /// <summary>Type spawned when this asteroid is destroyed. <c>default</c> when it does not split.</summary>
        EnemyTypeId ChildType { get; }

        /// <summary>How many children it splits into. 0 when <see cref="ChildType"/> is not set.</summary>
        int ChildCount { get; }

        /// <summary>Instances created ahead of time.</summary>
        int PoolPrewarm { get; }

        /// <summary>Most released instances the pool keeps. The pool still grows beyond it on demand.</summary>
        int PoolMaxIdle { get; }
    }
}
