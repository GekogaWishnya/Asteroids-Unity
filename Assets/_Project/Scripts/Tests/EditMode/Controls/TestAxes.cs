using System;
using System.Collections.Generic;

namespace Asteroids.Controls.Tests
{
    /// <summary>An <see cref="IAxisReader"/> with settable values. Axes never set read 0.</summary>
    internal sealed class TestAxes : IAxisReader
    {
        private readonly Dictionary<string, float> _values = new();

        public float this[string axis]
        {
            get => _values.TryGetValue(axis, out var value) ? value : 0f;
            set => _values[axis] = value;
        }

        public float Read(string axis) => this[axis];
    }
}
