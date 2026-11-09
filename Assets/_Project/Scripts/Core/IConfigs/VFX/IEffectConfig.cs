namespace Asteroids.Core.IConfigs
{
    public interface IEffectConfig
    {
        /// <summary>What the effect is called in error messages.</summary>
        string Name { get; }

        float Scale { get; }
        int PoolPrewarm { get; }
        int PoolMaxIdle { get; }
        int MaxActive { get; }
    }
}