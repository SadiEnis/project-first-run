using NUnit.Framework;
using System.Linq;
using UnityEditor;
using ProjectFirstRun.Arenas;
using ProjectFirstRun.Development.Arenas;
using ProjectFirstRun.Chests.Spawning;
using ProjectFirstRun.Enemies;
using ProjectFirstRun.Progression;
using ProjectFirstRun.Waves;
using Unity.AI.Navigation;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace ProjectFirstRun.Tests.EditMode.Arenas
{
    public sealed class PlayableMapFixtureLayoutTests
    {
        [Test]
        public void FixtureRegionsHaveMainSideRoutesAndSecondMain()
        {
            var session = new MapTraversalSession(PlayableMapFixtureBootstrap.Main, new[]
            {
                PlayableMapFixtureBootstrap.Main,
                PlayableMapFixtureBootstrap.SideNorth,
                PlayableMapFixtureBootstrap.SideSouth,
                PlayableMapFixtureBootstrap.SecondMain
            });

            Assert.That(session.ContainsRegion(PlayableMapFixtureBootstrap.Main), Is.True);
            Assert.That(session.ContainsRegion(PlayableMapFixtureBootstrap.SideNorth), Is.True);
            Assert.That(session.ContainsRegion(PlayableMapFixtureBootstrap.SideSouth), Is.True);
            Assert.That(session.ContainsRegion(PlayableMapFixtureBootstrap.SecondMain), Is.True);
            Assert.That(session.CurrentRegionId, Is.EqualTo(PlayableMapFixtureBootstrap.Main));
        }

        [Test]
        public void ReturnableSideRouteAllowsReturningButOneWayRouteDoesNot()
        {
            var session = new MapTraversalSession(PlayableMapFixtureBootstrap.Main, new[]
            {
                PlayableMapFixtureBootstrap.Main,
                PlayableMapFixtureBootstrap.SideNorth,
                PlayableMapFixtureBootstrap.SecondMain
            });
            var side = new RegionTransition(PlayableMapFixtureBootstrap.Main, PlayableMapFixtureBootstrap.SideNorth,
                RegionTransitionRequirement.Free, RegionTransitionTraversal.Walk,
                RegionTransitionDirection.Returnable);
            var oneWay = new RegionTransition(PlayableMapFixtureBootstrap.Main, PlayableMapFixtureBootstrap.SecondMain,
                RegionTransitionRequirement.Free, RegionTransitionTraversal.Walk,
                RegionTransitionDirection.OneWay);

            Assert.That(session.TryBegin(side, false, true, out var sideAttempt), Is.True);
            Assert.That(session.Complete(side, sideAttempt), Is.True);
            Assert.That(session.CurrentRegionId, Is.EqualTo(PlayableMapFixtureBootstrap.SideNorth));
            Assert.That(side.CanBegin(session.CurrentRegionId, false, true), Is.True);
            Assert.That(oneWay.CanBegin(PlayableMapFixtureBootstrap.SecondMain, false, true), Is.False);
        }

        [Test]
        public void FixtureTargetIsEditorLoadableBeforeBuildProfileRegistration()
        {
            Assert.That(new UnityMapSceneLoader().CanLoad(PlayableMapFixtureBootstrap.TargetScene), Is.True);
        }

        [Test]
        public void AuthoredFixtureSerializesItsTraversalReferences()
        {
            var scene = EditorSceneManager.OpenScene(
                "Assets/_Project/Scenes/Playable/PlayableMapFixture.unity", OpenSceneMode.Additive);
            try
            {
                var root = scene.GetRootGameObjects().SelectMany(x => x.GetComponentsInChildren<MapSceneRoot>(true)).ToArray();
                Assert.That(root, Has.Length.EqualTo(1));
                Assert.That(root[0].Content, Is.Not.Null);
                Assert.That(root[0].Map, Is.Not.Null);
                Assert.That(root[0].Content.activeSelf, Is.True);
                Assert.That(root[0].Content.GetComponentsInChildren<RegionPassageController>(true), Has.Length.EqualTo(3));
                var encounter = root[0].Content.GetComponentInChildren<PreparedRegionEncounter>(true);
                Assert.That(encounter, Is.Not.Null);
                var encounterSerialized = new SerializedObject(encounter);
                var group = encounterSerialized.FindProperty("_group").objectReferenceValue;
                Assert.That(group, Is.Not.Null);
                Assert.That(group.name, Is.EqualTo("EW_PlayableFixture"));
                var encounters = root[0].Content.GetComponentsInChildren<PreparedRegionEncounter>(true);
                Assert.That(encounters, Has.Length.EqualTo(3));
                foreach (var item in encounters)
                {
                    var serialized = new SerializedObject(item);
                    var wave = (EnemyWaveDefinition)serialized.FindProperty("_group").objectReferenceValue;
                    Assert.That(wave.TotalEnemyCount, Is.EqualTo(4));
                    Assert.That(serialized.FindProperty("_spawnPoints").arraySize, Is.EqualTo(4));
                    foreach (string field in new[] { "_player", "_playerHealth", "_registry", "_spawner" })
                        Assert.That(serialized.FindProperty(field).objectReferenceValue, Is.Not.Null, field);
                }
                Assert.That(root[0].Content.GetComponentInChildren<NavMeshSurface>().navMeshData, Is.Not.Null);
                Assert.That(root[0].Content.GetComponentInChildren<ExperienceRunBootstrap>(), Is.Not.Null);
                Assert.That(root[0].Content.GetComponentInChildren<LevelUpChestSource>(), Is.Not.Null);
                Assert.That(scene.GetRootGameObjects(), Has.Length.EqualTo(3));
            }
            finally
            {
                EditorSceneManager.CloseScene(scene, true);
            }
        }

        [Test]
        public void DestinationHasGroundNavigationRegistryAndRewardServicesWithoutAnotherPlayer()
        {
            var scene = EditorSceneManager.OpenScene(PlayableMapFixtureBootstrap.TargetScene, OpenSceneMode.Additive);
            try
            {
                var root = MapSceneRoot.FindIn(scene);
                Assert.That(root.Content.activeSelf, Is.False);
                Assert.That(root.ValidateDestination("arrival"), Is.Not.Null);
                Assert.That(root.Content.GetComponentsInChildren<EnemyRegistry>(true), Has.Length.EqualTo(1));
                Assert.That(root.Content.GetComponentInChildren<NavMeshSurface>(true).navMeshData, Is.Not.Null);
                Assert.That(root.Content.GetComponentInChildren<LevelUpChestSource>(true), Is.Not.Null);
                Assert.That(root.Content.GetComponentInChildren<ExperienceRunBootstrap>(true), Is.Null);
            }
            finally { EditorSceneManager.CloseScene(scene, true); }
        }
    }
}
