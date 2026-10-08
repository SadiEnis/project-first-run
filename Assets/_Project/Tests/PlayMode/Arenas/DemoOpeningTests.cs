#if UNITY_EDITOR
using System.Collections;
using System.Linq;
using NUnit.Framework;
using ProjectFirstRun.Arenas;
using ProjectFirstRun.Chests;
using ProjectFirstRun.Combat;
using ProjectFirstRun.Development.Arenas;
using ProjectFirstRun.Player;
using ProjectFirstRun.Enemies;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

namespace ProjectFirstRun.Tests.PlayMode.Arenas
{
    public sealed class DemoOpeningTests : InputTestFixture
    {
        private Scene _scene, _original;
        private DemoOpeningController _demo;
        private const string Path = "Assets/_Project/Scenes/Demo/DeepJam_Opening.unity";
        [SetUp] public override void Setup() { }
        [TearDown] public override void TearDown() { }

        [UnitySetUp]
        public IEnumerator Load()
        {
            base.Setup();
            InputSystem.AddDevice<Keyboard>(); InputSystem.AddDevice<Mouse>();
            _original = SceneManager.GetActiveScene();
            if (AssetDatabase.LoadAssetAtPath<SceneAsset>(Path) == null)
                Assert.Ignore("Create Opening Greybox before running demo scene acceptance.");
            Time.timeScale = 1;
            yield return EditorSceneManager.LoadSceneAsyncInPlayMode(Path, new LoadSceneParameters(LoadSceneMode.Additive));
            _scene = SceneManager.GetSceneByPath(Path);
            SceneManager.SetActiveScene(_scene);
            _demo = _scene.GetRootGameObjects().SelectMany(x => x.GetComponentsInChildren<DemoOpeningController>()).Single();
            for (int i = 0; i < 300 && (!_demo.IsReady || !_demo.Encounter.IsReadyForPassage); i++) yield return null;
            Assert.That(_demo.IsReady, Is.True);
            Assert.That(_demo.Encounter.IsReadyForPassage, Is.True, _demo.Encounter.LastError?.ToString());
        }

        [UnityTest]
        public IEnumerator StartIsSafeAndEnteringCorridorActivatesThePreparedEnemies()
        {
            Assert.That(_demo.Encounter.Status, Is.EqualTo(ArenaSessionStatus.Ready));
            Assert.That(_demo.Encounter.Enemies.Count, Is.EqualTo(4));
            Assert.That(_demo.Reward, Is.Null);
            var body = _demo.Health.GetComponent<CharacterController>();
            foreach (var enemy in _demo.Encounter.Enemies)
            {
                Assert.That(enemy.RequiresPerception, Is.True);
                enemy.Perception.Tick(.2f, true);
                Assert.That(enemy.Perception.State.Awareness, Is.EqualTo(EnemyAwareness.Idle));
                Assert.That(enemy.GetComponent<EnemyMotor>().IsMovementEnabled, Is.False);
            }
            body.enabled = false;
            _demo.Health.transform.position = new Vector3(2, .1f, 12);
            body.enabled = true;
            for (int i = 0; i < 60 && _demo.Encounter.Status != ArenaSessionStatus.Running; i++)
                yield return new WaitForFixedUpdate();
            Assert.That(_demo.Encounter.Status, Is.EqualTo(ArenaSessionStatus.Running));
            // Readiness/activation does not imply sight; allow a sampling interval first.
            for (int i = 0; i < 60 && !_demo.Encounter.Enemies.Any(x => x.Perception.HasSight); i++) yield return null;
            Assert.That(_demo.Encounter.Enemies.Any(x => x.Perception.HasSight), Is.True,
                "An activated enemy facing the exposed player should detect them.");
            foreach (var enemy in _demo.Encounter.Enemies.ToArray())
                enemy.Health.ApplyDamage(new DamageInfo(10000, _demo.Health.gameObject, enemy.transform.position, Vector3.forward));
            yield return null;
            Assert.That(_demo.Encounter.Status, Is.EqualTo(ArenaSessionStatus.Victory));
            Assert.That(_demo.Reward, Is.Not.Null);
            var reward = _demo.Reward;
            Assert.That(reward.Status, Is.EqualTo(ChestStatus.Available));
            _demo.Encounter.SetPlayerInside(false); _demo.Encounter.SetPlayerInside(true);
            yield return null;
            Assert.That(_demo.Reward, Is.SameAs(reward), "Re-entry must not create another guaranteed reward.");
            Assert.That(_demo.RewardClaimed, Is.False, "Spawning a chest is not claiming its reward.");
        }

        [UnityTest]
        public IEnumerator DeathBeforeCombatCancelsEncounterWithoutGrantingReward()
        {
            _demo.Health.ApplyDamage(new DamageInfo(10000, null, _demo.Health.transform.position, Vector3.zero));
            yield return null;
            Assert.That(_demo.Health.IsDead, Is.True);
            Assert.That(_demo.Encounter.Status, Is.EqualTo(ArenaSessionStatus.Defeat));
            Assert.That(_demo.Reward, Is.Null);
        }

        [UnityTest]
        public IEnumerator ExpandedMapCanBeWalkedDownAndBackUpWithoutJumpOrTeleport()
        {
            // Isolate geometry acceptance from combat. Teleport only once to the route's starting point.
            foreach (var encounter in AllEncounters()) encounter.Cancel();
            var player = _demo.Health.gameObject;
            player.GetComponent<PlayerController>().enabled = false;
            var motor = player.GetComponent<PlayerMotor>();
            var checkpoints = _scene.GetRootGameObjects().Single(x => x.name == "Authored demo geometry")
                .transform.Find("Traversal checkpoints");
            Vector3 Point(string name) => checkpoints.Find(name).position;
            motor.Teleport(Point("DescentTop") + Vector3.up * .05f, Quaternion.identity);
            Physics.SyncTransforms();
            foreach (string name in new[] {
                "DescentBottom", "RewardExit", "SecondRoom", "Overlook",
                "LeftStairTop", "LeftStairBottom", "LeftStairTop", "Overlook",
                "RightStairTop", "RightStairBottom", "RightStairTop", "Overlook",
                "SecondRoom", "RewardExit", "DescentBottom", "DescentTop" })
            {
                Vector3 target = Point(name);
                bool reached = false;
                for (int step = 0; step < 500; step++)
                {
                    Vector3 delta = target - player.transform.position;
                    delta.y = 0;
                    if (delta.magnitude < .2f && Mathf.Abs(player.transform.position.y - target.y) < .25f)
                    { reached = true; break; }
                    if (delta.sqrMagnitude > .0001f)
                        player.transform.rotation = Quaternion.LookRotation(delta);
                    motor.Tick(delta.magnitude < .05f ? Vector2.zero : Vector2.up, false, .02f);
                    Assert.That(player.transform.position.y, Is.GreaterThan(-9), "Fell outside the layout near " + name);
                    // Simulate several deterministic movement ticks per frame, without changing global time scale.
                    if (step % 5 == 0) yield return null;
                }
                Assert.That(reached, Is.True, "Could not walk to " + name + "; position=" + player.transform.position);
            }
        }

        private PreparedRegionEncounter[] AllEncounters() => _scene.GetRootGameObjects()
            .SelectMany(x => x.GetComponentsInChildren<PreparedRegionEncounter>()).ToArray();

        private EncounterChestReward OptionalReward() => _scene.GetRootGameObjects()
            .SelectMany(x => x.GetComponentsInChildren<EncounterChestReward>()).Single();

        private void MovePlayer(Vector3 position)
        {
            var body = _demo.Health.GetComponent<CharacterController>();
            body.enabled = false; _demo.Health.transform.position = position; body.enabled = true;
            Physics.SyncTransforms();
        }

        private IEnumerator ActivateRooms()
        {
            MovePlayer(new Vector3(0, -2.9f, 54));
            var rooms = AllEncounters().Where(x => x != _demo.Encounter).ToArray();
            for (int i = 0; i < 180 && rooms.Any(x => x.Status != ArenaSessionStatus.Running); i++)
                yield return null;
            foreach (var room in rooms)
            {
                Assert.That(room.LastError, Is.Null);
                Assert.That(room.Status, Is.EqualTo(ArenaSessionStatus.Running), room.name);
                Assert.That(room.Enemies.Count, Is.EqualTo(3));
            }
        }

        [UnityTest]
        public IEnumerator RoomPreparationPrecedesActivationAndReentryDoesNotRespawn()
        {
            var rooms = AllEncounters().Where(x => x != _demo.Encounter).ToArray();
            Assert.That(rooms.Length, Is.EqualTo(4));
            Assert.That(rooms.All(x => x.PreparationStatus == RegionPreparationStatus.Idle), Is.True);
            MovePlayer(new Vector3(0, -.5f, 32));
            for (int i = 0; i < 180 && rooms.Any(x => !x.IsReadyForPassage); i++) yield return null;
            foreach (var room in rooms)
            {
                Assert.That(room.IsReadyForPassage, Is.True, room.LastError?.ToString());
                Assert.That(room.Status, Is.EqualTo(ArenaSessionStatus.Ready));
                Assert.That(room.Enemies.All(x => !x.enabled && !x.Perception.State.IsAlerted), Is.True);
            }
            var instances = rooms.SelectMany(x => x.Enemies).ToArray();
            yield return ActivateRooms();
            MovePlayer(new Vector3(0, .1f, -3)); yield return new WaitForFixedUpdate();
            MovePlayer(new Vector3(0, -2.9f, 54)); yield return new WaitForFixedUpdate();
            CollectionAssert.AreEquivalent(instances, rooms.SelectMany(x => x.Enemies).ToArray());
            Assert.That(OptionalReward().Awarded, Is.False);
        }

        [UnityTest]
        public IEnumerator OptionalClearAwardsOnceRegardlessOfLivingEnemiesElsewhere()
        {
            yield return ActivateRooms();
            var award = OptionalReward();
            var enemies = award.Encounter.Enemies.ToArray();
            enemies[0].Health.ApplyDamage(new DamageInfo(10000, _demo.Health.gameObject, Vector3.zero, Vector3.forward));
            yield return null;
            Assert.That(award.Awarded, Is.False, "Partial clear cannot award the chest.");
            foreach (var enemy in enemies.Skip(1))
                enemy.Health.ApplyDamage(new DamageInfo(10000, _demo.Health.gameObject, Vector3.zero, Vector3.forward));
            yield return null; yield return null;
            Assert.That(award.LastError, Is.Null);
            Assert.That(award.Awarded, Is.True);
            Assert.That(award.Reward, Is.Not.Null);
            Assert.That(AllEncounters().Where(x => x != award.Encounter).Any(x => x.Enemies.Any(e => !e.IsDead)), Is.True);
            var first = award.Reward;
            award.Encounter.SetPlayerInside(false); award.Encounter.SetPlayerInside(true);
            yield return null;
            Assert.That(award.Reward, Is.SameAs(first));
            Object.Destroy(first.gameObject); yield return null; yield return null;
            Assert.That(award.Awarded, Is.True);
            Assert.That(award.Reward == null, Is.True, "A removed reward must not be respawned.");
        }

        [UnityTest]
        public IEnumerator CancelledOptionalEncounterDoesNotAwardChest()
        {
            yield return ActivateRooms();
            var award = OptionalReward(); award.Encounter.Cancel();
            yield return null;
            Assert.That(award.Awarded, Is.False);
            Assert.That(award.Reward, Is.Null);
        }

        [UnityTest]
        public IEnumerator DeathCancelsRoomGroupsAndSuppressesPendingReward()
        {
            yield return ActivateRooms();
            var award = OptionalReward();
            foreach (var enemy in award.Encounter.Enemies.ToArray())
                enemy.Health.ApplyDamage(new DamageInfo(10000, _demo.Health.gameObject, Vector3.zero, Vector3.forward));
            _demo.Health.ApplyDamage(new DamageInfo(10000, null, Vector3.zero, Vector3.zero));
            yield return null;
            Assert.That(award.Awarded, Is.False);
            foreach (var room in AllEncounters())
            {
                Assert.That(room.Status, Is.EqualTo(ArenaSessionStatus.Defeat));
                Assert.That(room.Enemies, Is.Empty);
            }
        }

        private ParkourRecovery IsolateParkour()
        {
            foreach (var encounter in AllEncounters()) encounter.Cancel();
            var recovery = _scene.GetRootGameObjects().SelectMany(x => x.GetComponentsInChildren<ParkourRecovery>()).Single();
            Assert.That(recovery.LastError, Is.Null);
            Assert.DoesNotThrow(recovery.ValidateReturnPoint);
            // Drive only the recovery clock manually; isolate damage/position from enemies.
            recovery.enabled = false;
            return recovery;
        }

        [UnityTest]
        public IEnumerator ParkourAllFourGapsCanBeJumpedAtUnupgradedWalkingSpeed()
        {
            IsolateParkour();
            var motor = _demo.Health.GetComponent<PlayerMotor>();
            motor.Teleport(new Vector3(22.5f, -4.9f, 97), Quaternion.Euler(0, 90, 0));
            Physics.SyncTransforms();
            for (int i = 0; i < 30; i++) motor.Tick(Vector2.zero, false, 1f / 60);
            Assert.That(motor.IsGrounded, Is.True);
            foreach (float launchX in new[] { 23.5f, 27.25f, 31.25f, 35.25f })
            {
                for (int i = 0; i < 100 && _demo.Health.transform.position.x < launchX; i++)
                    motor.Tick(Vector2.up, false, 1f / 60);
                Assert.That(motor.IsGrounded, Is.True, "Launch " + launchX);
                motor.Tick(Vector2.up, false, 1f / 60, true);
                for (int i = 0; i < 90 && !motor.IsGrounded; i++)
                    motor.Tick(Vector2.up, false, 1f / 60);
                Assert.That(motor.IsGrounded, Is.True, "Landing after " + launchX);
                Assert.That(_demo.Health.transform.position.y, Is.InRange(-5.1f, -4.8f), "Missed platform after " + launchX);
            }
            Assert.That(_demo.Health.transform.position.x, Is.GreaterThan(37.25f));
            yield return null;
        }

        [UnityTest]
        public IEnumerator ParkourFallDamagesOncePreservesRunAndRearmsAfterReturn()
        {
            var recovery = IsolateParkour();
            var health = _demo.Health;
            var weapon = health.GetComponent<ProjectFirstRun.Weapons.PlayerWeaponController>();
            var experience = health.GetComponent<ProjectFirstRun.Progression.PlayerExperienceController>();
            var motor = health.GetComponent<PlayerMotor>();
            var look = health.GetComponent<PlayerLook>();
            var input = health.GetComponent<ProjectFirstRun.Input.PlayerInputReader>();
            int ammo = weapon.MagazineAmmo;
            var level = experience.Level;
            var xp = experience.CurrentExperience;
            var states = AllEncounters().Select(x => x.Status).ToArray();
            float before = health.CurrentHealth;
            float damage = health.MaximumHealth * .1f;
            look.ApplyRecoil(5);
            MovePlayer(new Vector3(28.5f, -7.5f, 97));
            recovery.Tick(.02f);
            Assert.That(recovery.IsRecovering, Is.True);
            Assert.That(input.IsGameplayInputEnabled, Is.False);
            Assert.That(health.CurrentHealth, Is.EqualTo(before - damage).Within(.001f));
            var fallingPosition = health.transform.position;
            motor.Tick(Vector2.one, true, .1f, true);
            Assert.That(health.transform.position, Is.EqualTo(fallingPosition), "Recovery suspends motor gravity and movement.");
            for (int i = 0; i < 5; i++) recovery.Tick(.01f);
            Assert.That(health.CurrentHealth, Is.EqualTo(before - damage).Within(.001f));
            recovery.Tick(.2f); recovery.Tick(.2f);
            Assert.That(recovery.LastError, Is.Null);
            Assert.That(recovery.IsRecovering, Is.False);
            Assert.That(Vector3.Distance(health.transform.position, new Vector3(22.5f, -4.9f, 97)), Is.LessThan(.01f));
            Assert.That(Vector3.Angle(health.transform.forward, Vector3.right), Is.LessThan(.1f));
            Assert.That(motor.VerticalVelocity, Is.Zero);
            Assert.That(look.CurrentPitch, Is.Zero);
            Assert.That(look.RecoilOffset, Is.Zero);
            Assert.That(input.IsGameplayInputEnabled, Is.True);
            Assert.That(weapon.MagazineAmmo, Is.EqualTo(ammo));
            Assert.That(experience.Level, Is.EqualTo(level));
            Assert.That(experience.CurrentExperience, Is.EqualTo(xp));
            CollectionAssert.AreEqual(states, AllEncounters().Select(x => x.Status).ToArray());
            recovery.Tick(.02f); // Observe departure from hazard before a separate fall.
            MovePlayer(new Vector3(32.5f, -7.5f, 97)); recovery.Tick(.02f);
            Assert.That(health.CurrentHealth, Is.EqualTo(before - 2 * damage).Within(.001f));
            recovery.Tick(.2f); recovery.Tick(.2f);
            yield return null;
        }

        [UnityTest]
        public IEnumerator ParkourDamageUsesMaximumHealthAndExistingIncomingModifiers()
        {
            var recovery = IsolateParkour();
            _demo.Health.Initialize(200);
            _demo.Health.SetDamageReduction(.25f);
            _demo.Health.SetIncomingDamageMultiplier(1.5f);
            MovePlayer(new Vector3(28.5f, -7.5f, 97)); recovery.Tick(.02f);
            Assert.That(_demo.Health.CurrentHealth, Is.EqualTo(200 - 20 * 1.5f * .75f).Within(.001f));
            recovery.Tick(.2f); recovery.Tick(.2f);
            yield return null;
        }

        [UnityTest]
        public IEnumerator ParkourLethalFallNeverTeleportsOrRestoresInput()
        {
            var recovery = IsolateParkour();
            _demo.Health.ApplyDamage(new DamageInfo(_demo.Health.CurrentHealth - 1, null, Vector3.zero, Vector3.zero));
            MovePlayer(new Vector3(28.5f, -7.5f, 97)); var position = _demo.Health.transform.position;
            recovery.Tick(.02f); recovery.Tick(1);
            Assert.That(_demo.Health.IsDead, Is.True);
            Assert.That(recovery.IsRecovering, Is.False);
            Assert.That(_demo.Health.transform.position, Is.EqualTo(position));
            Assert.That(_demo.Health.GetComponent<ProjectFirstRun.Input.PlayerInputReader>().IsGameplayInputEnabled, Is.False);
            yield return null;
        }

        [UnityTest]
        public IEnumerator ParkourDeathDuringFadeOverridesRecovery()
        {
            var recovery = IsolateParkour();
            MovePlayer(new Vector3(28.5f, -7.5f, 97)); recovery.Tick(.02f); recovery.Tick(.03f);
            _demo.Health.ApplyDamage(new DamageInfo(10000, null, Vector3.zero, Vector3.zero));
            var position = _demo.Health.transform.position;
            recovery.Tick(1);
            Assert.That(_demo.Health.IsDead, Is.True);
            Assert.That(_demo.Health.transform.position, Is.EqualTo(position));
            Assert.That(recovery.Fade, Is.Zero);
            Assert.That(_demo.Health.GetComponent<ProjectFirstRun.Input.PlayerInputReader>().IsGameplayInputEnabled, Is.False);
            yield return null;
        }

        [UnityTest]
        public IEnumerator ParkourPauseAndIndependentControlLockAreNotOverwritten()
        {
            var recovery = IsolateParkour();
            var controller = _demo.Health.GetComponent<PlayerController>();
            var input = _demo.Health.GetComponent<ProjectFirstRun.Input.PlayerInputReader>();
            MovePlayer(new Vector3(28.5f, -7.5f, 97)); recovery.Tick(.02f);
            controller.SetControlEnabled(false);
            Time.timeScale = 0; recovery.Tick(1);
            Assert.That(recovery.IsRecovering, Is.True);
            Assert.That(_demo.Health.transform.position.x, Is.EqualTo(28.5f));
            Time.timeScale = 1;
            recovery.Tick(.2f); recovery.Tick(.2f);
            Assert.That(controller.IsControlEnabled, Is.False);
            Assert.That(input.IsGameplayInputEnabled, Is.False);
            controller.SetControlEnabled(true);
            Assert.That(input.IsGameplayInputEnabled, Is.True);
            yield return null;
        }

        [UnityTest]
        public IEnumerator ParkourRejectsUnsafeAnchorWithoutDamageOrTeleport()
        {
            var recovery = IsolateParkour();
            var anchor = recovery.transform.Find("Parkour return point");
            anchor.position = new Vector3(28.5f, -7.5f, 97);
            MovePlayer(anchor.position);
            float health = _demo.Health.CurrentHealth;
            recovery.Tick(.02f);
            Assert.That(recovery.LastError, Is.Not.Null);
            Assert.That(_demo.Health.CurrentHealth, Is.EqualTo(health));
            Assert.That(recovery.IsRecovering, Is.False);
            yield return null;
        }

        [UnityTearDown]
        public IEnumerator Unload()
        {
            Time.timeScale = 1;
            if (_original.IsValid() && _original.isLoaded) SceneManager.SetActiveScene(_original);
            if (_scene.IsValid() && _scene.isLoaded) yield return SceneManager.UnloadSceneAsync(_scene);
            Cursor.lockState = CursorLockMode.None; Cursor.visible = true;
            base.TearDown();
        }
    }
}
#endif
