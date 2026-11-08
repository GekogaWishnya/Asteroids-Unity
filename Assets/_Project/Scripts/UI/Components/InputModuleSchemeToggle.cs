using System;
using Asteroids.Core;
using UnityEngine;
using UnityEngine.EventSystems;
using Zenject;

namespace Asteroids.UI.Components
{
    /// <summary>
    /// Enables the EventSystem's input module only while <see cref="InputScheme.UI"/> is active, so menus never
    /// react to gameplay keys. Put it next to the EventSystem.
    /// </summary>
    [AddComponentMenu("Asteroids/UI/Input Module Scheme Toggle")]
    [DisallowMultipleComponent]
    public sealed class InputModuleSchemeToggle : MonoBehaviour
    {
        [Tooltip("The module to switch. Defaults to the input module on this GameObject")]
        [SerializeField] private BaseInputModule module;

        private IInputSchemes _schemes;

        [Inject]
        private void Construct(IInputSchemes schemes) =>
            _schemes = schemes ?? throw new ArgumentNullException(nameof(schemes));

        private void Awake()
        {
            if (!module) module = GetComponent<BaseInputModule>();
            if (!module)
                throw new InvalidOperationException(
                    $"{nameof(InputModuleSchemeToggle)} on '{name}' has no input module. " +
                    "Assign one or add it to this GameObject.");

            Apply();
        }

        private void LateUpdate() => Apply();

        private void Apply()
        {
            if (_schemes == null) return;

            var shouldListen = _schemes.Active.Value == InputScheme.UI;
            if (module.enabled != shouldListen) module.enabled = shouldListen;
        }
    }
}