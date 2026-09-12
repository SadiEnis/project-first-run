using System;
using System.Reflection;
using NUnit.Framework;
using ProjectFirstRun.Abilities;
using ProjectFirstRun.Abilities.Execution;
using ProjectFirstRun.Builds;
using ProjectFirstRun.Items;
using UnityEngine;
using Object = UnityEngine.Object;

namespace ProjectFirstRun.Tests.PlayMode.Abilities
{
    public sealed class PlayerAbilityAcquisitionControllerTests
    {
        private GameObject _playerObject;

        private PlayerBuildController _buildController;
        private PlayerAbilityController _abilityController;
        private PlayerAbilityAcquisitionController
            _acquisitionController;

        private TestAbilityDefinition _definition;

        [SetUp]
        public void SetUp()
        {
            _playerObject =
                new GameObject(
                    "AbilityAcquisition_Player");

            _buildController =
                _playerObject
                    .AddComponent<PlayerBuildController>();

            _abilityController =
                _playerObject
                    .AddComponent<PlayerAbilityController>();

            _acquisitionController =
                _playerObject
                    .AddComponent<
                        PlayerAbilityAcquisitionController>();

            _buildController.Initialize(
                PlayerBuildCapacity.CreateDefault());

            _definition =
                ScriptableObject
                    .CreateInstance<TestAbilityDefinition>();

            SetStableId(
                _definition,
                "ability.test");
        }

        [TearDown]
        public void TearDown()
        {
            if (_playerObject != null)
            {
                Object.DestroyImmediate(
                    _playerObject);
            }

            if (_definition != null)
            {
                Object.DestroyImmediate(
                    _definition);
            }
        }

        [Test]
        public void NewController_IsNotInitialized()
        {
            Assert.That(
                _acquisitionController.IsInitialized,
                Is.False);
        }

        [Test]
        public void Initialize_WithRegistry_InitializesController()
        {
            AbilityRuntimeFactoryRegistry registry =
                new AbilityRuntimeFactoryRegistry();

            _acquisitionController.Initialize(
                registry);

            Assert.That(
                _acquisitionController.IsInitialized,
                Is.True);
        }

        [Test]
        public void Initialize_WithNullRegistry_Throws()
        {
            Assert.That(
                () =>
                    _acquisitionController.Initialize(
                        null),
                Throws.ArgumentNullException);

            Assert.That(
                _acquisitionController.IsInitialized,
                Is.False);
        }

        [Test]
        public void Initialize_WhenAlreadyInitialized_Throws()
        {
            AbilityRuntimeFactoryRegistry registry =
                new AbilityRuntimeFactoryRegistry();

            _acquisitionController.Initialize(
                registry);

            Assert.That(
                () =>
                    _acquisitionController.Initialize(
                        new AbilityRuntimeFactoryRegistry()),
                Throws.InvalidOperationException);
        }

        [Test]
        public void TryAcquire_BeforeInitialize_Throws()
        {
            Assert.That(
                () =>
                    _acquisitionController.TryAcquire(
                        _definition),
                Throws.InvalidOperationException);
        }

        [Test]
        public void TryAcquire_WithNullDefinition_Throws()
        {
            InitializeWithFactory(
                _definition);

            Assert.That(
                () =>
                    _acquisitionController.TryAcquire(
                        null),
                Throws.ArgumentNullException);
        }

        [Test]
        public void TryAcquire_WithValidDefinition_ReturnsAcquired()
        {
            TestAbilityRuntimeFactory factory =
                InitializeWithFactory(
                    _definition);

            AbilityAcquireResult result =
                _acquisitionController.TryAcquire(
                    _definition);

            Assert.That(
                result,
                Is.EqualTo(
                    AbilityAcquireResult.Acquired));

            Assert.That(
                factory.CreateCallCount,
                Is.EqualTo(1));
        }

        [Test]
        public void TryAcquire_WithValidDefinition_AddsBuildOwnership()
        {
            InitializeWithFactory(
                _definition);

            _acquisitionController.TryAcquire(
                _definition);

            Assert.That(
                _buildController.Contains(
                    _definition),
                Is.True);

            Assert.That(
                _buildController.Build.GetCount(
                    ItemCategory.Ability),
                Is.EqualTo(1));
        }

        [Test]
        public void TryAcquire_WithValidDefinition_AddsRuntimeEntry()
        {
            InitializeWithFactory(
                _definition);

            _acquisitionController.TryAcquire(
                _definition);

            Assert.That(
                _abilityController.AbilityCount,
                Is.EqualTo(1));

            Assert.That(
                _abilityController
                    .Entries[0]
                    .Definition,
                Is.SameAs(_definition));
        }

        [Test]
        public void TryAcquire_WhenAlreadyOwned_ReturnsAlreadyOwned()
        {
            TestAbilityRuntimeFactory factory =
                InitializeWithFactory(
                    _definition);

            AbilityAcquireResult firstResult =
                _acquisitionController.TryAcquire(
                    _definition);

            AbilityAcquireResult secondResult =
                _acquisitionController.TryAcquire(
                    _definition);

            Assert.That(
                firstResult,
                Is.EqualTo(
                    AbilityAcquireResult.Acquired));

            Assert.That(
                secondResult,
                Is.EqualTo(
                    AbilityAcquireResult.AlreadyOwned));

            Assert.That(
                _abilityController.AbilityCount,
                Is.EqualTo(1));

            Assert.That(
                factory.CreateCallCount,
                Is.EqualTo(1));
        }

        [Test]
        public void TryAcquire_WhenCapacityReached_DoesNotCreateRuntime()
        {
            FillAbilityCapacity();

            TestAbilityRuntimeFactory factory =
                InitializeWithFactory(
                    _definition);

            AbilityAcquireResult result =
                _acquisitionController.TryAcquire(
                    _definition);

            Assert.That(
                result,
                Is.EqualTo(
                    AbilityAcquireResult.CapacityReached));

            Assert.That(
                factory.CreateCallCount,
                Is.EqualTo(0));

            Assert.That(
                _abilityController.AbilityCount,
                Is.EqualTo(0));

            Assert.That(
                _buildController.Contains(
                    _definition),
                Is.False);
        }

        [Test]
        public void TryAcquire_WhenFactoryIsMissing_ThrowsWithoutChangingBuild()
        {
            AbilityRuntimeFactoryRegistry registry =
                new AbilityRuntimeFactoryRegistry();

            _acquisitionController.Initialize(
                registry);

            Assert.That(
                () =>
                    _acquisitionController.TryAcquire(
                        _definition),
                Throws.InvalidOperationException);

            Assert.That(
                _buildController.Contains(
                    _definition),
                Is.False);

            Assert.That(
                _abilityController.AbilityCount,
                Is.EqualTo(0));
        }

        [Test]
        public void TryAcquire_WhenFactoryReturnsNull_ThrowsWithoutChangingBuild()
        {
            TestAbilityRuntimeFactory factory =
                InitializeWithFactory(
                    _definition);

            factory.ReturnNullEntry = true;

            Assert.That(
                () =>
                    _acquisitionController.TryAcquire(
                        _definition),
                Throws.InvalidOperationException);

            Assert.That(
                _buildController.Contains(
                    _definition),
                Is.False);

            Assert.That(
                _abilityController.AbilityCount,
                Is.EqualTo(0));
        }

        [Test]
        public void TryAcquire_PublishesAbilityAcquiredOnce()
        {
            InitializeWithFactory(
                _definition);

            int eventCount = 0;
            AbilityDefinition receivedDefinition = null;

            _acquisitionController.AbilityAcquired +=
                definition =>
                {
                    eventCount++;
                    receivedDefinition =
                        definition;
                };

            _acquisitionController.TryAcquire(
                _definition);

            _acquisitionController.TryAcquire(
                _definition);

            Assert.That(
                eventCount,
                Is.EqualTo(1));

            Assert.That(
                receivedDefinition,
                Is.SameAs(_definition));
        }
        
        private static void SetStableId(
            ItemDefinition definition,
            string stableId)
        {
            FieldInfo field =
                typeof(ItemDefinition)
                    .GetField(
                        "_stableId",
                        BindingFlags.Instance |
                        BindingFlags.NonPublic);

            Assert.That(
                field,
                Is.Not.Null);

            field.SetValue(
                definition,
                stableId);
        }

        private TestAbilityRuntimeFactory
            InitializeWithFactory(
                AbilityDefinition supportedDefinition)
        {
            TestAbilityRuntimeFactory factory =
                new TestAbilityRuntimeFactory(
                    supportedDefinition);

            AbilityRuntimeFactoryRegistry registry =
                new AbilityRuntimeFactoryRegistry();

            registry.Register(
                factory);

            _acquisitionController.Initialize(
                registry);

            return factory;
        }

        private void FillAbilityCapacity()
        {
            int capacity =
                _buildController
                    .Build
                    .GetCapacity(
                        ItemCategory.Ability);

            for (int index = 0;
                 index < capacity;
                 index++)
            {
                PlayerBuildAddResult result =
                    _buildController.Build.TryAdd(
                        ItemCategory.Ability,
                        $"ability.owned_{index}");

                Assert.That(
                    result,
                    Is.EqualTo(
                        PlayerBuildAddResult.Added));
            }
        }

        private sealed class TestAbilityRuntimeFactory :
            IAbilityRuntimeFactory
        {
            private readonly AbilityDefinition
                _supportedDefinition;

            public int CreateCallCount
            {
                get;
                private set;
            }

            public bool ReturnNullEntry
            {
                get;
                set;
            }

            public TestAbilityRuntimeFactory(
                AbilityDefinition supportedDefinition)
            {
                _supportedDefinition =
                    supportedDefinition;
            }

            public bool Supports(
                AbilityDefinition definition)
            {
                return ReferenceEquals(
                    definition,
                    _supportedDefinition);
            }

            public AbilityRuntimeEntry Create(
                AbilityDefinition definition)
            {
                CreateCallCount++;

                if (ReturnNullEntry)
                {
                    return null;
                }

                return new AbilityRuntimeEntry(
                    definition,
                    null,
                    new TestAbilityExecutor());
            }
        }

        private sealed class TestAbilityExecutor :
            IAbilityExecutor
        {
            public AbilityExecutionResult TryExecute(
                in AbilityExecutionContext context)
            {
                return AbilityExecutionResult.Performed;
            }
        }

        private sealed class TestAbilityDefinition :
            AbilityDefinition
        {
        }
    }
}