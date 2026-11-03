namespace Asteroids.Collisions
{
    /// <summary>
    /// <para>
    /// A gameplay or physics reaction to contacts, called in <see cref="Order"/> before the signal bus.
    /// Use it instead of subscribing to the collision signals when the reaction must happen in a known order.
    /// </para>
    /// <para>
    /// Bind it explicitly with <c>Bind&lt;ICollisionResponse&gt;().To&lt;T&gt;()</c>.
    /// </para>
    /// </summary>
    public interface ICollisionResponse : ICollisionEventSink
    {
        CollisionResponseOrder Order { get; }
    }
}
