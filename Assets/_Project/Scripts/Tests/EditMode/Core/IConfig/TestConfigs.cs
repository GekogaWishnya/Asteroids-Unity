using System.Collections.Generic;

namespace Asteroids.Core.IConfigs.Tests
{
    internal sealed class TestWaveEntry : IWaveEntryConfig
    {
        public EnemyTypeId Type { get; set; } = new("AsteroidBig");
        public int Count { get; set; } = 1;
        public float StartDelay { get; set; }
        public float Interval { get; set; }
    }

    internal sealed class TestWave : IWaveConfig
    {
        public float DelayBeforeStart { get; set; }

        public IReadOnlyList<IWaveEntryConfig> Entries { get; set; } =
            new List<IWaveEntryConfig> { new TestWaveEntry() };
    }

    internal sealed class TestLevel : ILevelConfig
    {
        public IReadOnlyList<IWaveConfig> Waves { get; set; } = new List<IWaveConfig> { new TestWave() };
    }

    internal sealed class TestAsteroidType : IAsteroidTypeConfig
    {
        public TestAsteroidType(string id) => Type = new EnemyTypeId(id);

        public EnemyTypeId Type { get; set; }
        public int MaxHealth { get; set; } = 1;
        public int ContactDamage { get; set; } = 1;
        public int ScorePoints { get; set; } = 20;
        public float MinSpeed { get; set; } = 1f;
        public float MaxSpeed { get; set; } = 2f;
        public float MinSpinSpeed { get; set; } = 10f;
        public float MaxSpinSpeed { get; set; } = 30f;
        public EnemyTypeId ChildType { get; set; }
        public int ChildCount { get; set; }
        public int PoolPrewarm { get; set; } = 2;
        public int PoolMaxIdle { get; set; } = 8;

        public TestAsteroidType SplitsInto(string childId, int count)
        {
            ChildType = new EnemyTypeId(childId);
            ChildCount = count;
            return this;
        }
    }

    internal sealed class TestAsteroidConfig : IAsteroidConfig
    {
        public TestAsteroidConfig(params IAsteroidTypeConfig[] types) =>
            Types = new List<IAsteroidTypeConfig>(types);

        public IReadOnlyList<IAsteroidTypeConfig> Types { get; set; }
    }

    internal sealed class TestUfoType : IUfoTypeConfig
    {
        public TestUfoType(string id = "Ufo") => Type = new EnemyTypeId(id);

        public EnemyTypeId Type { get; set; }
        public int MaxHealth { get; set; } = 1;
        public int ContactDamage { get; set; } = 1;
        public int ScorePoints { get; set; } = 150;
        public float LaunchSpeed { get; set; } = 2f;
        public float Thrust { get; set; } = 6f;
        public float TurnSpeed { get; set; } = 120f;
        public float MaxSpeed { get; set; } = 5f;
        public float LinearDamping { get; set; } = 0.5f;
        public float MinKnockbackSpeed { get; set; } = 3f;
        public float StunSeconds { get; set; } = 1.5f;
        public int PoolPrewarm { get; set; } = 4;
        public int PoolMaxIdle { get; set; } = 8;
    }

    internal sealed class TestUfoConfig : IUfoConfig
    {
        public TestUfoConfig(params IUfoTypeConfig[] types) => Types = new List<IUfoTypeConfig>(types);

        public IReadOnlyList<IUfoTypeConfig> Types { get; set; }
    }
}
