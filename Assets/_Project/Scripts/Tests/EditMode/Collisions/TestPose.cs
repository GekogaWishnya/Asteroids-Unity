using Asteroids.Core;
using UnityEngine;

namespace Asteroids.Collisions.Tests
{
    internal sealed class TestPose : IPose
    {
        private float _angle;

        public TestPose(Vector2 position, float angle = 0f)
        {
            Position = position;
            Angle = angle;
        }

        public Vector2 Position { get; set; }
        public Vector2 Up { get; private set; } = Vector2.up;
        public Vector2 Right { get; private set; } = Vector2.right;

        public float Angle
        {
            get => _angle;
            set
            {
                _angle = value;
                var radians = value * Mathf.Deg2Rad;
                Up = new Vector2(-Mathf.Sin(radians), Mathf.Cos(radians));
                Right = new Vector2(Mathf.Cos(radians), Mathf.Sin(radians));
            }
        }
    }
}