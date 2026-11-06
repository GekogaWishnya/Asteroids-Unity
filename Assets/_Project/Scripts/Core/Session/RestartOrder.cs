namespace Asteroids.Core
{
    /// <summary>The order in which <see cref="IGameSession.Restart"/> restarts the game, lowest first. </summary>
    public enum RestartOrder
    {
        Enemies = 0,
        Ship = 100,
        Waves = 200,
        Score = 300
    }
}