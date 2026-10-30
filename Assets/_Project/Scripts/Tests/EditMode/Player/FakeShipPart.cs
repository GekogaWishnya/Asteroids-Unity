using System;
using System.Collections.Generic;
using Asteroids.Core;

namespace Asteroids.Player.Tests
{
    /// <summary>Writes "+name" on attach and "-name" on detach into a shared log.</summary>
    internal sealed class FakeShipPart : IShipPart
    {
        private readonly string _name;
        private readonly List<string> _log;

        public FakeShipPart(string name, List<string> log)
        {
            _name = name;
            _log = log;
        }

        public List<IEntity> Ships { get; } = new();
        public int LiveCount { get; private set; }
        public bool FailOnAttach { get; set; }

        public IDisposable Attach(IEntity ship)
        {
            if (FailOnAttach) throw new InvalidOperationException("attach failed");

            Ships.Add(ship);
            LiveCount++;
            _log.Add("+" + _name);
            return new Detach(this);
        }

        private sealed class Detach : IDisposable
        {
            private FakeShipPart _owner;

            public Detach(FakeShipPart owner) => _owner = owner;

            public void Dispose()
            {
                if (_owner == null) return;

                _owner.LiveCount--;
                _owner._log.Add("-" + _owner._name);
                _owner = null;
            }
        }
    }
}
