using System;
using Asteroids.Core;
using UnityEngine;

namespace Asteroids.Weapons.Projectiles
{
    /// <summary>
    /// The object of one projectile. Created hidden. Showing and hiding put it into play and take it out again,
    /// and disposing destroys it. <see cref="Show"/> must come before the pose is set.
    /// </summary>
    public interface IProjectileView : IDisposable
    {
        IEntity Entity { get; }
        void Show(Vector2 position, float angle);
        void Hide();
    }
}
