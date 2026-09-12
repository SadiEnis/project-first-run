#if UNITY_EDITOR

using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using ProjectFirstRun.Abilities;
using ProjectFirstRun.Combat;
using ProjectFirstRun.Player;
using ProjectFirstRun.Weapons;
using UnityEditor;
using UnityEngine;
using UnityEngine.TestTools;

namespace ProjectFirstRun.Tests.PlayMode.Player
{
    public sealed class PlayerDeathControllerTests
    {
        private GameObject _playerObject;

        private HealthComponent _healthComponent;
        private PlayerController _playerController;
        private PlayerWeaponController _weaponController;
        private PlayerAbilityController _abilityController;
        private PlayerDeathController _deathController;

        [UnitySetUp]
        public IEnumerator SetUp()
        {
            GameObject playerPrefab =
                FindPlayerPrefab();

            _playerObject =
                Object.Instantiate(playerPrefab);

            _playerObject.name =
                "PlayerDeath_TestInstance";

            _healthComponent =
                GetRequiredComponent<HealthComponent>();

            _playerController =
                GetRequiredComponent<PlayerController>();

            _weaponController =
                GetRequiredComponent<PlayerWeaponController>();

            _abilityController =
                GetRequiredComponent<PlayerAbilityController>();

            _deathController =
                GetRequiredComponent<PlayerDeathController>();

            DisableCursorLockForTests();

            // Allows Awake, OnEnable and Start to complete.
            yield return null;

            _healthComponent.Initialize(100f);

            _playerController.SetControlEnabled(true);
            _weaponController.SetWeaponControlEnabled(true);
            _abilityController.SetAbilityControlEnabled(true);
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
        public void Start_WithLivingPlayer_EnablesGameplayControls()
        {
            Assert.That(
                _healthComponent.IsDead,
                Is.False);

            Assert.That(
                _deathController.IsDead,
                Is.False);

            Assert.That(
                _playerController.IsControlEnabled,
                Is.True);

            Assert.That(
                _weaponController.IsWeaponControlEnabled,
                Is.True);

            Assert.That(
                _abilityController.IsAbilityControlEnabled,
                Is.True);
        }

        [Test]
        public void NonLethalDamage_DoesNotDisableGameplayControls()
        {
            ApplyDamage(25f);

            Assert.That(
                _healthComponent.CurrentHealth,
                Is.EqualTo(75f));

            Assert.That(
                _deathController.IsDead,
                Is.False);

            Assert.That(
                _playerController.IsControlEnabled,
                Is.True);

            Assert.That(
                _weaponController.IsWeaponControlEnabled,
                Is.True);

            Assert.That(
                _abilityController.IsAbilityControlEnabled,
                Is.True);
        }

        [Test]
        public void LethalDamage_DisablesGameplayControls()
        {
            ApplyLethalDamage();

            Assert.That(
                _healthComponent.IsDead,
                Is.True);

            Assert.That(
                _deathController.IsDead,
                Is.True);

            Assert.That(
                _playerController.IsControlEnabled,
                Is.False);

            Assert.That(
                _weaponController.IsWeaponControlEnabled,
                Is.False);

            Assert.That(
                _abilityController.IsAbilityControlEnabled,
                Is.False);
        }

        [Test]
        public void RepeatedDamage_AfterDeath_PublishesPlayerDiedOnlyOnce()
        {
            int playerDiedCount = 0;

            _deathController.PlayerDied +=
                (_, _) => playerDiedCount++;

            ApplyLethalDamage();
            ApplyDamage(25f);
            ApplyDamage(25f);

            Assert.That(
                playerDiedCount,
                Is.EqualTo(1));

            Assert.That(
                _healthComponent.CurrentHealth,
                Is.EqualTo(0f));
        }

        [Test]
        public void HealthReset_AfterDeath_RestoresGameplayControls()
        {
            int playerRevivedCount = 0;

            _deathController.PlayerRevived +=
                () => playerRevivedCount++;

            ApplyLethalDamage();

            _healthComponent.ResetHealth();

            Assert.That(
                _healthComponent.CurrentHealth,
                Is.EqualTo(100f));

            Assert.That(
                _healthComponent.IsDead,
                Is.False);

            Assert.That(
                _deathController.IsDead,
                Is.False);

            Assert.That(
                _playerController.IsControlEnabled,
                Is.True);

            Assert.That(
                _weaponController.IsWeaponControlEnabled,
                Is.True);

            Assert.That(
                _abilityController.IsAbilityControlEnabled,
                Is.True);

            Assert.That(
                playerRevivedCount,
                Is.EqualTo(1));
        }

        [Test]
        public void HealthReset_DoesNotModifyWeaponAmmunition()
        {
            Assert.That(
                _weaponController.IsInitialized,
                Is.True,
                "The Player prefab must have a configured starting weapon.");

            int magazineAmmoBeforeDeath =
                _weaponController.MagazineAmmo;

            int reserveAmmoBeforeDeath =
                _weaponController.ReserveAmmo;

            ApplyLethalDamage();

            _healthComponent.ResetHealth();

            Assert.That(
                _weaponController.MagazineAmmo,
                Is.EqualTo(
                    magazineAmmoBeforeDeath));

            Assert.That(
                _weaponController.ReserveAmmo,
                Is.EqualTo(
                    reserveAmmoBeforeDeath));
        }

        [Test]
        public void HealthReset_WhileAlive_DoesNotPublishPlayerRevived()
        {
            int playerRevivedCount = 0;

            _deathController.PlayerRevived +=
                () => playerRevivedCount++;

            _healthComponent.ResetHealth();

            Assert.That(
                playerRevivedCount,
                Is.EqualTo(0));

            Assert.That(
                _deathController.IsDead,
                Is.False);

            Assert.That(
                _playerController.IsControlEnabled,
                Is.True);

            Assert.That(
                _weaponController.IsWeaponControlEnabled,
                Is.True);

            Assert.That(
                _abilityController.IsAbilityControlEnabled,
                Is.True);
        }

        [UnityTest]
        public IEnumerator Enable_AfterDeathWhileDisabled_SynchronizesDeadState()
        {
            _deathController.enabled =
                false;

            ApplyLethalDamage();

            Assert.That(
                _healthComponent.IsDead,
                Is.True);

            Assert.That(
                _deathController.IsDead,
                Is.False);

            Assert.That(
                _playerController.IsControlEnabled,
                Is.True);

            Assert.That(
                _weaponController.IsWeaponControlEnabled,
                Is.True);

            Assert.That(
                _abilityController.IsAbilityControlEnabled,
                Is.True);

            _deathController.enabled =
                true;

            yield return null;

            Assert.That(
                _deathController.IsDead,
                Is.True);

            Assert.That(
                _playerController.IsControlEnabled,
                Is.False);

            Assert.That(
                _weaponController.IsWeaponControlEnabled,
                Is.False);

            Assert.That(
                _abilityController.IsAbilityControlEnabled,
                Is.False);
        }

        [UnityTest]
        public IEnumerator Enable_AfterResetWhileDisabled_SynchronizesAliveState()
        {
            ApplyLethalDamage();

            Assert.That(
                _deathController.IsDead,
                Is.True);

            Assert.That(
                _abilityController.IsAbilityControlEnabled,
                Is.False);

            _deathController.enabled =
                false;

            _healthComponent.ResetHealth();

            Assert.That(
                _healthComponent.IsDead,
                Is.False);

            Assert.That(
                _deathController.IsDead,
                Is.True);

            Assert.That(
                _playerController.IsControlEnabled,
                Is.False);

            Assert.That(
                _weaponController.IsWeaponControlEnabled,
                Is.False);

            Assert.That(
                _abilityController.IsAbilityControlEnabled,
                Is.False);

            _deathController.enabled =
                true;

            yield return null;

            Assert.That(
                _deathController.IsDead,
                Is.False);

            Assert.That(
                _playerController.IsControlEnabled,
                Is.True);

            Assert.That(
                _weaponController.IsWeaponControlEnabled,
                Is.True);

            Assert.That(
                _abilityController.IsAbilityControlEnabled,
                Is.True);
        }

        [Test]
        public void RepeatedEnableCycles_DoNotCreateDuplicateSubscriptions()
        {
            _deathController.enabled =
                false;

            _deathController.enabled =
                true;

            _deathController.enabled =
                false;

            _deathController.enabled =
                true;

            _deathController.enabled =
                false;

            _deathController.enabled =
                true;

            int playerDiedCount = 0;

            _deathController.PlayerDied +=
                (_, _) => playerDiedCount++;

            ApplyLethalDamage();

            Assert.That(
                playerDiedCount,
                Is.EqualTo(1));
        }

        private void ApplyLethalDamage()
        {
            ApplyDamage(
                _healthComponent.MaximumHealth);
        }

        private void ApplyDamage(
            float amount)
        {
            DamageInfo damageInfo =
                new DamageInfo(
                    amount,
                    source: null,
                    _playerObject.transform.position,
                    Vector3.forward);

            _healthComponent.ApplyDamage(
                in damageInfo);
        }

        private T GetRequiredComponent<T>()
            where T : Component
        {
            T component =
                _playerObject.GetComponent<T>();

            Assert.That(
                component,
                Is.Not.Null,
                $"The Player prefab requires {typeof(T).Name}.");

            return component;
        }

        private void DisableCursorLockForTests()
        {
            SerializedObject serializedController =
                new SerializedObject(
                    _playerController);

            SerializedProperty lockCursorProperty =
                serializedController.FindProperty(
                    "_lockCursorWhileControlled");

            Assert.That(
                lockCursorProperty,
                Is.Not.Null);

            lockCursorProperty.boolValue =
                false;

            serializedController
                .ApplyModifiedPropertiesWithoutUndo();
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

                if (prefab == null ||
                    prefab.GetComponent<PlayerDeathController>() == null)
                {
                    continue;
                }

                if (prefab.name == "Player")
                {
                    return prefab;
                }

                candidates.Add(
                    prefab);
            }

            if (candidates.Count == 1)
            {
                return candidates[0];
            }

            Assert.Fail(
                candidates.Count == 0
                    ? "A Player prefab containing PlayerDeathController could not be found."
                    : "Multiple Player prefabs containing PlayerDeathController were found.");

            return null;
        }
    }
}

#endif