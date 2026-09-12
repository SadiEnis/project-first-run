using System.Reflection;
using NUnit.Framework;
using ProjectFirstRun.Abilities;
using ProjectFirstRun.Abilities.Fireball;
using ProjectFirstRun.Items;
using UnityEngine;
using Object = UnityEngine.Object;

namespace ProjectFirstRun.Tests.EditMode.Abilities.Fireball
{
    public sealed class FireballDefinitionTests
    {
        private FireballDefinition _definition;

        [SetUp]
        public void SetUp()
        {
            _definition =
                ScriptableObject
                    .CreateInstance<FireballDefinition>();
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
        public void ConfiguredValues_AreExposed()
        {
            SetPrivateField(
                "_damage",
                35f);

            SetPrivateField(
                "_projectileSpeed",
                12f);

            SetPrivateField(
                "_projectileLifetime",
                4f);

            Assert.That(
                _definition.Damage,
                Is.EqualTo(35f));

            Assert.That(
                _definition.ProjectileSpeed,
                Is.EqualTo(12f));

            Assert.That(
                _definition.ProjectileLifetime,
                Is.EqualTo(4f));
        }

        [Test]
        public void CreateRuntimeConfig_StillUsesGenericAbilityCooldown()
        {
            SetPrivateField(
                "_cooldown",
                3.5f,
                typeof(AbilityDefinition));

            AbilityRuntimeConfig config =
                _definition.CreateRuntimeConfig();

            Assert.That(
                config.Cooldown,
                Is.EqualTo(3.5f));
        }

        private void SetPrivateField(
            string fieldName,
            object value,
            System.Type declaringType = null)
        {
            System.Type targetType =
                declaringType ??
                typeof(FireballDefinition);

            FieldInfo field =
                targetType.GetField(
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