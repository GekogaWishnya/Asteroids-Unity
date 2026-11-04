using System;

namespace Asteroids.Enemies
{
    internal interface IPooledEnemy<out TSelf> : IDisposable where TSelf : class
    {
        /// <summary>Raised once per life, when the enemy is out of play again and can be reused.</summary>
        event Action<TSelf> Finished;

        bool IsInPlay { get; }

        /// <summary>Takes the enemy out of play without a death event.</summary>
        void Despawn();
    }
}