using System;
using System.Collections.Generic;
using Zenject;

namespace Asteroids.UI
{
    /// <summary>
    /// Refreshes every bound <see cref="IRefreshable"/> in <c>LateUpdate</c>, after the simulation has ticked
    /// in <c>Update</c>, so the HUD shows the state of the frame being drawn.
    /// </summary>
    public sealed class ViewModelRefresher : ILateTickable
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

        public void LateTick()
        {
            foreach (var refreshable in _refreshables) refreshable.Refresh();
        }
    }
}
