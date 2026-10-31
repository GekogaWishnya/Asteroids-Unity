using Asteroids.Core;

namespace Asteroids.Collisions.Response
{
    public interface IKineticBodyLookup
    {
        bool TryGet(IEntity entity, out IKineticBody body);
    }
}
