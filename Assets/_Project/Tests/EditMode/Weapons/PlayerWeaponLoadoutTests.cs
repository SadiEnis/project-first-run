using System.Reflection;
using NUnit.Framework;
using ProjectFirstRun.Items;
using ProjectFirstRun.Weapons;
using UnityEngine;
using Object = UnityEngine.Object;

namespace ProjectFirstRun.Tests.EditMode.Weapons
{
    public sealed class PlayerWeaponRuntimeEntryTests
    {
        private WeaponDefinition _definition;

        [SetUp]
        public void SetUp()
        {
            _definition =
                CreateWeapon(
                    "weapon.runtime_entry");
        }

        [TearDown]
        public void TearDown()
        {
            if (_definition != null)
            {
                Object.DestroyImmediate(
                    _definition);
            }
        }

        [Test]
        public void Constructor_WithDefinition_PreservesDefinition()
        {
            PlayerWeaponRuntimeEntry entry =
                new PlayerWeaponRuntimeEntry(
                    _definition);

            Assert.That(
                entry.Definition,
                Is.SameAs(_definition));
        }

        [Test]
        public void Constructor_WithDefinition_CreatesRuntimeState()
        {
            PlayerWeaponRuntimeEntry entry =
                new PlayerWeaponRuntimeEntry(
                    _definition);

            Assert.That(
                entry.RuntimeState,
                Is.Not.Null);
        }

        [Test]
        public void Constructor_WithNullDefinition_Throws()
        {
            Assert.That(
                () =>
                    new PlayerWeaponRuntimeEntry(
                        null),
                Throws.ArgumentNullException);
        }

        [Test]
        public void Constructor_WithInvalidRuntimeConfig_Throws()
        {
            SetPrivateField(
                _definition,
                "_magazineCapacity",
                0,
                typeof(WeaponDefinition));

            Assert.That(
                () =>
                    new PlayerWeaponRuntimeEntry(
                        _definition),
                Throws.TypeOf<
                    System.ArgumentOutOfRangeException>());
        }

        private static WeaponDefinition CreateWeapon(
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