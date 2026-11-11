using System;
using Asteroids.Core;

namespace Asteroids.Physics
{
    /// <summary>Where an <see cref="IPoseView"/> registers to follow a pose once per frame.</summary>
    public interface IPoseViewRegistry
    {
        /// <returns>Dispose to stop following.</returns>
        /// <exception cref="ArgumentNullException">Any argument is null.</exception>
        IDisposable Register(IPose pose, IPoseView view);
    }
}