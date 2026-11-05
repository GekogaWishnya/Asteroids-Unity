using System;

namespace Asteroids.Core
{
    public interface IWrapRegistry
    {
        /// <returns>Dispose to stop wrapping the pose.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="pose"/> is null.</exception>
        /// <exception cref="InvalidOperationException">The pose is already registered.</exception>
        IDisposable Register(IPose pose);

        /// <summary>
        /// Registers a pose that starts outside the play area and flies in, such as a spawned enemy. It isn't
        /// wrapped until it has been inside once. If it is still outside after the configured longest entry time,
        /// it is moved onto the nearest edge of the area. From then on it wraps like any other pose.
        /// </summary>
        /// <returns>Dispose to stop wrapping the pose.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="pose"/> is null.</exception>
        /// <exception cref="InvalidOperationException">The pose is already registered.</exception>
        IDisposable RegisterEntering(IPose pose);
    }
}