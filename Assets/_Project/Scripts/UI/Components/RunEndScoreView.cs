using UnityEngine;
using Zenject;

namespace Asteroids.UI.Components
{
    [AddComponentMenu("Asteroids/UI/Run End Score View")]
    public sealed class RunEndScoreView : ObservableTextView
    {
        [Inject]
        private void Construct(RunEndViewModel viewModel) => Initialize(viewModel.ScoreText);
    }
}