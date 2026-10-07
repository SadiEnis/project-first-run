using NUnit.Framework;
using ProjectFirstRun.Input;
using ProjectFirstRun.Player;
using UnityEngine;
using UnityEngine.InputSystem;

namespace ProjectFirstRun.Tests.PlayMode.Player
{
    public sealed class PlayerJumpTests : InputTestFixture
    {
        private GameObject _player;
        private GameObject _floor;
        private GameObject _ceiling;
        private PlayerMotor _motor;
        private const float Step = 1f / 60f;

        [SetUp]
        public void CreatePlayer()
        {
            _floor = GameObject.CreatePrimitive(PrimitiveType.Cube);
            _floor.transform.position = new Vector3(1000, -0.5f, 1000);
            _floor.transform.localScale = new Vector3(20, 1, 20);
            _player = new GameObject("Jump test player");
            _player.transform.position = new Vector3(1000, 0.1f, 1000);
            var capsule = _player.AddComponent<CharacterController>();
            capsule.height = 2;
            capsule.radius = 0.4f;
            capsule.center = Vector3.up;
            _motor = _player.AddComponent<PlayerMotor>();
            Physics.SyncTransforms();
            for (int i = 0; i < 30; i++) _motor.Tick(Vector2.zero, false, Step);
            Assert.That(_motor.IsGrounded, Is.True);
        }

        [TearDown]
        public void DestroyPlayer()
        {
            Object.DestroyImmediate(_player);
            Object.DestroyImmediate(_floor);
            if (_ceiling != null) Object.DestroyImmediate(_ceiling);
            Time.timeScale = 1;
        }

        [Test]
        public void GroundedPressJumps_AirbornePressCannotRestart_LandingAllowsNextJump()
        {
            float start = _player.transform.position.y;
            _motor.Tick(Vector2.zero, false, Step, true);
            float launchSpeed = _motor.VerticalVelocity;
            Assert.That(launchSpeed, Is.GreaterThan(0));
            Assert.That(_player.transform.position.y, Is.GreaterThan(start));
            _motor.Tick(Vector2.zero, false, Step, true);
            Assert.That(_motor.VerticalVelocity, Is.LessThan(launchSpeed));
            float peak = _player.transform.position.y;
            for (int i = 0; i < 120; i++)
            {
                _motor.Tick(Vector2.zero, false, Step);
                peak = Mathf.Max(peak, _player.transform.position.y);
            }
            Assert.That(peak - start, Is.InRange(1.1f, 1.35f));
            Assert.That(_motor.IsGrounded, Is.True);
            _motor.Tick(Vector2.zero, false, Step, true);
            Assert.That(_motor.VerticalVelocity, Is.GreaterThan(0));
        }

        [Test]
        public void CeilingCancelsUpwardVelocity()
        {
            _ceiling = GameObject.CreatePrimitive(PrimitiveType.Cube);
            _ceiling.transform.position = new Vector3(1000, 2.65f, 1000);
            _ceiling.transform.localScale = new Vector3(10, 0.5f, 10);
            Physics.SyncTransforms();
            _motor.Tick(Vector2.zero, false, Step, true);
            for (int i = 0; i < 20 && _motor.VerticalVelocity > 0; i++)
                _motor.Tick(Vector2.zero, false, Step);
            Assert.That(_motor.VerticalVelocity, Is.EqualTo(0));
        }

        [Test]
        public void ZeroDeltaCannotJump_AndTeleportClearsVerticalVelocity()
        {
            float before = _player.transform.position.y;
            _motor.Tick(Vector2.zero, false, 0, true);
            Assert.That(_player.transform.position.y, Is.EqualTo(before));
            _motor.Tick(Vector2.zero, false, Step, true);
            _motor.Teleport(new Vector3(1000, 3, 1000), Quaternion.identity);
            Assert.That(_motor.VerticalVelocity, Is.Zero);
            _motor.Tick(Vector2.zero, false, Step, true);
            Assert.That(_motor.VerticalVelocity, Is.LessThan(0));
        }

        [Test]
        public void JumpPressIsConsumedOnce_AndHeldInputDoesNotRepeat()
        {
            var keyboard = InputSystem.AddDevice<Keyboard>();
            var reader = _player.AddComponent<PlayerInputReader>();
            reader.SendMessage("Update");
            Press(keyboard.spaceKey);
            Assert.That(reader.ConsumeJumpPress(true), Is.True);
            Assert.That(reader.ConsumeJumpPress(true), Is.False);
            InputSystem.Update();
            Assert.That(reader.ConsumeJumpPress(true), Is.False);
            Release(keyboard.spaceKey);
            Press(keyboard.spaceKey);
            Assert.That(reader.ConsumeJumpPress(true), Is.True);
        }

        [Test]
        public void ControlLockDiscardsPress_AndRequiresReleaseAfterUnlock()
        {
            var keyboard = InputSystem.AddDevice<Keyboard>();
            var reader = _player.AddComponent<PlayerInputReader>();
            reader.SendMessage("Update");
            Press(keyboard.spaceKey);
            Assert.That(reader.ConsumeJumpPress(false), Is.False);
            Assert.That(reader.ConsumeJumpPress(true), Is.False);
            reader.SetGameplayInputEnabled(false);
            reader.SetGameplayInputEnabled(true);
            InputSystem.Update();
            reader.SendMessage("Update");
            Assert.That(reader.ConsumeJumpPress(true), Is.False);
            Release(keyboard.spaceKey);
            reader.SendMessage("Update");
            Press(keyboard.spaceKey);
            Assert.That(reader.ConsumeJumpPress(true), Is.True);
        }

        [Test]
        public void PausePressDoesNotSurviveResume()
        {
            var keyboard = InputSystem.AddDevice<Keyboard>();
            var reader = _player.AddComponent<PlayerInputReader>();
            reader.SendMessage("Update");
            Time.timeScale = 0;
            Press(keyboard.spaceKey);
            Time.timeScale = 1;
            Assert.That(reader.ConsumeJumpPress(true), Is.False);
        }

        [Test]
        public void GamepadSouthJumps_RightShoulderInteracts_WithoutOverlap()
        {
            var pad = InputSystem.AddDevice<Gamepad>();
            var reader = _player.AddComponent<PlayerInputReader>();
            reader.SendMessage("Update");
            Press(pad.buttonSouth);
            Assert.That(reader.ConsumeJumpPress(true), Is.True);
            Assert.That(reader.WasInteractPressedThisFrame, Is.False);
            Release(pad.buttonSouth);
            InputSystem.Update();
            Press(pad.rightShoulder);
            Assert.That(reader.WasInteractPressedThisFrame, Is.True);
            Assert.That(reader.ConsumeJumpPress(true), Is.False);
        }
    }
}
