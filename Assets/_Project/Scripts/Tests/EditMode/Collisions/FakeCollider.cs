using System.Collections.Generic;
using Asteroids.Core.IConfigs;

namespace Asteroids.Collisions.Tests
{
    /// <summary>Shape of <see cref="FakeCollider"/>. Its pair rule is <see cref="FakeShape.CreateContacts"/>.</summary>
    internal sealed class FakeShape : IShape
    {
        public FakeShape(FakeCollider collider) => Collider = collider;

        public FakeCollider Collider { get; }
        public bool Overlaps { get; set; }
        public Contact ContactToReturn { get; set; } = new(UnityEngine.Vector2.up, 1f);

        /// <summary>
        /// Contact table that knows only <see cref="FakeShape"/> against itself. The first shape of the pair decides.
        /// </summary>
        public static ShapeContactTable CreateContacts() =>
            new ShapeContactTable().Add<FakeShape, FakeShape>(Test);

        private static bool Test(FakeShape first, FakeShape second, out Contact contact)
        {
            first.Collider.ContactQueries.Add(second.Collider);
            contact = first.Overlaps ? first.ContactToReturn : default;
            return first.Overlaps;
        }
    }

    /// <summary>A shape the fake contact table has no rule for.</summary>
    internal sealed class UnknownShape : IShape
    {
    }
    
    internal sealed class FakeCollider : ICollider
    {
        private readonly FakeShape _shape;
        
        /// <param name="name">Shown in assertion messages.</param>
        public FakeCollider(string name)
        {
            Name = name;
            Owner = name;
            Layer = CollisionLayer.All;
            Mask = CollisionLayer.All;
            _shape = new FakeShape(this);
        }

        public string Name { get; }
        public object Owner { get; }
        public CollisionLayer Layer { get; set; }
        public CollisionLayer Mask { get; set; }
        public IShape Shape => _shape;

        /// <summary>When true, the pair rule reports <see cref="ContactToReturn"/> for pairs led by this collider.</summary>
        public bool Overlaps
        {
            get => _shape.Overlaps;
            set => _shape.Overlaps = value;
        }

        public Contact ContactToReturn
        {
            get => _shape.ContactToReturn;
            set => _shape.ContactToReturn = value;
        }

        /// <summary>Colliders this one was asked to test against, in call order.</summary>
        public List<ICollider> ContactQueries { get; } = new();

        public override string ToString() => Name;
    }
}
