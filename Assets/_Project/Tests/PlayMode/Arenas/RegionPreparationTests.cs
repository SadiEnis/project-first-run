using System;
using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using ProjectFirstRun.Arenas;
using ProjectFirstRun.Combat;
using ProjectFirstRun.Enemies;
using ProjectFirstRun.Enemies.Spawning;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.TestTools;
using Object = UnityEngine.Object;

namespace ProjectFirstRun.Tests.PlayMode.Arenas
{
    public sealed class RegionPreparationTests
    {
        private readonly List<GameObject> _objects = new List<GameObject>();
        private EnemyDefinition _definition;
        private EnemyController _prefab;
        private EnemySpawner _spawner;
        private EnemyRegistry _registry;
        private HealthComponent _health;
        private Transform _player;
        private PreparedRegionEncounter _encounter;
        private NavMeshData _navData;
        private NavMeshDataInstance _navInstance;

        private GameObject New(string name)
        {
            var value = new GameObject(name);
            _objects.Add(value);
            return value;
        }

        [SetUp]
        public void Setup()
        {
            Time.timeScale = 1;
            _navData = NavMeshBuilder.BuildNavMeshData(NavMesh.GetSettingsByID(0),
                new List<NavMeshBuildSource> { new NavMeshBuildSource {
                    shape = NavMeshBuildSourceShape.Box, size = new Vector3(250, .2f, 250),
                    transform = Matrix4x4.TRS(new Vector3(0, -.1f, 0), Quaternion.identity, Vector3.one)
                } }, new Bounds(Vector3.zero, new Vector3(260, 10, 260)), Vector3.zero, Quaternion.identity);
            Assert.That(_navData, Is.Not.Null);
            _navInstance = NavMesh.AddNavMeshData(_navData);
            _definition = ScriptableObject.CreateInstance<EnemyDefinition>();
            _prefab = NewPrefab("Preparation prefab");
            _spawner = New("Spawner").AddComponent<EnemySpawner>();
            _registry = New("Registry").AddComponent<EnemyRegistry>();
            _player = New("Player").transform;
            _player.gameObject.AddComponent<BoxCollider>().size = Vector3.one * .4f;
            _health = _player.gameObject.AddComponent<HealthComponent>();
            _encounter = New("Prepared encounter").AddComponent<PreparedRegionEncounter>();
        }

        private EnemyController NewPrefab(string name)
        {
            var prefab = New(name);
            prefab.transform.position = new Vector3(100, 0, 100);
            var controller = prefab.AddComponent<EnemyController>();
            prefab.AddComponent<EnemyAttackController>();
            prefab.AddComponent<BoxCollider>();
            return controller;
        }

        private EnemySpawnRequest Request(EnemyController prefab) =>
            new EnemySpawnRequest(prefab, _definition, _player, _health, _registry,
                Vector3.zero, Quaternion.identity);

        private void Initialize(int count = 4, int perFrame = 1)
        {
            var requests = new EnemySpawnRequest[count];
            for (int i = 0; i < count; i++) requests[i] = Request(_prefab);
            _encounter.Initialize("b", _spawner, requests, _health, perFrame, 1000);
        }

        private IEnumerator WaitReady()
        {
            for (int i = 0; i < 100 && !_encounter.IsReadyForPassage; i++)
            {
                Assert.That(_encounter.LastError, Is.Null);
                yield return null;
            }
            Assert.That(_encounter.IsReadyForPassage, Is.True,
                _encounter.LastError?.ToString());
        }

        [TearDown]
        public void Teardown()
        {
            Time.timeScale = 1;
            for (int i = _objects.Count - 1; i >= 0; i--)
                if (_objects[i] != null) Object.DestroyImmediate(_objects[i]);
            _objects.Clear();
            _navInstance.Remove();
            if (_navData != null) Object.DestroyImmediate(_navData);
            Object.DestroyImmediate(_definition);
        }

        [UnityTest]
        public IEnumerator PreparationHonorsItemBudgetAndKeepsEnemiesDormant()
        {
            Initialize();
            int registered = 0;
            _registry.EnemyRegistered += _ => registered++;
            _encounter.RequestPreparation();
            _encounter.RequestPreparation();
            int previous = 0;
            while (!_encounter.IsReadyForPassage && previous < 4)
            {
                yield return null;
                Assert.That(_encounter.LastError, Is.Null);
                Assert.That(_encounter.Enemies.Count - previous, Is.InRange(0, 1));
                previous = _encounter.Enemies.Count;
            }
            Assert.That(previous, Is.EqualTo(4));
            Assert.That(_registry.ActiveCount, Is.Zero);
            Assert.That(registered, Is.Zero);
            foreach (EnemyController enemy in _encounter.Enemies)
            {
                Assert.That(enemy.gameObject.activeInHierarchy, Is.True);
                Assert.That(enemy.enabled, Is.False);
                Assert.That(enemy.GetComponent<EnemyMotor>().IsMovementEnabled, Is.False);
                Assert.That(enemy.GetComponent<NavMeshAgent>().isOnNavMesh, Is.True);
                Assert.That(enemy.GetComponent<EnemyAttackController>().IsAttackEnabled, Is.False);
                Assert.That(enemy.GetComponent<Collider>().enabled, Is.False);
            }
            yield return new WaitForSeconds(.1f);
            Assert.That(_health.CurrentHealth, Is.EqualTo(100));
            TestContext.WriteLine("Preparation max step ms: " + _encounter.MaxPreparationStepMilliseconds);
        }

        [UnityTest]
        public IEnumerator EarlyEntryWaitsForEveryEnemyThenActivatesWithoutRespawning()
        {
            Initialize();
            _encounter.SetPlayerInside(true);
            Assert.That(_encounter.Status, Is.EqualTo(ArenaSessionStatus.Ready));
            Assert.That(_registry.ActiveCount, Is.Zero);
            yield return WaitReady();
            Assert.That(_encounter.Status, Is.EqualTo(ArenaSessionStatus.Running));
            Assert.That(_registry.ActiveCount, Is.EqualTo(4));
            Assert.That(_encounter.Region.Status, Is.EqualTo(RegionEncounterStatus.Active));
            foreach (EnemyController enemy in _encounter.Enemies)
                Assert.That(enemy.GetComponent<Collider>().enabled, Is.True);
            TestContext.WriteLine("Activation ms: " + _encounter.LastActivationMilliseconds);
        }

        [UnityTest]
        public IEnumerator LeavingBeforeReadyCancelsActivationIntent_ReentryPreservesIdentityAndHealth()
        {
            Initialize(3);
            _encounter.SetPlayerInside(true);
            _encounter.SetPlayerInside(false);
            yield return WaitReady();
            Assert.That(_encounter.Status, Is.EqualTo(ArenaSessionStatus.Ready));
            var first = _encounter.Enemies[0];
            _encounter.SetPlayerInside(true);
            yield return null;
            first.Health.ApplyDamage(new DamageInfo(1, null, Vector3.zero, Vector3.forward));
            float remaining = first.Health.CurrentHealth;
            _encounter.SetPlayerInside(false);
            _encounter.SetPlayerInside(true);
            yield return null;
            Assert.That(_encounter.Region.IsPlayerInside, Is.True);
            Assert.That(_encounter.Enemies[0], Is.SameAs(first));
            Assert.That(first.Health.CurrentHealth, Is.EqualTo(remaining));
            Assert.That(_encounter.Enemies.Count, Is.EqualTo(3));
        }

        [UnityTest]
        public IEnumerator CompletingGroupOutsideDoesNotRespawnOnReturn()
        {
            Initialize(2);
            _encounter.SetPlayerInside(true);
            yield return WaitReady();
            _encounter.SetPlayerInside(false);
            int wins = 0;
            _encounter.Victory += () => wins++;
            foreach (EnemyController enemy in _encounter.Enemies)
                enemy.Health.ApplyDamage(new DamageInfo(enemy.Health.MaximumHealth, null, Vector3.zero, Vector3.forward));
            Assert.That(_encounter.Region.Status, Is.EqualTo(RegionEncounterStatus.Completed));
            _encounter.SetPlayerInside(true);
            yield return null;
            Assert.That(wins, Is.EqualTo(1));
            Assert.That(_registry.ActiveCount, Is.Zero);
            Assert.That(_encounter.Enemies.Count, Is.EqualTo(2));
        }

        [UnityTest]
        public IEnumerator PartialPreparationFailureCleansAlreadyCreatedEnemies()
        {
            var doomed = NewPrefab("Destroyed before its preparation slot");
            _encounter.Initialize("b", _spawner, new[] { Request(_prefab), Request(doomed) }, _health, 1, 1000);
            _encounter.RequestPreparation();
            while (_encounter.Enemies.Count == 0) yield return null;
            var prepared = _encounter.Enemies[0];
            Object.DestroyImmediate(doomed.gameObject);
            yield return null;
            yield return null;
            Assert.That(_encounter.PreparationStatus, Is.EqualTo(RegionPreparationStatus.Failed));
            Assert.That(_encounter.LastError, Is.Not.Null);
            Assert.That(_encounter.IsReadyForPassage, Is.False);
            Assert.That(prepared == null, Is.True);
            Assert.That(_registry.ActiveCount, Is.Zero);
        }

        [UnityTest]
        public IEnumerator DeathCancelsPreparationAndDestroysOwnedInstances()
        {
            Initialize();
            _encounter.RequestPreparation();
            while (_encounter.Enemies.Count == 0) yield return null;
            var prepared = _encounter.Enemies[0];
            _health.ApplyDamage(new DamageInfo(100, null, Vector3.zero, Vector3.forward));
            yield return null;
            Assert.That(_encounter.PreparationStatus, Is.EqualTo(RegionPreparationStatus.Cancelled));
            Assert.That(prepared == null, Is.True);
            Assert.That(_encounter.Enemies.Count, Is.Zero);
        }

        [UnityTest]
        public IEnumerator DisablingOwnerCancelsWithoutFakeEnemyDeaths()
        {
            Initialize(1);
            _encounter.SetPlayerInside(true);
            yield return WaitReady();
            int deaths = 0;
            var enemy = _encounter.Enemies[0];
            enemy.Died += (_, __, ___) => deaths++;
            _encounter.enabled = false;
            Assert.That(_registry.ActiveCount, Is.Zero);
            yield return null;
            Assert.That(enemy == null, Is.True);
            Assert.That(deaths, Is.Zero);
        }

        [UnityTest]
        public IEnumerator PauseDoesNotAdvancePreparation()
        {
            Initialize();
            _encounter.RequestPreparation();
            Time.timeScale = 0;
            yield return null;
            yield return null;
            Assert.That(_encounter.Enemies.Count, Is.Zero);
            Time.timeScale = 1;
            yield return WaitReady();
        }

        [UnityTest]
        public IEnumerator PreparationAndActivationVolumesHaveSeparateRoles_IgnoreForeignColliders()
        {
            Initialize();
            var prepareVolume = New("Approach").AddComponent<BoxCollider>();
            prepareVolume.isTrigger = true;
            prepareVolume.size = Vector3.one * 2;
            prepareVolume.transform.position = new Vector3(0, 0, -4);
            prepareVolume.gameObject.AddComponent<RegionPreparationTrigger>().Configure(prepareVolume,
                _player, _encounter, RegionPreparationTrigger.Purpose.Prepare);
            var activationVolume = New("Entry").AddComponent<SphereCollider>();
            activationVolume.isTrigger = true;
            activationVolume.radius = 1;
            activationVolume.gameObject.AddComponent<RegionPreparationTrigger>().Configure(activationVolume,
                _player, _encounter, RegionPreparationTrigger.Purpose.Activate);
            _player.position = new Vector3(0, 0, -8);
            New("Unrelated collider").AddComponent<BoxCollider>().transform.position = prepareVolume.transform.position;
            Physics.SyncTransforms();
            yield return new WaitForFixedUpdate();
            yield return new WaitForFixedUpdate();
            Assert.That(_encounter.PreparationStatus, Is.EqualTo(RegionPreparationStatus.Idle));
            _player.position = prepareVolume.transform.position;
            Physics.SyncTransforms();
            yield return new WaitForFixedUpdate();
            yield return WaitReady();
            Assert.That(_encounter.Status, Is.EqualTo(ArenaSessionStatus.Ready));
            _player.position = Vector3.zero;
            Physics.SyncTransforms();
            yield return new WaitForFixedUpdate();
            yield return null;
            Assert.That(_encounter.Status, Is.EqualTo(ArenaSessionStatus.Running));
        }

        [UnityTest]
        public IEnumerator PassageRemainsBlockedUntilDestinationIsPrepared()
        {
            Initialize();
            _player.position = new Vector3(0, 0, -5);
            var volume = New("Passage").AddComponent<BoxCollider>();
            volume.isTrigger = true;
            volume.size = Vector3.one * 4;
            var blocker = New("Door").AddComponent<BoxCollider>();
            var passage = volume.gameObject.AddComponent<RegionPassageController>();
            passage.BindPreparation(_encounter);
            Physics.SyncTransforms();
            passage.Configure(volume, blocker, _player, _health,
                new RegionTransition("a", "b", 0, 0, RegionTransitionDirection.OneWay));
            Assert.That(blocker.enabled, Is.True);
            _encounter.RequestPreparation();
            yield return WaitReady();
            yield return new WaitForFixedUpdate();
            Assert.That(blocker.enabled, Is.False);
            Assert.That(_registry.ActiveCount, Is.Zero);
            Object.DestroyImmediate(_encounter.gameObject);
            yield return new WaitForFixedUpdate();
            Assert.That(blocker.enabled, Is.True, "A destroyed readiness source must fail closed.");
        }

        [UnityTest]
        public IEnumerator SphereVolumeUsesItsActualShapeRatherThanItsBoundingBox()
        {
            Initialize();
            var volume = New("Spherical activation volume").AddComponent<SphereCollider>();
            volume.isTrigger = true;
            volume.radius = 1;
            volume.gameObject.AddComponent<RegionPreparationTrigger>().Configure(volume,
                _player, _encounter, RegionPreparationTrigger.Purpose.Activate);
            _player.GetComponent<BoxCollider>().size = Vector3.one * .1f;
            _player.position = new Vector3(.9f, 0, .9f);
            Physics.SyncTransforms();
            yield return new WaitForFixedUpdate();
            yield return new WaitForFixedUpdate();
            Assert.That(_encounter.PreparationStatus, Is.EqualTo(RegionPreparationStatus.Idle));
            _player.position = Vector3.zero;
            Physics.SyncTransforms();
            yield return new WaitForFixedUpdate();
            yield return WaitReady();
            Assert.That(_encounter.Status, Is.EqualTo(ArenaSessionStatus.Running));
        }

        [Test]
        public void InactiveSpawnRequiresInactiveParent_ValidatesBudget()
        {
            var parent = New("Active parent");
            var request = Request(_prefab);
            Assert.Throws<ArgumentException>(() => _spawner.SpawnInactive(in request, parent.transform));
            Assert.Throws<ArgumentOutOfRangeException>(() =>
                _encounter.Initialize("b", _spawner, new[] { request }, _health, 0, 1));
            Assert.That(_encounter.IsInitialized, Is.False);
        }
    }
}
