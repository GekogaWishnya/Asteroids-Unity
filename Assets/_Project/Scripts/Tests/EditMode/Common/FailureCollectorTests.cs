using System;
using NUnit.Framework;

namespace Asteroids.Common.Tests
{
    [TestFixture]
    public sealed class FailureCollectorTests
    {
        [Test]
        public void ThrowIfFailed_NothingRecorded_DoesNothing()
        {
            var failures = new FailureCollector();

            Assert.That(failures.HasFailed, Is.False);
            Assert.DoesNotThrow(() => failures.ThrowIfFailed());
        }

        [Test]
        public void ThrowIfFailed_SeveralRecorded_RethrowsTheFirst()
        {
            var failures = new FailureCollector();
            var first = new InvalidOperationException("first");
            failures.Record(first);
            failures.Record(new ArgumentException("second"));
            failures.Record(null);

            Assert.That(failures.HasFailed, Is.True);
            var thrown = Assert.Throws<InvalidOperationException>(() => failures.ThrowIfFailed());
            Assert.That(thrown, Is.SameAs(first));
        }
    }
}
