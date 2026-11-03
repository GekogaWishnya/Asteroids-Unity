using System;
using Asteroids.Common;
using Asteroids.Core;

namespace Asteroids.Weapons.Laser
{
    public sealed class LaserBeamRegistry : ILaserBeamRegistry, ILaserBeamLookup
    {
        private readonly RegistrationMap<IEntity, ILaserBeamContacts> _beams = new("laser beam");

        public IDisposable Register(IEntity beam, ILaserBeamContacts contacts) =>
            _beams.Add(beam, contacts, nameof(beam), nameof(contacts));

        public bool TryGet(IEntity beam, out ILaserBeamContacts contacts) => _beams.TryGet(beam, out contacts);
    }
}
