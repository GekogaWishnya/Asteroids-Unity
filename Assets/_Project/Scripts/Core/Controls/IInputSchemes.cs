using System;
using Asteroids.Common;

namespace Asteroids.Core
{
    /// <summary>The active <see cref="InputScheme"/>.</summary>
    public interface IInputSchemes
    {
        IObservableValue<InputScheme> Active { get; }

        /// <exception cref="ArgumentOutOfRangeException"><paramref name="scheme"/> is not defined.</exception>
        void Activate(InputScheme scheme);
    }
}