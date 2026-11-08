using System;
using Asteroids.Common;
using Asteroids.Core;

namespace Asteroids.Controls
{
    /// <summary>Holds the active <see cref="InputScheme"/>. Starts with <see cref="InputScheme.None"/>.</summary>
    public sealed class InputSchemes : IInputSchemes
    {
        private readonly ObservableValue<InputScheme> _active = new(InputScheme.None);

        public IObservableValue<InputScheme> Active => _active;

        public void Activate(InputScheme scheme)
        {
            if (!Enum.IsDefined(typeof(InputScheme), scheme))
                throw new ArgumentOutOfRangeException(nameof(scheme), scheme, "Unknown input scheme.");

            _active.Value = scheme;
        }
    }
}
