using UnityEngine;
using Zenject;

namespace Asteroids.UI.Components
{
    [AddComponentMenu("Asteroids/UI/Run End Waves View")]
    public sealed class RunEndWavesView : ObservableTextView
    {
        [Inject]
        private void Construct(RunEndViewModel viewModel) => Initialize(viewModel.WavesText);
    }
}