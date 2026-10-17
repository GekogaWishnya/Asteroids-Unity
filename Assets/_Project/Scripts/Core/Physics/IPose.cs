using UnityEngine;

namespace Asteroids.Core
{
    /// <summary>
    /// Holds position and orientation of a simulated entity.
    /// </summary>
    public interface IPose
    {
        Vector2 Position { get; set; }

        /// <summary>Degrees, counter-clockwise, 0 faces +Y (same as the Unity z-euler angle).</summary>
        float Angle { get; set; }

        /// <summary>Unit vector the entity faces. Derived from <see cref="Angle"/>.</summary>
        Vector2 Up { get; }

        /// <summary>Unit vector 90 degrees clockwise from <see cref="Up"/>. Derived from <see cref="Angle"/>.</summary>
        Vector2 Right { get; }
    }
}
