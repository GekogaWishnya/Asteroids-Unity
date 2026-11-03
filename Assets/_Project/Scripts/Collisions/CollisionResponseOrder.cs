namespace Asteroids.Collisions
{
    /// <summary>The order in which the <see cref="ICollisionResponse"/>s see every contact.</summary>
    public enum CollisionResponseOrder
    {
        Bounce = 0,
        Damage = 100,
        ProjectileImpact = 200,
        LaserContact = 300
    }
}
