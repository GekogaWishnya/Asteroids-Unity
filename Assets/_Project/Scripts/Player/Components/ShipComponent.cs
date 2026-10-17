using System;
using Asteroids.Core;
using UnityEngine;
using Zenject;

namespace Asteroids.Player.Components
{
    /// <summary>
    /// Scene adapter of the player's ship. Asks the injected factory for the ship's controller
    /// and keeps it enabled while this component is enabled.
    /// Needs a sibling <see cref="IEntityProvider"/> (EntityComponent) that owns the pose.
    /// </summary>
    [AddComponentMenu("Asteroids/Player/Ship")]
    public sealed class ShipComponent : MonoBehaviour
    {
        private IShipController _controller;

        [Inject]
        private void Construct(ShipControllerFactory factory)
        {
            if (!TryGetComponent<IEntityProvider>(out var provider))
                throw new InvalidOperationException(
                    $"{nameof(ShipComponent)} on '{name}' needs a sibling component implementing " +
                    $"{nameof(IEntityProvider)} (add EntityComponent).");

            _controller = factory.Create(provider.Entity);

            if (isActiveAndEnabled) _controller.Enable();
        }

        private void OnEnable() => _controller?.Enable();
        private void OnDisable() => _controller?.Disable();
    }
}