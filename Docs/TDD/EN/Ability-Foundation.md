# Ability Foundation

## 1. Purpose

Ability Foundation defines the shared runtime architecture for automatically used abilities owned by the player.

The goal is to combine Vampire Survivors-style automatic abilities with Project First Run's existing FPS combat loop.

The first concrete ability will be Fireball, but this branch will not create a Fireball-specific architecture. Fireball will serve as the first real validation of the generic ability runtime.

Core flow:

```text
PlayerBuild
    ↓ ownership
Ability Runtime
    ↓ cooldown
Target Selection
    ↓
Auto Cast
    ↓
Ability Behaviour
    ↓
Damage / Effect
```

## 2. Design Goals

The ability system must:

1. Separate ability data from mutable runtime state.
2. Consume ability ownership from PlayerBuild.
3. Provide cooldown-based runtime state for automatic casting.
4. Separate target selection from ability execution.
5. Allow significantly different behaviours between abilities.
6. Support Fireball, Drone, Lightning Staff, Shuriken and future abilities on the same foundation.
7. Avoid placing every responsibility inside one monolithic AbilityController.
8. Keep rules that do not require Unity lifecycle in plain C# classes.
9. Integrate with the existing `DamageInfo`, `DamageResult` and `IDamageable` combat foundation.
10. Remain extensible for future stat modifiers, item levels and evolution.

## 3. Scope

This branch targets the generic ability runtime foundation and the infrastructure required for its first real integration.

Planned primary pieces:

```text
AbilityDefinition
AbilityRuntimeConfig
AbilityRuntimeState
PlayerAbilityController
Ability target selection contract
Auto-cast orchestration
Ability execution contract
```

The first concrete validation will use Fireball.

## 4. Out of Scope

The following are not completed in this branch:

```text
Chest / Reward system
Ability reward selection
Ability level-up system
Evolution system
Global stat modifier system
Meta progression
Save system
Ability UI
Cooldown UI
Full VFX / SFX polish
Object pooling
Addressables integration
Boss-specific ability rules
```

Fireball exists only as the minimum concrete implementation required to validate the foundation in gameplay.

## 5. Ability Definition

Abilities should be data-driven.

`AbilityDefinition` should derive from the existing `ItemDefinition`.

```text
ItemDefinition
    ↓
AbilityDefinition
```

`AbilityDefinition.Category` returns `ItemCategory.Ability`.

At foundation level the definition should provide at least:

```text
Stable ID
Display Name
Cooldown
```

Concrete ability-specific fields should not be accumulated inside the generic definition. Fireball may later require damage, projectile speed, lifetime or a projectile prefab.

## 6. Definition and Runtime State Separation

ScriptableObject definitions are configuration data only. Mutable runtime state must live separately.

```text
AbilityDefinition
├── stableId
├── displayName
└── cooldown

AbilityRuntimeState
├── cooldown remaining
└── ready state
```

Definition assets are never mutated during runtime.

## 7. Ability Runtime Config

A definition should be able to produce a pure runtime configuration:

```text
AbilityDefinition
      ↓
CreateRuntimeConfig()
      ↓
AbilityRuntimeConfig
```

`AbilityRuntimeConfig` should be immutable. The first version needs at least cooldown data.

## 8. Ability Runtime State

`AbilityRuntimeState` should be a plain C# class.

Responsibilities:

```text
Cooldown tracking
Ready state
Cast commit
Cooldown reset
```

Flow:

```text
Ready
  ↓ successful cast
CoolingDown
  ↓ Tick(deltaTime)
Ready
```

A normal cast request can return an explicit result such as:

```text
Performed
OnCooldown
```

Failure to find a target does not belong to the cooldown state itself.

## 9. Ownership

Ability ownership source-of-truth is `PlayerBuild`.

```text
PlayerBuild
└── ability.fireball
```

Ability runtime owns the active gameplay state.

```text
PlayerBuild
→ Which abilities does the player own?

Ability Runtime
→ Is this ability ready?
→ What is its cooldown?
→ How is it executed?
```

Ability runtime does not mutate PlayerBuild collections.

## 10. Runtime Ownership

The player's active ability runtime states require one clear runtime owner.

Initial target:

```text
Player
└── PlayerAbilityController
      ├── Ability Runtime Entry
      ├── Ability Runtime Entry
      └── Ability Runtime Entry
```

The controller does not implement every concrete ability behaviour. It owns runtime entries, ticks state and orchestrates casts.

## 11. Ability Runtime Entry

Each active ability runtime entry represents:

```text
Ability Definition
+
Ability Runtime State
+
Ability Execution Behaviour
```

Item level may be added later when item progression exists. Level is not part of this foundation.

## 12. Auto-Cast

Abilities do not depend on manual player input.

```text
Ability ready?
      ↓ yes
Does it require a target?
      ↓
Target Selection
      ↓
Valid target found?
      ↓ yes
Execute
      ↓
Commit cooldown
```

Important rule:

```text
No target
→ No cast
→ No cooldown reset
```

Cooldown starts only after successful execution.

## 13. Target Selection

Target selection remains separate from execution. Not every ability shares the same targeting model.

```text
Fireball       → nearest valid enemy
Lightning      → one or more valid enemies
Shuriken Ring  → may require no target
Drone          → may own its own targeting behaviour
```

The first Fireball integration may use a nearest-enemy selector.

## 14. Enemy Source

A target selector may read active candidates from the existing `EnemyRegistry`.

```text
EnemyRegistry
    ↓ active candidates
Target Selector
    ↓ selected target
Ability Execution
```

The ability system does not modify the registry. Dead or unregistered enemies must not be selected.

## 15. Target Selection Contract

Target selection should use a small and explicit contract.

Core responsibility:

```text
Find a valid target for a given origin
```

A target selector does not manage cooldown, apply damage, spawn projectiles or manage ownership.

## 16. Ability Execution

Concrete ability behaviour should expand through independent execution implementations.

Avoid:

```text
switch (abilityType)
{
    case Fireball:
    case Drone:
    case Lightning:
    ...
}
```

Prefer:

```text
FireballAbilityExecutor
LightningAbilityExecutor
ShurikenAbilityExecutor
```

The orchestration layer does not need concrete execution details.

## 17. Fireball as the First Concrete Ability

Fireball will be the first real gameplay validation.

```text
Cooldown ready
    ↓
Select nearest enemy
    ↓
Spawn projectile
    ↓
Projectile travels toward target
    ↓
Enemy hit
    ↓
IDamageable.ApplyDamage
```

Its purpose in this branch is validating the generic pipeline rather than final content polish.

## 18. Fireball and Projectile Ownership

Projectile behaviour does not belong inside the ability controller.

```text
PlayerAbilityController
    ↓ cast orchestration

Fireball Executor
    ↓ spawn

Fireball Projectile
    ↓ movement / hit

IDamageable
    ↓ damage
```

Projectile movement, hit processing and lifetime remain separate responsibilities.

## 19. Cooldown Semantics

In the first version cooldown means:

```text
seconds between successful casts
```

Cooldown resets only after a successful cast. `CooldownRemaining >= 0` remains invariant and negative delta time is invalid.

## 20. Player Death

Automatic ability casting must stop when the player dies.

Target integration:

```text
PlayerDeathController
      ↓
PlayerAbilityController.SetAbilityControlEnabled(false)
```

Revive/reset behaviour should remain compatible with the existing player lifecycle.

## 21. Arena and Wave Systems

The ability system does not own wave progression.

```text
Ability
→ Enemy Health
→ EnemyController.Died
→ WaveEnemyTracker
→ Wave progression
```

Wave code does not require ability-specific knowledge.

## 22. PlayerBuild Integration

A future reward flow may look like:

```text
Reward
    ↓
PlayerBuild.TryAdd(AbilityDefinition)
    ↓ Added
PlayerAbilityController.AddAbility(...)
```

Chest/reward orchestration is outside this branch.

For Fireball development integration, a starting/test ability may be supplied through development configuration.

## 23. Error Handling

Configuration/programmer failures may use exceptions or validation errors:

```text
Null definition
Invalid cooldown
Missing execution behaviour
Missing projectile prefab
Invalid runtime dependency
```

Normal gameplay outcomes do not use exceptions:

```text
Ability on cooldown
No valid target
Cast not performed
```

## 24. Performance Principles

```text
Do not use FindObjectsOfType every frame.
Use EnemyRegistry as the active candidate source.
Avoid unnecessary LINQ allocations in hot paths.
Never mutate definition assets during runtime.
```

Object pooling remains outside this branch. The first Fireball implementation may use `Instantiate/Destroy`.

## 25. Testing Strategy

### EditMode

```text
AbilityRuntimeConfig validation
AbilityRuntimeState initial state
Cooldown tick
Successful cast commit
OnCooldown result
Cooldown completion
Invalid delta time
Target selector rules where practical
```

### PlayMode

```text
Player ability runtime initializes
Auto-cast occurs
No target → no cast
Nearest enemy selected
Projectile spawns
Projectile damages enemy
Ability kill participates in wave progression
Player death disables auto-cast
```

## 26. Architecture

```text
PlayerBuild
    ↓ ownership

PlayerAbilityController
    ↓ runtime orchestration

AbilityRuntimeState
    ↓ cooldown

Target Selector
    ↓ target

Ability Executor
    ↓ concrete behaviour

Projectile / Effect
    ↓

Combat Foundation
    ↓

Enemy
```

No single class should own the entire pipeline.

## 27. Implementation Stages

### Stage 1 — Ability Domain Foundation

```text
AbilityDefinition
AbilityRuntimeConfig
AbilityRuntimeState
Cast result
EditMode tests
```

Suggested changeset:

```text
feat: add ability runtime foundation
```

### Stage 2 — Targeting Foundation

```text
Target selection contract
Nearest enemy selector
EnemyRegistry integration
Tests
```

Suggested changeset:

```text
feat: add ability target selection
```

### Stage 3 — Auto-Cast Runtime

```text
PlayerAbilityController
Ability runtime entries
Execution contract
Auto-cast orchestration
PlayMode tests
```

Suggested changeset:

```text
feat: add automatic ability runtime
```

### Stage 4 — Fireball Integration

```text
Fireball definition
Fireball executor
Projectile behaviour
Player / development integration
Manual scene validation
```

Suggested changeset:

```text
feat: add fireball ability integration
```

### Stage 5 — Regression and Merge

```text
EditMode Run All
PlayMode Run All
Manual combat validation
Console clean
ability-foundation → dev
```

## 28. Relationship to Future Systems

Ability Foundation becomes a base for:

```text
Item Progression
Stat / Upgrade System
Chest / Reward System
Evolution System
More Ability Content
```

Evolution must be able to replace concrete execution behaviour rather than only modifying numbers.

```text
Fireball
    ↓ evolution
Meteor Rain
```

may use completely different execution behaviour.
