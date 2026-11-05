using System;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;

namespace Asteroids.UI.Tests
{
    [TestFixture]
    public sealed class ShipTelemetryViewModelTests
    {
        [Test]
        public void PositionText_Initially_ShowsXAndYToATenth()
        {
            var viewModel = new ShipTelemetryViewModel(FakeShipTelemetry.At(new Vector2(12.34f, -4f)));

            Assert.That(viewModel.PositionText.Value, Is.EqualTo("X 12.3  Y -4.0"));
        }

        [Test]
        public void PositionText_HalfATenth_RoundsAwayFromZero()
        {
            var viewModel = new ShipTelemetryViewModel(FakeShipTelemetry.At(new Vector2(0.25f, -0.25f)));

            Assert.That(viewModel.PositionText.Value, Is.EqualTo("X 0.3  Y -0.3"));
        }

        [Test]
        public void PositionText_TinyNegative_ShowsZeroWithoutSign()
        {
            var viewModel = new ShipTelemetryViewModel(FakeShipTelemetry.At(new Vector2(-0.04f, -0.01f)));

            Assert.That(viewModel.PositionText.Value, Is.EqualTo("X 0.0  Y 0.0"));
        }

        [Test]
        public void AngleText_Initially_ShowsWholeDegrees()
        {
            var viewModel = new ShipTelemetryViewModel(FakeShipTelemetry.At(Vector2.zero, angle: 89.6f));

            Assert.That(viewModel.AngleText.Value, Is.EqualTo("ANGLE 90°"));
        }

        [TestCase(359.6f, "ANGLE 0°")]
        [TestCase(360f, "ANGLE 0°")]
        [TestCase(-90f, "ANGLE 270°")]
        [TestCase(725f, "ANGLE 5°")]
        public void AngleText_OutsideAFullTurn_IsShownIn0To359(float angle, string expected)
        {
            var viewModel = new ShipTelemetryViewModel(FakeShipTelemetry.At(Vector2.zero, angle: angle));

            Assert.That(viewModel.AngleText.Value, Is.EqualTo(expected));
        }

        [Test]
        public void SpeedText_Initially_ShowsTheSpeedToATenth()
        {
            var viewModel = new ShipTelemetryViewModel(FakeShipTelemetry.At(Vector2.zero, speed: 3.24f));

            Assert.That(viewModel.SpeedText.Value, Is.EqualTo("SPEED 3.2"));
        }

        [Test]
        public void SpeedText_Negative_IsShownAsZero()
        {
            var viewModel = new ShipTelemetryViewModel(FakeShipTelemetry.At(Vector2.zero, speed: -1f));

            Assert.That(viewModel.SpeedText.Value, Is.EqualTo("SPEED 0.0"));
        }

        [Test]
        public void Texts_NoShip_ShowDashes()
        {
            var viewModel = new ShipTelemetryViewModel(new FakeShipTelemetry());

            Assert.That(viewModel.PositionText.Value, Is.EqualTo("X --  Y --"));
            Assert.That(viewModel.AngleText.Value, Is.EqualTo("ANGLE --"));
            Assert.That(viewModel.SpeedText.Value, Is.EqualTo("SPEED --"));
        }

        [Test]
        [SetCulture("de-DE")]
        public void Texts_UnderACommaCulture_UseADot()
        {
            var viewModel = new ShipTelemetryViewModel(FakeShipTelemetry.At(new Vector2(1.5f, -2.5f), speed: 3.5f));

            Assert.That(viewModel.PositionText.Value, Is.EqualTo("X 1.5  Y -2.5"));
            Assert.That(viewModel.SpeedText.Value, Is.EqualTo("SPEED 3.5"));
        }

        [Test]
        public void Refresh_TelemetryChanged_FollowsIt()
        {
            var telemetry = FakeShipTelemetry.At(Vector2.zero);
            var viewModel = new ShipTelemetryViewModel(telemetry);

            telemetry.Position = new Vector2(-7.5f, 3.21f);
            telemetry.Angle = 180f;
            telemetry.Speed = 16.5f;
            viewModel.Refresh();

            Assert.That(viewModel.PositionText.Value, Is.EqualTo("X -7.5  Y 3.2"));
            Assert.That(viewModel.AngleText.Value, Is.EqualTo("ANGLE 180°"));
            Assert.That(viewModel.SpeedText.Value, Is.EqualTo("SPEED 16.5"));
        }

        [Test]
        public void PositionText_TelemetryChangedWithoutRefresh_KeepsTheShownText()
        {
            var telemetry = FakeShipTelemetry.At(Vector2.zero);
            var viewModel = new ShipTelemetryViewModel(telemetry);

            telemetry.Position = new Vector2(5f, 5f);

            Assert.That(viewModel.PositionText.Value, Is.EqualTo("X 0.0  Y 0.0"));
        }

        [Test]
        public void Refresh_ChangeBelowTheShownPrecision_DoesNotNotify()
        {
            var telemetry = FakeShipTelemetry.At(new Vector2(1.01f, 2.01f), angle: 10.1f, speed: 3.01f);
            var viewModel = new ShipTelemetryViewModel(telemetry);
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
            var viewModel = new ShipTelemetryViewModel(telemetry);
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
        public void Refresh_ShipLost_ShowsDashes()
        {
            var telemetry = FakeShipTelemetry.At(Vector2.one, angle: 45f, speed: 1f);
            var viewModel = new ShipTelemetryViewModel(telemetry);

            telemetry.HasShip = false;
            viewModel.Refresh();

            Assert.That(viewModel.PositionText.Value, Is.EqualTo("X --  Y --"));
            Assert.That(viewModel.AngleText.Value, Is.EqualTo("ANGLE --"));
            Assert.That(viewModel.SpeedText.Value, Is.EqualTo("SPEED --"));
        }

        [Test]
        public void Refresh_ShipAppearsAtZero_ShowsTheValues()
        {
            var telemetry = new FakeShipTelemetry();
            var viewModel = new ShipTelemetryViewModel(telemetry);

            telemetry.HasShip = true;
            viewModel.Refresh();

            Assert.That(viewModel.PositionText.Value, Is.EqualTo("X 0.0  Y 0.0"));
            Assert.That(viewModel.AngleText.Value, Is.EqualTo("ANGLE 0°"));
            Assert.That(viewModel.SpeedText.Value, Is.EqualTo("SPEED 0.0"));
        }

        [Test]
        public void Construct_Null_Throws() =>
            Assert.Throws<ArgumentNullException>(() => new ShipTelemetryViewModel(null));
    }
}