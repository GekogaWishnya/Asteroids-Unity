using System;
using Asteroids.Core;

namespace Asteroids.Player.Tests
{
    /// <summary>The ship's controls. Its own <see cref="IsFiring"/> is the primary trigger.</summary>
    internal sealed class TestShipInput : IMovementInput, IFireInput, IWeaponTriggers
    {
        public float Turn { get; set; }
        public float Thrust { get; set; }
        public bool IsFiring { get; set; }

        public TestTrigger Alternate { get; } = new();

        public IFireInput For(WeaponSlot slot) => slot switch
        {
            WeaponSlot.Primary => this,
            WeaponSlot.Alternate => Alternate,
            _ => throw new ArgumentOutOfRangeException(nameof(slot))
        };
    }

    internal sealed class TestTrigger : IFireInput
    {
        public bool IsFiring { get; set; }
    }

    internal sealed class FakeLoadout : IShipLoadout
    {
        public IWeapon Primary { get; set; }
        public IWeapon Alternate { get; set; }

        public bool TryGetWeapon(WeaponSlot slot, out IWeapon weapon)
        {
            weapon = slot == WeaponSlot.Primary ? Primary : Alternate;
            return weapon != null;
        }
    }
}
