using Asteroids.Core;
using UnityEngine;

namespace Asteroids.Physics
{
    public sealed class InertialBody : MonoBehaviour
    {
        private InertialMotion _motion;
        private IBoundaryPolicy _boundaryPolicy;

        public IMotionDriver Driver => _motion;
        public IMotionState State => _motion;
        
        public void Configure(IMotionConfig motionConfig, IBoundaryPolicy boundaryPolicy)
        {
            _motion = new InertialMotion(motionConfig);
            _boundaryPolicy = boundaryPolicy;
        }

        private void LateUpdate()
        {
            if (_motion == null)
            {
                Debug.LogError($"{nameof(InertialBody)} on '{name}' was never configured.", this);
                enabled = false;
                return;
            }

            var position = (Vector2)transform.position + _motion.Step(Time.deltaTime);
            position = _boundaryPolicy.Apply(position, 0.0f);
            transform.position = new Vector3(position.x, position.y, transform.position.z);
        }
    }
}