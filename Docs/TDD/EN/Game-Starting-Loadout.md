# Game starting loadout settings

## Status and purpose

### Implementation status

The user confirmed settings affect gameplay and the initial tests passed. A subsequent report identified fixed 2/5/8 pause slots; corrected to use runtime build capacity. Weapon/ability rows and each upgrade row (up to four columns) center independently: 1/3/5 displays 1, 3, 4+1; 2/5/8 retains 2, 5, 4+4. Preserved the user's current 1/3/5 asset choice. Scene tests now validate settings agreement and supported bounds rather than pinning editable values to Minigun/2/5/8. Static compilation of the pause fix passed; the user manually confirmed capacity-aware display and centering. A separate Unity rerun result after this final fix has not been recorded.

Implemented, with gameplay settings acceptance reported by the user. `SO_GameStartingLoadout` was initially created with the shared Minigun reference and 2/5/8 capacity; the user's latest 1/3/5 selection is preserved. Only player prefab startup overrides changed in both scenes; geometry is untouched. Both scenes enable `_requireSettings`: removing the asset cannot fall back to legacy defaults, including through automatic Start. Legacy fixtures retain their own paths without requiring settings.

`InitializeConfigured()` validates settings and uses the existing initialization path, preserving `Initialized`. Running capacity and starting weapon snapshots are unaffected by later asset edits. The editor builder assigns the same asset for future scenes; it was not executed in this increment.

Extended existing initializer EditMode tests for valid/invalid settings, required settings absence, single notification, repeated initialization, runtime snapshots and legacy conflicts. Demo layout tests verify both scenes' asset/capacity wiring. A PlayMode integration test covers configured automatic Start and reenabling. Runtime, EditMode and PlayMode sources compiled statically; the assistant did not run Unity tests or a build; the user reported the initial Unity tests passing. The existing `PlayerChestInteractorTests.TearDown` warning was left outside scope.

Manual acceptance: outside Play Mode, select a valid weapon/capacity on `Assets/_Project/Data/Builds/SO_GameStartingLoadout.asset`, start a new run and verify HUD/weapon, acquire the first chest ability and restart. The post-test configuration is the user's choice; do not automatically revert the current 1/3/5 selection. Asset edits persist.

Design decision: The existing demo scene is the main development space that will evolve into the actual game; do not create separate demo/test/normal-game architectures. Preserve the submitted version through its Git tag. Keep the content arena as a focused content-testing aid.

Unify starting weapon and slot capacity under one Inspector-editable data source without regenerating scenes or changing current gameplay.

## Current state

- `PlayerStartingLoadoutInitializer._startingWeapon` is already serialized and Inspector-editable; weapon choice is not hard-coded.
- Its automatic startup uses `PlayerBuildCapacity.CreateDefault()`.
- `DemoOpeningController.Awake()` explicitly forces maximum weapon/ability/upgrade capacity.
- `WeaponSwitchingDevelopmentBootstrap` explicitly initializes a different capacity in some test fixtures.
- The new ability installer depends on loadout completion; preserve that contract.

## Decision

Proposed ScriptableObject: `Scripts/Builds/PlayerStartingLoadoutSettings.cs`, containing:

- Starting `WeaponDefinition`.
- Weapon slots.
- Ability slots.
- Upgrade slots.

The asset is configuration only. Never store runtime build, ammunition, owned items or meta progression in it. Create runtime capacity from validated startup values; changing the asset during Play does not rebuild the current run.

`PlayerStartingLoadoutInitializer` remains the single owner of initialization. Demo opening may call its configured initialization method instead of choosing numbers. Automatic Start uses that same method and skips completed initialization. Publish the existing `Initialized` notification once, after successful weapon acquisition.

In configured scenes, the asset is the sole source of weapon and capacity. Retain the legacy serialized weapon field and explicit `Initialize(capacity)` API only as a narrow fixture compatibility path if needed. Reject conflicting legacy overrides when settings are assigned rather than silently combining two sources; test this contract. Do not migrate all old fixtures in this increment.

Bind both existing demo scenes to one game-start asset, preserving the current weapon and 2/5/8 capacity. Proposed location: `Assets/_Project/Data/Builds/SO_GameStartingLoadout.asset`. Verify actual scene/prefab weapon overrides during implementation; do not silently collapse different configurations.

## Validation rules

- Reject missing weapons, invalid weapon configuration and invalid capacities before changing build, ammunition or ability state.
- Keep `PlayerBuildCapacity` limits at 2/5/8. These are supported model limits, not selected startup values. Expanding capacity support is outside this task.
- Missing settings in the main development scene must fail clearly, not silently fall back to defaults.
- Future meta progression can supply resolved startup values to the same runtime initialization path. Do not add a meta provider, save system or abstract service framework now.

## Expected file scope

- New `Scripts/Builds/PlayerStartingLoadoutSettings.cs`, asset and required meta files.
- `Scripts/Builds/PlayerStartingLoadoutInitializer.cs`.
- `Scripts/Development/Arenas/DemoOpeningController.cs`.
- `Scripts/Editor/DeepJamOpeningSceneBuilder.cs`: author the same reference in future generated scenes; do not run it on existing scenes.
- `Scenes/Demo/DeepJam_Opening.unity` and `DeepJam_WideCombat.unity`: startup settings references only.
- Relevant build/startup/scene layout tests and new settings tests.

Source paths are relative to `Assets/_Project/`. Inspect dependent fixtures and make only necessary compatibility changes. Preserve authored geometry, enemy placement and unrelated prefab fields.

## Acceptance

1. The main scene starts with the same weapon and 2/5/8 capacity.
2. Another valid weapon/capacity selected through the asset is applied on the next run start; restore the main configuration after testing.
3. HUD slots reflect actual build capacity; no new free abilities/upgrades.
4. Ability initialization, first-chest acquisition and restart reset still work.
5. Invalid configuration cannot partially initialize; reject duplicate installation and conflicting legacy overrides.
6. Preserve content arena and legacy fixture startup behavior.
7. The user runs Unity tests. Do not claim unexecuted test/build success.

## Non-goals

Chest probabilities, XP, damage, cooldowns, new content, meta progression, save/load, menu flow and scene geometry. Do not include unrelated working-tree changes.

Delivery: TR/EN TDD check-in → implementation and focused tests → user acceptance → code and acceptance documentation commits.
