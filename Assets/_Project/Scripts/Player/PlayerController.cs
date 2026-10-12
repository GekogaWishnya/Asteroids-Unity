using Asteroids.Core;
using UnityEngine;
using Zenject;

namespace Asteroids.Player
{
    [RequireComponent(typeof(InertialBody))]
    public sealed class PlayerController : MonoBehaviour
    {
        private IMovementInput _input;
        private IShipMovementConfig _config;
        private InertialBody _ib;
            
        [Inject]
        public void Construct(IMovementInput input, IShipMovementConfig config, IBoundaryPolicy boundaryPolicy)
        {
            _input = input;
            _config = config;

            var body = GetComponent<InertialBody>();
            body.Configure(config, boundaryPolicy);
            _ib = body;
        }

        private void Update()
        {
            transform.Rotate(0f, 0f, -_input.Turn * _config.TurnSpeed * Time.deltaTime);
            _ib.Driver.Accelerate(transform.up * (_input.Thrust * _config.Thrust));
        }
    }
}
