using System.Linq;
using NUnit.Framework;
using ProjectFirstRun.Chests.Spawning;
using ProjectFirstRun.Development.Arenas;
using ProjectFirstRun.Enemies;
using ProjectFirstRun.Player;
using ProjectFirstRun.Progression;
using Unity.AI.Navigation;
using UnityEditor.SceneManagement;

namespace ProjectFirstRun.Tests.EditMode.Arenas
{
    public sealed class ContentArenaLayoutTests
    {
        [Test]
        public void AuthoredSceneContainsUniqueServicesAndCompleteCatalogs()
        {
            var scene = EditorSceneManager.OpenScene("Assets/_Project/Scenes/Tests/Test_ContentArena.unity", OpenSceneMode.Additive);
            try
            {
                var roots = scene.GetRootGameObjects();
                var arena = roots.SelectMany(x => x.GetComponentsInChildren<ContentArenaController>(true)).Single();
                Assert.DoesNotThrow(arena.ValidateConfiguration);
                Assert.That(roots.SelectMany(x => x.GetComponentsInChildren<PlayerController>(true)).Count(), Is.EqualTo(1));
                Assert.That(roots.SelectMany(x => x.GetComponentsInChildren<EnemyRegistry>(true)).Count(), Is.EqualTo(1));
                Assert.That(roots.SelectMany(x => x.GetComponentsInChildren<ExperienceRunBootstrap>(true)).Count(), Is.EqualTo(1));
                Assert.That(roots.SelectMany(x => x.GetComponentsInChildren<ChestSpawner>(true)).Count(), Is.EqualTo(1));
                Assert.That(roots.SelectMany(x => x.GetComponentsInChildren<EnemyChestDropSource>(true)).Count(), Is.EqualTo(1));
                Assert.That(roots.SelectMany(x => x.GetComponentsInChildren<LevelUpChestSource>(true)).Count(), Is.EqualTo(1));
                Assert.That(roots.SelectMany(x => x.GetComponentsInChildren<NavMeshSurface>(true)).Single().navMeshData, Is.Not.Null);
                Assert.That(arena.EnemyDefinitions.Count, Is.EqualTo(3));
                Assert.That(arena.Items.Count, Is.EqualTo(7));
                Assert.That(arena.Chests.Count, Is.EqualTo(7));
            }
            finally { EditorSceneManager.CloseScene(scene, true); }
        }
    }
}
