using System;
using Asteroids.Core;
using Asteroids.Player.Components;
using NUnit.Framework;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;
using Zenject;
using Object = UnityEngine.Object;

namespace Asteroids.Player.Tests
{
    /// <summary>
    /// <see cref="AnimatorDeathView"/> against a real <see cref="PlayerHealth"/> and a real Animator.
    /// The controller is built in memory with just the "Dead" parameter and one empty state; edit mode does not
    /// tick Animators, so each test initialises it with <c>Rebind</c> + <c>Update(0)</c> before reading parameters.
    /// Zenject calling Construct is the only entry point, exactly as in the scene.
    /// </summary>
    [TestFixture]
    public sealed class AnimatorDeathViewTests
    {
        private const string Dead = "Dead";

        private GameObject _sprite;
        private Animator _animator;
        private AnimatorController _controller;
        private PlayerHealth _health;

        [SetUp]
        public void SetUp()
        {
            _controller = new AnimatorController();
            _controller.AddLayer("Base Layer");
            _controller.AddParameter(Dead, AnimatorControllerParameterType.Bool);
            _controller.layers[0].stateMachine.AddState("Alive");

            _sprite = new GameObject("Sprite");
            _animator = _sprite.AddComponent<Animator>();
            _animator.runtimeAnimatorController = _controller;
            _animator.Rebind();
            _animator.Update(0f);

            _health = new PlayerHealth(new TestHealthConfig { MaxHealth = 2 });
        }

        [TearDown]
        public void TearDown()
        {
            Object.DestroyImmediate(_sprite);
            Object.DestroyImmediate(_controller);
        }

        private AnimatorDeathView AddView(Animator animator, string parameter = Dead)
        {
            var view = _sprite.AddComponent<AnimatorDeathView>();
            var serialized = new SerializedObject(view);
            serialized.FindProperty("animator").objectReferenceValue = animator;
            serialized.FindProperty("deadParameter").stringValue = parameter;
            serialized.ApplyModifiedPropertiesWithoutUndo();
            return view;
        }

        private void Inject(AnimatorDeathView view)
        {
            var container = new DiContainer();
            container.Bind<IPlayerHealth>().FromInstance(_health);
            container.Inject(view);
        }

        private bool IsDead => _animator.GetBool(Dead);

        [Test]
        public void Construct_ALivingShip_IsNotDead()
        {
            Inject(AddView(_animator));

            Assert.That(IsDead, Is.False);
        }

        [Test]
        public void Construct_AShipThatIsAlreadyDead_IsDead()
        {
            _health.TakeDamage(2);

            Inject(AddView(_animator));

            Assert.That(IsDead, Is.True);
        }

        [Test]
        public void ANonFatalHit_KeepsItAlive()
        {
            Inject(AddView(_animator));

            _health.TakeDamage(1);

            Assert.That(IsDead, Is.False);
        }

        [Test]
        public void Depletion_SetsDead_AndResetClearsIt()
        {
            Inject(AddView(_animator));

            _health.TakeDamage(2);
            Assert.That(IsDead, Is.True);

            _health.Reset();
            Assert.That(IsDead, Is.False);
        }

        [Test]
        public void Construct_WithoutAnAssignedAnimator_UsesTheOneOnItsGameObject()
        {
            // Scene injection runs before Awake, so this fallback must happen inside Construct.
            Inject(AddView(null));

            _health.TakeDamage(2);

            Assert.That(IsDead, Is.True);
        }

        [Test]
        public void Construct_WithNoAnimatorAnywhere_Throws()
        {
            var bare = new GameObject("Bare");
            try
            {
                var view = bare.AddComponent<AnimatorDeathView>();

                var thrown = Assert.Catch(() => Inject(view));
                Assert.That(Unwrap(thrown), Is.InstanceOf<InvalidOperationException>());
            }
            finally
            {
                Object.DestroyImmediate(bare);
            }
        }

        [Test]
        public void Construct_WithoutAParameterName_Throws()
        {
            var view = AddView(_animator, " ");

            var thrown = Assert.Catch(() => Inject(view));
            Assert.That(Unwrap(thrown), Is.InstanceOf<InvalidOperationException>());
        }

        // Zenject wraps exceptions thrown by inject methods, so look for ours anywhere in the chain.
        private static Exception Unwrap(Exception exception)
        {
            while (exception is not InvalidOperationException && exception?.InnerException != null)
                exception = exception.InnerException;
            return exception;
        }
    }
}
