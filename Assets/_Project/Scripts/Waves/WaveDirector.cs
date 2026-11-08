using System;
using Asteroids.Core;
using Asteroids.Core.IConfigs;

namespace Asteroids.Waves
{
    /// <summary>
    /// <para>
    /// Runs a level: waits for the delay of a wave, starts it, spawns its entries on schedule (the k-th
    /// enemy of an entry appears <c>StartDelay + k * Interval</c> seconds after the wave started) and
    /// considers the wave cleared once everything has appeared and no enemy is alive. The delay of the
    /// next wave counts from that moment. After the last wave the level is completed.
    /// </para>
    /// <para>
    /// A simulation step: register it in <see cref="SimulationPhase.Logic"/> so it sees what the collision
    /// phase of the same tick did. Does not look at the player: the level goes on if the ship dies.
    /// The level is checked against the spawners in the constructor, so a bad level fails when the game starts.
    /// </para>
    /// </summary>
    public sealed class WaveDirector : ISimulationStep, IRestartable, IWaveProgress
    {
        private readonly Wave[] _waves;
        private readonly IEnemyData _data;
        private readonly IWaveEventSink _events;

        private float _delayTimer;
        private float _waveTime;
        private int[] _spawned = Array.Empty<int>();

        /// <exception cref="ArgumentNullException">Any argument is null.</exception>
        /// <exception cref="InvalidOperationException">The level is invalid; the message lists every problem.</exception>
        public WaveDirector(ILevelConfig level, IEnemyData data, IWaveEventSink events)
        {
            if (level == null) throw new ArgumentNullException(nameof(level));
            _data = data ?? throw new ArgumentNullException(nameof(data));
            _events = events ?? throw new ArgumentNullException(nameof(events));

            LevelValidator.ThrowIfInvalid(level, data);
            _waves = Snapshot(level);
        }

        public WaveStage Stage { get; private set; } = WaveStage.Waiting;

        public int WaveCount => _waves.Length;

        /// <summary>Waves cleared so far.</summary>
        public int Cleared { get; private set; }

        int IWaveProgress.Total => WaveCount;

        /// <summary>Starts at 1 - the wave in progress or about to start.</summary>
        public int WaveNumber => Math.Min(Cleared + 1, _waves.Length);

        public RestartOrder Order => RestartOrder.Waves;

        public void Restart()
        {
            Cleared = 0;
            _delayTimer = 0f;
            _waveTime = 0f;
            _spawned = Array.Empty<int>();
            Stage = WaveStage.Waiting;
        }

        public void Tick(float deltaTime)
        {
            if (Stage == WaveStage.Completed) return;

            var elapsed = deltaTime;

            if (Stage == WaveStage.Waiting)
            {
                _delayTimer += deltaTime;
                var delay = _waves[Cleared].DelayBeforeStart;
                if (_delayTimer < delay) return;

                elapsed = _delayTimer - delay;
                BeginWave();
            }

            if (Stage == WaveStage.Spawning)
            {
                _waveTime += elapsed;
                SpawnDueEnemies();
            }

            if (Stage == WaveStage.Clearing && _data.AliveCount == 0) FinishWave();
        }

        private void BeginWave()
        {
            Stage = WaveStage.Spawning;
            _waveTime = 0f;
            _spawned = new int[_waves[Cleared].Entries.Length];

            _events.Started(Cleared + 1, _waves.Length);
        }

        private void SpawnDueEnemies()
        {
            var entries = _waves[Cleared].Entries;
            var allSpawned = true;

            for (var i = 0; i < entries.Length; i++)
            {
                var entry = entries[i];

                while (_spawned[i] < entry.Count && _waveTime >= entry.StartDelay + _spawned[i] * entry.Interval)
                {
                    _data.Spawn(entry.Type);
                    _spawned[i]++;
                }

                if (_spawned[i] < entry.Count) allSpawned = false;
            }

            if (allSpawned) Stage = WaveStage.Clearing;
        }

        private void FinishWave()
        {
            var number = Cleared + 1;
            Cleared++;

            var completed = Cleared >= _waves.Length;
            Stage = completed ? WaveStage.Completed : WaveStage.Waiting;
            _delayTimer = 0f;

            _events.Cleared(number);
            if (completed) _events.Completed(_waves.Length);
        }

        /// <summary>Copies the level so that later changes of the config cannot disturb a running level.</summary>
        private static Wave[] Snapshot(ILevelConfig level)
        {
            var waves = new Wave[level.Waves.Count];

            for (var i = 0; i < waves.Length; i++)
            {
                var wave = level.Waves[i];
                var entries = new Entry[wave.Entries.Count];

                for (var j = 0; j < entries.Length; j++)
                {
                    var entry = wave.Entries[j];
                    entries[j] = new Entry(entry.Type, entry.Count, entry.StartDelay, entry.Interval);
                }

                waves[i] = new Wave(wave.DelayBeforeStart, entries);
            }

            return waves;
        }

        private readonly struct Wave
        {
            public Wave(float delayBeforeStart, Entry[] entries)
            {
                DelayBeforeStart = delayBeforeStart;
                Entries = entries;
            }

            public float DelayBeforeStart { get; }
            public Entry[] Entries { get; }
        }

        private readonly struct Entry
        {
            public Entry(EnemyTypeId type, int count, float startDelay, float interval)
            {
                Type = type;
                Count = count;
                StartDelay = startDelay;
                Interval = interval;
            }

            public EnemyTypeId Type { get; }
            public int Count { get; }
            public float StartDelay { get; }
            public float Interval { get; }
        }
    }
}