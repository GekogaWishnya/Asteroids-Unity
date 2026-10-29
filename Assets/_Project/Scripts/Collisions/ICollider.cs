using Asteroids.Core.IConfigs;

namespace Asteroids.Collisions
{
    public interface ICollider
    {
        object Owner { get; }
        
        /// <summary>Layers this collider is a part of.</summary>
        CollisionLayer Layer { get; }
        
        /// <summary>
        /// Layers this collider reacts to. A pair interacts only if both masks accept the other's layer.
        /// May change while the collider is registered, the system reads it on every detection pass.
        /// </summary>
        CollisionLayer Mask { get; }
        
        /// <summary>Never null. The same instance for the whole life of the collider.</summary>
        IShape Shape { get; }
    }
}