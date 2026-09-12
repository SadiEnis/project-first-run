# Reward Selection UI

## Purpose

Present an existing `RewardOffer`, let the player choose one exact offered item, route that choice through `PlayerRewardClaimController`, and close the modal only when the claim is consumed.

```text
RewardClaimSession
    ↓
RewardSelectionController
    ↔ RewardSelectionView
        └── RewardChoiceView[]
    ↓
PlayerRewardClaimController
```

The UI displays reward state; it does not own offer generation, eligibility, acquisition, or claim rules.

## Existing Foundations

```text
RewardOffer                  immutable offered choices
RewardClaimSession           authoritative claim state
PlayerRewardClaimController  category-independent claim orchestration
ItemDefinition               stable identity and display metadata
```

The selection layer must use these objects directly and must not duplicate their state.

## Core Decisions

### One Authoritative Claim Session

`RewardSelectionController` holds the active `RewardClaimSession` reference. The view receives only the offer data required for rendering.

The controller never creates a second list of selectable domain objects and never compares selections by StableId.

### Single-Selection Modal

This branch supports one successful selection per offer.

```text
remaining selections = 1
successful claim     = close modal
failed claim          = keep modal open
```

Legendary/Boss multi-selection semantics belong to a later chest/reward-session layer. They will not be anticipated with unused generic abstractions.

### Fixed Reusable Choice Views

The prefab contains six reusable `RewardChoiceView` slots, matching the largest currently planned normal reward screen.

Unused slots are hidden. Runtime code does not instantiate or destroy choice objects every time a reward opens.

## Scope

```text
ItemDefinition gameplay-effect display text
RewardChoiceView
RewardSelectionView
RewardSelectionController
single-choice claim flow
modal time/control handling
Player/UI prefab composition
Input System UI event handling
EditMode and PlayMode tests
Test_Waves development integration
```

## Out of Scope

```text
chest spawning and interaction
chest rarity and colors
weighted chest tables
multi-selection rewards
item level-up and evolution
reroll and skip
gold fallback
final art, icons, animation, audio, and localization
inventory replacement when capacity is full
```

## Display Contract

Each choice shows:

```text
DisplayName
ItemCategory
GameplayEffect
NEW
```

The screen also shows:

```text
Choose a Reward
Remaining selections: 1
claim feedback when a recoverable claim fails
```

Current/new level and evolution state cannot be shown truthfully until those domain systems exist. The view must not invent placeholder runtime state.

`ItemDefinition.GameplayEffect` is static presentation metadata. It is trimmed during validation and never used to calculate gameplay behavior.

## RewardChoiceView

Responsibilities:

```text
bind one exact ItemDefinition reference
render display name/category/effect/state
publish selection when its Button is clicked
clear and hide unused slots
```

It does not call claim or acquisition APIs.

The Button listener is registered once. Rebinding must not accumulate callbacks.

## RewardSelectionView

Responsibilities:

```text
validate serialized references
render RewardOffer choices into fixed slots
show/hide modal root
publish exact selected ItemDefinition
show recoverable feedback
select the first available Button for keyboard/gamepad navigation
```

Opening an empty offer is rejected because a modal with no valid action would soft-lock the player. Chest fallback behavior will be added by the chest layer.

## RewardSelectionController

Public flow:

```text
Open(RewardClaimSession)
    validate non-null, unclaimed, non-empty session
    capture previous time scale/control state
    pause gameplay
    render and show view

Select(ItemDefinition)
    call PlayerRewardClaimController.TryClaim
    Claimed         → close and restore
    AlreadyClaimed  → close and restore
    AlreadyOwned    → keep open and show feedback
    CapacityReached → keep open and show feedback
```

Configuration errors remain exceptions.

## Modal Gameplay Policy

While the reward screen is open:

```text
Time.timeScale = 0
PlayerController gameplay input disabled
cursor unlocked and visible through PlayerController policy
UI input remains enabled
```

On close or component disable, restore the exact captured values rather than assuming the previous state was normal gameplay.

An already disabled/dead player must not be re-enabled by closing the reward screen.

## Events

Minimum controller events:

```text
SelectionOpened(RewardClaimSession)
SelectionClosed(RewardClaimResult)
```

Events are published once per actual transition.

## Prefab Composition

```text
RewardSelectionUI
├── Canvas
├── CanvasScaler
├── GraphicRaycaster
├── RewardSelectionView
├── RewardSelectionController
├── ModalRoot
│   ├── Header
│   ├── RemainingSelections
│   ├── Feedback
│   └── ChoiceContainer
│       └── RewardChoiceView x6
└── EventSystem
    └── InputSystemUIInputModule
```

The UI prefab is scene-owned. It does not belong on the Player prefab.

## Development Integration

`Test_Waves` uses a development-only bridge:

```text
RewardDevelopmentController generates offer
    ↓
RewardSelectionDevelopmentBootstrap opens session
    ↓
player claims Development Secondary
    ↓
modal closes and gameplay resumes
    ↓
Q / Gamepad North switches weapons
```

The existing debug presenter may remain as diagnostic tooling, but normal selection is performed through the new UI.

## Tests

### EditMode

```text
ItemDefinition trims effect text
choice binding renders exact definition
choice rebinding does not duplicate callbacks
unused choice clears and hides
view rejects insufficient slot configuration
view rejects empty offer
player/UI prefab composition
```

### PlayMode

```text
open displays all choices
open pauses time and disables player control
successful button selection claims exact definition
successful claim closes and restores prior state
AlreadyOwned keeps modal open
CapacityReached keeps modal open
already-claimed session cannot be opened
disable while open restores prior state
keyboard/gamepad UI navigation selects a choice
development bridge opens Test_Waves offer
```

## Implementation Stages

### Stage 1 — Architecture and Display Metadata

Checkpoint:

```text
docs: define reward selection UI architecture
```

### Stage 2 — Reusable Presentation Views

Checkpoint:

```text
feat: add reward selection presentation
```

### Stage 3 — Claim and Modal Orchestration

Checkpoint:

```text
feat: add reward selection orchestration
```

### Stage 4 — UI Prefab Composition

Checkpoint:

```text
feat: add reward selection UI prefab
```

### Stage 5 — Development Validation

Checkpoint only when development tooling/content changes:

```text
test: integrate reward selection UI with Test_Waves
```

### Stage 6 — Final Regression

```text
EditMode Run All
PlayMode Run All
manual Test_Waves
Console clean
reward-selection-ui → dev
```

## Next Layer

```text
chest-foundation
```

Chest Foundation will decide when an offer is created and when this UI opens. It must reuse the selection/claim boundary instead of moving chest lifecycle rules into the view.
