using System.Reflection;
using NUnit.Framework;
using ProjectFirstRun.Builds;
using ProjectFirstRun.Items;
using ProjectFirstRun.Weapons;
using UnityEngine;
using Object = UnityEngine.Object;

namespace ProjectFirstRun.Tests.PlayMode.Weapons
{
    public sealed class PlayerWeaponAcquisitionControllerTests
    {
        private GameObject _playerObject;
        private GameObject _muzzleObject;
        private Camera _camera;

        private PlayerBuildController _buildController;
        private PlayerWeaponController _weaponController;
        private PlayerWeaponAcquisitionController
            _acquisitionController;

        private WeaponDefinition _firstWeapon;
        private WeaponDefinition _secondWeapon;

        [SetUp]
        public void SetUp()
        {
            _playerObject =
                new GameObject(
                    "WeaponAcquisition_Player");

            _playerObject.SetActive(
                false);

            GameObject cameraObject =
                new GameObject(
                    "WeaponAcquisition_Camera");

            _camera =
                cameraObject.AddComponent<Camera>();

            _muzzleObject =
                new GameObject(
                    "WeaponAcquisition_Muzzle");

            _muzzleObject.transform.SetParent(
                _playerObject.transform);

            _buildController =
                _playerObject
                    .AddComponent<PlayerBuildController>();

            _weaponController =
                _playerObject
                    .AddComponent<PlayerWeaponController>();

            SetPrivateField(
                _weaponController,
                typeof(PlayerWeaponController),
                "_aimCamera",
                _camera);

            SetPrivateField(
                _weaponController,
                typeof(PlayerWeaponController),
                "_muzzle",
                _muzzleObject.transform);

            _acquisitionController =
                _playerObject
                    .AddComponent<
                        PlayerWeaponAcquisitionController>();

            _playerObject.SetActive(
                true);

            _buildController.Initialize(
                new PlayerBuildCapacity(
                    2,
                    PlayerBuildCapacity.DefaultAbilitySlots,
                    PlayerBuildCapacity.DefaultUpgradeSlots));

            _firstWeapon =
                CreateWeapon(
                    "weapon.first");

            _secondWeapon =
                CreateWeapon(
                    "weapon.second");
        }

        [TearDown]
        public void TearDown()
        {
            if (_camera != null)
            {
                Object.DestroyImmediate(
                    _camera.gameObject);
            }

            if (_playerObject != null)
            {
                Object.DestroyImmediate(
                    _playerObject);
            }

            if (_firstWeapon != null)
            {
                Object.DestroyImmediate(
                    _firstWeapon);
            }

            if (_secondWeapon != null)
            {
                Object.DestroyImmediate(
                    _secondWeapon);
            }
        }

        [Test]
        public void NewController_HasEmptyLoadout()
        {
            Assert.That(
                _acquisitionController.Loadout,
                Is.Not.Null);

            Assert.That(
                _acquisitionController
                    .Loadout
                    .WeaponCount,
                Is.EqualTo(0));

            Assert.That(
                _acquisitionController
                    .Loadout
                    .HasActiveWeapon,
                Is.False);
        }

        [Test]
        public void TryAcquire_WithNullDefinition_Throws()
        {
            Assert.That(
                () =>
                    _acquisitionController.TryAcquire(
                        null),
                Throws.ArgumentNullException);
        }

        [Test]
        public void TryAcquire_FirstWeapon_ReturnsAcquired()
        {
            WeaponAcquireResult result =
                _acquisitionController.TryAcquire(
                    _firstWeapon);

            Assert.That(
                result,
                Is.EqualTo(
                    WeaponAcquireResult.Acquired));
        }

        [Test]
        public void TryAcquire_FirstWeapon_AddsBuildOwnership()
        {
            _acquisitionController.TryAcquire(
                _firstWeapon);

            Assert.That(
                _buildController.Contains(
                    _firstWeapon),
                Is.True);

            Assert.That(
                _buildController.Build.GetCount(
                    ItemCategory.Weapon),
                Is.EqualTo(1));
        }

        [Test]
        public void TryAcquire_FirstWeapon_AddsLoadoutEntry()
        {
            _acquisitionController.TryAcquire(
                _firstWeapon);

            Assert.That(
                _acquisitionController
                    .Loadout
                    .WeaponCount,
                Is.EqualTo(1));

            Assert.That(
                _acquisitionController
                    .Loadout
                    .Weapons[0],
                Is.SameAs(_firstWeapon));
        }

        [Test]
        public void TryAcquire_FirstWeapon_AutoEquipsWeapon()
        {
            _acquisitionController.TryAcquire(
                _firstWeapon);

            Assert.That(
                _acquisitionController
                    .Loadout
                    .ActiveDefinition,
                Is.SameAs(_firstWeapon));

            Assert.That(
                _weaponController.IsInitialized,
                Is.True);

            Assert.That(
                _weaponController.ActiveDefinition,
                Is.SameAs(_firstWeapon));
        }

        [Test]
        public void TryAcquire_SecondWeapon_PreservesFirstAsActive()
        {
            _acquisitionController.TryAcquire(
                _firstWeapon);

            WeaponAcquireResult result =
                _acquisitionController.TryAcquire(
                    _secondWeapon);

            Assert.That(
                result,
                Is.EqualTo(
                    WeaponAcquireResult.Acquired));

            Assert.That(
                _acquisitionController
                    .Loadout
                    .WeaponCount,
                Is.EqualTo(2));

            Assert.That(
                _acquisitionController
                    .Loadout
                    .Weapons[1],
                Is.SameAs(_secondWeapon));

            Assert.That(
                _acquisitionController
                    .Loadout
                    .ActiveDefinition,
                Is.SameAs(_firstWeapon));

            Assert.That(
                _weaponController.ActiveDefinition,
                Is.SameAs(_firstWeapon));
        }

        [Test]
        public void TryAcquire_WhenAlreadyOwned_ReturnsAlreadyOwnedWithoutMutation()
        {
            _acquisitionController.TryAcquire(
                _firstWeapon);

            WeaponAcquireResult result =
                _acquisitionController.TryAcquire(
                    _firstWeapon);

            Assert.That(
                result,
                Is.EqualTo(
                    WeaponAcquireResult.AlreadyOwned));

            Assert.That(
                _buildController.Build.GetCount(
                    ItemCategory.Weapon),
                Is.EqualTo(1));

            Assert.That(
                _acquisitionController
                    .Loadout
                    .WeaponCount,
                Is.EqualTo(1));

            Assert.That(
                _weaponController.ActiveDefinition,
                Is.SameAs(_firstWeapon));
        }

        [Test]
        public void TryAcquire_WhenCapacityReached_DoesNotMutateRuntime()
        {
            GameObject testObject =
                CreatePlayerWithWeaponCapacity(
                    1,
                    out PlayerBuildController buildController,
                    out PlayerWeaponController weaponController,
                    out PlayerWeaponAcquisitionController
                        acquisitionController);

            try
            {
                acquisitionController.TryAcquire(
                    _firstWeapon);

                WeaponAcquireResult result =
                    acquisitionController.TryAcquire(
                        _secondWeapon);

                Assert.That(
                    result,
                    Is.EqualTo(
                        WeaponAcquireResult.CapacityReached));

                Assert.That(
                    buildController.Contains(
                        _secondWeapon),
                    Is.False);

                Assert.That(
                    acquisitionController
                        .Loadout
                        .WeaponCount,
                    Is.EqualTo(1));

                Assert.That(
                    weaponController.ActiveDefinition,
                    Is.SameAs(_firstWeapon));
            }
            finally
            {
                Object.DestroyImmediate(
                    testObject);
            }
        }

        [Test]
        public void TryAcquire_WithInvalidRuntimeConfig_ThrowsWithoutMutation()
        {
            SetPrivateField(
                _firstWeapon,
                typeof(WeaponDefinition),
                "_magazineCapacity",
                0);

            Assert.That(
                () =>
                    _acquisitionController.TryAcquire(
                        _firstWeapon),
                Throws.TypeOf<
                    System.ArgumentOutOfRangeException>());

            Assert.That(
                _buildController.Contains(
                    _firstWeapon),
                Is.False);

            Assert.That(
                _acquisitionController
                    .Loadout
                    .WeaponCount,
                Is.EqualTo(0));

            Assert.That(
                _weaponController.IsInitialized,
                Is.False);
        }

        [Test]
        public void TryAcquire_PublishesWeaponAcquiredOnce()
        {
            int eventCount = 0;
            WeaponDefinition receivedDefinition = null;

            _acquisitionController.WeaponAcquired +=
                definition =>
                {
                    eventCount++;
                    receivedDefinition =
                        definition;
                };

            _acquisitionController.TryAcquire(
                _firstWeapon);

            _acquisitionController.TryAcquire(
                _firstWeapon);

            Assert.That(
                eventCount,
                Is.EqualTo(1));

            Assert.That(
                receivedDefinition,
                Is.SameAs(_firstWeapon));
        }

        [Test]
        public void TryAcquire_WhenBuildAndLoadoutAreInconsistent_Throws()
        {
            _buildController.TryAdd(
                _firstWeapon);

            Assert.That(
                () =>
                    _acquisitionController.TryAcquire(
                        _firstWeapon),
                Throws.InvalidOperationException);

            Assert.That(
                _acquisitionController
                    .Loadout
                    .WeaponCount,
                Is.EqualTo(0));

            Assert.That(
                _weaponController.IsInitialized,
                Is.False);
        }

        private GameObject CreatePlayerWithWeaponCapacity(
            int weaponCapacity,
            out PlayerBuildController buildController,
            out PlayerWeaponController weaponController,
            out PlayerWeaponAcquisitionController
                acquisitionController)
        {
            GameObject player =
                new GameObject(
                    "WeaponCapacity_Player");

            player.SetActive(
                false);

            GameObject muzzle =
                new GameObject(
                    "WeaponCapacity_Muzzle");

            muzzle.transform.SetParent(
                player.transform);

            buildController =
                player.AddComponent<PlayerBuildController>();

            weaponController =
                player.AddComponent<PlayerWeaponController>();

            SetPrivateField(
                weaponController,
                typeof(PlayerWeaponController),
                "_aimCamera",
                _camera);

            SetPrivateField(
                weaponController,
                typeof(PlayerWeaponController),
                "_muzzle",
                muzzle.transform);

            acquisitionController =
                player.AddComponent<
                    PlayerWeaponAcquisitionController>();

            player.SetActive(
                true);

            buildController.Initialize(
                new PlayerBuildCapacity(
                    weaponCapacity,
                    PlayerBuildCapacity.DefaultAbilitySlots,
                    PlayerBuildCapacity.DefaultUpgradeSlots));

            return player;
        }

        private static WeaponDefinition CreateWeapon(
            string stableId)
        {
            WeaponDefinition definition =
                ScriptableObject
                    .CreateInstance<WeaponDefinition>();

            SetPrivateField(
                definition,
                typeof(ItemDefinition),
                "_stableId",
                stableId);

            return definition;
        }

        private static void SetPrivateField(
            object target,
            System.Type declaringType,
            string fieldName,
            object value)
        {
            FieldInfo field =
                declaringType.GetField(
                    fieldName,
                    BindingFlags.Instance |
                    BindingFlags.NonPublic);

            Assert.That(
                field,
                Is.Not.Null);

            field.SetValue(
                target,
                value);
        }
    }
}