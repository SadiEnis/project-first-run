using System;
using System.Collections;
using System.Collections.Generic;
using ProjectFirstRun.Abilities;
using ProjectFirstRun.Builds;
using ProjectFirstRun.Chests;
using ProjectFirstRun.Chests.Spawning;
using ProjectFirstRun.Combat;
using ProjectFirstRun.Enemies;
using ProjectFirstRun.Enemies.Spawning;
using ProjectFirstRun.Items;
using ProjectFirstRun.Progression;
using ProjectFirstRun.UI.Rewards;
using ProjectFirstRun.Upgrades;
using ProjectFirstRun.Weapons;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace ProjectFirstRun.Development.Arenas
{
    /// <summary>Explicitly wired test commands; all progression uses normal gameplay controllers.</summary>
    public sealed class ContentArenaController : MonoBehaviour
    {
        [SerializeField] private GameObject _player;
        [SerializeField] private EnemyRegistry _registry;
        [SerializeField] private EnemySpawner _enemySpawner;
        [SerializeField] private EnemyController _enemyPrefab;
        [SerializeField] private EnemyDefinition[] _enemyDefinitions;
        [SerializeField] private Transform[] _spawnPoints;
        [SerializeField] private ChestSpawner _chestSpawner;
        [SerializeField] private ChestSpawnPlacement _placement;
        [SerializeField] private RewardSelectionController _selection;
        [SerializeField] private ItemDefinition[] _items;
        [SerializeField] private ChestDefinition[] _chests;
        private readonly List<EnemyController> _enemies = new List<EnemyController>();

        public GameObject Player => _player;
        public EnemyRegistry Registry => _registry;
        public RewardSelectionController Selection => _selection;
        public IReadOnlyList<EnemyController> Enemies => _enemies;
        public IReadOnlyList<ItemDefinition> Items => _items;
        public IReadOnlyList<ChestDefinition> Chests => _chests;
        public IReadOnlyList<EnemyDefinition> EnemyDefinitions => _enemyDefinitions;
        public bool IsReady { get; private set; }
        public string LastResult { get; private set; } = "Initializing content arena...";
        public bool CanMutate => IsReady && isActiveAndEnabled && _player != null &&
            !_player.GetComponent<HealthComponent>().IsDead && !_selection.IsOpen;

        public void ValidateConfiguration()
        {
            if (_player == null || _registry == null || _enemySpawner == null || _enemyPrefab == null ||
                _chestSpawner == null || _placement == null || _selection == null)
                throw new InvalidOperationException("Assign all content arena scene references.");
            if (_player.GetComponent<HealthComponent>() == null ||
                _player.GetComponent<PlayerBuildController>() == null ||
                _player.GetComponent<PlayerExperienceController>() == null ||
                _player.GetComponent<PlayerWeaponAcquisitionController>() == null ||
                _player.GetComponent<PlayerAbilityAcquisitionController>() == null ||
                _player.GetComponent<PlayerUpgradeController>() == null)
                throw new InvalidOperationException("The content arena requires the complete gameplay player prefab.");
            if (_enemyDefinitions == null || _enemyDefinitions.Length == 0 || _spawnPoints == null || _spawnPoints.Length == 0)
                throw new InvalidOperationException("Assign enemy definitions and spawn points.");
            foreach (var definition in _enemyDefinitions)
            {
                if (definition == null) throw new InvalidOperationException("Missing enemy definition.");
                definition.ValidateBehavior();
            }
            foreach (var point in _spawnPoints)
                if (point == null) throw new InvalidOperationException("Missing enemy spawn point.");
            if (_items == null || _chests == null) throw new InvalidOperationException("Assign test catalogs.");
            var identities = new HashSet<string>();
            foreach (var item in _items)
            {
                if (item == null || string.IsNullOrWhiteSpace(item.StableId) ||
                    !identities.Add(item.Category + ":" + item.StableId))
                    throw new InvalidOperationException("Item catalog requires unique valid identities.");
                item.ValidateLevelConfiguration();
                if (!(item is WeaponDefinition) && !(item is AbilityDefinition) && !(item is UpgradeDefinition))
                    throw new InvalidOperationException("Unsupported item in content test catalog: " + item.name);
            }
            foreach (var chest in _chests)
            {
                if (chest == null) throw new InvalidOperationException("Missing test chest.");
                chest.Validate();
                _placement.ValidatePrefab(chest.WorldPrefab);
            }
        }

        private IEnumerator Start()
        {
            ValidateConfiguration();
            var build = _player.GetComponent<PlayerBuildController>();
            var xp = _player.GetComponent<PlayerExperienceController>();
            var abilities = _player.GetComponent<PlayerAbilityAcquisitionController>();
            float deadline = Time.realtimeSinceStartup + 10;
            while (!build.IsInitialized || !xp.IsInitialized || !abilities.IsInitialized || !_chestSpawner.IsInitialized)
            {
                if (Time.realtimeSinceStartup > deadline)
                    throw new InvalidOperationException("Content arena services did not initialize.");
                yield return null;
            }
            IsReady = true;
            ReplaceGroup(-1);
        }

        // -1 selects a mixed group; nonnegative indices select a single configured family.
        public bool ReplaceGroup(int family) => Execute(() =>
        {
            if (family < -1 || family >= _enemyDefinitions.Length) throw new ArgumentOutOfRangeException(nameof(family));
            var requests = new EnemySpawnRequest[_spawnPoints.Length];
            for (int i = 0; i < requests.Length; i++)
            {
                var definition = _enemyDefinitions[family < 0 ? i % _enemyDefinitions.Length : family];
                requests[i] = new EnemySpawnRequest(_enemyPrefab, definition, _player.transform,
                    _player.GetComponent<HealthComponent>(), _registry, _spawnPoints[i].position, _spawnPoints[i].rotation);
            }
            ClearOwnedEnemies();
            try
            {
                foreach (var request in requests)
                {
                    var enemy = _enemySpawner.Spawn(in request).EnemyController;
                    _enemies.Add(enemy);
                    SceneManager.MoveGameObjectToScene(enemy.gameObject, gameObject.scene);
                }
            }
            catch { ClearOwnedEnemies(); throw; }
            return "Group ready: " + requests.Length + " enemies";
        });

        public bool Acquire(int index) => Execute(() =>
        {
            switch (GetItem(index))
            {
                case WeaponDefinition weapon: return _player.GetComponent<PlayerWeaponAcquisitionController>().TryAcquire(weapon).ToString();
                case AbilityDefinition ability: return _player.GetComponent<PlayerAbilityAcquisitionController>().TryAcquire(ability).ToString();
                case UpgradeDefinition upgrade: return _player.GetComponent<PlayerUpgradeController>().TryAcquire(upgrade).ToString();
                default: throw new InvalidOperationException("Unsupported item category.");
            }
        });

        public bool LevelUp(int index) => Execute(() =>
        {
            switch (GetItem(index))
            {
                case WeaponDefinition weapon: return _player.GetComponent<PlayerWeaponAcquisitionController>().TryLevelUp(weapon).ToString();
                case AbilityDefinition ability: return _player.GetComponent<PlayerAbilityAcquisitionController>().TryLevelUp(ability).ToString();
                case UpgradeDefinition upgrade: return _player.GetComponent<PlayerUpgradeController>().TryLevelUp(upgrade).ToString();
                default: throw new InvalidOperationException("Unsupported item category.");
            }
        });

        public int ItemLevel(int index)
        {
            if (!IsReady) return 0;
            var item = GetItem(index);
            return _player.GetComponent<PlayerBuildController>().Build.TryGetItem(item.Category, item.StableId, out var owned)
                ? owned.Level : 0;
        }

        public bool GrantExperience(int amount) => Execute(() =>
        {
            if (amount <= 0 || amount > 10000) throw new ArgumentOutOfRangeException(nameof(amount));
            _player.GetComponent<PlayerExperienceController>().GainExperience(amount);
            return "Granted " + amount + " XP through gameplay progression";
        });

        public bool SpawnChest(int index) => Execute(() =>
        {
            if (index < 0 || index >= _chests.Length) throw new ArgumentOutOfRangeException(nameof(index));
            var definition = _chests[index];
            Physics.SyncTransforms();
            if (!_placement.TryFind(_player.transform, definition.WorldPrefab, out var position, out var rotation))
                throw new InvalidOperationException("No clear chest location nearby. Move to an open area.");
            var request = new ChestSpawnRequest(definition, position, rotation);
            _chestSpawner.Spawn(in request);
            return "Chest placed nearby; close panel and interact to choose a reward";
        });

        private ItemDefinition GetItem(int index)
        {
            if (index < 0 || index >= _items.Length) throw new ArgumentOutOfRangeException(nameof(index));
            return _items[index];
        }

        // True means the command ran, not that an acquisition necessarily succeeded; LastResult carries domain outcomes.
        private bool Execute(Func<string> command)
        {
            if (!CanMutate) { LastResult = "Unavailable: initializing, dead, disabled, or reward selection open"; return false; }
            try { LastResult = command(); return true; }
            catch (Exception error) { LastResult = error.Message; return false; }
        }

        private void ClearOwnedEnemies()
        {
            foreach (var enemy in _enemies)
            {
                if (enemy == null) continue;
                // Despawn, not damage: registry releases immediately and no death reward is emitted.
                enemy.gameObject.SetActive(false);
                Destroy(enemy.gameObject);
            }
            _enemies.Clear();
        }

        private void OnDestroy() => ClearOwnedEnemies();
    }
}
