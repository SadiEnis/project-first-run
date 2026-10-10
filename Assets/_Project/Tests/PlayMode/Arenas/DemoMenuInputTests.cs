using NUnit.Framework;
using ProjectFirstRun.Development.Arenas;
using UnityEngine;
using UnityEngine.InputSystem;

namespace ProjectFirstRun.Tests.PlayMode.Arenas
{
    public sealed class DemoMenuInputTests : InputTestFixture
    {
        [Test]
        public void HeldOpeningInputMustBeReleasedBeforeEitherMenuCanSubmit()
        {
            var menu = new DemoMenuInput();
            Assert.That(menu.Tick(true, -1, true, true), Is.EqualTo(-1));
            Assert.That(menu.Tick(true, -1, true, true), Is.EqualTo(-1));
            Assert.That(menu.Selected, Is.Zero);
            Assert.That(menu.IsArmed, Is.False);
            menu.Tick(true, 0, false);
            Assert.That(menu.IsArmed, Is.True);
            Assert.That(menu.Tick(true, 0, true), Is.Zero);
            Assert.That(menu.Tick(true, 0, true), Is.EqualTo(-1), "Held confirm must not repeat.");
            menu.Tick(false, 0, false);
            Assert.That(menu.IsArmed, Is.False);
            Assert.That(menu.Tick(true, 0, true), Is.EqualTo(-1));
        }

        [Test]
        public void AnalogNeedsNeutralBeforeAnotherMoveAndReopeningResetsSelection()
        {
            var menu = new DemoMenuInput();
            menu.Tick(true, 0, false); menu.Tick(true, 0, false);
            menu.Tick(true, -.8f, false);
            Assert.That(menu.Selected, Is.EqualTo(1));
            for (int i = 0; i < 20; i++) menu.Tick(true, -.8f, false);
            Assert.That(menu.Selected, Is.EqualTo(1));
            menu.Tick(true, -.4f, false);
            menu.Tick(true, -.8f, false);
            Assert.That(menu.Selected, Is.EqualTo(1), "Hysteresis must ignore threshold jitter.");
            menu.Tick(true, 0, false); menu.Tick(true, .8f, false);
            Assert.That(menu.Selected, Is.Zero);
            menu.Tick(true, 0, false); menu.Tick(true, -.8f, false);
            menu.Tick(false, 0, false); menu.Tick(true, 0, false);
            Assert.That(menu.Selected, Is.Zero);
        }

        [Test]
        public void GamepadAndKeyboardPollWhileTimeIsStoppedWithoutGameplayActions()
        {
            var pad = InputSystem.AddDevice<Gamepad>();
            var keyboard = InputSystem.AddDevice<Keyboard>();
            var menu = new DemoMenuInput();
            float previous = Time.timeScale;
            try
            {
                Time.timeScale = 0;
                Press(pad.buttonSouth);
                Assert.That(menu.Poll(true), Is.EqualTo(-1));
                Assert.That(menu.Poll(true), Is.EqualTo(-1));
                Release(pad.buttonSouth); menu.Poll(true);
                Press(pad.dpad.down); menu.Poll(true);
                Assert.That(menu.Selected, Is.EqualTo(1));
                Release(pad.dpad.down); menu.Poll(true);
                Press(pad.buttonSouth);
                Assert.That(menu.Poll(true), Is.EqualTo(1));
                Assert.That(menu.Poll(true), Is.EqualTo(-1));
                Release(pad.buttonSouth); menu.Poll(true);
                Press(keyboard.upArrowKey); menu.Poll(true);
                Release(keyboard.upArrowKey); menu.Poll(true);
                Press(keyboard.enterKey);
                Assert.That(menu.Poll(true), Is.Zero);
                Assert.That(menu.Poll(false), Is.EqualTo(-1), "Hidden menus cannot consume a submission.");
            }
            finally { Time.timeScale = previous; }
        }
    }
}
