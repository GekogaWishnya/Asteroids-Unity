using System;
using System.Collections.Generic;

namespace Asteroids.Common
{
    /// <summary>
    /// A dictionary whose entries are owned by the handles <see cref="Add"/> returns.
    /// Disposing a handle removes its own entry.
    /// </summary>
    public sealed class RegistrationMap<TKey, TValue> where TKey : class where TValue : class
    {
        private readonly Dictionary<TKey, TValue> _entries = new();
        private readonly string _role;

        /// <param name="role">What a value is, for error messages, such as "kinetic body".</param>
        /// <exception cref="ArgumentException"><paramref name="role"/> is null or empty.</exception>
        public RegistrationMap(string role)
        {
            if (string.IsNullOrEmpty(role)) throw new ArgumentException("A role is required.", nameof(role));
            _role = role;
        }

        public int Count => _entries.Count;

        /// <param name="keyName">Parameter name reported when <paramref name="key"/> is null.</param>
        /// <param name="valueName">Parameter name reported when <paramref name="value"/> is null.</param>
        /// <returns>Dispose to remove the entry.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="key"/> or <paramref name="value"/> is null.</exception>
        /// <exception cref="InvalidOperationException">The key is already registered.</exception>
        public IDisposable Add(TKey key, TValue value, string keyName = "key", string valueName = "value")
        {
            if (key == null) throw new ArgumentNullException(keyName);
            if (value == null) throw new ArgumentNullException(valueName);

            if (!_entries.TryAdd(key, value))
                throw new InvalidOperationException($"The entity is already registered as a {_role}.");

            return new DisposableAction(() => Remove(key, value));
        }
        
        public bool TryGet(TKey key, out TValue value)
        {
            if (key != null) return _entries.TryGetValue(key, out value);

            value = null;
            return false;
        }

        private void Remove(TKey key, TValue value)
        {
            if (_entries.TryGetValue(key, out var current) && ReferenceEquals(current, value))
                _entries.Remove(key);
        }
    }
}
