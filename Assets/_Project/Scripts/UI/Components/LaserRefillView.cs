using UnityEngine;
using Zenject;

namespace Asteroids.UI.Components
{
    [AddComponentMenu("Asteroids/UI/Laser Refill View")]
    public sealed class LaserRefillView : ObservableTextView
    {
        [Inject]
        private void Construct([InjectOptional] LaserViewModel viewModel)
        {
            if (viewModel != null) Initialize(viewModel.RefillText);
        }
    }
}
