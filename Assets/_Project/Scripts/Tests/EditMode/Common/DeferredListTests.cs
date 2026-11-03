using System;
using System.Collections.Generic;
using NUnit.Framework;

namespace Asteroids.Common.Tests
{
    [TestFixture]
    public sealed class DeferredListTests
    {
        [Test]
        public void Unlock_ACallbackThrows_LaterQueuedChangesAreStillAppliedAndTheFailureIsRethrown()
        {
            var removed = new List<string>();
            var list = new DeferredList<string>(onRemoved: item =>
            {
                removed.Add(item);
                if (item == "a") throw new InvalidOperationException("boom");
            });
            list.Add("a");
            list.Add("b");
            list.Lock();
            list.Remove("a");
            list.Remove("b");
            list.Add("c");

            Assert.Throws<InvalidOperationException>(() => list.Unlock());

            Assert.That(removed, Is.EqualTo(new[] { "a", "b" }));
            Assert.That(list.Items, Is.EqualTo(new[] { "c" }));
            Assert.That(list.IsLocked, Is.False);
        }

        [Test]
        public void AddAndRemove_WhileLocked_ApplyOnUnlockInCallOrder()
        {
            var list = new DeferredList<string>();
            list.Add("a");
            list.Lock();
            list.Remove("a");
            list.Add("b");

            Assert.That(list.Items, Is.EqualTo(new[] { "a" }));
            list.Unlock();
            Assert.That(list.Items, Is.EqualTo(new[] { "b" }));
        }
    }
}
