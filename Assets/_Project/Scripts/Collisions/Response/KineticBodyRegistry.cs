using System;
using Asteroids.Common;
using Asteroids.Core;

namespace Asteroids.Collisions.Response
{
    /// <summary>Maps entities to the bodies they bounce with.</summary>
    public sealed class KineticBodyRegistry : IKineticBodyRegistry, IKineticBodyLookup
    {
        private readonly RegistrationMap<IEntity, IKineticBody> _bodies = new("kinetic body");

        public IDisposable Register(IEntity entity, IKineticBody body) =>
            _bodies.Add(entity, body, nameof(entity), nameof(body));

        public bool TryGet(IEntity entity, out IKineticBody body) => _bodies.TryGet(entity, out body);
    }
}
