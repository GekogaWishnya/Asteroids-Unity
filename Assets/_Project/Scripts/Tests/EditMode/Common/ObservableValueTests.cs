using System;
using System.Collections.Generic;
using NUnit.Framework;

namespace Asteroids.Common.Tests
{
    [TestFixture]
    public sealed class ObservableValueTests
    {
        [Test]
        public void Constructor_WithoutArgument_ValueIsDefault()
        {
            Assert.That(new ObservableValue<int>().Value, Is.EqualTo(0));
            Assert.That(new ObservableValue<string>().Value, Is.Null);
        }

        [Test]
        public void Subscribe_ReplaysCurrentValueImmediately()
        {
            var value = new ObservableValue<int>(5);
            var seen = new List<int>();

            value.Subscribe(seen.Add);

            Assert.That(seen, Is.EqualTo(new[] { 5 }));
        }

        [Test]
        public void Set_NewValue_NotifiesSubscribersInSubscriptionOrder()
        {
            var value = new ObservableValue<int>(0);
            var log = new List<string>();
            value.Subscribe(v => log.Add($"first {v}"));
            value.Subscribe(v => log.Add($"second {v}"));
            log.Clear();

            value.Value = 1;

            Assert.That(log, Is.EqualTo(new[] { "first 1", "second 1" }));
            Assert.That(value.Value, Is.EqualTo(1));
        }

        [Test]
        public void Set_EqualValue_DoesNotNotify()
        {
            var value = new ObservableValue<int>(3);
            var seen = new List<int>();
            value.Subscribe(seen.Add);
            seen.Clear();

            value.Value = 3;

            Assert.That(seen, Is.Empty);
        }

        [Test]
        public void Dispose_StopsNotifications()
        {
            var value = new ObservableValue<int>(0);
            var seen = new List<int>();
            var subscription = value.Subscribe(seen.Add);
            seen.Clear();

            subscription.Dispose();
            value.Value = 1;

            Assert.That(seen, Is.Empty);
        }

        [Test]
        public void Dispose_Twice_DoesNotThrow()
        {
            var subscription = new ObservableValue<int>().Subscribe(_ => { });

            subscription.Dispose();

            Assert.DoesNotThrow(() => subscription.Dispose());
        }

        [Test]
        public void Subscribe_NullCallback_Throws()
        {
            Assert.Throws<ArgumentNullException>(() => new ObservableValue<int>().Subscribe(null));
        }

        [Test]
        public void Dispose_InsideCallback_SkipsThatSubscriberForRestOfPass()
        {
            var value = new ObservableValue<int>(0);
            var log = new List<string>();
            IDisposable second = null;
            value.Subscribe(v =>
            {
                log.Add($"first {v}");
                second?.Dispose();
            });
            second = value.Subscribe(v => log.Add($"second {v}"));
            log.Clear();

            value.Value = 1;

            Assert.That(log, Is.EqualTo(new[] { "first 1" }));
        }

        [Test]
        public void Subscribe_InsideCallback_ReplaysCurrentValueAndJoinsNextPass()
        {
            var value = new ObservableValue<int>(0);
            var innerSeen = new List<int>();
            var subscribed = false;
            value.Subscribe(v =>
            {
                if (v != 1 || subscribed) return;
                subscribed = true;
                value.Subscribe(innerSeen.Add);
            });

            value.Value = 1;
            Assert.That(innerSeen, Is.EqualTo(new[] { 1 }));

            value.Value = 2;
            Assert.That(innerSeen, Is.EqualTo(new[] { 1, 2 }));
        }

        [Test]
        public void Set_InsideCallback_FinishesCurrentPassThenDeliversLatestValue()
        {
            var value = new ObservableValue<int>(0);
            var seenByFirst = new List<int>();
            var seenBySecond = new List<int>();
            value.Subscribe(v =>
            {
                seenByFirst.Add(v);
                if (v == 1) value.Value = 2;
            });
            value.Subscribe(seenBySecond.Add);

            value.Value = 1;

            Assert.That(seenByFirst, Is.EqualTo(new[] { 0, 1, 2 }));
            Assert.That(seenBySecond, Is.EqualTo(new[] { 0, 1, 2 }));
        }

        [Test]
        public void Subscribe_ReplayThrows_SubscriptionIsNotKept()
        {
            var value = new ObservableValue<int>(0);

            Assert.Throws<InvalidOperationException>(
                () => value.Subscribe(_ => throw new InvalidOperationException()));

            Assert.DoesNotThrow(() => value.Value = 1);
        }

        [Test]
        public void Set_CallbackThrows_ValueStaysChangedAndExceptionPropagates()
        {
            var value = new ObservableValue<int>(0);
            value.Subscribe(v =>
            {
                if (v == 1) throw new InvalidOperationException();
            });

            Assert.Throws<InvalidOperationException>(() => value.Value = 1);
            Assert.That(value.Value, Is.EqualTo(1));
        }

        [Test]
        public void Set_AfterCallbackThrew_NotifiesNormally()
        {
            var value = new ObservableValue<int>(0);
            var seen = new List<int>();
            value.Subscribe(v =>
            {
                if (v == 1) throw new InvalidOperationException();
                seen.Add(v);
            });
            Assert.Throws<InvalidOperationException>(() => value.Value = 1);
            seen.Clear();

            value.Value = 2;

            Assert.That(seen, Is.EqualTo(new[] { 2 }));
        }
    }
}
