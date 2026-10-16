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
    
    internal sealed class RecordingSink : ICollisionEventSink
    {
        public List<Notification> Notifications { get; } = new();
        
        private readonly Dictionary<ICollider, Action<Notification>> _handlers = new();
        public Action<Notification> this[ICollider self]
        {
            set
            {
                if (value == null) _handlers.Remove(self);
                else _handlers[self] = value;
            }
        }
        
        public List<Notification> For(ICollider self)
        {
            var result = new List<Notification>();
            foreach (var notification in Notifications)
                if (ReferenceEquals(notification.Self, self)) result.Add(notification);
            return result;
        }

        public void Entered(ICollider self, ICollider other, in Contact contact) =>
            Record(new Notification(NotificationKind.Entered, self, other, contact));

        public void Stayed(ICollider self, ICollider other, in Contact contact) =>
            Record(new Notification(NotificationKind.Stayed, self, other, contact));

        public void Exited(ICollider self, ICollider other) =>
            Record(new Notification(NotificationKind.Exited, self, other, default));

        private void Record(Notification notification)
        {
            Notifications.Add(notification);
            if (_handlers.TryGetValue(notification.Self, out var handler)) handler(notification);
        }
    }
}
