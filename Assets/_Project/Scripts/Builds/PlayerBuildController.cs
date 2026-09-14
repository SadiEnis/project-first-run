using System;
using ProjectFirstRun.Items;
using UnityEngine;

namespace ProjectFirstRun.Builds
{
    [DisallowMultipleComponent]
    public sealed class PlayerBuildController : MonoBehaviour
    {
        private PlayerBuild _build;

        public bool IsInitialized =>
            _build != null;

        public PlayerBuild Build
        {
            get
            {
                EnsureInitialized();
                return _build;
            }
        }

        public void Initialize(
            PlayerBuildCapacity capacity)
        {
            if (IsInitialized)
            {
                throw new InvalidOperationException(
                    $"{nameof(PlayerBuildController)} " +
                    "has already been initialized.");
            }

            if (capacity == null)
            {
                throw new ArgumentNullException(
                    nameof(capacity));
            }

            _build =
                new PlayerBuild(capacity);
        }

        public PlayerBuildAddResult TryAdd(
            ItemDefinition itemDefinition)
        {
            EnsureInitialized();

            if (itemDefinition == null)
            {
                throw new ArgumentNullException(
                    nameof(itemDefinition));
            }

            itemDefinition.ValidateLevelConfiguration();
            return _build.TryAdd(
                itemDefinition.Category,
                itemDefinition.StableId,
                itemDefinition.MaximumLevel);
        }

        public bool Contains(
            ItemDefinition itemDefinition)
        {
            EnsureInitialized();

            if (itemDefinition == null)
            {
                throw new ArgumentNullException(
                    nameof(itemDefinition));
            }

            return _build.Contains(
                itemDefinition.Category,
                itemDefinition.StableId);
        }

        private void EnsureInitialized()
        {
            if (!IsInitialized)
            {
                throw new InvalidOperationException(
                    $"{nameof(PlayerBuildController)} " +
                    "must be initialized before use.");
            }
        }
    }
}
