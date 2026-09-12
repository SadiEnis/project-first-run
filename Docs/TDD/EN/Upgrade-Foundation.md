# Upgrade Foundation

## 1. Purpose

Upgrade Foundation defines how run-scoped upgrades in Project First Run are described as data, acquired under `PlayerBuild` ownership rules, and applied to `PlayerStats` as runtime modifiers.

Core flow:

```text
UpgradeDefinition
      ↓
PlayerUpgradeController
      ├── PlayerBuildController
      └── PlayerStatsController
                ↓
          StatModifier
```

The upgrade system does not directly mutate weapon or ability runtime logic. Numeric effects reach consumers through `PlayerStats`.

## 2. Core Design Principles

Ownership and effect are separate responsibilities:

```text
PlayerBuild → ownership
PlayerStats → runtime numeric effects
PlayerUpgradeController → acquisition orchestration
```

`UpgradeDefinition` is content/configuration data only; it is not runtime state. ScriptableObjects are not mutated at runtime.

The foundation controller does not hard-code concrete upgrades with stable-ID switches.

## 3. Scope

```text
UpgradeDefinition
UpgradeStatModifierData
UpgradeAcquireResult
PlayerUpgradeController
Player prefab integration
EditMode tests
PlayMode integration tests
development upgrade content
weapon + ability stat effect validation
```

## 4. Out of Scope

```text
Chest / Reward generation
Upgrade selection UI
Upgrade level-up
Evolution
Temporary buffs
Conditional effects
On-kill / on-hit proc effects
Health-threshold effects
Save serialization
Meta progression
Reroll
Upgrade removal / replacement UI
```

## 5. UpgradeDefinition

`UpgradeDefinition` derives from `ItemDefinition`.

```text
Category → Upgrade
StableId
DisplayName
Stat Modifiers
```

Initial foundation effects are represented through stat modifier data.

Example:

```text
upgrade.development_damage_boost
├── WeaponDamage +20%
└── AbilityDamage +20%
```

## 6. UpgradeStatModifierData

A small Unity-serializable data structure.

Fields:

```text
PlayerStatType
StatModifierOperation
Value
```

`SourceId` is not duplicated in the asset. Runtime `StatModifier.SourceId` always uses `UpgradeDefinition.StableId`.

## 7. Validation

Configuration error examples:

```text
empty stable ID
invalid stat type
invalid operation
NaN / Infinity value
```

Generic upgrade data does not enforce every stat-specific consumer invariant.

## 8. Acquisition Result

```text
Acquired
AlreadyOwned
CapacityReached
```

Normal gameplay outcomes are not exceptions. Programmer/configuration errors are exceptions.

## 9. PlayerUpgradeController

Dependencies:

```text
PlayerBuildController
PlayerStatsController
```

Main API:

```text
UpgradeAcquireResult TryAcquire(UpgradeDefinition definition)
```

Flow:

```text
1. validate definition
2. create runtime StatModifier instances
3. call PlayerBuild.TryAdd(definition)
4. if not Added, apply no modifiers
5. if Added, add modifiers to PlayerStats
6. publish UpgradeAcquired
```

Modifiers are created before ownership mutation so invalid modifier configuration fails before `PlayerBuild` changes.

## 10. Duplicate Upgrade

Existing `PlayerBuild` stable-ID ownership remains the source of truth.

```text
first acquire → Acquired → modifiers applied once
same StableId again → AlreadyOwned → no stacking
```

## 11. Capacity

Existing PlayerBuild capacity rules remain authoritative.

```text
capacity full
→ CapacityReached
→ PlayerStats unchanged
```

The upgrade system does not maintain a second capacity counter.

## 12. Runtime Modifier Source

All modifiers produced by one upgrade use the same source ID:

```text
UpgradeDefinition.StableId
```

This remains compatible with future `RemoveBySource` cleanup.

## 13. Events

Minimum event:

```text
UpgradeAcquired(UpgradeDefinition definition)
```

Normal failed acquisition outcomes do not require events.

## 14. Player Prefab

```text
Player
├── PlayerBuildController
├── PlayerStatsController
└── PlayerUpgradeController
```

## 15. Development Integration

Development-only content:

```text
Development Damage Boost
├── WeaponDamage +20%
└── AbilityDamage +20%
```

This is not final game content. It validates both existing stat consumers.

## 16. Dependency Direction

```text
Items
  ↓
UpgradeDefinition
  ↓
PlayerUpgradeController
  ├── Builds
  └── Stats

Weapons / Abilities
       ↓
     Stats
```

Upgrade Foundation does not know concrete weapon or Fireball classes.

## 17. Error Handling

Programmer/configuration errors:

```text
null definition
invalid definition data
PlayerBuildController not initialized
PlayerStatsController not initialized
unexpected PlayerBuild result
```

Normal gameplay outcomes:

```text
AlreadyOwned
CapacityReached
```

## 18. Performance

```text
no FindObjectsByType
no required LINQ
no runtime ScriptableObject mutation
modifier allocation only during acquisition
```

## 19. Testing Strategy

### EditMode

```text
UpgradeDefinition category
modifier data validation
runtime modifier creation
source ID from UpgradeDefinition.StableId
multiple modifiers supported
```

### PlayMode

```text
successful acquisition adds ownership
successful acquisition applies modifiers
duplicate acquisition does not stack
capacity reached does not apply modifiers
event publishes once
real Player prefab contains controller
weapon/ability damage reflect acquired modifier
```

## 20. Implementation Stages

### Stage 1 — Upgrade Content Model

```text
UpgradeStatModifierData
UpgradeDefinition
EditMode tests
```

Changeset:

```text
feat: add upgrade content foundation
```

### Stage 2 — Upgrade Runtime Acquisition

```text
UpgradeAcquireResult
PlayerUpgradeController
PlayMode tests
Player prefab integration
```

Changeset:

```text
feat: add player upgrade acquisition runtime
```

### Stage 3 — Development Gameplay Integration

```text
development damage upgrade asset
development bootstrap
weapon damage validation
Fireball damage validation
duplicate/capacity validation
```

Changeset:

```text
feat: integrate development upgrade gameplay
```

### Stage 4 — Final Regression

```text
EditMode Run All
PlayMode Run All
manual Test_Waves validation
Console clean
upgrade-foundation → dev
```

## 21. Next Layer

A natural next system is:

```text
reward-foundation
```

Reward/chest logic can then work with all three item categories:

```text
Weapon
Ability
Upgrade
```

It should delegate ownership and capacity rules to existing PlayerBuild and acquisition runtimes instead of duplicating them.
