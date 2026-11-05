using UnityEngine;
using Zenject;

namespace Asteroids.UI.Components
{
    [AddComponentMenu("Asteroids/UI/Ship Angle View")]
    public sealed class ShipAngleView : ObservableTextView
    {
        [Inject]
        private void Construct(ShipTelemetryViewModel viewModel) => Initialize(viewModel.AngleText);
    }
}