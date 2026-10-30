namespace Asteroids.Core
{
    public interface IFireInput
    {
        /// <summary>True while the trigger is held.</summary>
        bool IsFiring { get; }
    }
}
