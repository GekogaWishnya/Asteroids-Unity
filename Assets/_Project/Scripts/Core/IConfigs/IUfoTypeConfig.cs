namespace Asteroids.Core.IConfigs
{
    public interface IUfoTypeConfig : IMotionConfig, IKnockbackConfig
    {
        EnemyTypeId Type { get; }

        int MaxHealth { get; }
        int ContactDamage { get; }
        int ScorePoints { get; }

        /// <summary>Speed it enters the play area with, in units per second. At most <see cref="IMotionConfig.MaxSpeed"/>.</summary>
        float LaunchSpeed { get; }

        /// <summary>Units per second squared.</summary>
        float Thrust { get; }

        /// <summary>Fastest it can turn toward the ship in degrees per second.</summary>
        float TurnSpeed { get; }

        /// <summary>
        /// Seconds it stops steering and thrusting after it is knocked back by a collision.
        /// </summary>
        float StunSeconds { get; }
        
        int PoolPrewarm { get; }
        int PoolMaxIdle { get; }
    }
}
