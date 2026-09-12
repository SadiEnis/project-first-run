#if UNITY_EDITOR

using System.Collections;
using NUnit.Framework;
using ProjectFirstRun.Builds;
using ProjectFirstRun.Development.Weapons;
using UnityEditor;
using UnityEngine;
using UnityEngine.TestTools;
using Object = UnityEngine.Object;

namespace ProjectFirstRun.Tests.PlayMode.Development.Weapons
{
    public sealed class WeaponSwitchingDevelopmentBootstrapTests
    {
        private const string PlayerPrefabPath =
            "Assets/_Project/Prefabs/Player/Player.prefab";

        private GameObject _playerObject;

        [TearDown]
        public void TearDown()
        {
            if (_playerObject != null)
            {
                Object.DestroyImmediate(
                    _playerObject);
            }
        }

        [UnityTest]
        public IEnumerator Awake_InitializesTwoWeaponSlots()
        {
            GameObject playerPrefab =
                AssetDatabase.LoadAssetAtPath<GameObject>(
                    PlayerPrefabPath);

            Assert.That(
                playerPrefab,
                Is.Not.Null);

            _playerObject =
                Object.Instantiate(
                    playerPrefab);

            _playerObject.AddComponent<
                WeaponSwitchingDevelopmentBootstrap>();

            yield return null;

            PlayerBuildController buildController =
                _playerObject.GetComponent<
                    PlayerBuildController>();

            Assert.That(
                buildController.IsInitialized,
                Is.True);

            Assert.That(
                buildController.Build.Capacity.GetCapacity(
                    ProjectFirstRun.Items.ItemCategory.Weapon),
                Is.EqualTo(
                    PlayerBuildCapacity.MaximumWeaponSlots));
        }
    }
}

#endif
