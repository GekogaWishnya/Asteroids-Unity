namespace Asteroids.Controls
{
    public interface IAxisReader
    {
        /// <summary>
        /// The unsmoothed value of <paramref name="axis"/> this frame. Keys and buttons read 1 (or -1 for a
        /// negative button) while held. Controllers read their dead-zoned value.
        /// </summary>
        /// <exception cref="System.InvalidOperationException">No Input Manager entry is named <paramref name="axis"/>.</exception>
        float Read(string axis);
    }
}