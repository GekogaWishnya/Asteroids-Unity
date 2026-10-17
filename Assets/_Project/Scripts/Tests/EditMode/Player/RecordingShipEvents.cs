using System;
using System.Collections.Generic;
using Asteroids.Core;

namespace Asteroids.Player.Tests
{
    internal sealed class RecordingShipEvents : IShipEventSink
    {
        private readonly List<IEntity> _destroyed = new();

        public IReadOnlyList<IEntity> DestroyedEntities => _destroyed;
        
        public Action OnDestroyed { get; set; }

        public void Destroyed(IEntity entity)
        {
            _destroyed.Add(entity);
            OnDestroyed?.Invoke();
        }
    }
}
