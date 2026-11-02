using Asteroids.Core;

namespace Asteroids.Weapons.Laser
{
    public interface ILaserBeamLookup
    {
        bool TryGet(IEntity beam, out ILaserBeamContacts contacts);
    }
}
