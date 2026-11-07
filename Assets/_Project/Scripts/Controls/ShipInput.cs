using System;
using System.Collections.Generic;
using Asteroids.Core;
using Zenject;

namespace Asteroids.Controls
{
    /// <summary>The ship's controls over the legacy Input Manager.</summary>
    public sealed class ShipInput : IMovementInput, IWeaponTriggers, IInitializable
    {
        /// <summary>How far a fire axis must be pushed to count as held. Keys and buttons always read 1.</summary>
        public const float TriggerThreshold = 0.5f;

        private readonly List<Axis> _bound = new();
        private readonly Axis _turn;
        private readonly Axis _thrust;
        private readonly IFireInput _fire;
        private readonly IFireInput _altFire;

        public ShipInput(IAxisReader axes)
        {
            if (axes == null) throw new ArgumentNullException(nameof(axes));

            _turn = Bind(axes, ShipInputAxes.Turn);
            _thrust = Bind(axes, ShipInputAxes.Thrust);
            _fire = new AxisTrigger(Bind(axes, ShipInputAxes.Fire));
            _altFire = new AxisTrigger(Bind(axes, ShipInputAxes.AltFire));
        }

        public float Turn => Math.Clamp(_turn.Read(), -1f, 1f);
        public float Thrust => Math.Clamp(_thrust.Read(), 0f, 1f);

        public IFireInput For(WeaponSlot slot) => slot switch
        {
            WeaponSlot.Primary => _fire,
            WeaponSlot.Alternate => _altFire,
            _ => throw new ArgumentOutOfRangeException(nameof(slot), slot, "Unknown weapon slot.")
        };

        /// <summary>
        /// Reads every axis this input reads, once, so a missing Input Manager entry fails at startup.
        /// </summary>
        /// <exception cref="InvalidOperationException">An axis is not defined in the Input Manager.</exception>
        public void Initialize()
        {
            foreach (var axis in _bound)
                axis.Read();
        }

        private Axis Bind(IAxisReader reader, string name)
        {
            var axis = new Axis(reader, name);
            _bound.Add(axis);
            return axis;
        }

        private sealed class Axis
        {
            private readonly IAxisReader _reader;
            private readonly string _name;

            public Axis(IAxisReader reader, string name)
            {
                _reader = reader;
                _name = name;
            }

            public float Read() => _reader.Read(_name);
        }

        private sealed class AxisTrigger : IFireInput
        {
            private readonly Axis _axis;

            public AxisTrigger(Axis axis) => _axis = axis;

            public bool IsFiring => _axis.Read() >= TriggerThreshold;
        }
    }
}