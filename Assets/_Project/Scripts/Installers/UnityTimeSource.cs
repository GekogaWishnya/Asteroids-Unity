using Asteroids.Core;
using UnityEngine;

namespace Asteroids.Installers
{
    /// <summary>
    /// Unity-backed clock for the simulation loop using unscaled delta time.
    /// </summary>
    public sealed class UnityTimeSource : ISimulationTime
    {
        public float DeltaTime => Time.unscaledDeltaTime;
    }
}
