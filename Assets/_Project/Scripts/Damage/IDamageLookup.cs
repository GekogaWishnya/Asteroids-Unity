using Asteroids.Core;

namespace Asteroids.Damage
{
    /// <summary>Used to hide registering methods from the code that applies damage.</summary>
    public interface IDamageLookup
    {
        bool TryGetDamageable(IEntity entity, out IDamageable damageable);
        bool TryGetSource(IEntity entity, out IDamageSource source);
    }
}
