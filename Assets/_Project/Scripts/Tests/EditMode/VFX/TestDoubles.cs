using System;
using System.Collections.Generic;
using Asteroids.Common;
using Asteroids.Core;
using Asteroids.Core.IConfigs;
using UnityEngine;

namespace Asteroids.VFX.Tests
{
    internal sealed class TestEffectConfig : IEffectConfig
    {
        public TestEffectConfig(string name = "Effect") => Name = name;

        public string Name { get; }
        public float Scale { get; set; } = 1f;
        public int PoolPrewarm { get; set; }
        public int PoolMaxIdle { get; set; } = 8;
        public int MaxActive { get; set; } = 8;
    }

    internal class TestFeedback : IFeedbackConfig
    {
        public IEffectConfig Hit { get; set; }
        public bool PlayHitOnFatal { get; set; }
        public IEffectConfig Death { get; set; } = new TestEffectConfig("Death");
    }

    internal sealed class TestEnemyFeedback : TestFeedback, IEnemyFeedbackConfig
    {
        public TestEnemyFeedback(string type) => Type = new EnemyTypeId(type);

        public EnemyTypeId Type { get; }
    }

    internal sealed class TestEffectsConfig : IEffectsConfig
    {
        public IFeedbackConfig Ship { get; set; } = new TestFeedback();
        public List<IEnemyFeedbackConfig> EnemyList { get; } = new();
        public IReadOnlyList<IEnemyFeedbackConfig> Enemies => EnemyList;
    }

    internal sealed class FakeEffect : IEffect
    {
        public FakeEffect(IEffectConfig config) => Config = config;

        public IEffectConfig Config { get; }
        public bool IsPlaying { get; private set; }
        public bool IsPaused { get; private set; }
        public bool IsDisposed { get; private set; }
        public int PlayCount { get; private set; }
        public int StopCount { get; private set; }
        public Vector2 Position { get; private set; }
        public float Scale { get; private set; }
        public bool FailOnPlay { get; set; }

        public event Action<IEffect> Finished;

        public void Play(Vector2 position, float scale)
        {
            if (FailOnPlay) throw new InvalidOperationException("play failed");

            Position = position;
            Scale = scale;
            IsPlaying = true;
            IsPaused = false;
            PlayCount++;
        }

        public void Stop()
        {
            IsPlaying = false;
            IsPaused = false;
            StopCount++;
        }

        public void Pause()
        {
            if (IsPlaying) IsPaused = true;
        }

        public void Resume() => IsPaused = false;

        public void Dispose() => IsDisposed = true;

        /// <summary>Simulates the particle system finishing by itself.</summary>
        public void Finish()
        {
            IsPlaying = false;
            Finished?.Invoke(this);
        }
    }

    internal sealed class FakeEffectFactory : IEffectFactory
    {
        public List<FakeEffect> Created { get; } = new();
        public bool FailOnCreate { get; set; }

        public IEffect Create(IEffectConfig config)
        {
            if (FailOnCreate) throw new InvalidOperationException("broken prefab");

            var effect = new FakeEffect(config);
            Created.Add(effect);
            return effect;
        }

        public List<FakeEffect> Playing(IEffectConfig config) =>
            Created.FindAll(effect => effect.IsPlaying && ReferenceEquals(effect.Config, config));
    }

    internal sealed class TestEntity : IEntity
    {
        public TestEntity(Vector2 position) => Pose = new TestPose { Position = position };

        public IPose Pose { get; }
    }

    internal sealed class TestPose : IPose
    {
        public Vector2 Position { get; set; }
        public float Angle { get; set; }
        public Vector2 Up => Vector2.up;
        public Vector2 Right => Vector2.right;
    }

    /// <summary>Keeps every failure the code under test reports, instead of writing to the Unity console.</summary>
    internal sealed class RecordingErrorLog : IErrorLog
    {
        public List<(Exception Failure, string Context)> Reports { get; } = new();

        public void Report(Exception failure, string context) => Reports.Add((failure, context));
    }
}