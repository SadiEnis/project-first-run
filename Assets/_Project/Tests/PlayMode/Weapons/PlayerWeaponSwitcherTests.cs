using System.Collections;
using System.Reflection;
using NUnit.Framework;
using ProjectFirstRun.Builds;
using ProjectFirstRun.Items;
using ProjectFirstRun.Weapons;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.TestTools;
using Object = UnityEngine.Object;

namespace ProjectFirstRun.Tests.PlayMode.Weapons
{
    public sealed class PlayerWeaponSwitcherTests :
        InputTestFixture
    {
        private GameObject _playerObject;
        private GameObject _muzzleObject;
        private Camera _camera;

        private PlayerBuildController _buildController;
        private PlayerWeaponController _weaponController;
        private PlayerWeaponAcquisitionController
            _acquisitionController;
        private PlayerWeaponSwitcher _weaponSwitcher;

        private Keyboard _keyboard;
        private Gamepad _gamepad;

        private WeaponDefinition _firstWeapon;
        private WeaponDefinition _secondWeapon;

        [SetUp]
        public void SetUp()
        {
            _keyboard =
                InputSystem.AddDevice<Keyboard>();

            _gamepad =
                InputSystem.AddDevice<Gamepad>();

            _playerObject =
                new GameObject(
                    "WeaponSwitcher_Player");

            _playerObject.SetActive(
                false);

            GameObject cameraObject =
                new GameObject(
                    "WeaponSwitcher_Camera");

            _camera =
                cameraObject.AddComponent<Camera>();

            _muzzleObject =
                new GameObject(
                    "WeaponSwitcher_Muzzle");

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

            _weaponSwitcher =
                _playerObject
                    .AddComponent<
                        PlayerWeaponSwitcher>();

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
        public void CleanUp()
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
        public void TrySwitchNext_WithNoWeapons_ReturnsFalse()
        {
            bool result =
                _weaponSwitcher.TrySwitchNext();

            Assert.That(
                result,
                Is.False);

            Assert.That(
                _acquisitionController
                    .Loadout
                    .HasActiveWeapon,
                Is.False);

            Assert.That(
                _weaponController.IsInitialized,
                Is.False);
        }

        [Test]
        public void TrySwitchNext_WithSingleWeapon_ReturnsFalse()
        {
            _acquisitionController.TryAcquire(
                _firstWeapon);

            PlayerWeaponRuntimeEntry originalEntry =
                _acquisitionController
                    .Loadout
                    .ActiveEntry;

            bool result =
                _weaponSwitcher.TrySwitchNext();

            Assert.That(
                result,
                Is.False);

            Assert.That(
                _acquisitionController
                    .Loadout
                    .ActiveEntry,
                Is.SameAs(originalEntry));

            Assert.That(
                _weaponController.ActiveEntry,
                Is.SameAs(originalEntry));
        }

        [Test]
        public void TrySwitchNext_WithTwoWeapons_SwitchesFirstToSecond()
        {
            _acquisitionController.TryAcquire(
                _firstWeapon);

            _acquisitionController.TryAcquire(
                _secondWeapon);

            PlayerWeaponRuntimeEntry secondEntry =
                _acquisitionController
                    .Loadout
                    .GetEntry(
                        _secondWeapon);

            bool result =
                _weaponSwitcher.TrySwitchNext();

            Assert.That(
                result,
                Is.True);

            Assert.That(
                _acquisitionController
                    .Loadout
                    .ActiveEntry,
                Is.SameAs(secondEntry));

            Assert.That(
                _weaponController.ActiveEntry,
                Is.SameAs(secondEntry));

            Assert.That(
                _weaponController.ActiveDefinition,
                Is.SameAs(_secondWeapon));
        }

        [Test]
        public void TrySwitchNext_FromSecondWeapon_WrapsToFirst()
        {
            _acquisitionController.TryAcquire(
                _firstWeapon);

            _acquisitionController.TryAcquire(
                _secondWeapon);

            PlayerWeaponRuntimeEntry firstEntry =
                _acquisitionController
                    .Loadout
                    .GetEntry(
                        _firstWeapon);

            _weaponSwitcher.TrySwitchNext();

            bool result =
                _weaponSwitcher.TrySwitchNext();

            Assert.That(
                result,
                Is.True);

            Assert.That(
                _acquisitionController
                    .Loadout
                    .ActiveEntry,
                Is.SameAs(firstEntry));

            Assert.That(
                _weaponController.ActiveEntry,
                Is.SameAs(firstEntry));

            Assert.That(
                _weaponController.ActiveDefinition,
                Is.SameAs(_firstWeapon));
        }

        [Test]
        public void TrySwitchNext_FirstSecondFirst_RestoresExactRuntimeState()
        {
            _acquisitionController.TryAcquire(
                _firstWeapon);

            _acquisitionController.TryAcquire(
                _secondWeapon);

            PlayerWeaponRuntimeEntry firstEntry =
                _acquisitionController
                    .Loadout
                    .GetEntry(
                        _firstWeapon);

            PlayerWeaponRuntimeEntry secondEntry =
                _acquisitionController
                    .Loadout
                    .GetEntry(
                        _secondWeapon);

            WeaponRuntimeState firstRuntimeState =
                firstEntry.RuntimeState;

            WeaponRuntimeState secondRuntimeState =
                secondEntry.RuntimeState;

            _weaponSwitcher.TrySwitchNext();

            Assert.That(
                _weaponController
                    .ActiveEntry
                    .RuntimeState,
                Is.SameAs(secondRuntimeState));

            _weaponSwitcher.TrySwitchNext();

            Assert.That(
                _weaponController.ActiveEntry,
                Is.SameAs(firstEntry));

            Assert.That(
                _weaponController
                    .ActiveEntry
                    .RuntimeState,
                Is.SameAs(firstRuntimeState));
        }

        [Test]
        public void TrySwitchNext_PublishesActiveWeaponChangedOnce()
        {
            _acquisitionController.TryAcquire(
                _firstWeapon);

            _acquisitionController.TryAcquire(
                _secondWeapon);

            int eventCount = 0;

            WeaponDefinition previousDefinition =
                null;

            WeaponDefinition currentDefinition =
                null;

            _weaponSwitcher.ActiveWeaponChanged +=
                (previous, current) =>
                {
                    eventCount++;

                    previousDefinition =
                        previous;

                    currentDefinition =
                        current;
                };

            bool result =
                _weaponSwitcher.TrySwitchNext();

            Assert.That(
                result,
                Is.True);

            Assert.That(
                eventCount,
                Is.EqualTo(1));

            Assert.That(
                previousDefinition,
                Is.SameAs(_firstWeapon));

            Assert.That(
                currentDefinition,
                Is.SameAs(_secondWeapon));
        }

        [Test]
        public void TrySwitchNext_WithSingleWeapon_DoesNotPublishEvent()
        {
            _acquisitionController.TryAcquire(
                _firstWeapon);

            int eventCount = 0;

            _weaponSwitcher.ActiveWeaponChanged +=
                (_, _) =>
                {
                    eventCount++;
                };

            _weaponSwitcher.TrySwitchNext();

            Assert.That(
                eventCount,
                Is.EqualTo(0));
        }

        [Test]
        public void TrySwitchNext_WhenWeaponControlIsDisabled_ReturnsFalse()
        {
            _acquisitionController.TryAcquire(
                _firstWeapon);

            _acquisitionController.TryAcquire(
                _secondWeapon);

            _weaponController.SetWeaponControlEnabled(
                false);

            bool result =
                _weaponSwitcher.TrySwitchNext();

            Assert.That(
                result,
                Is.False);

            Assert.That(
                _weaponController.ActiveDefinition,
                Is.SameAs(_firstWeapon));
        }

        [UnityTest]
        public IEnumerator Update_WithKeyboardSwitchInput_SwitchesWeapon()
        {
            _acquisitionController.TryAcquire(
                _firstWeapon);

            _acquisitionController.TryAcquire(
                _secondWeapon);

            Press(
                _keyboard.qKey,
                queueEventOnly: true);

            yield return null;

            Assert.That(
                _weaponController.ActiveDefinition,
                Is.SameAs(_secondWeapon));
        }

        [UnityTest]
        public IEnumerator Update_WithGamepadSwitchInput_SwitchesWeapon()
        {
            _acquisitionController.TryAcquire(
                _firstWeapon);

            _acquisitionController.TryAcquire(
                _secondWeapon);

            Press(
                _gamepad.buttonNorth,
                queueEventOnly: true);

            yield return null;

            Assert.That(
                _weaponController.ActiveDefinition,
                Is.SameAs(_secondWeapon));
        }

        [UnityTest]
        public IEnumerator Update_WhenWeaponControlIsDisabled_DoesNotSwitch()
        {
            _acquisitionController.TryAcquire(
                _firstWeapon);

            _acquisitionController.TryAcquire(
                _secondWeapon);

            _weaponController.SetWeaponControlEnabled(
                false);

            Press(
                _keyboard.qKey,
                queueEventOnly: true);

            yield return null;

            Assert.That(
                _weaponController.ActiveDefinition,
                Is.SameAs(_firstWeapon));
        }

        [Test]
        public void TrySwitchNext_PreservesLoadoutControllerEntryInvariant()
        {
            _acquisitionController.TryAcquire(
                _firstWeapon);

            _acquisitionController.TryAcquire(
                _secondWeapon);

            _weaponSwitcher.TrySwitchNext();

            Assert.That(
                _weaponController.ActiveEntry,
                Is.SameAs(
                    _acquisitionController
                        .Loadout
                        .ActiveEntry));

            _weaponSwitcher.TrySwitchNext();

            Assert.That(
                _weaponController.ActiveEntry,
                Is.SameAs(
                    _acquisitionController
                        .Loadout
                        .ActiveEntry));
        }

        [Test]
        public void TrySwitchNext_WhenControllerEntryDoesNotMatchLoadout_Throws()
        {
            _acquisitionController.TryAcquire(
                _firstWeapon);

            _acquisitionController.TryAcquire(
                _secondWeapon);

            /*
             * Intentionally create a different runtime entry
             * with the same definition to break the runtime
             * identity invariant.
             */
            _weaponController.Initialize(
                _firstWeapon);

            Assert.That(
                () =>
                    _weaponSwitcher.TrySwitchNext(),
                Throws.InvalidOperationException);
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
