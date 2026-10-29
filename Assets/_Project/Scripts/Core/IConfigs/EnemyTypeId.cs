using System;

namespace Asteroids.Core.IConfigs
{
    /// <summary>
    /// Identity of an enemy kind, as levels and spawners refer to it.
    /// <c>default</c> is the "no type" value and is not <see cref="IsValid"/>.
    /// </summary>
    public readonly struct EnemyTypeId : IEquatable<EnemyTypeId>
    {
        /// <exception cref="ArgumentException"><paramref name="value"/> is null, empty or white space.</exception>
        public EnemyTypeId(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                throw new ArgumentException("The id must not be empty or white space.", nameof(value));

            Value = value;
        }

        /// <summary>Null for <c>default</c>.</summary>
        public string Value { get; }

        public bool IsValid => Value != null;

        public bool Equals(EnemyTypeId other) => string.Equals(Value, other.Value, StringComparison.Ordinal);

        public override bool Equals(object obj) => obj is EnemyTypeId other && Equals(other);

        public override int GetHashCode() => Value == null ? 0 : StringComparer.Ordinal.GetHashCode(Value);

        public override string ToString() => Value ?? "<none>";

        public static bool operator ==(EnemyTypeId left, EnemyTypeId right) => left.Equals(right);

        public static bool operator !=(EnemyTypeId left, EnemyTypeId right) => !left.Equals(right);
    }
}
