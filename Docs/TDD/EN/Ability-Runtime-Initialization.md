# Ability runtime initialization

## Status and purpose

2026-10-25: Pre-implementation technical plan. This document does not claim implementation or test completion.

Separate initialization of the eight existing abilities from demo/test code and make startup ordering explicit. Preserve gameplay, balance, capacity and content choices.

## Current state

- `Scripts/Development/Abilities/FireballDevelopmentBootstrap.cs` combines registration of eight factories with an optional starting Fireball grant.
- Its `Start()` waits one frame before calling `Install()`, allowing `PlayerStartingLoadoutInitializer.Start()` to initialize the build. This is a timing assumption, not a readiness contract.
- `PlayerStatsController` initializes in `Awake()`. `PlayerAbilityAcquisitionController.Initialize()` receives the factory registry; acquiring abilities also requires build/runtime readiness.
- Demo opening can initialize its own loadout capacity. The new installer must not initialize default capacity prematurely or reset an existing build.
- The current bootstrap requires a Fireball definition and projectile prefab even without a starting grant. General factory installation must not depend on that development content.

## Proposed boundaries and sequence

1. Add a clearly named shared factory installation component under `Scripts/Abilities`; proposed name: `PlayerAbilityRuntimeInstaller`.
2. Reuse all eight factories and the same enemy registry, damage source and stats dependencies. Do not rewrite factories or ability algorithms.
3. Use an explicit readiness contract after successful loadout initialization. Support attaching both before and after loadout readiness. Do not replace the one-frame delay with polling or execution-order numbers alone.
4. Once stats and loadout are ready, initialize acquisition once. Missing dependencies or a competing installation owner produce clear errors. Disable/re-enable must not create another registry or starting reward; clean up subscriptions.
5. Keep optional starting Fireball acquisition as a separate responsibility of the legacy development adapter. General installation grants no free ability. The adapter delegates to shared installation rather than maintaining another factory list.
6. Preserve legacy serialized fields and script GUID during migration. Do not remove `Install()` / `IsInstalled` behavior without evaluating existing tests. The production path must not depend on the development namespace.

Finalize the readiness notification API alongside lifecycle tests during implementation. This is not a general service framework or a rewrite of all player startup.

## Impact inventory

Confirmed existing sources:

- `Scripts/Development/Abilities/FireballDevelopmentBootstrap.cs`
- `Scripts/Builds/PlayerStartingLoadoutInitializer.cs`
- `Scripts/Editor/ContentArenaSceneBuilder.cs`
- `Scripts/Editor/PlayableMapFixtureSceneBuilder.cs`
- `Tests/PlayMode/Development/Chests/ChestDevelopmentBootstrapTests.cs`
- `Tests/EditMode/Chests/CommonChestContentTests.cs`

Scenes with serialized usage:

- `Scenes/Demo/DeepJam_Opening.unity`
- `Scenes/Demo/DeepJam_WideCombat.unity`
- `Scenes/Playable/PlayableMapFixture.unity`
- `Scenes/Tests/Test_ContentArena.unity`
- `Scenes/Tests/Test_Waves.unity`

Paths are relative to `Assets/_Project/`. This is an inspection/migration inventory, not a promise to change every file. Track new components/tests together with their `.meta` files. Update builders as needed, but do not regenerate existing scenes; preserve geometry, enemy placement and other user edits.

## Acceptance and validation

- Installation completes once whether attached before or after loadout readiness.
- Missing dependencies and competing ownership fail clearly; incomplete installation is not reported as success.
- Preserve demo maximum capacity and each test scene's starting loadout.
- All eight abilities remain acquirable and functional through existing paths; only explicitly configured development scenes grant starting Fireball.
- Disable/re-enable and scene restart do not duplicate initialization or grants. Preserve existing enemy registry rebinding during scene travel.
- Add/update focused EditMode and PlayMode tests. The user runs Unity tests; investigate only relevant failures. Do not report unexecuted tests as passing.
- User acceptance covers startup, first ability chest and restart in both demo scenes; acquisition of eight abilities in the content arena; and startup compatibility in legacy fixtures.

## Non-goals and delivery

No changes to damage, cooldowns, item levels, slot counts, reward pools, chest probabilities, HUD/menus, meta/save systems or map design. Separating demo/test/normal-game profiles is a later task.

Check in/commit these TR/EN documents first. Implementation, focused tests, user acceptance and a separate code commit follow. Do not change the demo baseline tag.
