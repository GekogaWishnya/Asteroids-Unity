using System;
using Asteroids.Common;
using TMPro;
using UnityEngine;

namespace Asteroids.UI.Components
{
    /// <summary>
    /// Shows an observable string in a text label while the view is enabled.
    /// </summary>
    public abstract class ObservableTextView : MonoBehaviour
    {
        [Tooltip("Label that shows the text.")]
        [SerializeField] private TMP_Text label;

        private IObservableValue<string> _source;
        private IDisposable _subscription;

        /// <exception cref="ArgumentNullException"><paramref name="source"/> is null.</exception>
        protected void Initialize(IObservableValue<string> source)
        {
            _source = source ?? throw new ArgumentNullException(nameof(source));

            if (isActiveAndEnabled) Bind();
        }

        private void OnEnable()
        {
            if (_source != null) Bind();
        }

        private void OnDisable() => Unbind();

        private void Bind()
        {
            if (label == null)
                throw new InvalidOperationException($"{GetType().Name} on '{name}' has no label assigned.");

            Unbind();
            _subscription = _source.Subscribe(Apply);
        }

        private void Unbind()
        {
            _subscription?.Dispose();
            _subscription = null;
        }

        private void Apply(string text) => label.text = text;
    }
}
