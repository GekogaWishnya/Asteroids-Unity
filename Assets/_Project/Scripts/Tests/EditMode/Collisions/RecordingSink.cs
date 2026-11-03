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
        public Notification(NotificationKind kind, ICollider first, ICollider second, Contact contact)
        {
            Kind = kind;
            First = first;
            Second = second;
            Contact = contact;
        }

        public NotificationKind Kind { get; }
        public ICollider First { get; }
        public ICollider Second { get; }
        /// <summary>Seen from <see cref="First"/>.</summary>
        public Contact Contact { get; }

        public bool Involves(ICollider collider) =>
            ReferenceEquals(First, collider) || ReferenceEquals(Second, collider);

        /// <summary>The other collider of the pair, seen from <paramref name="collider"/>.</summary>
        public ICollider OtherThan(ICollider collider) => ReferenceEquals(First, collider) ? Second : First;

        /// <summary>The contact seen from <paramref name="collider"/>.</summary>
        public Contact ContactFor(ICollider collider) => ReferenceEquals(First, collider) ? Contact : Contact.Inverted;
    }
    
    internal sealed class RecordingSink : ICollisionEventSink
    {
        public List<Notification> Notifications { get; } = new();
        
        private readonly Dictionary<ICollider, Action<Notification>> _handlers = new();
        /// <summary>Runs on every notification that involves the collider, on either side.</summary>
        public Action<Notification> this[ICollider collider]
        {
            set
            {
                if (value == null) _handlers.Remove(collider);
                else _handlers[collider] = value;
            }
        }
        
        /// <summary>Notifications that involve the collider, on either side.</summary>
        public List<Notification> For(ICollider collider)
        {
            var result = new List<Notification>();
            foreach (var notification in Notifications)
                if (notification.Involves(collider)) result.Add(notification);
            return result;
        }

        public void Entered(ICollider first, ICollider second, in Contact contact) =>
            Record(new Notification(NotificationKind.Entered, first, second, contact));

        public void Stayed(ICollider first, ICollider second, in Contact contact) =>
            Record(new Notification(NotificationKind.Stayed, first, second, contact));

        public void Exited(ICollider first, ICollider second) =>
            Record(new Notification(NotificationKind.Exited, first, second, default));

        private void Record(Notification notification)
        {
            Notifications.Add(notification);
            if (_handlers.TryGetValue(notification.First, out var handler)) handler(notification);
            if (!ReferenceEquals(notification.First, notification.Second) &&
                _handlers.TryGetValue(notification.Second, out handler)) handler(notification);
        }
    }
}
