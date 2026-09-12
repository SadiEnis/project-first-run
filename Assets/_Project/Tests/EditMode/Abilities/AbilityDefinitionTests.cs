using System.Reflection;
using NUnit.Framework;
using ProjectFirstRun.Abilities;
using ProjectFirstRun.Items;
using UnityEngine;
using Object = UnityEngine.Object;

namespace ProjectFirstRun.Tests.EditMode.Abilities
{
    public sealed class AbilityDefinitionTests
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
        public void Category_IsAbility()
        {
            Assert.That(
                _definition.Category,
                Is.EqualTo(
                    ItemCategory.Ability));
        }

        [Test]
        public void CreateRuntimeConfig_UsesConfiguredCooldown()
        {
            SetCooldown(4.5f);

            AbilityRuntimeConfig config =
                _definition.CreateRuntimeConfig();

            Assert.That(
                config.Cooldown,
                Is.EqualTo(4.5f));
        }

        [Test]
        public void Cooldown_ReturnsConfiguredValue()
        {
            SetCooldown(2.25f);

            Assert.That(
                _definition.Cooldown,
                Is.EqualTo(2.25f));
        }

        private void SetCooldown(
            float cooldown)
        {
            FieldInfo field =
                typeof(AbilityDefinition)
                    .GetField(
                        "_cooldown",
                        BindingFlags.Instance |
                        BindingFlags.NonPublic);

            Assert.That(
                field,
                Is.Not.Null);

            field.SetValue(
                _definition,
                cooldown);
        }

        private sealed class TestAbilityDefinition :
            AbilityDefinition
        {
        }
    }
}