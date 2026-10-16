namespace Asteroids.Collisions
{
    /// <summary>
    /// Receives the Enter/Stay/Exit events produced by <see cref="ICollisionSystem"/>.
    /// </summary>
    public interface ICollisionEventSink
    {
        void Entered(ICollider self, ICollider other, in Contact contact);
        void Stayed(ICollider self, ICollider other, in Contact contact);
        void Exited(ICollider self, ICollider other);
    }
}
