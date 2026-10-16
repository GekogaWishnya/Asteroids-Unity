using System;

namespace Asteroids.Collisions
{
    public interface IShapeContactTester
    {
        /// <summary>True when a rule exists for this pair of shape types.</summary>
        bool Supports(Type first, Type second);

        /// <summary>Tests two shapes. Runs for every candidate pair every tick.</summary>
        /// <exception cref="NotSupportedException">No rule exists for this pair of shape types.</exception>
        bool TryGetContact(IShape first, IShape second, out Contact contact);
    }
}
