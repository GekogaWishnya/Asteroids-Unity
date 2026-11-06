using Asteroids.Core.IConfigs;
using NUnit.Framework;
using UnityEngine;

namespace Asteroids.Physics.Tests
{
    [TestFixture]
    public sealed class InertialMotionTests
    {
        private sealed class Config : IMotionConfig
        {
            public float MaxSpeed => 100f;
            public float LinearDamping => 0f;
        }

        [Test]
        public void Stop_DropsTheVelocityAndThePendingAcceleration()
        {
            var motion = new InertialMotion(new Config());
            motion.AddVelocity(new Vector2(3f, 4f));
            motion.Accelerate(new Vector2(10f, 0f));

            motion.Stop();
            var displacement = motion.Step(1f);

            Assert.That(motion.Velocity, Is.EqualTo(Vector2.zero));
            Assert.That(displacement, Is.EqualTo(Vector2.zero));
        }
    }
}
