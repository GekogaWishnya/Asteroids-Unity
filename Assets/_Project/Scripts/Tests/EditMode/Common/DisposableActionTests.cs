using System;
using NUnit.Framework;

namespace Asteroids.Common.Tests
{
    [TestFixture]
    public sealed class DisposableActionTests
    {
        [Test]
        public void Constructor_NullAction_Throws()
        {
            Assert.Throws<ArgumentNullException>(() => new DisposableAction(null));
        }

        [Test]
        public void Dispose_RunsTheActionOnce()
        {
            var calls = 0;
            var handle = new DisposableAction(() => calls++);

            handle.Dispose();
            handle.Dispose();

            Assert.That(calls, Is.EqualTo(1));
            Assert.That(handle.IsDisposed, Is.True);
        }

        [Test]
        public void Dispose_ActionThrows_IsNotRunAgain()
        {
            var calls = 0;
            var handle = new DisposableAction(() =>
            {
                calls++;
                throw new InvalidOperationException();
            });

            Assert.Throws<InvalidOperationException>(() => handle.Dispose());
            Assert.DoesNotThrow(() => handle.Dispose());
            Assert.That(calls, Is.EqualTo(1));
        }
    }
}
