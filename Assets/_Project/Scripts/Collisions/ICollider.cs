namespace Asteroids.Collisions
{
    public interface ICollider
    {
        object Owner { get; }
        /// <summary>Layers this collider is a part of.</summary>
        CollisionLayer Layer { get; }
        /// <summary>Layers this collider reacts to. A pair interacts only if both masks accept the other's layer.</summary>
        CollisionLayer Mask { get; }
        
        /// <param name="contact">contact expressed from this collider's perspective.</param>>
        bool TryGetContact(ICollider other, out Contact contact);

        void NotifyEntered(ICollider other, in Contact contact);
        void NotifyStayed(ICollider other, in Contact contact);
        void NotifyExited(ICollider other);
    }
}
