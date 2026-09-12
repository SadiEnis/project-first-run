#if UNITY_EDITOR

using NUnit.Framework;
using ProjectFirstRun.UI.Rewards;
using UnityEditor;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.UI;

namespace ProjectFirstRun.Tests.EditMode.UI.Rewards
{
    public sealed class RewardSelectionUIPrefabTests
    {
        private const string PrefabPath =
            "Assets/_Project/Prefabs/UI/RewardSelectionUI.prefab";

        [Test]
        public void Prefab_ContainsRequiredModalComposition()
        {
            GameObject prefab =
                LoadPrefab();

            Assert.That(
                prefab.GetComponent<Canvas>(),
                Is.Not.Null);

            Assert.That(
                prefab.GetComponent<CanvasScaler>(),
                Is.Not.Null);

            Assert.That(
                prefab.GetComponent<GraphicRaycaster>(),
                Is.Not.Null);

            Assert.That(
                prefab.GetComponent<RewardSelectionView>(),
                Is.Not.Null);

            Assert.That(
                prefab.GetComponent<RewardSelectionController>(),
                Is.Not.Null);

            Assert.That(
                prefab.transform.localScale,
                Is.EqualTo(Vector3.one));
        }

        [Test]
        public void Prefab_ContainsSixReusableChoiceViews()
        {
            GameObject prefab =
                LoadPrefab();

            RewardChoiceView[] choices =
                prefab.GetComponentsInChildren<
                    RewardChoiceView>(
                        true);

            Assert.That(
                choices,
                Has.Length.EqualTo(6));

            foreach (RewardChoiceView choice in choices)
            {
                Assert.That(
                    choice.Button,
                    Is.Not.Null);

                Assert.That(
                    choice.GetComponent<Button>(),
                    Is.SameAs(choice.Button));
            }
        }

        [Test]
        public void Prefab_ViewReferencesAreCompleteAndModalStartsHidden()
        {
            GameObject prefab =
                LoadPrefab();

            RewardSelectionView view =
                prefab.GetComponent<
                    RewardSelectionView>();

            SerializedObject serializedView =
                new SerializedObject(
                    view);

            SerializedProperty modalRoot =
                serializedView.FindProperty(
                    "_modalRoot");

            Assert.That(
                modalRoot.objectReferenceValue,
                Is.Not.Null);

            GameObject modalObject =
                (GameObject)
                    modalRoot.objectReferenceValue;

            Assert.That(
                modalObject.activeSelf,
                Is.False);

            AssertObjectReferenceIsAssigned(
                serializedView,
                "_headerText");

            AssertObjectReferenceIsAssigned(
                serializedView,
                "_remainingSelectionsText");

            AssertObjectReferenceIsAssigned(
                serializedView,
                "_feedbackText");

            SerializedProperty choices =
                serializedView.FindProperty(
                    "_choiceViews");

            Assert.That(
                choices.arraySize,
                Is.EqualTo(6));

            for (int index = 0;
                 index < choices.arraySize;
                 index++)
            {
                Assert.That(
                    choices.GetArrayElementAtIndex(
                            index)
                        .objectReferenceValue,
                    Is.Not.Null);
            }
        }

        [Test]
        public void Prefab_ControllerReferencesItsView()
        {
            GameObject prefab =
                LoadPrefab();

            RewardSelectionView view =
                prefab.GetComponent<
                    RewardSelectionView>();

            RewardSelectionController controller =
                prefab.GetComponent<
                    RewardSelectionController>();

            SerializedObject serializedController =
                new SerializedObject(
                    controller);

            Assert.That(
                serializedController.FindProperty(
                        "_view")
                    .objectReferenceValue,
                Is.SameAs(view));
        }

        [Test]
        public void Prefab_UsesInputSystemEventHandling()
        {
            GameObject prefab =
                LoadPrefab();

            EventSystem[] eventSystems =
                prefab.GetComponentsInChildren<
                    EventSystem>(
                        true);

            InputSystemUIInputModule[] inputModules =
                prefab.GetComponentsInChildren<
                    InputSystemUIInputModule>(
                        true);

            Assert.That(
                eventSystems,
                Has.Length.EqualTo(1));

            Assert.That(
                inputModules,
                Has.Length.EqualTo(1));

            Assert.That(
                inputModules[0].gameObject,
                Is.SameAs(
                    eventSystems[0].gameObject));

            Assert.That(
                inputModules[0].actionsAsset,
                Is.Not.Null);
        }

        private static GameObject LoadPrefab()
        {
            GameObject prefab =
                AssetDatabase.LoadAssetAtPath<
                    GameObject>(
                        PrefabPath);

            Assert.That(
                prefab,
                Is.Not.Null,
                $"Reward selection UI prefab is required at {PrefabPath}.");

            return prefab;
        }

        private static void AssertObjectReferenceIsAssigned(
            SerializedObject serializedObject,
            string propertyName)
        {
            SerializedProperty property =
                serializedObject.FindProperty(
                    propertyName);

            Assert.That(
                property,
                Is.Not.Null);

            Assert.That(
                property.objectReferenceValue,
                Is.Not.Null);
        }
    }
}

#endif
