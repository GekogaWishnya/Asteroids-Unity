using System;
using Asteroids.Common;
using Asteroids.Core;

namespace Asteroids.Physics
{
    /// <summary>Shows every registered pose on its view once per frame.</summary>
    public sealed class PoseViewSync : IPoseViewRegistry, IFrameTick
    {
        private readonly DeferredList<Entry> _entries = new();

        /// <summary>Views that are currently following their poses.</summary>
        public int Count => _entries.Items.Count;

        public IDisposable Register(IPose pose, IPoseView view)
        {
            if (pose == null) throw new ArgumentNullException(nameof(pose));
            if (view == null) throw new ArgumentNullException(nameof(view));

            var entry = new Entry(pose, view);
            _entries.Add(entry);
            return new DisposableAction(() => _entries.Remove(entry));
        }

        /// <exception cref="InvalidOperationException">Called from inside a view while views are being shown.</exception>
        public void Tick()
        {
            _entries.Lock();
            var failures = new FailureCollector();
            try
            {
                var entries = _entries.Items;
                for (var i = 0; i < entries.Count; i++)
                {
                    var entry = entries[i];
                    try
                    {
                        entry.View.Show(entry.Pose.Position, entry.Pose.Angle);
                    }
                    catch (Exception e)
                    {
                        failures.Record(e);
                    }
                }
            }
            finally
            {
                _entries.Unlock();
            }

            failures.ThrowIfFailed();
        }

        private sealed class Entry
        {
            public Entry(IPose pose, IPoseView view)
            {
                Pose = pose;
                View = view;
            }

            public IPose Pose { get; }
            public IPoseView View { get; }
        }
    }
}