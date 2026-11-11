using UnityEngine;

namespace Asteroids.Physics
{
    public interface IPoseView
    {
        /// <param name="position">World position.</param>
        /// <param name="angle">Degrees, as <see cref="Asteroids.Core.IPose.Angle"/>.</param>
        void Show(Vector2 position, float angle);
    }
}