# Weapon Switching

## 1. Purpose

The `weapon-switching` branch makes multiple acquired weapons usable in gameplay without losing per-weapon runtime state.

```text
PlayerWeaponLoadout
        ↓
active runtime entry
        ↓
PlayerWeaponSwitcher
        ↓
PlayerWeaponController
        ↓
fire / ammo / reload
```

## 2. Core Problem

The current `PlayerWeaponController.Initialize(WeaponDefinition)` creates a new `WeaponRuntimeState`.

Reinitializing on every switch would reset ammo/reload/cooldown state and create exploits.

```text
Weapon A magazine = 0
switch to B
switch back to A
Initialize(A)
→ magazine becomes full again
```

## 3. Core Design Decision

Each acquired weapon owns a persistent runtime state.

```text
PlayerWeaponRuntimeEntry
├── WeaponDefinition
└── WeaponRuntimeState
```

`PlayerWeaponLoadout` owns runtime entries rather than only definition references. Switching equips an existing entry instead of allocating new state.

## 4. Responsibility Split

### PlayerWeaponRuntimeEntry

Plain C# object representing one acquired weapon and its persistent runtime state.

### PlayerWeaponLoadout

Owns:

```text
runtime entries
acquisition order
active entry
lookup / active selection
```

Capacity remains owned by `PlayerBuild`.

### PlayerWeaponController

Owns only active-weapon combat orchestration:

```text
fire
reload
ammo
hitscan
damage evaluation
```

### PlayerWeaponSwitcher

Small Unity orchestration component:

```text
switch request
→ resolve next loadout entry
→ set active
→ equip existing runtime entry
```

## 5. Scope

```text
PlayerWeaponRuntimeEntry
PlayerWeaponLoadout runtime-state migration
PlayerWeaponController existing-state equip boundary
PlayerWeaponSwitcher
SwitchWeapon input action integration
first/second weapon switching
ammo/reload/cooldown state preservation
events
Player prefab integration
EditMode tests
PlayMode tests
Test_Waves validation
```

## 6. Out of Scope

```text
weapon selection UI
weapon wheel
direct slot-number selection
drop/remove/replacement
world pickups
weapon level-up/evolution
rarity
animation polish
save serialization
meta progression
```

## 7. Runtime Entry

Suggested contract:

```text
PlayerWeaponRuntimeEntry
{
    WeaponDefinition Definition
    WeaponRuntimeState RuntimeState
}
```

Creation:

```text
definition.CreateRuntimeConfig()
→ new WeaponRuntimeState(config)
→ PlayerWeaponRuntimeEntry
```

## 8. Loadout Migration

Preserve useful existing semantics:

```text
WeaponCount
HasActiveWeapon
ActiveDefinition
Contains(...)
```

Add runtime-facing access:

```text
ActiveEntry
Entries
GetEntry(...)
SetActive(...)
GetNextEntry()
```

Acquisition order is the initial switch order:

```text
[A, B]
A → B
B → A
```

One weapon produces no effective switch.

## 9. Controller Equip Boundary

Switching uses an explicit boundary such as:

```text
Equip(PlayerWeaponRuntimeEntry entry)
```

The controller references the existing definition and runtime state instead of creating a new state.

## 10. Acquisition Integration

First weapon:

```text
validate
→ PlayerBuild add
→ create/add runtime entry
→ set active
→ equip
```

Second weapon:

```text
validate
→ PlayerBuild add
→ create/add runtime entry
→ preserve existing active entry
```

## 11. Switching Rules

```text
0 weapons → no-op
1 weapon  → no-op
2 weapons → A ↔ B
```

Normal unavailable switching is not an exception. Runtime consistency violations are errors.

## 12. Input

New Input System action:

```text
Gameplay/SwitchWeapon
```

Suggested development bindings:

```text
Keyboard → Q
Gamepad  → North/Y
```

Exact Input Actions and `PlayerInputReader` integration must be based on the existing asset/API rather than guessed.

## 13. Events

Minimum event:

```text
ActiveWeaponChanged(
    previous WeaponDefinition,
    current WeaponDefinition)
```

Publish only when the active weapon actually changes.

## 14. Player Control / Death

Switching must follow the existing gameplay control/death policy. Dead or disabled players must not switch weapons.

## 15. Runtime State Preservation

Preserve:

```text
magazine ammo
reserve ammo
reload state
reload remaining
fire cooldown
```

Example:

```text
A ammo 36
fire 6 → 30
switch B
switch A
→ A remains 30
```

## 16. Inactive Weapon Tick Policy

Initial policy:

```text
inactive weapons do not tick reload
inactive weapons do not tick fire cooldown
```

Only the active runtime state is ticked.

## 17. Tests

### EditMode

```text
runtime entry validation
loadout entry order
active entry selection
round-robin selection
single/empty behavior
same-StableId different-instance protection
read-only exposure
runtime state identity preservation
ammo/reserve/reload/cooldown preservation
```

### PlayMode

```text
A → B switching
B → A switching
single weapon no-op
ActiveWeaponChanged once
second acquisition remains inactive
controller active definition matches loadout
fire after switch uses active state
player prefab contains switcher
```

## 18. Development Validation

`Test_Waves`:

```text
obtain two weapon definitions
fire A and reduce ammo
switch to B
use B
switch to A
A ammo remains reduced
reload state does not reset
keyboard switching works
Xbox controller switching works
existing wave/ability/reward flows remain correct
Console clean
```

## 19. Implementation Stages

### Stage 1 — Persistent Weapon Runtime Entry
Checkpoint:
```text
feat: add persistent weapon runtime entries
```

### Stage 2 — Controller Equip Boundary
Checkpoint:
```text
feat: support equipping existing weapon runtime
```

### Stage 3 — Acquisition Migration
Checkpoint:
```text
feat: integrate weapon acquisition with persistent runtime entries
```

### Stage 4 — Switching Runtime
Checkpoint:
```text
feat: add player weapon switching runtime
```

### Stage 5 — Input + Prefab Integration
Checkpoint:
```text
feat: integrate weapon switching input
```

### Stage 6 — Development Validation

Only create a checkpoint if new development tooling/content is added.

### Stage 7 — Final Regression

```text
EditMode Run All
PlayMode Run All
manual Test_Waves
Console clean
weapon-switching → dev
```

## 20. Next Layer

After this branch:

```text
Reward Offer
→ Claim
→ second weapon acquired
→ switch between weapons
```

Recommended next branch:

```text
reward-selection-ui
```

Then:

```text
chest-foundation
```
