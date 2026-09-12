# Wave Foundation

## 1. Purpose

This module manages the ordered enemy waves inside an arena and tracks their
runtime progression.

EnemySpawner remains responsible for creating one enemy. WaveController
coordinates which enemies are created, how many are created, and when the sequence
advances.

The first version covers:

- Defining multiple waves
- Defining one or more enemy entries inside each wave
- Specifying a prefab, EnemyDefinition, and count for each entry
- Spawning all enemies in the current wave
- Using spawn points in sequence
- Tracking only enemies created by the current WaveController
- Advancing after all enemies in the wave die
- Publishing sequence completion after the final wave
- Cleaning partially spawned waves after a failure

Spawn timers, delays between waves, and difficulty scaling are outside the scope
of this version.

---

## 2. Design Goals

- WaveController must not manage enemy AI behaviour.
- Existing EnemySpawner must perform enemy creation.
- WaveController must not search the scene for dependencies.
- Player, EnemyRegistry, and spawn points must be supplied explicitly.
- ScriptableObject definitions must not contain scene Transform references.
- Wave completion must not depend on the global EnemyRegistry count.
- WaveController must track only enemies it created.
- Global singletons, service locators, and global event buses must not be used.
- Runtime progression must not modify ScriptableObject assets.
- Wave completion events must be published only once.

---

## 3. Core Components

### EnemyWaveEntry

Represents one enemy group inside a wave.

It contains:

- EnemyController prefab
- EnemyDefinition
- Enemy count

A wave may contain multiple EnemyWaveEntry records.

### EnemyWaveDefinition

A ScriptableObject defining the contents of one wave.

It contains:

- A stable identifier
- A display name
- An ordered list of EnemyWaveEntry records

It does not contain scene references or runtime progression.

### ArenaWaveDefinition

A ScriptableObject defining the ordered wave sequence for one arena.

It contains:

- A stable identifier
- An ordered list of EnemyWaveDefinition assets

WaveController processes these assets in their defined order.

### WaveProgressState

A pure C# class representing the runtime progression of one wave.

It tracks:

- Total planned enemies
- Spawned enemies
- Living enemies
- Defeated enemies
- Whether spawning has completed
- Whether the wave has completed

It does not modify ScriptableObject data and has no MonoBehaviour dependency.

### WaveController

A runtime MonoBehaviour coordinating the wave sequence.

Responsibilities:

- Initialize with explicit dependencies
- Begin the first wave
- Spawn enemies through EnemySpawner
- Use spawn points in sequence
- Track the EnemyController instances it creates
- Listen to EnemyController.Died
- Advance after wave completion
- Publish sequence completion after the final wave
- Clean up event subscriptions

Not responsible for:

- Enemy movement or attacks
- Enemy health
- Random enemy selection
- Delays between waves
- Spawn effects
- Reward calculation
- Arena success or failure
- Player death
- Object pooling

---

## 4. Dependencies

WaveController explicitly receives:

- ArenaWaveDefinition
- EnemySpawner
- EnemyRegistry
- Target Transform
- Target IDamageable
- One or more spawn points

Spawn points belong to the scene and are therefore not stored in ScriptableObject
assets.

The target Transform and target IDamageable are not required to belong to the same
object.

---

## 5. Wave Start Flow

1. WaveController is initialized.
2. Begin is called.
3. The first EnemyWaveDefinition is selected.
4. WaveStarted is published.
5. EnemyWaveEntry records are processed in order.
6. An EnemySpawnRequest is created for each enemy.
7. EnemySpawner.Spawn is called.
8. WaveController subscribes to the spawned EnemyController.Died event.
9. Spawn points are selected using round-robin order.
10. Wave spawning is marked as complete after all planned enemies are created.

All enemies in the wave are spawned immediately in the first version.

---

## 6. Wave Completion Flow

1. An enemy created by the wave dies.
2. WaveController processes its Died event.
3. The enemy event subscription is removed.
4. The living enemy count is reduced.
5. The wave completes when spawning is complete and no tracked enemies remain
   alive.
6. WaveCompleted is published.
7. The next wave begins automatically when available.
8. SequenceCompleted is published when no wave remains.

The same enemy death must not be processed more than once.

---

## 7. EnemyRegistry Usage

EnemyRegistry remains the global runtime registry and is still supplied to every
EnemySpawnRequest.

Wave completion does not use:

`EnemyRegistry.ActiveCount == 0`

That value may include enemies created outside the wave sequence.

Completion is determined only from EnemyController instances created and tracked
by the current WaveController.

---

## 8. Spawn-Point Selection

Spawn points are used in their supplied order.

With three spawn points:

- Enemy 1 uses Spawn Point 0
- Enemy 2 uses Spawn Point 1
- Enemy 3 uses Spawn Point 2
- Enemy 4 uses Spawn Point 0

Random selection is not included in the first version.

A null or empty spawn-point collection is rejected. Null Transform entries inside
the collection are also rejected.

---

## 9. Failure Behaviour

When any spawn operation fails while starting a wave:

1. Event subscriptions for enemies already created by that wave are removed.
2. Those instances are disabled and cleaned up.
3. Wave progression is cleared.
4. WaveController remains in a failed state.
5. The original exception is propagated to the caller.

A partially started wave must not continue running.

---

## 10. First-Version Events

WaveController publishes local events for:

- WaveStarted
- EnemySpawned
- EnemyDefeated
- WaveCompleted
- SequenceCompleted

These events are observed through the WaveController instance. No global event bus
is used.

---

## 11. First-Version Acceptance Criteria

- A valid arena definition begins the first wave.
- All enemies in the current wave are spawned.
- Multiple enemy entries are supported.
- Spawn points are used in sequence.
- Spawned enemies are registered with EnemyRegistry.
- WaveController tracks only enemies it created.
- Living enemy count decreases after an enemy dies.
- The next wave begins after all enemies in the current wave die.
- SequenceCompleted is published once after the final wave.
- Enemies outside the wave do not affect completion.
- Invalid definitions and dependencies produce explicit exceptions.
- Failed spawning does not leave partial wave instances.
- Existing enemy and spawning tests continue to pass.

---

## 12. Deferred Topics

- Delays between waves
- Timed enemy spawning
- Random spawn-point selection
- Weighted enemy selection
- Difficulty scaling
- Elite and boss-wave rules
- Object pooling
- Spawn animation and effects
- Wave UI
- Arena rewards
- Arena completion decisions
- Run success and failure flow
- Dead-enemy cleanup