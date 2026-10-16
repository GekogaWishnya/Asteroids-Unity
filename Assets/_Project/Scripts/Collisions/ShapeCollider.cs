using System;

namespace Asteroids.Collisions
{
    /// <summary>Collider data.</summary>
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
        public CollisionLayer Mask { get; }
        public IShape Shape { get; }
    }
}
