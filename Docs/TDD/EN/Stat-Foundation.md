# Stat Foundation

## 1. Purpose

Stat Foundation defines the shared runtime layer that lets player-facing numeric values in Project First Run be modified safely and consistently by upgrades, buffs, debuffs and future meta progression systems.

This branch does not implement the Upgrade system itself. It first establishes the independent stat layer that Upgrade Foundation will depend on.

Core idea:

```text
Domain Base Value
      ↓
PlayerStatCollection
      ↓ modifiers
Final Runtime Value
```

Base-value ownership remains in the existing domain systems.

Example:

```text
WeaponDefinition.BaseDamage
        +
Player stat modifiers
        ↓
Final Runtime Damage
```

The stat system is not the source-of-truth for weapon, ability or movement configuration.

## 2. Core Design Decision

The stat system does not duplicate existing base player values. Each consumer supplies its own base value:

```text
stats.Evaluate(PlayerStatType.WeaponDamage, weaponBaseDamage)
```

Example ownership:

```text
WeaponDefinition → base weapon damage
Concrete AbilityDefinition → base ability values
PlayerMotor / movement config → base movement values
Health config → base health value
PlayerStatCollection → modifiers only
```

## 3. Scope

```text
PlayerStatType
StatModifierOperation
StatModifier
PlayerStatCollection
PlayerStatsController
EditMode tests
PlayMode owner tests
initial consumer integration points
```

## 4. Out of Scope

```text
UpgradeDefinition
Upgrade acquisition
Chest / Reward
Upgrade UI
Upgrade level-up
Evolution
Meta progression
Save
Temporary timed buffs
Conditional perks
Kill-triggered perks
Complex proc systems
```

## 5. PlayerStatType

The initial version includes only stats with near-term consumers:

```text
WeaponDamage
AbilityDamage
MoveSpeed
WeaponFireRate
ReloadSpeed
AbilityCooldown
MaxHealth
Luck
```

New stat types are added only when a real consumer requires them.

## 6. Modifier Operations

Supported operations:

```text
Flat
AdditivePercent
MultiplicativePercent
```

Percent values use fractional representation:

```text
+10% → 0.10
-20% → -0.20
```

Deterministic evaluation order:

```text
base
+ sum of flat modifiers
× (1 + sum of additive percentages)
× each (1 + multiplicative percentage)
```

Example:

```text
Base = 100
Flat = +20
Additive = +25%
Multiplicative = +10%

(100 + 20) × 1.25 × 1.10 = 165
```

## 7. StatModifier

`StatModifier` is an immutable plain C# value object containing:

```text
PlayerStatType
StatModifierOperation
Value
SourceId
```

Example source IDs:

```text
upgrade.glass_cannon
buff.arena_frenzy
meta.damage_01
```

Source identity later allows all modifiers from one source to be removed together.

## 8. Source Identity

```text
null / empty source id → invalid
leading/trailing whitespace → invalid
comparison → ordinal, case-sensitive
```

Unity object references are not modifier identity.

## 9. PlayerStatCollection

Plain C# runtime owner.

Responsibilities:

```text
add modifier
remove modifier
remove by source
evaluate stat
expose read-only modifiers
```

It does not own:

```text
PlayerBuild ownership
Upgrade acquisition
ScriptableObject loading
UI
Save
Ability execution
Weapon firing
Movement
Health lifecycle
```

## 10. Evaluation

Core API:

```text
float Evaluate(PlayerStatType statType, float baseValue)
```

`baseValue` must be finite. A NaN or Infinity result is a configuration/programmer error.

The generic stat layer does not clamp values positive automatically. Domain-specific invariants belong to the consumer.

## 11. Duplicate Modifiers

One source may provide multiple modifiers:

```text
upgrade.example
├── WeaponDamage +20%
└── AbilityDamage +20%
```

Duplicate upgrade ownership is handled later by Upgrade Foundation.

## 12. Removal

```text
Remove(modifier)
RemoveBySource(sourceId)
```

Removing a missing modifier/source is a normal runtime outcome.

## 13. PlayerStatsController

Unity runtime owner:

```text
Player
└── PlayerStatsController
      └── PlayerStatCollection
```

The controller creates and exposes the collection. It does not own upgrade behaviours or concrete effects.

## 14. Existing Systems Integration

### Weapon

```text
WeaponDefinition.BaseDamage
      ↓
Evaluate WeaponDamage
      ↓
DamageInfo
```

### Ability

```text
FireballDefinition.Damage
      ↓
Evaluate AbilityDamage
      ↓
FireballProjectile
```

### Ability Cooldown

Later:

```text
AbilityDefinition.Cooldown
      ↓
Evaluate AbilityCooldown
      ↓
AbilityRuntimeConfig
```

### Movement / Health

MoveSpeed and MaxHealth integration can wait until a real Upgrade consumer requires them.

## 15. Dependency Direction

```text
Combat / Weapon / Ability / Player systems
            ↓
      Stat Foundation
```

Stat Foundation does not know concrete weapons, Fireball, enemies or upgrades.

Upgrade Foundation later depends on Stat Foundation:

```text
UpgradeDefinition
       ↓
StatModifier
       ↓
PlayerStatCollection
```

## 16. Error Handling

Programmer/configuration errors:

```text
invalid source id
NaN / Infinity modifier value
NaN / Infinity base value
unsupported operation
non-finite evaluation result
```

Normal runtime outcomes:

```text
modifier not found
no modifiers for source
```

## 17. Performance

```text
no FindObjectsByType in hot paths
no LINQ allocation in hot paths
never mutate ScriptableObject assets at runtime
do not allocate new modifiers every frame
```

Caching is added only when profiling proves it necessary.

## 18. Testing Strategy

### EditMode

```text
modifier validation
flat evaluation
additive percent evaluation
multiplicative percent evaluation
mixed operation order
multiple modifiers
stat-type isolation
remove exact modifier
remove by source
invalid base value
non-finite result protection
read-only exposure
```

### PlayMode

```text
PlayerStatsController owns one collection
normal Unity lifecycle
real player prefab contains PlayerStatsController
consumer integration uses evaluated value
```

## 19. Implementation Stages

### Stage 1 — Pure Stat Domain

```text
PlayerStatType
StatModifierOperation
StatModifier
PlayerStatCollection
EditMode tests
```

Changeset:

```text
feat: add player stat runtime foundation
```

### Stage 2 — Unity Runtime Owner

```text
PlayerStatsController
Player prefab integration
PlayMode tests
```

Changeset:

```text
feat: add player stat runtime owner
```

### Stage 3 — Combat Consumer Integration

First validation:

```text
WeaponDamage
AbilityDamage
```

Existing weapon and Fireball base damage values remain the source values.

Changeset:

```text
feat: integrate combat damage stats
```

### Stage 4 — Regression

```text
EditMode Run All
PlayMode Run All
manual weapon damage test
manual Fireball damage test
Console clean
stat-foundation → dev
```

## 20. Next Branch

```text
upgrade-foundation
```

Upgrade Foundation builds on Stat Foundation with:

```text
UpgradeDefinition
stat modifier data
PlayerBuild ownership
upgrade runtime application
duplicate/capacity rules
first concrete upgrade validation
```
