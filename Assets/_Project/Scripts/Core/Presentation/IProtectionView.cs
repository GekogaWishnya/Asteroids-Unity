namespace Asteroids.Core
{
    /// <summary>Presentation of an entity's invulnerability window.</summary>
    public interface IProtectionView
    {
        /// <summary>Called with true when protection starts and with false when it ends.</summary>
        void SetProtected(bool isProtected);
    }
}
