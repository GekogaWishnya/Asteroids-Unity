using System;
using System.Collections.Generic;
using Asteroids.Core;

namespace Asteroids.Player.Tests
{
    internal sealed class FakeProtectionView : IProtectionView
    {
        public List<bool> Calls { get; } = new();

        public void SetProtected(bool isProtected) => Calls.Add(isProtected);
    }

    internal sealed class FakeProtectionTargets : IProtectionTargets
    {
        public FakeProtectionTargets(
            IReadOnlyList<ICollisionIgnore> colliders = null, IReadOnlyList<IProtectionView> views = null)
        {
            Colliders = colliders ?? Array.Empty<ICollisionIgnore>();
            Views = views ?? Array.Empty<IProtectionView>();
        }

        public IReadOnlyList<ICollisionIgnore> Colliders { get; }
        public IReadOnlyList<IProtectionView> Views { get; }
    }
}
