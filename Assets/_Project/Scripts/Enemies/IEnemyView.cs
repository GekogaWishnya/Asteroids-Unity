using System;
using Asteroids.Core;
using UnityEngine;

namespace Asteroids.Enemies
{
    /// <summary>
    /// Created hidden. Showing and hiding put it into play and take it out again, and disposing destroys it.
    /// <see cref="Show"/> must come before the pose is set: an entity may copy its transform into its pose
    /// the first time it wakes up.
    /// </summary>
    public interface IEnemyView : IDisposable
    {
        IEntity Entity { get; }
        void Show(Vector2 position, float angle);
        void Hide();
    }
}
