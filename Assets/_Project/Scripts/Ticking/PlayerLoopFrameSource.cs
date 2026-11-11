using System.Threading;
using Cysharp.Threading.Tasks;

namespace Asteroids.Ticking
{
    public sealed class PlayerLoopFrameSource : IFrameSource
    {
        public UniTask<bool> NextFrameAsync(PlayerLoopTiming timing, CancellationToken cancellation) =>
            UniTask.Yield(timing, cancellation, cancelImmediately: true).SuppressCancellationThrow();
    }
}