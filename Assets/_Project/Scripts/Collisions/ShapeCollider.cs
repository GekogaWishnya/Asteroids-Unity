using System;
using Asteroids.Core.IConfigs;

namespace Asteroids.Collisions
{
    /// <summary>Collider data. The mask can change at runtime, while the layer cannot.</summary>
    public sealed class ShapeCollider : ICollider
    {
        public ShapeCollider(object owner, CollisionLayer layer, CollisionLayer mask, IShape shape)
        {
            Owner = owner ?? throw new ArgumentNullException(nameof(owner));
            Layer = layer;
            Mask = mask;
            Shape = shape ?? throw new ArgumentNullException(nameof(shape));
        }

        public object Owner { get; }
        public CollisionLayer Layer { get; }
        public CollisionLayer Mask { get; set; }
        public IShape Shape { get; }
    }
}
