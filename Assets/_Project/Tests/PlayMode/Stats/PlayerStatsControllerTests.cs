#if UNITY_EDITOR

using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using ProjectFirstRun.Stats;
using UnityEditor;
using UnityEngine;
using UnityEngine.TestTools;
using Object = UnityEngine.Object;

namespace ProjectFirstRun.Tests.PlayMode.Stats
{
    public sealed class PlayerStatsControllerTests
    {
        private GameObject _testObject;
        private PlayerStatsController _controller;

        [UnitySetUp]
        public IEnumerator SetUp()
        {
            _testObject =
                new GameObject(
                    "PlayerStatsController_Test");

            _controller =
                _testObject
                    .AddComponent<PlayerStatsController>();

            yield return null;
        }

        [TearDown]
        public void TearDown()
        {
            if (_testObject != null)
            {
                Object.DestroyImmediate(
                    _testObject);
            }
        }

        [Test]
        public void Awake_InitializesStatCollection()
        {
            Assert.That(
                _controller.IsInitialized,
                Is.True);

            Assert.That(
                _controller.Stats,
                Is.Not.Null);
        }

        [Test]
        public void Stats_ReturnsSameCollectionInstance()
        {
            PlayerStatCollection first =
                _controller.Stats;

            PlayerStatCollection second =
                _controller.Stats;

            Assert.That(
                second,
                Is.SameAs(first));
        }

        [Test]
        public void NewController_StartsWithoutModifiers()
        {
            Assert.That(
                _controller.Stats.ModifierCount,
                Is.EqualTo(0));
        }

        [Test]
        public void Evaluate_WithNoModifiers_ReturnsBaseValue()
        {
            float result =
                _controller.Evaluate(
                    PlayerStatType.WeaponDamage,
                    100f);

            Assert.That(
                result,
                Is.EqualTo(100f));
        }

        [Test]
        public void Evaluate_UsesOwnedStatCollection()
        {
            StatModifier modifier =
                new StatModifier(
                    PlayerStatType.WeaponDamage,
                    StatModifierOperation.AdditivePercent,
                    0.25f,
                    "test.weapon_damage");

            _controller.Stats.Add(
                modifier);

            float result =
                _controller.Evaluate(
                    PlayerStatType.WeaponDamage,
                    100f);

            Assert.That(
                result,
                Is.EqualTo(125f));
        }

        [Test]
        public void RemovingModifier_ChangesFutureEvaluation()
        {
            StatModifier modifier =
                new StatModifier(
                    PlayerStatType.AbilityDamage,
                    StatModifierOperation.Flat,
                    10f,
                    "test.ability_damage");

            _controller.Stats.Add(
                modifier);

            float modifiedResult =
                _controller.Evaluate(
                    PlayerStatType.AbilityDamage,
                    20f);

            bool removed =
                _controller.Stats.Remove(
                    modifier);

            float restoredResult =
                _controller.Evaluate(
                    PlayerStatType.AbilityDamage,
                    20f);

            Assert.That(
                modifiedResult,
                Is.EqualTo(30f));

            Assert.That(
                removed,
                Is.True);

            Assert.That(
                restoredResult,
                Is.EqualTo(20f));
        }

        [UnityTest]
        public IEnumerator Collection_PersistsAcrossFrames()
        {
            StatModifier modifier =
                new StatModifier(
                    PlayerStatType.WeaponDamage,
                    StatModifierOperation.Flat,
                    15f,
                    "test.persistence");

            PlayerStatCollection originalCollection =
                _controller.Stats;

            _controller.Stats.Add(
                modifier);

            yield return null;
            yield return null;

            Assert.That(
                _controller.Stats,
                Is.SameAs(originalCollection));

            Assert.That(
                _controller.Stats.Contains(
                    modifier),
                Is.True);

            Assert.That(
                _controller.Stats.ModifierCount,
                Is.EqualTo(1));
        }

        [UnityTest]
        public IEnumerator DisableAndEnable_DoesNotRecreateCollection()
        {
            StatModifier modifier =
                new StatModifier(
                    PlayerStatType.WeaponDamage,
                    StatModifierOperation.Flat,
                    15f,
                    "test.disable_enable");

            PlayerStatCollection originalCollection =
                _controller.Stats;

            _controller.Stats.Add(
                modifier);

            _controller.enabled =
                false;

            yield return null;

            _controller.enabled =
                true;

            yield return null;

            Assert.That(
                _controller.Stats,
                Is.SameAs(originalCollection));

            Assert.That(
                _controller.Stats.Contains(
                    modifier),
                Is.True);
        }

        [Test]
        public void PlayerPrefab_ContainsPlayerStatsController()
        {
            GameObject playerPrefab =
                FindPlayerPrefab();

            PlayerStatsController controller =
                playerPrefab.GetComponent<
                    PlayerStatsController>();

            Assert.That(
                controller,
                Is.Not.Null,
                "The Player prefab requires " +
                $"{nameof(PlayerStatsController)}.");
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
                    AssetDatabase.LoadAssetAtPath<GameObject>(
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