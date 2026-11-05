using UnityEngine;
using Zenject;

namespace Asteroids.UI.Components
{
    [AddComponentMenu("Asteroids/UI/Ship Position View")]
    public sealed class ShipPositionView : ObservableTextView
    {
        [Inject]
        private void Construct(ShipTelemetryViewModel viewModel) => Initialize(viewModel.PositionText);
    }
}