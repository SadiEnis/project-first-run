using System.Linq;
using NUnit.Framework;
using ProjectFirstRun.Editor;
using ProjectFirstRun.Flow;
using ProjectFirstRun.Progression;
using ProjectFirstRun.UI.Hud;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
namespace ProjectFirstRun.Tests.EditMode.Arenas
{
    public sealed class RunHubLayoutTests
    {
        [Test] public void HubUsesSharedRoutesAndContainsNoRunEconomyOrPlayerInventory()
        {
            var routes = AssetDatabase.LoadAssetAtPath<RunSceneRoutes>(RunHubSceneTools.RoutesPath);
            Assert.That(routes, Is.Not.Null); Assert.DoesNotThrow(routes.ValidateConfiguration);
            Assert.That(RunHubSceneTools.BuildScenePaths(), Is.EqualTo(new[] { routes.HubScenePath, routes.RunScenePath }));
            foreach (string path in RunHubSceneTools.BuildScenePaths())
                Assert.That(EditorBuildSettings.scenes.Any(x => x.enabled && x.path == path), Is.True);
            var existing = SceneManager.GetSceneByPath(routes.HubScenePath);
            bool opened = !existing.IsValid() || !existing.isLoaded;
            var scene = opened ? EditorSceneManager.OpenScene(routes.HubScenePath, OpenSceneMode.Additive) : existing;
            try
            {
                var roots = scene.GetRootGameObjects();
                var hub = roots.SelectMany(x => x.GetComponentsInChildren<RunHubController>(true)).Single();
                var view = roots.SelectMany(x => x.GetComponentsInChildren<RunHubView>(true)).Single();
                Assert.DoesNotThrow(hub.ValidateConfiguration);
                Assert.DoesNotThrow(view.ValidateConfiguration);
                var interaction = roots.SelectMany(x => x.GetComponentsInChildren<RunHubInteraction>(true)).Single();
                Assert.DoesNotThrow(interaction.ValidateConfiguration);
                Assert.That(roots.SelectMany(x => x.GetComponentsInChildren<ProjectFirstRun.Player.PlayerController>(true)).Count(), Is.EqualTo(1));
                Assert.That(roots.SelectMany(x => x.GetComponentsInChildren<ProjectFirstRun.Builds.PlayerBuildController>(true)), Is.Empty);
                Assert.That(roots.SelectMany(x => x.GetComponentsInChildren<ProjectFirstRun.Combat.HealthComponent>(true)), Is.Empty);
                var cards = new SerializedObject(view).FindProperty("_cards");
                Assert.That(cards.arraySize, Is.EqualTo(3));
                foreach (var button in roots.SelectMany(x => x.GetComponentsInChildren<UnityEngine.UI.Button>(true)))
                {
                    Assert.That(button.navigation.mode, Is.EqualTo(UnityEngine.UI.Navigation.Mode.Explicit));
                    Assert.That(button.GetComponent<UnityEngine.EventSystems.EventTrigger>(), Is.Not.Null);
                }
                var ammo = AssetDatabase.LoadAssetAtPath<StartingAmmoUpgradeDefinition>(RunHubSceneTools.AmmoUpgradePath);
                Assert.That(ammo, Is.Not.Null);
                Assert.DoesNotThrow(ammo.Validate);
                Assert.That(hub.AmmoUpgrade, Is.SameAs(ammo));
                Assert.That(hub.AbilitySlotUpgrade, Is.SameAs(AssetDatabase.LoadAssetAtPath<SlotCapacityUpgradeDefinition>(RunHubSceneTools.AbilitySlotUpgradePath)));
                Assert.That(hub.UpgradeSlotUpgrade, Is.SameAs(AssetDatabase.LoadAssetAtPath<SlotCapacityUpgradeDefinition>(RunHubSceneTools.UpgradeSlotUpgradePath)));
                Assert.That(new SerializedObject(hub).FindProperty("_routes").objectReferenceValue, Is.SameAs(routes));
                Assert.That(new SerializedObject(view).FindProperty("_hub").objectReferenceValue, Is.SameAs(hub));
                Assert.That(roots.SelectMany(x => x.GetComponentsInChildren<RunEconomyController>(true)), Is.Empty);
                Assert.That(roots.SelectMany(x => x.GetComponentsInChildren<ProjectFirstRun.Weapons.PlayerWeaponController>(true)), Is.Empty);
                Assert.That(roots.SelectMany(x => x.GetComponentsInChildren<Camera>(true)).Count(), Is.EqualTo(1));
            }
            finally { if (opened) EditorSceneManager.CloseScene(scene, true); }
        }
    }
}
