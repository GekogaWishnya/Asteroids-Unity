using System;
using System.Collections.Generic;
using NUnit.Framework;

namespace Asteroids.Common.Tests
{
    [TestFixture]
    public sealed class RegistrationScopeTests
    {
        private List<string> _log;

        [SetUp]
        public void SetUp() => _log = new List<string>();

        private Func<IDisposable> Factory(string name, Exception disposeFailure = null) => () =>
        {
            _log.Add($"create {name}");
            return new Registration(() =>
            {
                _log.Add($"dispose {name}");
                if (disposeFailure != null) throw disposeFailure;
            });
        };

        [Test]
        public void Enable_Twice_RunsFactoriesOnce()
        {
            var scope = new RegistrationScope(new[] { Factory("a"), Factory("b") });

            scope.Enable();
            scope.Enable();

            Assert.That(scope.IsEnabled, Is.True);
            Assert.That(_log, Is.EqualTo(new[] { "create a", "create b" }));
        }

        [Test]
        public void Disable_WithoutEnable_DoesNothing()
        {
            var scope = new RegistrationScope(new[] { Factory("a") });

            scope.Disable();

            Assert.That(scope.IsEnabled, Is.False);
            Assert.That(_log, Is.Empty);
        }

        [Test]
        public void Disable_DisposesInReverseOrder()
        {
            var scope = new RegistrationScope(new[] { Factory("a"), Factory("b"), Factory("c") });
            scope.Enable();
            _log.Clear();

            scope.Disable();
            scope.Disable();

            Assert.That(scope.IsEnabled, Is.False);
            Assert.That(_log, Is.EqualTo(new[] { "dispose c", "dispose b", "dispose a" }));
        }

        [Test]
        public void EnableDisableEnable_RunsFactoriesAgain()
        {
            var scope = new RegistrationScope(new[] { Factory("a") });

            scope.Enable();
            scope.Disable();
            scope.Enable();

            Assert.That(scope.IsEnabled, Is.True);
            Assert.That(_log, Is.EqualTo(new[] { "create a", "dispose a", "create a" }));
        }

        [Test]
        public void Enable_WhenSecondFactoryThrows_DisposesFirstAndStaysDisabled()
        {
            var failure = new InvalidOperationException("boom");
            var scope = new RegistrationScope(new Func<IDisposable>[]
            {
                Factory("a"),
                () => throw failure,
                Factory("c")
            });

            var thrown = Assert.Throws<InvalidOperationException>(() => scope.Enable());

            Assert.That(thrown, Is.SameAs(failure));
            Assert.That(scope.IsEnabled, Is.False);
            Assert.That(_log, Is.EqualTo(new[] { "create a", "dispose a" }));
        }

        [Test]
        public void Enable_WhenFactoryReturnsNull_ThrowsAndStaysDisabled()
        {
            var scope = new RegistrationScope(new Func<IDisposable>[] { Factory("a"), () => null });

            Assert.Throws<InvalidOperationException>(() => scope.Enable());

            Assert.That(scope.IsEnabled, Is.False);
            Assert.That(_log, Is.EqualTo(new[] { "create a", "dispose a" }));
        }

        [Test]
        public void Enable_AfterFailedEnable_CanSucceed()
        {
            var shouldFail = true;
            var scope = new RegistrationScope(new Func<IDisposable>[]
            {
                Factory("a"),
                () => shouldFail ? throw new InvalidOperationException() : Factory("b")()
            });
            Assert.Throws<InvalidOperationException>(() => scope.Enable());
            shouldFail = false;

            scope.Enable();

            Assert.That(scope.IsEnabled, Is.True);
        }

        [Test]
        public void Disable_WhenOneDisposeThrows_StillDisposesTheRest()
        {
            var failure = new InvalidOperationException("dispose failed");
            var scope = new RegistrationScope(new[] { Factory("a"), Factory("b", failure), Factory("c") });
            scope.Enable();
            _log.Clear();

            var thrown = Assert.Throws<InvalidOperationException>(() => scope.Disable());

            Assert.That(thrown, Is.SameAs(failure));
            Assert.That(scope.IsEnabled, Is.False);
            Assert.That(_log, Is.EqualTo(new[] { "dispose c", "dispose b", "dispose a" }));
        }

        [Test]
        public void Dispose_WhileEnabled_Disables()
        {
            var scope = new RegistrationScope(new[] { Factory("a") });
            scope.Enable();

            scope.Dispose();

            Assert.That(scope.IsEnabled, Is.False);
            Assert.That(_log, Is.EqualTo(new[] { "create a", "dispose a" }));
        }

        [Test]
        public void Constructor_NullFactories_ThrowsArgumentNull()
        {
            Assert.Throws<ArgumentNullException>(() => new RegistrationScope(null));
            Assert.Throws<ArgumentNullException>(() => new RegistrationScope(new Func<IDisposable>[] { null }));
        }

        private sealed class Registration : IDisposable
        {
            private readonly Action _onDispose;
            public Registration(Action onDispose) => _onDispose = onDispose;
            public void Dispose() => _onDispose();
        }
    }
}
