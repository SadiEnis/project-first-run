#if UNITY_EDITOR
using System;
using System.Collections;
using System.Reflection;
using NUnit.Framework;
using ProjectFirstRun.Abilities;
using ProjectFirstRun.Builds;
using ProjectFirstRun.Enemies;
using ProjectFirstRun.Stats;
using UnityEditor;
using UnityEngine;
using UnityEngine.TestTools;
using Object = UnityEngine.Object;

namespace ProjectFirstRun.Tests.PlayMode.Abilities
{
    public sealed class AbilityRuntimeInstallerTests
    {
        private GameObject _player, _services;
        private PlayerStartingLoadoutInitializer _loadout;
        private PlayerAbilityRuntimeInstaller _installer;

        [SetUp]
        public void SetUp()
        {
            _player = Object.Instantiate(AssetDatabase.LoadAssetAtPath<GameObject>(
                "Assets/_Project/Prefabs/Player/Player.prefab"));
            _loadout = _player.GetComponent<PlayerStartingLoadoutInitializer>();
            _loadout.enabled = false; // Tests control when and with which capacity it becomes ready.
            _services = new GameObject("Ability installation test");
            var enemies = _services.AddComponent<EnemyRegistry>();
            _installer = _services.AddComponent<PlayerAbilityRuntimeInstaller>();
            _installer.Configure(_player.GetComponent<PlayerAbilityAcquisitionController>(),
                _player.GetComponent<PlayerStatsController>(), _player, enemies);
        }

        private void InitializeLoadout() => _loadout.Initialize(new PlayerBuildCapacity(
            PlayerBuildCapacity.MaximumWeaponSlots, PlayerBuildCapacity.MaximumAbilitySlots,
            PlayerBuildCapacity.MaximumUpgradeSlots));

        [Test]
        public void ReadyBeforeBegin_InstallsAllFactoriesWithoutGrantingOrChangingCapacity()
        {
            InitializeLoadout();
            _installer.Begin();
            Assert.That(_installer.IsInstalled, Is.True);
            Assert.That(_player.GetComponent<PlayerAbilityController>().AbilityCount, Is.Zero);
            var acquisition = _player.GetComponent<PlayerAbilityAcquisitionController>();
            var registry = (AbilityRuntimeFactoryRegistry)typeof(PlayerAbilityAcquisitionController)
                .GetField("_factoryRegistry", BindingFlags.Instance | BindingFlags.NonPublic)
                .GetValue(acquisition);
            Assert.That(registry.FactoryCount, Is.EqualTo(8));
            foreach (string asset in new[] { "Fireball/AD_Fireball", "AD_ForceWave", "AD_Drone",
                "AD_AcidBottle", "AD_LightningStaff", "AD_Shuriken", "AD_EnchantedStaff", "AD_SniperBomb" })
            {
                var definition = AssetDatabase.LoadAssetAtPath<AbilityDefinition>(
                    "Assets/_Project/Data/Abilities/" + asset + ".asset");
                Assert.That(definition, Is.Not.Null, asset);
                Assert.That(registry.Resolve(definition), Is.Not.Null, asset);
            }
            Assert.That(_loadout.IsInitialized, Is.True);
            var capacity = _player.GetComponent<PlayerBuildController>().Build.Capacity;
            Assert.That(capacity.WeaponSlots, Is.EqualTo(2));
            Assert.That(capacity.AbilitySlots, Is.EqualTo(5));
            Assert.That(capacity.UpgradeSlots, Is.EqualTo(8));
            Assert.That(_player.GetComponent<PlayerBuildController>().Build.Abilities, Is.Empty);
        }

        [UnityTest]
        public IEnumerator LoadoutReadyBeforeStart_InstallsAutomatically()
        {
            InitializeLoadout();
            yield return null;
            Assert.That(_installer.IsInstalled, Is.True);
            Assert.That(_player.GetComponent<PlayerAbilityController>().AbilityCount, Is.Zero);
        }

        [UnityTest]
        public IEnumerator StartWaitsForExplicitLoadoutReadiness_WithoutDefaultInitialization()
        {
            yield return null;
            Assert.That(_installer.IsInstalled, Is.False);
            Assert.That(_loadout.IsInitialized, Is.False);
            InitializeLoadout();
            Assert.That(_installer.IsInstalled, Is.True, "Readiness installs synchronously, not next frame.");
            _installer.enabled = false;
            _installer.enabled = true;
            yield return null;
            Assert.That(_player.GetComponent<PlayerAbilityController>().AbilityCount, Is.Zero);
        }

        [UnityTest]
        public IEnumerator DisabledWhileWaiting_UnsubscribesAndResumesWhenEnabled()
        {
            yield return null;
            _installer.enabled = false;
            InitializeLoadout();
            Assert.That(_installer.IsInstalled, Is.False);
            _installer.enabled = true;
            Assert.That(_installer.IsInstalled, Is.True);
        }

        [Test]
        public void ExplicitInstallBeforeReadiness_FailsWithoutInitializingAcquisition()
        {
            Assert.Throws<InvalidOperationException>(() => _installer.Install());
            Assert.That(_player.GetComponent<PlayerAbilityAcquisitionController>().IsInitialized, Is.False);
            InitializeLoadout();
            _installer.Install();
            Assert.Throws<InvalidOperationException>(() => _installer.Install());
        }

        [Test]
        public void CompetingOwner_IsRejected()
        {
            InitializeLoadout();
            _player.GetComponent<PlayerAbilityAcquisitionController>().Initialize(new AbilityRuntimeFactoryRegistry());
            Assert.Throws<InvalidOperationException>(() => _installer.Install());
            Assert.That(_installer.IsInstalled, Is.False);
        }

        [Test]
        public void MissingDependency_IsRejectedBeforeAcquisitionIsModified()
        {
            _installer.Configure(_player.GetComponent<PlayerAbilityAcquisitionController>(),
                _player.GetComponent<PlayerStatsController>(), _player, null);
            Assert.Throws<InvalidOperationException>(() => _installer.Begin());
            Assert.That(_player.GetComponent<PlayerAbilityAcquisitionController>().IsInitialized, Is.False);
        }

        [TearDown]
        public void TearDown()
        {
            Object.DestroyImmediate(_services);
            Object.DestroyImmediate(_player);
        }
    }
}
#endif
