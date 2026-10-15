using System;
using Asteroids.Core;
using UnityEngine;

namespace Asteroids.Physics
{
    public sealed class BodyPose : IPose
    {
        private float _angle;

        public Vector2 Position { get; set; }

        /// <summary>Degrees, counter-clockwise, 0 faces +Y. Normalised to [0, 360).</summary>
        public float Angle
        {
            get => _angle;
            set
            {
                if (!float.IsFinite(value))
                    throw new ArgumentOutOfRangeException(nameof(value), "Angle must be finite.");

                _angle = Mathf.Repeat(value, 360f);

                var radians = _angle * Mathf.Deg2Rad;
                var sin = Mathf.Sin(radians);
                var cos = Mathf.Cos(radians);
                Up = new Vector2(-sin, cos);
                Right = new Vector2(cos, sin);
            }
        }

        public Vector2 Up { get; private set; } = Vector2.up;

        public Vector2 Right { get; private set; } = Vector2.right;
    }
}
