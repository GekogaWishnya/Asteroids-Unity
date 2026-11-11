using System;
using System.Threading;
using Asteroids.Common;
using Cysharp.Threading.Tasks;
using Zenject;

namespace Asteroids.Ticking
{
    /// <summary>
    /// Ticks one <typeparamref name="TTick"/> once per frame at one <see cref="PlayerLoopTiming"/>, from
    /// <see cref="Initialize"/> until <see cref="Dispose"/>.
    /// Bind it with <see cref="FrameTickBindings.BindFrameTick{TTick}"/>.
    /// </summary>
    public sealed class FrameTicker<TTick> : IInitializable, IDisposable where TTick : class, IFrameTick
    {
        private readonly TTick _tick;
        private readonly PlayerLoopTiming _timing;
        private readonly IFrameSource _frames;
        private readonly IErrorLog _errors;
        private CancellationTokenSource _cancellation;
        private bool _isDisposed;

        /// <exception cref="ArgumentNullException">Any reference argument is null.</exception>
        public FrameTicker(TTick tick, PlayerLoopTiming timing, IFrameSource frames, IErrorLog errors)
        {
            _tick = tick ?? throw new ArgumentNullException(nameof(tick));
            _timing = timing;
            _frames = frames ?? throw new ArgumentNullException(nameof(frames));
            _errors = errors ?? throw new ArgumentNullException(nameof(errors));
        }

        public bool IsRunning => _cancellation != null;

        /// <summary>Starts the loop. The first tick comes on the next <see cref="PlayerLoopTiming"/> it waits for.</summary>
        /// <exception cref="ObjectDisposedException">Already disposed.</exception>
        /// <exception cref="InvalidOperationException">Already running.</exception>
        public void Initialize()
        {
            if (_isDisposed) throw new ObjectDisposedException(GetType().Name);
            if (_cancellation != null) throw new InvalidOperationException($"{Name} is already running.");

            _cancellation = new CancellationTokenSource();
            RunAsync(_cancellation.Token).Forget();
        }

        /// <summary>Stops the loop. No tick runs after this returns.</summary>
        public void Dispose()
        {
            if (_isDisposed) return;
            _isDisposed = true;

            var cancellation = _cancellation;
            if (cancellation == null) return;

            _cancellation = null;
            cancellation.Cancel();
            cancellation.Dispose();
        }

        private string Name => $"{nameof(FrameTicker<TTick>)}<{typeof(TTick).Name}>";

        private async UniTaskVoid RunAsync(CancellationToken cancellation)
        {
            while (!await _frames.NextFrameAsync(_timing, cancellation))
            {
                if (cancellation.IsCancellationRequested) return;

                try
                {
                    _tick.Tick();
                }
                catch (Exception e)
                {
                    _errors.Report(e, $"{typeof(TTick).Name} failed at {_timing}; it runs again on the next frame.");
                }
            }
        }
    }
}