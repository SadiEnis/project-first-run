using System;
using System.Collections;
using ProjectFirstRun.Arenas;
using ProjectFirstRun.Builds;
using ProjectFirstRun.Chests.Spawning;
using ProjectFirstRun.Combat;
using ProjectFirstRun.Player;
using ProjectFirstRun.Progression;
using ProjectFirstRun.Weapons;
using UnityEngine;
using UnityEngine.AI;

namespace ProjectFirstRun.Development.Arenas
{
    /// <summary>Opt-in standalone verification. Does nothing in ordinary play or without --validate-map.</summary>
    public sealed class PlayableMapBuildSmokeCheck : MonoBehaviour
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void Install()
        {
            if (Application.isEditor || Array.IndexOf(Environment.GetCommandLineArgs(), "--validate-map") < 0) return;
            var go = new GameObject("Standalone map verification");
            DontDestroyOnLoad(go);
            go.AddComponent<PlayableMapBuildSmokeCheck>();
        }

        private IEnumerator Start()
        {
            var run = Verify();
            float deadline = Time.realtimeSinceStartup + 90;
            while (true)
            {
                object next;
                try
                {
                    Require(Time.realtimeSinceStartup < deadline, "Verification timed out.");
                    if (!run.MoveNext()) break;
                    next = run.Current;
                }
                catch (Exception error)
                {
                    Debug.LogError("STAGE10_PLAYER_FAIL " + error);
                    Application.Quit(1);
                    yield break;
                }
                yield return next;
            }
            Debug.Log("STAGE10_PLAYER_PASS");
            Application.Quit(0);
        }

        private IEnumerator Verify()
        {
            for (int i = 0; i < 10; i++) yield return null;
            var fixture = FindFirstObjectByType<PlayableMapFixtureBootstrap>();
            Require(fixture != null, "Source fixture missing.");
            var travel = FindFirstObjectByType<SceneTravelController>();
            var player = travel.gameObject;
            player.GetComponent<PlayerController>().enabled = false;
            var motor = player.GetComponent<PlayerMotor>();
            var health = player.GetComponent<HealthComponent>();
            health.ApplyDamage(new DamageInfo(17, null, player.transform.position, Vector3.forward));
            var xp = player.GetComponent<PlayerExperienceController>();
            xp.GainExperience(25);
            var build = player.GetComponent<PlayerBuildController>().Build;
            var weapon = player.GetComponent<PlayerWeaponController>().ActiveEntry;
            weapon.RuntimeState.TryFire();
            int ammo = weapon.RuntimeState.MagazineAmmo;
            int identity = player.GetInstanceID();
            motor.Teleport(new Vector3(8, .1f, 0), Quaternion.identity);
            Physics.SyncTransforms();
            var encounter = fixture.Encounter;
            while (!encounter.IsReadyForPassage)
            {
                Require(encounter.LastError == null, encounter.LastError?.ToString());
                yield return null;
            }
            Require(encounter.Enemies.Count == 4, "Prepared group count differs.");
            foreach (var enemy in encounter.Enemies)
                Require(enemy.GetComponent<NavMeshAgent>().isOnNavMesh, "Enemy is off the baked NavMesh.");
            Debug.Log($"STAGE10_TIMING preparation_max_ms={encounter.MaxPreparationStepMilliseconds:F4}");
            for (int i = 0; i < 48; i++)
            {
                player.GetComponent<CharacterController>().Move(new Vector3(.25f, 0, 0));
                yield return new WaitForFixedUpdate();
            }
            Require(fixture.Map.Session.CurrentRegionId == "second-main", "Walking passage did not complete.");
            motor.Teleport(new Vector3(27, .1f, 6), Quaternion.identity);
            Physics.SyncTransforms();
            yield return new WaitForFixedUpdate();
            yield return null;
            Require(encounter.Status == ArenaSessionStatus.Running, "Activation did not begin combat.");
            Debug.Log($"STAGE10_TIMING activation_ms={encounter.LastActivationMilliseconds:F4}");
            motor.Teleport(new Vector3(38, .1f, 7), Quaternion.identity);
            Physics.SyncTransforms();
            float loadStart = Time.realtimeSinceStartup;
            while (travel.Status != SceneTravelStatus.Succeeded)
            {
                Require(travel.LastError == null, travel.LastError);
                yield return null;
            }
            Require(player.GetInstanceID() == identity, "Player was replaced.");
            Require(player.GetComponent<PlayerBuildController>().Build == build, "Build was replaced.");
            Require(weapon == player.GetComponent<PlayerWeaponController>().ActiveEntry, "Weapon state was replaced.");
            Require(weapon.RuntimeState.MagazineAmmo == ammo && xp.TotalExperience == 25, "Ammo/XP was reset.");
            Require(Mathf.Approximately(health.CurrentHealth, 83), "Health was reset.");
            Require(Physics.Raycast(player.transform.position + Vector3.up, Vector3.down, 3, 1 << 7), "Arrival floor missing.");
            var chests = travel.CurrentMap.Content.GetComponentInChildren<LevelUpChestSource>();
            Require(chests.SpawnedChestCount == 0, "Arrival generated an unearned reward.");
            xp.GainExperience(75);
            while (chests.SpawnedChestCount == 0) yield return null;
            Debug.Log($"STAGE10_TIMING travel_and_reward_ms={(Time.realtimeSinceStartup - loadStart) * 1000:F2}");
        }

        private static void Require(bool condition, string message)
        {
            if (!condition) throw new InvalidOperationException(message);
        }
    }
}
