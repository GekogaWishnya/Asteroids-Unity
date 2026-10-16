using System;
using System.Collections.Generic;
using System.Runtime.ExceptionServices;

namespace Asteroids.Common
{
    /// <summary>
    /// <para>
    /// Owns a fixed set of registrations that are live while the scope is enabled.
    /// </para>
    /// <para>
    /// <see cref="Enable"/> runs the factories in the order given while <see cref="Disable"/> disposes
    /// the created registrations in the reverse order.
    /// </para>
    /// <para>
    /// If a factory throws during <see cref="Enable"/>, the registrations created so far
    /// are disposed in reverse order, the failure propagates and the scope stays disabled. Exceptions thrown by
    /// those cleanup disposals are hidden so that they cannot mask the original failure.
    /// If a registration throws on dispose during <see cref="Disable"/>, the remaining ones are still disposed,
    /// the scope ends disabled and the first exception is rethrown after all disposals while later ones are dropped.
    /// </para>
    /// <para>
    /// Calling <see cref="Enable"/> or <see cref="Disable"/> from inside a factory or dispose
    /// is not supported. Not thread-safe.
    /// </para>
    /// </summary>
    public sealed class RegistrationScope : IDisposable
    {
        private readonly Func<IDisposable>[] _factories;
        private readonly List<IDisposable> _live;

        /// <param name="factories">Registration factories. Copied.</param>
        /// <exception cref="ArgumentNullException"><paramref name="factories"/> or one of its items is null.</exception>
        public RegistrationScope(IEnumerable<Func<IDisposable>> factories)
        {
            if (factories == null) throw new ArgumentNullException(nameof(factories));

            _factories = new List<Func<IDisposable>>(factories).ToArray();
            foreach (var factory in _factories)
            {
                if (factory == null)
                    throw new ArgumentNullException(nameof(factories), "A registration factory must not be null.");
            }

            _live = new List<IDisposable>(_factories.Length);
        }

        public bool IsEnabled { get; private set; }

        /// <summary>Creates all registrations. Exits if already enabled.</summary>
        public void Enable()
        {
            if (IsEnabled) return;

            try
            {
                foreach (var factory in _factories)
                {
                    var registration = factory() 
                                       ?? throw new InvalidOperationException(
                                           "A registration factory returned null.");
                    _live.Add(registration);
                }
            }
            catch
            {
                DisposeAll(out _);
                throw;
            }

            IsEnabled = true;
        }

        /// <summary>Disposes all registrations in reverse order. Exits if already disabled.</summary>
        /// <exception cref="Exception">The first exception thrown by a registration's dispose, after all were disposed.</exception>
        public void Disable()
        {
            if (!IsEnabled) return;

            IsEnabled = false;
            DisposeAll(out var firstFailure);
            if (firstFailure != null) ExceptionDispatchInfo.Capture(firstFailure).Throw();
        }
        
        public void Dispose() => Disable();

        private void DisposeAll(out Exception firstFailure)
        {
            firstFailure = null;

            for (var i = _live.Count - 1; i >= 0; i--)
            {
                try
                {
                    _live[i].Dispose();
                }
                catch (Exception e)
                {
                    firstFailure ??= e;
                }
            }

            _live.Clear();
        }
    }
}
