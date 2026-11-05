using System;
using Asteroids.Core.IConfigs;
using UnityEngine;

namespace Asteroids.Enemies
{
    /// <summary>
    /// Moves a spawn point on the edge of the play area back outside, so an enemy flies in
    /// instead of popping in on the edge. The enemy still crosses the edge where the point is, so every
    /// guarantee of <see cref="PerimeterSpawnPoints"/> about the path inside the area holds.
    /// </summary>
    internal sealed class OffscreenEntry
    {
        public const float MinEntryCosine = 0.25f;
        private readonly float _depth;

        /// <param name="depth">Distance in world units outside the edges.</param>
        /// <exception cref="ArgumentOutOfRangeException">A distance is negative or not finite.</exception>
        public OffscreenEntry(float depth)
        {
            _depth = Check(depth, nameof(depth));
        }

        /// <exception cref="ArgumentNullException">Any argument is null.</exception>
        /// <exception cref="InvalidOperationException">The config is invalid.</exception>
        public static OffscreenEntry OutsideView(ISpawnEntryConfig config)
        {
            if (config == null) throw new ArgumentNullException(nameof(config));

            ConfigValidation.ThrowIfInvalid("The spawn entry config", SpawnEntryConfigValidator.Validate(config));

            var margin = config.SpawnMargin;
            return new OffscreenEntry(margin);
        }

        /// <summary>
        /// Where an enemy that should cross the edge at <paramref name="point"/> while moving along
        /// <paramref name="direction"/> starts.
        /// </summary>
        public Vector2 StartFor(PerimeterPoint point, Vector2 direction)
        {
            var length = direction.magnitude;
            if (!float.IsFinite(length) || !(length > 0f)) return point.Position - point.Inward * _depth;

            var along = direction / length;
            var into = Vector2.Dot(along, point.Inward);
            if (into >= MinEntryCosine) return point.Position - along * (_depth / into);

            return point.Position - point.Inward * _depth;
        }

        private static float Check(float value, string name) =>
            float.IsFinite(value) && value >= 0f
                ? value
                : throw new ArgumentOutOfRangeException(name, value, "Must be a finite distance of 0 or more.");
    }
}