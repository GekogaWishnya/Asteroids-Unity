using System;
using NUnit.Framework;

namespace Asteroids.Controls.Tests
{
    /// <summary>
    /// Guards the real wiring: every axis <see cref="ShipInput"/> reads must exist in this project's
    /// Input Manager. If someone renames or deletes an entry in Project Settings, this fails in the editor
    /// instead of the game throwing at startup. It runs the same startup check the game runs, so it covers
    /// new axes without being edited.
    /// </summary>
    [TestFixture]
    public sealed class InputManagerAxesTests
    {
        [Test]
        public void EveryShipAxis_IsDefinedInInputManager()
        {
            var input = new ShipInput(new UnityAxisReader());

            // Outside play mode Unity returns 0 for a defined axis and throws for an unknown one.
            Assert.DoesNotThrow(input.Initialize);
        }

        [Test]
        public void UnknownAxis_IsReportedWithAHint()
        {
            var reader = new UnityAxisReader();

            var error = Assert.Throws<InvalidOperationException>(() => reader.Read("Definitely Not An Axis"));
            StringAssert.Contains("Input Manager", error.Message);
        }
    }
}
