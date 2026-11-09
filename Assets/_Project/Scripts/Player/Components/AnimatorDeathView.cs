using System;
using Asteroids.Core;
using UnityEngine;
using Zenject;

namespace Asteroids.Player.Components
{
    /// <summary>
    /// Presents the ship's death through an Animator. A bool parameter is true while the ship has no health left.
    /// </summary>
    [AddComponentMenu("Asteroids/Player/Animator Death View")]
    [DisallowMultipleComponent]
    public sealed class AnimatorDeathView : MonoBehaviour
    {
        [Tooltip("Animator that presents the death. Defaults to the one on this GameObject")]
        [SerializeField] private Animator animator;
        [Tooltip("Bool parameter of the Animator Controller that is true while the ship is dead")]
        [SerializeField] private string deadParameter = "Dead";

        private IDisposable _subscription;
        private int _parameterHash;

        /// <exception cref="ArgumentNullException"><paramref name="health"/> is null.</exception>
        /// <exception cref="InvalidOperationException">There is no Animator or no parameter name.</exception>
        [Inject]
        private void Construct(IPlayerHealth health)
        {
            if (health == null) throw new ArgumentNullException(nameof(health));

            if (!animator) TryGetComponent(out animator);

            var problem = FindProblem();
            if (problem != null) throw new InvalidOperationException(problem);

            _parameterHash = Animator.StringToHash(deadParameter);
            _subscription?.Dispose();
            _subscription = health.Current.Subscribe(current => Apply(current <= 0));
        }

        private void Apply(bool isDead)
        {
            if (!animator || !animator.isActiveAndEnabled) return;

            animator.SetBool(_parameterHash, isDead);
        }

        private void Reset() => animator = GetComponent<Animator>();

        private void OnDestroy()
        {
            _subscription?.Dispose();
            _subscription = null;
        }

        private string FindProblem()
        {
            if (!animator) return $"{nameof(AnimatorDeathView)} on '{name}' has no Animator.";
            if (string.IsNullOrWhiteSpace(deadParameter))
                return $"{nameof(AnimatorDeathView)} on '{name}' has no parameter name.";
            return null;
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            UnityEditor.EditorApplication.delayCall -= ValidateWhenSettled;
            UnityEditor.EditorApplication.delayCall += ValidateWhenSettled;
        }

        private void ValidateWhenSettled()
        {
            if (!this) return;

            if (!animator && TryGetComponent<Animator>(out _)) return;

            var problem = FindProblem();
            if (problem != null) Debug.LogWarning(problem, this);
        }
#endif
    }
}