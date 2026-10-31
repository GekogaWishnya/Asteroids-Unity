using System;
using System.Linq;

namespace Asteroids.Collisions
{
    public sealed class CompositeCollisionEventSink : ICollisionEventSink
    {
        private readonly ICollisionEventSink[] _sinks;

        /// <exception cref="ArgumentNullException"><paramref name="sinks"/> is null.</exception>
        /// <exception cref="ArgumentException"><paramref name="sinks"/> contains null.</exception>
        public CompositeCollisionEventSink(params ICollisionEventSink[] sinks)
        {
            if (sinks == null) throw new ArgumentNullException(nameof(sinks));

            _sinks = sinks.ToArray();
            if (Array.IndexOf(_sinks, null) >= 0)
                throw new ArgumentException("Sinks must not contain null.", nameof(sinks));
        }

        public void Entered(ICollider self, ICollider other, in Contact contact)
        {
            foreach (var sink in _sinks) sink.Entered(self, other, contact);
        }

        public void Stayed(ICollider self, ICollider other, in Contact contact)
        {
            foreach (var sink in _sinks) sink.Stayed(self, other, contact);
        }

        public void Exited(ICollider self, ICollider other)
        {
            foreach (var sink in _sinks) sink.Exited(self, other);
        }
    }
}
