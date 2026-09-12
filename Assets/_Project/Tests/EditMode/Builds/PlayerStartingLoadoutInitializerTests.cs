using System.Reflection;
using NUnit.Framework;
using ProjectFirstRun.Builds;
using ProjectFirstRun.Items;
using ProjectFirstRun.Weapons;
using UnityEngine;
using Object = UnityEngine.Object;

namespace ProjectFirstRun.Tests.EditMode.Builds
{
    public sealed class PlayerStartingLoadoutInitializerTests
    {
        private GameObject _playerObject;

        private PlayerBuildController _buildController;
        private PlayerWeaponController _weaponController;
        private PlayerStartingLoadoutInitializer _initializer;
        private PlayerWeaponAcquisitionController
            _weaponAcquisitionController;

        private WeaponDefinition _startingWeapon;

        [SetUp]
        public void SetUp()
        {
            _playerObject =
                new GameObject(
                    "PlayerStartingLoadout_Test");

            _playerObject.SetActive(
                false);

            _buildController =
                _playerObject
                    .AddComponent<PlayerBuildController>();

            _weaponController =
                _playerObject
                    .AddComponent<PlayerWeaponController>();

            _weaponAcquisitionController =
                _playerObject
                    .AddComponent<
                        PlayerWeaponAcquisitionController>();

            _initializer =
                _playerObject
                    .AddComponent<
                        PlayerStartingLoadoutInitializer>();

            _startingWeapon =
                CreateWeaponDefinition(
                    "weapon.test_starting");

            SetPrivateField(
                _initializer,
                "_startingWeapon",
                _startingWeapon);
        }

        [TearDown]
        public void TearDown()
        {
            if (_playerObject != null)
            {
                Object.DestroyImmediate(
                    _playerObject);
            }

            if (_startingWeapon != null)
            {
                Object.DestroyImmediate(
                    _startingWeapon);
            }
        }

        [Test]
        public void NewInitializer_IsNotInitialized()
        {
            Assert.That(
                _initializer.IsInitialized,
                Is.False);
        }

        [Test]
        public void Initialize_WithDefaultCapacity_InitializesBuild()
        {
            PlayerBuildCapacity capacity =
                PlayerBuildCapacity.CreateDefault();

            _initializer.Initialize(
                capacity);

            Assert.That(
                _initializer.IsInitialized,
                Is.True);

            Assert.That(
                _buildController.IsInitialized,
                Is.True);

            Assert.That(
                _buildController.Build.Capacity,
                Is.SameAs(capacity));
        }

        [Test]
        public void Initialize_AddsStartingWeaponToBuild()
        {
            _initializer.Initialize(
                PlayerBuildCapacity.CreateDefault());

            Assert.That(
                _buildController.Contains(
                    _startingWeapon),
                Is.True);

            Assert.That(
                _buildController.Build.GetCount(
                    ItemCategory.Weapon),
                Is.EqualTo(1));

            Assert.That(
                _buildController.Build.Weapons[0],
                Is.EqualTo(
                    "weapon.test_starting"));
        }

        [Test]
        public void Initialize_InitializesWeaponRuntimeWithSameDefinition()
        {
            _initializer.Initialize(
                PlayerBuildCapacity.CreateDefault());

            Assert.That(
                _weaponController.IsInitialized,
                Is.True);

            Assert.That(
                _weaponController.ActiveDefinition,
                Is.SameAs(_startingWeapon));
        }

        [Test]
        public void Initialize_WithCustomCapacity_PreservesCapacity()
        {
            PlayerBuildCapacity capacity =
                new PlayerBuildCapacity(
                    2,
                    4,
                    6);

            _initializer.Initialize(
                capacity);

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
        }

        [Test]
        public void Initialize_WithNullCapacity_Throws()
        {
            Assert.That(
                () => _initializer.Initialize(null),
                Throws.ArgumentNullException);

            Assert.That(
                _initializer.IsInitialized,
                Is.False);

            Assert.That(
                _buildController.IsInitialized,
                Is.False);

            Assert.That(
                _weaponController.IsInitialized,
                Is.False);
        }

        [Test]
        public void Initialize_WithoutStartingWeapon_Throws()
        {
            SetPrivateField(
                _initializer,
                "_startingWeapon",
                null);

            Assert.That(
                () =>
                    _initializer.Initialize(
                        PlayerBuildCapacity.CreateDefault()),
                Throws.InvalidOperationException);

            Assert.That(
                _initializer.IsInitialized,
                Is.False);

            Assert.That(
                _buildController.IsInitialized,
                Is.False);

            Assert.That(
                _weaponController.IsInitialized,
                Is.False);
        }

        [Test]
        public void Initialize_WhenAlreadyInitialized_Throws()
        {
            _initializer.Initialize(
                PlayerBuildCapacity.CreateDefault());

            Assert.That(
                () =>
                    _initializer.Initialize(
                        PlayerBuildCapacity.CreateDefault()),
                Throws.InvalidOperationException);

            Assert.That(
                _buildController.Build.GetCount(
                    ItemCategory.Weapon),
                Is.EqualTo(1));
        }

        [Test]
        public void Initialize_WhenBuildControllerAlreadyInitialized_Throws()
        {
            _buildController.Initialize(
                PlayerBuildCapacity.CreateDefault());

            Assert.That(
                () =>
                    _initializer.Initialize(
                        PlayerBuildCapacity.CreateDefault()),
                Throws.InvalidOperationException);

            Assert.That(
                _initializer.IsInitialized,
                Is.False);

            Assert.That(
                _weaponController.IsInitialized,
                Is.False);
        }

        [Test]
        public void Initialize_WhenWeaponControllerAlreadyInitialized_Throws()
        {
            _weaponController.Initialize(
                _startingWeapon);

            Assert.That(
                () =>
                    _initializer.Initialize(
                        PlayerBuildCapacity.CreateDefault()),
                Throws.InvalidOperationException);

            Assert.That(
                _initializer.IsInitialized,
                Is.False);

            Assert.That(
                _buildController.IsInitialized,
                Is.False);
        }
        
        [Test]
        public void Initialize_WithInvalidStartingWeaponConfig_ThrowsBeforeBuildInitialization()
        {
            SetPrivateField(
                _startingWeapon,
                "_magazineCapacity",
                0,
                typeof(WeaponDefinition));

            Assert.That(
                () =>
                    _initializer.Initialize(
                        PlayerBuildCapacity.CreateDefault()),
                Throws.TypeOf<
                    System.ArgumentOutOfRangeException>());

            Assert.That(
                _initializer.IsInitialized,
                Is.False);

            Assert.That(
                _buildController.IsInitialized,
                Is.False);

            Assert.That(
                _weaponController.IsInitialized,
                Is.False);

            Assert.That(
                _weaponAcquisitionController
                    .Loadout
                    .WeaponCount,
                Is.EqualTo(0));
        }

        private static WeaponDefinition
            CreateWeaponDefinition(
                string stableId)
        {
            WeaponDefinition definition =
                ScriptableObject
                    .CreateInstance<WeaponDefinition>();

            SetPrivateField(
                definition,
                "_stableId",
                stableId,
                typeof(ItemDefinition));

            return definition;
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