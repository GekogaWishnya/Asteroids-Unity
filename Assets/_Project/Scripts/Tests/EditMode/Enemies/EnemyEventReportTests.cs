using System;
using Asteroids.Core.IConfigs;
using NUnit.Framework;
using UnityEngine;

namespace Asteroids.Enemies.Tests
{
    [TestFixture]
    public sealed class EnemyEventReportTests
    {
        private static readonly EnemyTypeId Ufo = new("Ufo");

        [Test]
        public void Constructors_InvalidArguments_Throw()
        {
            var events = new RecordingEnemyEvents();

            Assert.Throws<ArgumentNullException>(() => new ReportDamage(null, Ufo));
            Assert.Throws<ArgumentException>(() => new ReportDamage(events, default));
            Assert.Throws<ArgumentNullException>(() => new ReportDeath(null, Ufo));
            Assert.Throws<ArgumentException>(() => new ReportDeath(events, default));
        }

        [Test]
        public void ReportDamage_PassesTypePositionAndFatality()
        {
            var events = new RecordingEnemyEvents();

            new ReportDamage(events, Ufo).Damaged(new Vector2(1f, 2f), true);

            Assert.That(events.Damaged, Is.EqualTo(new[] { (Ufo, new Vector2(1f, 2f), true) }));
            Assert.That(events.Died, Is.Empty);
        }

        [Test]
        public void ReportDeath_PassesTypeAndPosition()
        {
            var events = new RecordingEnemyEvents();

            new ReportDeath(events, Ufo).Died(new Vector2(3f, 4f));

            Assert.That(events.Died, Is.EqualTo(new[] { (Ufo, new Vector2(3f, 4f)) }));
            Assert.That(events.Damaged, Is.Empty);
        }
    }
}
