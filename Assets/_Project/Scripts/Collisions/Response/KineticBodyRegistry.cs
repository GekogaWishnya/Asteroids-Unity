using System;
using System.Collections.Generic;
using Asteroids.Core;

namespace Asteroids.Collisions.Response
{
    /// <summary>Maps entities to the bodies they bounce with.</summary>
    public sealed class KineticBodyRegistry : IKineticBodyRegistry, IKineticBodyLookup
    {
        private readonly Dictionary<IEntity, IKineticBody> _bodies = new();

        public IDisposable Register(IEntity entity, IKineticBody body)
        {
            if (entity == null) throw new ArgumentNullException(nameof(entity));
            if (body == null) throw new ArgumentNullException(nameof(body));

            if (!_bodies.TryAdd(entity, body))
                throw new InvalidOperationException("The entity is already registered as a kinetic body.");

            return new Registration(_bodies, entity, body);
        }

        public bool TryGet(IEntity entity, out IKineticBody body)
        {
            if (entity == null)
            {
                body = null;
                return false;
            }

            return _bodies.TryGetValue(entity, out body);
        }
        
        private sealed class Registration : IDisposable
        {
            private readonly Dictionary<IEntity, IKineticBody> _bodies;
            private readonly IEntity _entity;
            private readonly IKineticBody _body;

            public Registration(Dictionary<IEntity, IKineticBody> bodies, IEntity entity, IKineticBody body)
            {
                _bodies = bodies;
                _entity = entity;
                _body = body;
            }

            public void Dispose()
            {
                if (_bodies.TryGetValue(_entity, out var current) && ReferenceEquals(current, _body))
                    _bodies.Remove(_entity);
            }
        }
    }
}
