namespace Asteroids.Core
{
    public interface ISingleUseDamageSource : IDamageSource
    {
        bool IsSpent { get; }
        void Spend();
    }
}
