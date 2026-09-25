#if UNITY_EDITOR
using System.Collections;
using System.Linq;
using NUnit.Framework;
using ProjectFirstRun.Chests;
using ProjectFirstRun.Combat;
using ProjectFirstRun.Development.Arenas;
using ProjectFirstRun.Enemies;
using ProjectFirstRun.Items;
using ProjectFirstRun.Rewards.Claims;
using ProjectFirstRun.Weapons;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using Object = UnityEngine.Object;

namespace ProjectFirstRun.Tests.PlayMode.Arenas
{
    public sealed class ShotgunArenaTests : InputTestFixture
    {
        private Scene _scene, _original;
        private ContentArenaController _arena;
        private PlayerWeaponController _weapon;
        private Mouse _mouse;
        private Keyboard _keyboard;
        private int ShotgunIndex => _arena.Items.ToList().FindIndex(x => x.StableId == "weapon.shotgun");
        private WeaponDefinition Shotgun => (WeaponDefinition)_arena.Items[ShotgunIndex];
        // Own input isolation across the asynchronous scene lifetime; NUnit SetUp would run after UnitySetUp.
        [SetUp] public override void Setup() { }
        [TearDown] public override void TearDown() { }

        [UnitySetUp]
        public IEnumerator LoadArena()
        {
            base.Setup();
            _mouse = InputSystem.AddDevice<Mouse>();
            _keyboard = InputSystem.AddDevice<Keyboard>();
            Time.timeScale = 1;
            _original = SceneManager.GetActiveScene();
            const string path = "Assets/_Project/Scenes/Tests/Test_ContentArena.unity";
            yield return EditorSceneManager.LoadSceneAsyncInPlayMode(path, new LoadSceneParameters(LoadSceneMode.Additive));
            _scene = SceneManager.GetSceneByPath(path);
            SceneManager.SetActiveScene(_scene);
            _arena = Find<ContentArenaController>().Single();
            for (int i = 0; i < 200 && !_arena.IsReady; i++) yield return null;
            Assert.That(_arena.IsReady, Is.True);
            Assert.That(ShotgunIndex, Is.GreaterThanOrEqualTo(0));
            _weapon = _arena.Player.GetComponent<PlayerWeaponController>();
            yield return null;
        }
        [UnityTearDown]
        public IEnumerator UnloadArena()
        {
            if (_arena != null) Find<ContentArenaPanel>().Single().Close();
            Time.timeScale = 1;
            if (_original.IsValid() && _original.isLoaded) SceneManager.SetActiveScene(_original);
            if (_scene.IsValid() && _scene.isLoaded) yield return SceneManager.UnloadSceneAsync(_scene);
            if (_mouse != null && _mouse.added) InputSystem.RemoveDevice(_mouse);
            if (_keyboard != null && _keyboard.added) InputSystem.RemoveDevice(_keyboard);
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
            base.TearDown();
        }
        private T[] Find<T>() where T : Component => _scene.GetRootGameObjects()
            .SelectMany(x => x.GetComponentsInChildren<T>(true)).ToArray();
        private void EquipShotgun()
        {
            Assert.That(_arena.Acquire(ShotgunIndex), Is.True, _arena.LastResult);
            Assert.That(_arena.LastResult, Is.EqualTo("Acquired"));
            Assert.That(_arena.Player.GetComponent<PlayerWeaponSwitcher>().TrySwitchNext(), Is.True);
            Assert.That(_weapon.ActiveDefinition, Is.SameAs(Shotgun));
        }

        [UnityTest]
        public IEnumerator RealMouseTriggerConsumesOneShellAndHoldingDoesNotRepeat()
        {
            EquipShotgun();
            int events = 0;
            HitscanVolleyResult shot = null;
            _weapon.ShotFired += x => { events++; shot = x; };
            Press(_mouse.leftButton, queueEventOnly: true);
            yield return null;
            yield return null;
            Assert.That(events, Is.EqualTo(1));
            Assert.That(_weapon.MagazineAmmo, Is.EqualTo(5));
            Assert.That(shot.Pellets.Count, Is.EqualTo(8));
            yield return new WaitForSeconds(.95f);
            Assert.That(events, Is.EqualTo(1));
            Release(_mouse.leftButton, queueEventOnly: true);
            yield return null;
            yield return null;
            Assert.That(_arena.Player.GetComponent<ProjectFirstRun.Input.PlayerInputReader>().IsFireHeld, Is.False);
            Assert.That(_weapon.ActiveEntry.RuntimeState.FireCooldownRemaining, Is.Zero);
            Press(_mouse.leftButton, queueEventOnly: true);
            yield return null;
            yield return null;
            Assert.That(events, Is.EqualTo(2));
            Assert.That(_weapon.MagazineAmmo, Is.EqualTo(4));
            Assert.That(_arena.Player.GetComponentsInChildren<LineRenderer>(true).Length, Is.EqualTo(8));
            Release(_mouse.leftButton, queueEventOnly: true);
            yield return new WaitForSeconds(.15f);
            Assert.That(_arena.Player.GetComponentsInChildren<LineRenderer>(true).All(x => !x.enabled), Is.True);
        }

        [UnityTest]
        public IEnumerator ReloadEmptyMagazinePanelAndDeathBlockFiring()
        {
            EquipShotgun();
            int events = 0;
            _weapon.ShotFired += _ => events++;
            var state = _weapon.ActiveEntry.RuntimeState;
            for (int i = 0; i < 6; i++) { state.Tick(1); state.TryFire(); }
            state.Tick(1);
            Press(_mouse.leftButton, queueEventOnly: true);
            yield return null;
            Assert.That(events, Is.Zero);
            Assert.That(_weapon.MagazineAmmo, Is.Zero);
            Release(_mouse.leftButton, queueEventOnly: true);
            Press(_keyboard.rKey, queueEventOnly: true);
            yield return null;
            Assert.That(_weapon.IsReloading, Is.True);
            Press(_mouse.leftButton, queueEventOnly: true);
            yield return null;
            Assert.That(events, Is.Zero);
            Release(_mouse.leftButton, queueEventOnly: true);
            Release(_keyboard.rKey, queueEventOnly: true);
            state.Tick(2);
            var panel = Find<ContentArenaPanel>().Single();
            Assert.That(panel.TryOpen(), Is.True);
            Press(_mouse.leftButton, queueEventOnly: true);
            yield return null;
            Assert.That(events, Is.Zero);
            Release(_mouse.leftButton, queueEventOnly: true);
            panel.Close();
            yield return null;
            _arena.Player.GetComponent<HealthComponent>().ApplyDamage(new DamageInfo(10000, null, Vector3.zero, Vector3.forward));
            Press(_mouse.leftButton, queueEventOnly: true);
            yield return null;
            Assert.That(events, Is.Zero);
        }

        [UnityTest]
        public IEnumerator LevelsPreserveInactiveStateAndApplyStatsWithoutMutatingAsset()
        {
            EquipShotgun();
            var entry = _weapon.ActiveEntry;
            var state = entry.RuntimeState;
            state.TryFire();
            state.TryStartReload();
            state.Tick(.2f);
            float reload = state.ReloadTimeRemaining, cooldown = state.FireCooldownRemaining;
            var switcher = _arena.Player.GetComponent<PlayerWeaponSwitcher>();
            switcher.TrySwitchNext();
            for (int i = 1; i < 8; i++) Assert.That(_arena.LevelUp(ShotgunIndex), Is.True);
            Assert.That(_weapon.ActiveDefinition, Is.Not.SameAs(Shotgun));
            Assert.That(entry.Shot.PelletCount, Is.EqualTo(10));
            Assert.That(entry.Shot.PushDistance, Is.EqualTo(1));
            Assert.That(state.MagazineAmmo, Is.EqualTo(5));
            Assert.That(state.ReserveAmmo, Is.EqualTo(48));
            Assert.That(state.ReloadTimeRemaining, Is.EqualTo(reload));
            Assert.That(state.FireCooldownRemaining, Is.EqualTo(cooldown));
            Assert.That(_arena.LevelUp(ShotgunIndex), Is.True);
            Assert.That(_arena.LastResult, Is.EqualTo("MaximumLevelReached"));
            switcher.TrySwitchNext();
            Assert.That(_weapon.ActiveEntry, Is.SameAs(entry));
            Assert.That(_arena.Acquire(3), Is.True); // existing damage upgrade
            Assert.That(_weapon.CurrentDamage, Is.EqualTo(14.4f).Within(.001f));
            Assert.That(new PlayerWeaponRuntimeEntry(Shotgun).Shot.PelletCount, Is.EqualTo(8));
            yield return null;
        }

        [UnityTest]
        public IEnumerator RealWeaponChestAcquiresAndLevelsShotgunThenExcludesMaxLevel()
        {
            Assert.That(_arena.SpawnChest(0), Is.True);
            var chest = Find<ChestController>().Single();
            // Four weapons now compete for three offers; control randomness, not production eligibility.
            typeof(ChestController).GetField("_offerGenerator", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic)
                .SetValue(chest, new ProjectFirstRun.Rewards.RewardOfferGenerator(
                    new ProjectFirstRun.Rewards.RewardCandidateFilter(), new FirstCandidate()));
            chest.TryOpen();
            Assert.That(_arena.Selection.ActiveSession.Offer.Choices, Does.Contain(Shotgun));
            Assert.That(_arena.Selection.Select(Shotgun), Is.EqualTo(RewardClaimResult.Claimed));
            Assert.That(_arena.ItemLevel(ShotgunIndex), Is.EqualTo(1));
            yield return null;
            Assert.That(_arena.SpawnChest(0), Is.True);
            Find<ChestController>().Last(x => x.gameObject.activeInHierarchy).TryOpen();
            Assert.That(_arena.Selection.Select(Shotgun), Is.EqualTo(RewardClaimResult.Claimed));
            Assert.That(_arena.ItemLevel(ShotgunIndex), Is.EqualTo(2));
            for (int i = 2; i < 8; i++) _arena.LevelUp(ShotgunIndex);
            yield return null;
            Assert.That(_arena.SpawnChest(0), Is.True);
            Find<ChestController>().Last(x => x.gameObject.activeInHierarchy).TryOpen();
            Assert.That(_arena.Selection.ActiveSession.Offer.Choices, Has.No.Member(Shotgun));
            Assert.That(_arena.Selection.ActiveSession.Offer.Choices.All(x => x.StableId != "weapon.development-secondary"), Is.True);
            _arena.Selection.Select(_arena.Selection.ActiveSession.Offer.Choices[0]);
        }

        private sealed class FirstCandidate : ProjectFirstRun.Rewards.IRandomSource
        {
            public int Next(int minInclusive, int maxExclusive) => minInclusive;
        }

        [UnityTest]
        public IEnumerator FullSlotsRejectShotgunWithoutChangingLoadout()
        {
            _arena.Acquire(1);
            Assert.That(_arena.Acquire(ShotgunIndex), Is.True);
            Assert.That(_arena.LastResult, Is.EqualTo("CapacityReached"));
            Assert.That(_arena.ItemLevel(ShotgunIndex), Is.Zero);
            yield return null;
        }

        [UnityTest]
        public IEnumerator PushMovesAllFamiliesWithoutResettingAttackAndRejectsDeath()
        {
            foreach (var enemy in _arena.Enemies.Take(3))
            {
                var motor = enemy.GetComponent<EnemyMotor>();
                var attack = enemy.GetComponent<EnemyAttackController>();
                float cooldown = attack.CooldownRemaining;
                bool attackEnabled = attack.IsAttackEnabled;
                var origin = enemy.transform.position;
                Assert.That(motor.TryPush(Vector3.right, .6f), Is.True, enemy.Definition.name);
                Assert.That(Vector3.Distance(origin, enemy.transform.position), Is.EqualTo(.6f).Within(.05f));
                Assert.That(attack.CooldownRemaining, Is.EqualTo(cooldown));
                Assert.That(attack.IsAttackEnabled, Is.EqualTo(attackEnabled));
                Assert.That(enemy.GetComponent<NavMeshAgent>().isOnNavMesh, Is.True);
                enemy.Health.ApplyDamage(new DamageInfo(10000, null, origin, Vector3.forward));
                Assert.That(motor.TryPush(Vector3.right, 1), Is.False);
            }
            yield return null;
        }

        [UnityTest]
        public IEnumerator PushDoesNotCancelCommittedChargeOrMovePreparedEnemy()
        {
            var enemy = _arena.Enemies.First(x => x.GetComponent<EnemyAttackController>().ChargeState != null);
            var attack = enemy.GetComponent<EnemyAttackController>();
            var motor = enemy.GetComponent<EnemyMotor>();
            var charge = attack.ChargeState;
            Assert.That(charge.TryBegin(Vector3.forward), Is.True);
            charge.Tick(charge.TimeRemaining);
            Assert.That(charge.Phase, Is.EqualTo(EnemyChargePhase.Charging));
            float remaining = charge.DistanceRemaining;
            Assert.That(motor.TryPush(Vector3.right, .6f), Is.True);
            Assert.That(charge.Phase, Is.EqualTo(EnemyChargePhase.Charging));
            Assert.That(charge.DistanceRemaining, Is.EqualTo(remaining));
            attack.Tick(.01f);
            Assert.That(charge.DistanceRemaining, Is.LessThan(remaining));
            motor.enabled = false;
            Assert.That(motor.TryPush(Vector3.right, 1), Is.False);
            motor.enabled = true;
            enemy.enabled = false;
            Assert.That(motor.TryPush(Vector3.right, 1), Is.False);
            yield return null;
        }

        [UnityTest]
        public IEnumerator PushStopsAtWallsPlayerAndNavMeshEdgesAndPreservesStoppedState()
        {
            var enemy = _arena.Enemies[0];
            var motor = enemy.GetComponent<EnemyMotor>();
            var agent = enemy.GetComponent<NavMeshAgent>();
            motor.Stop();
            Assert.That(agent.isStopped, Is.True, "Stop must halt the agent before any push.");
            var obstacle = GameObject.CreatePrimitive(PrimitiveType.Cube);
            try
            {
                obstacle.transform.position = enemy.transform.position + new Vector3(1.2f, 1, 0);
                obstacle.transform.localScale = new Vector3(.3f, 3, 4);
                Physics.SyncTransforms();
                float x = enemy.transform.position.x;
                motor.TryPush(Vector3.right, 3);
                Assert.That(enemy.transform.position.x - x, Is.LessThan(1));
                Assert.That(agent.isStopped, Is.True);
                obstacle.SetActive(false);
                Assert.That(agent.Warp(new Vector3(0, 0, -4)), Is.True);
                Physics.SyncTransforms();
                motor.TryPush(Vector3.back, 4);
                Assert.That(enemy.transform.position.z, Is.GreaterThan(_arena.Player.transform.position.z + .4f));
                Assert.That(agent.Warp(new Vector3(18, 0, 20)), Is.True);
                motor.TryPush(Vector3.right, 10);
                Assert.That(enemy.transform.position.x, Is.LessThan(20));
                Assert.That(agent.isOnNavMesh, Is.True);
                enemy.gameObject.SetActive(false);
                Assert.That(motor.TryPush(Vector3.left, 1), Is.False);
            }
            finally { Object.Destroy(obstacle); }
            yield return null;
        }
    }
}
#endif
