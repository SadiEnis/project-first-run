# Enemy, Wave, and Arena System

## 1. Purpose

This module defines the runtime structure of enemies, target tracking,
scene-owned enemy registration, and the boundaries of the future wave system
in Project First Run.

The first version implements only the Enemy Foundation:

- ScriptableObject-based enemy definitions
- Explicit target assignment
- NavMeshAgent-based movement
- Approaching the player
- Stopping at a configured distance
- Integration with the Combat Foundation
- A scene-owned EnemyRegistry
- Registry removal after death
- A basic chaser enemy prototype

Enemy attacks, wave spawning, spawn budgets, and arena completion rules are
outside the first-version scope.

---

## 2. Design Goals

- Enemy configuration must remain separate from runtime state.
- ScriptableObject assets must not be modified at runtime.
- Enemies must not locate the player through global searches.
- The target Transform must be supplied explicitly during initialization.
- NavMeshAgent usage must remain isolated behind the movement component.
- Health and death rules must use the existing HealthComponent.
- The active enemy collection must belong to the current scene.
- A global EnemyManager or service locator must not be used.
- Death decisions must remain separate from death presentation.
- The structure must support object pooling later.

---

## 3. EnemyDefinition

EnemyDefinition is a ScriptableObject containing immutable enemy configuration.

First-version fields:

- Stable ID
- Display Name
- Maximum Health
- Movement Speed
- Acceleration
- Angular Speed
- Stopping Distance
- Destination Update Interval

EnemyDefinition does not store:

- Current health
- Current target
- NavMesh path
- Death state
- Registry membership
- Active attack cooldown

The initial prototype uses:

`enemy.chaser-basic`

---

## 4. EnemyController

EnemyController coordinates the runtime enemy prefab.

Responsibilities:

- Receive an EnemyDefinition
- Receive a target Transform
- Receive an EnemyRegistry
- Configure HealthComponent
- Configure EnemyMotor
- Register the enemy
- Listen for death
- Stop movement after death
- Unregister the enemy after death

Not responsible for:

- NavMeshAgent movement details
- Damage calculation
- Finding the player
- Producing loot
- Granting experience
- Advancing waves
- Death animation
- Returning the object to a pool

Initialization explicitly receives:

- EnemyDefinition
- Target Transform
- EnemyRegistry

The enemy must not begin gameplay behaviour before initialization completes.

---

## 5. EnemyMotor

EnemyMotor defines the boundary around NavMeshAgent target movement.

Responsibilities:

- Apply navigation settings from EnemyDefinition
- Forward the target position to the agent at a configured interval
- Apply stopping distance
- Enable and stop movement
- Handle invalid or unreachable targets safely

Not responsible for:

- Enemy health
- Enemy death
- Player attacks
- Registry membership
- Enemy animation

The first version approaches the target through NavMeshAgent navigation rather
than direct transform movement.

---

## 6. Target Assignment

An enemy does not search for its own target.

The target is supplied during initialization by one of the following:

- ArenaSceneBootstrap
- EnemySpawner
- TestEnemyBootstrap

The first test scene uses TestEnemyBootstrap to pass the player Transform to the
enemy.

The following methods are not used:

- FindObjectOfType
- FindFirstObjectByType
- GameObject.Find
- Player singleton
- Global service locator

---

## 7. EnemyRegistry

EnemyRegistry is the scene-owned collection of active enemies.

Responsibilities:

- Register initialized active enemies
- Remove dead or disabled enemies
- Expose the active enemy count
- Provide read-only enemy access to future ability and arena systems

EnemyRegistry:

- Does not use DontDestroyOnLoad.
- Is not a static instance.
- Does not spawn enemies.
- Does not manage wave state.
- Does not apply damage.
- Does not select targets.

Registry contents are destroyed with the arena scene.

---

## 8. Health Configuration

Maximum Health from EnemyDefinition is applied to HealthComponent during enemy
initialization.

HealthComponent must support explicit configuration before gameplay begins.

Rules:

- Maximum Health must be finite and greater than zero.
- Configuration occurs before registry registration.
- Configuration recreates HealthState with the supplied maximum.
- The enemy begins at full health.
- Health is not arbitrarily reconfigured after receiving damage.
- Pooled enemies are explicitly initialized again before reuse.

---

## 9. Movement Model

EnemyMotor does not need to rewrite the destination every rendered frame.

Initial value:

`Destination Update Interval: 0.1 seconds`

On each destination update:

1. Validate the target.
2. Confirm that the NavMeshAgent is active and on a NavMesh.
3. Assign the target position as the destination.
4. Allow the agent to move until it reaches Stopping Distance.

Custom avoidance, flocking, and enemy-to-enemy pushing are outside the first
version.

---

## 10. Death Flow

After lethal damage:

1. HealthComponent publishes Died.
2. EnemyController enters the dead state.
3. EnemyMotor stops.
4. NavMeshAgent stops receiving destinations.
5. The enemy is removed from EnemyRegistry.
6. Local death presentation is notified.

EnemyController processes death only once.

The first prototype does not immediately destroy the enemy object. A simple
visual death state may be used.

Returning the enemy to an object pool is deferred.

---

## 11. Initial Chaser Values

| Setting | Initial Value |
|---|---:|
| Maximum Health | 100 |
| Movement Speed | 3.5 |
| Acceleration | 12 |
| Angular Speed | 720 |
| Stopping Distance | 1.5 |
| Destination Update Interval | 0.1 |

These values are for technical validation only.

---

## 12. First-Version Acceptance Criteria

- The enemy is initialized with EnemyDefinition.
- The player target is supplied externally.
- The enemy approaches the player on a valid NavMesh.
- The enemy stops within Stopping Distance.
- The enemy follows when the player changes position.
- The enemy registers with EnemyRegistry after initialization.
- Four 25-damage Plasma Rifle hits kill the enemy.
- Death is processed only once.
- Movement stops after death.
- The enemy unregisters after death.
- A dead enemy rejects additional damage.
- The Console produces no errors or continuous warnings.

---

## 13. Deferred Topics

- Enemy attacks
- Player damage
- Attack cooldowns
- Attack animation
- Hit reactions
- Knockback
- Enemy pooling
- WaveDefinition
- WaveSpawner
- Spawn-point selection
- Spawn budgets
- Elite enemies
- Boss enemies
- Arena completion
- Experience and loot
- Distance-based culling
- Crowd avoidance optimisation
- Alternative navigation models