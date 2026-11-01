using System;
using System.Collections.Generic;
using Asteroids.Core;
using NUnit.Framework;
using UnityEngine;

namespace Asteroids.Physics.Tests
{
    [TestFixture]
    public sealed class StunOnKnockbackTests
    {
        [Test]
        public void Constructor_InvalidArguments_Throw()
        {
            var body = new StubKineticBody();
            var owner = new RecordingStunnable();

            Assert.Throws<ArgumentNullException>(() => new StunOnKnockback(null, owner, 1f));
            Assert.Throws<ArgumentNullException>(() => new StunOnKnockback(body, null, 1f));
            Assert.Throws<ArgumentOutOfRangeException>(() => new StunOnKnockback(body, owner, -1f));
            Assert.Throws<ArgumentOutOfRangeException>(() => new StunOnKnockback(body, owner, float.NaN));
        }

        [Test]
        public void VelocityAndMinKnockbackSpeed_ComeFromTheBody()
        {
            var body = new StubKineticBody { Velocity = new Vector2(1f, 2f), MinKnockbackSpeed = 3f };
            var stun = new StunOnKnockback(body, new RecordingStunnable(), 1f);

            Assert.That(stun.Velocity, Is.EqualTo(new Vector2(1f, 2f)));
            Assert.That(stun.MinKnockbackSpeed, Is.EqualTo(3f));
        }

        [Test]
        public void AddVelocity_KnocksTheBodyAndStunsTheOwner()
        {
            var body = new StubKineticBody { Velocity = new Vector2(1f, 0f) };
            var owner = new RecordingStunnable();
            var stun = new StunOnKnockback(body, owner, 1.5f);

            stun.AddVelocity(new Vector2(-3f, 1f));

            Assert.That(body.Velocity, Is.EqualTo(new Vector2(-2f, 1f)));
            Assert.That(owner.Stuns, Is.EqualTo(new[] { 1.5f }));
        }
    }
    
    internal sealed class RecordingStunnable : IStunnable
    {
        public List<float> Stuns { get; } = new();

        public void Stun(float seconds) => Stuns.Add(seconds);
    }

    internal sealed class StubKineticBody : IKineticBody
    {
        public Vector2 Velocity { get; set; }
        public float MinKnockbackSpeed { get; set; }

        public void AddVelocity(Vector2 delta) => Velocity += delta;
    }
}
