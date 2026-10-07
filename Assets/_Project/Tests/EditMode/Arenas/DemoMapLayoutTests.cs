using System.Linq;
using NUnit.Framework;
using ProjectFirstRun.Chests;
using ProjectFirstRun.Arenas;
using ProjectFirstRun.Enemies;
using ProjectFirstRun.Waves;
using ProjectFirstRun.Development.Arenas;
using ProjectFirstRun.Editor;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.SceneManagement;

namespace ProjectFirstRun.Tests.EditMode.Arenas
{
    public sealed class DemoMapLayoutTests
    {
        private Scene _scene;
        private Transform _geometry;
        private DemoOpeningController _demo;

        [SetUp]
        public void Open()
        {
            _scene = EditorSceneManager.OpenScene(DeepJamOpeningSceneBuilder.ScenePath, OpenSceneMode.Additive);
            _geometry = _scene.GetRootGameObjects().Single(x => x.name == "Authored demo geometry").transform;
            _demo = _scene.GetRootGameObjects().SelectMany(x => x.GetComponentsInChildren<DemoOpeningController>()).Single();
        }

        [TearDown]
        public void Close()
        {
            if (_scene.IsValid() && _scene.isLoaded) EditorSceneManager.CloseScene(_scene, true);
        }

        [Test]
        public void SavedExpansionPreservesOpeningAndHasDistinctRouteEntrances()
        {
            Assert.DoesNotThrow(_demo.ValidateConfiguration);
            Assert.That(_geometry.Find("Start room"), Is.Not.Null);
            Assert.That(_geometry.Find("Combat corridor"), Is.Not.Null);
            foreach (string section in new[] {
                "01 - Descent to reward room", "02 - Reward room continuation",
                "03 - Second room", "04 - Arena overlook",
                "05 - Arena left stairs", "06 - Arena right stairs",
                "07 - Large arena", "08 - Optional room",
                "09 - Key route entrance - unfinished", "10 - Final entrance - unfinished" })
                Assert.That(_geometry.Find(section), Is.Not.Null, section);
            Assert.That(_geometry.Find("Traversal checkpoints").childCount, Is.EqualTo(12));
            var encounters = _scene.GetRootGameObjects().SelectMany(x =>
                x.GetComponentsInChildren<ProjectFirstRun.Arenas.PreparedRegionEncounter>(true));
            Assert.That(encounters.Count(), Is.EqualTo(5), "Opening plus four independent room groups.");
            foreach (var box in _geometry.GetComponentsInChildren<BoxCollider>())
                if (box.name.StartsWith("Temporary "))
                {
                    Assert.That(box.enabled && !box.isTrigger, Is.True);
                    Assert.That(box.GetComponent<MeshRenderer>().enabled, Is.True);
                }
        }

        [Test]
        public void RoomGroupsHaveExpectedCompositionNavigationAndExplicitDependencies()
        {
            var rooms = _scene.GetRootGameObjects().Single(x => x.name == "Demo room encounters");
            var encounters = rooms.GetComponentsInChildren<PreparedRegionEncounter>();
            Assert.That(encounters.Length, Is.EqualTo(4));
            var expected = new[] {
                (Id: "demo.second", Chasers: 2, Rangers: 1, Chargers: 0),
                (Id: "demo.arena.left", Chasers: 3, Rangers: 0, Chargers: 0),
                (Id: "demo.arena.right", Chasers: 0, Rangers: 2, Chargers: 1),
                (Id: "demo.optional", Chasers: 2, Rangers: 0, Chargers: 1) };
            foreach (var spec in expected)
            {
                var encounter = encounters.Single(x => new SerializedObject(x).FindProperty("_regionId").stringValue == spec.Id);
                var properties = new SerializedObject(encounter);
                foreach (string field in new[] { "_group", "_spawner", "_registry", "_player", "_playerHealth" })
                    Assert.That(properties.FindProperty(field).objectReferenceValue, Is.Not.Null, spec.Id + field);
                var group = (EnemyWaveDefinition)properties.FindProperty("_group").objectReferenceValue;
                Assert.DoesNotThrow(group.Validate);
                Assert.That(group.TotalEnemyCount, Is.EqualTo(3));
                int Count(EnemyBehavior type) => group.Entries.Where(x => x.EnemyDefinition.Behavior == type).Sum(x => x.Count);
                Assert.That(Count(EnemyBehavior.Chaser), Is.EqualTo(spec.Chasers));
                Assert.That(Count(EnemyBehavior.Ranger), Is.EqualTo(spec.Rangers));
                Assert.That(Count(EnemyBehavior.Charger), Is.EqualTo(spec.Chargers));
                var spawner = new SerializedObject(properties.FindProperty("_spawner").objectReferenceValue);
                Assert.That(spawner.FindProperty("_perceptionProfile").objectReferenceValue, Is.Not.Null);
                var points = properties.FindProperty("_spawnPoints");
                Assert.That(points.arraySize, Is.EqualTo(group.TotalEnemyCount));
                Assert.That(NavMesh.SamplePosition(new Vector3(0, -3, 54), out var origin, .6f, NavMesh.AllAreas), Is.True);
                for (int i = 0; i < points.arraySize; i++)
                {
                    var point = (Transform)points.GetArrayElementAtIndex(i).objectReferenceValue;
                    Assert.That(point, Is.Not.Null);
                    Assert.That(NavMesh.SamplePosition(point.position, out var hit, .6f, NavMesh.AllAreas), Is.True,
                        spec.Id + " spawn " + i);
                    var path = new NavMeshPath();
                    Assert.That(NavMesh.CalculatePath(origin.position, hit.position, NavMesh.AllAreas, path), Is.True);
                    Assert.That(path.status, Is.EqualTo(NavMeshPathStatus.PathComplete));
                }
                var triggers = encounter.GetComponentsInChildren<RegionPreparationTrigger>();
                Assert.That(triggers.Length, Is.EqualTo(2));
                foreach (var trigger in triggers)
                {
                    var triggerProperties = new SerializedObject(trigger);
                    Assert.That(triggerProperties.FindProperty("_encounter").objectReferenceValue, Is.SameAs(encounter));
                    Assert.That(triggerProperties.FindProperty("_player").objectReferenceValue, Is.Not.Null);
                    var volume = (Collider)triggerProperties.FindProperty("_volume").objectReferenceValue;
                    Assert.That(volume != null && volume.isTrigger && volume.enabled, Is.True);
                }
                var prepare = encounter.transform.Find("Prepare on descent").GetComponent<BoxCollider>();
                var activate = encounter.transform.Find("Activate before sightline").GetComponent<BoxCollider>();
                Assert.That(prepare.bounds.min.z, Is.LessThan(activate.bounds.min.z));
                Assert.That(activate.bounds.min.z, Is.LessThan(57), "Activate before the overlook or second-room firing line.");
            }
            var reward = rooms.GetComponentsInChildren<EncounterChestReward>().Single();
            Assert.DoesNotThrow(reward.ValidateConfiguration);
            var rewardProperties = new SerializedObject(reward);
            var definition = (ChestDefinition)rewardProperties.FindProperty("_definition").objectReferenceValue;
            Assert.That(definition.StableId, Is.EqualTo("chest.advanced.green"));
            var anchor = (Transform)rewardProperties.FindProperty("_anchor").objectReferenceValue;
            var collider = definition.WorldPrefab.GetComponent<BoxCollider>();
            float bottom = anchor.position.y + (collider.center.y - collider.size.y * .5f) * collider.transform.lossyScale.y;
            Assert.That(bottom, Is.EqualTo(-6.98f).Within(.005f));
            Assert.That(new SerializedObject(reward.Encounter).FindProperty("_regionId").stringValue, Is.EqualTo("demo.optional"));
        }

        [Test]
        public void LoweredRewardStaysAboveTheFloorAndStairsFitPlayerStepOffset()
        {
            var floor = _geometry.Find("First reward room");
            float top = floor.position.y + floor.lossyScale.y * .5f;
            Assert.That(top, Is.EqualTo(-3).Within(.001f));
            var serialized = new SerializedObject(_demo);
            var anchor = (Transform)serialized.FindProperty("_rewardPoint").objectReferenceValue;
            var definition = (ChestDefinition)serialized.FindProperty("_firstReward").objectReferenceValue;
            var chest = definition.WorldPrefab.GetComponent<BoxCollider>();
            float bottom = anchor.position.y + (chest.center.y - chest.size.y * .5f) * chest.transform.lossyScale.y;
            Assert.That(bottom - top, Is.EqualTo(.02f).Within(.005f));
            float stepLimit = _demo.Health.GetComponent<CharacterController>().stepOffset;
            foreach (var staircase in new[] {
                (Name: "01 - Descent to reward room", Prefix: "Descent step ", Count: 15),
                (Name: "05 - Arena left stairs", Prefix: "Arena left step ", Count: 20),
                (Name: "06 - Arena right stairs", Prefix: "Arena right step ", Count: 20) })
            {
                var steps = _geometry.Find(staircase.Name).Cast<Transform>()
                    .Where(t => t.name.StartsWith(staircase.Prefix)).OrderBy(t => t.name).ToArray();
                Assert.That(steps.Length, Is.EqualTo(staircase.Count));
                for (int i = 0; i < steps.Length; i++)
                {
                    Assert.That(steps[i].gameObject.layer, Is.EqualTo(7));
                    Assert.That(steps[i].lossyScale.x, Is.GreaterThanOrEqualTo(4));
                    if (i == 0) continue;
                    float previous = steps[i-1].position.y + steps[i-1].lossyScale.y / 2;
                    float current = steps[i].position.y + steps[i].lossyScale.y / 2;
                    Assert.That(previous-current, Is.EqualTo(.2f).Within(.001f));
                    Assert.That(previous-current, Is.LessThan(stepLimit));
                }
            }
        }

        [Test]
        public void BakedNavigationReachesNewElevationsAndOptionalRoom()
        {
            const string bakeHint = "Run Project First Run/Demo/Rebake Demo Navigation after scene geometry changes.";
            var points = _geometry.Find("Traversal checkpoints").Cast<Transform>().ToArray();
            var start = points.Single(p => p.name == "DescentTop");
            Assert.That(NavMesh.SamplePosition(start.position, out var origin, .6f, NavMesh.AllAreas), Is.True, bakeHint);
            foreach (var point in points)
            {
                Assert.That(NavMesh.SamplePosition(point.position, out var destination, .6f, NavMesh.AllAreas),
                    Is.True, point.name + ": " + bakeHint);
                var path = new NavMeshPath();
                Assert.That(NavMesh.CalculatePath(origin.position, destination.position, NavMesh.AllAreas, path),
                    Is.True, point.name);
                Assert.That(path.status, Is.EqualTo(NavMeshPathStatus.PathComplete), point.name);
            }
        }
    }
}
