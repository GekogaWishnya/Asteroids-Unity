using UnityEngine;
using Zenject;

namespace Asteroids.UI.Components
{
    /// <summary>Shows the player's health as text.</summary>
    [AddComponentMenu("Asteroids/UI/Health View")]
    public sealed class HealthView : ObservableTextView
    {
        [Inject]
        private void Construct(HealthViewModel viewModel) => Initialize(viewModel.Text);
    }
}
