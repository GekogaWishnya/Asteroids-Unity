using Asteroids.Core;

namespace Asteroids.Weapons.Laser
{
    public interface ILaserBeamContacts
    {
        void Entered(IEntity target);
        void Exited(IEntity target);
    }
}
