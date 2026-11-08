using System;
using Asteroids.Core;
using NUnit.Framework;

namespace Asteroids.Controls.Tests
{
    [TestFixture]
    public sealed class InputSchemesTests
    {
        /// <summary>
        /// Nothing listens until someone decides. The session binder activates Gameplay on startup; if it is missing,
        /// a deaf ship is easier to notice than a menu reacting to gameplay keys.
        /// </summary>
        [Test]
        public void New_IsNone() => Assert.That(new InputSchemes().Active.Value, Is.EqualTo(InputScheme.None));

        [Test]
        public void Activate_ChangesAndNotifies()
        {
            var schemes = new InputSchemes();
            InputScheme? seen = null;
            schemes.Active.Subscribe(scheme => seen = scheme);

            schemes.Activate(InputScheme.UI);

            Assert.That(schemes.Active.Value, Is.EqualTo(InputScheme.UI));
            Assert.That(seen, Is.EqualTo(InputScheme.UI));
        }

        [Test]
        public void Activate_UndefinedScheme_ThrowsAndKeepsTheCurrentOne()
        {
            var schemes = new InputSchemes();
            schemes.Activate(InputScheme.Gameplay);

            Assert.Throws<ArgumentOutOfRangeException>(() => schemes.Activate((InputScheme)42));
            Assert.That(schemes.Active.Value, Is.EqualTo(InputScheme.Gameplay));
        }
    }
}
