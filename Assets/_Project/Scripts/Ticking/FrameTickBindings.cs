using System;
using Asteroids.Common;
using Cysharp.Threading.Tasks;
using Zenject;

namespace Asteroids.Ticking
{
    public static class FrameTickBindings
    {
        /// <summary>
        /// Requires <typeparamref name="TTick"/> itself, <see cref="IFrameSource"/> and
        /// <see cref="IErrorLog"/> to be bound.
        /// </summary>
        /// <exception cref="ArgumentNullException"><paramref name="container"/> is null.</exception>
        public static void BindFrameTick<TTick>(this DiContainer container, PlayerLoopTiming timing)
            where TTick : class, IFrameTick
        {
            if (container == null) throw new ArgumentNullException(nameof(container));

            container.BindInterfacesTo<FrameTicker<TTick>>().AsSingle().WithArguments(timing);
        }
    }
}