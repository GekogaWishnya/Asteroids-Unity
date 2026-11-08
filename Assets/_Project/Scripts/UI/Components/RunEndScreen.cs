using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Zenject;

namespace Asteroids.UI.Components
{
    /// <summary>
    /// Shows its panel while <see cref="RunEndViewModel.IsVisible"/>, wires the buttons to the view model,
    /// and selects Replay each time the panel appears, so Submit and gamepad navigation work without a mouse.
    /// Put it on an object that stays active, with the panel as a child.
    /// </summary>
    [AddComponentMenu("Asteroids/UI/Run End Screen")]
    [DisallowMultipleComponent]
    public sealed class RunEndScreen : MonoBehaviour
    {
        [Tooltip("The content shown when the run has ended. Must not be this GameObject")]
        [SerializeField] private GameObject panel;

        [Tooltip("Starts a new run. Selected when the panel appears")]
        [SerializeField] private Button replayButton;

        private RunEndViewModel _viewModel;
        private IDisposable _subscription;

        [Inject]
        private void Construct(RunEndViewModel viewModel)
        {
            _viewModel = viewModel ?? throw new ArgumentNullException(nameof(viewModel));
            if (isActiveAndEnabled) Bind();
        }

        private void Bind()
        {
            ThrowIfMisconfigured();

            Unbind();
            replayButton.onClick.AddListener(_viewModel.Replay);
            _subscription = _viewModel.IsVisible.Subscribe(Show);
        }

        private void ThrowIfMisconfigured()
        {
            if (!panel)
                throw new InvalidOperationException($"{nameof(RunEndScreen)} on '{name}' has no panel assigned.");
            if (panel == gameObject)
                throw new InvalidOperationException(
                    $"{nameof(RunEndScreen)} on '{name}': the panel must be a child. Hiding this object would stop the screen.");
            if (!replayButton)
                throw new InvalidOperationException(
                    $"{nameof(RunEndScreen)} on '{name}' has no Replay button assigned.");
        }

        private void OnEnable()
        {
            if (_viewModel != null) Bind();
        }

        private void OnDisable() => Unbind();

        private void Unbind()
        {
            _subscription?.Dispose();
            _subscription = null;
            if (_viewModel != null && replayButton) replayButton.onClick.RemoveListener(_viewModel.Replay);
        }

        private void Show(bool visible)
        {
            panel.SetActive(visible);
            if (!visible) return;

            var eventSystem = EventSystem.current;
            if (eventSystem) eventSystem.SetSelectedGameObject(replayButton.gameObject);
        }
    }
}