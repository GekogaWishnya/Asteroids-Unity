using UnityEngine;
using Zenject;

namespace Asteroids.UI.Components
{
    [AddComponentMenu("Asteroids/UI/Run End Title View")]
    public sealed class RunEndTitleView : ObservableTextView
    {
        [Inject]
        private void Construct(RunEndViewModel viewModel) => Initialize(viewModel.Title);
    }
}