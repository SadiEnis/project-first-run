#if UNITY_EDITOR

using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using ProjectFirstRun.Stats;
using ProjectFirstRun.Weapons;
using UnityEditor;
using UnityEngine;
using UnityEngine.TestTools;
using Object = UnityEngine.Object;

namespace ProjectFirstRun.Tests.PlayMode.Stats
{
    public sealed class PlayerWeaponStatIntegrationTests
    {
        private GameObject _playerObject;

        private PlayerWeaponController _weaponController;
        private PlayerStatsController _statsController;

        [UnitySetUp]
        public IEnumerator SetUp()
        {
            GameObject playerPrefab =
                FindPlayerPrefab();

            _playerObject =
                Object.Instantiate(
                    playerPrefab);

            _playerObject.name =
                "PlayerWeaponStat_TestInstance";

            _weaponController =
                GetRequiredComponent<
                    PlayerWeaponController>();

            _statsController =
                GetRequiredComponent<
                    PlayerStatsController>();

            /*
             * Allows Awake, Start and the starting
             * loadout initializer to complete.
             */
            yield return null;

            Assert.That(
                _weaponController.IsInitialized,
                Is.True,
                "Player weapon must be initialized " +
                "by the starting loadout.");
        }

        [TearDown]
        public void TearDown()
        {
            if (_playerObject != null)
            {
                Object.DestroyImmediate(
                    _playerObject);
            }

            Cursor.lockState =
                CursorLockMode.None;

            Cursor.visible =
                true;
        }

        [Test]
        public void CurrentDamage_WithoutModifiers_EqualsBaseDamage()
        {
            float baseDamage =
                _weaponController
                    .ActiveDefinition
                    .BaseDamage;

            Assert.That(
                _weaponController.CurrentDamage,
                Is.EqualTo(baseDamage)
                    .Within(0.0001f));
        }

        [Test]
        public void CurrentDamage_WithWeaponDamageModifier_UsesStats()
        {
            float baseDamage =
                _weaponController
                    .ActiveDefinition
                    .BaseDamage;

            StatModifier modifier =
                new StatModifier(
                    PlayerStatType.WeaponDamage,
                    StatModifierOperation.AdditivePercent,
                    0.50f,
                    "test.weapon_damage");

            _statsController
                .Stats
                .Add(modifier);

            float expectedDamage =
                baseDamage * 1.50f;

            Assert.That(
                _weaponController.CurrentDamage,
                Is.EqualTo(expectedDamage)
                    .Within(0.0001f));
        }

        [Test]
        public void CurrentDamage_IgnoresAbilityDamageModifier()
        {
            float baseDamage =
                _weaponController
                    .ActiveDefinition
                    .BaseDamage;

            StatModifier modifier =
                new StatModifier(
                    PlayerStatType.AbilityDamage,
                    StatModifierOperation.AdditivePercent,
                    10f,
                    "test.ability_damage");

            _statsController
                .Stats
                .Add(modifier);

            Assert.That(
                _weaponController.CurrentDamage,
                Is.EqualTo(baseDamage)
                    .Within(0.0001f));
        }

        [Test]
        public void CurrentDamage_AfterModifierRemoval_ReturnsBaseDamage()
        {
            float baseDamage =
                _weaponController
                    .ActiveDefinition
                    .BaseDamage;

            StatModifier modifier =
                new StatModifier(
                    PlayerStatType.WeaponDamage,
                    StatModifierOperation.Flat,
                    20f,
                    "test.temporary_damage");

            _statsController
                .Stats
                .Add(modifier);

            Assert.That(
                _weaponController.CurrentDamage,
                Is.EqualTo(baseDamage + 20f)
                    .Within(0.0001f));

            bool removed =
                _statsController
                    .Stats
                    .Remove(modifier);

            Assert.That(
                removed,
                Is.True);

            Assert.That(
                _weaponController.CurrentDamage,
                Is.EqualTo(baseDamage)
                    .Within(0.0001f));
        }

        [Test]
        public void CurrentDamage_WhenEvaluatedDamageIsZero_Throws()
        {
            float baseDamage =
                _weaponController
                    .ActiveDefinition
                    .BaseDamage;

            StatModifier modifier =
                new StatModifier(
                    PlayerStatType.WeaponDamage,
                    StatModifierOperation.Flat,
                    -baseDamage,
                    "test.invalid_damage");

            _statsController
                .Stats
                .Add(modifier);

            Assert.That(
                () =>
                {
                    float _ =
                        _weaponController.CurrentDamage;
                },
                Throws.InvalidOperationException);
        }

        private T GetRequiredComponent<T>()
            where T : Component
        {
            T component =
                _playerObject
                    .GetComponent<T>();

            Assert.That(
                component,
                Is.Not.Null,
                $"Player prefab requires " +
                $"{typeof(T).Name}.");

            return component;
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
                        PlayerWeaponController>() != null &&
                    prefab.GetComponent<
                        PlayerStatsController>() != null)
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
                    ? "A Player prefab could not be found."
                    : "Multiple Player prefab candidates were found.");

            return null;
        }
    }
}

#endif