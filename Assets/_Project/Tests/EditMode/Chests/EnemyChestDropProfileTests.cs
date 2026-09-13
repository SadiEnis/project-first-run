using System;
using System.Collections.Generic;
using System.Reflection;
using NUnit.Framework;
using ProjectFirstRun.Chests;
using ProjectFirstRun.Chests.Spawning;
using ProjectFirstRun.Rewards;
using UnityEngine;
using Object = UnityEngine.Object;

namespace ProjectFirstRun.Tests.EditMode.Chests
{
    public sealed class EnemyChestDropProfileTests
    {
        private EnemyChestDropProfile _profile;
        private ChestDefinition _first;
        private ChestDefinition _second;
        private RewardItemPool _pool;
        private GameObject _prefab;

        [SetUp]
        public void SetUp()
        {
            _profile = ScriptableObject.CreateInstance<EnemyChestDropProfile>();
            _pool = ScriptableObject.CreateInstance<RewardItemPool>();
            _prefab = new GameObject("Chest template", typeof(ChestController));
            _first = Definition("first");
            _second = Definition("second");
            SetField(_profile, "_chanceBasisPoints", 2500);
            SetField(_profile, "_entries", new[] { new WeightedChestEntry(_first, 2), new WeightedChestEntry(_second, 3) });
        }

        [TearDown]
        public void TearDown()
        {
            Object.DestroyImmediate(_profile);
            Object.DestroyImmediate(_first);
            Object.DestroyImmediate(_second);
            Object.DestroyImmediate(_pool);
            Object.DestroyImmediate(_prefab);
        }

        [Test]
        public void ZeroChance_NeedsNoEntriesOrRandomCalls()
        {
            SetField(_profile, "_chanceBasisPoints", 0);
            SetField(_profile, "_entries", null);
            var random = new SequenceRandom();
            Assert.That(_profile.TryRoll(random, out var definition), Is.False);
            Assert.That(definition, Is.Null);
            Assert.That(random.Maximums, Is.Empty);
        }

        [TestCase(0, true)]
        [TestCase(2499, true)]
        [TestCase(2500, false)]
        [TestCase(9999, false)]
        public void Chance_UsesExclusiveUpperBoundary(int roll, bool succeeds)
        {
            var random = new SequenceRandom(roll, 0);
            Assert.That(_profile.TryRoll(random, out var selected), Is.EqualTo(succeeds));
            Assert.That(selected, Is.EqualTo(succeeds ? _first : null));
            Assert.That(random.Maximums, Is.EqualTo(succeeds ? new[] { 10000, 5 } : new[] { 10000 }));
        }

        [TestCase(0, false)]
        [TestCase(1, false)]
        [TestCase(2, true)]
        [TestCase(4, true)]
        public void GuaranteedChance_SkipsChanceRollAndSelectsWeightedInterval(int roll, bool second)
        {
            SetField(_profile, "_chanceBasisPoints", 10000);
            var random = new SequenceRandom(roll);
            Assert.That(_profile.TryRoll(random, out var selected), Is.True);
            Assert.That(selected, Is.EqualTo(second ? _second : _first));
            Assert.That(random.Maximums, Is.EqualTo(new[] { 5 }));
        }

        [TestCase(-1)]
        [TestCase(10001)]
        public void InvalidChance_IsRejected(int chance)
        {
            SetField(_profile, "_chanceBasisPoints", chance);
            Assert.Throws<InvalidOperationException>(() => _profile.Validate());
        }

        [TestCase("empty")]
        [TestCase("null_entry")]
        [TestCase("null_definition")]
        [TestCase("zero_weight")]
        [TestCase("negative_weight")]
        [TestCase("overflow")]
        public void InvalidEnabledTable_IsRejected(string reason)
        {
            WeightedChestEntry[] entries = reason switch
            {
                "empty" => Array.Empty<WeightedChestEntry>(),
                "null_entry" => new WeightedChestEntry[] { null },
                "null_definition" => new[] { new WeightedChestEntry(null, 1) },
                "zero_weight" => new[] { new WeightedChestEntry(_first, 0) },
                "negative_weight" => new[] { new WeightedChestEntry(_first, -1) },
                _ => new[] { new WeightedChestEntry(_first, int.MaxValue), new WeightedChestEntry(_second, 1) }
            };
            SetField(_profile, "_entries", entries);
            Assert.Throws<InvalidOperationException>(() => _profile.Validate());
        }

        [Test]
        public void LargestSupportedTotal_SelectsLastValueWithoutOverflow()
        {
            SetField(_profile, "_chanceBasisPoints", 10000);
            SetField(_profile, "_entries", new[] { new WeightedChestEntry(_first, int.MaxValue) });
            Assert.That(_profile.TryRoll(new SequenceRandom(int.MaxValue - 1), out var definition), Is.True);
            Assert.That(definition, Is.SameAs(_first));
        }

        [TestCase(-1)]
        [TestCase(10000)]
        public void InvalidRandomOutput_IsRejected(int roll) =>
            Assert.Throws<InvalidOperationException>(() => _profile.TryRoll(new SequenceRandom(roll), out _));

        [Test]
        public void NullRandom_IsRejected() =>
            Assert.Throws<ArgumentNullException>(() => _profile.TryRoll(null, out _));

        private ChestDefinition Definition(string id)
        {
            var definition = ScriptableObject.CreateInstance<ChestDefinition>();
            SetField(definition, "_stableId", id);
            SetField(definition, "_rewardItemPool", _pool);
            SetField(definition, "_worldPrefab", _prefab);
            return definition;
        }

        private sealed class SequenceRandom : IRandomSource
        {
            private readonly Queue<int> _values;
            public readonly List<int> Maximums = new List<int>();
            public SequenceRandom(params int[] values) => _values = new Queue<int>(values);
            public int Next(int minInclusive, int maxExclusive)
            {
                Assert.That(minInclusive, Is.Zero);
                Maximums.Add(maxExclusive);
                return _values.Dequeue();
            }
        }

        private static void SetField(object target, string name, object value) =>
            target.GetType().GetField(name, BindingFlags.Instance | BindingFlags.NonPublic).SetValue(target, value);
    }
}
