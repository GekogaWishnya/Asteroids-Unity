using System;
using Asteroids.Core;
using Asteroids.Core.IConfigs;
using Asteroids.Invulnerability;

namespace Asteroids.Player
{
    /// <summary>
    /// Protects the ship when a wave starts. The ship's controller attaches its guard while it is enabled.
    /// The duration is read from the config on every wave.
    /// </summary>
    public sealed class WaveStartProtection
    {
        private readonly IWaveProtectionConfig _config;
        private InvulnerabilityGuard _guard;

        /// <exception cref="ArgumentNullException"><paramref name="config"/> is null.</exception>
        public WaveStartProtection(IWaveProtectionConfig config) =>
            _config = config ?? throw new ArgumentNullException(nameof(config));

        /// <exception cref="ArgumentNullException"><paramref name="guard"/> is null.</exception>
        /// <exception cref="InvalidOperationException">A guard is already attached.</exception>
        public IDisposable Attach(InvulnerabilityGuard guard)
        {
            if (guard == null) throw new ArgumentNullException(nameof(guard));
            if (_guard != null) throw new InvalidOperationException("A ship is already attached.");

            _guard = guard;
            return new Attachment(this, guard);
        }

        public void OnWaveStarted(WaveStarted signal)
        {
            var seconds = _config.WaveStartProtectionSeconds;
            if (_guard == null || seconds <= 0f) return;

            _guard.Grant(seconds);
        }

        private sealed class Attachment : IDisposable
        {
            private readonly WaveStartProtection _owner;
            private readonly InvulnerabilityGuard _guard;

            public Attachment(WaveStartProtection owner, InvulnerabilityGuard guard)
            {
                _owner = owner;
                _guard = guard;
            }

            public void Dispose()
            {
                if (ReferenceEquals(_owner._guard, _guard)) _owner._guard = null;
            }
        }
    }
}
