# Reward Claim

## Purpose

Route a selected `ItemDefinition` from a `RewardOffer` into the correct acquisition path and ensure a successfully claimed offer can only be consumed once.

```text
RewardOffer
    ↓ selected ItemDefinition
RewardClaimSession
    ↓
PlayerRewardClaimController
    ↓
RewardClaimHandlerRegistry
    ├── WeaponRewardClaimHandler
    ├── AbilityRewardClaimHandler
    └── UpgradeRewardClaimHandler
```

Reward Claim remains unaware of concrete Fireball, weapon, or upgrade behavior.

## Existing Acquisition Paths

```text
Weapon  → PlayerWeaponAcquisitionController
Ability → PlayerAbilityAcquisitionController
Upgrade → PlayerUpgradeController
```

Reward Claim orchestrates these systems; it does not reimplement them.

## Core Decision

Use `IRewardClaimHandler` instead of one central `ItemCategory` switch.

Each handler knows one acquisition system and maps its category-specific result to a shared `RewardClaimResult`.

## Scope

```text
RewardClaimResult
RewardClaimSession
IRewardClaimHandler
RewardClaimHandlerRegistry
WeaponRewardClaimHandler
AbilityRewardClaimHandler
UpgradeRewardClaimHandler
PlayerRewardClaimController
Player prefab wiring
tests
development validation
```

## Out of Scope

```text
final reward UI
chest interaction/lifecycle
rarity
weighted rewards
reroll
currency fallback
item level-up/evolution
save/meta
weapon switching
```

## RewardClaimSession

`RewardOffer` stays immutable. Claim state lives in a separate plain C# object:

```text
RewardClaimSession
├── Offer
├── IsClaimed
└── ClaimedDefinition
```

A selection is valid only if it is the **exact `ItemDefinition` instance** stored in the offer. Another instance with the same StableId is rejected.

## RewardClaimResult

```text
Claimed
AlreadyClaimed
AlreadyOwned
CapacityReached
```

- `Claimed`: acquisition succeeded and the session is consumed.
- `AlreadyClaimed`: the session was already consumed.
- `AlreadyOwned`: the offer may be stale; keep the session open.
- `CapacityReached`: the offer may be stale; keep the session open.

Configuration/programmer errors remain exceptions.

## Handler Contract

```text
IRewardClaimHandler
{
    bool Supports(ItemDefinition definition);
    RewardClaimResult TryClaim(ItemDefinition definition);
}
```

Handlers do not own session state or UI.

Mappings:

```text
Weapon/Ability/Upgrade Acquired        → Claimed
Weapon/Ability/Upgrade AlreadyOwned    → AlreadyOwned
Weapon/Ability/Upgrade CapacityReached → CapacityReached
```

## Handler Registry

`RewardClaimHandlerRegistry` resolves exactly one supporting handler.

```text
null registration rejected
same instance duplicate rejected
no match → configuration error
multiple matches → ambiguity error
```

No concrete category switch.

## PlayerRewardClaimController

Flow:

```text
1. validate session
2. validate selection belongs to offer
3. guard already-claimed session
4. resolve handler
5. execute category acquisition
6. commit session only on Claimed
7. publish RewardClaimed
```

Invariant:

```text
acquisition failure → session remains open
successful claim    → session consumed
```

Event:

```text
RewardClaimed(ItemDefinition)
```

is published only on success.

## Dependency Direction

```text
Rewards
  ↓
Reward Claim
  ↓
Claim Handlers
  ↓
Weapon / Ability / Upgrade Acquisition
```

Acquisition layers never depend on Rewards.

## Player Composition

```text
Player
├── PlayerWeaponAcquisitionController
├── PlayerAbilityAcquisitionController
├── PlayerUpgradeController
└── PlayerRewardClaimController
```

Ability factory-registry initialization remains an Ability concern.

## Tests

### EditMode

```text
RewardClaimSession
- starts unclaimed
- exact offer instance accepted
- non-offered selection rejected
- same StableId different instance rejected
- successful commit stores selection
- second commit rejected

RewardClaimHandlerRegistry
- registration
- null rejection
- duplicate instance rejection
- matching resolution
- no match
- ambiguous match
```

### PlayMode

```text
weapon handler mapping
ability handler mapping
upgrade handler mapping

PlayerRewardClaimController
- weapon / ability / upgrade claim
- successful claim consumes session
- success event once
- second claim → AlreadyClaimed
- outside selection rejected
- AlreadyOwned keeps session open
- CapacityReached keeps session open
- missing handler keeps session open
- prefab contains controller
```

## Development Validation

Use the existing `Test_Waves` reward offer.

```text
offer is visible
claim a real offered item
PlayerBuild ownership updates
category acquisition effect is visible
session cannot be successfully claimed twice
Console clean
```

Final UI is not part of this branch.

## Implementation Stages

### Stage 1 — Claim Session Domain
Checkpoint:
```text
feat: add reward claim session domain
```

### Stage 2 — Handler Contract + Registry
Checkpoint:
```text
feat: add reward claim handler registry
```

### Stage 3 — Category Acquisition Adapters
Checkpoint:
```text
feat: add reward acquisition claim handlers
```

### Stage 4 — Player Claim Orchestration
Checkpoint:
```text
feat: add player reward claim orchestration
```

### Stage 5 — Development Validation
Only create a checkpoint if new development tooling is added.

### Stage 6 — Final Regression
```text
EditMode Run All
PlayMode Run All
Test_Waves
Console clean
reward-claim → dev
```

## Recommended Next Order

```text
1. weapon-switching
2. reward-selection-ui
3. chest-foundation
```
