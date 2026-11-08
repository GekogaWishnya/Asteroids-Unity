using System;
using System.Collections.Generic;
using Asteroids.Common;
using Asteroids.Core;
using Asteroids.Core.IConfigs;

namespace Asteroids.Session.Tests
{
    internal sealed class FakeSimulation : ISimulationControl
    {
        public bool IsPaused { get; set; }
    }

    /// <summary>Records its order into a shared log so tests can check the restart sequence.</summary>
    internal sealed class FakeRestartable : IRestartable
    {
        private readonly List<string> _log;

        public FakeRestartable(RestartOrder order, List<string> log)
        {
            Order = order;
            _log = log;
        }

        public RestartOrder Order { get; }
        public Exception Failure { get; set; }

        public void Restart()
        {
            _log.Add(Order.ToString());
            if (Failure != null) throw Failure;
        }
    }

    internal sealed class FakeRunEndConfig : IRunEndConfig
    {
        public float DefeatDelay { get; set; } = 1.5f;
        public float VictoryDelay { get; set; } = 1f;
    }

    internal sealed class FakeScore : IScore
    {
        private readonly ObservableValue<int> _value = new();

        public IObservableValue<int> Value => _value;

        public int Points
        {
            get => _value.Value;
            set => _value.Value = value;
        }
    }

    internal sealed class FakeWaveProgress : IWaveProgress
    {
        public int Cleared { get; set; }
        public int Total { get; set; } = 5;
    }

    internal sealed class RecordingRunEvents : IRunEventSink
    {
        public List<RunResult> Ended { get; } = new();

        void IRunEventSink.Ended(RunResult result) => Ended.Add(result);
    }

    internal sealed class FakeInputSchemes : IInputSchemes
    {
        private readonly ObservableValue<InputScheme> _active = new(InputScheme.None);

        public IObservableValue<InputScheme> Active => _active;
        public List<InputScheme> Activated { get; } = new();

        public void Activate(InputScheme scheme)
        {
            Activated.Add(scheme);
            _active.Value = scheme;
        }
    }
}
