using System;
using System.Collections.Generic;

namespace Asteroids.Common
{
    /// <summary>
    /// While <see cref="IsLocked"/>, <see cref="Add"/> and <see cref="Remove"/> are queued and
    /// applied in call order by <see cref="Unlock"/>. When not locked they apply immediately.
    /// Changes queued by the callbacks that run during <see cref="Unlock"/> are applied in the same pass.
    /// </summary>
    public sealed class DeferredList<T>
    {
        private readonly List<T> _items = new();
        private readonly List<Change> _pending = new();
        private readonly Action<T> _onAdded;
        private readonly Action<T> _onRemoved;
        
        public DeferredList(Action<T> onAdded = null, Action<T> onRemoved = null)
        {
            _onAdded = onAdded;
            _onRemoved = onRemoved;
        }

        /// <summary>Applied items only. Safe to iterate by index while locked, because it cannot change.</summary>
        public IReadOnlyList<T> Items => _items;

        public bool IsLocked { get; private set; }

        public void Add(T item)
        {
            if (IsLocked) _pending.Add(new Change(item, isAdd: true));
            else ApplyAdd(item);
        }

        public void Remove(T item)
        {
            if (IsLocked) _pending.Add(new Change(item, isAdd: false));
            else ApplyRemove(item);
        }

        /// <exception cref="InvalidOperationException">Already locked.</exception>
        public void Lock()
        {
            if (IsLocked) throw new InvalidOperationException($"{nameof(DeferredList<T>)} is already locked.");
            IsLocked = true;
        }

        /// <summary>Applies queued changes, then unlocks. The list is unlocked even if a callback throws.</summary>
        /// <exception cref="InvalidOperationException">Not locked.</exception>
        public void Unlock()
        {
            if (!IsLocked) throw new InvalidOperationException($"{nameof(DeferredList<T>)} is not locked.");

            try
            {
                // Index loop on purpose - callbacks fired below may queue further changes.
                for (var i = 0; i < _pending.Count; i++)
                {
                    var change = _pending[i];
                    if (change.IsAdd) ApplyAdd(change.Item);
                    else ApplyRemove(change.Item);
                }
            }
            finally
            {
                _pending.Clear();
                IsLocked = false;
            }
        }

        private void ApplyAdd(T item)
        {
            _items.Add(item);
            _onAdded?.Invoke(item);
        }

        private void ApplyRemove(T item)
        {
            if (!_items.Remove(item)) return;
            _onRemoved?.Invoke(item);
        }

        private readonly struct Change
        {
            public Change(T item, bool isAdd)
            {
                Item = item;
                IsAdd = isAdd;
            }

            public T Item { get; }
            public bool IsAdd { get; }
        }
    }
}
