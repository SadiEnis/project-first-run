#if UNITY_EDITOR

using System.Collections.Generic;
using NUnit.Framework;
using ProjectFirstRun.Builds;
using ProjectFirstRun.Weapons;
using UnityEditor;
using UnityEngine;

namespace ProjectFirstRun.Tests.EditMode.Weapons
{
    public sealed class PlayerWeaponAcquisitionPrefabTests
    {
        [Test]
        public void PlayerPrefab_ContainsWeaponAcquisitionController()
        {
            GameObject playerPrefab =
                FindPlayerPrefab();

            Assert.That(
                playerPrefab.GetComponent<
                    PlayerWeaponAcquisitionController>(),
                Is.Not.Null);
        }

        [Test]
        public void PlayerPrefab_ContainsWeaponSwitcher()
        {
            GameObject playerPrefab =
                FindPlayerPrefab();

            Assert.That(
                playerPrefab.GetComponent<
                    PlayerWeaponSwitcher>(),
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
                    AssetDatabase.GUIDToAssetPath(
                        prefabGuid);

                GameObject prefab =
                    AssetDatabase.LoadAssetAtPath<
                        GameObject>(
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
