using Zenject;

namespace Asteroids.Scoring
{
    /// <summary>
    /// Binds <see cref="ScoreModel"/>, available as <see cref="Asteroids.Core.IScore"/> and
    /// <see cref="Asteroids.Core.IScoreSink"/>.
    /// </summary>
    public sealed class ScoringInstaller : Installer<ScoringInstaller>
    {
        public override void InstallBindings()
        {
            Container.BindInterfacesTo<ScoreModel>().AsSingle();
        }
    }
}
