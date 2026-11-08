namespace Asteroids.Core.IConfigs
{
    public interface IRunEndScreenConfig
    {
        string DefeatTitle { get; }
        string VictoryTitle { get; }

        /// <summary>{0} is the final score, an int.</summary>
        string ScoreFormat { get; }

        /// <summary>{0} is the number of waves cleared and {1} the number of waves in the level, both ints.</summary>
        string WavesFormat { get; }
    }
}