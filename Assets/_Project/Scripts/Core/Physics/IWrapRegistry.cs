using System;

namespace Asteroids.Core
{
    public interface IWrapRegistry
    {
        /// <returns>Dispose to stop wrapping the pose.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="pose"/> is null.</exception>
        /// <exception cref="InvalidOperationException">The pose is already registered.</exception>
        IDisposable Register(IPose pose);
    }
}
