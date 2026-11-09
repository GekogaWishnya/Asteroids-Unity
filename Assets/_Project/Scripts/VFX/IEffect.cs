using System;
using UnityEngine;

namespace Asteroids.VFX
{
    public interface IEffect : IDisposable
    {
        /// <summary>Raised once per play, when the effect has finished by itself.</summary>
        event Action<IEffect> Finished;

        /// <summary>Shows the effect at <paramref name="position"/> and plays it from the start.</summary>
        void Play(Vector2 position, float scale);

        /// <summary>Ends the play and hides the effect.</summary>
        void Stop();

        void Pause();
        void Resume();
    }
}