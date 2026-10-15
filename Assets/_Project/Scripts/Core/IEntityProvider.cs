namespace Asteroids.Core
{
    /// <summary>
    /// Implemented by scene adapters that own an <see cref="IEntity"/>.
    /// </summary>
    public interface IEntityProvider
    {
        IEntity Entity { get; }
    }
}
