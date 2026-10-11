using System;
using Asteroids.Core;
using Zenject;

namespace Asteroids.Player
{
    public class ShipInput : IMovementInput, IInitializable, IDisposable
    {
        private readonly GameInput _input = new();

        public float Turn   => _input.Gameplay.Turn.ReadValue<float>();
        public float Thrust => _input.Gameplay.Thrust.ReadValue<float>();

        public void Initialize()
        {
            _input.Gameplay.Enable();
        }

        public void Dispose()
        {
            _input.Dispose();
        }
    }
}
