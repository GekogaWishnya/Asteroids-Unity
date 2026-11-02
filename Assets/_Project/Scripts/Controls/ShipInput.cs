using System;
using Asteroids.Core;
using UnityEngine.InputSystem;
using Zenject;

namespace Asteroids.Controls
{
    /// <summary>The ship's controls: turn and thrust, Fire for the primary weapon, Alt Fire for the alternate one.</summary>
    public class ShipInput : IMovementInput, IWeaponTriggers, IInitializable, IDisposable
    {
        private readonly GameInput _input = new();
        private readonly IFireInput _fire;
        private readonly IFireInput _altFire;

        public ShipInput()
        {
            _fire = new ButtonTrigger(_input.Gameplay.Fire);
            _altFire = new ButtonTrigger(_input.Gameplay.AltFire);
        }

        public float Turn   => _input.Gameplay.Turn.ReadValue<float>();
        public float Thrust => _input.Gameplay.Thrust.ReadValue<float>();

        public IFireInput For(WeaponSlot slot) => slot switch
        {
            WeaponSlot.Primary => _fire,
            WeaponSlot.Alternate => _altFire,
            _ => throw new ArgumentOutOfRangeException(nameof(slot), slot, "Unknown weapon slot.")
        };

        public void Initialize() => _input.Gameplay.Enable();

        public void Dispose()
        {
            _input.Gameplay.Disable();
            _input.Dispose();
        }

        private sealed class ButtonTrigger : IFireInput
        {
            private readonly InputAction _action;

            public ButtonTrigger(InputAction action) => _action = action;

            public bool IsFiring => _action.IsPressed();
        }
    }
}
