using System.Collections;
using System.Reflection;
using NUnit.Framework;
using ProjectFirstRun.Abilities;
using ProjectFirstRun.Abilities.Execution;
using ProjectFirstRun.Abilities.Targeting;
using ProjectFirstRun.Items;
using UnityEngine;
using UnityEngine.TestTools;
using Object = UnityEngine.Object;

namespace ProjectFirstRun.Tests.PlayMode.Abilities
{
    public sealed class PlayerAbilityControllerTests
    {
        private GameObject _playerObject;

        private PlayerAbilityController _controller;

        private TestPlayerAbilityDefinition _firstDefinition;
        private TestPlayerAbilityDefinition _secondDefinition;

        [SetUp]
        public void SetUp()
        {
            _playerObject =
                new GameObject(
                    "PlayerAbilityController_Test");

            _playerObject.SetActive(false);

            _controller =
                _playerObject
                    .AddComponent<PlayerAbilityController>();

            _firstDefinition =
                CreateDefinition(
                    "ability.test_first",
                    1f);

            _secondDefinition =
                CreateDefinition(
                    "ability.test_second",
                    2f);
        }

        [TearDown]
        public void TearDown()
        {
            if (_playerObject != null)
            {
                Object.DestroyImmediate(
                    _playerObject);
            }

            if (_firstDefinition != null)
            {
                Object.DestroyImmediate(
                    _firstDefinition);
            }

            if (_secondDefinition != null)
            {
                Object.DestroyImmediate(
                    _secondDefinition);
            }
        }

        [Test]
        public void NewController_HasNoAbilities()
        {
            Assert.That(
                _controller.AbilityCount,
                Is.EqualTo(0));

            Assert.That(
                _controller.Entries.Count,
                Is.EqualTo(0));

            Assert.That(
                _controller.IsAbilityControlEnabled,
                Is.True);
        }

        [Test]
        public void AddAbility_WithValidEntry_AddsEntry()
        {
            FakeAbilityExecutor executor =
                new FakeAbilityExecutor();

            AbilityRuntimeEntry entry =
                CreateTargetlessEntry(
                    _firstDefinition,
                    executor);

            _controller.AddAbility(
                entry);

            Assert.That(
                _controller.AbilityCount,
                Is.EqualTo(1));

            Assert.That(
                _controller.Contains(entry),
                Is.True);

            Assert.That(
                _controller.Entries[0],
                Is.SameAs(entry));
        }

        [Test]
        public void AddAbility_WithNullEntry_Throws()
        {
            Assert.That(
                () => _controller.AddAbility(null),
                Throws.ArgumentNullException);
        }

        [Test]
        public void AddAbility_WithSameEntryTwice_Throws()
        {
            FakeAbilityExecutor executor =
                new FakeAbilityExecutor();

            AbilityRuntimeEntry entry =
                CreateTargetlessEntry(
                    _firstDefinition,
                    executor);

            _controller.AddAbility(
                entry);

            Assert.That(
                () => _controller.AddAbility(entry),
                Throws.InvalidOperationException);

            Assert.That(
                _controller.AbilityCount,
                Is.EqualTo(1));
        }

        [Test]
        public void Contains_WithNullEntry_ReturnsFalse()
        {
            Assert.That(
                _controller.Contains(null),
                Is.False);
        }

        [Test]
        public void Entries_CannotBeMutatedExternally()
        {
            FakeAbilityExecutor executor =
                new FakeAbilityExecutor();

            AbilityRuntimeEntry entry =
                CreateTargetlessEntry(
                    _firstDefinition,
                    executor);

            _controller.AddAbility(
                entry);

            Assert.That(
                _controller.Entries,
                Is.Not.InstanceOf<
                    System.Collections.Generic.List<
                        AbilityRuntimeEntry>>());

            System.Collections.Generic.ICollection<
                AbilityRuntimeEntry> collection =
                _controller.Entries as
                    System.Collections.Generic.ICollection<
                        AbilityRuntimeEntry>;

            Assert.That(
                collection,
                Is.Not.Null);

            Assert.That(
                collection.IsReadOnly,
                Is.True);

            Assert.That(
                () =>
                    collection.Add(
                        CreateTargetlessEntry(
                            _secondDefinition,
                            new FakeAbilityExecutor())),
                Throws.TypeOf<System.NotSupportedException>());

            Assert.That(
                _controller.AbilityCount,
                Is.EqualTo(1));
        }

        [Test]
        public void Tick_WithInvalidDeltaTime_Throws()
        {
            Assert.That(
                () => _controller.Tick(-0.1f),
                Throws.TypeOf<
                    System.ArgumentOutOfRangeException>());

            Assert.That(
                () => _controller.Tick(float.NaN),
                Throws.TypeOf<
                    System.ArgumentOutOfRangeException>());

            Assert.That(
                () =>
                    _controller.Tick(
                        float.PositiveInfinity),
                Throws.TypeOf<
                    System.ArgumentOutOfRangeException>());
        }

        [Test]
        public void Tick_WhenEnabled_AutoCastsReadyAbility()
        {
            FakeAbilityExecutor executor =
                new FakeAbilityExecutor();

            AbilityRuntimeEntry entry =
                CreateTargetlessEntry(
                    _firstDefinition,
                    executor);

            _controller.AddAbility(
                entry);

            _controller.Tick(
                0f);

            Assert.That(
                executor.ExecutionCount,
                Is.EqualTo(1));

            Assert.That(
                entry.IsReady,
                Is.False);
        }

        [Test]
        public void Tick_DoesNotCastAbilityAgainWhileOnCooldown()
        {
            FakeAbilityExecutor executor =
                new FakeAbilityExecutor();

            AbilityRuntimeEntry entry =
                CreateTargetlessEntry(
                    _firstDefinition,
                    executor);

            _controller.AddAbility(
                entry);

            _controller.Tick(
                0f);

            _controller.Tick(
                0.25f);

            _controller.Tick(
                0.25f);

            Assert.That(
                executor.ExecutionCount,
                Is.EqualTo(1));

            Assert.That(
                entry.State.CooldownRemaining,
                Is.EqualTo(0.5f)
                    .Within(0.0001f));
        }

        [Test]
        public void Tick_WhenCooldownCompletes_CastsAgain()
        {
            FakeAbilityExecutor executor =
                new FakeAbilityExecutor();

            AbilityRuntimeEntry entry =
                CreateTargetlessEntry(
                    _firstDefinition,
                    executor);

            _controller.AddAbility(
                entry);

            _controller.Tick(
                0f);

            _controller.Tick(
                1f);

            Assert.That(
                executor.ExecutionCount,
                Is.EqualTo(2));

            Assert.That(
                entry.State.CooldownRemaining,
                Is.EqualTo(1f));
        }

        [Test]
        public void Tick_WithMultipleReadyAbilities_CastsAll()
        {
            FakeAbilityExecutor firstExecutor =
                new FakeAbilityExecutor();

            FakeAbilityExecutor secondExecutor =
                new FakeAbilityExecutor();

            _controller.AddAbility(
                CreateTargetlessEntry(
                    _firstDefinition,
                    firstExecutor));

            _controller.AddAbility(
                CreateTargetlessEntry(
                    _secondDefinition,
                    secondExecutor));

            _controller.Tick(
                0f);

            Assert.That(
                firstExecutor.ExecutionCount,
                Is.EqualTo(1));

            Assert.That(
                secondExecutor.ExecutionCount,
                Is.EqualTo(1));

            Assert.That(
                _controller.AbilityCount,
                Is.EqualTo(2));
        }

        [Test]
        public void Tick_WhenControlDisabled_DoesNotCast()
        {
            FakeAbilityExecutor executor =
                new FakeAbilityExecutor();

            AbilityRuntimeEntry entry =
                CreateTargetlessEntry(
                    _firstDefinition,
                    executor);

            _controller.AddAbility(
                entry);

            _controller.SetAbilityControlEnabled(
                false);

            _controller.Tick(
                0f);

            Assert.That(
                executor.ExecutionCount,
                Is.EqualTo(0));

            Assert.That(
                entry.IsReady,
                Is.True);
        }

        [Test]
        public void Tick_WhenControlDisabled_StillAdvancesCooldown()
        {
            FakeAbilityExecutor executor =
                new FakeAbilityExecutor();

            AbilityRuntimeEntry entry =
                CreateTargetlessEntry(
                    _firstDefinition,
                    executor);

            _controller.AddAbility(
                entry);

            _controller.Tick(
                0f);

            Assert.That(
                entry.State.CooldownRemaining,
                Is.EqualTo(1f));

            _controller.SetAbilityControlEnabled(
                false);

            _controller.Tick(
                0.5f);

            Assert.That(
                executor.ExecutionCount,
                Is.EqualTo(1));

            Assert.That(
                entry.State.CooldownRemaining,
                Is.EqualTo(0.5f)
                    .Within(0.0001f));
        }

        [Test]
        public void ReEnablingControl_AllowsReadyAbilityToCast()
        {
            FakeAbilityExecutor executor =
                new FakeAbilityExecutor();

            AbilityRuntimeEntry entry =
                CreateTargetlessEntry(
                    _firstDefinition,
                    executor);

            _controller.AddAbility(
                entry);

            _controller.SetAbilityControlEnabled(
                false);

            _controller.Tick(
                1f);

            Assert.That(
                executor.ExecutionCount,
                Is.EqualTo(0));

            _controller.SetAbilityControlEnabled(
                true);

            _controller.Tick(
                0f);

            Assert.That(
                executor.ExecutionCount,
                Is.EqualTo(1));
        }

        [Test]
        public void Tick_WhenNoTarget_DoesNotConsumeCooldown()
        {
            FakeTargetSelector selector =
                new FakeTargetSelector();

            FakeAbilityExecutor executor =
                new FakeAbilityExecutor();

            AbilityRuntimeEntry entry =
                new AbilityRuntimeEntry(
                    _firstDefinition,
                    selector,
                    executor);

            _controller.AddAbility(
                entry);

            _controller.Tick(
                0f);

            Assert.That(
                selector.SelectionCount,
                Is.EqualTo(1));

            Assert.That(
                executor.ExecutionCount,
                Is.EqualTo(0));

            Assert.That(
                entry.IsReady,
                Is.True);
        }

        [UnityTest]
        public IEnumerator Update_AutomaticallyCastsReadyAbility()
        {
            FakeAbilityExecutor executor =
                new FakeAbilityExecutor();

            AbilityRuntimeEntry entry =
                CreateTargetlessEntry(
                    _firstDefinition,
                    executor);

            _controller.AddAbility(
                entry);

            _playerObject.SetActive(true);

            yield return null;

            Assert.That(
                executor.ExecutionCount,
                Is.EqualTo(1));

            Assert.That(
                entry.IsReady,
                Is.False);
        }

        [UnityTest]
        public IEnumerator Update_WhenControlDisabled_DoesNotAutoCast()
        {
            FakeAbilityExecutor executor =
                new FakeAbilityExecutor();

            AbilityRuntimeEntry entry =
                CreateTargetlessEntry(
                    _firstDefinition,
                    executor);

            _controller.AddAbility(
                entry);

            _controller.SetAbilityControlEnabled(
                false);

            _playerObject.SetActive(true);

            yield return null;

            Assert.That(
                executor.ExecutionCount,
                Is.EqualTo(0));

            Assert.That(
                entry.IsReady,
                Is.True);
        }

        private static AbilityRuntimeEntry
            CreateTargetlessEntry(
                AbilityDefinition definition,
                IAbilityExecutor executor)
        {
            return new AbilityRuntimeEntry(
                definition,
                null,
                executor);
        }

        private static TestPlayerAbilityDefinition
            CreateDefinition(
                string stableId,
                float cooldown)
        {
            TestPlayerAbilityDefinition definition =
                ScriptableObject.CreateInstance<
                    TestPlayerAbilityDefinition>();

            SetPrivateField(
                definition,
                "_stableId",
                stableId,
                typeof(ItemDefinition));

            SetPrivateField(
                definition,
                "_cooldown",
                cooldown,
                typeof(AbilityDefinition));

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
                Is.Not.Null,
                $"Field '{fieldName}' could not be found.");

            field.SetValue(
                target,
                value);
        }

        private sealed class FakeTargetSelector :
            IAbilityTargetSelector
        {
            public int SelectionCount
            {
                get;
                private set;
            }

            public bool TrySelectTarget(
                Vector3 origin,
                out Transform target)
            {
                SelectionCount++;

                target = null;

                return false;
            }
        }

        private sealed class FakeAbilityExecutor :
            IAbilityExecutor
        {
            public int ExecutionCount
            {
                get;
                private set;
            }

            public AbilityExecutionResult TryExecute(
                in AbilityExecutionContext context)
            {
                ExecutionCount++;

                return AbilityExecutionResult.Performed;
            }
        }
    }

    internal sealed class TestPlayerAbilityDefinition :
        AbilityDefinition
    {
    }
}