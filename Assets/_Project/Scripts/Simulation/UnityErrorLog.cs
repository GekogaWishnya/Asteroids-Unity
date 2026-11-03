using System;
using Asteroids.Common;
using UnityEngine;

namespace Asteroids.Simulation
{
    /// <summary>Reports caught failures to the Unity console with the original stack trace.</summary>
    public sealed class UnityErrorLog : IErrorLog
    {
        public void Report(Exception failure, string context)
        {
            if (failure == null) throw new ArgumentNullException(nameof(failure));

            Debug.LogError(context);
            Debug.LogException(failure);
        }
    }
}
