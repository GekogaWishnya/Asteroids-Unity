using UnityEngine;

namespace Asteroids.Core
{
    /// <summary>Where the player's ship is, for anything that targets it.</summary>
    public interface IPlayerTarget
    {
        bool HasTarget { get; }

        /// <summary>World position of the ship. <see cref="Vector2.zero"/> when <see cref="HasTarget"/> is false.</summary>
        Vector2 Position { get; }

        /// <summary>Units per second. <see cref="Vector2.zero"/> when <see cref="HasTarget"/> is false.</summary>
        Vector2 Velocity { get; }
    }
}
