using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Asteroids.Common;

namespace Asteroids.Collisions
{
    public sealed class CollisionSystem : ICollisionSystem
    {
        private readonly HashSet<ICollider> _registered = new();
        private readonly DeferredList<ICollider> _colliders;
        private readonly List<DetectedContact> _detected = new();
        private readonly Dictionary<ColliderPair, int> _active = new();
        private readonly List<ColliderPair> _expired = new();
        /// <summary>Pool of available lists of <see cref="ColliderPair"/> for allocation used in <see cref="Detach"/>.</summary>
        private readonly Stack<List<ColliderPair>> _scratchLists = new();
        private readonly ICollisionEventSink _sink;
        private readonly IShapeContactTester _contacts;
        private readonly Dictionary<Type, int> _shapeTypeCounts = new();
        private int _stamp;

        public CollisionSystem(ICollisionEventSink sink, IShapeContactTester contacts)
        {
            _sink = sink ?? throw new ArgumentNullException(nameof(sink));
            _contacts = contacts ?? throw new ArgumentNullException(nameof(contacts));
            _colliders = new DeferredList<ICollider>(onRemoved: Detach);
        }

        public IDisposable Register(ICollider collider)
        {
            if (collider == null) throw new ArgumentNullException(nameof(collider));
            if (collider.Shape == null)
                throw new ArgumentException("Collider has no shape.", nameof(collider));
            if (_registered.Contains(collider))
                throw new InvalidOperationException("Collider is already registered.");

            var shapeType = collider.Shape.GetType();
            EnsureSupported(shapeType);

            _registered.Add(collider);
            _shapeTypeCounts[shapeType] = _shapeTypeCounts.TryGetValue(shapeType, out var count) ? count + 1 : 1;
            _colliders.Add(collider);
            
            return new Registration(this, collider);
        }

        public bool Unregister(ICollider collider)
        {
            if (collider == null) throw new ArgumentNullException(nameof(collider));
            if (!_registered.Remove(collider)) return false;

            ReleaseShapeType(collider.Shape.GetType());
            _colliders.Remove(collider);
            return true;
        }
        
        private void EnsureSupported(Type shapeType)
        {
            if (!_contacts.Supports(shapeType, shapeType))
                throw new NotSupportedException($"No contact rule between {shapeType.Name} and {shapeType.Name}.");

            foreach (var present in _shapeTypeCounts.Keys)
                if (!_contacts.Supports(shapeType, present))
                    throw new NotSupportedException(
                        $"No contact rule between {shapeType.Name} and {present.Name}.");
        }
        
        private void ReleaseShapeType(Type shapeType)
        {
            var left = _shapeTypeCounts[shapeType] - 1;
            if (left > 0) _shapeTypeCounts[shapeType] = left;
            else _shapeTypeCounts.Remove(shapeType);
        }
        
        public void Tick(float _)
        {
            if (_colliders.IsLocked)
                throw new InvalidOperationException($"{nameof(Tick)} can't be called during stepping.");

            _colliders.Lock();
            try
            {
                unchecked { _stamp++; }
                Detect();
                Dispatch();
                ExpireStalePairs();
            }
            finally
            {
                _detected.Clear();
                _colliders.Unlock();
            }
        }

        private void Detect()
        {
            _detected.Clear();
            var colliders = _colliders.Items;
            for (var i = 0; i < colliders.Count - 1; i++)
            {
                var first = colliders[i];
                for (var j = i + 1; j < colliders.Count; j++)
                {
                    var second = colliders[j];
                    if (CanInteract(first, second) && _contacts.TryGetContact(first.Shape, second.Shape, out var contact))
                        _detected.Add(new DetectedContact(first, second, contact));
                }
            }
        }

        private void Dispatch()
        {
            foreach (var detected in _detected)
            {
                var pair = new ColliderPair(detected.First, detected.Second);
                var isNew = !_active.ContainsKey(pair);
                _active[pair] = _stamp;

                var inverted = detected.Contact.Inverted;
                if (isNew)
                {
                    _sink.Entered(detected.First, detected.Second, detected.Contact);
                    _sink.Entered(detected.Second, detected.First, inverted);
                }
                else
                {
                    _sink.Stayed(detected.First, detected.Second, detected.Contact);
                    _sink.Stayed(detected.Second, detected.First, inverted);
                }
            }
        }

        private void ExpireStalePairs()
        {
            _expired.Clear();
            foreach (var entry in _active)
                if (entry.Value != _stamp) _expired.Add(entry.Key);

            foreach (var pair in _expired)
            {
                _active.Remove(pair);
                _sink.Exited(pair.First, pair.Second);
                _sink.Exited(pair.Second, pair.First);
            }
        }

        private static bool CanInteract(ICollider a, ICollider b) =>
            (a.Mask & b.Layer) != 0 && (b.Mask & a.Layer) != 0;

        /// <summary>Called by <see cref="_colliders"/> after a collider has been removed from the list.</summary>
        private void Detach(ICollider collider)
        {
            var ended = _scratchLists.Count > 0 ? _scratchLists.Pop() : new List<ColliderPair>();
            try
            {
                foreach (var entry in _active)
                    if (entry.Key.Contains(collider)) ended.Add(entry.Key);

                for (var i = 0; i < ended.Count; i++) _active.Remove(ended[i]);

                for (var i = 0; i < ended.Count; i++)
                {
                    var pair = ended[i];
                    _sink.Exited(pair.First, pair.Second);
                    _sink.Exited(pair.Second, pair.First);
                }
            }
            finally
            {
                ended.Clear();
                _scratchLists.Push(ended);
            }
        }

        private readonly struct DetectedContact
        {
            public DetectedContact(ICollider first, ICollider second, Contact contact)
            {
                First = first;
                Second = second;
                Contact = contact;
            }

            public ICollider First { get; }
            public ICollider Second { get; }
            public Contact Contact { get; }
        }

        /// <summary>Unordered pair of colliders.</summary>
        private readonly struct ColliderPair : IEquatable<ColliderPair>
        {
            public ColliderPair(ICollider first, ICollider second)
            {
                First = first;
                Second = second;
            }

            public ICollider First { get; }
            public ICollider Second { get; }

            public bool Contains(ICollider other) => ReferenceEquals(First, other) || ReferenceEquals(Second, other);

            public bool Equals(ColliderPair other) =>
                (ReferenceEquals(First, other.First) && ReferenceEquals(Second, other.Second)) ||
                (ReferenceEquals(First, other.Second) && ReferenceEquals(Second, other.First));

            public override bool Equals(object obj) => obj is ColliderPair other && Equals(other);

            public override int GetHashCode() =>
                RuntimeHelpers.GetHashCode(First) ^ RuntimeHelpers.GetHashCode(Second);
        }
        
        private sealed class Registration : IDisposable
        {
            private readonly CollisionSystem _owner;
            private readonly ICollider _collider;
            private bool _released;

            public Registration(CollisionSystem owner, ICollider collider)
            {
                _owner = owner;
                _collider = collider;
            }

            public void Dispose()
            {
                if (_released) return;
                _released = true;
                _owner.Unregister(_collider);
            }
        }
    }
}
