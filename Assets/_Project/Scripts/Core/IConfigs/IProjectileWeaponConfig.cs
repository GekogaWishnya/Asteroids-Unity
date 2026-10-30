namespace Asteroids.Core.IConfigs
{
    public interface IProjectileWeaponConfig
    {
        /// <summary>Seconds between two shots while the trigger is held.</summary>
        float FireInterval { get; }

        /// <summary>
        /// Distance in world units from the carrier's position, along its facing, where a projectile appears.
        /// Not negative.
        /// </summary>
        float MuzzleOffset { get; }

        /// <summary>Units per second.</summary>
        float ProjectileSpeed { get; }

        /// <summary>Seconds a projectile flies before it expires.</summary>
        float ProjectileLifetime { get; }

        /// <summary>Damage dealt to whatever a projectile hits and may hurt. At least 1.</summary>
        int ProjectileDamage { get; }

        /// <summary>Projectiles created ahead of time.</summary>
        int PoolPrewarm { get; }

        /// <summary>Most released projectiles the pool keeps. The pool still grows beyond it on demand.</summary>
        int PoolMaxIdle { get; }
    }
}
