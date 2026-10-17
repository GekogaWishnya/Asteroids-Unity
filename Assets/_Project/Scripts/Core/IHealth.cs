using System;
using Asteroids.Common;

namespace Asteroids.Core
{
    /// <summary>View of an entity's health.</summary>
    public interface IHealth
    {
        IObservableValue<int> Current { get; }
        IObservableValue<int> Max { get; }
        
        bool IsDead { get; }
        event Action Depleted;
    }
}
