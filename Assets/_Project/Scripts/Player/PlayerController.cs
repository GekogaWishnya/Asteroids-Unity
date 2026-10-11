using Asteroids.Core;
using Asteroids.Data;
using UnityEngine;
using Zenject;

namespace Asteroids.Player
{
    [RequireComponent(typeof(InertialBody))]
    public class PlayerController : MonoBehaviour
    {
        [SerializeField] private ShipConfig config;
        
        private IMovementInput _input;
        private IMotion _motion;
        private float _angle;
        
        public bool ControlEnabled { get; set; } = true;
        public float Angle => Mathf.Repeat(_angle, 360f);
        public Vector2 Forward => new(-Mathf.Sin(_angle * Mathf.Deg2Rad), Mathf.Cos(_angle * Mathf.Deg2Rad));

        [Inject]
        public void Construct(IMovementInput input)
        {
            _input = input;
        }

        private void Awake()
        {
            var body = GetComponent<InertialBody>();
            body.MaxSpeed = config.MaxSpeed;
            body.Damping = config.LinearDamping;
            _motion = body;
        }

        private void Update()
        {
            if (!ControlEnabled) return;
            _angle -= _input.Turn * config.TurnSpeed * Time.deltaTime;
            transform.rotation = Quaternion.Euler(0f, 0f, _angle);
        }

        private void FixedUpdate()
        {
            if (!ControlEnabled) return;
            _motion.AddVelocity(Forward * (_input.Thrust * config.Thrust * Time.fixedDeltaTime));
        }
    }
}
