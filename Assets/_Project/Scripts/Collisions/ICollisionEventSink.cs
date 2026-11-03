namespace Asteroids.Collisions
{
    /// <summary>
    /// Receives the Enter/Stay/Exit events produced by <see cref="ICollisionSystem"/>.
    /// </summary>
    public interface ICollisionEventSink
    {
        void Entered(ICollider first, ICollider second, in Contact contact);
        void Stayed(ICollider first, ICollider second, in Contact contact);
        void Exited(ICollider first, ICollider second);
    }
}
