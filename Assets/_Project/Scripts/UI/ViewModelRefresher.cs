using System;
using System.Collections.Generic;
using Asteroids.Common;

namespace Asteroids.UI
{
    /// <summary>Refreshes every bound <see cref="IRefreshable"/>, in binding order.</summary>
    public sealed class ViewModelRefresher : IFrameTick
    {
        private readonly IRefreshable[] _refreshables;

        /// <exception cref="ArgumentNullException"><paramref name="refreshables"/> or an item is null.</exception>
        public ViewModelRefresher(List<IRefreshable> refreshables)
        {
            if (refreshables == null) throw new ArgumentNullException(nameof(refreshables));

            _refreshables = refreshables.ToArray();
            if (Array.IndexOf(_refreshables, null) >= 0)
                throw new ArgumentNullException(nameof(refreshables), "An item is null.");
        }

        public void Tick()
        {
            foreach (var refreshable in _refreshables) refreshable.Refresh();
        }
    }
}