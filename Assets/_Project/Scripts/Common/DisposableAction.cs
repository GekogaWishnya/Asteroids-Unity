using System;

namespace Asteroids.Common
{
    /// <summary>Runs an action on the first <see cref="Dispose"/> and ignores every later call.</summary>
    public sealed class DisposableAction : IDisposable
    {
        private Action _release;

        /// <exception cref="ArgumentNullException"><paramref name="release"/> is null.</exception>
        public DisposableAction(Action release) =>
            _release = release ?? throw new ArgumentNullException(nameof(release));

        public bool IsDisposed => _release == null;
        
        public void Dispose()
        {
            var release = _release;
            if (release == null) return;

            _release = null;
            release();
        }
    }
}
