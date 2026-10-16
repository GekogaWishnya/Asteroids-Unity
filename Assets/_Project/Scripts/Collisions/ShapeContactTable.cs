using System;
using System.Collections.Generic;

namespace Asteroids.Collisions
{
    /// <summary>Contact rule for two shape types.</summary>
    public delegate bool ContactTest<in TFirst, in TSecond>(TFirst first, TSecond second, out Contact contact)
        where TFirst : class, IShape
        where TSecond : class, IShape;

    /// <summary>
    /// Pair rules keyed by the exact runtime types of the two shapes.
    /// A rule added for (A, B) also serves (B, A).
    /// </summary>
    public sealed class ShapeContactTable : IShapeContactTester
    {
        private delegate bool Rule(IShape first, IShape second, out Contact contact);

        private readonly Dictionary<TypePair, Rule> _rules = new();

        /// <exception cref="ArgumentNullException"><paramref name="test"/> is null.</exception>
        /// <exception cref="InvalidOperationException">A rule for this pair of types already exists, in either order.</exception>
        public ShapeContactTable Add<TFirst, TSecond>(ContactTest<TFirst, TSecond> test)
            where TFirst : class, IShape
            where TSecond : class, IShape
        {
            if (test == null) throw new ArgumentNullException(nameof(test));

            var direct = new TypePair(typeof(TFirst), typeof(TSecond));
            var reversed = new TypePair(typeof(TSecond), typeof(TFirst));
            if (_rules.ContainsKey(direct) || _rules.ContainsKey(reversed))
                throw new InvalidOperationException(
                    $"A contact rule for {typeof(TFirst).Name} and {typeof(TSecond).Name} already exists.");

            _rules.Add(direct, (IShape first, IShape second, out Contact contact) =>
                test((TFirst)first, (TSecond)second, out contact));

            if (!direct.Equals(reversed))
            {
                _rules.Add(reversed, (IShape first, IShape second, out Contact contact) =>
                {
                    var found = test((TFirst)second, (TSecond)first, out var inner);
                    contact = found ? inner.Inverted : default;
                    return found;
                });
            }

            return this;
        }

        public bool Supports(Type first, Type second)
        {
            if (first == null) throw new ArgumentNullException(nameof(first));
            if (second == null) throw new ArgumentNullException(nameof(second));
            return _rules.ContainsKey(new TypePair(first, second));
        }

        public bool TryGetContact(IShape first, IShape second, out Contact contact)
        {
            if (_rules.TryGetValue(new TypePair(first.GetType(), second.GetType()), out var rule))
                return rule(first, second, out contact);

            throw new NotSupportedException(
                $"No contact rule between {first.GetType().Name} and {second.GetType().Name}.");
        }

        private readonly struct TypePair : IEquatable<TypePair>
        {
            private readonly Type _first;
            private readonly Type _second;

            public TypePair(Type first, Type second)
            {
                _first = first;
                _second = second;
            }

            public bool Equals(TypePair other) => _first == other._first && _second == other._second;
            public override bool Equals(object obj) => obj is TypePair other && Equals(other);
            public override int GetHashCode() => unchecked(_first.GetHashCode() * 397) ^ _second.GetHashCode();
        }
    }
}
