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
        private IMotionDriver _motion;

        [Inject]
        public void Construct(IMovementInput input, IShipMovementConfig config)
        {
            _input = input;
            _config = config;

            var body = GetComponent<InertialBody>();
            body.Configure(config);
            _motion = body;
        }

        private void Update()
        {
            transform.Rotate(0f, 0f, -_input.Turn * _config.TurnSpeed * Time.deltaTime);
            _motion.Accelerate(transform.up * (_input.Thrust * _config.Thrust));
        }
    }
}
