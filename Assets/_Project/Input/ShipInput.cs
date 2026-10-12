using Asteroids.Core;
using System;
using Zenject;

namespace Asteroids.Input
{
    public class ShipInput : IMovementInput, IInitializable, IDisposable
    {
        private readonly GameInput _input = new();

        public float Turn   => _input.Gameplay.Turn.ReadValue<float>();
        public float Thrust => _input.Gameplay.Thrust.ReadValue<float>();

        public void Initialize() => _input.Gameplay.Enable();

        public void Dispose()
        {
            _input.Gameplay.Disable();
            _input.Dispose();
        }
    }
}
