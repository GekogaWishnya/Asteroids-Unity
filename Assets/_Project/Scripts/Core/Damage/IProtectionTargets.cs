using System.Collections.Generic;

namespace Asteroids.Core
{
    public interface IProtectionTargets
    {
        IReadOnlyList<ICollisionIgnore> Colliders { get; }
        IReadOnlyList<IProtectionView> Views { get; }
    }
}
