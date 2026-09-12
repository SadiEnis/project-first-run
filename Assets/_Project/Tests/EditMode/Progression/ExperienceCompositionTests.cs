#if UNITY_EDITOR
using System.Linq;
using NUnit.Framework;
using ProjectFirstRun.Chests.Interaction;
using ProjectFirstRun.Combat;
using ProjectFirstRun.Development;
using ProjectFirstRun.Enemies;
using ProjectFirstRun.Progression;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace ProjectFirstRun.Tests.EditMode.Progression
{
    public sealed class ExperienceCompositionTests
    {
        private const string PickupPath = "Assets/_Project/Prefabs/Progression/ExperiencePickup.prefab";

        [Test]
        public void PickupPrefab_HasNonBlockingTriggerAndVisibleCyanSphere()
        {
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(PickupPath);
            Assert.That(prefab, Is.Not.Null);
            Assert.That(prefab.activeSelf, Is.True);
            Assert.That(prefab.GetComponent<ExperiencePickup>(), Is.Not.Null);
            SphereCollider trigger = prefab.GetComponent<SphereCollider>();
            Assert.That(trigger.isTrigger, Is.True);
            Assert.That(trigger.radius, Is.EqualTo(0.85f));
            Assert.That(prefab.GetComponentsInChildren<Collider>(), Has.Length.EqualTo(1));
            Assert.That(prefab.layer, Is.EqualTo(2));
            Assert.That(Physics.GetIgnoreLayerCollision(2, 3), Is.False);
            Assert.That(prefab.GetComponent<Rigidbody>().isKinematic, Is.True);
            Assert.That(prefab.GetComponent<Rigidbody>().useGravity, Is.False);
            MeshRenderer renderer = prefab.GetComponentInChildren<MeshRenderer>();
            Assert.That(renderer.sharedMaterial.shader.name, Is.EqualTo("Universal Render Pipeline/Unlit"));
            Assert.That(renderer.sharedMaterial.GetColor("_BaseColor").b, Is.EqualTo(1f));
        }

        [Test]
        public void PlayerPrefab_CollectsXPWithoutPickupBlockingChestRay()
        {
            GameObject player = AssetDatabase.LoadAssetAtPath<GameObject>(
                "Assets/_Project/Prefabs/Player/Player.prefab");
            Assert.That(player.GetComponent<PlayerExperienceController>(), Is.Not.Null);
            Assert.That(player.GetComponent<PlayerExperienceCollector>(), Is.Not.Null);
            Assert.That(player.GetComponent<HealthComponent>(), Is.Not.Null);
            var interactor = new SerializedObject(player.GetComponent<PlayerChestInteractor>());
            Assert.That(interactor.FindProperty("_interactionLayers").intValue & (1 << 2), Is.Zero);
        }

        [Test]
        public void EnemyPrefab_ReferencesPickupAndDefinitionAuthors25XP()
        {
            GameObject enemy = AssetDatabase.LoadAssetAtPath<GameObject>(
                "Assets/_Project/Prefabs/Enemies/Enemy_ChaserBasic.prefab");
            var drop = new SerializedObject(enemy.GetComponent<EnemyExperienceDrop>());
            Assert.That(drop.FindProperty("_pickupPrefab").objectReferenceValue,
                Is.EqualTo(AssetDatabase.LoadAssetAtPath<GameObject>(PickupPath).GetComponent<ExperiencePickup>()));
            Assert.That(AssetDatabase.LoadAssetAtPath<EnemyDefinition>(
                "Assets/_Project/Data/Enemies/ED_ChaserBasic.asset").ExperienceReward, Is.EqualTo(25));
        }

        [Test]
        public void TestWaves_BindsOneRunAndIndicatorToTheSamePlayer()
        {
            Scene scene = EditorSceneManager.OpenScene(
                "Assets/_Project/Scenes/Tests/Test_Waves.unity", OpenSceneMode.Additive);
            try
            {
                PlayerExperienceController player = FindSingle<PlayerExperienceController>(scene);
                var bootstrap = new SerializedObject(FindSingle<ExperienceRunBootstrap>(scene));
                Assert.That(bootstrap.FindProperty("_playerExperience").objectReferenceValue, Is.EqualTo(player));
                var definition = (ExperienceDefinition)bootstrap.FindProperty("_definition").objectReferenceValue;
                Assert.That(definition, Is.Not.Null);
                Assert.That(definition.CreateRuntimeCurve().GetRequiredExperience(1), Is.EqualTo(100));
                Assert.That(definition.CreateRuntimeCurve().GetRequiredExperience(2), Is.EqualTo(150));
                var presenter = new SerializedObject(FindSingle<ExperienceDebugPresenter>(scene));
                Assert.That(presenter.FindProperty("_playerExperience").objectReferenceValue, Is.EqualTo(player));
            }
            finally
            {
                EditorSceneManager.CloseScene(scene, true);
            }
        }

        private static T FindSingle<T>(Scene scene) where T : Component =>
            scene.GetRootGameObjects().SelectMany(root => root.GetComponentsInChildren<T>(true)).Single();
    }
}
#endif
