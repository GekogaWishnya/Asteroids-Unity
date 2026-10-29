using System;
using System.Collections.Generic;
using Asteroids.Core;
using UnityEngine;
using Zenject;

namespace Asteroids.Player.Components
{
    /// <summary>
    /// Scene adapter of the player's ship. Asks the injected factory for the ship's controller
    /// and keeps it enabled while this component is enabled.
    /// It is also what the scene lends to the ship's protection: every <see cref="ICollisionIgnore"/> and
    /// <see cref="IProtectionView"/> on this GameObject.
    /// Needs a sibling <see cref="IEntityProvider"/> (EntityComponent) that owns the pose.
    /// </summary>
    [AddComponentMenu("Asteroids/Player/Ship")]
    public sealed class ShipComponent : MonoBehaviour, IProtectionTargets
    {
        private IShipController _controller;

        public IReadOnlyList<ICollisionIgnore> Colliders { get; private set; } = Array.Empty<ICollisionIgnore>();
        public IReadOnlyList<IProtectionView> Views { get; private set; } = Array.Empty<IProtectionView>();

        [Inject]
        private void Construct(ShipControllerFactory factory)
        {
            if (!TryGetComponent<IEntityProvider>(out var provider))
                throw new InvalidOperationException(
                    $"{nameof(ShipComponent)} on '{name}' needs a sibling component implementing " +
                    $"{nameof(IEntityProvider)} (add EntityComponent).");

            Colliders = GetComponents<ICollisionIgnore>();
            Views = GetComponents<IProtectionView>();
            _controller = factory.Create(provider.Entity, this);

            if (isActiveAndEnabled) _controller.Enable();
        }

        private void OnEnable() => _controller?.Enable();
        private void OnDisable() => _controller?.Disable();
    }
}
