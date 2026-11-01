namespace Asteroids.Physics
{
    public interface IStunnable
    {
        /// <summary>Stops acting for at least <paramref name="seconds"/>. A longer stun in progress is kept.</summary>
        /// <exception cref="System.ArgumentOutOfRangeException"><paramref name="seconds"/> is negative or not finite.</exception>
        void Stun(float seconds);
    }
}
