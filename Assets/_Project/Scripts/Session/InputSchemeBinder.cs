using System;
using Asteroids.Core;
using Zenject;

namespace Asteroids.Session
{
    /// <summary>Controls the current <see cref="IInputSchemes"/>.</summary>
    public sealed class InputSchemeBinder : IInitializable, IDisposable
    {
        private readonly IGameSession _session;
        private readonly IInputSchemes _schemes;
        private IDisposable _subscription;

        /// <exception cref="ArgumentNullException">Any argument is null.</exception>
        public InputSchemeBinder(IGameSession session, IInputSchemes schemes)
        {
            _session = session ?? throw new ArgumentNullException(nameof(session));
            _schemes = schemes ?? throw new ArgumentNullException(nameof(schemes));
        }

        public void Initialize()
        {
            _subscription?.Dispose();
            _subscription = _session.State.Subscribe(state => _schemes.Activate(SchemeFor(state.Phase)));
        }

        public void Dispose()
        {
            _subscription?.Dispose();
            _subscription = null;
        }

        /// <exception cref="ArgumentOutOfRangeException"><paramref name="phase"/> is not defined.</exception>
        public static InputScheme SchemeFor(RunPhase phase) => phase switch
        {
            RunPhase.Playing => InputScheme.Gameplay,
            RunPhase.Ending => InputScheme.None,
            RunPhase.Ended => InputScheme.UI,
            _ => throw new ArgumentOutOfRangeException(nameof(phase), phase, "Unknown run phase.")
        };
    }
}