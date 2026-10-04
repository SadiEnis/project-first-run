using System.Reflection;
using NUnit.Framework;
using ProjectFirstRun.Abilities;
using ProjectFirstRun.Abilities.Execution;
using ProjectFirstRun.Abilities.Targeting;
using UnityEngine;
using Object = UnityEngine.Object;

namespace ProjectFirstRun.Tests.EditMode.Abilities
{
    public sealed class AbilityRuntimeEntryTests
    {
        private TestRuntimeAbilityDefinition _definition;

        [Test]
        public void ReducedCooldownOnlyAppliesToNewSuccessfulCasts()
        {
            var executor = new FakeAbilityExecutor();
            var entry = new AbilityRuntimeEntry(_definition, null, executor);
            Assert.That(entry.TryAutoCast(Vector3.zero), Is.EqualTo(AbilityAutoCastResult.Performed));
            entry.Tick(.5f);
            Assert.That(entry.TryAutoCast(Vector3.zero, .75f), Is.EqualTo(AbilityAutoCastResult.OnCooldown));
            Assert.That(entry.State.CooldownRemaining, Is.EqualTo(1.5f));
            entry.Tick(1.5f);
            executor.Result = AbilityExecutionResult.Failed;
            Assert.That(entry.TryAutoCast(Vector3.zero, .75f), Is.EqualTo(AbilityAutoCastResult.ExecutionFailed));
            Assert.That(entry.State.IsReady, Is.True);
            executor.Result = AbilityExecutionResult.Performed;
            entry.TryAutoCast(Vector3.zero, .75f);
            Assert.That(entry.State.CooldownRemaining, Is.EqualTo(1.5f));
            Assert.That(entry.State.Cooldown, Is.EqualTo(2f));
        }

        [TestCase(.95f, 1.9f)]
        [TestCase(.90f, 1.8f)]
        [TestCase(.85f, 1.7f)]
        [TestCase(.80f, 1.6f)]
        [TestCase(.75f, 1.5f)]
        [TestCase(-1f, .5f)]
        [TestCase(2f, 2f)]
        public void CooldownMultiplierHasExpectedDurationAndCap(float multiplier, float expected)
        {
            var entry = new AbilityRuntimeEntry(_definition, null, new FakeAbilityExecutor());
            entry.TryAutoCast(Vector3.zero, multiplier);
            Assert.That(entry.State.CooldownRemaining, Is.EqualTo(expected).Within(.0001f));
        }

        [Test]
        public void InvalidMultiplierCannotExecuteAndStatSourcesAdd()
        {
            var executor = new FakeAbilityExecutor();
            var entry = new AbilityRuntimeEntry(_definition, null, executor);
            Assert.Throws<System.ArgumentOutOfRangeException>(() => entry.TryAutoCast(Vector3.zero, float.NaN));
            Assert.That(executor.ExecutionCount, Is.Zero);
            var stats = new ProjectFirstRun.Stats.PlayerStatCollection();
            stats.Add(new ProjectFirstRun.Stats.StatModifier(ProjectFirstRun.Stats.PlayerStatType.AbilityCooldown,
                ProjectFirstRun.Stats.StatModifierOperation.AdditivePercent, -.25f, "hourglass"));
            stats.Add(new ProjectFirstRun.Stats.StatModifier(ProjectFirstRun.Stats.PlayerStatType.AbilityCooldown,
                ProjectFirstRun.Stats.StatModifierOperation.AdditivePercent, -.15f, "other"));
            Assert.That(AbilityCooldownScaling.Multiplier(stats), Is.EqualTo(.6f).Within(.0001f));
        }

        [SetUp]
        public void SetUp()
        {
            _definition =
                ScriptableObject
                    .CreateInstance<TestRuntimeAbilityDefinition>();

            SetCooldown(
                _definition,
                2f);
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
        public void Constructor_WithValidDependencies_CreatesReadyEntry()
        {
            FakeTargetSelector selector =
                new FakeTargetSelector();

            FakeAbilityExecutor executor =
                new FakeAbilityExecutor();

            AbilityRuntimeEntry entry =
                new AbilityRuntimeEntry(
                    _definition,
                    selector,
                    executor);

            Assert.That(
                entry.Definition,
                Is.SameAs(_definition));

            Assert.That(
                entry.IsReady,
                Is.True);

            Assert.That(
                entry.State.CooldownRemaining,
                Is.EqualTo(0f));
        }

        [Test]
        public void Constructor_WithNullDefinition_Throws()
        {
            FakeAbilityExecutor executor =
                new FakeAbilityExecutor();

            Assert.That(
                () =>
                    new AbilityRuntimeEntry(
                        null,
                        null,
                        executor),
                Throws.ArgumentNullException);
        }

        [Test]
        public void Constructor_WithNullExecutor_Throws()
        {
            Assert.That(
                () =>
                    new AbilityRuntimeEntry(
                        _definition,
                        null,
                        null),
                Throws.ArgumentNullException);
        }

        [Test]
        public void TryAutoCast_WithTarget_PerformsExecutionAndStartsCooldown()
        {
            GameObject targetObject =
                new GameObject("AbilityTarget");

            try
            {
                FakeTargetSelector selector =
                    new FakeTargetSelector(
                        targetObject.transform);

                FakeAbilityExecutor executor =
                    new FakeAbilityExecutor();

                AbilityRuntimeEntry entry =
                    CreateEntry(
                        selector,
                        executor);

                AbilityAutoCastResult result =
                    entry.TryAutoCast(
                        Vector3.zero);

                Assert.That(
                    result,
                    Is.EqualTo(
                        AbilityAutoCastResult.Performed));

                Assert.That(
                    executor.ExecutionCount,
                    Is.EqualTo(1));

                Assert.That(
                    executor.LastContext.Target,
                    Is.SameAs(
                        targetObject.transform));

                Assert.That(
                    entry.IsReady,
                    Is.False);

                Assert.That(
                    entry.State.CooldownRemaining,
                    Is.EqualTo(2f));
            }
            finally
            {
                Object.DestroyImmediate(
                    targetObject);
            }
        }

        [Test]
        public void TryAutoCast_WhileOnCooldown_DoesNotSelectOrExecuteAgain()
        {
            GameObject targetObject =
                new GameObject("AbilityTarget");

            try
            {
                FakeTargetSelector selector =
                    new FakeTargetSelector(
                        targetObject.transform);

                FakeAbilityExecutor executor =
                    new FakeAbilityExecutor();

                AbilityRuntimeEntry entry =
                    CreateEntry(
                        selector,
                        executor);

                entry.TryAutoCast(
                    Vector3.zero);

                AbilityAutoCastResult result =
                    entry.TryAutoCast(
                        Vector3.zero);

                Assert.That(
                    result,
                    Is.EqualTo(
                        AbilityAutoCastResult.OnCooldown));

                Assert.That(
                    selector.SelectionCount,
                    Is.EqualTo(1));

                Assert.That(
                    executor.ExecutionCount,
                    Is.EqualTo(1));
            }
            finally
            {
                Object.DestroyImmediate(
                    targetObject);
            }
        }

        [Test]
        public void TryAutoCast_WhenNoTarget_ReturnsNoTargetWithoutCooldown()
        {
            FakeTargetSelector selector =
                new FakeTargetSelector();

            FakeAbilityExecutor executor =
                new FakeAbilityExecutor();

            AbilityRuntimeEntry entry =
                CreateEntry(
                    selector,
                    executor);

            AbilityAutoCastResult result =
                entry.TryAutoCast(
                    Vector3.zero);

            Assert.That(
                result,
                Is.EqualTo(
                    AbilityAutoCastResult.NoTarget));

            Assert.That(
                selector.SelectionCount,
                Is.EqualTo(1));

            Assert.That(
                executor.ExecutionCount,
                Is.EqualTo(0));

            Assert.That(
                entry.IsReady,
                Is.True);

            Assert.That(
                entry.State.CooldownRemaining,
                Is.EqualTo(0f));
        }

        [Test]
        public void TryAutoCast_WhenExecutionFails_DoesNotStartCooldown()
        {
            GameObject targetObject =
                new GameObject("AbilityTarget");

            try
            {
                FakeTargetSelector selector =
                    new FakeTargetSelector(
                        targetObject.transform);

                FakeAbilityExecutor executor =
                    new FakeAbilityExecutor();

                executor.Result =
                    AbilityExecutionResult.Failed;

                AbilityRuntimeEntry entry =
                    CreateEntry(
                        selector,
                        executor);

                AbilityAutoCastResult result =
                    entry.TryAutoCast(
                        Vector3.zero);

                Assert.That(
                    result,
                    Is.EqualTo(
                        AbilityAutoCastResult.ExecutionFailed));

                Assert.That(
                    executor.ExecutionCount,
                    Is.EqualTo(1));

                Assert.That(
                    entry.IsReady,
                    Is.True);

                Assert.That(
                    entry.State.CooldownRemaining,
                    Is.EqualTo(0f));
            }
            finally
            {
                Object.DestroyImmediate(
                    targetObject);
            }
        }

        [Test]
        public void TryAutoCast_WithoutTargetSelector_PerformsTargetlessExecution()
        {
            FakeAbilityExecutor executor =
                new FakeAbilityExecutor();

            AbilityRuntimeEntry entry =
                CreateEntry(
                    null,
                    executor);

            Vector3 origin =
                new Vector3(
                    1f,
                    2f,
                    3f);

            AbilityAutoCastResult result =
                entry.TryAutoCast(
                    origin);

            Assert.That(
                result,
                Is.EqualTo(
                    AbilityAutoCastResult.Performed));

            Assert.That(
                executor.ExecutionCount,
                Is.EqualTo(1));

            Assert.That(
                executor.LastContext.HasTarget,
                Is.False);

            Assert.That(
                executor.LastContext.Origin,
                Is.EqualTo(origin));

            Assert.That(
                entry.IsReady,
                Is.False);
        }

        [Test]
        public void Tick_AfterSuccessfulCast_ReducesCooldown()
        {
            FakeAbilityExecutor executor =
                new FakeAbilityExecutor();

            AbilityRuntimeEntry entry =
                CreateEntry(
                    null,
                    executor);

            entry.TryAutoCast(
                Vector3.zero);

            entry.Tick(
                0.75f);

            Assert.That(
                entry.State.CooldownRemaining,
                Is.EqualTo(1.25f)
                    .Within(0.0001f));
        }

        [Test]
        public void Tick_UntilCooldownCompletes_AllowsAnotherCast()
        {
            FakeAbilityExecutor executor =
                new FakeAbilityExecutor();

            AbilityRuntimeEntry entry =
                CreateEntry(
                    null,
                    executor);

            entry.TryAutoCast(
                Vector3.zero);

            entry.Tick(
                2f);

            AbilityAutoCastResult result =
                entry.TryAutoCast(
                    Vector3.zero);

            Assert.That(
                result,
                Is.EqualTo(
                    AbilityAutoCastResult.Performed));

            Assert.That(
                executor.ExecutionCount,
                Is.EqualTo(2));
        }

        [Test]
        public void TryAutoCast_WithInvalidOrigin_Throws()
        {
            FakeAbilityExecutor executor =
                new FakeAbilityExecutor();

            AbilityRuntimeEntry entry =
                CreateEntry(
                    null,
                    executor);

            Vector3 invalidOrigin =
                new Vector3(
                    float.NaN,
                    0f,
                    0f);

            Assert.That(
                () =>
                    entry.TryAutoCast(
                        invalidOrigin),
                Throws.ArgumentException);

            Assert.That(
                executor.ExecutionCount,
                Is.EqualTo(0));
        }

        private AbilityRuntimeEntry CreateEntry(
            IAbilityTargetSelector selector,
            IAbilityExecutor executor)
        {
            return new AbilityRuntimeEntry(
                _definition,
                selector,
                executor);
        }

        private static void SetCooldown(
            AbilityDefinition definition,
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
                definition,
                cooldown);
        }

        private sealed class FakeTargetSelector :
            IAbilityTargetSelector
        {
            private readonly Transform _target;

            public int SelectionCount
            {
                get;
                private set;
            }

            public FakeTargetSelector(
                Transform target = null)
            {
                _target = target;
            }

            public bool TrySelectTarget(
                Vector3 origin,
                out Transform target)
            {
                SelectionCount++;

                target =
                    _target;

                return target != null;
            }
        }

        private sealed class FakeAbilityExecutor :
            IAbilityExecutor
        {
            public AbilityExecutionResult Result
            {
                get;
                set;
            } =
                AbilityExecutionResult.Performed;

            public int ExecutionCount
            {
                get;
                private set;
            }

            public AbilityExecutionContext LastContext
            {
                get;
                private set;
            }

            public AbilityExecutionResult TryExecute(
                in AbilityExecutionContext context)
            {
                ExecutionCount++;

                LastContext =
                    context;

                return Result;
            }
        }
    }

    internal sealed class TestRuntimeAbilityDefinition :
        AbilityDefinition
    {
    }
}
