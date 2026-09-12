# Weapon Acquisition

## 1. Purpose

The Weapon Acquisition branch defines the generic acquisition layer used when a `WeaponDefinition` is selected. It adds run-scoped ownership, stores the concrete definition in a runtime loadout, and installs the weapon into `PlayerWeaponController` when it should become active.

```text
WeaponDefinition
      ↓
PlayerWeaponAcquisitionController
      ↓
PlayerBuildController
      ↓
PlayerWeaponLoadout
      ↓
PlayerWeaponController
```

## 2. Current Runtime Constraint

`PlayerWeaponController` currently owns only one active weapon runtime:

```text
_activeDefinition
_runtimeState
```

Calling `Initialize(WeaponDefinition)` replaces that active definition and creates a new runtime state.

However, `PlayerBuildCapacity` supports up to two owned weapons.

Acquisition therefore cannot be implemented as a direct `PlayerWeaponController.Initialize(...)` call for every acquired weapon.

## 3. Core Architecture Decision

Ownership and active weapon state are separated.

```text
PlayerBuild
→ ownership + capacity source of truth

PlayerWeaponLoadout
→ runtime WeaponDefinition references
→ active weapon selection state

PlayerWeaponController
→ firing/ammo/reload runtime for the active weapon only
```

`PlayerWeaponLoadout` is not a second ownership authority. It stores concrete runtime references only after successful acquisition.

## 4. Scope

```text
WeaponAcquireResult
PlayerWeaponLoadout
PlayerWeaponAcquisitionController
first-weapon auto-equip
second-weapon ownership + loadout storage
active weapon runtime initialization
existing starting weapon flow integration
Player prefab integration
EditMode / PlayMode tests
development validation
```

## 5. Out of Scope

```text
weapon switch input
weapon wheel UI
next/previous weapon controls
weapon drop/remove
weapon replacement UI
per-weapon preserved ammo state
weapon level-up
weapon evolution
save/load
meta progression
reward claim UI
```

## 6. PlayerWeaponLoadout

`PlayerWeaponLoadout` is a pure C# runtime object.

Responsibilities:

```text
store acquired WeaponDefinition references in acquisition order
provide read-only access
store active definition
prevent duplicate runtime installation
```

Capacity and ownership decisions remain in `PlayerBuild`.

## 7. Acquisition Semantics

Entry point:

```text
PlayerWeaponAcquisitionController.TryAcquire(WeaponDefinition)
```

Normal results:

```text
Acquired
AlreadyOwned
CapacityReached
```

First weapon:

```text
PlayerBuild add
→ PlayerWeaponLoadout add
→ if no active weapon, auto-equip
→ PlayerWeaponController.Initialize(definition)
```

Second weapon:

```text
PlayerBuild add
→ PlayerWeaponLoadout add
→ preserve existing active weapon
```

Acquisition does not automatically replace the active weapon with the second weapon.

## 8. Equip Boundary

`PlayerWeaponController` remains responsible only for the active weapon runtime.

The loadout may expose an explicit active selection API such as:

```text
SetActive / TrySetActive
```

No gameplay input is wired in this branch.

A later switching layer may perform:

```text
input
→ select loadout weapon
→ change active definition
→ PlayerWeaponController.Initialize(selected)
```

## 9. Runtime Reset Semantics

Current `PlayerWeaponController.Initialize(...)` creates a new `WeaponRuntimeState`.

Therefore weapon switching currently implies reset magazine/reserve runtime state.

This branch does not change that behavior because it does not implement switching.

## 10. Transaction Boundary

Acquisition must avoid partial state.

Desired invariant:

```text
PlayerBuild owns weapon
⇔ successful runtime loadout installation
```

Conceptual flow:

```text
1. validate definition / StableId
2. validate WeaponRuntimeConfig can be created
3. PlayerBuild ownership/capacity preflight
4. PlayerBuild.TryAdd(definition)
5. add to PlayerWeaponLoadout
6. auto-equip if first weapon
7. publish WeaponAcquired
```

`WeaponDefinition.CreateRuntimeConfig()` should be evaluated before ownership mutation so malformed content fails early.

## 11. PlayerWeaponController Boundary

`PlayerWeaponController` does not learn acquisition ownership.

It keeps:

```text
active WeaponDefinition
WeaponRuntimeState
fire
reload
ammo
hitscan resolution
WeaponDamage stat evaluation
```

The existing `Initialize(WeaponDefinition)` API remains the active-runtime installation boundary.

## 12. Starting Loadout Integration

If the current starting loadout directly manipulates `PlayerBuildController` and `PlayerWeaponController.Initialize(...)`, it should be migrated to the new acquisition path.

The exact refactor must not be guessed before inspecting `PlayerStartingLoadoutInitializer`.

Target:

```text
starting loadout
reward claim
development tooling
        ↓
PlayerWeaponAcquisitionController
```

## 13. Events

Minimum event:

```text
WeaponAcquired(WeaponDefinition)
```

An `ActiveWeaponChanged` event should only be introduced when switching actually needs it.

## 14. Dependency Direction

```text
WeaponDefinition
      ↓
PlayerWeaponAcquisitionController
      ↓
PlayerBuildController
PlayerWeaponLoadout
PlayerWeaponController
```

Reward may depend on acquisition; weapon acquisition does not depend on Reward/Chest/UI layers.

## 15. Error Handling

Programmer/configuration errors:

```text
null definition
invalid StableId
invalid WeaponRuntimeConfig
missing dependencies
loadout/build invariant violation
unexpected PlayerBuildAddResult
```

Normal runtime outcomes:

```text
AlreadyOwned
CapacityReached
Acquired
```

## 16. Tests

### EditMode — PlayerWeaponLoadout

```text
new loadout empty
add preserves order
read-only collection
duplicate add rejected
active weapon initially null
active weapon must belong to loadout
null validation
```

### PlayMode — Acquisition

```text
first acquisition adds PlayerBuild ownership
first acquisition adds loadout entry
first acquisition initializes active PlayerWeaponController
second acquisition adds ownership/loadout
second acquisition does not replace active weapon
duplicate does not add twice
capacity reached does not mutate loadout/controller
event publishes once
invalid definition does not mutate ownership
Player prefab contains acquisition/loadout runtime
```

## 17. Development Validation

In `Test_Waves`:

```text
starting weapon acquisition remains correct
weapon fire/reload remains correct
PlayerBuild weapon ownership is correct
active weapon remains correct
a second weapon can be acquired if real content exists
second acquisition does not replace active weapon
Console clean
```

Do not create fake production content only for manual validation if no second real weapon exists.

## 18. Implementation Stages

### Stage 1 — Runtime Loadout

Checkpoint:
```text
feat: add player weapon runtime loadout
```

### Stage 2 — Acquisition Runtime

Checkpoint:
```text
feat: add player weapon acquisition runtime
```

### Stage 3 — Existing Flow Integration

Checkpoint:
```text
feat: integrate weapon acquisition with player loadout
```

### Stage 4 — Development Validation

Only create a checkpoint if new development code is required.

### Stage 5 — Final Regression

```text
EditMode Run All
PlayMode Run All
Test_Waves
Console clean
weapon-acquisition → dev
```

## 19. Next Layer

After this branch, all three item categories have acquisition paths:

```text
Weapon
→ PlayerWeaponAcquisitionController

Ability
→ PlayerAbilityAcquisitionController

Upgrade
→ PlayerUpgradeController
```

A later `reward-claim` branch can orchestrate category-specific acquisition without knowing concrete Fireball or concrete weapon runtime logic.
