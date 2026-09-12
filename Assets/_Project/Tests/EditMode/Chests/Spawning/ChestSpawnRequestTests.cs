using System;
using System.Reflection;
using NUnit.Framework;
using ProjectFirstRun.Chests;
using ProjectFirstRun.Chests.Spawning;
using ProjectFirstRun.Rewards;
using UnityEngine;
using Object = UnityEngine.Object;

namespace ProjectFirstRun.Tests.EditMode.Chests.Spawning
{
    public sealed class ChestSpawnRequestTests
    {
        private ChestDefinition _definition;
        private RewardItemPool _rewardItemPool;
        private GameObject _worldPrefab;

        [SetUp]
        public void SetUp()
        {
            _rewardItemPool =
                ScriptableObject.CreateInstance<
                    RewardItemPool>();

            _worldPrefab =
                new GameObject(
                    "ChestSpawnRequest_Prefab");

            _worldPrefab.AddComponent<
                ChestController>();

            _definition =
                ScriptableObject.CreateInstance<
                    ChestDefinition>();

            SetDefinitionField(
                "_stableId",
                "chest.spawn-request-test");

            SetDefinitionField(
                "_rewardItemPool",
                _rewardItemPool);

            SetDefinitionField(
                "_requestedChoiceCount",
                3);

            SetDefinitionField(
                "_worldPrefab",
                _worldPrefab);
        }

        [TearDown]
        public void TearDown()
        {
            if (_definition != null)
            {
                Object.DestroyImmediate(
                    _definition);
            }

            if (_rewardItemPool != null)
            {
                Object.DestroyImmediate(
                    _rewardItemPool);
            }

            if (_worldPrefab != null)
            {
                Object.DestroyImmediate(
                    _worldPrefab);
            }
        }

        [Test]
        public void Constructor_WithValidValues_PreservesValues()
        {
            Vector3 position =
                new Vector3(
                    2f,
                    1f,
                    -3f);

            Quaternion rotation =
                Quaternion.Euler(
                    0f,
                    90f,
                    0f);

            ChestSpawnRequest request =
                new ChestSpawnRequest(
                    _definition,
                    position,
                    rotation);

            Assert.That(
                request.Definition,
                Is.SameAs(_definition));

            Assert.That(
                request.Position,
                Is.EqualTo(position));

            Assert.That(
                Quaternion.Angle(
                    request.Rotation,
                    rotation),
                Is.LessThan(0.01f));
        }

        [Test]
        public void Constructor_WithNullDefinition_Throws()
        {
            Assert.That(
                () =>
                {
                    _ = new ChestSpawnRequest(
                        null,
                        Vector3.zero,
                        Quaternion.identity);
                },
                Throws.ArgumentNullException);
        }

        [Test]
        public void Constructor_WithNonFinitePosition_Throws()
        {
            Assert.That(
                () =>
                {
                    _ = new ChestSpawnRequest(
                        _definition,
                        new Vector3(
                            float.NaN,
                            0f,
                            0f),
                        Quaternion.identity);
                },
                Throws.TypeOf<
                    ArgumentOutOfRangeException>());
        }

        [Test]
        public void Constructor_WithNonFiniteRotation_Throws()
        {
            Assert.That(
                () =>
                {
                    _ = new ChestSpawnRequest(
                        _definition,
                        Vector3.zero,
                        new Quaternion(
                            0f,
                            float.PositiveInfinity,
                            0f,
                            1f));
                },
                Throws.TypeOf<
                    ArgumentOutOfRangeException>());
        }

        [Test]
        public void Constructor_WithZeroRotation_Throws()
        {
            Assert.That(
                () =>
                {
                    _ = new ChestSpawnRequest(
                        _definition,
                        Vector3.zero,
                        new Quaternion(
                            0f,
                            0f,
                            0f,
                            0f));
                },
                Throws.TypeOf<
                    ArgumentOutOfRangeException>());
        }

        [Test]
        public void Validate_OnDefaultRequest_Throws()
        {
            ChestSpawnRequest request =
                default;

            Assert.That(
                request.Validate,
                Throws.ArgumentNullException);
        }

        [Test]
        public void Validate_AfterWorldPrefabDestroyed_Throws()
        {
            ChestSpawnRequest request =
                new ChestSpawnRequest(
                    _definition,
                    Vector3.zero,
                    Quaternion.identity);

            Object.DestroyImmediate(
                _worldPrefab);

            Assert.That(
                request.Validate,
                Throws.InvalidOperationException);
        }

        private void SetDefinitionField(
            string fieldName,
            object value)
        {
            FieldInfo field =
                typeof(ChestDefinition)
                    .GetField(
                        fieldName,
                        BindingFlags.Instance |
                        BindingFlags.NonPublic);

            Assert.That(
                field,
                Is.Not.Null);

            field.SetValue(
                _definition,
                value);
        }
    }
}
