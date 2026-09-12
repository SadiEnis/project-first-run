using System.Collections.Generic;
using System.Reflection;
using NUnit.Framework;
using ProjectFirstRun.Items;
using ProjectFirstRun.Weapons;
using UnityEngine;
using Object = UnityEngine.Object;

namespace ProjectFirstRun.Tests.EditMode.Weapons
{
    public sealed class PlayerWeaponLoadoutRuntimeEntryTests
    {
        private readonly List<WeaponDefinition>
            _definitions =
                new List<WeaponDefinition>();

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
        }

        [Test]
        public void Add_CreatesRuntimeEntry()
        {
            WeaponDefinition weapon =
                CreateWeapon(
                    "weapon.a");

            PlayerWeaponLoadout loadout =
                new PlayerWeaponLoadout();

            PlayerWeaponRuntimeEntry entry =
                loadout.Add(
                    weapon);

            Assert.That(
                loadout.Entries.Count,
                Is.EqualTo(1));

            Assert.That(
                loadout.Entries[0],
                Is.SameAs(entry));

            Assert.That(
                entry.Definition,
                Is.SameAs(weapon));

            Assert.That(
                entry.RuntimeState,
                Is.Not.Null);
        }

        [Test]
        public void Add_RuntimeEntriesPreserveAcquisitionOrder()
        {
            WeaponDefinition first =
                CreateWeapon(
                    "weapon.first");

            WeaponDefinition second =
                CreateWeapon(
                    "weapon.second");

            PlayerWeaponLoadout loadout =
                new PlayerWeaponLoadout();

            PlayerWeaponRuntimeEntry firstEntry =
                loadout.Add(
                    first);

            PlayerWeaponRuntimeEntry secondEntry =
                loadout.Add(
                    second);

            Assert.That(
                loadout.Entries[0],
                Is.SameAs(firstEntry));

            Assert.That(
                loadout.Entries[1],
                Is.SameAs(secondEntry));
        }

        [Test]
        public void SetActive_WithDefinition_SelectsStoredRuntimeEntry()
        {
            WeaponDefinition weapon =
                CreateWeapon(
                    "weapon.a");

            PlayerWeaponLoadout loadout =
                new PlayerWeaponLoadout();

            PlayerWeaponRuntimeEntry entry =
                loadout.Add(
                    weapon);

            loadout.SetActive(
                weapon);

            Assert.That(
                loadout.ActiveEntry,
                Is.SameAs(entry));

            Assert.That(
                loadout.ActiveDefinition,
                Is.SameAs(weapon));
        }

        [Test]
        public void SetActive_WithStoredEntry_SelectsEntry()
        {
            WeaponDefinition weapon =
                CreateWeapon(
                    "weapon.a");

            PlayerWeaponLoadout loadout =
                new PlayerWeaponLoadout();

            PlayerWeaponRuntimeEntry entry =
                loadout.Add(
                    weapon);

            loadout.SetActive(
                entry);

            Assert.That(
                loadout.ActiveEntry,
                Is.SameAs(entry));
        }

        [Test]
        public void SetActive_WithForeignEntry_Throws()
        {
            WeaponDefinition storedWeapon =
                CreateWeapon(
                    "weapon.stored");

            WeaponDefinition foreignWeapon =
                CreateWeapon(
                    "weapon.foreign");

            PlayerWeaponLoadout loadout =
                new PlayerWeaponLoadout();

            loadout.Add(
                storedWeapon);

            PlayerWeaponRuntimeEntry foreignEntry =
                new PlayerWeaponRuntimeEntry(
                    foreignWeapon);

            Assert.That(
                () =>
                    loadout.SetActive(
                        foreignEntry),
                Throws.InvalidOperationException);
        }

        [Test]
        public void GetEntry_WithStoredDefinition_ReturnsExactEntry()
        {
            WeaponDefinition weapon =
                CreateWeapon(
                    "weapon.a");

            PlayerWeaponLoadout loadout =
                new PlayerWeaponLoadout();

            PlayerWeaponRuntimeEntry expected =
                loadout.Add(
                    weapon);

            PlayerWeaponRuntimeEntry result =
                loadout.GetEntry(
                    weapon);

            Assert.That(
                result,
                Is.SameAs(expected));
        }

        [Test]
        public void GetEntry_WithDifferentInstanceHavingSameStableId_ReturnsNull()
        {
            WeaponDefinition stored =
                CreateWeapon(
                    "weapon.same");

            WeaponDefinition equivalent =
                CreateWeapon(
                    "weapon.same");

            PlayerWeaponLoadout loadout =
                new PlayerWeaponLoadout();

            loadout.Add(
                stored);

            Assert.That(
                loadout.Contains(
                    equivalent),
                Is.True);

            Assert.That(
                loadout.GetEntry(
                    equivalent),
                Is.Null);
        }

        [Test]
        public void GetNextEntry_WithEmptyLoadout_ReturnsNull()
        {
            PlayerWeaponLoadout loadout =
                new PlayerWeaponLoadout();

            Assert.That(
                loadout.GetNextEntry(),
                Is.Null);
        }

        [Test]
        public void GetNextEntry_WithNoActiveWeapon_ReturnsFirstEntry()
        {
            WeaponDefinition first =
                CreateWeapon(
                    "weapon.first");

            WeaponDefinition second =
                CreateWeapon(
                    "weapon.second");

            PlayerWeaponLoadout loadout =
                new PlayerWeaponLoadout();

            PlayerWeaponRuntimeEntry firstEntry =
                loadout.Add(
                    first);

            loadout.Add(
                second);

            Assert.That(
                loadout.GetNextEntry(),
                Is.SameAs(firstEntry));
        }

        [Test]
        public void GetNextEntry_WithSingleWeapon_ReturnsSameEntry()
        {
            WeaponDefinition weapon =
                CreateWeapon(
                    "weapon.single");

            PlayerWeaponLoadout loadout =
                new PlayerWeaponLoadout();

            PlayerWeaponRuntimeEntry entry =
                loadout.Add(
                    weapon);

            loadout.SetActive(
                entry);

            Assert.That(
                loadout.GetNextEntry(),
                Is.SameAs(entry));
        }

        [Test]
        public void GetNextEntry_WithTwoWeapons_ReturnsNextEntry()
        {
            WeaponDefinition first =
                CreateWeapon(
                    "weapon.first");

            WeaponDefinition second =
                CreateWeapon(
                    "weapon.second");

            PlayerWeaponLoadout loadout =
                new PlayerWeaponLoadout();

            PlayerWeaponRuntimeEntry firstEntry =
                loadout.Add(
                    first);

            PlayerWeaponRuntimeEntry secondEntry =
                loadout.Add(
                    second);

            loadout.SetActive(
                firstEntry);

            Assert.That(
                loadout.GetNextEntry(),
                Is.SameAs(secondEntry));
        }

        [Test]
        public void GetNextEntry_FromLastWeapon_WrapsToFirst()
        {
            WeaponDefinition first =
                CreateWeapon(
                    "weapon.first");

            WeaponDefinition second =
                CreateWeapon(
                    "weapon.second");

            PlayerWeaponLoadout loadout =
                new PlayerWeaponLoadout();

            PlayerWeaponRuntimeEntry firstEntry =
                loadout.Add(
                    first);

            PlayerWeaponRuntimeEntry secondEntry =
                loadout.Add(
                    second);

            loadout.SetActive(
                secondEntry);

            Assert.That(
                loadout.GetNextEntry(),
                Is.SameAs(firstEntry));
        }

        [Test]
        public void SwitchingActiveEntry_DoesNotReplaceRuntimeState()
        {
            WeaponDefinition first =
                CreateWeapon(
                    "weapon.first");

            WeaponDefinition second =
                CreateWeapon(
                    "weapon.second");

            PlayerWeaponLoadout loadout =
                new PlayerWeaponLoadout();

            PlayerWeaponRuntimeEntry firstEntry =
                loadout.Add(
                    first);

            PlayerWeaponRuntimeEntry secondEntry =
                loadout.Add(
                    second);

            WeaponRuntimeState originalState =
                firstEntry.RuntimeState;

            loadout.SetActive(
                firstEntry);

            loadout.SetActive(
                secondEntry);

            loadout.SetActive(
                firstEntry);

            Assert.That(
                loadout.ActiveEntry.RuntimeState,
                Is.SameAs(originalState));
        }

        [Test]
        public void Entries_CannotBeMutatedExternally()
        {
            WeaponDefinition weapon =
                CreateWeapon(
                    "weapon.a");

            PlayerWeaponLoadout loadout =
                new PlayerWeaponLoadout();

            loadout.Add(
                weapon);

            Assert.That(
                loadout.Entries,
                Is.Not.InstanceOf<
                    List<PlayerWeaponRuntimeEntry>>());
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