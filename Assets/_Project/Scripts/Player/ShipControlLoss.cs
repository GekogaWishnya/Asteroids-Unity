using System;
using Asteroids.Core;

namespace Asteroids.Player
{
    /// <summary>
    /// Locks the ship's controls for a while with <see cref="Lose"/>. While attached, the countdown runs
    /// in <see cref="SimulationPhase.Logic"/>. Detaching gives control back.
    /// </summary>
    public sealed class ShipControlLoss : IShipPart, IShipControlLock
    {
        private readonly ISimulationLoop _loop;
        private readonly PartAttachment _attachment = new("ship control loss");
        private readonly Countdown _countdown = new();
        private bool _isAttached;

        /// <exception cref="ArgumentNullException"><paramref name="loop"/> is null.</exception>
        public ShipControlLoss(ISimulationLoop loop) => _loop = loop ?? throw new ArgumentNullException(nameof(loop));

        public bool IsLocked => _countdown.Remaining > 0f;

        /// <summary>Seconds until control comes back.</summary>
        public float Remaining => _countdown.Remaining;

        /// <summary>
        /// Locks the controls for at least <paramref name="seconds"/>. Other locks don't get overwritten.
        /// </summary>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="seconds"/> is negative or not finite.</exception>
        public void Lose(float seconds)
        {
            if (!float.IsFinite(seconds) || seconds < 0f)
                throw new ArgumentOutOfRangeException(
                    nameof(seconds), "Duration must be a finite number that is not negative.");

            if (!_isAttached) return;

            _countdown.Remaining = Math.Max(_countdown.Remaining, seconds);
        }

        public IDisposable Attach(IEntity ship)
        {
            if (ship == null) throw new ArgumentNullException(nameof(ship));

            return _attachment.Start(new Func<IDisposable>[]
            {
                () => _loop.Register(SimulationPhase.Logic, _countdown),
                Track
            });
        }

        private IDisposable Track()
        {
            _isAttached = true;
            return new Tracking(this);
        }

        private sealed class Countdown : ISimulationStep
        {
            public float Remaining { get; set; }

            public void Tick(float deltaTime) => Remaining = Math.Max(0f, Remaining - deltaTime);
        }

        private sealed class Tracking : IDisposable
        {
            private readonly ShipControlLoss _owner;

            public Tracking(ShipControlLoss owner) => _owner = owner;

            public void Dispose()
            {
                _owner._isAttached = false;
                _owner._countdown.Remaining = 0f;
            }
        }
    }
}
