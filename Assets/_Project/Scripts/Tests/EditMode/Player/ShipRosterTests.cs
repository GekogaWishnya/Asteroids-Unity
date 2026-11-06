using System;
using System.Collections.Generic;
using Asteroids.Core;
using NUnit.Framework;

namespace Asteroids.Player.Tests
{
    [TestFixture]
    public sealed class ShipRosterTests
    {
        private sealed class FakeShip : IShipController
        {
            public int Restarts { get; private set; }
            public bool FailOnRestart { get; set; }

            public void Enable() { }
            public void Disable() { }

            public void Restart()
            {
                Restarts++;
                if (FailOnRestart) throw new InvalidOperationException("restart failed");
            }
        }

        [Test]
        public void Order_IsTheShips() => Assert.That(new ShipRoster().Order, Is.EqualTo(RestartOrder.Ship));

        [Test]
        public void Restart_RestartsEveryShip()
        {
            var roster = new ShipRoster();
            var ships = new List<FakeShip> { new(), new() };
            foreach (var ship in ships) roster.Add(ship);

            roster.Restart();

            Assert.That(roster.Count, Is.EqualTo(2));
            Assert.That(ships.TrueForAll(ship => ship.Restarts == 1), Is.True);
        }

        [Test]
        public void Restart_AShipFails_RestartsTheOthersAndRethrows()
        {
            var roster = new ShipRoster();
            var failing = new FakeShip { FailOnRestart = true };
            var other = new FakeShip();
            roster.Add(failing);
            roster.Add(other);

            Assert.Throws<InvalidOperationException>(() => roster.Restart());

            Assert.That(other.Restarts, Is.EqualTo(1));
        }

        [Test]
        public void Add_Null_Throws() =>
            Assert.Throws<ArgumentNullException>(() => new ShipRoster().Add(null));
    }
}
