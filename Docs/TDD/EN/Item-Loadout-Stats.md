# Item-Loadout-Stats

# Item, Loadout, and Stat Architecture

## 1. Purpose

This document defines the technical structure used to represent items, store the player's run loadout, reconstruct equipment between arena scenes, calculate player statistics, and apply passive upgrades and traits.

The architecture must support:

- Manually controlled weapons.
- Automatically activated abilities.
- Passive stat upgrades.
- Gameplay-changing traits.
- Item levels.
- Equipment slot limits.
- Item evolutions.
- Chest reward filtering.
- Runtime reconstruction between arenas.
- Permanent slot upgrades.

The system should remain explicit and strongly typed without becoming a universal item or effect framework.

Class names in this document are working names and may change during implementation. Their responsibilities are more important than their exact names.

---

## 2. Design Goals

The item, loadout, and stat architecture should:

- Keep ScriptableObject definitions separate from mutable runtime state.
- Store persistent run equipment through stable item identifiers.
- Allow the player runtime to be safely rebuilt after an arena transition.
- Make chest reward eligibility easy to query.
- Prevent duplicate or invalid equipment states.
- Allow evolutions to replace an item in the same slot.
- Support visible numerical and behavioral progression.
- Keep stat changes and behavior-changing traits separate.
- Avoid deep inheritance hierarchies.
- Avoid one universal effect system for every item.
- Remain testable without loading a complete gameplay scene.

---

## 3. Core Concepts

The architecture separates five concepts:

```
Item Definition
    Static content data stored as ScriptableObject assets

Loadout State
    Persistent run data containing item IDs, levels, and slot positions

Item Runtime
    Scene-local weapon, ability, or trait behavior

Stat Collection
    Scene-local calculated player statistics

Presentation
    Models, animation, audio, VFX, and UI
```

Example:

```
ShotgunDefinition
    Static weapon configuration
        ↓
LoadoutEntry
    Item ID: weapon.shotgun
    Level: 5
        ↓
ShotgunRuntime
    Current ammunition and reload state
        ↓
ShotgunView
    Model, animation, sound, and VFX
```

These concepts may not always require separate classes, but their ownership must remain clear.

---

## 4. Item Categories

The initial item categories are:

```
public enum ItemCategory
{
    Weapon,
    Ability,
    Upgrade
}
```

The category determines:

- Which slot pool the item uses.
- Which chest types may offer the item.
- Which runtime construction path is used.
- Which level and evolution rules are valid.

Stat upgrades and traits are both members of the `Upgrade` category. They do not use separate player-facing slot pools.

---

## 5. Item Definition Model

Item definitions are immutable ScriptableObject assets.

A shallow shared definition hierarchy is justified because all item categories require common identity and presentation data.

Conceptual structure:

```
ItemDefinition
├── WeaponDefinition
├── AbilityDefinition
└── UpgradeDefinition
```

### 5.1 Common Item Data

`ItemDefinition` may contain:

```
Stable ID
Display Name
Description
Icon
Item Category
Normal Reward Pool Eligibility
Level Count
```

The stable ID is used by:

- Run state.
- Save data.
- Item catalogs.
- Evolution definitions.
- Chest rewards.
- Debugging and analytics.

Example IDs:

```
weapon.shotgun
weapon.minigun
weapon.rocket_launcher
ability.force_wave
ability.fireball
upgrade.damage
```

Display names must never be used as persistent identifiers.

### 5.2 Category-Specific Data

`WeaponDefinition` may contain:

- Weapon runtime prefab.
- Weapon level data.
- Magazine configuration.
- Firing configuration.
- Weapon presentation references.

`AbilityDefinition` may contain:

- Ability runtime prefab.
- Ability level data.
- Cooldown configuration.
- Targeting configuration where shared.
- Ability presentation references.

`UpgradeDefinition` may contain:

- Upgrade kind.
- Upgrade level data.
- Stat modifier specifications.
- Optional trait runtime information.

Unique behaviors may use more focused definition subclasses when strongly typed configuration is required.

Examples:

```
ShotgunDefinition : WeaponDefinition
RocketLauncherDefinition : WeaponDefinition
DroneDefinition : AbilityDefinition
```

A dedicated subclass should only be created when the item has configuration that does not belong in the common category definition.

---

## 6. Item Level Data

### 6.1 Level Representation

Item level data should use cumulative snapshots rather than incremental mutations.

A level snapshot describes the final configuration of the item at that level.

Example:

```
Shotgun Level 5
├── Damage: final level-5 damage
├── Magazine Size: final level-5 capacity
├── Pellet Count: final level-5 count
├── Knockback: final level-5 value
└── Reload Duration: final level-5 duration
```

This is preferred over replaying every previous level change.

Benefits:

- An item can be reconstructed directly at any level.
- Arena transitions do not require replaying upgrade history.
- Save migration becomes simpler.
- Level behavior is easier to test.
- The final value does not depend on upgrade application order.

Editor tooling may later copy values from the previous level to reduce repetitive data entry. Runtime data remains cumulative.

### 6.2 Level Rules

- Base items begin at level 1.
- A normal item can level up until its definition's maximum level.
- Maximum level is derived from the number of valid level snapshots.
- Level 0 is not a valid owned-item state.
- An evolved item currently has one level and cannot level further.
- Maximum-level items are excluded from normal level-up rewards.

### 6.3 Reward Descriptions

Each level may contain a player-facing upgrade description.

Example:

```
Level 8:
“Adds additional pellets.”
```

The description should explain the gameplay change rather than expose implementation details.

---

## 7. Normal Reward Eligibility

Not every item definition should appear in ordinary chest reward pools.

An item definition should contain a simple normal-reward eligibility setting.

Examples:

| Item | Normal Reward Pool |
| --- | --- |
| Shotgun | Included |
| Fireball | Included |
| Damage Upgrade | Included |
| Meteor Evolution | Excluded |
| Evolved Shotgun | Excluded |

Evolution result items are acquired only through the evolution system.

The project will not initially create a complex acquisition-rule graph. More detailed unlock conditions may be added later through profile progression filtering.

---

## 8. Item Catalog

`ItemCatalog` is an application-lifetime catalog containing all registered item definitions.

Its responsibilities are:

- Resolve stable item IDs into definitions.
- Provide category-based item queries.
- Detect duplicate item IDs.
- Detect missing definitions.
- Support reward-pool filtering.
- Support future Addressables integration.

Conceptual lookup:

```
"weapon.shotgun"
        ↓
ItemCatalog
        ↓
ShotgunDefinition
```

The initial catalog may hold direct ScriptableObject references.

Addressables must not be introduced into the catalog until there is a demonstrated loading or memory-management requirement.

### 8.1 Catalog Rules

- Every item ID must be unique.
- Every registered definition must have a non-empty ID.
- The category stored by a definition must match its runtime type.
- Evolution result definitions must exist in the catalog.
- Invalid catalog data should fail validation during development.

---

## 9. Loadout State

`LoadoutState` belongs to `RunSession`.

It is a pure C# data model and contains no scene-object references.

Conceptual structure:

```
LoadoutState
├── Capacities
│   ├── Weapon Slots
│   ├── Ability Slots
│   └── Upgrade Slots
│
├── Weapons
├── Abilities
└── Upgrades
```

Each owned item is represented by a `LoadoutEntry`.

```
LoadoutEntry
├── Item ID
└── Level
```

The order of entries represents the player's slot order.

This is especially important for weapons because weapon switching may depend on slot order.

An evolution replaces the entry at the same list index.

---

## 10. Slot Capacity

Slot capacities are determined when a run begins.

They are calculated from:

```
Default Slot Capacity
        +
Permanent Profile Slot Upgrades
        ↓
Run Slot Capacity Snapshot
```

The snapshot is stored in `LoadoutState`.

Initial proposed capacities remain design values:

| Category | Starting Capacity | Proposed Maximum |
| --- | --- | --- |
| Weapons | 1 | 2 |
| Abilities | 3 | 5 |
| Upgrades | 5 | 8 |

These values may change after playtesting.

Slot capacity does not change during a run unless a future gameplay feature explicitly introduces temporary slot changes.

---

## 11. Run Loadout Domain Object

A pure C# domain object, working name `RunLoadout`, owns loadout rules and mutations.

It operates on `LoadoutState`.

Responsibilities:

- Query whether an item is owned.
- Query the current item level.
- Query remaining category capacity.
- Validate whether a new item can be acquired.
- Validate whether an item can level up.
- Add a new item.
- Increase an item's level.
- Replace an item through evolution.
- Preserve slot ordering.
- Reject invalid loadout mutations.

It must not:

- Instantiate GameObjects.
- Update UI.
- Play effects.
- Generate chest rewards.
- Load item assets.
- Control weapon switching.

---

## 12. Loadout Invariants

The following rules must always remain true:

1. An item ID may appear only once in the player's loadout.
2. An item must be stored in the collection matching its category.
3. The number of entries may not exceed category capacity.
4. An item's level must remain between 1 and its maximum level.
5. An evolved result must replace its source in the same slot.
6. A weapon may only evolve into another weapon.
7. An ability may only evolve into another ability.
8. Upgrade items are not currently evolution sources.
9. An evolution result may not already exist elsewhere in the loadout.
10. Invalid operations must not partially mutate the loadout.

Evolution replacement must be atomic: either the complete replacement succeeds or the loadout remains unchanged.

---

## 13. Item Acquisition Flow

Chest rewards do not directly modify scene runtime objects.

The normal acquisition flow is:

```
Player selects reward
        ↓
Reward selection is validated
        ↓
RunLoadout mutation is performed
        ↓
Loadout mutation result is created
        ↓
Scene runtime is synchronized
        ↓
Player UI is updated
        ↓
Feedback is played
```

Possible mutation results include:

```
ItemAdded
ItemLevelIncreased
ItemEvolved
RejectedNoCapacity
RejectedMaximumLevel
RejectedDuplicate
RejectedInvalidCategory
RejectedInvalidDefinition
```

The exact implementation may use a result enum and optional result data.

---

## 14. Reward Eligibility Queries

The chest system must be able to inspect the loadout without mutating it.

Required queries include:

```
CanAcquireNewItem(definition)
CanLevelUpItem(itemId)
IsItemOwned(itemId)
IsItemAtMaximumLevel(itemId)
HasAvailableSlot(category)
```

The chest reward system uses these queries to:

- Remove maximum-level items.
- Remove already-owned items from new-item candidates.
- Remove new items when category slots are full.
- Detect when gold fallback is required.
- Determine which evolution results are valid.

Reward filtering belongs to the reward system, while ownership and capacity rules belong to `RunLoadout`.

---

## 15. Item Runtime Reconstruction

When an arena scene loads, the player's runtime equipment is rebuilt from `LoadoutState`.

Conceptual sequence:

```
Arena loads
    ↓
PlayerLoadoutController receives LoadoutState
    ↓
Resolve each Item ID through ItemCatalog
    ↓
Instantiate weapon and ability runtimes
    ↓
Apply current item level snapshots
    ↓
Apply upgrade stat modifiers
    ↓
Create active trait runtimes
    ↓
Update presentation and HUD
```

`PlayerLoadoutController` is a scene-lifetime coordinator.

Its responsibilities are:

- Build scene runtime equipment from loadout state.
- Keep runtime objects synchronized after an item mutation.
- Maintain weapon slot ordering.
- Create and remove item runtime objects.
- Connect runtime items to player dependencies.

It must not become the authoritative owner of item IDs or levels. That ownership remains in `LoadoutState`.

---

## 16. Scene-Local Item Runtime State

The following values belong to scene-local item runtimes:

### Weapon Runtime State

- Current magazine ammunition.
- Reload progress.
- Fire cooldown.
- Recoil state.
- Temporary weapon effects.
- Current weapon presentation state.

### Ability Runtime State

- Current cooldown.
- Active duration.
- Current targets.
- Spawned persistent ability objects.
- Temporary behavior state.

### Trait Runtime State

- Active condition state.
- Registered event subscriptions.
- Temporary modifier handles.
- Internal cooldowns where required.

These values are not stored inside ScriptableObject definitions.

Unless a later design decision changes this behavior, ammunition, cooldown progress, and temporary item state reset when entering a new arena.

---

## 17. Evolution Definitions

Evolution recipes are stored separately from item definitions.

Conceptual structure:

```
EvolutionDefinition
├── Stable Evolution ID
├── Source Item
├── Requirements
└── Result Item
```

Each requirement contains:

```
Required Item ID
Required Level
```

This supports conditions such as:

```
Fireball Level 8
+
Cooldown Upgrade Level 6
        ↓
Meteor
```

Whether supporting multiple requirements is needed for the initial content will be reviewed. The data model may use a small list because multiple requirements are already a plausible game-design need.

### 17.1 Evolution Validation

An evolution is eligible only when:

- The source item is owned.
- The source item meets its required level.
- Every additional requirement is satisfied.
- The result item exists.
- The result category matches the source category.
- The result item is not already owned.
- The source item occupies a valid slot.

### 17.2 Evolution Mutation

When evolution is selected:

```
Find source slot
    ↓
Remove source entry
    ↓
Insert result entry at the same index
    ↓
Set result level to 1
    ↓
Rebuild affected runtime object
```

The evolved definition currently has a maximum level of 1.

---

## 18. Stat System Overview

The stat system handles numerical player properties shared across multiple gameplay systems.

Examples:

- Maximum health.
- Armor.
- Movement speed.
- Global damage.
- Weapon fire rate.
- Reload speed.
- Ability cooldown recovery.
- Pickup radius.
- Experience gain.
- Luck.

The stat system must not contain:

- Current health.
- Current ammunition.
- Current experience.
- Item levels.
- Projectile count.
- Shotgun pellet count.
- Active enemy status effects.

These values belong to their own runtime systems or item-level configurations.

---

## 19. Initial Stat Identifiers

The initial type-safe stat set is:

```
public enum StatId
{
    MaxHealth,
    Armor,
    MovementSpeed,
    GlobalDamageMultiplier,
    WeaponFireRateMultiplier,
    ReloadSpeedMultiplier,
    AbilityCooldownRate,
    PickupRadius,
    ExperienceGainMultiplier,
    Luck
}
```

Additional stats should be added only when a real gameplay requirement exists.

Weapon-specific values such as critical chance, magazine size, projectile speed, or explosion radius should remain inside weapon level data unless they become genuinely shared player statistics.

---

## 20. Stat Semantics

Stats must have clear meanings and units.

| Stat | Meaning |
| --- | --- |
| MaxHealth | Maximum player health |
| Armor | Input used by the damage-mitigation formula |
| MovementSpeed | Player movement speed in world units per second |
| GlobalDamageMultiplier | Multiplier applied to eligible player damage |
| WeaponFireRateMultiplier | Multiplier applied to weapon firing rate |
| ReloadSpeedMultiplier | Reload-speed multiplier; higher values mean faster reloads |
| AbilityCooldownRate | Cooldown recovery multiplier; higher values mean shorter cooldowns |
| PickupRadius | Radius used to attract or collect pickups |
| ExperienceGainMultiplier | Multiplier applied to gained experience |
| Luck | Input used by explicitly supported probability systems |

Using cooldown recovery rate avoids negative or invalid cooldown durations.

Example:

```
Base Cooldown: 10 seconds
Cooldown Rate: 1.25
Effective Cooldown: 10 / 1.25 = 8 seconds
```

---

## 21. Base Stat Snapshot

When a run begins:

```
Default Player Stats
        +
Permanent Profile Upgrades
        ↓
Run Base Stat Snapshot
```

The snapshot is stored in `RunSession`.

It remains stable for the duration of the run.

Scene runtime creates a `StatCollection` from this snapshot when the player is created.

Permanent profile data is not queried repeatedly during combat.

---

## 22. Stat Modifiers

A stat modifier contains:

```
Stat ID
Operation
Value
Source ID
Modifier Key
```

Initial modifier operations:

```
public enum StatModifierOperation
{
    Flat,
    AdditivePercent
}
```

Initial calculation:

```
Final Value =
    (Base Value + Sum of Flat Modifiers)
    ×
    (1 + Sum of Additive Percentage Modifiers)
```

Example:

```
Base Max Health: 100
Flat Modifier: +20
Percentage Modifier: +15%

Final Max Health:
(100 + 20) × 1.15 = 138
```

A separate multiplicative modifier phase should not be introduced until balancing demonstrates a real need.

---

## 23. Modifier Sources

Every modifier must have an identifiable source.

Examples:

```
upgrade.damage
upgrade.heavy_armor
trait.berserker.active
temporary.arena_blessing
```

The combination of source ID and modifier key allows the system to:

- Replace a modifier when an item levels up.
- Remove modifiers when a condition ends.
- Prevent accidental duplicate application.
- Explain final values during debugging.
- Reconstruct item-derived modifiers between arenas.

A modifier registration may return a handle for efficient removal.

---

## 24. Stat Collection

`StatCollection` is a pure C# scene-runtime object.

Responsibilities:

- Store base stat values.
- Add modifiers.
- Remove modifiers.
- Replace source modifiers.
- Calculate final stat values.
- Clamp values to valid ranges.
- Notify local consumers when relevant stats change.
- Provide debugging information about modifier sources.

The collection should use dirty-value caching.

A stat is recalculated only when:

- Its base value changes.
- A modifier affecting it is added.
- A modifier affecting it is removed.
- A modifier affecting it is replaced.

Stats must not be fully recalculated every frame.

---

## 25. Stat Value Constraints

Invalid final values must be prevented.

Examples:

- Maximum health must remain above zero.
- Movement speed must not become negative.
- Fire-rate multiplier must remain above a safe minimum.
- Reload-speed multiplier must remain above a safe minimum.
- Cooldown rate must remain above a safe minimum.
- Pickup radius must not become negative.
- Experience gain multiplier must not become negative.

Exact minimum and maximum values will be determined during balancing.

Clamping rules should be centralized rather than repeated by individual consumers.

---

## 26. Applying Stat Upgrades

A stat upgrade level contains the final modifier specification for that level.

Example:

```
Damage Upgrade
Level 1: +10%
Level 2: +20%
Level 3: +30%
```

When the upgrade increases from level 2 to level 3:

```
Remove or replace modifier from upgrade.damage
        ↓
Apply level-3 snapshot: +30%
```

The system must not add another +30% on top of the previous +20%.

This keeps item level data cumulative and prevents stacking errors.

---

## 27. Trait Upgrades

Traits change behavior and are not represented only by stat values.

Examples:

- Berserker.
- Vampire.
- Ammo Expert.
- Lucky.
- Blood Pact.
- Glass Cannon.
- Heavy Armor.

A trait may contain both:

- Persistent stat modifiers.
- Dedicated runtime behavior.

Example:

```
Heavy Armor
├── Armor modifier
└── Movement-speed penalty
```

Example:

```
Berserker
├── Observes current health ratio
├── Activates below a threshold
├── Adds a temporary damage modifier
└── Removes the modifier when the condition ends
```

Example:

```
Vampire
├── Observes eligible enemy defeats
├── Applies healing according to its rules
└── May use an internal cooldown or probability
```

---

## 28. Trait Runtime Boundary

The exact runtime construction of traits will be defined in the Upgrade and Trait System document.

This module establishes the following rules:

- Trait definitions remain immutable.
- Trait runtime state is scene-local.
- Trait runtime instances may subscribe only to explicit local events.
- Trait subscriptions must be removed when the runtime is destroyed.
- Trait-derived modifiers must use identifiable modifier sources.
- Trait behavior must not modify ScriptableObject assets.
- Trait runtime objects are reconstructed from loadout state after an arena transition.

The project will not initially create one universal trait interpreter.

Focused trait implementations are allowed when behavior is genuinely different.

---

## 29. Item-Derived and Temporary Modifiers

Modifiers derived directly from owned upgrades are not stored separately as authoritative run data.

They can be reconstructed from:

```
Loadout Entry
+
Upgrade Definition
+
Current Upgrade Level
```

`RunSession` may store only modifiers that:

- Are not represented by an owned item.
- Must survive arena transitions.
- Were created by a temporary run-wide effect.

This prevents the same modifier from being stored both in the loadout and in a separate modifier list.

This section refines the earlier `RunSession.StatState` proposal.

---

## 30. Runtime Synchronization

When a loadout mutation occurs inside an arena:

```
RunLoadout state changes
        ↓
PlayerLoadoutController receives mutation result
        ↓
Affected runtime is added, updated, removed, or replaced
        ↓
Stat modifiers are refreshed when required
        ↓
Trait runtimes are refreshed when required
        ↓
HUD is updated
```

The system should update only the affected category or item rather than reconstructing every runtime object after every reward.

Full reconstruction is used when entering a new arena.

---

## 31. Local Events

The loadout system may expose local C# events such as:

```
ItemAdded
ItemLevelChanged
ItemEvolved
LoadoutRebuilt
```

These are not global events.

Possible listeners include:

- Player loadout runtime synchronization.
- HUD.
- Feedback presentation.
- Development debugging tools.

Mutation must occur before the event is raised.

Events report completed state changes; they do not request the mutation.

---

## 32. Editor Validation

The project should validate item content during development.

Validation includes:

- Empty item IDs.
- Duplicate item IDs.
- Missing icons or runtime prefabs where required.
- Empty level lists.
- Invalid level counts.
- Missing level descriptions.
- Invalid category assignments.
- Evolution category mismatches.
- Missing evolution requirements.
- Evolution results incorrectly enabled in normal reward pools.
- Invalid stat modifier values.
- Duplicate modifier keys inside one level.

Validation may use:

- `OnValidate`.
- Catalog validation tools.
- Edit Mode tests.
- Custom editor warnings when justified.

---

## 33. Testing Strategy

Pure C# Edit Mode tests should cover:

### Loadout Tests

- Adding an item with available capacity.
- Rejecting an item when slots are full.
- Rejecting duplicate ownership.
- Increasing an owned item's level.
- Rejecting a maximum-level upgrade.
- Preserving slot order.
- Replacing an item through evolution.
- Rejecting invalid evolution categories.
- Ensuring failed mutations leave state unchanged.

### Stat Tests

- Flat modifier calculation.
- Additive percentage calculation.
- Combined modifier calculation.
- Replacing modifiers from the same source.
- Removing conditional trait modifiers.
- Applying stat constraints.
- Preventing duplicate modifier application.
- Reconstructing stats from loadout state.

### Catalog Tests

- Resolving known item IDs.
- Rejecting duplicate IDs.
- Detecting missing definitions.
- Validating evolution references.

Play Mode tests should verify:

- Runtime item creation.
- Weapon slot ordering.
- Runtime replacement after evolution.
- Trait cleanup after scene destruction.
- Reconstruction after arena loading.

---

## 34. Performance Considerations

The loadout and stat architecture should avoid frequent allocations.

Guidelines:

- Do not use LINQ in frequently repeated gameplay paths.
- Cache catalog dictionaries after initialization.
- Recalculate stats only when modifiers change.
- Do not search scene objects to locate item runtimes.
- Do not rebuild the complete loadout after each ordinary stat change.
- Avoid reflection-based runtime item construction.
- Pool projectiles and temporary item effects separately from item definitions.
- Keep reward filtering outside frame-update loops.

The total number of equipped items is small, so clarity is more important than complex data structures.

---

## 35. Open Technical Questions

The following questions remain unresolved:

- What item does the player begin a new run with?
- Does the player choose the starting weapon?
- Should current health increase when maximum health increases?
- Should current health scale proportionally when maximum health decreases?
- Can the player voluntarily remove or replace a non-evolved item?
- Can an item have more than one possible evolution?
- Must every evolution requirement be at maximum level?
- Can permanent progression unlock items that are initially unavailable?
- Should upgrade level data always contain one cumulative modifier snapshot?
- Which Lucky-related probability systems will consume the Luck stat?
- Should temporary run-wide modifiers survive every arena transition?
- Will weapon ammunition or ability cooldowns ever be preserved between arenas?
- Is a dedicated player stat debugging panel required for development builds?

---

## 36. Confirmed Technical Decisions

| Topic | Decision |
| --- | --- |
| Definition storage | Immutable ScriptableObject assets |
| Runtime persistence | Stable item IDs and levels |
| Definition hierarchy | Shallow category-based hierarchy |
| Unique behavior data | Focused subclasses only when required |
| Level data | Cumulative level snapshots |
| Loadout ownership | `LoadoutState` inside `RunSession` |
| Loadout rules | Pure C# `RunLoadout` domain object |
| Slot ordering | Preserved by ordered category collections |
| Duplicate items | Not allowed |
| Runtime reconstruction | Rebuilt from loadout on arena entry |
| Evolution | Atomic same-slot item replacement |
| Evolved item level | Level 1 with no further levels |
| Upgrade slots | Shared by stat upgrades and traits |
| Stat identifiers | Type-safe enum |
| Stat operations | Flat and additive percentage initially |
| Stat recalculation | Dirty-value caching |
| Trait behavior | Dedicated runtime behavior where required |
| Universal effect graph | Not used initially |
| Normal reward eligibility | Explicitly defined per item |
| Item-derived modifiers | Reconstructed from loadout |
| Global loadout events | Not used |
| Validation | Editor and automated validation |