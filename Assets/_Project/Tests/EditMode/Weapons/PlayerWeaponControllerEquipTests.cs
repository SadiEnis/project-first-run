using System.Collections.Generic;
using System.Reflection;
using NUnit.Framework;
using ProjectFirstRun.Items;
using ProjectFirstRun.Weapons;
using UnityEngine;
using Object = UnityEngine.Object;

namespace ProjectFirstRun.Tests.EditMode.Weapons
{
    public sealed class PlayerWeaponControllerEquipTests
    {
        private GameObject _playerObject;

        private PlayerWeaponController
            _weaponController;

        private readonly List<WeaponDefinition>
            _definitions =
                new List<WeaponDefinition>();

        [SetUp]
        public void SetUp()
        {
            _playerObject =
                new GameObject(
                    "PlayerWeaponControllerEquip_Test");

            /*
             * Keep inactive so PlayerWeaponController Awake
             * does not require scene camera/muzzle setup.
             */
            _playerObject.SetActive(
                false);

            _weaponController =
                _playerObject.AddComponent<
                    PlayerWeaponController>();
        }

        [TearDown]
        public void TearDown()
        {
            foreach (WeaponDefinition definition
                     in _definitions)
            {
                if (definition != null)
                {
                    Object.DestroyImmediate(
                        definition);
                }
            }

            _definitions.Clear();

            if (_playerObject != null)
            {
                Object.DestroyImmediate(
                    _playerObject);
            }
        }

        [Test]
        public void Equip_WithRuntimeEntry_InitializesController()
        {
            PlayerWeaponRuntimeEntry entry =
                CreateEntry(
                    "weapon.a");

            _weaponController.Equip(
                entry);

            Assert.That(
                _weaponController.IsInitialized,
                Is.True);

            Assert.That(
                _weaponController.ActiveEntry,
                Is.SameAs(entry));

            Assert.That(
                _weaponController.ActiveDefinition,
                Is.SameAs(entry.Definition));
        }

        [Test]
        public void Equip_UsesExactRuntimeStateInstance()
        {
            PlayerWeaponRuntimeEntry entry =
                CreateEntry(
                    "weapon.a");

            WeaponRuntimeState expectedState =
                entry.RuntimeState;

            _weaponController.Equip(
                entry);

            Assert.That(
                _weaponController.ActiveEntry.RuntimeState,
                Is.SameAs(expectedState));
        }

        [Test]
        public void Equip_SecondEntry_ReplacesActiveEntry()
        {
            PlayerWeaponRuntimeEntry first =
                CreateEntry(
                    "weapon.first");

            PlayerWeaponRuntimeEntry second =
                CreateEntry(
                    "weapon.second");

            _weaponController.Equip(
                first);

            _weaponController.Equip(
                second);

            Assert.That(
                _weaponController.ActiveEntry,
                Is.SameAs(second));

            Assert.That(
                _weaponController.ActiveDefinition,
                Is.SameAs(second.Definition));
        }

        [Test]
        public void Equip_PublishesEquippedWeaponAmmo()
        {
            PlayerWeaponRuntimeEntry entry =
                CreateEntry(
                    "weapon.ammo_event");

            int? publishedMagazineAmmo = null;
            int? publishedReserveAmmo = null;

            _weaponController.AmmoChanged +=
                (magazineAmmo, reserveAmmo) =>
                {
                    publishedMagazineAmmo =
                        magazineAmmo;

                    publishedReserveAmmo =
                        reserveAmmo;
                };

            _weaponController.Equip(
                entry);

            Assert.That(
                publishedMagazineAmmo,
                Is.EqualTo(
                    entry.RuntimeState.MagazineAmmo));

            Assert.That(
                publishedReserveAmmo,
                Is.EqualTo(
                    entry.RuntimeState.ReserveAmmo));
        }

        [Test]
        public void Equip_FirstSecondFirst_RestoresExactFirstRuntimeState()
        {
            PlayerWeaponRuntimeEntry first =
                CreateEntry(
                    "weapon.first");

            PlayerWeaponRuntimeEntry second =
                CreateEntry(
                    "weapon.second");

            WeaponRuntimeState originalFirstState =
                first.RuntimeState;

            _weaponController.Equip(
                first);

            _weaponController.Equip(
                second);

            _weaponController.Equip(
                first);

            Assert.That(
                _weaponController.ActiveEntry,
                Is.SameAs(first));

            Assert.That(
                _weaponController.ActiveEntry.RuntimeState,
                Is.SameAs(originalFirstState));
        }

        [Test]
        public void Equip_WithNullEntry_Throws()
        {
            Assert.That(
                () =>
                    _weaponController.Equip(
                        null),
                Throws.ArgumentNullException);
        }

        [Test]
        public void Initialize_CreatesAndEquipsRuntimeEntry()
        {
            WeaponDefinition definition =
                CreateWeapon(
                    "weapon.legacy_initialize");

            _weaponController.Initialize(
                definition);

            Assert.That(
                _weaponController.IsInitialized,
                Is.True);

            Assert.That(
                _weaponController.ActiveEntry,
                Is.Not.Null);

            Assert.That(
                _weaponController.ActiveEntry.Definition,
                Is.SameAs(definition));

            Assert.That(
                _weaponController.ActiveDefinition,
                Is.SameAs(definition));
        }

        [Test]
        public void Initialize_EachCallCreatesNewRuntimeEntry()
        {
            WeaponDefinition definition =
                CreateWeapon(
                    "weapon.initialize_reset");

            _weaponController.Initialize(
                definition);

            PlayerWeaponRuntimeEntry firstEntry =
                _weaponController.ActiveEntry;

            _weaponController.Initialize(
                definition);

            PlayerWeaponRuntimeEntry secondEntry =
                _weaponController.ActiveEntry;

            Assert.That(
                secondEntry,
                Is.Not.SameAs(firstEntry));

            Assert.That(
                secondEntry.RuntimeState,
                Is.Not.SameAs(firstEntry.RuntimeState));
        }

        private PlayerWeaponRuntimeEntry CreateEntry(
            string stableId)
        {
            return new PlayerWeaponRuntimeEntry(
                CreateWeapon(
                    stableId));
        }

        private WeaponDefinition CreateWeapon(
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

            _definitions.Add(
                definition);

            return definition;
        }

        private static void SetPrivateField(
            object target,
            string fieldName,
            object value,
            System.Type declaringType)
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
