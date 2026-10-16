using System;
using System.Collections.Generic;

namespace Asteroids.Collisions.Tests
{
    internal enum NotificationKind
    {
        Entered,
        Stayed,
        Exited
    }
    
    internal readonly struct Notification
    {
        public Notification(NotificationKind kind, ICollider self, ICollider other, Contact contact)
        {
            Kind = kind;
            Self = self;
            Other = other;
            Contact = contact;
        }

        public NotificationKind Kind { get; }
        public ICollider Self { get; }
        public ICollider Other { get; }
        public Contact Contact { get; }
    }
    
    internal sealed class FakeCollider : ICollider
    {
        private readonly List<Notification> _journal;

        /// <param name="name">Shown in assertion messages.</param>
        /// <param name="journal">Optional list shared between fakes to check cross-collider ordering.</param>
        public FakeCollider(string name, List<Notification> journal = null)
        {
            Name = name;
            _journal = journal;
            Owner = name;
            Layer = CollisionLayer.All;
            Mask = CollisionLayer.All;
        }

        public string Name { get; }
        public object Owner { get; }
        public CollisionLayer Layer { get; set; }
        public CollisionLayer Mask { get; set; }

        /// <summary>When true, <see cref="TryGetContact"/> reports <see cref="ContactToReturn"/>.</summary>
        public bool Overlaps { get; set; }
        public Contact ContactToReturn { get; set; } = new(UnityEngine.Vector2.up, 1f);

        /// <summary>Colliders this one was asked to test against, in call order.</summary>
        public List<ICollider> ContactQueries { get; } = new();
        public List<Notification> Notifications { get; } = new();

        /// <summary>Invoked after each notification is recorded.</summary>
        public Action<Notification> OnNotified { get; set; }

        public bool TryGetContact(ICollider other, out Contact contact)
        {
            ContactQueries.Add(other);
            contact = Overlaps ? ContactToReturn : default;
            return Overlaps;
        }

        public void NotifyEntered(ICollider other, in Contact contact) =>
            Record(new Notification(NotificationKind.Entered, this, other, contact));

        public void NotifyStayed(ICollider other, in Contact contact) =>
            Record(new Notification(NotificationKind.Stayed, this, other, contact));

        public void NotifyExited(ICollider other) =>
            Record(new Notification(NotificationKind.Exited, this, other, default));

        private void Record(Notification notification)
        {
            Notifications.Add(notification);
            _journal?.Add(notification);
            OnNotified?.Invoke(notification);
        }

        public override string ToString() => Name;
    }
}
