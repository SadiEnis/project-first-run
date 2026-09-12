#if UNITY_EDITOR

using NUnit.Framework;
using ProjectFirstRun.Chests.Interaction;
using UnityEditor;
using UnityEngine;

namespace ProjectFirstRun.Tests.EditMode.Chests
{
    public sealed class PlayerChestInteractionPrefabTests
    {
        private const string PlayerPrefabPath =
            "Assets/_Project/Prefabs/Player/Player.prefab";

        [Test]
        public void PlayerPrefab_WiresChestInteractionFromCamera()
        {
            GameObject prefab =
                AssetDatabase.LoadAssetAtPath<GameObject>(
                    PlayerPrefabPath);

            Assert.That(prefab, Is.Not.Null);

            PlayerChestInteractor interactor =
                prefab.GetComponent<PlayerChestInteractor>();

            Assert.That(interactor, Is.Not.Null);
            Assert.That(interactor.InteractionOrigin, Is.Not.Null);
            Assert.That(
                interactor.InteractionOrigin.GetComponent<Camera>(),
                Is.Not.Null);
            Assert.That(interactor.InteractionDistance, Is.EqualTo(3f));

            SerializedObject serializedInteractor =
                new SerializedObject(interactor);

            Assert.That(
                serializedInteractor.FindProperty("_interactionLayers").intValue,
                Is.EqualTo(-1));
        }
    }
}

#endif
