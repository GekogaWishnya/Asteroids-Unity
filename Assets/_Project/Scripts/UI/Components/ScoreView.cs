using UnityEngine;
using Zenject;

namespace Asteroids.UI.Components
{
    /// <summary>Shows the player's score as text.</summary>
    [AddComponentMenu("Asteroids/UI/Score View")]
    public sealed class ScoreView : ObservableTextView
    {
        [Inject]
        private void Construct(ScoreViewModel viewModel) => Initialize(viewModel.Text);
    }
}
