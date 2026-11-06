using System;
using System.Collections.Generic;
using Asteroids.Common;
using Asteroids.Core;

namespace Asteroids.Player
{
    /// <summary>
    /// Every ship built in this scene, so a new game can restart them. Ships are added by
    /// <see cref="ShipControllerAssembler"/> and live as long as the scene.
    /// </summary>
    public sealed class ShipRoster : IRestartable
    {
        private readonly List<IShipController> _ships = new();

        public int Count => _ships.Count;

        public RestartOrder Order => RestartOrder.Ship;

        /// <exception cref="ArgumentNullException"><paramref name="ship"/> is null.</exception>
        public void Add(IShipController ship) =>
            _ships.Add(ship ?? throw new ArgumentNullException(nameof(ship)));

        /// <summary>Restarts every ship.</summary>
        public void Restart()
        {
            var failures = new FailureCollector();
            foreach (var ship in _ships)
            {
                try
                {
                    ship.Restart();
                }
                catch (Exception e)
                {
                    failures.Record(e);
                }
            }

            failures.ThrowIfFailed();
        }
    }
}