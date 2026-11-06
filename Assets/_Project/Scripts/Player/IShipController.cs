namespace Asteroids.Player
{
    /// <summary>
    /// Switches ship on and off. While disabled the ship neither reads input nor moves.
    /// </summary>
    public interface IShipController
    {
        void Enable();
        void Disable();

        /// <summary>Puts the ship back to how the game started.</summary>
        void Restart();
    }
}