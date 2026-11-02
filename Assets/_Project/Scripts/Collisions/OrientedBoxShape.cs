using System;
using Asteroids.Core;
using UnityEngine;

namespace Asteroids.Collisions
{
    public sealed class OrientedBoxShape : IShape
    {
        private readonly IPose _pose;
        private readonly Vector2 _offset;
        
        /// <param name="offset">Center offset in the pose's local space (x along <see cref="IPose.Right"/>, y along <see cref="IPose.Up"/>).</param>
        /// <exception cref="ArgumentNullException"><paramref name="pose"/> is null.</exception>
        /// <exception cref="ArgumentOutOfRangeException">A side of <paramref name="size"/> is negative or not finite.</exception>
        public OrientedBoxShape(IPose pose, Vector2 offset, Vector2 size)
        {
            _pose = pose ?? throw new ArgumentNullException(nameof(pose));
            if (!IsValidSide(size.x) || !IsValidSide(size.y))
                throw new ArgumentOutOfRangeException(nameof(size), "Box sides must be finite and non-negative.");

            _offset = offset;
            HalfExtents = size * 0.5f;
        }

        /// <summary>Half of the size.</summary>
        public Vector2 HalfExtents { get; }

        /// <summary>World-space center.</summary>
        public Vector2 Center => _pose.Position + _pose.Right * _offset.x + _pose.Up * _offset.y;

        /// <summary>Unit vector along the width.</summary>
        public Vector2 AxisX => _pose.Right;

        /// <summary>Unit vector along the height.</summary>
        public Vector2 AxisY => _pose.Up;

        private static bool IsValidSide(float value) => float.IsFinite(value) && value >= 0f;
    }
}
