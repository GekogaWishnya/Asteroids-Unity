using System;
using Asteroids.Core;
using Asteroids.Core.IConfigs;
using UnityEngine;
using Zenject;

namespace Asteroids.Enemies
{
    internal sealed class SignalBusEnemyEventSink : IEnemyEventSink
    {
        private readonly SignalBus _signalBus;

        /// <exception cref="ArgumentNullException"><paramref name="signalBus"/> is null.</exception>
        public SignalBusEnemyEventSink(SignalBus signalBus) =>
            _signalBus = signalBus ?? throw new ArgumentNullException(nameof(signalBus));

        public void Damaged(EnemyTypeId type, Vector2 position, bool isFatal) =>
            _signalBus.Fire(new EnemyDamaged(type, position, isFatal));

        public void Died(EnemyTypeId type, Vector2 position) => _signalBus.Fire(new EnemyDied(type, position));
    }
}