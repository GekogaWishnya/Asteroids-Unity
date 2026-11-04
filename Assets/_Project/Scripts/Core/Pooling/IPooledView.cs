using System;
using UnityEngine;

namespace Asteroids.Core
{
    /// <summary>
    /// The object of one pooled entity. Created hidden.
    /// Showing and hiding put it into play and take it out again, and disposing destroys it.
    /// <see cref="Show"/> must come before the pose is set.
    /// </summary>
    public interface IPooledView : IDisposable
    {
        IEntity Entity { get; }

        void Show(Vector2 position, float angle);
        void Hide();

        /// <summary>Puts every collider of the object into collision detection until the handle is disposed.</summary>
        IDisposable RegisterColliders();
    }
}