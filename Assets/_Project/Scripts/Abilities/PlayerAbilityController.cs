using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using UnityEngine;
using ProjectFirstRun.Stats;

namespace ProjectFirstRun.Abilities
{
    [DisallowMultipleComponent]
    public sealed class PlayerAbilityController :
        MonoBehaviour
    {
        [Header("Runtime")]
        [SerializeField]
        private Transform _abilityOrigin;

        private readonly List<AbilityRuntimeEntry> _entries =
            new List<AbilityRuntimeEntry>();

        private ReadOnlyCollection<AbilityRuntimeEntry> _readOnlyEntries;

        private bool _abilityControlEnabled = true;
        private readonly HashSet<object> _controlBlocks = new();
        public void SetAbilityBlocked(object owner, bool blocked)
        {
            if (owner == null) throw new ArgumentNullException(nameof(owner));
            if (blocked) _controlBlocks.Add(owner);
            else _controlBlocks.Remove(owner);
        }
        private PlayerStatsController _stats;

        public IReadOnlyList<AbilityRuntimeEntry> Entries
        {
            get
            {
                EnsureReadOnlyEntries();
                return _readOnlyEntries;
            }
        }

        public int AbilityCount =>
            _entries.Count;

        public bool IsAbilityControlEnabled =>
            _abilityControlEnabled && _controlBlocks.Count == 0;

        public Transform AbilityOrigin =>
            _abilityOrigin != null
                ? _abilityOrigin
                : transform;

        private void Awake()
        {
            if (_abilityOrigin == null)
            {
                _abilityOrigin = transform;
            }
        }

        private void Update()
        {
            Tick(
                Time.deltaTime);
        }

        public void AddAbility(
            AbilityRuntimeEntry entry)
        {
            if (entry == null)
            {
                throw new ArgumentNullException(
                    nameof(entry));
            }

            if (_entries.Contains(entry))
            {
                throw new InvalidOperationException(
                    "The same ability runtime entry " +
                    "has already been added.");
            }

            _entries.Add(entry);
        }

        public bool Contains(
            AbilityRuntimeEntry entry)
        {
            if (entry == null)
            {
                return false;
            }

            return _entries.Contains(entry);
        }

        public AbilityRuntimeEntry GetEntry(AbilityDefinition definition)
        {
            if (definition == null) throw new ArgumentNullException(nameof(definition));
            foreach (AbilityRuntimeEntry entry in _entries)
                if (ReferenceEquals(entry.Definition, definition)) return entry;
            return null;
        }

        public void SetAbilityControlEnabled(
            bool isEnabled)
        {
            _abilityControlEnabled =
                isEnabled;
        }

        public void Tick(
            float deltaTime)
        {
            ValidateDeltaTime(
                deltaTime);

            TickCooldowns(
                deltaTime);
            foreach (var entry in _entries)
                entry.TickContinuous(deltaTime, AbilityOrigin.position, IsAbilityControlEnabled && isActiveAndEnabled);

            if (!IsAbilityControlEnabled)
            {
                return;
            }

            TryAutoCastReadyAbilities();
        }

        private void TickCooldowns(
            float deltaTime)
        {
            foreach (AbilityRuntimeEntry entry
                     in _entries)
            {
                entry.Tick(
                    deltaTime);
            }
        }

        private void TryAutoCastReadyAbilities()
        {
            Vector3 origin =
                AbilityOrigin.position;

            foreach (AbilityRuntimeEntry entry
                     in _entries)
            {
                if (entry.IsContinuous || !entry.IsReady)
                {
                    continue;
                }

                entry.TryAutoCast(
                    origin, CurrentCooldownMultiplier());
            }
        }

        private float CurrentCooldownMultiplier()
        {
            if (_stats == null) _stats = GetComponent<PlayerStatsController>();
            return AbilityCooldownScaling.Multiplier(_stats == null ? null : _stats.Stats);
        }

        private void EnsureReadOnlyEntries()
        {
            if (_readOnlyEntries != null)
            {
                return;
            }

            _readOnlyEntries =
                _entries.AsReadOnly();
        }

        private static void ValidateDeltaTime(
            float deltaTime)
        {
            if (float.IsNaN(deltaTime) ||
                float.IsInfinity(deltaTime) ||
                deltaTime < 0f)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(deltaTime),
                    deltaTime,
                    "Delta time must be finite and non-negative.");
            }
        }
    }
}
