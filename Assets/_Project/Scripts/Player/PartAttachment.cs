using System;
using System.Collections.Generic;
using Asteroids.Common;

namespace Asteroids.Player
{
    /// <summary>
    /// Allows one live attachment per part and owns the registrations of the live one.
    /// </summary>
    internal sealed class PartAttachment
    {
        private readonly string _partName;
        private Handle _live;

        public PartAttachment(string partName) => _partName = partName;
        
        /// <exception cref="InvalidOperationException">An attachment is still live.</exception>
        public IDisposable Start(IEnumerable<Func<IDisposable>> registrations)
        {
            if (_live != null) throw new InvalidOperationException($"The {_partName} is already attached.");

            var scope = new RegistrationScope(registrations);
            scope.Enable();
            return _live = new Handle(this, scope);
        }

        private sealed class Handle : IDisposable
        {
            private readonly PartAttachment _owner;
            private readonly RegistrationScope _scope;
            private bool _disposed;

            public Handle(PartAttachment owner, RegistrationScope scope)
            {
                _owner = owner;
                _scope = scope;
            }

            public void Dispose()
            {
                if (_disposed) return;
                _disposed = true;

                try
                {
                    _scope.Dispose();
                }
                finally
                {
                    if (ReferenceEquals(_owner._live, this)) _owner._live = null;
                }
            }
        }
    }
}
