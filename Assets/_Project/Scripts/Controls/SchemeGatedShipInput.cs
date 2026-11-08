using System;
using Asteroids.Core;
using Zenject;

namespace Asteroids.Controls
{
    /// <summary>
    /// The ship's controls as <see cref="ShipInput"/>.
    /// Does nothing unless <see cref="InputScheme.Gameplay"/> is active.
    /// </summary>
    public sealed class SchemeGatedShipInput : IMovementInput, IWeaponTriggers, ITickable, IDisposable
    {
        private readonly ShipInput _shipInput;
        private readonly IInputSchemes _schemes;
        private readonly GameplayFireTrigger _primary;
        private readonly GameplayFireTrigger _alternate;
        private readonly IDisposable _subscription;

        /// <exception cref="ArgumentNullException">Any argument is null.</exception>
        public SchemeGatedShipInput(ShipInput shipInput, IInputSchemes schemes)
        {
            _shipInput = shipInput ?? throw new ArgumentNullException(nameof(shipInput));
            _schemes = schemes ?? throw new ArgumentNullException(nameof(schemes));

            _primary = new GameplayFireTrigger(shipInput.For(WeaponSlot.Primary), schemes);
            _alternate = new GameplayFireTrigger(shipInput.For(WeaponSlot.Alternate), schemes);
            _subscription = schemes.Active.Subscribe(OnSchemeChanged);
        }

        public float Turn => IsGameplay ? _shipInput.Turn : 0f;
        public float Thrust => IsGameplay ? _shipInput.Thrust : 0f;

        private bool IsGameplay => _schemes.Active.Value == InputScheme.Gameplay;

        public IFireInput For(WeaponSlot slot) => slot switch
        {
            WeaponSlot.Primary => _primary,
            WeaponSlot.Alternate => _alternate,
            _ => throw new ArgumentOutOfRangeException(nameof(slot), slot, "Unknown weapon slot.")
        };

        public void Tick()
        {
            _primary.Sample();
            _alternate.Sample();
        }

        public void Dispose() => _subscription.Dispose();

        private void OnSchemeChanged(InputScheme scheme)
        {
            if (scheme != InputScheme.Gameplay) return;

            _primary.Disarm();
            _alternate.Disarm();
        }
    }
}