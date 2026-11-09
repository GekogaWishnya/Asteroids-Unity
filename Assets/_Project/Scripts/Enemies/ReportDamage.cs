using System;
using Asteroids.Core.IConfigs;
using UnityEngine;

namespace Asteroids.Enemies
{
    /// <summary>Reports each hit an enemy takes to an <see cref="IEnemyEventSink"/>.</summary>
    internal sealed class ReportDamage : IEnemyDamageHandler
    {
        private readonly IEnemyEventSink _events;
        private readonly EnemyTypeId _type;

        /// <exception cref="ArgumentNullException"><paramref name="events"/> is null.</exception>
        /// <exception cref="ArgumentException"><paramref name="type"/> is not valid.</exception>
        public ReportDamage(IEnemyEventSink events, EnemyTypeId type)
        {
            _events = events ?? throw new ArgumentNullException(nameof(events));
            if (!type.IsValid) throw new ArgumentException("The enemy type must be valid.", nameof(type));

            _type = type;
        }

        public void Damaged(Vector2 position, bool isFatal) => _events.Damaged(_type, position, isFatal);
    }
}