using System;
using Asteroids.Core;
using UnityEngine;

namespace Asteroids.Weapons.Laser
{
    public interface ILaserBeamView : IDisposable
    {
        IEntity Entity { get; }

        void Show(Vector2 position, float angle);
        void Hide();
    }
}
