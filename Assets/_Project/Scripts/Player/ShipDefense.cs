using System;
using Asteroids.Core;
using Asteroids.Core.IConfigs;
using Asteroids.Invulnerability;
using Zenject;

namespace Asteroids.Player
{
    /// <summary>
    /// Makes the ship hurtable with invulnerability frames. While attached, the ship is registered as a damageable,
    /// and what the damage system hurts is an <see cref="InvulnerabilityGuard"/> around the player's health. Each hit
    /// that gets through, and each wave start (through <see cref="WaveStartProtection"/>), starts a protection window.
    /// </summary>
    public sealed class ShipDefense : IShipPart
    {
        private readonly InvulnerabilityGuard _guard;
        private readonly WaveStartProtection _waveProtection;
        private readonly ISimulationLoop _loop;
        private readonly IDamageRegistry _damageRegistry;
        private readonly PartAttachment _attachment = new("ship defense");

        /// <param name="health">Player's health.</param>
        /// <exception cref="ArgumentNullException">Any argument, or a list of <paramref name="targets"/>, is null.</exception>
        /// <exception cref="ArgumentException">A list of <paramref name="targets"/> contains null.</exception>
        public ShipDefense(
            IDamageable health,
            IInvulnerabilityConfig config,
            IProtectionTargets targets,
            WaveStartProtection waveProtection,
            ISimulationLoop loop,
            IDamageRegistry damageRegistry)
        {
            _waveProtection = waveProtection ?? throw new ArgumentNullException(nameof(waveProtection));
            _loop = loop ?? throw new ArgumentNullException(nameof(loop));
            _damageRegistry = damageRegistry ?? throw new ArgumentNullException(nameof(damageRegistry));
            _guard = new InvulnerabilityGuard(health, config, targets);
        }

        public IDisposable Attach(IEntity ship)
        {
            if (ship == null) throw new ArgumentNullException(nameof(ship));

            return _attachment.Start(new Func<IDisposable>[]
            {
                () => _guard.Attach(_loop),
                () => _waveProtection.Attach(_guard),
                () => _damageRegistry.RegisterDamageable(ship, _guard)
            });
        }
        
        public sealed class Factory : PlaceholderFactory<IProtectionTargets, ShipDefense> { }
    }
}
