using System;
using System.Collections.Generic;
using Asteroids.Core;

namespace Asteroids.Player.Tests
{
    internal sealed class FakeKineticBodyRegistry : IKineticBodyRegistry
    {
        private readonly List<Registration> _registrations = new();

        public IReadOnlyList<Registration> Registrations => _registrations;

        /// <summary>Called on every registration.</summary>
        public Action OnRegister { get; set; }

        public int LiveCount
        {
            get
            {
                var count = 0;
                foreach (var registration in _registrations)
                    if (!registration.IsDisposed) count++;
                return count;
            }
        }

        public IDisposable Register(IEntity entity, IKineticBody body)
        {
            var registration = new Registration(entity, body);
            _registrations.Add(registration);
            OnRegister?.Invoke();
            return registration;
        }

        public sealed class Registration : IDisposable
        {
            public Registration(IEntity entity, IKineticBody body)
            {
                Entity = entity;
                Body = body;
            }

            public IEntity Entity { get; }
            public IKineticBody Body { get; }
            public bool IsDisposed { get; private set; }

            public void Dispose() => IsDisposed = true;
        }
    }

    internal sealed class TestControlLock : IShipControlLock
    {
        public bool IsLocked { get; set; }
    }
}
