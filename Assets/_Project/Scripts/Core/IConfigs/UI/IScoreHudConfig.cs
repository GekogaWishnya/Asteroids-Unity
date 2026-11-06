namespace Asteroids.Core.IConfigs
{
    /// <summary>Formatting (<see cref="string.Format(string, object)"/>) of the score to show on HUD.</summary>
    public interface IScoreHudConfig
    {
        /// <summary>{0} is the score, an int. For example "SCORE {0:D6}" pads it to six digits.</summary>
        string ScoreFormat { get; }
    }
}