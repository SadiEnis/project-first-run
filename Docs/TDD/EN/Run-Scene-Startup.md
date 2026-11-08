# Run scene startup

## Ownership

`RunSceneStartup` prepares the authored starting loadout through the existing `PlayerStartingLoadoutInitializer`. Starting weapon and capacity still come from the same ScriptableObject. An initialized loadout is not initialized again.

Existing owners still install experience, chest and ability services. Startup waits for their readiness without duplicating installation or factory registration. This change introduces neither a service locator nor a generic dependency framework.

After shared startup completes, `DemoOpeningController` requests corridor encounter preparation. Guaranteed rewards, room rules and death presentation remain unchanged. Gameplay permissions and pause decisions remain owned by `GameFlowController`.

## Wiring and failure

- Both Opening and WideCombat contain one `RunSceneStartup` on `Demo opening services`.
- Inspector references: loadout, player experience, chest spawner and ability runtime installer.
- Waiting uses real time, so a paused entry menu cannot stall initialization.
- The default ten-second timeout names pending services. Readiness remains false and the entry view displays the error.
- The scene builder creates the same wiring. Existing scenes were not regenerated.

## Validation

EditMode scene coverage checks startup wiring in both scenes. PlayMode opening acceptance also checks loadout, experience, chest and ability readiness before gameplay. Unity test execution and gameplay acceptance remain user-run.

Manual acceptance: enter through Start, acquire the first ability, then verify a fresh run after pause/death/finale restart. Meta persistence and death presentation extraction are outside this slice.
