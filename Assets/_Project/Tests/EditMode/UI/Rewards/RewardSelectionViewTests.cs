using System.Collections.Generic;
using System.Reflection;
using NUnit.Framework;
using ProjectFirstRun.Items;
using ProjectFirstRun.Rewards;
using ProjectFirstRun.UI.Rewards;
using ProjectFirstRun.Weapons;
using UnityEngine;
using UnityEngine.UI;
using Object = UnityEngine.Object;

namespace ProjectFirstRun.Tests.EditMode.UI.Rewards
{
    public sealed class RewardSelectionViewTests
    {
        private readonly List<Object> _createdObjects =
            new List<Object>();

        [TearDown]
        public void TearDown()
        {
            for (int index = _createdObjects.Count - 1;
                 index >= 0;
                 index--)
            {
                if (_createdObjects[index] != null)
                {
                    Object.DestroyImmediate(
                        _createdObjects[index]);
                }
            }

            _createdObjects.Clear();
        }

        [Test]
        public void ItemDefinition_ExposesGameplayEffect()
        {
            WeaponDefinition definition =
                CreateWeapon(
                    "weapon.test",
                    "Test Weapon",
                    "Deals test damage.");

            Assert.That(
                definition.GameplayEffect,
                Is.EqualTo("Deals test damage."));
        }

        [Test]
        public void ItemDefinition_OnValidateTrimsGameplayEffect()
        {
            WeaponDefinition definition =
                CreateWeapon(
                    "weapon.test",
                    "Test Weapon",
                    "  Deals test damage.  ");

            MethodInfo onValidate =
                typeof(ItemDefinition)
                    .GetMethod(
                        "OnValidate",
                        BindingFlags.Instance |
                        BindingFlags.NonPublic);

            Assert.That(
                onValidate,
                Is.Not.Null);

            onValidate.Invoke(
                definition,
                null);

            Assert.That(
                definition.GameplayEffect,
                Is.EqualTo("Deals test damage."));
        }

        [Test]
        public void ChoiceView_BindPresentsDefinitionAndForwardsExactChoice()
        {
            RewardChoiceView choiceView =
                CreateChoiceView(
                    "Choice");

            WeaponDefinition definition =
                CreateWeapon(
                    "weapon.test",
                    "Test Weapon",
                    "Deals test damage.");

            ItemDefinition selected = null;
            choiceView.Selected += choice =>
                selected = choice;

            choiceView.Bind(
                definition);
            choiceView.Button.onClick.Invoke();

            Text[] labels =
                choiceView.GetComponentsInChildren<Text>();

            Assert.That(
                choiceView.Definition,
                Is.SameAs(definition));
            Assert.That(
                labels[0].text,
                Is.EqualTo("Test Weapon"));
            Assert.That(
                labels[1].text,
                Is.EqualTo("Weapon"));
            Assert.That(
                labels[2].text,
                Is.EqualTo("Deals test damage."));
            Assert.That(
                labels[3].text,
                Is.EqualTo("NEW"));
            Assert.That(
                selected,
                Is.SameAs(definition));
        }

        [Test]
        public void ChoiceView_RebindingDoesNotAccumulateButtonCallbacks()
        {
            RewardChoiceView choiceView =
                CreateChoiceView(
                    "Choice");

            WeaponDefinition first =
                CreateWeapon(
                    "weapon.first",
                    "First",
                    "First effect");

            WeaponDefinition second =
                CreateWeapon(
                    "weapon.second",
                    "Second",
                    "Second effect");

            int selectionCount = 0;
            ItemDefinition selected = null;
            choiceView.Selected += choice =>
            {
                selectionCount++;
                selected = choice;
            };

            choiceView.Bind(first);
            choiceView.Bind(second);
            choiceView.Button.onClick.Invoke();

            Assert.That(
                selectionCount,
                Is.EqualTo(1));
            Assert.That(
                selected,
                Is.SameAs(second));
        }

        [Test]
        public void SelectionView_ShowBindsOfferOrderAndHidesUnusedChoices()
        {
            RewardChoiceView[] choices =
            {
                CreateChoiceView("Choice 1"),
                CreateChoiceView("Choice 2"),
                CreateChoiceView("Choice 3")
            };

            RewardSelectionView selectionView =
                CreateSelectionView(
                    choices,
                    out GameObject modalRoot,
                    out Text header,
                    out Text remaining,
                    out Text feedback);

            WeaponDefinition first =
                CreateWeapon(
                    "weapon.first",
                    "First",
                    "First effect");

            WeaponDefinition second =
                CreateWeapon(
                    "weapon.second",
                    "Second",
                    "Second effect");

            selectionView.Show(
                new RewardOffer(
                    new ItemDefinition[]
                    {
                        first,
                        second
                    }));

            Assert.That(
                selectionView.IsVisible,
                Is.True);
            Assert.That(
                modalRoot.activeSelf,
                Is.True);
            Assert.That(
                choices[0].Definition,
                Is.SameAs(first));
            Assert.That(
                choices[1].Definition,
                Is.SameAs(second));
            Assert.That(
                choices[2].gameObject.activeSelf,
                Is.False);
            Assert.That(
                header.text,
                Is.EqualTo("Choose a Reward"));
            Assert.That(
                remaining.text,
                Is.EqualTo("Selections Remaining: 1"));
            Assert.That(
                feedback.text,
                Is.Empty);
        }

        [Test]
        public void SelectionView_RepeatedShowForwardsOneExactSelection()
        {
            RewardChoiceView choice =
                CreateChoiceView(
                    "Choice");

            RewardSelectionView selectionView =
                CreateSelectionView(
                    new[] { choice },
                    out _,
                    out _,
                    out _,
                    out _);

            WeaponDefinition definition =
                CreateWeapon(
                    "weapon.test",
                    "Test",
                    "Test effect");

            RewardOffer offer =
                new RewardOffer(
                    new ItemDefinition[]
                    {
                        definition
                    });

            int selectionCount = 0;
            ItemDefinition selected = null;
            selectionView.ChoiceSelected += candidate =>
            {
                selectionCount++;
                selected = candidate;
            };

            selectionView.Show(offer);
            selectionView.Hide();
            selectionView.Show(offer);
            choice.Button.onClick.Invoke();

            Assert.That(
                selectionCount,
                Is.EqualTo(1));
            Assert.That(
                selected,
                Is.SameAs(definition));
        }

        [Test]
        public void SelectionView_WithMoreChoicesThanSlots_Throws()
        {
            RewardSelectionView selectionView =
                CreateSelectionView(
                    new[]
                    {
                        CreateChoiceView("Choice")
                    },
                    out _,
                    out _,
                    out _,
                    out _);

            RewardOffer offer =
                new RewardOffer(
                    new ItemDefinition[]
                    {
                        CreateWeapon(
                            "weapon.first",
                            "First",
                            "First effect"),
                        CreateWeapon(
                            "weapon.second",
                            "Second",
                            "Second effect")
                    });

            Assert.That(
                () => selectionView.Show(offer),
                Throws.InvalidOperationException);
        }

        [Test]
        public void SelectionView_WithEmptyOffer_Throws()
        {
            RewardSelectionView selectionView =
                CreateSelectionView(
                    new[]
                    {
                        CreateChoiceView("Choice")
                    },
                    out _,
                    out _,
                    out _,
                    out _);

            RewardOffer offer =
                new RewardOffer(
                    new ItemDefinition[0]);

            Assert.That(
                () => selectionView.Show(offer),
                Throws.ArgumentException);
        }

        [Test]
        public void SelectionView_ShowFeedbackReplacesExistingMessage()
        {
            RewardSelectionView selectionView =
                CreateSelectionView(
                    new[]
                    {
                        CreateChoiceView("Choice")
                    },
                    out _,
                    out _,
                    out _,
                    out Text feedback);

            selectionView.ShowFeedback(
                "Inventory is full.");

            Assert.That(
                feedback.text,
                Is.EqualTo("Inventory is full."));
        }

        private RewardSelectionView CreateSelectionView(
            RewardChoiceView[] choices,
            out GameObject modalRoot,
            out Text header,
            out Text remaining,
            out Text feedback)
        {
            GameObject root =
                CreateGameObject(
                    "Reward Selection");

            RewardSelectionView selectionView =
                root.AddComponent<RewardSelectionView>();

            modalRoot =
                CreateGameObject(
                    "Modal Root");
            header = CreateText("Header");
            remaining = CreateText("Remaining");
            feedback = CreateText("Feedback");

            selectionView.Initialize(
                modalRoot,
                header,
                remaining,
                feedback,
                choices);

            return selectionView;
        }

        private RewardChoiceView CreateChoiceView(
            string name)
        {
            GameObject root =
                CreateGameObject(
                    name,
                    typeof(RectTransform),
                    typeof(Button));

            RewardChoiceView choiceView =
                root.AddComponent<RewardChoiceView>();

            choiceView.Initialize(
                root.GetComponent<Button>(),
                CreateText("Name", root.transform),
                CreateText("Category", root.transform),
                CreateText("Effect", root.transform),
                CreateText("State", root.transform));

            return choiceView;
        }

        private Text CreateText(
            string name,
            Transform parent = null)
        {
            GameObject root =
                CreateGameObject(
                    name,
                    typeof(RectTransform),
                    typeof(CanvasRenderer),
                    typeof(Text));

            if (parent != null)
            {
                root.transform.SetParent(
                    parent,
                    false);
            }

            return root.GetComponent<Text>();
        }

        private GameObject CreateGameObject(
            string name,
            params System.Type[] components)
        {
            GameObject gameObject =
                new GameObject(
                    name,
                    components);

            _createdObjects.Add(
                gameObject);

            return gameObject;
        }

        private WeaponDefinition CreateWeapon(
            string stableId,
            string displayName,
            string gameplayEffect)
        {
            WeaponDefinition definition =
                ScriptableObject
                    .CreateInstance<WeaponDefinition>();

            SetItemField(
                definition,
                "_stableId",
                stableId);
            SetItemField(
                definition,
                "_displayName",
                displayName);
            SetItemField(
                definition,
                "_gameplayEffect",
                gameplayEffect);

            _createdObjects.Add(
                definition);

            return definition;
        }

        private static void SetItemField(
            ItemDefinition definition,
            string fieldName,
            string value)
        {
            FieldInfo field =
                typeof(ItemDefinition)
                    .GetField(
                        fieldName,
                        BindingFlags.Instance |
                        BindingFlags.NonPublic);

            Assert.That(
                field,
                Is.Not.Null);

            field.SetValue(
                definition,
                value);
        }
    }
}
