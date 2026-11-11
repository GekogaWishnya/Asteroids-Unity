using System.Threading;
using Cysharp.Threading.Tasks;

namespace Asteroids.Ticking
{
    public interface IFrameSource
    {
        /// <returns>True when <paramref name="cancellation"/> fired before the frame came. Never throws on cancellation.</returns>
        UniTask<bool> NextFrameAsync(PlayerLoopTiming timing, CancellationToken cancellation);
    }
}