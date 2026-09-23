#if UNITY_EDITOR
using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using NUnit.Framework;
using ProjectFirstRun.Arenas;
using ProjectFirstRun.Builds;
using ProjectFirstRun.Chests;
using ProjectFirstRun.Chests.Spawning;
using ProjectFirstRun.Combat;
using ProjectFirstRun.Items;
using ProjectFirstRun.Player;
using ProjectFirstRun.Progression;
using ProjectFirstRun.Rewards;
using ProjectFirstRun.UI.Rewards;
using ProjectFirstRun.Weapons;
using UnityEditor;
using UnityEngine;
using UnityEngine.TestTools;
using Object = UnityEngine.Object;

namespace ProjectFirstRun.Tests.PlayMode.Arenas
{
    public sealed class MapTraversalIntegrationTests
    {
        private readonly List<GameObject> _objects = new List<GameObject>();
        private GameObject _player;
        private HealthComponent _health;
        private MapTraversalController _map;
        private RegionPassageController _side, _next;

        private GameObject New(string name)
        {
            var result = new GameObject(name);
            _objects.Add(result);
            return result;
        }

        [SetUp]
        public void Setup()
        {
            Time.timeScale = 1;
            _player = Object.Instantiate(AssetDatabase.LoadAssetAtPath<GameObject>(
                "Assets/_Project/Prefabs/Player/Player.prefab"));
            _objects.Add(_player);
            _player.GetComponent<PlayerStartingLoadoutInitializer>().Initialize(new PlayerBuildCapacity(2, 3, 5));
            _player.GetComponent<PlayerController>().enabled = false; // Deterministic positions, no gravity/input ticks.
            _player.transform.position = new Vector3(0, 0, -4);
            _health = _player.GetComponent<HealthComponent>();
            _map = New("Map").AddComponent<MapTraversalController>();
            _map.Initialize("main", new[] { "main", "side", "next" });
            _side = Passage("side", Vector3.zero, RegionTransitionDirection.Returnable);
            _next = Passage("next", new Vector3(10, 0, 0), RegionTransitionDirection.OneWay);
        }

        private RegionPassageController Passage(string to, Vector3 position, RegionTransitionDirection direction)
        {
            var root = New(to + " passage");
            root.transform.position = position;
            var volume = root.AddComponent<BoxCollider>();
            volume.size = new Vector3(4, 8, 4);
            volume.isTrigger = true;
            var body = root.AddComponent<Rigidbody>();
            body.isKinematic = true;
            body.useGravity = false;
            var barrier = New(to + " blocker").AddComponent<BoxCollider>();
            barrier.transform.position = position;
            barrier.size = new Vector3(3, 6, .2f);
            var passage = root.AddComponent<RegionPassageController>();
            // Exercise the same serialized map reference used by scene composition.
            typeof(RegionPassageController).GetField("_map", BindingFlags.Instance | BindingFlags.NonPublic)
                .SetValue(passage, _map);
            Physics.SyncTransforms();
            passage.Configure(volume, barrier, _player.transform, _health,
                new RegionTransition("main", to, RegionTransitionRequirement.Free,
                    RegionTransitionTraversal.Walk, direction));
            return passage;
        }

        [TearDown]
        public void Teardown()
        {
            Time.timeScale = 1;
            for (int i = _objects.Count - 1; i >= 0; i--)
                if (_objects[i] != null) Object.DestroyImmediate(_objects[i]);
            _objects.Clear();
        }

        private IEnumerator Move(float x, float z)
        {
            _player.transform.position = new Vector3(x, 0, z);
            Physics.SyncTransforms();
            yield return new WaitForFixedUpdate();
            yield return new WaitForFixedUpdate();
            yield return null;
        }

        [UnityTest]
        public IEnumerator SideTripThenOneWay_PreservesRealPlayerBuildAmmoXpAndUncollectedLoot()
        {
            var build = _player.GetComponent<PlayerBuildController>();
            var originalBuild = build.Build;
            var weapon = _player.GetComponent<PlayerWeaponController>();
            var acquisition = _player.GetComponent<PlayerWeaponAcquisitionController>();
            acquisition.TryLevelUp(weapon.ActiveDefinition);
            var entry = weapon.ActiveEntry;
            Assert.That(entry.RuntimeState.TryFire(), Is.EqualTo(WeaponFireResult.Fired));
            int ammo = entry.RuntimeState.MagazineAmmo;
            int reserve = entry.RuntimeState.ReserveAmmo;
            _health.ApplyDamage(new DamageInfo(17, null, Vector3.zero, Vector3.forward));
            var collector = _player.GetComponent<PlayerExperienceCollector>();
            if (collector == null) collector = _player.AddComponent<PlayerExperienceCollector>();
            collector.SetAttractionRadius(0);
            var xp = collector.Experience;
            if (!xp.IsInitialized) xp.Initialize(new ExperienceState(new ExperienceCurve(100, 50)));
            xp.GainExperience(125);
            long total = xp.TotalExperience;
            int level = xp.Level;

            var pickup = New("Left behind XP").AddComponent<ExperiencePickup>();
            pickup.transform.position = new Vector3(20, 0, -10);
            pickup.GetComponent<SphereCollider>().isTrigger = true;
            pickup.GetComponent<Rigidbody>().isKinematic = true;
            pickup.Initialize(25);
            var spawner = New("Map chest spawner").AddComponent<ChestSpawner>();
            spawner.Initialize(build, New("Selection").AddComponent<RewardSelectionController>(),
                new RewardOfferGenerator(new RewardCandidateFilter(), new FirstRandom()));
            var definition = AssetDatabase.LoadAssetAtPath<ChestDefinition>(
                "Assets/_Project/Data/Chests/Dev/CD_WeaponChest.asset");
            var chest = spawner.Spawn(new ChestSpawnRequest(definition,
                new Vector3(20, 0, -15), Quaternion.identity)).ChestController;
            _objects.Add(chest.gameObject);

            yield return Move(0, -1);
            Assert.That(_map.Session.CurrentRegionId, Is.EqualTo("main"));
            Assert.That(_next.Barrier.Status, Is.EqualTo(TransitionBarrierStatus.Closed));
            yield return Move(0, 4);
            Assert.That(_next.CurrentRegionId, Is.EqualTo("side"));
            yield return Move(0, 1);
            yield return Move(0, -4);
            Assert.That(_map.Session.CurrentRegionId, Is.EqualTo("main"));
            yield return Move(10, -4);
            yield return Move(10, -1);
            yield return Move(10, 4);
            Assert.That(_side.CurrentRegionId, Is.EqualTo("next"));
            Assert.That(_next.Barrier.Status, Is.EqualTo(TransitionBarrierStatus.Closed));
            Assert.That(build.Build, Is.SameAs(originalBuild));
            Assert.That(weapon.ActiveEntry, Is.SameAs(entry));
            Assert.That(entry.Level, Is.EqualTo(2));
            Assert.That(build.Build.GetLevel(ItemCategory.Weapon, entry.Definition.StableId), Is.EqualTo(2));
            Assert.That(entry.RuntimeState.MagazineAmmo, Is.EqualTo(ammo));
            Assert.That(entry.RuntimeState.ReserveAmmo, Is.EqualTo(reserve));
            Assert.That(_health.CurrentHealth, Is.EqualTo(83));
            Assert.That(xp.TotalExperience, Is.EqualTo(total));
            Assert.That(xp.Level, Is.EqualTo(level));
            Assert.That(pickup != null && !pickup.IsCollected, Is.True);
            Assert.That(chest != null && chest.IsInitialized, Is.True);
            Assert.That(chest.Status, Is.EqualTo(ChestStatus.Available));
        }

        [UnityTest]
        public IEnumerator DisabledPendingPassage_ReleasesSharedLockWithoutAdvancing()
        {
            yield return Move(0, -1);
            Assert.That(_map.Session.IsTransitioning, Is.True);
            _side.enabled = false;
            Assert.That(_map.Session.IsTransitioning, Is.False);
            Assert.That(_map.Session.CurrentRegionId, Is.EqualTo("main"));
            yield return Move(10, -4);
            yield return Move(10, -1);
            yield return Move(10, 4);
            Assert.That(_map.Session.CurrentRegionId, Is.EqualTo("next"));
        }

        [UnityTest]
        public IEnumerator DeathInPassage_DoesNotAdvanceAndReleasesSharedLock()
        {
            yield return Move(0, -1);
            Assert.That(_map.Session.IsTransitioning, Is.True);
            _health.ApplyDamage(new DamageInfo(1000, null, Vector3.zero, Vector3.forward));
            yield return Move(0, 4);
            Assert.That(_map.Session.IsTransitioning, Is.False);
            Assert.That(_map.Session.CurrentRegionId, Is.EqualTo("main"));
        }

        [Test]
        public void MapController_CannotReinitializeAndRejectsUnknownPassage()
        {
            Assert.Throws<InvalidOperationException>(() => _map.Initialize("main", new[] { "main" }));
            Assert.Throws<ArgumentException>(() => Passage("missing", Vector3.one * 100,
                RegionTransitionDirection.Returnable));
        }

        private sealed class FirstRandom : IRandomSource
        {
            public int Next(int minInclusive, int maxExclusive) => minInclusive;
        }
    }
}
#endif
