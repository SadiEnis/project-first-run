using System;
using NUnit.Framework;
using ProjectFirstRun.Abilities;
using UnityEngine;
using Object = UnityEngine.Object;

namespace ProjectFirstRun.Tests.EditMode.Abilities
{
    public sealed class AbilityRuntimeFactoryRegistryTests
    {
        private TestAbilityDefinition _definition;

        [SetUp]
        public void SetUp()
        {
            _definition =
                ScriptableObject
                    .CreateInstance<TestAbilityDefinition>();
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
        public void NewRegistry_HasNoFactories()
        {
            AbilityRuntimeFactoryRegistry registry =
                new AbilityRuntimeFactoryRegistry();

            Assert.That(
                registry.FactoryCount,
                Is.EqualTo(0));
        }

        [Test]
        public void Register_WithFactory_AddsFactory()
        {
            AbilityRuntimeFactoryRegistry registry =
                new AbilityRuntimeFactoryRegistry();

            TestAbilityRuntimeFactory factory =
                new TestAbilityRuntimeFactory(
                    _definition,
                    true);

            registry.Register(
                factory);

            Assert.That(
                registry.FactoryCount,
                Is.EqualTo(1));
        }

        [Test]
        public void Register_WithNullFactory_Throws()
        {
            AbilityRuntimeFactoryRegistry registry =
                new AbilityRuntimeFactoryRegistry();

            Assert.That(
                () => registry.Register(null),
                Throws.ArgumentNullException);
        }

        [Test]
        public void Register_SameFactoryInstanceTwice_Throws()
        {
            AbilityRuntimeFactoryRegistry registry =
                new AbilityRuntimeFactoryRegistry();

            TestAbilityRuntimeFactory factory =
                new TestAbilityRuntimeFactory(
                    _definition,
                    true);

            registry.Register(
                factory);

            Assert.That(
                () => registry.Register(factory),
                Throws.InvalidOperationException);

            Assert.That(
                registry.FactoryCount,
                Is.EqualTo(1));
        }

        [Test]
        public void Resolve_WithMatchingFactory_ReturnsFactory()
        {
            AbilityRuntimeFactoryRegistry registry =
                new AbilityRuntimeFactoryRegistry();

            TestAbilityRuntimeFactory unsupportedFactory =
                new TestAbilityRuntimeFactory(
                    _definition,
                    false);

            TestAbilityRuntimeFactory supportedFactory =
                new TestAbilityRuntimeFactory(
                    _definition,
                    true);

            registry.Register(
                unsupportedFactory);

            registry.Register(
                supportedFactory);

            IAbilityRuntimeFactory result =
                registry.Resolve(
                    _definition);

            Assert.That(
                result,
                Is.SameAs(supportedFactory));
        }

        [Test]
        public void Resolve_WhenNoFactorySupportsDefinition_Throws()
        {
            AbilityRuntimeFactoryRegistry registry =
                new AbilityRuntimeFactoryRegistry();

            TestAbilityRuntimeFactory factory =
                new TestAbilityRuntimeFactory(
                    _definition,
                    false);

            registry.Register(
                factory);

            Assert.That(
                () =>
                    registry.Resolve(
                        _definition),
                Throws.InvalidOperationException);
        }

        [Test]
        public void Resolve_WhenNoFactoriesRegistered_Throws()
        {
            AbilityRuntimeFactoryRegistry registry =
                new AbilityRuntimeFactoryRegistry();

            Assert.That(
                () =>
                    registry.Resolve(
                        _definition),
                Throws.InvalidOperationException);
        }

        [Test]
        public void Resolve_WhenMultipleFactoriesSupportDefinition_Throws()
        {
            AbilityRuntimeFactoryRegistry registry =
                new AbilityRuntimeFactoryRegistry();

            TestAbilityRuntimeFactory firstFactory =
                new TestAbilityRuntimeFactory(
                    _definition,
                    true);

            TestAbilityRuntimeFactory secondFactory =
                new TestAbilityRuntimeFactory(
                    _definition,
                    true);

            registry.Register(
                firstFactory);

            registry.Register(
                secondFactory);

            Assert.That(
                () =>
                    registry.Resolve(
                        _definition),
                Throws.InvalidOperationException);
        }

        [Test]
        public void Resolve_WithNullDefinition_Throws()
        {
            AbilityRuntimeFactoryRegistry registry =
                new AbilityRuntimeFactoryRegistry();

            Assert.That(
                () => registry.Resolve(null),
                Throws.ArgumentNullException);
        }

        private sealed class TestAbilityRuntimeFactory :
            IAbilityRuntimeFactory
        {
            private readonly AbilityDefinition
                _supportedDefinition;

            private readonly bool _supports;

            public TestAbilityRuntimeFactory(
                AbilityDefinition supportedDefinition,
                bool supports)
            {
                _supportedDefinition =
                    supportedDefinition;

                _supports =
                    supports;
            }

            public bool Supports(
                AbilityDefinition definition)
            {
                return _supports &&
                       ReferenceEquals(
                           definition,
                           _supportedDefinition);
            }

            public AbilityRuntimeEntry Create(
                AbilityDefinition definition)
            {
                throw new NotSupportedException(
                    "Runtime creation is not required " +
                    "by registry resolution tests.");
            }
        }

        private sealed class TestAbilityDefinition :
            AbilityDefinition
        {
        }
    }
}