using System;
using System.Collections.Generic;
using Asteroids.Core;

namespace Asteroids.Player.Tests
{
    /// <summary>Records attachments and whether they are still live.</summary>
    internal sealed class FakeWeapon : IWeapon
    {
        private readonly List<Attachment> _attachments = new();
        public IReadOnlyList<Attachment> Attachments => _attachments;

        public int LiveCount
        {
            get
            {
                var count = 0;
                foreach (var attachment in _attachments)
                    if (!attachment.IsDisposed) count++;
                return count;
            }
        }

        public IDisposable Attach(IEntity carrier, IFireInput trigger)
        {
            var attachment = new Attachment(carrier, trigger);
            _attachments.Add(attachment);
            return attachment;
        }

        public sealed class Attachment : IDisposable
        {
            public Attachment(IEntity carrier, IFireInput trigger)
            {
                Carrier = carrier;
                Trigger = trigger;
            }

            public IEntity Carrier { get; }
            public IFireInput Trigger { get; }
            public bool IsDisposed { get; private set; }

            public void Dispose() => IsDisposed = true;
        }
    }
}
