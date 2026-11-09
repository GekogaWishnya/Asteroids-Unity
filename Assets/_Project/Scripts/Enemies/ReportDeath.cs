using System;
using Asteroids.Core.IConfigs;
using UnityEngine;

namespace Asteroids.Enemies
{
    /// <summary>Reports the death of an enemy to an <see cref="IEnemyEventSink"/>.</summary>
    internal sealed class ReportDeath : IEnemyDeathHandler
    {
        private readonly IEnemyEventSink _events;
        private readonly EnemyTypeId _type;

        /// <exception cref="ArgumentNullException"><paramref name="events"/> is null.</exception>
        /// <exception cref="ArgumentException"><paramref name="type"/> is not valid.</exception>
        public ReportDeath(IEnemyEventSink events, EnemyTypeId type)
        {
            _events = events ?? throw new ArgumentNullException(nameof(events));
            if (!type.IsValid) throw new ArgumentException("The enemy type must be valid.", nameof(type));

            _type = type;
        }

        public void Died(Vector2 position) => _events.Died(_type, position);
    }
}