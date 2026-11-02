using System;
using System.Collections.Generic;
using Asteroids.Core;

namespace Asteroids.Weapons.Laser
{
    public sealed class LaserBeamRegistry : ILaserBeamRegistry, ILaserBeamLookup
    {
        private readonly Dictionary<IEntity, ILaserBeamContacts> _beams = new();

        public IDisposable Register(IEntity beam, ILaserBeamContacts contacts)
        {
            if (beam == null) throw new ArgumentNullException(nameof(beam));
            if (contacts == null) throw new ArgumentNullException(nameof(contacts));

            if (!_beams.TryAdd(beam, contacts))
                throw new InvalidOperationException("The entity is already registered as a laser beam.");

            return new Registration(_beams, beam, contacts);
        }

        public bool TryGet(IEntity beam, out ILaserBeamContacts contacts)
        {
            if (beam != null) return _beams.TryGetValue(beam, out contacts);
            contacts = null;
            return false;

        }

        private sealed class Registration : IDisposable
        {
            private readonly Dictionary<IEntity, ILaserBeamContacts> _map;
            private readonly IEntity _beam;
            private readonly ILaserBeamContacts _contacts;

            public Registration(Dictionary<IEntity, ILaserBeamContacts> map, IEntity beam, ILaserBeamContacts contacts)
            {
                _map = map;
                _beam = beam;
                _contacts = contacts;
            }

            public void Dispose()
            {
                if (_map.TryGetValue(_beam, out var current) && ReferenceEquals(current, _contacts))
                    _map.Remove(_beam);
            }
        }
    }
}
