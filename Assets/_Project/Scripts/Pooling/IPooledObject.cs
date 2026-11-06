using System;

namespace Asteroids.Pooling
{
    public interface IPooledObject<out TSelf> : IDisposable where TSelf : class
    {
        /// <summary>Raised once per use, when the object is out of play again and can be reused.</summary>
        event Action<TSelf> Finished;

        /// <summary>Takes the object out of play without raising death events or such.</summary>
        void Despawn();
    }
}