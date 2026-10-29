using System;
using System.Collections.Generic;
using Asteroids.Core.IConfigs;
using UnityEngine;

namespace Asteroids.Data
{
    [CreateAssetMenu(fileName = "Level", menuName = "Asteroids/Configs/Level")]
    public sealed class LevelConfig : ScriptableObject, ILevelConfig
    {
        [SerializeField] private List<WaveData> waves = new();

        public IReadOnlyList<IWaveConfig> Waves => waves;

        private void OnValidate()
        {
            foreach (var problem in LevelConfigValidator.Validate(this))
                Debug.LogWarning($"{name}: {problem}", this);
        }

        [Serializable]
        private sealed class WaveData : IWaveConfig
        {
            [Tooltip("Seconds to wait before this wave. Counted from the moment the previous wave is cleared, " +
                     "or from the start of the level for the first wave")]
            [SerializeField, Min(0f)] private float delayBeforeStart = 2f;
            [SerializeField] private List<EntryData> entries = new();

            public float DelayBeforeStart => delayBeforeStart;
            public IReadOnlyList<IWaveEntryConfig> Entries => entries;
        }

        [Serializable]
        private sealed class EntryData : IWaveEntryConfig
        {
            [SerializeField] private EnemyTypeDefinition type;
            [SerializeField, Min(1)] private int count = 1;
            [Tooltip("Seconds from the start of the wave until the first enemy of this entry appears")]
            [SerializeField, Min(0f)] private float startDelay;
            [Tooltip("Seconds between two enemies of this entry. 0 spawns them all at once")]
            [SerializeField, Min(0f)] private float interval;

            public EnemyTypeId Type => type ? type.Id : default;
            public int Count => count;
            public float StartDelay => startDelay;
            public float Interval => interval;
        }
    }
}
