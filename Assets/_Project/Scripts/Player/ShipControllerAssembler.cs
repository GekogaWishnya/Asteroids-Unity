using System;
using Asteroids.Core;
using Zenject;

namespace Asteroids.Player
{
    /// <summary>Builds a ship from its parts.</summary>
    public sealed class ShipControllerAssembler : IFactory<IEntity, IProtectionTargets, IShipController>
    {
        private readonly ShipMovement.Factory _movement;
        private readonly ShipDefense.Factory _defense;
        private readonly ShipRam.Factory _ram;
        private readonly ShipArmament.Factory _armament;
        private readonly IPlayerHealth _health;
        private readonly IShipEventSink _events;

        /// <exception cref="ArgumentNullException">Any argument is null.</exception>
        public ShipControllerAssembler(
            ShipMovement.Factory movement,
            ShipDefense.Factory defense,
            ShipRam.Factory ram,
            ShipArmament.Factory armament,
            IPlayerHealth health,
            IShipEventSink events)
        {
            _movement = movement ?? throw new ArgumentNullException(nameof(movement));
            _defense = defense ?? throw new ArgumentNullException(nameof(defense));
            _ram = ram ?? throw new ArgumentNullException(nameof(ram));
            _armament = armament ?? throw new ArgumentNullException(nameof(armament));
            _health = health ?? throw new ArgumentNullException(nameof(health));
            _events = events ?? throw new ArgumentNullException(nameof(events));
        }

        public IShipController Create(IEntity entity, IProtectionTargets targets)
        {
            var parts = new IShipPart[]
            {
                _movement.Create(),
                _defense.Create(targets),
                _ram.Create(),
                _armament.Create()
            };

            return new ShipController(entity, parts, _health, _events);
        }
    }
}
