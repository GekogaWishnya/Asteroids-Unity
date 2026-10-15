using System;
using Asteroids.Core;
using Asteroids.Core.IConfigs;
using Asteroids.Physics;
using UnityEngine;
using Zenject;

namespace Asteroids.Player.Components
{
    /// <summary>
    /// Scene adapter of the player's ship. Builds the ship's steps from the injected configuration
    /// and keeps them registered in the simulation loop while the component is enabled.
    /// Needs a sibling <see cref="IEntityProvider"/> (EntityComponent) that owns the pose.
    /// </summary>
    [AddComponentMenu("Asteroids/Player/Ship")]
    public sealed class ShipComponent : MonoBehaviour
    {
        private ISimulationLoop _loop;
        private ISimulationStep _controlStep;
        private ISimulationStep _motionStep;
        private IDisposable _controlRegistration;
        private IDisposable _motionRegistration;

        [Inject]
        private void Construct(IMovementInput input, IShipMovementConfig config, ISimulationLoop loop)
        {
            if (!TryGetComponent<IEntityProvider>(out var provider))
                throw new InvalidOperationException(
                    $"{nameof(ShipComponent)} on '{name}' needs a sibling component implementing " +
                    $"{nameof(IEntityProvider)} (add EntityComponent).");

            var pose = provider.Entity.Pose;
            var motion = new InertialMotion(config);

            _controlStep = new ShipControlStep(input, config, pose, motion);
            _motionStep = new MotionStep(pose, motion);
            _loop = loop;

            if (isActiveAndEnabled) Register();
        }

        private void OnEnable()
        {
            if (_loop != null) Register();
        }

        private void OnDisable() => Unregister();

        private void Register()
        {
            if (_controlRegistration != null) return;

            // Order of these two calls does not matter.
            _controlRegistration = _loop.Register(SimulationPhase.Input, _controlStep);
            _motionRegistration = _loop.Register(SimulationPhase.Motion, _motionStep);
        }

        private void Unregister()
        {
            _controlRegistration?.Dispose();
            _motionRegistration?.Dispose();
            _controlRegistration = null;
            _motionRegistration = null;
        }
    }
}
