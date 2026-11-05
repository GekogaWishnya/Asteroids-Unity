using System;
using System.Collections.Generic;
using Asteroids.Core;

namespace Asteroids.Player.Tests
{
    /// <summary>Records which poses wrap. Disposing a handle removes its pose.</summary>
    internal sealed class FakeWrapRegistry : IWrapRegistry
    {
        private readonly List<IPose> _poses = new();
        private readonly List<IPose> _entering = new();

        public IReadOnlyList<IPose> Poses => _poses;

        /// <summary>The poses of <see cref="Poses"/> that were registered as entering.</summary>
        public IReadOnlyList<IPose> Entering => _entering;

        public IDisposable Register(IPose pose)
        {
            if (pose == null) throw new ArgumentNullException(nameof(pose));
            _poses.Add(pose);
            return new Handle(_poses, pose);
        }

        public IDisposable RegisterEntering(IPose pose)
        {
            var handle = Register(pose);
            _entering.Add(pose);
            return new Handle(_entering, pose, handle);
        }

        private sealed class Handle : IDisposable
        {
            private readonly List<IPose> _poses;
            private readonly IPose _pose;
            private readonly IDisposable _inner;
            private bool _disposed;

            public Handle(List<IPose> poses, IPose pose, IDisposable inner = null)
            {
                _poses = poses;
                _pose = pose;
                _inner = inner;
            }

            public void Dispose()
            {
                if (_disposed) return;
                _disposed = true;
                _poses.Remove(_pose);
                _inner?.Dispose();
            }
        }
    }
}
