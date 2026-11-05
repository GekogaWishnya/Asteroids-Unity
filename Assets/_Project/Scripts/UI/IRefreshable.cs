namespace Asteroids.UI
{
    /// <summary>
    /// A view model that reads a source without change events and
    /// is refreshed once per frame by <see cref="ViewModelRefresher"/>.
    /// </summary>
    public interface IRefreshable
    {
        void Refresh();
    }
}