using System;
using System.Linq;
using Asteroids.Common;

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

        public void Entered(ICollider first, ICollider second, in Contact contact)
        {
            var failures = new FailureCollector();
            foreach (var sink in _sinks)
            {
                try
                {
                    sink.Entered(first, second, contact);
                }
                catch (Exception e)
                {
                    failures.Record(e);
                }
            }

            failures.ThrowIfFailed();
        }

        public void Stayed(ICollider first, ICollider second, in Contact contact)
        {
            var failures = new FailureCollector();
            foreach (var sink in _sinks)
            {
                try
                {
                    sink.Stayed(first, second, contact);
                }
                catch (Exception e)
                {
                    failures.Record(e);
                }
            }

            failures.ThrowIfFailed();
        }

        public void Exited(ICollider first, ICollider second)
        {
            var failures = new FailureCollector();
            foreach (var sink in _sinks)
            {
                try
                {
                    sink.Exited(first, second);
                }
                catch (Exception e)
                {
                    failures.Record(e);
                }
            }

            failures.ThrowIfFailed();
        }
    }
}
