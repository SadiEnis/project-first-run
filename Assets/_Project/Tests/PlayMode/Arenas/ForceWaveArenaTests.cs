#if UNITY_EDITOR
using System.Collections;
using System.Linq;
using NUnit.Framework;
using ProjectFirstRun.Abilities;
using ProjectFirstRun.Abilities.ForceWave;
using ProjectFirstRun.Chests;
using ProjectFirstRun.Combat;
using ProjectFirstRun.Development.Arenas;
using ProjectFirstRun.Enemies;
using ProjectFirstRun.Player;
using ProjectFirstRun.Rewards.Claims;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

namespace ProjectFirstRun.Tests.PlayMode.Arenas
{
    public sealed class ForceWaveArenaTests
    {
        Scene scene, original;
        ContentArenaController arena;
        PlayerAbilityController abilities;
        GameObject wall;
        int Index => arena.Items.ToList().FindIndex(x => x.StableId == "ability.force-wave");
        T[] Find<T>() where T : Component => scene.GetRootGameObjects().SelectMany(x => x.GetComponentsInChildren<T>(true)).ToArray();
        [UnitySetUp] public IEnumerator Load()
        {
            Time.timeScale = 1; original = SceneManager.GetActiveScene();
            const string path = "Assets/_Project/Scenes/Tests/Test_ContentArena.unity";
            yield return EditorSceneManager.LoadSceneAsyncInPlayMode(path, new LoadSceneParameters(LoadSceneMode.Additive));
            scene = SceneManager.GetSceneByPath(path); SceneManager.SetActiveScene(scene);
            arena = Find<ContentArenaController>().Single();
            for (int i = 0; i < 200 && !arena.IsReady; i++) yield return null;
            Assert.That(arena.IsReady, Is.True); Assert.That(Index, Is.GreaterThanOrEqualTo(0));
            abilities = arena.Player.GetComponent<PlayerAbilityController>(); abilities.SetAbilityControlEnabled(false);
            arena.Player.GetComponent<PlayerController>().SetControlEnabled(false);
        }
        [UnityTearDown] public IEnumerator Unload()
        {
            Time.timeScale = 1;
            if (wall != null) Object.DestroyImmediate(wall);
            if (original.IsValid() && original.isLoaded) SceneManager.SetActiveScene(original);
            if (scene.IsValid() && scene.isLoaded)
            {
                foreach (var root in scene.GetRootGameObjects()) root.SetActive(false);
                yield return SceneManager.UnloadSceneAsync(scene);
            }
            Cursor.lockState = CursorLockMode.None; Cursor.visible = true;
        }
        EnemyController Prepare()
        {
            var enemy = arena.Enemies.First();
            foreach (var other in arena.Enemies.Where(x => x != enemy)) other.gameObject.SetActive(false);
            enemy.Health.Initialize(1000);
            arena.Player.transform.position = enemy.transform.position - Vector3.forward * 3;
            arena.Player.transform.rotation = Quaternion.identity; Physics.SyncTransforms();
            Assert.That(arena.Acquire(Index), Is.True);
            Assert.That(arena.LastResult, Is.EqualTo("Acquired"));
            return enemy;
        }
        AbilityRuntimeEntry Entry => abilities.Entries.Single(x => x.Definition.StableId == "ability.force-wave");

        [UnityTest] public IEnumerator AcquiredWaveDamagesPushesAndPreservesCooldownAcrossAllLevels()
        {
            var enemy = Prepare(); Vector3 before = enemy.transform.position;
            Entry.TryAutoCast(abilities.AbilityOrigin.position);
            Assert.That(enemy.Health.CurrentHealth, Is.EqualTo(980));
            Assert.That(Vector3.Distance(before, enemy.transform.position), Is.GreaterThan(1.5f));
            Assert.That(Find<ForceWaveVisual>().Length, Is.EqualTo(1));
            float cooldown = Entry.State.CooldownRemaining;
            for (int i = 1; i < 8; i++) Assert.That(arena.LevelUp(Index), Is.True);
            Assert.That(Entry.Level, Is.EqualTo(8));
            Assert.That(Entry.State.CooldownRemaining, Is.EqualTo(cooldown));
            Assert.That(arena.LevelUp(Index), Is.True); Assert.That(arena.LastResult, Is.EqualTo("MaximumLevelReached"));
            Entry.Tick(10);
            arena.Player.transform.position = enemy.transform.position - Vector3.forward * 3; Physics.SyncTransforms();
            Entry.TryAutoCast(abilities.AbilityOrigin.position);
            Assert.That(enemy.Health.CurrentHealth, Is.EqualTo(940));
            Assert.That(Entry.State.CooldownRemaining, Is.EqualTo(1.5f));
            yield return null;
        }
        [UnityTest] public IEnumerator PushStopsBeforeWallWithoutCancellingDamage()
        {
            var enemy = Prepare(); var before = enemy.transform.position;
            wall = GameObject.CreatePrimitive(PrimitiveType.Cube);
            wall.transform.position = before + Vector3.forward * 1.5f + Vector3.up;
            wall.transform.localScale = new Vector3(4, 3, .1f); Physics.SyncTransforms();
            Entry.TryAutoCast(abilities.AbilityOrigin.position);
            Assert.That(enemy.Health.CurrentHealth, Is.EqualTo(980));
            Assert.That(enemy.transform.position.z - before.z, Is.InRange(0, 1.4f));
            yield return null;
        }
        [UnityTest] public IEnumerator AbilityChestClaimsWaveThenExcludesItsMaximumLevel()
        {
            var wave = arena.Items[Index];
            Assert.That(arena.SpawnChest(1), Is.True);
            Find<ChestController>().Last(x => x.gameObject.activeInHierarchy).TryOpen();
            Assert.That(arena.Selection.ActiveSession.Offer.Choices, Has.Member(wave));
            Assert.That(arena.Selection.Select(wave), Is.EqualTo(RewardClaimResult.Claimed));
            Assert.That(arena.ItemLevel(Index), Is.EqualTo(1));
            for (int i = 1; i < 8; i++) arena.LevelUp(Index);
            yield return null;
            Assert.That(arena.SpawnChest(1), Is.True);
            Find<ChestController>().Last(x => x.gameObject.activeInHierarchy).TryOpen();
            Assert.That(arena.Selection.ActiveSession.Offer.Choices, Has.No.Member(wave));
        }
    }
}
#endif
