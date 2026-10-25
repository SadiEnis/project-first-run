# Shared game flow and restart

## Status

Pre-implementation plan. Only documentation changed in this increment; no runtime or scene edits. The current game scene remains the main development space; the delivered demo stays preserved by its tag.

## Findings and responsibilities

- `DemoOpeningController.Restart` checks death, marks entry bypass, resets time and loads the scene.
- `DemoPauseController.Restart` releases pause ownership, loads the scene and attempts to reopen pause on failure.
- `DemoFinalController.Replay` requests permission from its final session and rolls back replay on failure.
- `DemoEntryController` carries next-load entry bypass and manages its own controls/time.
- `RewardSelectionController` also manages time and controls. Centralizing pause alone will not solve all shared ownership.
- Existing `RunSessionController` handles arena sequencing and run outcomes; it is not the demo screen coordinator and is unchanged here.

Keep flow rules, Unity loading, screen presentation and run progression separate. No global singleton or all-purpose GameManager.

## Slice A — First implementation: one restart operation

### Proposed new code

Under `Assets/_Project/Scripts/Flow/`:

- `RunRestartOperation`: pure C# owner of Idle/Loading/Failed transitions, one active request and failure result; no Canvas, Cinemachine or Unity SceneManager dependency.
- `IRunSceneReloader`: narrow dependency boundary for reloading the run scene, not a service locator.
- `UnityRunSceneReloader`: Editor/player loading implementation; null operation or startup failure becomes an explicit failure.
- `RunRestartController`: single scene composition point with serialized references, forwarding screen requests to the operation and reporting its status. It must not grow into an all-purpose replacement for future GameFlowController.

Names are proposed. Avoid extra layers if fewer classes can preserve these responsibilities clearly.

### Existing files

- `Scripts/Development/Arenas/DemoOpeningController.cs`: replace local loading with a shared request from the death screen.
- `Scripts/UI/Hud/DemoPauseController.cs`: same request; preserve visuals, cards, input and animation.
- `Scripts/Development/Arenas/DemoFinalController.cs`: same request; preserve ending presentation and final-session rules.
- `Scripts/Development/Arenas/DemoEntryController.cs`: shared restart becomes the sole producer of entry bypass, preserving target scoping and failure cleanup.
- Both saved demo/game scenes and `DeepJamOpeningSceneBuilder`: wire shared controller references; do not regenerate scenes.
- Focused EditMode/PlayMode tests: operation rules and all three screen integrations.

### Contract

1. Accept only eligible sources: open pause, visible death screen or completed ending. Reject entry/camera transition, reward selection and ongoing ending animation.
2. First accepted request starts loading; double clicks, competing screens and repeated input cannot start another load.
3. Screen interaction and gameplay remain blocked during transition. Do not briefly resume gameplay in the old scene.
4. Scope entry bypass to the target scene. Clear it on failure; never leak it to unrelated scenes.
5. Successful loading initializes the new run through existing startup. Old OnDisable cleanup must not overwrite the new scene's locks/time.
6. If loading cannot start, roll back the request and preserve the source screen's time/audio/cursor and locks consistently; allow retry without releasing other owners' locks.
7. Do not assume the old screen can be recovered after source-scene unload. Document and test the Unity loader's recoverable failure boundary.
8. Reloading a run is distinct from `SceneTravelController` map transfer; do not change that system.

## Slice B — Shared screen/game flow

Separate implementation/check-in after A acceptance:

- Pure transition rules plus a small `GameFlowController` coordinate entry, gameplay, death, ending and loading eligibility.
- Treat pause/reward selection separately from run outcomes; avoid one giant enum covering every combination.
- Review time/audio/cursor ownership together. Do not claim centralization complete until entry, pause, reward and ending use the same contract.
- Preserve existing owner-based player locks; closing a screen releases only its ownership.
- Screens focus on presentation/input, not transition authority or restart implementation.
- Keep existing health/death event producers authoritative; do not duplicate health or run state.
- Evaluate RunSession/free-roaming map integration separately; do not introduce mandatory arena clearing.

## Validation and non-goals

A pure tests: acceptance, rejected sources, duplicate requests, loader failure/null operation, retry after failure. Integration: restart from pause/death/completion, entry bypass, fresh build/ammunition, controls blocked during loading, preservation of other owners.

The user runs Unity tests; investigate focused failures when needed. Report compilation, tests and builds separately. Use a test loader for failure simulation, never corrupt real scene assets.

No visual redesign, IMGUI-to-Canvas migration, meta, saves, XP/reward balance, enemies or geometry in A. Preserve current 1/3/5 settings. Exclude unrelated existing material changes and performance-file deletions.

Delivery: TR/EN TDD → Slice A implementation/tests/user acceptance/commit → refine Slice B. This plan does not authorize changing all flow in one increment.
