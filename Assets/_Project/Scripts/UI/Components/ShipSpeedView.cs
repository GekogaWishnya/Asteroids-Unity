using UnityEngine;
using Zenject;

namespace Asteroids.UI.Components
{
    [AddComponentMenu("Asteroids/UI/Ship Speed View")]
    public sealed class ShipSpeedView : ObservableTextView
    {
        [Inject]
        private void Construct(ShipTelemetryViewModel viewModel) => Initialize(viewModel.SpeedText);
    }
}