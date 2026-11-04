using UnityEngine;

namespace Asteroids.Weapons.Components
{
    /// <summary>Weapons the ship carries in a level, one per slot. Both slots are optional.</summary>
    [CreateAssetMenu(fileName = "ShipLoadout", menuName = "Asteroids/Weapons/Ship Loadout")]
    public sealed class ShipLoadoutDefinition : ScriptableObject
    {
        [Tooltip("Fired with Fire. Leave empty for none")] [SerializeField]
        private WeaponDefinition primary;

        [Tooltip("Fired with Alt Fire. Leave empty for none")] [SerializeField]
        private WeaponDefinition alternate;

        public WeaponDefinition Primary => primary;
        public WeaponDefinition Alternate => alternate;
    }
}