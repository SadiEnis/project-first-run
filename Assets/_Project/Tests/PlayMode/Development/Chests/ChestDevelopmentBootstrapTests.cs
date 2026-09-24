#if UNITY_EDITOR

using System.Collections;
using System.Linq;
using System.Reflection;
using NUnit.Framework;
using ProjectFirstRun.Abilities;
using ProjectFirstRun.Abilities.Fireball;
using ProjectFirstRun.Builds;
using ProjectFirstRun.Chests;
using ProjectFirstRun.Chests.Spawning;
using ProjectFirstRun.Development.Chests;
using ProjectFirstRun.Development.Abilities;
using ProjectFirstRun.Enemies;
using ProjectFirstRun.Stats;
using ProjectFirstRun.Player;
using ProjectFirstRun.Items;
using ProjectFirstRun.Rewards.Claims;
using ProjectFirstRun.UI.Rewards;
using UnityEditor;
using UnityEngine;
using UnityEngine.TestTools;
using Object = UnityEngine.Object;

namespace ProjectFirstRun.Tests.PlayMode.Development.Chests
{
    public sealed class ChestDevelopmentBootstrapTests
    {
        private const string PlayerPrefabPath =
            "Assets/_Project/Prefabs/Player/Player.prefab";

        private const string SelectionPrefabPath =
            "Assets/_Project/Prefabs/UI/RewardSelectionUI.prefab";

        private const string DefinitionPath =
            "Assets/_Project/Data/Chests/Dev/CD_DevelopmentChest.asset";

        private GameObject _playerObject;
        private GameObject _selectionObject;
        private GameObject _bootstrapObject;

        [UnityTest]
        public IEnumerator CommonShowcase_OffersAndClaimsEachCategoryOnceWithLabelledGreyPresentation()
        {
            CreatePlayer();
            // Match Test_Waves' WeaponSwitchingDevelopmentBootstrap, not the one-slot default.
            _playerObject.GetComponent<PlayerStartingLoadoutInitializer>().Initialize(
                new PlayerBuildCapacity(PlayerBuildCapacity.MaximumWeaponSlots,
                    PlayerBuildCapacity.DefaultAbilitySlots, PlayerBuildCapacity.DefaultUpgradeSlots));
            yield return null;
            var fireballSetup = CreateFireballSetup(false);
            fireballSetup.Install();
            var selection = CreateSelectionController();
            var bootstrap = CreateCommonBootstrap(selection, true);
            float deadline = Time.realtimeSinceStartup + 2f;
            while (!bootstrap.HasSpawnedChest && Time.realtimeSinceStartup < deadline) yield return null;
            Assert.That(bootstrap.SpawnedChests.Count, Is.EqualTo(3));
            var build = _playerObject.GetComponent<PlayerBuildController>().Build;
            Assert.That(build.Upgrades, Is.Empty);
            Assert.That(build.Abilities, Is.Empty);
            var spawner = _bootstrapObject.GetComponent<ChestSpawner>();
            Vector3 firstPosition = bootstrap.SpawnedChest.transform.position;
            for (int i = 0; i < 3; i++)
            {
                var chest = bootstrap.SpawnedChests[i];
                Assert.That(chest.transform.position.x, Is.EqualTo(firstPosition.x + (i == 0 ? 0f : i == 1 ? -3f : 3f)));
                var view = chest.GetComponent<ChestView>();
                var labels = view.VisualRoot.GetComponentsInChildren<TextMesh>(true);
                Assert.That(labels.Length, Is.EqualTo(1));
                Assert.That(labels.All(label => label.text == chest.Definition.DisplayName && label.font != null), Is.True);
                var body = view.VisualRoot.GetComponentsInChildren<Renderer>().First(renderer => renderer.GetComponent<TextMesh>() == null);
                var properties = new MaterialPropertyBlock();
                body.GetPropertyBlock(properties);
                Color tint = properties.GetColor("_BaseColor");
                Assert.That(tint.r, Is.EqualTo(0.55f).Within(0.001f));
                Assert.That(tint.g, Is.EqualTo(0.55f).Within(0.001f));
                Assert.That(tint.b, Is.EqualTo(0.55f).Within(0.001f));
                Assert.That(tint.a, Is.EqualTo(1f).Within(0.001f));
                view.enabled = false;
                view.enabled = true;
                Assert.That(view.VisualRoot.GetComponentsInChildren<TextMesh>(true).Length, Is.EqualTo(1), "Reenable must not duplicate labels.");

                Assert.That(chest.TryOpen(), Is.EqualTo(ChestOpenResult.SelectionOpened), chest.Definition.DisplayName);
                int expectedChoiceCount = i == 0 ? 3 : 1;
                Assert.That(chest.ActiveSession.Offer.Choices.Count, Is.EqualTo(expectedChoiceCount),
                    "The starting weapon level-up and both new weapons are eligible.");
                var reward = chest.ActiveSession.Offer.Choices.First(choice =>
                    build.GetLevel(choice.Category, choice.StableId) == 0);
                Assert.That(reward.Category, Is.EqualTo((ItemCategory)i));
                _selectionObject.GetComponentsInChildren<RewardChoiceView>(true)
                    .First(choice => choice.gameObject.activeInHierarchy && choice.Button.interactable &&
                        ReferenceEquals(choice.Definition, reward)).Button.onClick.Invoke();
                Assert.That(build.Contains(reward.Category, reward.StableId), Is.True);
                Assert.That(chest.Status, Is.EqualTo(ChestStatus.Opened));
                Assert.That(view.VisualRoot.activeSelf, Is.False);
                Assert.That(chest.TryOpen(), Is.EqualTo(ChestOpenResult.AlreadyOpened));
                Assert.That(selection.IsOpen, Is.False);
                Assert.That(Time.timeScale, Is.EqualTo(1f));

                var request = new ChestSpawnRequest(chest.Definition, new Vector3(20f, 0f, i * 3f), Quaternion.identity);
                var exhausted = spawner.Spawn(in request).ChestController;
                Assert.That(exhausted.TryOpen(), Is.EqualTo(ChestOpenResult.SelectionOpened));
                Assert.That(selection.Select(exhausted.ActiveSession.Offer.Choices.First(choice =>
                    build.GetLevel(choice.Category, choice.StableId) > 0)),
                    Is.EqualTo(RewardClaimResult.Claimed));
                Assert.That(exhausted.Status, Is.EqualTo(ChestStatus.Opened));
            }
            Assert.Throws<System.InvalidOperationException>(() => bootstrap.SpawnChest());
            Object.DestroyImmediate(bootstrap.SpawnedChest.gameObject);
            Assert.Throws<System.InvalidOperationException>(() => bootstrap.SpawnChest(), "Destroying the primary does not reset the batch entitlement.");
        }

        [UnityTest]
        public IEnumerator FireballSetup_DefaultStillGrantsAbilityInOtherScenes()
        {
            CreatePlayer();
            yield return null;
            var setup = CreateFireballSetup(null);
            setup.Install();
            Assert.That(setup.IsInstalled, Is.True);
            Assert.That(_playerObject.GetComponent<PlayerAbilityController>().AbilityCount, Is.EqualTo(1));
            Assert.That(_playerObject.GetComponent<PlayerBuildController>().Build.Abilities.Count, Is.EqualTo(1));
        }

        private FireballDevelopmentBootstrap CreateFireballSetup(bool? grant)
        {
            // Keep the setup inactive so this test drives Install explicitly instead of Start.
            var setupObject = new GameObject("Fireball setup");
            setupObject.transform.SetParent(_playerObject.transform);
            setupObject.SetActive(false);
            var setup = setupObject.AddComponent<FireballDevelopmentBootstrap>();
            var registry = setupObject.AddComponent<EnemyRegistry>();
            SetField(setup, "_fireballDefinition", AssetDatabase.LoadAssetAtPath<FireballDefinition>(
                "Assets/_Project/Data/Abilities/Fireball/AD_Fireball.asset"));
            SetField(setup, "_playerAbilityAcquisitionController", _playerObject.GetComponent<PlayerAbilityAcquisitionController>());
            SetField(setup, "_playerStatsController", _playerObject.GetComponent<PlayerStatsController>());
            SetField(setup, "_damageSource", _playerObject);
            SetField(setup, "_enemyRegistry", registry);
            if (grant.HasValue) SetField(setup, "_grantStartingAbility", grant.Value);
            return setup;
        }

        [UnityTest]
        public IEnumerator CommonLabel_FollowsMainCameraAndToleratesMissingCamera()
        {
            CreatePlayer();
            yield return null;
            var bootstrap = CreateCommonBootstrap(CreateSelectionController(), false);
            var chest = bootstrap.SpawnChest();
            var label = chest.GetComponent<ChestView>().VisualRoot.GetComponentInChildren<TextMesh>();
            foreach (Camera playerCamera in _playerObject.GetComponentsInChildren<Camera>(true))
                playerCamera.tag = "Untagged";
            var cameraObject = new GameObject("Label test camera");
            cameraObject.transform.SetParent(_playerObject.transform);
            var viewingCamera = cameraObject.AddComponent<Camera>();
            viewingCamera.tag = "MainCamera";
            viewingCamera.transform.rotation = Quaternion.Euler(20f, 135f, 0f);
            yield return null;
            yield return null;
            Assert.That(Quaternion.Angle(label.transform.rotation, viewingCamera.transform.rotation), Is.LessThan(0.1f));
            viewingCamera.transform.rotation = Quaternion.Euler(-15f, -40f, 0f);
            yield return null;
            yield return null;
            Assert.That(Quaternion.Angle(label.transform.rotation, viewingCamera.transform.rotation), Is.LessThan(0.1f));
            Quaternion lastRotation = label.transform.rotation;
            cameraObject.SetActive(false);
            yield return null;
            Assert.That(Quaternion.Angle(label.transform.rotation, lastRotation), Is.LessThan(0.1f));
        }

        [UnityTest]
        public IEnumerator InvalidAdditionalDefinition_DoesNotSpawnAPartialShowcase()
        {
            CreatePlayer();
            yield return null;
            var bootstrap = CreateCommonBootstrap(CreateSelectionController(), false);
            SetField(bootstrap, "_additionalChestDefinitions", new ChestDefinition[] { null });
            Assert.Throws<System.InvalidOperationException>(() => bootstrap.SpawnChest());
            Assert.That(bootstrap.HasSpawnedChest, Is.False);
            Assert.That(bootstrap.SpawnedChests, Is.Empty);
        }

        [UnityTest]
        public IEnumerator LaterSpawnFailure_CleansUpEarlierShowcaseChest()
        {
            CreatePlayer();
            yield return null;
            var bootstrap = CreateCommonBootstrap(CreateSelectionController(), false);
            var invalidPrefab = new GameObject("Invalid view template");
            invalidPrefab.SetActive(false);
            invalidPrefab.AddComponent<ChestController>();
            invalidPrefab.AddComponent<ChestView>();
            var definition = Object.Instantiate(AssetDatabase.LoadAssetAtPath<ChestDefinition>(
                "Assets/_Project/Data/Chests/Dev/CD_AbilityChest.asset"));
            try
            {
                SetField(definition, "_worldPrefab", invalidPrefab);
                SetField(bootstrap, "_additionalChestDefinitions", new[] { definition });
                Assert.Throws<System.InvalidOperationException>(() => bootstrap.SpawnChest());
                Assert.That(bootstrap.HasSpawnedChest, Is.False);
                Assert.That(bootstrap.SpawnedChests, Is.Empty);
                yield return null;
                Assert.That(Object.FindObjectsByType<ChestController>(FindObjectsSortMode.None), Is.Empty);
            }
            finally { Object.DestroyImmediate(definition); Object.DestroyImmediate(invalidPrefab); }
        }

        private ChestDevelopmentBootstrap CreateCommonBootstrap(RewardSelectionController selection, bool activate)
        {
            _bootstrapObject = new GameObject("Common chest showcase");
            _bootstrapObject.SetActive(false);
            var spawner = _bootstrapObject.AddComponent<ChestSpawner>();
            var bootstrap = _bootstrapObject.AddComponent<ChestDevelopmentBootstrap>();
            var point = new GameObject("SpawnPoint").transform;
            point.SetParent(_bootstrapObject.transform);
            point.position = new Vector3(0f, 0.12f, 4f);
            ChestDefinition Load(string name) => AssetDatabase.LoadAssetAtPath<ChestDefinition>(
                $"Assets/_Project/Data/Chests/Dev/CD_{name}Chest.asset");
            SetField(bootstrap, "_additionalChestDefinitions", new[] { Load("Ability"), Load("Upgrade") });
            bootstrap.Initialize(spawner, _playerObject.GetComponent<PlayerBuildController>(), selection, Load("Weapon"), point);
            if (activate) _bootstrapObject.SetActive(true);
            return bootstrap;
        }

        private static void SetField(object target, string name, object value) =>
            target.GetType().GetField(name, BindingFlags.Instance | BindingFlags.NonPublic).SetValue(target, value);

        [TearDown]
        public void TearDown()
        {
            Time.timeScale = 1f;

            DestroyImmediateIfExists(_bootstrapObject);
            DestroyImmediateIfExists(_selectionObject);
            DestroyImmediateIfExists(_playerObject);

            ChestController[] remainingChests =
                Object.FindObjectsByType<ChestController>(
                    FindObjectsInactive.Include,
                    FindObjectsSortMode.None);

            foreach (ChestController chest in remainingChests)
            {
                DestroyImmediateIfExists(chest.gameObject);
            }
        }

        [UnityTest]
        public IEnumerator Start_WhenPlayerBuildIsReady_SpawnsDevelopmentChest()
        {
            CreatePlayer();
            RewardSelectionController selectionController =
                CreateSelectionController();

            ChestDefinition definition =
                AssetDatabase.LoadAssetAtPath<ChestDefinition>(
                    DefinitionPath);

            Assert.That(definition, Is.Not.Null);

            _bootstrapObject =
                new GameObject("ChestDevelopmentBootstrap_Test");

            _bootstrapObject.SetActive(false);

            ChestSpawner spawner =
                _bootstrapObject.AddComponent<ChestSpawner>();

            ChestDevelopmentBootstrap bootstrap =
                _bootstrapObject.AddComponent<ChestDevelopmentBootstrap>();

            Transform spawnPoint =
                new GameObject("SpawnPoint").transform;

            spawnPoint.SetParent(_bootstrapObject.transform);
            spawnPoint.position = new Vector3(2f, 0.75f, 4f);
            spawnPoint.rotation = Quaternion.Euler(0f, 35f, 0f);

            bootstrap.Initialize(
                spawner,
                _playerObject.GetComponent<PlayerBuildController>(),
                selectionController,
                definition,
                spawnPoint);

            _bootstrapObject.SetActive(true);

            for (int frame = 0;
                 frame < 5 && !bootstrap.HasSpawnedChest;
                 frame++)
            {
                yield return null;
            }

            Assert.That(bootstrap.HasSpawnedChest, Is.True);
            Assert.That(bootstrap.SpawnedChest.IsInitialized, Is.True);
            Assert.That(bootstrap.SpawnedChest.Definition, Is.SameAs(definition));
            Assert.That(
                bootstrap.SpawnedChest.transform.position,
                Is.EqualTo(spawnPoint.position));
            Assert.That(selectionController.IsOpen, Is.False);
            Assert.That(Time.timeScale, Is.EqualTo(1f));
        }

        private void CreatePlayer()
        {
            GameObject prefab =
                AssetDatabase.LoadAssetAtPath<GameObject>(
                    PlayerPrefabPath);

            Assert.That(prefab, Is.Not.Null);

            _playerObject = Object.Instantiate(prefab);

            SerializedObject serializedPlayer =
                new SerializedObject(
                    _playerObject.GetComponent<PlayerController>());

            serializedPlayer.FindProperty("_lockCursorWhileControlled")
                .boolValue = false;

            serializedPlayer.ApplyModifiedPropertiesWithoutUndo();
        }

        private RewardSelectionController CreateSelectionController()
        {
            GameObject prefab =
                AssetDatabase.LoadAssetAtPath<GameObject>(
                    SelectionPrefabPath);

            Assert.That(prefab, Is.Not.Null);

            _selectionObject = Object.Instantiate(prefab);

            RewardSelectionController controller =
                _selectionObject.GetComponent<RewardSelectionController>();

            controller.Initialize(
                _selectionObject.GetComponent<RewardSelectionView>(),
                _playerObject.GetComponent<PlayerRewardClaimController>(),
                _playerObject.GetComponent<PlayerController>(),
                _playerObject.GetComponent<PlayerDeathController>());

            return controller;
        }

        private static void DestroyImmediateIfExists(
            GameObject target)
        {
            if (target != null)
            {
                Object.DestroyImmediate(target);
            }
        }
    }
}

#endif
