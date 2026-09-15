using NUnit.Framework;
using UnityEditor;
using ProjectFirstRun.Arenas;
using ProjectFirstRun.Development.Arenas;
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
                var root = Object.FindObjectsByType<MapSceneRoot>(FindObjectsSortMode.None);
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
                Assert.That(scene.GetRootGameObjects(), Has.Length.EqualTo(3));
            }
            finally
            {
                EditorSceneManager.CloseScene(scene, true);
            }
        }
    }
}
