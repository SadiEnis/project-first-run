using System.Reflection;
using NUnit.Framework;
using ProjectFirstRun.Builds;
using ProjectFirstRun.Items;
using ProjectFirstRun.Weapons;
using UnityEngine;
using Object = UnityEngine.Object;

namespace ProjectFirstRun.Tests.EditMode.Builds
{
    public sealed class PlayerBuildControllerTests
    {
        private GameObject _controllerObject;
        private PlayerBuildController _controller;

        private WeaponDefinition _firstWeapon;
        private WeaponDefinition _secondWeapon;

        [SetUp]
        public void SetUp()
        {
            _controllerObject =
                new GameObject(
                    "PlayerBuildController_Test");

            _controller =
                _controllerObject
                    .AddComponent<PlayerBuildController>();

            _firstWeapon =
                CreateWeaponDefinition(
                    "weapon.test_first");

            _secondWeapon =
                CreateWeaponDefinition(
                    "weapon.test_second");
        }

        [TearDown]
        public void TearDown()
        {
            if (_controllerObject != null)
            {
                Object.DestroyImmediate(
                    _controllerObject);
            }

            if (_firstWeapon != null)
            {
                Object.DestroyImmediate(
                    _firstWeapon);
            }

            if (_secondWeapon != null)
            {
                Object.DestroyImmediate(
                    _secondWeapon);
            }
        }

        [Test]
        public void NewController_IsNotInitialized()
        {
            Assert.That(
                _controller.IsInitialized,
                Is.False);
        }

        [Test]
        public void Build_BeforeInitialize_Throws()
        {
            Assert.That(
                () =>
                {
                    PlayerBuild build =
                        _controller.Build;
                },
                Throws.InvalidOperationException);
        }

        [Test]
        public void Initialize_WithValidCapacity_CreatesEmptyBuild()
        {
            PlayerBuildCapacity capacity =
                new PlayerBuildCapacity(
                    2,
                    4,
                    6);

            _controller.Initialize(
                capacity);

            Assert.That(
                _controller.IsInitialized,
                Is.True);

            Assert.That(
                _controller.Build.Capacity,
                Is.SameAs(capacity));

            Assert.That(
                _controller.Build.GetCount(
                    ItemCategory.Weapon),
                Is.EqualTo(0));

            Assert.That(
                _controller.Build.GetCount(
                    ItemCategory.Ability),
                Is.EqualTo(0));

            Assert.That(
                _controller.Build.GetCount(
                    ItemCategory.Upgrade),
                Is.EqualTo(0));
        }

        [Test]
        public void Initialize_WithNullCapacity_Throws()
        {
            Assert.That(
                () => _controller.Initialize(null),
                Throws.ArgumentNullException);

            Assert.That(
                _controller.IsInitialized,
                Is.False);
        }

        [Test]
        public void Initialize_WhenAlreadyInitialized_Throws()
        {
            PlayerBuildCapacity firstCapacity =
                PlayerBuildCapacity.CreateDefault();

            _controller.Initialize(
                firstCapacity);

            PlayerBuild originalBuild =
                _controller.Build;

            PlayerBuildCapacity secondCapacity =
                new PlayerBuildCapacity(
                    2,
                    4,
                    6);

            Assert.That(
                () =>
                    _controller.Initialize(
                        secondCapacity),
                Throws.InvalidOperationException);

            Assert.That(
                _controller.Build,
                Is.SameAs(originalBuild));

            Assert.That(
                _controller.Build.Capacity,
                Is.SameAs(firstCapacity));
        }

        [Test]
        public void TryAdd_BeforeInitialize_Throws()
        {
            Assert.That(
                () =>
                    _controller.TryAdd(
                        _firstWeapon),
                Throws.InvalidOperationException);
        }

        [Test]
        public void TryAdd_WithNullDefinition_Throws()
        {
            InitializeDefault();

            Assert.That(
                () => _controller.TryAdd(null),
                Throws.ArgumentNullException);
        }

        [Test]
        public void TryAdd_WithValidDefinition_AddsDefinitionIdentity()
        {
            InitializeDefault();

            PlayerBuildAddResult result =
                _controller.TryAdd(
                    _firstWeapon);

            Assert.That(
                result,
                Is.EqualTo(
                    PlayerBuildAddResult.Added));

            Assert.That(
                _controller.Build.Contains(
                    ItemCategory.Weapon,
                    "weapon.test_first"),
                Is.True);

            Assert.That(
                _controller.Contains(
                    _firstWeapon),
                Is.True);
        }

        [Test]
        public void TryAdd_WithAlreadyOwnedDefinition_ReturnsAlreadyOwned()
        {
            InitializeDefault();

            _controller.TryAdd(
                _firstWeapon);

            PlayerBuildAddResult result =
                _controller.TryAdd(
                    _firstWeapon);

            Assert.That(
                result,
                Is.EqualTo(
                    PlayerBuildAddResult.AlreadyOwned));

            Assert.That(
                _controller.Build.GetCount(
                    ItemCategory.Weapon),
                Is.EqualTo(1));
        }

        [Test]
        public void TryAdd_WhenCategoryIsFull_ReturnsCapacityReached()
        {
            InitializeDefault();

            PlayerBuildAddResult firstResult =
                _controller.TryAdd(
                    _firstWeapon);

            PlayerBuildAddResult secondResult =
                _controller.TryAdd(
                    _secondWeapon);

            Assert.That(
                firstResult,
                Is.EqualTo(
                    PlayerBuildAddResult.Added));

            Assert.That(
                secondResult,
                Is.EqualTo(
                    PlayerBuildAddResult.CapacityReached));

            Assert.That(
                _controller.Contains(
                    _firstWeapon),
                Is.True);

            Assert.That(
                _controller.Contains(
                    _secondWeapon),
                Is.False);
        }

        [Test]
        public void Contains_BeforeInitialize_Throws()
        {
            Assert.That(
                () =>
                    _controller.Contains(
                        _firstWeapon),
                Throws.InvalidOperationException);
        }

        [Test]
        public void Contains_WithNullDefinition_Throws()
        {
            InitializeDefault();

            Assert.That(
                () => _controller.Contains(null),
                Throws.ArgumentNullException);
        }

        private void InitializeDefault()
        {
            _controller.Initialize(
                PlayerBuildCapacity.CreateDefault());
        }

        private static WeaponDefinition
            CreateWeaponDefinition(
                string stableId)
        {
            WeaponDefinition definition =
                ScriptableObject
                    .CreateInstance<WeaponDefinition>();

            SetItemStableId(
                definition,
                stableId);

            return definition;
        }

        private static void SetItemStableId(
            ItemDefinition itemDefinition,
            string stableId)
        {
            FieldInfo stableIdField =
                typeof(ItemDefinition).GetField(
                    "_stableId",
                    BindingFlags.Instance |
                    BindingFlags.NonPublic);

            Assert.That(
                stableIdField,
                Is.Not.Null);

            stableIdField.SetValue(
                itemDefinition,
                stableId);
        }
    }
}