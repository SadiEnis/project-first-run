# Player Build Foundation

## 1. Purpose

The Player Build system represents the build owned by the player during a single run.

A build consists of three primary item categories:

- Weapon
- Ability
- Upgrade

The system is not responsible for executing item behaviour. Player Build only manages which items the player owns, how many slots are available for each category, and whether a new item can be added to the build.

This foundation will later become a shared runtime source for:

- Weapon system
- Ability system
- Upgrade system
- Chest / Reward system
- Evolution system
- Run progression
- Meta progression

## 2. Design Goals

The Player Build system must:

1. Represent the player's complete run build in one place.
2. Keep Weapon, Ability and Upgrade categories separate.
3. Support independent slot capacities for each category.
4. Support both starting capacities and capacities increased through meta progression.
5. Prevent the same item from incorrectly occupying multiple slots.
6. Explicitly report when a category has reached its capacity.
7. Remain independent from chest and reward generation.
8. Not execute item behaviour.
9. Use small runtime/domain classes that do not depend on Unity lifecycle.
10. Remain extensible for future level-up and evolution systems.

## 3. Build Categories

The first version contains three build categories:

| Category | Starting Capacity | Maximum Capacity |
|---|---:|---:|
| Weapon | 1 | 2 |
| Ability | 3 | 5 |
| Upgrade | 5 | 8 |

Starting Capacity represents the standard capacity available to a new player at the beginning of a run.

Maximum Capacity represents the upper limit reachable through meta progression.

For example, a player may later start a run with:

```text
Weapons   = 2
Abilities = 4
Upgrades  = 6
```

Player Build does not know where these capacity values came from.

The meta progression system will only provide the appropriate capacity configuration.

## 4. Ownership

Player Build owns:

```text
PlayerBuild
├── Capacity
├── Weapon Entries
├── Ability Entries
└── Upgrade Entries
```

Player Build does not own:

```text
Weapon ammo / reload / cooldown
Ability cooldown / targeting / casting
Stat modifiers
Chest generation
Reward selection
Evolution conditions
Save data
Meta currency
```

Those behaviours remain in their respective systems.

## 5. PlayerBuildCapacity

Slot capacity must not be hard-coded directly inside Player Build.

Capacity is represented by a separate runtime value/model:

```text
PlayerBuildCapacity
├── WeaponSlots
├── AbilitySlots
└── UpgradeSlots
```

Default values:

```text
Weapon  = 1
Ability = 3
Upgrade = 5
```

Maximum values:

```text
Weapon  = 2
Ability = 5
Upgrade = 8
```

Capacity should be immutable after creation.

Changing capacity arbitrarily during a run is outside the scope of this foundation.

In the future, meta progression will provide the correct `PlayerBuildCapacity` when a new run is created.

## 6. Item Identity

Build item identity must not depend on Unity instance reference equality.

Identity should be based on the persistent identity of the related item definition.

Examples:

```text
weapon.shotgun
ability.fireball
upgrade.glass_cannon
```

Items with the same stable identity are considered the same build item.

This decision is important for:

- Reward generation
- Save system
- Evolution requirements
- Duplicate detection
- Content validation

Player Build does not need to understand the gameplay behaviour of the item.

## 7. Adding Items

When an item is requested to be added to the build, the system follows this validation flow:

```text
Is the item valid?
        ↓
Is the category supported?
        ↓
Is the item already owned?
        ↓
Is there an available slot?
        ↓
Add item
```

Normal unsuccessful gameplay operations should not require exceptions.

Expected runtime results should be represented explicitly.

Example results:

```text
Added
AlreadyOwned
CapacityReached
```

Invalid programming or configuration usage may still throw exceptions.

Examples include a null definition or unsupported category.

## 8. Duplicate Item Behaviour

During the foundation stage, receiving the same item again does not add another copy to the build.

For example, if the player already owns:

```text
Fireball
```

a second Fireball must not occupy:

```text
Ability Slot 2
```

Player Build instead reports:

```text
AlreadyOwned
```

Player Build also does not automatically level up the item during this foundation stage.

A future reward system will be able to distinguish:

```text
Item not owned
→ Add new item

Item already owned
→ Item progression / level-up candidate
```

This keeps slot ownership separate from item progression.

## 9. Capacity Behaviour

When a category reaches its capacity, a new item cannot be added.

For example:

```text
Weapon Capacity = 1

Weapon Slot
└── Shotgun
```

A request to add Minigun should produce:

```text
CapacityReached
```

Player Build does not automatically replace or remove an existing item.

If replacement is required later, it should be handled as a separate player choice / reward flow.

## 10. Query API

Other gameplay systems should be able to ask Player Build questions such as:

```text
Does the player own this item?
How many items are in this category?
What is the capacity of this category?
Is there a free slot?
Is this category full?
Which items are currently owned?
```

Query operations must not mutate Player Build state.

Mutable internal collections must not be exposed to external systems.

## 11. Runtime Architecture

Initial architecture:

```text
PlayerBuild
│
├── PlayerBuildCapacity
│
├── Weapon collection
├── Ability collection
└── Upgrade collection
```

Player Build will not be a `MonoBehaviour` or `ScriptableObject`.

It will be a plain C# object representing run-scoped runtime state.

ScriptableObject definition assets may be used as data sources, but the build lifecycle must not depend on Unity scene lifecycle.

## 12. Future Runtime Integration

The future runtime flow is expected to resemble:

```text
Meta Progression
      ↓
PlayerBuildCapacity
      ↓
PlayerBuild created
      ↓
Starting Weapon added
      ↓
Arena starts
      ↓
Chest / Reward
      ↓
PlayerBuild query
      ↓
New Item / Level Up / Evolution
```

Player Build does not generate rewards.

The reward system queries Player Build state and determines which reward candidates are valid.

## 13. Weapon Integration

Player Build stores information such as:

```text
Player owns Shotgun
```

The weapon runtime system continues to own operational state such as:

```text
Ammo
Reload state
Fire cooldown
Trigger state
Active weapon
```

These responsibilities must remain separate.

## 14. Ability Integration

Once Ability Foundation exists, Player Build will provide ownership information such as:

```text
Player owns Fireball
Player owns Drone
```

The ability runtime system will handle:

```text
Cooldown
Target selection
Auto cast
Projectile / behaviour execution
```

## 15. Upgrade Integration

Player Build determines which upgrades the player owns.

Stat effects are not applied inside Player Build.

For example:

```text
PlayerBuild
└── Glass Cannon owned

Upgrade Runtime / Stat System
└── damage modifier applied
```

This separation must be preserved.

## 16. Evolution Integration

Evolution Foundation is outside the scope of this branch.

However, Player Build must later be capable of answering ownership-related queries required by the evolution system.

For example:

```text
Fireball owned?
Required Upgrade owned?
Is Fireball at the required level?
```

The exact ownership model for item level information will be finalized during the item progression stage.

Player Build Foundation does not apply evolution behaviour.

## 17. Save and Meta Progression

Player Build is run-scoped.

It does not own persistent save state.

The future flow will resemble:

```text
Save / Meta Progression
        ↓
Unlocked Capacity
        ↓
Run creation
        ↓
PlayerBuildCapacity
        ↓
PlayerBuild
```

The save system does not need to directly serialize the live runtime `PlayerBuild` instance.

Persistent data should use dedicated save models.

## 18. Invariants

Player Build must always preserve:

```text
Weapon count  <= Weapon capacity
Ability count <= Ability capacity
Upgrade count <= Upgrade capacity

Capacity <= global maximum

The same stable item cannot occupy multiple slots in the same category.

External systems cannot directly mutate internal collections.
```

## 19. Error Handling

Programmer or configuration errors may use exceptions:

```text
Null item definition
Invalid capacity
Unsupported category
Invalid stable identity
```

Normal gameplay results must not use exceptions:

```text
Item already owned
Slot full
```

These are represented using explicit operation results.

## 20. Testing Strategy

Most foundation behaviour will be covered with EditMode tests.

Expected coverage:

```text
Default capacity
Custom valid capacity
Maximum capacity
Invalid capacity

Empty PlayerBuild creation

Add Weapon
Add Ability
Add Upgrade

Duplicate rejection

Weapon capacity reached
Ability capacity reached
Upgrade capacity reached

Contains queries
Count queries
Capacity queries

Collections cannot be mutated externally
```

Required PlayMode integration tests will be added once runtime integration begins.

## 21. Out of Scope

The following are outside the scope of this foundation branch:

```text
Chest UI
Reward generation
Reward selection UI

Ability execution
Ability targeting
Ability cooldown

Upgrade stat modifiers

Item level-up implementation
Evolution implementation

Save system
Meta progression purchase system

Weapon switching UI
Inventory UI

Pooling
Addressables integration
```

## 22. Implementation Stages

### Stage 1 — Domain Foundation

```text
PlayerBuildSlotType
PlayerBuildCapacity
PlayerBuild operation result
PlayerBuild
EditMode tests
```

A first implementation checkpoint should be created after this stage.

Suggested changeset:

```text
feat: add player build domain foundation
```

### Stage 2 — Runtime Integration

Player Build creation during a run and starting-item integration will be implemented.

A second checkpoint should be created after the related integration tests pass.

Suggested changeset:

```text
feat: integrate player build runtime
```

### Stage 3 — Downstream Systems

Future branches will build on the Player Build API:

```text
Ability Foundation
Stat / Upgrade System
Chest / Reward System
Item Progression
Evolution System
```
