namespace Asteroids.Collisions
{
    public readonly struct CollisionEntered
    {
        public CollisionEntered(ICollider first, ICollider second, Contact contact)
        {
            First = first;
            Second = second;
            Contact = contact;
        }

        public ICollider First { get; }
        public ICollider Second { get; }
        public Contact Contact { get; }
    }

    public readonly struct CollisionStayed
    {
        public CollisionStayed(ICollider first, ICollider second, Contact contact)
        {
            First = first;
            Second = second;
            Contact = contact;
        }

        public ICollider First { get; }
        public ICollider Second { get; }
        public Contact Contact { get; }
    }

    public readonly struct CollisionExited
    {
        public CollisionExited(ICollider first, ICollider second)
        {
            First = first;
            Second = second;
        }

        public ICollider First { get; }
        public ICollider Second { get; }
    }
}