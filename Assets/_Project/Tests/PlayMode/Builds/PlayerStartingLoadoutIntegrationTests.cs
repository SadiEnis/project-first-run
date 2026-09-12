using System.Collections;
using System.Reflection;
using NUnit.Framework;
using ProjectFirstRun.Builds;
using ProjectFirstRun.Input;
using ProjectFirstRun.Items;
using ProjectFirstRun.Weapons;
using UnityEngine;
using UnityEngine.TestTools;
using Object = UnityEngine.Object;

namespace ProjectFirstRun.Tests.PlayMode.Builds
{
    public sealed class PlayerStartingLoadoutIntegrationTests
    {
        private GameObject _playerObject;
        private GameObject _cameraObject;
        private GameObject _muzzleObject;

        private PlayerBuildController _buildController;
        private PlayerWeaponController _weaponController;
        private PlayerStartingLoadoutInitializer _initializer;

        private WeaponDefinition _startingWeapon;

        [SetUp]
        public void SetUp()
        {
            CreateStartingWeapon();
            CreatePlayer();
        }

        [TearDown]
        public void TearDown()
        {
            if (_playerObject != null)
            {
                Object.DestroyImmediate(
                    _playerObject);
            }

            if (_cameraObject != null)
            {
                Object.DestroyImmediate(
                    _cameraObject);
            }

            if (_muzzleObject != null)
            {
                Object.DestroyImmediate(
                    _muzzleObject);
            }

            if (_startingWeapon != null)
            {
                Object.DestroyImmediate(
                    _startingWeapon);
            }
        }

        [UnityTest]
        public IEnumerator Start_InitializesBuildAndWeaponRuntime()
        {
            Assert.That(
                _buildController.IsInitialized,
                Is.False);

            Assert.That(
                _weaponController.IsInitialized,
                Is.False);

            Assert.That(
                _initializer.IsInitialized,
                Is.False);

            _playerObject.SetActive(true);

            yield return null;

            Assert.That(
                _initializer.IsInitialized,
                Is.True);

            Assert.That(
                _buildController.IsInitialized,
                Is.True);

            Assert.That(
                _weaponController.IsInitialized,
                Is.True);

            Assert.That(
                _weaponController.ActiveDefinition,
                Is.SameAs(_startingWeapon));
        }

        [UnityTest]
        public IEnumerator Start_AddsStartingWeaponExactlyOnce()
        {
            _playerObject.SetActive(true);

            yield return null;

            Assert.That(
                _buildController.Build.GetCount(
                    ItemCategory.Weapon),
                Is.EqualTo(1));

            Assert.That(
                _buildController.Contains(
                    _startingWeapon),
                Is.True);

            Assert.That(
                _buildController.Build.Weapons[0],
                Is.EqualTo(
                    "weapon.integration_starting"));

            yield return null;

            Assert.That(
                _buildController.Build.GetCount(
                    ItemCategory.Weapon),
                Is.EqualTo(1));
        }

        [UnityTest]
        public IEnumerator Start_UsesDefaultBuildCapacity()
        {
            _playerObject.SetActive(true);

            yield return null;

            PlayerBuild build =
                _buildController.Build;

            Assert.That(
                build.GetCapacity(
                    ItemCategory.Weapon),
                Is.EqualTo(
                    PlayerBuildCapacity.DefaultWeaponSlots));

            Assert.That(
                build.GetCapacity(
                    ItemCategory.Ability),
                Is.EqualTo(
                    PlayerBuildCapacity.DefaultAbilitySlots));

            Assert.That(
                build.GetCapacity(
                    ItemCategory.Upgrade),
                Is.EqualTo(
                    PlayerBuildCapacity.DefaultUpgradeSlots));
        }

        [UnityTest]
        public IEnumerator Start_AfterExplicitInitialization_DoesNotInitializeAgain()
        {
            PlayerBuildCapacity customCapacity =
                new PlayerBuildCapacity(
                    2,
                    4,
                    6);

            _initializer.Initialize(
                customCapacity);

            PlayerBuild buildBeforeActivation =
                _buildController.Build;

            Assert.That(
                _initializer.IsInitialized,
                Is.True);

            Assert.That(
                _buildController.Build.GetCapacity(
                    ItemCategory.Weapon),
                Is.EqualTo(2));

            _playerObject.SetActive(true);

            yield return null;

            Assert.That(
                _buildController.Build,
                Is.SameAs(buildBeforeActivation));

            Assert.That(
                _buildController.Build.GetCapacity(
                    ItemCategory.Weapon),
                Is.EqualTo(2));

            Assert.That(
                _buildController.Build.GetCapacity(
                    ItemCategory.Ability),
                Is.EqualTo(4));

            Assert.That(
                _buildController.Build.GetCapacity(
                    ItemCategory.Upgrade),
                Is.EqualTo(6));

            Assert.That(
                _buildController.Build.GetCount(
                    ItemCategory.Weapon),
                Is.EqualTo(1));

            Assert.That(
                _weaponController.ActiveDefinition,
                Is.SameAs(_startingWeapon));
        }

        private void CreatePlayer()
        {
            _playerObject =
                new GameObject(
                    "PlayerStartingLoadout_Integration");

            /*
             * Configure the complete object before Unity invokes
             * Awake/Start when it becomes active.
             */
            _playerObject.SetActive(false);

            _cameraObject =
                new GameObject(
                    "PlayerStartingLoadout_Camera");

            Camera aimCamera =
                _cameraObject.AddComponent<Camera>();

            _muzzleObject =
                new GameObject(
                    "PlayerStartingLoadout_Muzzle");

            _muzzleObject.transform.SetParent(
                _playerObject.transform);

            /*
             * PlayerWeaponController requires PlayerInputReader.
             * Input itself is irrelevant to this integration test,
             * so both input and weapon ticking are disabled.
             *
             * Public initialization still remains available.
             */
            PlayerInputReader inputReader =
                _playerObject.AddComponent<PlayerInputReader>();

            inputReader.enabled = false;

            _weaponController =
                _playerObject.AddComponent<PlayerWeaponController>();

            _weaponController.enabled = false;

            SetPrivateField(
                _weaponController,
                "_aimCamera",
                aimCamera);

            SetPrivateField(
                _weaponController,
                "_muzzle",
                _muzzleObject.transform);

            SetPrivateField(
                _weaponController,
                "_damageSource",
                _playerObject);

            _buildController =
                _playerObject.AddComponent<PlayerBuildController>();

            _initializer =
                _playerObject
                    .AddComponent<PlayerStartingLoadoutInitializer>();

            SetPrivateField(
                _initializer,
                "_startingWeapon",
                _startingWeapon);
        }

        private void CreateStartingWeapon()
        {
            _startingWeapon =
                ScriptableObject
                    .CreateInstance<WeaponDefinition>();

            SetPrivateField(
                _startingWeapon,
                "_stableId",
                "weapon.integration_starting",
                typeof(ItemDefinition));
        }

        private static void SetPrivateField(
            object target,
            string fieldName,
            object value,
            System.Type declaringType = null)
        {
            System.Type targetType =
                declaringType ??
                target.GetType();

            FieldInfo field =
                targetType.GetField(
                    fieldName,
                    BindingFlags.Instance |
                    BindingFlags.NonPublic);

            Assert.That(
                field,
                Is.Not.Null,
                $"Field '{fieldName}' could not be found.");

            field.SetValue(
                target,
                value);
        }
    }
}