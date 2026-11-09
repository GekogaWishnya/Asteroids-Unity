namespace Asteroids.VFX
{
    /// <summary>Freezes and resumes every effect. Effects run on Unity's clock, not the simulation's.</summary>
    public interface IEffectsPause
    {
        bool IsPaused { get; }

        void SetPaused(bool paused);
    }
}