using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;

namespace Asteroids.Ticking.Tests
{
    /// <summary>
    /// A frame source the test steps by hand. Completing the pending wait runs the awaiting loop synchronously, so
    /// after <see cref="Frame"/> returns the tick has already happened and the loop is waiting again.
    /// No player loop is involved, so these tests run in Edit Mode.
    /// </summary>
    internal sealed class ManualFrames : IFrameSource
    {
        private UniTaskCompletionSource<bool> _pending;
        private CancellationTokenRegistration _registration;

        /// <summary>Every timing a loop asked to wait for, in order.</summary>
        public List<PlayerLoopTiming> Requested { get; } = new();

        public bool IsWaiting => _pending != null;

        public UniTask<bool> NextFrameAsync(PlayerLoopTiming timing, CancellationToken cancellation)
        {
            Requested.Add(timing);
            if (cancellation.IsCancellationRequested) return UniTask.FromResult(true);
            if (_pending != null) throw new InvalidOperationException("Only one loop may wait on a ManualFrames.");

            var pending = new UniTaskCompletionSource<bool>();
            _pending = pending;
            _registration = cancellation.Register(() =>
            {
                if (_pending == pending) _pending = null;
                pending.TrySetResult(true);
            });
            return pending.Task;
        }

        /// <summary>Lets one frame pass.</summary>
        public void Frame()
        {
            var pending = _pending ?? throw new InvalidOperationException("Nothing is waiting for a frame.");
            _pending = null;
            _registration.Dispose();
            pending.TrySetResult(false);
        }
    }
}
