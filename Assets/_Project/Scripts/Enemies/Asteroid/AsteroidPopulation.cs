using System;
using Asteroids.Common;
using Asteroids.Core.IConfigs;
using Asteroids.Pooling;
using UnityEngine;

namespace Asteroids.Enemies
{
    /// <summary>
    /// A pool of <see cref="AsteroidController"/>s, the count of those in play
    /// and the rolling of a new asteroid's heading, speed and spin.
    /// A finished asteroid goes back to the pool by itself. The pool grows on demand, so
    /// <see cref="Launch"/> never fails for lack of an idle asteroid.
    /// </summary>
    internal sealed class AsteroidPopulation
    {
        private readonly IAsteroidTypeConfig _config;
        private readonly IRandom _random;
        private readonly TrackedPool<AsteroidController> _pool;

        /// <param name="createController">Creates one idle asteroid, object included.</param>
        /// <exception cref="ArgumentNullException">Any argument is null.</exception>
        public AsteroidPopulation(
            IAsteroidTypeConfig config, Func<AsteroidController> createController, IRandom random)
        {
            _config = config ?? throw new ArgumentNullException(nameof(config));
            if (createController == null) throw new ArgumentNullException(nameof(createController));
            _random = random ?? throw new ArgumentNullException(nameof(random));

            _pool = new TrackedPool<AsteroidController>(createController, config.PoolMaxIdle);
        }

        public EnemyTypeId Type => _config.Type;

        /// <summary>Asteroids launched and not finished yet.</summary>
        public int AliveCount => _pool.InPlayCount;

        /// <summary>Creates the idle asteroids the config asks for.</summary>
        public void Prewarm() => _pool.Prewarm(_config.PoolPrewarm);

        /// <summary>
        /// Puts an asteroid in play at <paramref name="position"/> with a random heading and a rolled speed and spin.
        /// </summary>
        public void Launch(Vector2 position) => Launch(position, _random.Range(0f, 360f));

        /// <summary>
        /// Puts an asteroid in play at <paramref name="position"/> flying along <paramref name="headingDegrees"/>
        /// (counter-clockwise from +X) with a rolled speed and spin.
        /// </summary>
        public void Launch(Vector2 position, float headingDegrees)
        {
            var launch = Roll(position, headingDegrees);
            _pool.Spawn(launch, static (controller, l) => controller.Launch(l));
        }

        /// <inheritdoc cref="TrackedPool{T}.DespawnAll"/>
        public void DespawnAll() => _pool.DespawnAll();

        private AsteroidLaunch Roll(Vector2 position, float headingDegrees)
        {
            var heading = headingDegrees * Mathf.Deg2Rad;
            var speed = _random.Range(_config.MinSpeed, _config.MaxSpeed);
            var spin = _random.Range(_config.MinSpinSpeed, _config.MaxSpinSpeed);
            if (_random.Range(0, 2) == 0) spin = -spin;
            var angle = _random.Range(0f, 360f);

            var velocity = new Vector2(Mathf.Cos(heading), Mathf.Sin(heading)) * speed;
            return new AsteroidLaunch(position, angle, velocity, spin);
        }
    }
}