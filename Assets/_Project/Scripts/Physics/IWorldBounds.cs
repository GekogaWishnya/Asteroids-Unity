using UnityEngine;

namespace Asteroids.Physics
{
    public interface IWorldBounds
    {
        Rect Rect { get; }

        /// <summary>
        /// Moves a point inside the <see cref="Rect"/>.
        /// Points already inside <see cref="Rect"/> are returned unchanged, so calling this repeatedly is safe.
        /// </summary>
        Vector2 Wrap(Vector2 position);
    }
}
