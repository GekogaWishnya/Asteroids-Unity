using System;
using Asteroids.Core;

namespace Asteroids.Controls
{
    /// <summary>
    /// A fire trigger as gameplay sees it. It fires only while <see cref="InputScheme.Gameplay"/> is active, and
    /// only once its button has been seen released since Gameplay last became active, so a button still held from a
    /// menu does not fire.
    /// </summary>
    public sealed class GameplayFireTrigger : IFireInput
    {
        private readonly IFireInput _fireInput;
        private readonly IInputSchemes _schemes;
        private bool _armed;

        /// <param name="fireInput">The button as the hardware reports it.</param>
        /// <exception cref="ArgumentNullException">Any argument is null.</exception>
        public GameplayFireTrigger(IFireInput fireInput, IInputSchemes schemes)
        {
            _fireInput = fireInput ?? throw new ArgumentNullException(nameof(fireInput));
            _schemes = schemes ?? throw new ArgumentNullException(nameof(schemes));
        }

        public bool IsFiring => _armed && IsGameplay && _fireInput.IsFiring;

        private bool IsGameplay => _schemes.Active.Value == InputScheme.Gameplay;

        /// <summary>Requires a fresh release before the next shot. Call when Gameplay becomes active.</summary>
        public void Disarm() => _armed = false;

        /// <summary>Arms the trigger if Gameplay is active and the button is up. Call once per rendered frame.</summary>
        public void Sample()
        {
            if (!_armed && IsGameplay && !_fireInput.IsFiring) _armed = true;
        }
    }
}