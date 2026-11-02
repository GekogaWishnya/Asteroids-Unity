using UnityEngine;
using Zenject;

namespace Asteroids.UI.Components
{
    [AddComponentMenu("Asteroids/UI/Laser Energy View")]
    public sealed class LaserEnergyView : ObservableTextView
    {
        [Inject]
        private void Construct([InjectOptional] LaserViewModel viewModel)
        {
            if (viewModel != null) Initialize(viewModel.EnergyText);
        }
    }
}
