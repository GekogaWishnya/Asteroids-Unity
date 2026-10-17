using System;
using Asteroids.Common;
using Asteroids.Core;

namespace Asteroids.UI.Tests
{
    internal sealed class FakePlayerHealth : IPlayerHealth
    {
        public FakePlayerHealth(int current, int max)
        {
            CurrentValue = new ObservableValue<int>(current);
            MaxValue = new ObservableValue<int>(max);
        }

        public ObservableValue<int> CurrentValue { get; }
        public ObservableValue<int> MaxValue { get; }

        public IObservableValue<int> Current => CurrentValue;
        public IObservableValue<int> Max => MaxValue;
        public bool IsDead => CurrentValue.Value == 0;

        public event Action Depleted
        {
            add { }
            remove { }
        }
    }

    internal sealed class FakeScore : IScore
    {
        public FakeScore(int value) => ValueSource = new ObservableValue<int>(value);

        public ObservableValue<int> ValueSource { get; }

        public IObservableValue<int> Value => ValueSource;
    }
}
