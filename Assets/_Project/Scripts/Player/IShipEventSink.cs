using Asteroids.Core;

namespace Asteroids.Player
{
    public interface IShipEventSink
    {
        /// <param name="isFatal">Whether the hit took the last of its health.</param>
        void Damaged(IEntity ship, bool isFatal);

        void Destroyed(IEntity entity);
    }
}