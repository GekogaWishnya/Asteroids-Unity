using System;
using System.Collections.Generic;
using Asteroids.Core;

namespace Asteroids.Player.Tests
{
    internal sealed class RecordingShipEvents : IShipEventSink
    {
        private readonly List<IEntity> _destroyed = new();
        private readonly List<(IEntity Ship, bool IsFatal)> _damaged = new();

        public IReadOnlyList<(IEntity Ship, bool IsFatal)> Damaged => _damaged;

        public IReadOnlyList<IEntity> DestroyedEntities => _destroyed;
        
        public Action OnDestroyed { get; set; }

        /// <summary>Runs on every damage report, after it was recorded.</summary>
        public Action OnDamaged { get; set; }

        void IShipEventSink.Damaged(IEntity ship, bool isFatal)
        {
            _damaged.Add((ship, isFatal));
            OnDamaged?.Invoke();
        }

        public void Destroyed(IEntity entity)
        {
            _destroyed.Add(entity);
            OnDestroyed?.Invoke();
        }
    }
}
