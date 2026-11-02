namespace Asteroids.Core
{
    /// <summary>Weapons the ship carries in a level.</summary>
    public interface IShipLoadout
    {
        /// <returns>False when nothing is mounted in <paramref name="slot"/>.</returns>
        bool TryGetWeapon(WeaponSlot slot, out IWeapon weapon);
    }
}
