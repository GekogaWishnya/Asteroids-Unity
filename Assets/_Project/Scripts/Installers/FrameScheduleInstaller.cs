using Asteroids.Common;
using Asteroids.Controls;
using Asteroids.Physics;
using Asteroids.Simulation;
using Asteroids.Ticking;
using Asteroids.UI;
using Cysharp.Threading.Tasks;
using Zenject;

namespace Asteroids.Installers
{
    /// <summary>
    /// Binds all <see cref="IFrameTick"/> to their player loop timings.
    /// Binding order decides the update order inside one timing.
    /// Requires every scheduled type, bound to itself, and <see cref="Asteroids.Common.IErrorLog"/>.
    /// </summary>
    public sealed class FrameScheduleInstaller : Installer<FrameScheduleInstaller>
    {
        public override void InstallBindings()
        {
            Container.Bind<IFrameSource>().To<PlayerLoopFrameSource>().AsSingle();

            Container.BindFrameTick<SchemeGatedShipInput>(PlayerLoopTiming.PreUpdate);
            Container.BindFrameTick<SimulationLoop>(PlayerLoopTiming.Update);
            Container.BindFrameTick<PoseViewSync>(PlayerLoopTiming.PreLateUpdate);
            Container.BindFrameTick<ViewModelRefresher>(PlayerLoopTiming.PreLateUpdate);
        }
    }
}