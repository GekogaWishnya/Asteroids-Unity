using System;
using Asteroids.Core;
using UnityEngine;

namespace Asteroids.Collisions
{
    /// <summary>Circle that follows an <see cref="IPose"/>.</summary>
    public sealed class CircleShape : IShape
    {
        private readonly IPose _pose;
        private readonly Vector2 _offset;
        private float _radius;

        /// <param name="pose">Source of the circle's position. Read lazily, every time <see cref="Center"/> is requested.</param>
        /// <param name="offset">Center offset in the pose's local space (x along <see cref="IPose.Right"/>, y along <see cref="IPose.Up"/>).</param>
        /// <param name="radius">Finite and non-negative.</param>
        public CircleShape(IPose pose, Vector2 offset, float radius)
        {
            _pose = pose ?? throw new ArgumentNullException(nameof(pose));
            _offset = offset;
            Radius = radius;
        }

        /// <summary>World-space center from the current pose.</summary>
        public Vector2 Center => _pose.Position + _pose.Right * _offset.x + _pose.Up * _offset.y;

        public float Radius
        {
            get => _radius;
            set
            {
                if (value < 0f || !float.IsFinite(value))
                    throw new ArgumentOutOfRangeException(nameof(value), "Radius must be finite and non-negative.");
                _radius = value;
            }
        }
    }
}