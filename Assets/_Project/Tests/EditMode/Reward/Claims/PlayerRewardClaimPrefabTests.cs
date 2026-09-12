#if UNITY_EDITOR

using System.Collections.Generic;
using NUnit.Framework;
using ProjectFirstRun.Builds;
using ProjectFirstRun.Rewards.Claims;
using UnityEditor;
using UnityEngine;

namespace ProjectFirstRun.Tests.EditMode.Rewards.Claims
{
    public sealed class PlayerRewardClaimPrefabTests
    {
        [Test]
        public void PlayerPrefab_ContainsRewardClaimController()
        {
            GameObject playerPrefab =
                FindPlayerPrefab();

            Assert.That(
                playerPrefab.GetComponent<
                    PlayerRewardClaimController>(),
                Is.Not.Null);
        }

        private static GameObject FindPlayerPrefab()
        {
            string[] prefabGuids =
                AssetDatabase.FindAssets(
                    "Player t:Prefab");

            List<GameObject> candidates =
                new List<GameObject>();

            foreach (string prefabGuid
                     in prefabGuids)
            {
                string assetPath =
                    AssetDatabase
                        .GUIDToAssetPath(
                            prefabGuid);

                GameObject prefab =
                    AssetDatabase
                        .LoadAssetAtPath<GameObject>(
                            assetPath);

                if (prefab == null)
                {
                    continue;
                }

                if (prefab.name == "Player")
                {
                    return prefab;
                }

                if (prefab.GetComponent<
                        PlayerStartingLoadoutInitializer>() != null)
                {
                    candidates.Add(
                        prefab);
                }
            }

            if (candidates.Count == 1)
            {
                return candidates[0];
            }

            Assert.Fail(
                candidates.Count == 0
                    ? "Player prefab could not be found."
                    : "Multiple Player prefab candidates were found.");

            return null;
        }
    }
}

#endif