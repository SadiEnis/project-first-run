# Runtime-Architecture

> Design update, 15 September 2026: The [map/region/encounter plan](Map-Region-Encounter-Plan.md) supersedes the arena-per-scene and wave-clear-to-advance assumptions below. Region travel need not load a scene or recreate/reset the player. Cross-map persistence, destination entry identities and exit-driven progression will be designed in their planned stages. The remaining architecture below is not evidence that those systems are implemented.

# Runtime Architecture and Scene Flow

## 1. Purpose

This section defines the runtime lifetime model, application startup flow, scene transitions, run-state ownership, and arena initialization process.

The primary goals are:

- Preserve the player's run state between arena scenes.
- Prevent scene objects from becoming global dependencies.
- Avoid a monolithic `GameManager`.
- Keep persistent application services separate from run-specific and arena-specific systems.
- Make arena scenes independently testable.
- Recreate scene objects safely without losing the player's build or progression.
- Avoid unnecessary abstractions and hidden global references.

---

## 2. Lifetime Model

The project uses three primary lifetimes.

### 2.1 Application Lifetime

Application-lifetime systems remain alive from game startup until the application is closed.

Examples:

- Scene flow
- Profile data
- Save system
- Settings
- Item catalog
- Run coordination
- Loading-screen presentation

These systems are created once by the Bootstrap scene.

### 2.2 Run Lifetime

Run-lifetime data exists from the beginning of a run until the player dies, abandons the run, or defeats the final boss.

Examples:

- Current health
- Current run level
- Current experience
- Collected run gold
- Equipped items
- Item levels
- Active run upgrades
- Current arena
- Run seed
- Run statistics

Run state must survive arena scene changes but does not initially need to survive closing the game.

### 2.3 Scene Lifetime

Scene-lifetime objects belong only to the currently loaded scene.

Examples:

- Player GameObject
- Camera
- Weapon and ability runtime components
- Enemies
- Projectiles
- Pickups
- Arena layout
- Wave controller
- Enemy registry
- Scene UI
- Local object pools
- Lighting and environment objects

These objects are destroyed when the current scene is unloaded.

---

## 3. Persistent Application Root

The game begins from a small `Bootstrap` scene.

The Bootstrap scene creates a single persistent root object:

```
ApplicationRoot
├── SceneFlowController
├── RunCoordinator
├── ProfileService
├── SaveService
├── ItemCatalog
└── GlobalLoadingOverlay
```

`ApplicationRoot` is the only root object expected to use `DontDestroyOnLoad`.

The following objects must not use `DontDestroyOnLoad`:

- Player
- Camera
- Weapons
- Abilities
- Enemies
- Arena controllers
- Scene UI
- Projectiles
- Pickups

This rule prevents stale scene references, duplicate player objects, persistent cameras, and difficult-to-debug arena transitions.

`ApplicationRoot` is not a global `GameManager`. It does not contain gameplay logic. It owns separate systems with clearly defined responsibilities.

---

## 4. Dependency Access

The project will not initially use:

- A global service locator
- A static dependency container
- A global event bus
- A dependency injection framework
- Static access such as `GameManager.Instance`

The Bootstrap scene acts as the composition root. It creates application services and passes only the required dependencies to newly loaded scenes.

A scene must not receive the entire application context when it only requires a small number of dependencies.

Example:

```
ArenaLoadContext
├── RunSession
├── ItemCatalog
├── RandomSource
└── Arena completion callback
```

An arena does not need direct access to:

- SaveService
- SettingsService
- Main-menu UI
- Profile file storage
- Scene transition implementation

This keeps dependencies explicit and limited.

---

## 5. Scene Structure

The current planned scene structure is:

```
Scenes
├── Bootstrap
├── MainMenu
├── Base
├── Arena_01
├── Arena_02
├── Arena_Boss_01
├── Arena_FinalBoss
└── Test
    ├── Test_Player
    ├── Test_Weapons
    └── Test_Combat
```

Each arena is stored in a separate Unity scene.

Arena transitions use asynchronous `LoadSceneMode.Single` loading.

Additive scene loading is not currently required.

The game will use a persistent loading overlay instead of requiring a separate Loading scene. This reduces scene complexity while still providing a full loading screen between arenas.

---

## 6. Application Flow

The initial application flow is:

```
Application Start
    ↓
Bootstrap Scene
    ↓
Create ApplicationRoot
    ↓
Load Profile and Settings
    ↓
Load Main Menu or Base
    ↓
Player Starts a Run
    ↓
Create RunSession
    ↓
Load First Arena
    ↓
Initialize Arena
    ↓
Play Arena
    ↓
Complete Arena
    ↓
Update Run Progress
    ↓
Load Next Arena
    ↓
Final Boss Victory or Player Death
    ↓
Create RunResult
    ↓
Apply Permanent Rewards
    ↓
Save Profile
    ↓
Return to Base
```

A simple application-flow enum may be used to prevent invalid transitions:

```
Booting
MainMenu
Base
LoadingArena
PlayingArena
ResolvingRun
```

A class-based state machine will not be introduced unless the application flow becomes complex enough to justify it.

---

## 7. Scene Transition Process

Only `SceneFlowController` may perform normal scene transitions.

An arena transition follows this sequence:

```
Arena transition requested
    ↓
Reject duplicate transition requests
    ↓
Disable gameplay input
    ↓
Show loading overlay
    ↓
Load target scene asynchronously
    ↓
Activate target scene
    ↓
Locate scene entry point
    ↓
Initialize scene dependencies
    ↓
Create player runtime objects
    ↓
Restore run state
    ↓
Start arena
    ↓
Hide loading overlay
    ↓
Enable gameplay input
```

A one-time scene lookup is acceptable at the scene boundary.

For example, after loading an arena, `SceneFlowController` may locate the scene's `ArenaSceneBootstrap` once. Repeated runtime searches such as `FindObjectOfType` inside gameplay updates are not allowed.

The loading overlay remains part of `ApplicationRoot`, so it remains visible while the old scene is replaced.

---

## 8. Run Coordinator

`RunCoordinator` owns the current run lifecycle.

Its responsibilities are:

- Start a new run.
- Create the initial `RunSession`.
- Store the active `RunSession`.
- Request arena transitions.
- Process arena completion.
- Determine whether another arena should be loaded.
- End the run after death, abandonment, or final victory.
- Create the final `RunResult`.
- Apply retained gold and permanent rewards to the player profile.
- Request profile saving.
- Return the player to the Base scene.

`RunCoordinator` must not:

- Spawn enemies.
- Control waves.
- Calculate weapon damage.
- Generate chest choices.
- Control the player.
- Update gameplay UI.
- Contain arena-specific rules.

---

## 9. Run Session

`RunSession` is a pure C# runtime model.

It is not a `MonoBehaviour` and is not stored directly inside a scene.

Its proposed structure is:

```
RunSession
├── RunIdentity
│   ├── RunId
│   └── RandomSeed
│
├── RouteState
│   ├── CurrentArenaId
│   ├── CurrentArenaIndex
│   └── DefeatedBosses
│
├── VitalsState
│   └── CurrentHealth
│
├── ProgressionState
│   ├── RunLevel
│   ├── CurrentExperience
│   └── RequiredExperience
│
├── WalletState
│   └── RunGold
│
├── LoadoutState
│   ├── Weapons
│   ├── Abilities
│   └── Upgrades
│
├── StatState
│   ├── BaseStatSnapshot
│   └── ActiveRunModifiers
│
└── Statistics
    ├── EnemiesDefeated
    ├── ElitesDefeated
    ├── DamageDealt
    └── RunDuration
```

The exact statistics list may change.

The random seed is stored to make reward generation and difficult bugs easier to reproduce during development.

---

## 10. Run-State Ownership

`RunSession` is the single source of truth for state that must survive arena transitions.

Scene objects do not keep separate authoritative copies of persistent run data.

Examples:

```
PlayerHealth
    ↓
Reads and updates RunSession.VitalsState
```

```
ExperienceSystem
    ↓
Reads and updates RunSession.ProgressionState
```

```
RunWallet
    ↓
Reads and updates RunSession.WalletState
```

```
LoadoutSystem
    ↓
Reads and updates RunSession.LoadoutState
```

Systems should receive only the part of the run model they require. A weapon does not need access to the complete `RunSession`.

---

## 11. Data Preserved Between Arenas

The following state is preserved by default:

- Current health
- Run level
- Current experience
- Run gold
- Equipped weapons
- Equipped abilities
- Equipped upgrades
- Equipment levels
- Evolved equipment
- Run-wide stat modifiers
- Current route progress
- Run statistics

The following state is scene-local by default:

- Active enemies
- Projectiles
- Temporary visual effects
- Ability cooldown progress
- Active status effects on enemies
- Arena wave progress
- Current weapon-object references
- Local object pools
- Temporary scene interactions

Whether ammunition, cooldowns, and temporary player effects should carry between arenas is a game-design decision and remains open.

Until another decision is made:

- Weapons are recreated when entering a new arena.
- Magazines begin in their default ready state.
- Ability cooldowns begin from their default state.
- Enemy-related status effects are discarded with the arena.

---

## 12. Profile Data and Run Data

Persistent profile data and temporary run data are separate concepts.

### 12.1 Profile Data

Saved to disk:

```
PlayerProfile
├── PermanentGold
├── UnlockedSlots
├── PermanentUpgrades
├── UnlockedContent
├── ProgressionFlags
└── SaveVersion
```

### 12.2 Run Data

Stored in memory during a run:

```
RunSession
├── CurrentHealth
├── RunGold
├── RunLevel
├── Equipment
├── ItemLevels
└── ArenaProgress
```

The initial version of the game does not require saving and resuming an unfinished run after closing the application.

A future suspend-and-resume system may serialize a `RunSessionSnapshot`, but the initial architecture will not be complicated around that unconfirmed feature.

---

## 13. Base Stat Snapshot

When a run starts, permanent profile improvements are converted into an initial base-stat snapshot.

```
Default Character Stats
    +
Permanent Profile Upgrades
    ↓
Run Base Stat Snapshot
```

During the run:

```
Run Base Stat Snapshot
    +
Equipment Modifiers
    +
Trait Modifiers
    +
Temporary Run Modifiers
    ↓
Final Runtime Stats
```

Permanent profile data is not repeatedly queried by combat systems during gameplay.

This ensures that a run begins with a stable base and that all run-specific changes remain inside `RunSession`.

---

## 14. Item Identity and Catalog

Save files and run state must not rely on display names or direct scene-object references.

Every item definition uses a stable identifier.

Examples:

```
weapon.shotgun
weapon.minigun
ability.force_wave
ability.fireball
upgrade.damage
```

A central `ItemCatalog` resolves stable IDs into ScriptableObject definitions.

```
Stable Item ID
    ↓
ItemCatalog
    ↓
ItemDefinition
```

The catalog is responsible for:

- Resolving item IDs.
- Validating duplicate IDs.
- Providing definitions to loadout and reward systems.
- Supporting future Addressables integration.

`RunSession` stores:

```
Item ID
Item Level
```

It does not need to store a direct reference to a ScriptableObject.

---

## 15. Item Runtime Reconstruction

When an arena is loaded, the player's equipment is reconstructed from `RunSession.LoadoutState`.

Example:

```
Run Loadout Entry
├── Item ID: weapon.shotgun
└── Level: 5
        ↓
ItemCatalog resolves ShotgunDefinition
        ↓
Weapon runtime is created
        ↓
Level data is applied
        ↓
Weapon is attached to player loadout
```

Evolved equipment is handled identically.

An evolution result is simply another item ID:

```
ability.fireball
    ↓ evolution
ability.meteor
```

The new arena creates the Meteor runtime rather than recreating Fireball.

---

## 16. Arena Scene Bootstrap

Every gameplay scene contains one `ArenaSceneBootstrap`.

Its responsibilities are:

- Hold references to arena-specific scene objects.
- Receive `ArenaLoadContext`.
- Initialize arena-local systems.
- Create or prepare the player.
- Connect the player to the current run state.
- Initialize enemy registry and wave systems.
- Start the arena after all dependencies are ready.
- Report arena completion to `RunCoordinator`.

Its scene references may include:

```
ArenaSceneBootstrap
├── PlayerSpawnPoint
├── EnemySpawnAreas
├── WaveController
├── ArenaController
├── ArenaCameraReferences
└── SceneUIRoot
```

`ArenaSceneBootstrap` must not:

- Save the player profile.
- Decide permanent gold retention.
- Directly load the next scene.
- Own the complete application context.
- Implement weapon or ability behavior.

---

## 17. Arena Completion

When an arena is completed:

```
Wave or boss objective completed
    ↓
ArenaController marks arena complete
    ↓
Player interaction is limited
    ↓
ArenaResult is created
    ↓
RunCoordinator receives ArenaResult
    ↓
Run route is updated
    ↓
Next arena is selected
    ↓
SceneFlowController loads next arena
```

`ArenaResult` may contain:

```
ArenaResult
├── ArenaId
├── CompletionType
├── BossDefeated
├── CompletionTime
└── Arena-specific rewards
```

Most persistent player data is already stored in `RunSession`, so `ArenaResult` should not duplicate the entire run state.

---

## 18. Run Completion

A run ends through one of the following outcomes:

```
PlayerDeath
FinalBossVictory
PlayerAbandon
```

The end flow is:

```
Run ends
    ↓
Create RunResult
    ↓
Calculate retained gold
    ↓
Apply permanent rewards
    ↓
Update PlayerProfile
    ↓
Save profile
    ↓
Clear active RunSession
    ↓
Return to Base
```

`RunResult` may include:

```
RunResult
├── Outcome
├── TotalGoldCollected
├── GoldRetained
├── ArenasCompleted
├── BossesDefeated
├── EnemiesDefeated
├── RunDuration
└── FinalBuild
```

---

## 19. Scene Ownership Rules

The following rules apply:

1. Scene objects must not be stored inside persistent ScriptableObjects.
2. Application services must not keep references to destroyed scene objects after a transition.
3. Arena scenes must not directly access save-file storage.
4. Arena systems must not load scenes directly.
5. The player GameObject is recreated for each arena.
6. Run data survives; scene presentation does not.
7. Only one normal scene transition may occur at a time.
8. Input remains disabled until arena initialization is complete.
9. Gameplay scenes must contain exactly one valid arena entry point.
10. Runtime object searches are not allowed in frequently executed gameplay code.

---

## 20. Initial Vertical Slice Flow

The first vertical slice validates this architecture with:

```
Bootstrap
    ↓
Base Scene
    ↓
Start Run
    ↓
Create RunSession
    ↓
Load Test Arena
    ↓
Create Player
    ↓
Equip Shotgun
    ↓
Spawn Enemies
    ↓
Gain Experience
    ↓
Open Chest
    ↓
Acquire Force Wave or Upgrade
    ↓
Complete Arena or Die
    ↓
Create RunResult
    ↓
Return to Base
```

The architecture is considered validated only if:

- The player can move between scenes without losing run state.
- The player GameObject can be destroyed and recreated safely.
- The current build can be reconstructed from item IDs and levels.
- Arena systems do not depend on persistent scene objects.
- A new arena can be added without modifying existing arena code.
- The save system remains separate from combat systems.
- No global `GameManager` or service locator is required.

---

## 21. Confirmed Technical Decisions

| Topic | Decision |
| --- | --- |
| Persistent root | One `ApplicationRoot` |
| Persistent gameplay objects | Not allowed |
| Run state | Pure C# `RunSession` |
| Player between arenas | Destroyed and recreated |
| Arena loading | Asynchronous Single scene loading |
| Loading presentation | Persistent loading overlay |
| Additive loading | Not currently used |
| Scene initialization | Explicit arena context |
| Scene entry discovery | One-time lookup after loading |
| Item persistence | Stable item IDs and levels |
| Item resolution | Central `ItemCatalog` |
| Save and run state | Separate models |
| Mid-run save | Not required initially |
| Application flow | Simple coordinator with guarded state |
| Global service locator | Not used |
| Global event bus | Not used |
| DI framework | Not used initially |
| `DontDestroyOnLoad` | Restricted to `ApplicationRoot` |
