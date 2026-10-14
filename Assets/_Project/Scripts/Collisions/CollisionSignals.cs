namespace Asteroids.Collisions
{
    public readonly struct CollisionEntered
    {
        public CollisionEntered(ICollider self, ICollider other, Contact contact)
        {
            Self = self;
            Other = other;
            Contact = contact;
        }

        public ICollider Self { get; }
        public ICollider Other { get; }
        public Contact Contact { get; }
    }

    public readonly struct CollisionStayed
    {
        public CollisionStayed(ICollider self, ICollider other, Contact contact)
        {
            Self = self;
            Other = other;
            Contact = contact;
        }

        public ICollider Self { get; }
        public ICollider Other { get; }
        public Contact Contact { get; }
    }

    public readonly struct CollisionExited
    {
        public CollisionExited(ICollider self, ICollider other)
        {
            Self = self;
            Other = other;
        }

        public ICollider Self { get; }
        public ICollider Other { get; }
    }
}
