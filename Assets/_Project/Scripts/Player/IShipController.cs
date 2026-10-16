namespace Asteroids.Player
{
    /// <summary>
    /// Switches ship on and off. While disabled the ship neither reads input nor moves.
    /// </summary>
    public interface IShipController
    {
        void Enable();
        void Disable();
    }
}
