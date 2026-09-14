using System;
using System.Collections.Generic;
using System.Reflection;
using NUnit.Framework;
using ProjectFirstRun.Abilities.Fireball;
using ProjectFirstRun.Builds;
using ProjectFirstRun.Items;
using ProjectFirstRun.Upgrades;
using ProjectFirstRun.Weapons;
using UnityEngine;
using Object = UnityEngine.Object;

namespace ProjectFirstRun.Tests.EditMode.Builds
{
    public sealed class PlayerBuildItemLevelTests
    {
        private static PlayerBuild CreateBuild() => new PlayerBuild(PlayerBuildCapacity.CreateDefault());

        [TestCase(ItemCategory.Weapon)]
        [TestCase(ItemCategory.Ability)]
        [TestCase(ItemCategory.Upgrade)]
        public void Acquisition_StartsAtOneAndLevelsUpWithoutAnotherSlot(ItemCategory category)
        {
            var build = CreateBuild();
            Assert.That(build.TryAdd(category, "sample", 3), Is.EqualTo(PlayerBuildAddResult.Added));
            Assert.That(build.TryGetItem(category, "sample", out var item), Is.True);
            Assert.That(item.Category, Is.EqualTo(category));
            Assert.That(item.StableId, Is.EqualTo("sample"));
            Assert.That(item.Level, Is.EqualTo(1));
            Assert.That(item.MaximumLevel, Is.EqualTo(3));
            Assert.That(item.IsAtMaximumLevel, Is.False);
            Assert.That(build.TryLevelUp(category, "sample"), Is.EqualTo(ItemLevelUpResult.LevelIncreased));
            Assert.That(item.Level, Is.EqualTo(2), "An existing read-only entry observes its run's progress.");
            Assert.That(build.TryLevelUp(category, "sample"), Is.EqualTo(ItemLevelUpResult.LevelIncreased));
            Assert.That(item.Level, Is.EqualTo(3));
            Assert.That(item.IsAtMaximumLevel, Is.True);
            Assert.That(build.TryLevelUp(category, "sample"), Is.EqualTo(ItemLevelUpResult.MaximumLevelReached));
            Assert.That(build.GetLevel(category, "sample"), Is.EqualTo(3));
            Assert.That(build.GetCount(category), Is.EqualTo(1));
        }

        [TestCase(ItemCategory.Weapon)]
        [TestCase(ItemCategory.Ability)]
        [TestCase(ItemCategory.Upgrade)]
        public void LegacyAdd_HasOneLevelAndCannotBeLeveled(ItemCategory category)
        {
            var build = CreateBuild();
            build.TryAdd(category, "legacy");
            Assert.That(build.GetLevel(category, "legacy"), Is.EqualTo(1));
            Assert.That(build.TryLevelUp(category, "legacy"), Is.EqualTo(ItemLevelUpResult.MaximumLevelReached));
        }

        [TestCase(ItemCategory.Weapon)]
        [TestCase(ItemCategory.Ability)]
        [TestCase(ItemCategory.Upgrade)]
        public void MissingItem_DoesNotCreateOwnershipOrLevel(ItemCategory category)
        {
            var build = CreateBuild();
            Assert.That(build.TryGetItem(category, "missing", out var item), Is.False);
            Assert.That(item, Is.Null);
            Assert.That(build.GetLevel(category, "missing"), Is.Zero);
            Assert.That(build.TryLevelUp(category, "missing"), Is.EqualTo(ItemLevelUpResult.NotOwned));
            Assert.That(build.GetCount(category), Is.Zero);
        }

        [TestCase(ItemCategory.Weapon)]
        [TestCase(ItemCategory.Ability)]
        [TestCase(ItemCategory.Upgrade)]
        public void FullCategory_CanLevelOwnedItemButCannotAddAnother(ItemCategory category)
        {
            var build = CreateBuild();
            for (int i = 0; i < build.GetCapacity(category); i++) build.TryAdd(category, "item" + i, 8);
            Assert.That(build.IsFull(category), Is.True);
            Assert.That(build.TryAdd(category, "extra", 8), Is.EqualTo(PlayerBuildAddResult.CapacityReached));
            Assert.That(build.TryGetItem(category, "extra", out _), Is.False);
            Assert.That(build.TryLevelUp(category, "item0"), Is.EqualTo(ItemLevelUpResult.LevelIncreased));
            Assert.That(build.GetCount(category), Is.EqualTo(build.GetCapacity(category)));
        }

        [Test]
        public void DuplicateAcquisition_DoesNotResetLevelOrReplaceMaximum()
        {
            var build = CreateBuild();
            build.TryAdd(ItemCategory.Weapon, "sample", 3);
            build.TryLevelUp(ItemCategory.Weapon, "sample");
            Assert.That(build.TryAdd(ItemCategory.Weapon, "sample", 8), Is.EqualTo(PlayerBuildAddResult.AlreadyOwned));
            build.TryGetItem(ItemCategory.Weapon, "sample", out var item);
            Assert.That(item.Level, Is.EqualTo(2));
            Assert.That(item.MaximumLevel, Is.EqualTo(3));
        }

        [Test]
        public void IndependentBuildsAndCategories_DoNotShareProgress()
        {
            var first = CreateBuild();
            var second = CreateBuild();
            first.TryAdd(ItemCategory.Weapon, "shared", 8);
            first.TryAdd(ItemCategory.Ability, "shared", 8);
            second.TryAdd(ItemCategory.Weapon, "shared", 8);
            first.TryLevelUp(ItemCategory.Weapon, "shared");
            Assert.That(first.GetLevel(ItemCategory.Weapon, "shared"), Is.EqualTo(2));
            Assert.That(first.GetLevel(ItemCategory.Ability, "shared"), Is.EqualTo(1));
            Assert.That(second.GetLevel(ItemCategory.Weapon, "shared"), Is.EqualTo(1));
            Assert.That(CreateBuild().GetLevel(ItemCategory.Weapon, "shared"), Is.Zero);
        }

        [Test]
        public void IDsRemainOrdinalAndOwnershipListsRemainReadOnly()
        {
            var build = CreateBuild();
            var liveList = build.GetItems(ItemCategory.Ability);
            build.TryAdd(ItemCategory.Ability, "Sample", 8);
            build.TryAdd(ItemCategory.Ability, "sample", 3);
            build.TryLevelUp(ItemCategory.Ability, "Sample");
            Assert.That(build.GetLevel(ItemCategory.Ability, "sample"), Is.EqualTo(1));
            Assert.That(liveList, Is.EqualTo(new[] { "Sample", "sample" }));
            Assert.Throws<NotSupportedException>(() => ((IList<string>)liveList).Clear());
            Assert.That(typeof(PlayerBuildItem).GetConstructors(), Is.Empty);
            foreach (PropertyInfo property in typeof(PlayerBuildItem).GetProperties())
                Assert.That(property.GetSetMethod(), Is.Null, property.Name + " must not expose a public setter.");
        }

        [TestCase(0)]
        [TestCase(-1)]
        public void InvalidMaximum_FailsBeforeMutation(int maximum)
        {
            var build = CreateBuild();
            Assert.Throws<ArgumentOutOfRangeException>(() => build.TryAdd(ItemCategory.Weapon, "sample", maximum));
            Assert.That(build.Weapons, Is.Empty);
            Assert.That(build.GetLevel(ItemCategory.Weapon, "sample"), Is.Zero);
        }

        [TestCase(null)]
        [TestCase("")]
        [TestCase(" ")]
        [TestCase(" sample")]
        public void InvalidIdentity_IsRejectedByEveryLevelOperation(string stableId)
        {
            var build = CreateBuild();
            Assert.Throws<ArgumentException>(() => build.TryAdd(ItemCategory.Weapon, stableId, 8));
            Assert.Throws<ArgumentException>(() => build.GetLevel(ItemCategory.Weapon, stableId));
            Assert.Throws<ArgumentException>(() => build.TryGetItem(ItemCategory.Weapon, stableId, out _));
            Assert.Throws<ArgumentException>(() => build.TryLevelUp(ItemCategory.Weapon, stableId));
            Assert.That(build.Weapons, Is.Empty);
        }

        [TestCase(-1)]
        [TestCase(3)]
        public void InvalidCategory_IsRejectedByEveryLevelOperation(int value)
        {
            var build = CreateBuild();
            var category = (ItemCategory)value;
            Assert.Throws<ArgumentOutOfRangeException>(() => build.TryAdd(category, "sample", 8));
            Assert.Throws<ArgumentOutOfRangeException>(() => build.GetLevel(category, "sample"));
            Assert.Throws<ArgumentOutOfRangeException>(() => build.TryGetItem(category, "sample", out _));
            Assert.Throws<ArgumentOutOfRangeException>(() => build.TryLevelUp(category, "sample"));
        }

        [Test]
        public void LargeMaximum_DoesNotOverflowWhenStored()
        {
            var build = CreateBuild();
            build.TryAdd(ItemCategory.Weapon, "sample", int.MaxValue);
            Assert.That(build.TryLevelUp(ItemCategory.Weapon, "sample"), Is.EqualTo(ItemLevelUpResult.LevelIncreased));
            build.TryGetItem(ItemCategory.Weapon, "sample", out var item);
            Assert.That(item.MaximumLevel, Is.EqualTo(int.MaxValue));
            Assert.That(item.Level, Is.EqualTo(2));
        }

        [TestCase(ItemCategory.Weapon)]
        [TestCase(ItemCategory.Ability)]
        [TestCase(ItemCategory.Upgrade)]
        public void Controller_SnapshotsDefinitionMaximumWithoutChangingAsset(ItemCategory category)
        {
            var owner = new GameObject("Item level test");
            ItemDefinition definition = CreateDefinition(category);
            try
            {
                var controller = owner.AddComponent<PlayerBuildController>();
                controller.Initialize(PlayerBuildCapacity.CreateDefault());
                Assert.That(definition.MaximumLevel, Is.EqualTo(1), "Existing content stays single-level until effects are configured.");
                SetField(definition, "_maximumLevel", 8);
                Assert.That(controller.TryAdd(definition), Is.EqualTo(PlayerBuildAddResult.Added));
                SetField(definition, "_maximumLevel", 2);
                Assert.That(controller.Build.TryLevelUp(category, definition.StableId), Is.EqualTo(ItemLevelUpResult.LevelIncreased));
                controller.Build.TryGetItem(category, definition.StableId, out var item);
                Assert.That(item.MaximumLevel, Is.EqualTo(8));
                Assert.That(definition.MaximumLevel, Is.EqualTo(2));
                Assert.That(controller.TryAdd(definition), Is.EqualTo(PlayerBuildAddResult.AlreadyOwned));
                Assert.That(item.Level, Is.EqualTo(2));
            }
            finally { Object.DestroyImmediate(owner); Object.DestroyImmediate(definition); }
        }

        [TestCase(0)]
        [TestCase(-5)]
        public void InvalidDefinitionMaximum_ThrowsWithoutClaimingOwnership(int maximum)
        {
            var owner = new GameObject("Invalid item level test");
            var definition = CreateDefinition(ItemCategory.Weapon);
            try
            {
                var controller = owner.AddComponent<PlayerBuildController>();
                controller.Initialize(PlayerBuildCapacity.CreateDefault());
                SetField(definition, "_maximumLevel", maximum);
                Assert.Throws<InvalidOperationException>(() => definition.ValidateLevelConfiguration());
                Assert.Throws<InvalidOperationException>(() => controller.TryAdd(definition));
                Assert.That(controller.Build.Weapons, Is.Empty);
            }
            finally { Object.DestroyImmediate(owner); Object.DestroyImmediate(definition); }
        }

        private static ItemDefinition CreateDefinition(ItemCategory category)
        {
            ItemDefinition definition = category switch
            {
                ItemCategory.Weapon => ScriptableObject.CreateInstance<WeaponDefinition>(),
                ItemCategory.Ability => ScriptableObject.CreateInstance<FireballDefinition>(),
                _ => ScriptableObject.CreateInstance<UpgradeDefinition>()
            };
            SetField(definition, "_stableId", "item.level.test");
            return definition;
        }

        private static void SetField(ItemDefinition definition, string name, object value) =>
            typeof(ItemDefinition).GetField(name, BindingFlags.NonPublic | BindingFlags.Instance).SetValue(definition, value);
    }
}
