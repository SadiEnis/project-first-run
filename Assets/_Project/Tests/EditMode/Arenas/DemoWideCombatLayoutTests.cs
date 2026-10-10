#if UNITY_EDITOR
using System.Linq;
using NUnit.Framework;
using ProjectFirstRun.Arenas;
using ProjectFirstRun.Development.Arenas;
using ProjectFirstRun.Editor;
using ProjectFirstRun.Waves;
using Unity.AI.Navigation;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.SceneManagement;

namespace ProjectFirstRun.Tests.EditMode.Arenas
{
    public sealed class DemoWideCombatLayoutTests
    {
        private Scene _source, _wide;

        [SetUp]
        public void Open()
        {
            _source = EditorSceneManager.OpenScene(DeepJamOpeningSceneBuilder.ScenePath, OpenSceneMode.Additive);
            _wide = EditorSceneManager.OpenScene(DeepJamOpeningSceneBuilder.WideScenePath, OpenSceneMode.Additive);
            Physics.SyncTransforms();
        }

        [TearDown]
        public void Close()
        {
            if (_wide.IsValid() && _wide.isLoaded) EditorSceneManager.CloseScene(_wide, true);
            if (_source.IsValid() && _source.isLoaded) EditorSceneManager.CloseScene(_source, true);
        }

        private static T[] Components<T>(Scene scene) where T : Component => scene.GetRootGameObjects()
            .SelectMany(x => x.GetComponentsInChildren<T>(true)).ToArray();
        private static Transform Named(Scene scene, string name) => Components<Transform>(scene).Single(x => x.name == name);

        [Test]
        public void LoweredArenaKeepsUserDimensionsAndGameplayReferences()
        {
            var arena = Named(_wide, "Large arena floor");
            Assert.That(arena.localScale.x, Is.EqualTo(48).Within(.001f));
            Assert.That(arena.localScale.z, Is.EqualTo(71.3016f).Within(.001f));
            float ground = arena.GetComponent<BoxCollider>().bounds.max.y;
            Assert.That(ground, Is.EqualTo(-19.9f).Within(.001f));
            foreach (string floor in new[] { "Optional room floor", "Key route landing", "Key room floor",
                         "Return bridge", "Return arena connector", "Final entry floor" })
                Assert.That(Named(_wide, floor).GetComponent<BoxCollider>().bounds.max.y,
                    Is.EqualTo(ground).Within(.001f), floor);
            var second = Named(_wide, "Second room floor");
            Assert.That(second.localScale.x, Is.EqualTo(23.5f).Within(.001f));
            Assert.That(second.localScale.z, Is.EqualTo(24).Within(.001f));
            Assert.That(Components<PreparedRegionEncounter>(_wide).Length, Is.EqualTo(Components<PreparedRegionEncounter>(_source).Length));
            foreach (var component in Components<DemoFinalController>(_wide)) Assert.DoesNotThrow(component.ValidateConfiguration);
            foreach (var component in Components<DemoKeyAmbushController>(_wide)) Assert.DoesNotThrow(component.ValidateConfiguration);
            var sourceGroups = Components<PreparedRegionEncounter>(_source).OrderBy(x => x.name).ToArray();
            var wideGroups = Components<PreparedRegionEncounter>(_wide).OrderBy(x => x.name).ToArray();
            for (int i = 0; i < sourceGroups.Length; i++)
            {
                if (new[] { "Encounter - SecondRoom", "Encounter - ArenaLeft", "Encounter - ArenaRight" }.Contains(wideGroups[i].name))
                    continue; // These three encounters intentionally use WideCombat-only density assets.
                Assert.That(new SerializedObject(wideGroups[i]).FindProperty("_group").objectReferenceValue,
                    Is.EqualTo(new SerializedObject(sourceGroups[i]).FindProperty("_group").objectReferenceValue));
            }
        }

        [TestCase("Encounter - SecondRoom", "EW_WideSecondRoom", 6, 4, 2, 0)]
        [TestCase("Encounter - ArenaLeft", "EW_WideArenaLeft", 15, 15, 0, 0)]
        [TestCase("Encounter - ArenaRight", "EW_WideArenaRight", 9, 0, 6, 3)]
        public void WideDensityIsIsolatedAndHasDistinctSpawnPoints(string name, string asset,
            int total, int chasers, int rangers, int chargers)
        {
            var group = Named(_wide, name).GetComponent<PreparedRegionEncounter>();
            var properties = new SerializedObject(group);
            var wave = (EnemyWaveDefinition)properties.FindProperty("_group").objectReferenceValue;
            Assert.That(AssetDatabase.GetAssetPath(wave),
                Is.EqualTo(DeepJamOpeningSceneBuilder.Folder + "/" + asset + ".asset"));
            Assert.That(wave.TotalEnemyCount, Is.EqualTo(total));
            var source = new SerializedObject(Named(_source, name).GetComponent<PreparedRegionEncounter>());
            var original = (EnemyWaveDefinition)source.FindProperty("_group").objectReferenceValue;
            Assert.That(wave, Is.Not.SameAs(original));
            Assert.That(original.TotalEnemyCount, Is.EqualTo(3));
            Assert.That(wave.Entries.Where(x => x.EnemyDefinition.name == "ED_ChaserChestDropTest").Sum(x => x.Count), Is.EqualTo(chasers));
            Assert.That(wave.Entries.Where(x => x.EnemyDefinition.name == "ED_Ranger").Sum(x => x.Count), Is.EqualTo(rangers));
            Assert.That(wave.Entries.Where(x => x.EnemyDefinition.name == "ED_Charger").Sum(x => x.Count), Is.EqualTo(chargers));
            var points = properties.FindProperty("_spawnPoints");
            Assert.That(points.arraySize, Is.EqualTo(total), "Do not reuse a spawn position for simultaneous enemies.");
            var positions = Enumerable.Range(0, total)
                .Select(i => ((Transform)points.GetArrayElementAtIndex(i).objectReferenceValue).position).ToArray();
            for (int i = 0; i < total; i++)
                for (int j = i + 1; j < total; j++)
                    Assert.That(Vector3.Distance(positions[i], positions[j]), Is.GreaterThan(2f));
        }

        [Test]
        public void FrontFloorHasSixAssignedChasersBetweenTheStairs()
        {
            var group = Named(_wide, "Encounter - ArenaLeft").GetComponent<PreparedRegionEncounter>();
            var serialized = new SerializedObject(group);
            var wave = (EnemyWaveDefinition)serialized.FindProperty("_group").objectReferenceValue;
            Assert.That(wave.EntryCount, Is.EqualTo(1));
            Assert.That(wave.Entries[0].EnemyDefinition.name, Is.EqualTo("ED_ChaserChestDropTest"));
            var points = serialized.FindProperty("_spawnPoints");
            var front = Enumerable.Range(0, points.arraySize)
                .Select(i => (Transform)points.GetArrayElementAtIndex(i).objectReferenceValue)
                .Where(x => x.name.StartsWith("Front floor spawn ")).ToArray();
            Assert.That(front.Length, Is.EqualTo(6));
            float ground = Named(_wide, "Large arena floor").GetComponent<BoxCollider>().bounds.max.y;
            foreach (var point in front)
            {
                Assert.That(point.position.y, Is.EqualTo(ground).Within(.001f));
                Assert.That(Mathf.Abs(point.position.x), Is.LessThan(14));
                Assert.That(point.position.z, Is.InRange(105, 135));
            }
        }

        [Test]
        public void ParkourIsTranslatedWithoutChangingJumpGapsOrRecoveryShape()
        {
            var source = Named(_source, "Demo parkour");
            var wide = Named(_wide, "Demo parkour");
            Vector3 offset = wide.Find("Parkour return point").position - source.Find("Parkour return point").position;
            foreach (Transform original in source)
            {
                var copy = wide.Find(original.name);
                Assert.That(copy, Is.Not.Null, original.name);
                Assert.That(Vector3.Distance(copy.position - original.position, offset), Is.LessThan(.001f));
                Assert.That(Vector3.Distance(copy.localScale, original.localScale), Is.LessThan(.001f));
            }
        }

        [Test]
        public void BothStairwaysReachTheLowerFloorWithoutOversizedRisersOrGaps()
        {
            float start = Named(_wide, "Overlook floor").GetComponent<BoxCollider>().bounds.max.y;
            var arena = Named(_wide, "Large arena floor").GetComponent<BoxCollider>().bounds;
            float stepLimit = Named(_wide, "Player (demo)").GetComponent<CharacterController>().stepOffset;
            foreach (string stairs in new[] { "05 - Arena left stairs", "06 - Arena right stairs" })
            {
                var steps = Named(_wide, stairs).Cast<Transform>().Where(x => x.name.Contains("step"))
                    .OrderBy(x => x.position.z).Select(x => x.GetComponent<BoxCollider>().bounds).ToArray();
                Assert.That(steps.Length, Is.EqualTo(68));
                float previousTop = start;
                float previousEnd = Named(_wide, "Overlook floor").GetComponent<BoxCollider>().bounds.max.z;
                foreach (var step in steps)
                {
                    Assert.That(previousTop - step.max.y, Is.InRange(.01f, stepLimit));
                    Assert.That(step.min.z, Is.EqualTo(previousEnd).Within(.002f));
                    previousTop = step.max.y;
                    previousEnd = step.max.z;
                }
                Assert.That(previousTop, Is.EqualTo(arena.max.y).Within(.002f));
                Assert.That(previousEnd, Is.LessThan(Named(_wide, "Optional connector floor").position.z - 2));
            }
        }

        [Test]
        public void RewardAndFinaleTargetsFollowTheirEditedRooms()
        {
            var rewardFloor = Named(_wide, "First reward room").GetComponent<BoxCollider>().bounds;
            Vector3 reward = Named(_wide, "Guaranteed ability chest spawn").position;
            Assert.That(reward.x, Is.InRange(rewardFloor.min.x + 1, rewardFloor.max.x - 1));
            Assert.That(reward.z, Is.InRange(rewardFloor.min.z + 1, rewardFloor.max.z - 1));
            var final = Components<DemoFinalController>(_wide).Single();
            var entry = (BoxCollider)new SerializedObject(final).FindProperty("_entryVolume").objectReferenceValue;
            var floor = Named(_wide, "Final entry floor").GetComponent<BoxCollider>().bounds;
            Assert.That(entry.bounds.min.z, Is.GreaterThan(floor.min.z));
            Assert.That(entry.bounds.max.z, Is.LessThan(floor.max.z));
            Assert.That(entry.bounds.min.y, Is.EqualTo(floor.max.y).Within(.002f));
            Assert.That(Named(_wide, "Giant focus").position.z,
                Is.EqualTo(Named(_wide, "Giant torso").position.z).Within(.001f));
            Assert.That(Named(_wide, "Final gate - key and ambush required").position.z,
                Is.EqualTo(Named(_wide, "Large arena floor").GetComponent<BoxCollider>().bounds.max.z).Within(.001f));
        }

        [Test]
        public void StaggeredScreensHideAtLeastOneSpawnInEachLaneFromBalcony()
        {
            Vector3 eye = Named(_wide, "Overlook floor").position + new Vector3(0, 2.1f, 0);
            var screens = new[] { Named(_wide, "Protected approach pillar").GetComponent<BoxCollider>(),
                Named(_wide, "Central sight breaker").GetComponent<BoxCollider>(),
                Named(_wide, "Far right cover").GetComponent<BoxCollider>() };
            foreach (string lane in new[] { "Encounter - ArenaLeft", "Encounter - ArenaRight" })
            {
                var group = Named(_wide, lane).GetComponent<PreparedRegionEncounter>();
                var points = new SerializedObject(group).FindProperty("_spawnPoints");
                int hidden = 0;
                for (int i = 0; i < points.arraySize; i++)
                {
                    var point = (Transform)points.GetArrayElementAtIndex(i).objectReferenceValue;
                    Vector3 delta = point.position + Vector3.up - eye;
                    if (screens.Any(x => x.bounds.IntersectRay(new Ray(eye, delta.normalized), out float d) && d < delta.magnitude)) hidden++;
                }
                Assert.That(hidden, Is.GreaterThan(0), lane);
            }
        }

        [Test]
        public void WideNavigationHasItsOwnSavedAssetAfterBake()
        {
            var source = Components<NavMeshSurface>(_source).Single();
            var wide = Components<NavMeshSurface>(_wide).Single();
            Assert.That(wide.navMeshData, Is.Not.Null, "Open Wide Combat Greybox, then Rebake Demo Navigation and save.");
            Assert.That(wide.navMeshData, Is.Not.SameAs(source.navMeshData));
            Assert.That(AssetDatabase.GetAssetPath(wide.navMeshData), Is.EqualTo(DeepJamOpeningSceneBuilder.WideNavigationPath));
        }

        [Test]
        public void RebakedNavigationSupportsSpawnsAndBothStairRoutes()
        {
            // Do not let navigation from the accepted demo mask a stale alternative bake.
            var source = Components<NavMeshSurface>(_source).Single();
            source.RemoveData();
            try
            {
                const string hint = "Open Wide Combat Greybox, Rebake Demo Navigation, then save.";
                foreach (var group in Components<PreparedRegionEncounter>(_wide))
                {
                    var points = new SerializedObject(group).FindProperty("_spawnPoints");
                    for (int i = 0; i < points.arraySize; i++)
                    {
                        var point = (Transform)points.GetArrayElementAtIndex(i).objectReferenceValue;
                        Assert.That(NavMesh.SamplePosition(point.position, out _, .4f, NavMesh.AllAreas),
                            Is.True, group.name + "/" + point.name + ": " + hint);
                    }
                }
                Assert.That(NavMesh.SamplePosition(Named(_wide, "Overlook").position, out var origin,
                    .4f, NavMesh.AllAreas), Is.True, hint);
                foreach (string name in new[] { "LeftStairBottom", "RightStairBottom", "OptionalRoom", "KeyLanding" })
                {
                    Assert.That(NavMesh.SamplePosition(Named(_wide, name).position, out var destination,
                        .4f, NavMesh.AllAreas), Is.True, name + ": " + hint);
                    var path = new NavMeshPath();
                    Assert.That(NavMesh.CalculatePath(origin.position, destination.position, NavMesh.AllAreas, path), Is.True);
                    Assert.That(path.status, Is.EqualTo(NavMeshPathStatus.PathComplete), name + ": " + hint);
                }
            }
            finally { source.AddData(); }
        }
    }
}
#endif
