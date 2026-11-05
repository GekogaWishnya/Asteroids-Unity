using UnityEngine;

namespace Asteroids.Core
{
    /// <summary>Ship's data to shon on HUD.</summary>
    public interface IShipTelemetry
    {
        /// <summary>True while there is a ship to read.</summary>
        bool HasShip { get; }

        /// <summary>World position in units.</summary>
        Vector2 Position { get; }

        /// <summary>Degrees, counter-clockwise, 0 faces +Y, as <see cref="IPose.Angle"/>.</summary>
        float Angle { get; }

        /// <summary>Units per second.</summary>
        float Speed { get; }
    }
}