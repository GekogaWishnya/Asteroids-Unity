namespace Asteroids.Controls
{
    /// <summary>
    /// Names of the Input Manager axes the ship reads.
    /// Each name has a keyboard/mouse entry and a gamepad entry in ProjectSettings/InputManager.asset.
    /// </summary>
    public static class ShipInputAxes
    {
        /// <summary>-1 turns left, +1 turns right.</summary>
        public const string Turn = "Ship Turn";

        /// <summary>+1 is full thrust.</summary>
        public const string Thrust = "Ship Thrust";

        public const string Fire = "Ship Fire";
        public const string AltFire = "Ship Alt Fire";
    }
}