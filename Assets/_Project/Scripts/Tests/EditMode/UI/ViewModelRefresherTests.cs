using System;
using System.Collections.Generic;
using NUnit.Framework;

namespace Asteroids.UI.Tests
{
    /// <summary>
    /// <see cref="ViewModelRefresher"/> moved from Zenject's <c>ILateTickable</c> to the frame schedule; its
    /// contract did not change: every refreshable, once per tick, in binding order.
    /// </summary>
    [TestFixture]
    public sealed class ViewModelRefresherTests
    {
        private sealed class Refreshable : IRefreshable
        {
            private readonly string _name;
            private readonly List<string> _log;

            public Refreshable(string name, List<string> log)
            {
                _name = name;
                _log = log;
            }

            public void Refresh() => _log.Add(_name);
        }

        [Test]
        public void Constructor_NullListOrItem_Throws()
        {
            Assert.Throws<ArgumentNullException>(() => new ViewModelRefresher(null));
            Assert.Throws<ArgumentNullException>(() => new ViewModelRefresher(new List<IRefreshable> { null }));
        }

        [Test]
        public void Tick_RefreshesEveryItemOnceInOrder()
        {
            var log = new List<string>();
            var refresher = new ViewModelRefresher(new List<IRefreshable>
            {
                new Refreshable("telemetry", log),
                new Refreshable("laser", log)
            });

            refresher.Tick();
            refresher.Tick();

            Assert.That(log, Is.EqualTo(new[] { "telemetry", "laser", "telemetry", "laser" }));
        }

        /// <summary>The list is copied: binding more later cannot change what an existing refresher ticks.</summary>
        [Test]
        public void Tick_UsesTheItemsGivenAtConstruction()
        {
            var log = new List<string>();
            var items = new List<IRefreshable> { new Refreshable("a", log) };
            var refresher = new ViewModelRefresher(items);

            items.Add(new Refreshable("b", log));
            refresher.Tick();

            Assert.That(log, Is.EqualTo(new[] { "a" }));
        }

        [Test]
        public void Tick_NothingBound_DoesNothing()
        {
            Assert.DoesNotThrow(() => new ViewModelRefresher(new List<IRefreshable>()).Tick());
        }
    }
}
