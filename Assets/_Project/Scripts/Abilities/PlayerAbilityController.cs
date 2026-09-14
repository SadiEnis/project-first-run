using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using UnityEngine;

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
            _abilityControlEnabled;

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

            if (!_abilityControlEnabled)
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
                if (!entry.IsReady)
                {
                    continue;
                }

                entry.TryAutoCast(
                    origin);
            }
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
