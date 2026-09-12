# Ability Acquisition

## Purpose
This branch defines the generic acquisition layer used when an `AbilityDefinition` is selected and must be added to `PlayerBuild` ownership and installed into `PlayerAbilityController` as the correct runtime entry.

```text
AbilityDefinition
      ↓
PlayerAbilityAcquisitionController
      ↓
PlayerBuildController
      ↓
AbilityRuntimeFactoryRegistry
      ↓
PlayerAbilityController
```

## Problem
The current runtime can host ability runtime entries and Fireball already has a concrete runtime factory, but a higher-level reward/chest system cannot yet grant an ability generically from only an `AbilityDefinition`.

Target:
```text
Reward / Chest / Debug
        ↓
AbilityDefinition
        ↓
generic acquisition
```

## Scope
```text
IAbilityRuntimeFactory
AbilityRuntimeFactoryRegistry
AbilityAcquireResult
PlayerAbilityAcquisitionController
FireballAbilityRuntimeFactory generic contract integration
Player prefab integration
EditMode / PlayMode tests
development validation
```

## Out of Scope
```text
Reward claim UI
Chest interaction
Ability level-up
Ability evolution
Ability replacement/removal
Save/load
Meta progression
Rarity / weighted selection
```

## Ownership Source of Truth
Ownership and capacity are not duplicated. `PlayerBuildController.TryAdd(...)` remains the source of truth.

Normal outcomes:
```text
Acquired
AlreadyOwned
CapacityReached
```

Missing factories or invalid runtime output are configuration/programmer errors; the exact behavior should be finalized after inspecting the existing factory API.

## Runtime Factory Contract
`IAbilityRuntimeFactory` should identify whether it supports a given `AbilityDefinition` and create the matching `AbilityRuntimeEntry`.

A factory does not own:
```text
PlayerBuild ownership
capacity
reward logic
UI
```

## Factory Registry
`AbilityRuntimeFactoryRegistry` resolves the correct factory for a definition.

```text
AbilityDefinition
      ↓
Registry
      ↓
matching factory
      ↓
AbilityRuntimeEntry
```

No concrete type switch should be used. Each factory exposes its own support contract.

## Fireball Integration
The existing `FireballAbilityRuntimeFactory` is adapted to the generic contract. Fireball-specific details remain inside that factory.

The acquisition controller does not know:
```text
FireballProjectile
EnemyRegistry
FireballAbilityExecutor
NearestEnemyTargetSelector
```

## Transaction Boundary
Partial state must be avoided:

```text
ownership exists, runtime missing   ✗
runtime exists, ownership missing   ✗
```

Conceptual flow:
```text
1. validate definition
2. resolve matching factory
3. validate runtime creation preconditions
4. PlayerBuild.TryAdd
5. if Added, install runtime entry
6. publish AbilityAcquired
```

If runtime creation has side effects, the exact ordering must be finalized after inspecting the current factory implementation.

## Event
```text
AbilityAcquired(AbilityDefinition)
```

## Dependency Direction
```text
Items / AbilityDefinition
        ↓
Ability Acquisition
        ↓
PlayerBuildController
PlayerAbilityController
AbilityRuntimeFactoryRegistry
        ↓
Concrete Ability Runtime Factories
```

Reward may depend on acquisition; acquisition must not depend on reward.

## Tests
### EditMode
```text
factory registration
factory resolution
duplicate/ambiguous registration
unsupported definition
null validation
```

### PlayMode
```text
successful acquisition adds build ownership
successful acquisition adds runtime entry
duplicate does not add runtime twice
capacity reached does not add runtime
event publishes once
Fireball uses real factory
Player prefab contains acquisition controller
```

## Development Validation
Manual validation in `Test_Waves`:
```text
Fireball initially unowned
acquire
PlayerBuild ownership added
PlayerAbilityController runtime added
Fireball auto-cast works
Console clean
```

## Implementation Stages
### Stage 1 — Factory Contract + Registry
Checkpoint:
```text
feat: add ability runtime factory registry
```

### Stage 2 — Acquisition Runtime
Checkpoint:
```text
feat: add player ability acquisition runtime
```

### Stage 3 — Fireball Integration
Checkpoint:
```text
feat: integrate fireball ability acquisition
```

### Stage 4 — Development Validation
Checkpoint:
```text
feat: add development ability acquisition validation
```

### Stage 5 — Final Regression
```text
EditMode Run All
PlayMode Run All
Test_Waves
Console clean
ability-acquisition → dev
```

## Next Layer
Likely next branch:
```text
weapon-acquisition
```

Reward claim can then orchestrate Weapon / Ability / Upgrade acquisition without concrete content knowledge.
