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
using UnityEngine.AI;
using Unity.AI.Navigation;

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
            foreach (var enemy in _demo.Encounter.Enemies)
            {
                Assert.That(enemy.Definition.ExperienceReward, Is.EqualTo(10));
                Assert.That(enemy.Definition.ChestDropProfile, Is.Null);
            }
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
            var collector = _demo.Health.GetComponent<ProjectFirstRun.Progression.PlayerExperienceCollector>();
            var pickups = _scene.GetRootGameObjects()
                .SelectMany(x => x.GetComponentsInChildren<ProjectFirstRun.Progression.ExperiencePickup>(true)).ToArray();
            foreach (var pickup in pickups)
                if (!pickup.IsCollected)
                    Assert.That(pickup.TryCollect(collector), Is.True);
            var experience = _demo.Health.GetComponent<ProjectFirstRun.Progression.PlayerExperienceController>();
            Assert.That(experience.TotalExperience, Is.EqualTo(40));
            Assert.That(experience.Level, Is.EqualTo(1), "Collecting all corridor XP must not spawn a level-up chest.");
            var chests = _scene.GetRootGameObjects().SelectMany(x => x.GetComponentsInChildren<ChestController>(true)).ToArray();
            Assert.That(chests.Length, Is.EqualTo(1), "Only the guaranteed ability chest should exist after corridor combat.");
            Assert.That(chests[0], Is.SameAs(reward));
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
            .SelectMany(x => x.GetComponentsInChildren<PreparedRegionEncounter>())
            .Where(x => x.GetComponentInParent<DemoKeyAmbushController>() == null).ToArray();

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

        private DemoKeyAmbushController KeyAmbush() => _scene.GetRootGameObjects()
            .SelectMany(x => x.GetComponentsInChildren<DemoKeyAmbushController>()).Single();

        private void AimAtKey()
        {
            var motor = _demo.Health.GetComponent<PlayerMotor>();
            motor.Teleport(new Vector3(47.5f, -4.9f, 97), Quaternion.Euler(0, 90, 0));
            var origin = _demo.Health.GetComponent<ProjectFirstRun.Chests.Interaction.PlayerChestInteractor>().InteractionOrigin;
            Vector3 target = KeyAmbush().transform.Find("Key pickup").position;
            Vector3 delta = target - origin.position;
            _demo.Health.GetComponent<PlayerLook>().ResetPitch(-Mathf.Asin(delta.normalized.y) * Mathf.Rad2Deg);
            Physics.SyncTransforms();
        }

        private IEnumerator CollectKey()
        {
            AimAtKey();
            Assert.That(KeyAmbush().CanCollect(), Is.True, KeyAmbush().Session.Error);
            Press(Keyboard.current.eKey, queueEventOnly: true);
            yield return null;
            Release(Keyboard.current.eKey, queueEventOnly: true);
            yield return null;
            Assert.That(KeyAmbush().Session.HasKey, Is.True);
        }

        private IEnumerator WaitForAmbush(KeyAmbushPhase phase)
        {
            var ambush = KeyAmbush();
            float deadline = Time.realtimeSinceStartup + 8;
            while (Time.realtimeSinceStartup < deadline && ambush.Session.Phase != phase && ambush.Session.Phase != KeyAmbushPhase.Failed)
                yield return null;
            Assert.That(ambush.Session.Phase, Is.EqualTo(phase), ambush.Session.Error);
        }

        [UnityTest]
        public IEnumerator KeyAmbushPickupAndTwoGroupsOpenOnlySeparateExit()
        {
            var ambush = KeyAmbush();
            var groups = ambush.GetComponentsInChildren<PreparedRegionEncounter>();
            var fields = new SerializedObject(ambush);
            var entrance = (GameObject)fields.FindProperty("_entranceGate").objectReferenceValue;
            var exit = (GameObject)fields.FindProperty("_exitGate").objectReferenceValue;
            Assert.That(entrance.activeSelf, Is.False);
            Assert.That(exit.activeSelf, Is.True);
            Assert.That(groups.All(x => x.PreparationStatus == RegionPreparationStatus.Idle), Is.True);
            yield return CollectKey();
            Assert.That(entrance.activeSelf, Is.True);
            Assert.That(ambush.transform.Find("Key pickup").gameObject.activeSelf, Is.False);
            Assert.That(ambush.TryCollect(), Is.False);
            yield return WaitForAmbush(KeyAmbushPhase.FightingFirst);
            Assert.That(groups[0].Enemies.Count, Is.EqualTo(3));
            Assert.That(groups[0].Enemies.All(x => x.Perception.State.IsAlerted), Is.True);
            Assert.That(groups[1].PreparationStatus, Is.EqualTo(RegionPreparationStatus.Idle));
            foreach (var enemy in groups[0].Enemies.ToArray())
                enemy.Health.ApplyDamage(new DamageInfo(10000, _demo.Health.gameObject, enemy.transform.position, Vector3.forward));
            yield return WaitForAmbush(KeyAmbushPhase.Intermission);
            Assert.That(exit.activeSelf, Is.True);
            Assert.That(ambush.CanEnterFinal, Is.False);
            Time.timeScale = 0;
            for (int i = 0; i < 5; i++) yield return null;
            Assert.That(ambush.Session.Phase, Is.EqualTo(KeyAmbushPhase.Intermission));
            Time.timeScale = 1;
            yield return WaitForAmbush(KeyAmbushPhase.FightingSecond);
            Assert.That(groups[1].Enemies.Count, Is.EqualTo(3));
            foreach (var enemy in groups[1].Enemies.ToArray())
                enemy.Health.ApplyDamage(new DamageInfo(10000, _demo.Health.gameObject, enemy.transform.position, Vector3.forward));
            yield return WaitForAmbush(KeyAmbushPhase.Completed);
            Assert.That(entrance.activeSelf, Is.True);
            Assert.That(exit.activeSelf, Is.False);
            Assert.That(ambush.CanEnterFinal, Is.True);
            Assert.That(_demo.Encounter.Enemies.Any(x => x != null && !x.IsDead), Is.True,
                "Unrelated living enemies must not hold the exit closed.");
        }

        [UnityTest]
        public IEnumerator KeyAmbushPickupRequiresSightRangeAndUnpausedControl()
        {
            var ambush = KeyAmbush();
            Assert.That(ambush.CanCollect(), Is.False);
            AimAtKey();
            Assert.That(ambush.CanCollect(), Is.True);
            Time.timeScale = 0;
            Assert.That(ambush.CanCollect(), Is.False);
            Time.timeScale = 1;
            _demo.Health.GetComponent<PlayerController>().SetControlEnabled(false);
            Assert.That(ambush.CanCollect(), Is.False);
            _demo.Health.GetComponent<PlayerController>().SetControlEnabled(true);
            var wall = GameObject.CreatePrimitive(PrimitiveType.Cube);
            try
            {
                wall.transform.position = new Vector3(48.2f, -3.5f, 97);
                wall.transform.localScale = new Vector3(.2f, 2, 2);
                Physics.SyncTransforms();
                Assert.That(ambush.CanCollect(), Is.False, "Solid occlusion must block collection.");
            }
            finally { Object.Destroy(wall); }
            Assert.That(ambush.Session.HasKey, Is.False);
            yield return null;
        }

        [UnityTest]
        public IEnumerator KeyAmbushDeathCancelsGroupsWithoutUnlockingExit()
        {
            yield return CollectKey();
            yield return WaitForAmbush(KeyAmbushPhase.FightingFirst);
            var ambush = KeyAmbush();
            _demo.Health.ApplyDamage(new DamageInfo(10000, null, Vector3.zero, Vector3.zero));
            yield return null;
            Assert.That(ambush.Session.Phase, Is.EqualTo(KeyAmbushPhase.Cancelled));
            Assert.That(ambush.Session.HasKey, Is.False);
            Assert.That(ambush.CanEnterFinal, Is.False);
            Assert.That(ambush.GetComponentsInChildren<PreparedRegionEncounter>().All(x => x.Enemies.Count == 0), Is.True);
            Assert.That(ambush.transform.Find("Downward exit gate").gameObject.activeSelf, Is.True);
        }

        [UnityTest]
        public IEnumerator KeyAmbushFailedGroupDoesNotGrantCompletion()
        {
            yield return CollectKey();
            yield return WaitForAmbush(KeyAmbushPhase.FightingFirst);
            var ambush = KeyAmbush();
            ambush.GetComponentsInChildren<PreparedRegionEncounter>()[0].Cancel();
            yield return null;
            Assert.That(ambush.Session.Phase, Is.EqualTo(KeyAmbushPhase.Failed));
            Assert.That(ambush.Session.Error, Is.Not.Empty);
            Assert.That(ambush.CanEnterFinal, Is.False);
            Assert.That(ambush.transform.Find("Downward exit gate").gameObject.activeSelf, Is.True);
        }

        [UnityTest]
        public IEnumerator KeyAmbushMissingNavigationFailsEvenWhenPlayerIsFarFromSpawns()
        {
            var ambush = KeyAmbush();
            var first = ambush.GetComponentsInChildren<PreparedRegionEncounter>()[0];
            first.RequestPreparation();
            float deadline = Time.realtimeSinceStartup + 8;
            while (!first.IsReadyForPassage && Time.realtimeSinceStartup < deadline) yield return null;
            Assert.That(first.IsReadyForPassage, Is.True);
            AimAtKey();
            Assert.That(first.Enemies.All(x => Vector3.Distance(x.transform.position, _demo.Health.transform.position) > 2f), Is.True);
            // The fixture loads additively: the original scene may contain an overlapping
            // copy of this NavMesh. Removing only the fixture's surface is not isolation.
            var surfaces = NavMeshSurface.activeSurfaces.ToArray();
            try
            {
                foreach (var surface in surfaces) surface.RemoveData();
                // Do not use isOnNavMesh as proof of removal: dormant agents can retain
                // that flag. Verify the actual data using the same type/mask as each agent.
                foreach (var enemy in first.Enemies)
                {
                    var agent = enemy.GetComponent<NavMeshAgent>();
                    var filter = new NavMeshQueryFilter { agentTypeID = agent.agentTypeID, areaMask = agent.areaMask };
                    Assert.That(NavMesh.SamplePosition(enemy.transform.position, out _, .5f, filter), Is.False,
                        "Missing-navigation setup failed: navigation data remains at an ambush spawn.");
                }
                yield return CollectKey();
                yield return WaitForAmbush(KeyAmbushPhase.Failed);
                StringAssert.Contains("Rebake Demo Navigation", ambush.Session.Error);
                Assert.That(ambush.CanEnterFinal, Is.False);
                Assert.That(ambush.transform.Find("Downward exit gate").gameObject.activeSelf, Is.True);
                Assert.That(ambush.GetComponentsInChildren<PreparedRegionEncounter>().All(x => x.Enemies.Count == 0), Is.True);
            }
            finally
            {
                foreach (var surface in surfaces)
                    if (surface != null && surface.isActiveAndEnabled) surface.AddData();
            }
        }

        [UnityTest]
        public IEnumerator KeyAmbushSecondGroupDisabledAgentFailsBeforeCombat()
        {
            var ambush = KeyAmbush();
            var groups = ambush.GetComponentsInChildren<PreparedRegionEncounter>();
            // Prepare the second group early only to inject a fault before its activation.
            groups[1].RequestPreparation();
            float deadline = Time.realtimeSinceStartup + 8;
            while (!groups[1].IsReadyForPassage && Time.realtimeSinceStartup < deadline) yield return null;
            Assert.That(groups[1].IsReadyForPassage, Is.True);
            groups[1].Enemies[0].GetComponent<NavMeshAgent>().enabled = false;
            yield return CollectKey();
            yield return WaitForAmbush(KeyAmbushPhase.FightingFirst);
            foreach (var enemy in groups[0].Enemies.ToArray())
                enemy.Health.ApplyDamage(new DamageInfo(10000, _demo.Health.gameObject, enemy.transform.position, Vector3.forward));
            yield return WaitForAmbush(KeyAmbushPhase.Failed);
            StringAssert.Contains("NavMesh", ambush.Session.Error);
            Assert.That(ambush.CanEnterFinal, Is.False);
            Assert.That(ambush.transform.Find("Downward exit gate").gameObject.activeSelf, Is.True);
            Assert.That(groups.All(x => x.Enemies.Count == 0), Is.True);
        }

        [UnityTest]
        public IEnumerator KeyReturnRouteCanBeWalkedBackToArenaWithoutParkour()
        {
            foreach (var encounter in AllEncounters()) encounter.Cancel();
            var ambush = KeyAmbush();
            ambush.enabled = false; // Isolate geometry, gate sequencing is covered separately.
            ambush.transform.Find("Downward exit gate").gameObject.SetActive(false);
            _demo.Health.GetComponent<PlayerController>().SetControlEnabled(false);
            var motor = _demo.Health.GetComponent<PlayerMotor>();
            motor.Teleport(new Vector3(49, -4.9f, 105), Quaternion.identity);
            Physics.SyncTransforms();
            foreach (var target in new[] {new Vector3(49,-7,115), new Vector3(19,-7,115),
                new Vector3(19,-7,105), new Vector3(13,-7,105)})
            {
                bool reached = false;
                for (int step = 0; step < 1000; step++)
                {
                    Vector3 delta = target - _demo.Health.transform.position; delta.y = 0;
                    if (delta.magnitude < .2f) { reached = true; break; }
                    _demo.Health.transform.rotation = Quaternion.LookRotation(delta);
                    motor.Tick(Vector2.up, false, .02f);
                    Assert.That(_demo.Health.transform.position.y, Is.GreaterThan(-8));
                    if (step % 5 == 0) yield return null;
                }
                Assert.That(reached, Is.True, "Return route blocked before " + target);
            }
        }

        private DemoFinalController FinalEntry() => _scene.GetRootGameObjects()
            .SelectMany(x => x.GetComponentsInChildren<DemoFinalController>()).Single();

        private void CompleteKeyConditionForFinalTest()
        {
            // Isolate final entry; real combat ordering is covered by the ambush tests.
            var session = KeyAmbush().Session;
            Assert.That(session.TryCollect(true, true, true), Is.True);
            Assert.That(session.TryStartGroup(0), Is.True);
            session.CompleteGroup(0);
            session.Tick(1);
            Assert.That(session.TryStartGroup(1), Is.True);
            session.CompleteGroup(1);
        }

        [UnityTest]
        public IEnumerator FinalGateRequiresBothGroupsAndEntryLocksCombatWithoutDeath()
        {
            var final = FinalEntry();
            Assert.That(final.IsGateOpen, Is.False);
            var session = KeyAmbush().Session;
            session.TryCollect(true, true, true);
            yield return null;
            Assert.That(final.IsGateOpen, Is.False, "A key alone is insufficient.");
            // Complete the condition independently of enemy deaths for this focused test.
            if (session.Phase == KeyAmbushPhase.PreparingFirst) session.TryStartGroup(0);
            session.CompleteGroup(0);
            session.Tick(1);
            session.TryStartGroup(1);
            session.CompleteGroup(1);
            yield return null;
            Assert.That(final.IsGateOpen, Is.True);
            Assert.That(final.Session.Phase, Is.EqualTo(DemoFinalPhase.Playing));
            Assert.That(_demo.Encounter.Enemies.Any(x => !x.IsDead), Is.True);
            _demo.Health.GetComponent<PlayerMotor>().Teleport(new Vector3(0, -6.9f, 112), Quaternion.identity);
            Physics.SyncTransforms();
            yield return null;
            Assert.That(final.Session.Phase, Is.EqualTo(DemoFinalPhase.Ending));
            Assert.That(Time.timeScale, Is.Zero);
            Assert.That(final.TryBegin(), Is.False);
            Assert.That(_demo.Health.GetComponent<ProjectFirstRun.Input.PlayerInputReader>().IsGameplayInputEnabled, Is.False);
            Assert.That(_demo.Health.GetComponent<ProjectFirstRun.Weapons.PlayerWeaponController>().IsWeaponControlEnabled, Is.False);
            Assert.That(_demo.Health.GetComponent<ProjectFirstRun.Abilities.PlayerAbilityController>().IsAbilityControlEnabled, Is.False);
            float health = _demo.Health.CurrentHealth;
            Assert.That(_demo.Health.ApplyDamage(new DamageInfo(10000, null, Vector3.zero, Vector3.zero)).WasApplied, Is.False);
            Assert.That(_demo.Health.CurrentHealth, Is.EqualTo(health));
            Assert.That(_demo.Health.IsDead, Is.False);
        }

        [UnityTest]
        public IEnumerator FinalEntryRejectsPauseInputLocksAndDeath()
        {
            var final = FinalEntry();
            CompleteKeyConditionForFinalTest();
            yield return null;
            _demo.Health.GetComponent<PlayerMotor>().Teleport(new Vector3(0, -6.9f, 112), Quaternion.identity);
            Physics.SyncTransforms();
            Time.timeScale = 0;
            Assert.That(final.TryBegin(), Is.False);
            Time.timeScale = 1;
            var input = _demo.Health.GetComponent<ProjectFirstRun.Input.PlayerInputReader>();
            object owner = new object();
            input.SetGameplayBlocked(owner, true);
            Assert.That(final.TryBegin(), Is.False);
            input.SetGameplayBlocked(owner, false);
            _demo.Health.ApplyDamage(new DamageInfo(10000, null, Vector3.zero, Vector3.zero));
            Assert.That(final.TryBegin(), Is.False);
            yield return null;
            Assert.That(final.Session.Phase, Is.EqualTo(DemoFinalPhase.Dead));
            Assert.That(final.IsEnding, Is.False);
        }

        [UnityTest]
        public IEnumerator FinalReleasePreservesOtherOwnersCombatAndDamageBlocks()
        {
            var final = FinalEntry();
            CompleteKeyConditionForFinalTest();
            yield return null;
            _demo.Health.GetComponent<PlayerMotor>().Teleport(new Vector3(0, -6.9f, 112), Quaternion.identity);
            Physics.SyncTransforms();
            Assert.That(final.TryBegin(), Is.True);
            var weapons = _demo.Health.GetComponent<ProjectFirstRun.Weapons.PlayerWeaponController>();
            var abilities = _demo.Health.GetComponent<ProjectFirstRun.Abilities.PlayerAbilityController>();
            var input = _demo.Health.GetComponent<ProjectFirstRun.Input.PlayerInputReader>();
            object other = new object();
            weapons.SetWeaponBlocked(other, true);
            abilities.SetAbilityBlocked(other, true);
            input.SetGameplayBlocked(other, true);
            _demo.Health.SetDamageBlocked(other, true);
            final.enabled = false;
            Assert.That(weapons.IsWeaponControlEnabled, Is.False);
            Assert.That(abilities.IsAbilityControlEnabled, Is.False);
            Assert.That(input.IsGameplayInputEnabled, Is.False);
            Assert.That(_demo.Health.ApplyDamage(new DamageInfo(1, null, Vector3.zero, Vector3.zero)).WasApplied, Is.False);
            weapons.SetWeaponBlocked(other, false);
            abilities.SetAbilityBlocked(other, false);
            input.SetGameplayBlocked(other, false);
            _demo.Health.SetDamageBlocked(other, false);
            Assert.That(weapons.IsWeaponControlEnabled, Is.True);
            Assert.That(abilities.IsAbilityControlEnabled, Is.True);
            Assert.That(input.IsGameplayInputEnabled, Is.True);
            Assert.That(_demo.Health.ApplyDamage(new DamageInfo(1, null, Vector3.zero, Vector3.zero)).WasApplied, Is.True);
        }

        [UnityTest]
        public IEnumerator FinalPresentationCompletesOnUnscaledTimeWithOneImpactAndSafeReplayState()
        {
            var final = FinalEntry();
            CompleteKeyConditionForFinalTest();
            yield return null;
            _demo.Health.GetComponent<PlayerMotor>().Teleport(new Vector3(0, -6.9f, 112), Quaternion.Euler(0, 180, 0));
            Physics.SyncTransforms();
            var view = _demo.Health.GetComponentInChildren<Camera>().transform;
            Vector3 position = view.localPosition;
            Quaternion rotation = view.localRotation;
            Assert.That(final.TryBegin(), Is.True);
            Assert.That(final.Session.TryRequestReplay(), Is.False);
            float deadline = Time.realtimeSinceStartup + 12;
            while (final.Session.Phase == DemoFinalPhase.Ending && Time.realtimeSinceStartup < deadline)
            {
                Assert.That(Time.timeScale, Is.Zero);
                yield return null;
            }
            Assert.That(final.Session.Phase, Is.EqualTo(DemoFinalPhase.Completed));
            Assert.That(final.Presentation.ImpactCount, Is.EqualTo(1));
            Assert.That(final.Presentation.Fade, Is.EqualTo(1));
            Assert.That(Cursor.lockState, Is.EqualTo(CursorLockMode.None));
            Assert.That(Cursor.visible, Is.True);
            Assert.That(_demo.Health.IsDead, Is.False);
            Assert.That(final.TryBegin(), Is.False);
            Assert.That(_demo.Health.ApplyDamage(new DamageInfo(10000, null, Vector3.zero, Vector3.zero)).WasApplied, Is.False);
            // Verify replay request ownership without unloading the runner's additive fixture.
            Assert.That(final.Session.TryRequestReplay(), Is.True);
            Assert.That(final.Session.TryRequestReplay(), Is.False);
            final.Presentation.Tick(100);
            Assert.That(final.Presentation.ImpactCount, Is.EqualTo(1));
            final.enabled = false;
            Assert.That(Vector3.Distance(view.localPosition, position), Is.LessThan(.001f));
            Assert.That(Quaternion.Angle(view.localRotation, rotation), Is.LessThan(.01f));
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
