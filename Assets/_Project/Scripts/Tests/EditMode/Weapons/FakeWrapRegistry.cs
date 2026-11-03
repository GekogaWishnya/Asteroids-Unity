using System;
using System.Collections.Generic;
using Asteroids.Core;

namespace Asteroids.Weapons.Tests
{
    /// <summary>Records which poses wrap. Disposing a handle removes its pose.</summary>
    internal sealed class FakeWrapRegistry : IWrapRegistry
    {
        private readonly List<IPose> _poses = new();

        public IReadOnlyList<IPose> Poses => _poses;

        public IDisposable Register(IPose pose)
        {
            if (pose == null) throw new ArgumentNullException(nameof(pose));
            _poses.Add(pose);
            return new Handle(_poses, pose);
        }

        private sealed class Handle : IDisposable
        {
            private readonly List<IPose> _poses;
            private readonly IPose _pose;
            private bool _disposed;

            public Handle(List<IPose> poses, IPose pose)
            {
                _poses = poses;
                _pose = pose;
            }

            public void Dispose()
            {
                if (_disposed) return;
                _disposed = true;
                _poses.Remove(_pose);
            }
        }
    }
}
