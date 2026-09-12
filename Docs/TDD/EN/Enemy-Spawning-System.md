# Enemy Spawning System

## 1. Purpose

This module manages the runtime creation of enemy prefabs and initializes them
with their required gameplay dependencies.

Enemy creation is currently performed only by an editor test bootstrap.
The Enemy Spawning System turns that operation into a reusable runtime service.

The first version covers:

- Instantiating an enemy prefab at a specified position and rotation
- Initializing EnemyController
- Initializing EnemyAttackController
- Supplying the player target, target health, and EnemyRegistry dependencies
- Returning the spawned enemy to the caller
- Supporting use with one or multiple spawn points
- Rejecting invalid dependencies with explicit errors
- Cleaning up partially created instances after failed initialization

Wave management, timed spawning, and object pooling are outside the scope of this
version.

---

## 2. Design Goals

- EnemySpawner must not control enemy AI behaviour.
- EnemySpawner must not own enemy health or attack decisions.
- Spawn dependencies must be supplied explicitly.
- EnemySpawner must not search the scene automatically for the Player.
- `FindObjectOfType`, global singletons, and service locators must not be used.
- A prefab with missing components must fail with a clear error.
- Failed initialization must not leave a partial enemy instance in the scene.
- EnemyController and EnemyAttackController must retain their existing
  responsibilities.
- Future object pooling should be able to reuse the same spawn contract.

---

## 3. Core Components

### EnemySpawnRequest

Represents the information required to create an enemy.

It contains:

- EnemyController prefab
- EnemyDefinition
- Target Transform
- Target IDamageable
- EnemyRegistry
- Spawn position
- Spawn rotation

The request validates its data and prevents invalid dependencies from reaching the
spawn operation.

### EnemySpawnResult

Represents the result of a successful spawn operation.

It contains:

- The spawned EnemyController
- The spawned EnemyAttackController
- The spawned GameObject

The result allows wave and arena systems to access the created instance through an
explicit contract.

### EnemySpawner

EnemySpawner is the runtime MonoBehaviour service that performs the spawn
operation.

Responsibilities:

- Accept an EnemySpawnRequest
- Instantiate the prefab
- Validate the presence of EnemyAttackController
- Call EnemyController.Initialize
- Call EnemyAttackController.Initialize
- Return a successful result
- Clean up the instance after a failed operation

Not responsible for:

- Choosing the spawn time
- Choosing a spawn point
- Randomly selecting an enemy type
- Managing the maximum enemy count
- Calculating wave progression
- Scaling enemy difficulty
- Destroying dead enemies
- Performing object pooling

---

## 4. Spawn Flow

1. The caller creates an EnemySpawnRequest.
2. The request validates its required dependencies.
3. EnemySpawner instantiates the enemy prefab at the supplied position and
   rotation.
4. EnemyAttackController is retrieved from the created instance.
5. EnemyController is initialized with the definition, target, and registry.
6. EnemyAttackController is initialized with the definition, target, and target
   IDamageable.
7. EnemySpawnResult is returned to the caller.

If initialization throws an exception, the created enemy instance is cleaned up
and the error is propagated back to the caller.

---

## 5. Dependency Rules

The spawn operation requires:

- EnemyController prefab
- EnemyDefinition
- Target Transform
- Target IDamageable
- EnemyRegistry

Spawn position and rotation are stored directly in the request.

The target Transform and target IDamageable are not required to belong to the same
object. This allows future separation between an aiming point and a damageable
collider or health object.

The target IDamageable must be a Unity Object because of the existing
EnemyAttackController contract.

---

## 6. Multiple Spawn Points

EnemySpawner creates one enemy from one request.

A higher-level system that needs several spawn points:

1. Selects the spawn points.
2. Creates one EnemySpawnRequest per point.
3. Calls EnemySpawner.Spawn repeatedly.
4. Stores the returned results in its own collection.

This keeps EnemySpawner independent from wave and arena rules.

---

## 7. Failure Behaviour

The spawn operation must fail when:

- The prefab is null
- EnemyDefinition is null
- The target is null
- The target IDamageable is null
- EnemyRegistry is null
- The target IDamageable is not a Unity Object
- The spawned prefab does not contain EnemyAttackController
- EnemyController initialization fails
- EnemyAttackController initialization fails

When failure occurs after instantiation, the instance must not remain in the
scene.

---

## 8. First-Version Acceptance Criteria

- A valid request creates an enemy.
- The enemy is created at the requested position and rotation.
- EnemyController is initialized.
- EnemyAttackController is initialized.
- The enemy is registered with EnemyRegistry.
- The enemy moves toward the supplied target.
- The enemy damages the target after entering attack range.
- The spawn result exposes the created controllers.
- Null dependencies produce explicit exceptions.
- A prefab without an attack component is rejected.
- A failed spawn does not leave a partial instance in the scene.
- Existing Enemy Foundation and Enemy Attack tests continue to pass.

---

## 9. Deferred Topics

- Wave system
- Spawn timers
- Random spawn-point selection
- Weighted enemy selection
- Difficulty scaling
- Maximum active enemy limits
- Object pooling
- Spawn effects
- Spawn animations
- Spawn safety distance
- Spawning outside the player's view
- Finding a valid NavMesh position
- Dead-enemy cleanup
- Arena completion decisions