using System;
using NUnit.Framework;

namespace Asteroids.Common.Tests
{
    [TestFixture]
    public sealed class UniqueOrderTests
    {
        private enum Place { First = 0, Second = 10, Third = 20 }

        private sealed class Item
        {
            public Item(Place place) => Place = place;
            public Place Place { get; }
        }

        [Test]
        public void Sort_OrdersByTheDeclaredPlace()
        {
            var third = new Item(Place.Third);
            var first = new Item(Place.First);
            var second = new Item(Place.Second);

            var sorted = UniqueOrder.Sort(new[] { third, first, second }, item => item.Place, "item");

            Assert.That(sorted, Is.EqualTo(new[] { first, second, third }));
        }

        [Test]
        public void Sort_TwoItemsInTheSamePlace_ThrowsNamingThePlace()
        {
            var items = new[] { new Item(Place.Second), new Item(Place.First), new Item(Place.Second) };

            var error = Assert.Throws<ArgumentException>(() => UniqueOrder.Sort(items, item => item.Place, "item"));

            StringAssert.Contains("Second", error.Message);
            StringAssert.Contains("Each item needs its own Place value", error.Message);
        }

        [Test]
        public void Sort_Empty_ReturnsEmpty() =>
            Assert.That(UniqueOrder.Sort(Array.Empty<Item>(), item => item.Place, "item"), Is.Empty);

        [Test]
        public void Sort_NullArguments_Throw()
        {
            Assert.Throws<ArgumentNullException>(() => UniqueOrder.Sort<Item, Place>(null, item => item.Place, "item"));
            Assert.Throws<ArgumentNullException>(() => UniqueOrder.Sort<Item, Place>(new Item[0], null, "item"));
            Assert.Throws<ArgumentException>(
                () => UniqueOrder.Sort(new[] { new Item(Place.First), null }, item => item.Place, "item"));
        }

        [Test]
        public void Sort_DoesNotChangeTheInput()
        {
            var items = new[] { new Item(Place.Third), new Item(Place.First) };
            var copy = (Item[])items.Clone();

            UniqueOrder.Sort(items, item => item.Place, "item");

            Assert.That(items, Is.EqualTo(copy));
        }
    }
}
