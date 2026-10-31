using System;
using UnityEngine;

namespace Asteroids.Core
{
    public interface IKineticBody
    {
        /// <summary>Units per second.</summary>
        Vector2 Velocity { get; }

        /// <summary>Slowest speed (units/s) the body leaves a bounce with.</summary>
        float MinKnockbackSpeed { get; }

        /// <summary>Units per second.</summary>
        void AddVelocity(Vector2 delta);
    }
}
