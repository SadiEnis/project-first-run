using NUnit.Framework;
using ProjectFirstRun.Abilities;
using ProjectFirstRun.Abilities.Fireball;
using ProjectFirstRun.Enemies;
using ProjectFirstRun.Stats;
using UnityEngine;
using Object = UnityEngine.Object;

namespace ProjectFirstRun.Tests.EditMode.Abilities.Fireball
{
    public sealed class FireballAbilityRuntimeFactoryTests
    {
        private GameObject _registryObject;
        private GameObject _damageSource;

        private EnemyRegistry _registry;
        private FireballDefinition _definition;
        private PlayerStatCollection _stats;

        [SetUp]
        public void SetUp()
        {
            _registryObject =
                new GameObject(
                    "FireballFactory_Registry");

            _registry =
                _registryObject
                    .AddComponent<EnemyRegistry>();

            _damageSource =
                new GameObject(
                    "FireballFactory_DamageSource");

            _definition =
                ScriptableObject
                    .CreateInstance<FireballDefinition>();

            _stats =
                new PlayerStatCollection();
        }

        [TearDown]
        public void TearDown()
        {
            if (_registryObject != null)
            {
                Object.DestroyImmediate(
                    _registryObject);
            }

            if (_damageSource != null)
            {
                Object.DestroyImmediate(
                    _damageSource);
            }

            if (_definition != null)
            {
                Object.DestroyImmediate(
                    _definition);
            }
        }

        [Test]
        public void Constructor_WithNullRegistry_Throws()
        {
            Assert.That(
                () =>
                    new FireballAbilityRuntimeFactory(
                        null,
                        _damageSource,
                        _stats),
                Throws.ArgumentNullException);
        }

        [Test]
        public void Constructor_WithNullDamageSource_Throws()
        {
            Assert.That(
                () =>
                    new FireballAbilityRuntimeFactory(
                        _registry,
                        null,
                        _stats),
                Throws.ArgumentNullException);
        }

        [Test]
        public void Constructor_WithNullStats_Throws()
        {
            Assert.That(
                () =>
                    new FireballAbilityRuntimeFactory(
                        _registry,
                        _damageSource,
                        null),
                Throws.ArgumentNullException);
        }

        [Test]
        public void Create_WithValidDefinition_CreatesReadyEntry()
        {
            FireballAbilityRuntimeFactory factory =
                CreateFactory();

            AbilityRuntimeEntry entry =
                factory.Create(
                    _definition);

            Assert.That(
                entry,
                Is.Not.Null);

            Assert.That(
                entry.Definition,
                Is.SameAs(_definition));

            Assert.That(
                entry.IsReady,
                Is.True);
        }

        [Test]
        public void Create_WithNullDefinition_Throws()
        {
            FireballAbilityRuntimeFactory factory =
                CreateFactory();

            Assert.That(
                () => factory.Create(null),
                Throws.ArgumentNullException);
        }

        [Test]
        public void CreatedEntry_WithNoEnemies_ReturnsNoTarget()
        {
            FireballAbilityRuntimeFactory factory =
                CreateFactory();

            AbilityRuntimeEntry entry =
                factory.Create(
                    _definition);

            AbilityAutoCastResult result =
                entry.TryAutoCast(
                    Vector3.zero);

            Assert.That(
                result,
                Is.EqualTo(
                    AbilityAutoCastResult.NoTarget));

            Assert.That(
                entry.IsReady,
                Is.True);
        }

        [Test]
        public void Create_AfterRegistryDestroyed_Throws()
        {
            FireballAbilityRuntimeFactory factory =
                CreateFactory();

            Object.DestroyImmediate(
                _registryObject);

            Assert.That(
                () => factory.Create(_definition),
                Throws.InvalidOperationException);
        }

        [Test]
        public void Create_AfterDamageSourceDestroyed_Throws()
        {
            FireballAbilityRuntimeFactory factory =
                CreateFactory();

            Object.DestroyImmediate(
                _damageSource);

            Assert.That(
                () => factory.Create(_definition),
                Throws.InvalidOperationException);
        }
        
        [Test]
        public void Supports_WithFireballDefinition_ReturnsTrue()
        {
            FireballAbilityRuntimeFactory factory =
                CreateFactory();

            bool result =
                factory.Supports(
                    _definition);

            Assert.That(
                result,
                Is.True);
        }

        [Test]
        public void Supports_WithNullDefinition_ReturnsFalse()
        {
            FireballAbilityRuntimeFactory factory =
                CreateFactory();

            bool result =
                factory.Supports(
                    null);

            Assert.That(
                result,
                Is.False);
        }

        [Test]
        public void Supports_WithDifferentAbilityDefinition_ReturnsFalse()
        {
            TestAbilityDefinition otherDefinition =
                ScriptableObject
                    .CreateInstance<TestAbilityDefinition>();

            try
            {
                FireballAbilityRuntimeFactory factory =
                    CreateFactory();

                bool result =
                    factory.Supports(
                        otherDefinition);

                Assert.That(
                    result,
                    Is.False);
            }
            finally
            {
                Object.DestroyImmediate(
                    otherDefinition);
            }
        }

        [Test]
        public void GenericCreate_WithFireballDefinition_CreatesReadyEntry()
        {
            IAbilityRuntimeFactory factory =
                CreateFactory();

            AbilityRuntimeEntry entry =
                factory.Create(
                    _definition);

            Assert.That(
                entry,
                Is.Not.Null);

            Assert.That(
                entry.Definition,
                Is.SameAs(_definition));

            Assert.That(
                entry.IsReady,
                Is.True);
        }

        [Test]
        public void GenericCreate_WithUnsupportedDefinition_Throws()
        {
            TestAbilityDefinition otherDefinition =
                ScriptableObject
                    .CreateInstance<TestAbilityDefinition>();

            try
            {
                IAbilityRuntimeFactory factory =
                    CreateFactory();

                Assert.That(
                    () =>
                        factory.Create(
                            otherDefinition),
                    Throws.ArgumentException);
            }
            finally
            {
                Object.DestroyImmediate(
                    otherDefinition);
            }
        }

        private FireballAbilityRuntimeFactory CreateFactory()
        {
            return new FireballAbilityRuntimeFactory(
                _registry,
                _damageSource,
                _stats);
        }
        
        private sealed class TestAbilityDefinition :
            AbilityDefinition
        {
        }
    }
}