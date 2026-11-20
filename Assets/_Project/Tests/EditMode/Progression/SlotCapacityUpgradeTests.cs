using System;
using NUnit.Framework;
using ProjectFirstRun.Progression;
using UnityEngine;
namespace ProjectFirstRun.Tests.EditMode.Progression
{
    public sealed class SlotCapacityUpgradeTests
    {
        private SlotCapacityUpgradeDefinition _definition;
        [SetUp] public void Setup()
        {
            _definition = ScriptableObject.CreateInstance<SlotCapacityUpgradeDefinition>();
            JsonUtility.FromJsonOverwrite("{\"_stableId\":\"meta.test-slots\",\"_displayName\":\"Slots\",\"_startingCapacity\":2,\"_costs\":[75,150,250]}", _definition);
        }
        [TearDown] public void Cleanup() => UnityEngine.Object.DestroyImmediate(_definition);
        [Test] public void EveryPurchaseAddsOneSlotAndMaximumCannotBePurchasedAgain()
        {
            _definition.Validate(5);
            for (int level = 0; level <= 3; level++)
                Assert.That(_definition.GetCapacity(level), Is.EqualTo(2 + level));
            Assert.That(_definition.GetNextCost(1), Is.EqualTo(150));
            Assert.Throws<ArgumentOutOfRangeException>(() => _definition.GetNextCost(3));
            Assert.Throws<ArgumentOutOfRangeException>(() => _definition.GetCapacity(4));
            Assert.Throws<ArgumentOutOfRangeException>(() => _definition.GetCapacity(-1));
        }
        [Test] public void InvalidContentCannotExceedBuildLimitOrOfferFreePurchases()
        {
            Assert.Throws<InvalidOperationException>(() => _definition.Validate(4));
            JsonUtility.FromJsonOverwrite("{\"_costs\":[0]}", _definition);
            Assert.Throws<InvalidOperationException>(() => _definition.Validate(5));
        }
    }
}
