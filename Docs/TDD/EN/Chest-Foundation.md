# Chest Foundation

## Purpose

Turn the existing reward offer, claim, and selection systems into a world-owned
chest flow.

```text
Chest spawn source
    ↓
ChestSpawner
    ↓
ChestController (Available)
    ← PlayerChestInteractor
    ↓
RewardOfferGenerator
    ↓
RewardClaimSession
    ↓
RewardSelectionController
    ↓ successful claim
ChestController (Opened)
```

Chest Foundation decides when an offer is generated and when the reward UI is
opened. It does not duplicate offer, eligibility, claim, or acquisition rules.

## Existing Foundations

```text
RewardItemPool              data-driven candidate source
RewardOfferGenerator        eligibility and unique offer generation
RewardClaimSession          authoritative claim state
RewardSelectionController   single-selection modal and claim flow
PlayerBuildController       current ownership and capacity source
PlayerInputReader           gameplay input boundary
```

## Core Decisions

### Claim Completion Owns Chest Completion

Opening the selection UI does not consume the chest.

```text
Available
    ↓ successful interaction and non-empty offer
Selecting
    ↓ RewardSelectionController.SelectionClosed
Opened
```

Only `Claimed` or `AlreadyClaimed` can complete the current single-selection
session. Recoverable claim failures leave the selection UI and chest session
active.

### One Runtime State Owner

`ChestState` is a pure C# object owned by one `ChestController`.

The view may hide presentation and the interactor may observe availability, but
neither stores a competing lifecycle state.

### Definition-Driven Chest Content

`ChestDefinition` is immutable design data. The foundation needs only:

```text
StableId
RewardItemPool
RequestedChoiceCount
WorldPrefab
```

The definition does not contain mutable offer or claim state.

Rarity, colors, evolution probability, multi-selection count, and gold value are
not guessed in the foundation. They will extend the definition when their domain
rules exist.

### Explicit First-Person Interaction

The player looks at an available chest within a configured range and presses the
Interact action.

```text
Keyboard / Mouse  E
Gamepad            Button South
```

`PlayerChestInteractor` owns the raycast and calls the exact focused
`ChestController`. Chests do not scan the scene or poll player input separately.

### Empty Offer Does Not Open a Modal

The current reward layer can legitimately return an empty offer when the player
has no eligible item.

Until gold fallback exists:

```text
empty offer
    → return NoEligibleRewards
    → keep chest Available
    → do not open RewardSelectionController
```

This avoids both a modal soft-lock and silent loss of a reward. A later fallback
layer can replace this outcome with gold without changing the selection UI.

## Scope

```text
ChestDefinition
ChestStatus and ChestState
ChestOpenResult
ChestController
ChestSpawnRequest and ChestSpawnResult
ChestSpawner
PlayerChestInteractor
Interact input action
foundation chest presentation/prefab
Test_Waves development integration
EditMode and PlayMode tests
```

## Out of Scope

```text
weighted chest-type tables
enemy or elite drop probabilities
experience and level-up spawning
boss and final-boss spawning rules
rarity and color policy
item levels and evolution
Legendary/Boss multi-selection
gold fallback and Golden Chest behavior
inventory replacement
opening animation, final art, audio, VFX, and localization
save/meta progression
interaction prompt UI
```

Spawn sources may call `ChestSpawner` later. They do not belong inside the chest
lifecycle controller.

## ChestDefinition

`ChestDefinition` is a `ScriptableObject` content asset.

Validation rejects:

```text
missing or untrimmed StableId
missing RewardItemPool
RequestedChoiceCount <= 0
missing WorldPrefab
```

The requested count may exceed the number of eligible candidates; the existing
offer generator already returns all available eligible choices.

## ChestState

```text
ChestStatus.Available
ChestStatus.Selecting
ChestStatus.Opened
```

Valid transitions:

```text
Available → Selecting
Selecting → Available   only when opening orchestration fails before UI ownership
Selecting → Opened      after the claim session is consumed
```

Invalid transitions are rejected. Repeated completion cannot publish duplicate
open events.

## ChestController

Responsibilities:

```text
own ChestState
generate one offer for an accepted open attempt
create one RewardClaimSession
open RewardSelectionController
observe selection completion
complete presentation exactly once
publish ChestOpened
```

It does not:

```text
read input
perform raycasts
select reward candidates itself
claim items
decide drop chances
destroy itself before claim completion
```

Open outcomes:

```text
SelectionOpened
AlreadySelecting
AlreadyOpened
SelectionBusy
NoEligibleRewards
```

Configuration errors remain exceptions.

## Spawning

`ChestSpawner` instantiates the prefab from the validated definition and injects
the scene-owned dependencies.

```text
ChestSpawnRequest
├── ChestDefinition
├── Position
└── Rotation

ChestSpawnResult
├── ChestController
└── SpawnedObject
```

The spawner does not choose which chest definition should drop. It receives that
decision from a future level-up, enemy-drop, or boss-reward source.

## Interaction

`PlayerChestInteractor` uses one forward raycast from an explicit origin.

```text
Interact pressed
    ↓
raycast within InteractionDistance
    ↓
resolve ChestController from hit object/parent
    ↓
TryOpen
```

Raycasts use a serialized layer mask and respect world occlusion. Gameplay input
is disabled by the reward modal immediately after a successful interaction, so
the same input cannot repeatedly open the chest.

## Presentation

The foundation prefab provides a recognizable placeholder world object with a
collider and a focused `ChestView`.

On completion the view disables interaction collision and hides its visual root.
The controller object may remain alive long enough to publish and retain its
authoritative Opened state. Pooling or destruction policy is deferred until a
real chest spawn economy exists.

## Dependency Direction

```text
Player Input
    ↓
PlayerChestInteractor
    ↓
ChestController
    ↓
Reward Foundation / Claim / Selection UI
```

Reward and UI layers do not depend on Chests. Wave, enemy, level-up, and boss
systems may depend on the spawning boundary later; Chests do not depend on those
sources.

## Tests

### EditMode

```text
ChestDefinition validation
ChestState valid transitions
ChestState invalid/repeated transitions
ChestSpawnRequest validation
chest prefab composition
player prefab interaction composition
Test_Waves scene wiring
```

### PlayMode

```text
valid open creates exact offer/session and opens UI
successful selection marks chest Opened exactly once
empty offer leaves chest Available and UI closed
busy selection leaves another chest Available
repeated interaction cannot generate another active session
spawner injects dependencies and preserves transform
interactor opens the exact looked-at chest
out-of-range or occluded chest is not opened
disabled/dead gameplay input cannot interact
```

## Development Validation

`Test_Waves` replaces the automatic reward popup with one development chest.

```text
start gameplay
    ↓
look at chest and press E / Gamepad South
    ↓
reward selection opens
    ↓
claim Development Secondary or Fireball
    ↓
chest disappears and gameplay resumes
    ↓
acquired behavior works; Console remains clean
```

The old reward debug presenter may remain as diagnostic tooling, but it must not
automatically open a second selection session.

## Implementation Stages

### Stage 1 — Architecture

Checkpoint:

```text
docs: define chest foundation architecture
```

### Stage 2 — Definition and Lifecycle Domain

```text
ChestDefinition
ChestStatus
ChestState
ChestOpenResult
EditMode tests
```

Checkpoint:

```text
feat: add chest lifecycle domain
```

### Stage 3 — Reward Opening Orchestration

```text
ChestController
claim-completion lifecycle
empty-offer and busy-selection guards
PlayMode tests
```

Checkpoint:

```text
feat: add chest reward orchestration
```

### Stage 4 — Spawn and Player Interaction

```text
ChestSpawnRequest
ChestSpawnResult
ChestSpawner
Interact input action
PlayerChestInteractor
tests
```

Checkpoint:

```text
feat: add chest spawning and interaction
```

### Stage 5 — Prefab and Development Integration

```text
foundation chest prefab
Player prefab composition
Test_Waves chest spawn/integration
```

Checkpoint:

```text
test: integrate chest foundation with Test_Waves
```

### Stage 6 — Final Regression

```text
EditMode Run All
PlayMode Run All
manual Test_Waves
Console clean
chest-foundation → dev
```

## Next Layers

```text
chest-drop-sources
chest-type-and-rarity-rules
item-level-and-evolution rewards
multi-selection reward sessions
gold fallback
```

These layers reuse the spawn, lifecycle, offer, claim, and UI boundaries instead
of adding source-specific logic to `ChestController`.
