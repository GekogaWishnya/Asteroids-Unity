using System;
using System.Threading;
using Asteroids.Core;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.EventSystems;
using Zenject;

namespace Asteroids.UI.Components
{
    /// <summary>
    /// Enables the EventSystem's input module only while <see cref="InputScheme.UI"/> is active, so menus never
    /// react to gameplay keys. Put it next to the EventSystem.
    /// A scheme change is applied at the next <see cref="PlayerLoopTiming.PreLateUpdate"/>.
    /// </summary>
    [AddComponentMenu("Asteroids/UI/Input Module Scheme Toggle")]
    [DisallowMultipleComponent]
    public sealed class InputModuleSchemeToggle : MonoBehaviour
    {
        [Tooltip("The module to switch. Defaults to the input module on this GameObject")]
        [SerializeField] private BaseInputModule module;

        private IInputSchemes _schemes;
        private IDisposable _subscription;
        private CancellationTokenSource _whileEnabled;

        [Inject]
        private void Construct(IInputSchemes schemes)
        {
            _schemes = schemes ?? throw new ArgumentNullException(nameof(schemes));
            if (isActiveAndEnabled) Listen();
        }

        private void Awake()
        {
            if (!module) module = GetComponent<BaseInputModule>();
            if (!module)
                throw new InvalidOperationException(
                    $"{nameof(InputModuleSchemeToggle)} on '{name}' has no input module. " +
                    "Assign one or add it to this GameObject.");

            Apply();
        }

        private void OnEnable() => Listen();

        private void OnDisable()
        {
            _subscription?.Dispose();
            _subscription = null;

            _whileEnabled?.Cancel();
            _whileEnabled?.Dispose();
            _whileEnabled = null;
        }

        private void Listen()
        {
            if (_schemes == null || _subscription != null) return;

            _whileEnabled = new CancellationTokenSource();
            var token = _whileEnabled.Token;
            _subscription = _schemes.Active.Subscribe(_ => ApplyLaterAsync(token).Forget());
        }

        private async UniTaskVoid ApplyLaterAsync(CancellationToken cancellation)
        {
            if (await UniTask.Yield(PlayerLoopTiming.PreLateUpdate, cancellation,
                    cancelImmediately: true).SuppressCancellationThrow())
                return;

            Apply();
        }

        private void Apply()
        {
            if (_schemes == null) return;

            var shouldListen = _schemes.Active.Value == InputScheme.UI;
            if (module.enabled != shouldListen) module.enabled = shouldListen;
        }
    }
}