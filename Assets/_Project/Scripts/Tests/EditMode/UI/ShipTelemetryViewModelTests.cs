using System;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;

namespace Asteroids.UI.Tests
{
    /// <summary>
    /// The view model's behaviour: rounding to a tenth, whole degrees in [0, 360), the no-ship texts and
    /// notifying only when a shown value changes. The wording comes from <see cref="TestTelemetryHudConfig"/>,
    /// never from the shipped <c>HudConfig</c>, so editing the HUD text in the inspector never breaks these tests.
    /// </summary>
    [TestFixture]
    public sealed class ShipTelemetryViewModelTests
    {
        private static ShipTelemetryViewModel Create(FakeShipTelemetry telemetry) =>
            new(telemetry, new TestTelemetryHudConfig());

        [Test]
        public void PositionText_Initially_ShowsXAndYToATenth()
        {
            var viewModel = Create(FakeShipTelemetry.At(new Vector2(12.34f, -4f)));

            Assert.That(viewModel.PositionText.Value, Is.EqualTo("12.3|-4.0"));
        }

        [Test]
        public void PositionText_HalfATenth_RoundsAwayFromZero()
        {
            var viewModel = Create(FakeShipTelemetry.At(new Vector2(0.25f, -0.25f)));

            Assert.That(viewModel.PositionText.Value, Is.EqualTo("0.3|-0.3"));
        }

        [Test]
        public void PositionText_TinyNegative_ShowsZeroWithoutSign()
        {
            var viewModel = Create(FakeShipTelemetry.At(new Vector2(-0.04f, -0.01f)));

            Assert.That(viewModel.PositionText.Value, Is.EqualTo("0.0|0.0"));
        }

        /// <summary>The config picks the wording, not the precision: values reach the format already rounded to a tenth.</summary>
        [Test]
        public void Texts_FinerFormat_StillShowValuesRoundedToATenth()
        {
            var config = new TestTelemetryHudConfig { PositionFormat = "{0:0.00}|{1:0.00}", SpeedFormat = "{0:0.00}" };
            var viewModel = new ShipTelemetryViewModel(
                FakeShipTelemetry.At(new Vector2(1.23f, -4.56f), speed: 7.89f), config);

            Assert.That(viewModel.PositionText.Value, Is.EqualTo("1.20|-4.60"));
            Assert.That(viewModel.SpeedText.Value, Is.EqualTo("7.90"));
        }

        [Test]
        public void Texts_UseTheConfiguredFormats()
        {
            var config = new TestTelemetryHudConfig
            {
                PositionFormat = "pos {0:0.0} {1:0.0}",
                AngleFormat = "rot {0}",
                SpeedFormat = "vel {0:0.0}"
            };
            var viewModel = new ShipTelemetryViewModel(
                FakeShipTelemetry.At(new Vector2(1f, 2f), angle: 30f, speed: 4f), config);

            Assert.That(viewModel.PositionText.Value, Is.EqualTo("pos 1.0 2.0"));
            Assert.That(viewModel.AngleText.Value, Is.EqualTo("rot 30"));
            Assert.That(viewModel.SpeedText.Value, Is.EqualTo("vel 4.0"));
        }

        [Test]
        public void AngleText_Initially_ShowsWholeDegrees()
        {
            var viewModel = Create(FakeShipTelemetry.At(Vector2.zero, angle: 89.6f));

            Assert.That(viewModel.AngleText.Value, Is.EqualTo("90"));
        }

        [TestCase(359.6f, "0")]
        [TestCase(360f, "0")]
        [TestCase(-90f, "270")]
        [TestCase(725f, "5")]
        public void AngleText_OutsideAFullTurn_IsShownIn0To359(float angle, string expected)
        {
            var viewModel = Create(FakeShipTelemetry.At(Vector2.zero, angle: angle));

            Assert.That(viewModel.AngleText.Value, Is.EqualTo(expected));
        }

        [Test]
        public void SpeedText_Initially_ShowsTheSpeedToATenth()
        {
            var viewModel = Create(FakeShipTelemetry.At(Vector2.zero, speed: 3.24f));

            Assert.That(viewModel.SpeedText.Value, Is.EqualTo("3.2"));
        }

        [Test]
        public void SpeedText_Negative_IsShownAsZero()
        {
            var viewModel = Create(FakeShipTelemetry.At(Vector2.zero, speed: -1f));

            Assert.That(viewModel.SpeedText.Value, Is.EqualTo("0.0"));
        }

        [Test]
        public void Texts_NoShip_ShowTheNoShipTexts()
        {
            var viewModel = Create(new FakeShipTelemetry());

            Assert.That(viewModel.PositionText.Value, Is.EqualTo("no position"));
            Assert.That(viewModel.AngleText.Value, Is.EqualTo("no angle"));
            Assert.That(viewModel.SpeedText.Value, Is.EqualTo("no speed"));
        }

        [Test]
        [SetCulture("de-DE")]
        public void Texts_UnderACommaCulture_UseADot()
        {
            var viewModel = Create(FakeShipTelemetry.At(new Vector2(1.5f, -2.5f), speed: 3.5f));

            Assert.That(viewModel.PositionText.Value, Is.EqualTo("1.5|-2.5"));
            Assert.That(viewModel.SpeedText.Value, Is.EqualTo("3.5"));
        }

        [Test]
        public void Refresh_TelemetryChanged_FollowsIt()
        {
            var telemetry = FakeShipTelemetry.At(Vector2.zero);
            var viewModel = Create(telemetry);

            telemetry.Position = new Vector2(-7.5f, 3.21f);
            telemetry.Angle = 180f;
            telemetry.Speed = 16.5f;
            viewModel.Refresh();

            Assert.That(viewModel.PositionText.Value, Is.EqualTo("-7.5|3.2"));
            Assert.That(viewModel.AngleText.Value, Is.EqualTo("180"));
            Assert.That(viewModel.SpeedText.Value, Is.EqualTo("16.5"));
        }

        [Test]
        public void PositionText_TelemetryChangedWithoutRefresh_KeepsTheShownText()
        {
            var telemetry = FakeShipTelemetry.At(Vector2.zero);
            var viewModel = Create(telemetry);

            telemetry.Position = new Vector2(5f, 5f);

            Assert.That(viewModel.PositionText.Value, Is.EqualTo("0.0|0.0"));
        }

        [Test]
        public void Refresh_ChangeBelowTheShownPrecision_DoesNotNotify()
        {
            var telemetry = FakeShipTelemetry.At(new Vector2(1.01f, 2.01f), angle: 10.1f, speed: 3.01f);
            var viewModel = Create(telemetry);
            var changes = new List<string>();
            using var position = viewModel.PositionText.Subscribe(changes.Add);
            using var angle = viewModel.AngleText.Subscribe(changes.Add);
            using var speed = viewModel.SpeedText.Subscribe(changes.Add);
            changes.Clear();

            telemetry.Position = new Vector2(1.04f, 1.96f);
            telemetry.Angle = 10.4f;
            telemetry.Speed = 3.04f;
            viewModel.Refresh();

            Assert.That(changes, Is.Empty);
        }

        [Test]
        public void Refresh_OnlyOneValueChanged_NotifiesOnlyItsText()
        {
            var telemetry = FakeShipTelemetry.At(Vector2.zero, angle: 0f, speed: 0f);
            var viewModel = Create(telemetry);
            var positions = 0;
            var angles = 0;
            var speeds = 0;
            using var position = viewModel.PositionText.Subscribe(_ => positions++);
            using var angle = viewModel.AngleText.Subscribe(_ => angles++);
            using var speed = viewModel.SpeedText.Subscribe(_ => speeds++);

            telemetry.Speed = 2f;
            viewModel.Refresh();

            Assert.That(positions, Is.EqualTo(1));
            Assert.That(angles, Is.EqualTo(1));
            Assert.That(speeds, Is.EqualTo(2));
        }

        [Test]
        public void Refresh_ShipLost_ShowsTheNoShipTexts()
        {
            var telemetry = FakeShipTelemetry.At(Vector2.one, angle: 45f, speed: 1f);
            var viewModel = Create(telemetry);

            telemetry.HasShip = false;
            viewModel.Refresh();

            Assert.That(viewModel.PositionText.Value, Is.EqualTo("no position"));
            Assert.That(viewModel.AngleText.Value, Is.EqualTo("no angle"));
            Assert.That(viewModel.SpeedText.Value, Is.EqualTo("no speed"));
        }

        [Test]
        public void Refresh_ShipAppearsAtZero_ShowsTheValues()
        {
            var telemetry = new FakeShipTelemetry();
            var viewModel = Create(telemetry);

            telemetry.HasShip = true;
            viewModel.Refresh();

            Assert.That(viewModel.PositionText.Value, Is.EqualTo("0.0|0.0"));
            Assert.That(viewModel.AngleText.Value, Is.EqualTo("0"));
            Assert.That(viewModel.SpeedText.Value, Is.EqualTo("0.0"));
        }

        [Test]
        public void Construct_NullTelemetry_Throws() =>
            Assert.Throws<ArgumentNullException>(() => new ShipTelemetryViewModel(null, new TestTelemetryHudConfig()));

        [Test]
        public void Construct_NullConfig_Throws() =>
            Assert.Throws<ArgumentNullException>(() =>
                new ShipTelemetryViewModel(FakeShipTelemetry.At(Vector2.zero), null));

        /// <summary>A broken format fails at startup with the validator's message, not mid-game in <c>Refresh</c>.</summary>
        [Test]
        public void Construct_InvalidConfig_Throws()
        {
            var config = new TestTelemetryHudConfig { PositionFormat = "{0} {2}" };

            var error = Assert.Throws<InvalidOperationException>(() =>
                new ShipTelemetryViewModel(FakeShipTelemetry.At(Vector2.zero), config));
            StringAssert.Contains("Position format", error.Message);
        }
    }
}
