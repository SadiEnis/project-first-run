# Shared game flow and restart

## Status

Slice A implemented; the user accepted pause/death/finale restart gameplay and reported completing check-ins. October 30, 2026: B1/B2 code and B2 scene wiring are implemented. The user reported green tests and no observed gameplay issues; B2 is accepted. The current game scene remains the main development space; the delivered demo stays preserved by its tag.

### B2 implementation and validation boundary

- Both game scenes have `GameFlowController` and `GameplayAccessController` on Demo opening services. Entry, pause, rewards and restart use serialized references. Final/death/ambush failure are read from existing health/session authorities, not another mutable run model.
- Entry and pause own tokens. Approach updates the entry token; pause releases only after closing animation/input release. The coordinator derives terminal tokens from existing Ending/Completed/death state. Rewards retain claim/multi-claim presentation and no longer rewrite base control on the shared path. Death dismisses an open reward without reversing death-disabled controls.
- `GameplayAccessController` is the sole time/audio/cursor writer in target scenes. Motor/damage/input/weapon/ability locks use its own identity. Entry/pause/finale/loading/failure protect health; rewards/death do not add immortality. PlayerController skips cursor writes when an owner is bound. Input reader safely retains locks received before its Awake.
- Baseline time is serialized as 1; normal audio is unmuted. UI/camera animation remains unscaled. Loading acquires its token first; startup failure releases only that token. Successful loading retires the old application component. Its private static ownership marker only guards writes to Unity globals; it exposes no global manager or service lookup API. Late old-scene cleanup cannot overwrite a newer scene's globals.
- Audit also found `DemoKeyAmbushController.RestartFailedRun`; it now uses the shared loader. `FailedEncounter` requires `KeyAmbushPhase.Failed`. The failure screen freezes/protects the player and permits retry after startup failure. Normal ambush behavior is unchanged.
- Geometry, visuals, loadout and capacities were not changed; scenes were not regenerated. The opening builder wires the same dependencies for new setups.

**Explicit compatibility boundary:** `Test_ContentArena`, `Test_Waves`, `PlayableMapFixture`, `PlayableMapDestination` and isolated reward fixtures remain outside shared flow in this slice. RewardSelectionUI's prefab `_flow` stays empty; both game scenes bind it with overrides. Without shared management, the legacy reward time/control path and PlayerController cursor path remain supported. Those paths are inactive in target scenes. SceneTravel migration remains separate.

**Validation:** The assistant compiled runtime, EditMode and PlayMode assemblies statically without running Unity tests/builds. The user subsequently reported green tests and no observed gameplay issues. A new build validation was not reported. The existing PlayMode `PlayerChestInteractorTests.TearDown` CS0114 warning is unrelated. Policy tests now contain 25 cases; saved-scene wiring assertions were extended. `RunRestartTests` cover reward rejection during entry/pause, death during rewards, other owners, late old-scene cleanup and ambush-error restart. The ambush intermission test now freezes through a flow token rather than writing global time.

User test targets: EditMode `GameFlowPolicyTests`, `RunRestartOperationTests`, `DemoOpeningLayoutTests`; PlayMode `RunRestartTests`, `DemoEntryTests`, `DemoOpeningTests` and existing `RewardSelectionControllerTests`. Manual acceptance: entry-to-gameplay, pause/resume, multi-claim rewards, death, finale audio/animation and restart from the three main screens, with keyboard/mouse and gamepad. The user provided general test/gameplay acceptance without a separate per-device result or test count.

### Slice A acceptance record (before B2)

The three screens now forward to one serialized `RunRestartController` on Demo opening services in both saved scenes. `RunRestartOperation` guards a single in-flight request and permits retry after startup failure. `UnityRunSceneReloader` alone calls the Editor/player reload API. Existing player lock owners remain intact. No geometry or startup capacity edits.

The old scene stays frozen/muted with restart-owned player locks until it unloads. Pause/final cleanup does not restore global state during a successful restart. A target-scoped arrival marker resets time/audio/cursor before the new entry controller awakens, then existing startup initializes the fresh player. Entry consumes its menu-bypass marker; unmatched scenes discard it.

Failure recovery covers synchronous exceptions and a null loading operation before loading starts. It restores the source time/audio/cursor, clears both markers and removes only restart's locks. After Unity accepts asynchronous loading, the old scene cannot be promised recoverable; no speculative timeout/retry is implemented.

Validation: runtime and EditMode/PlayMode test assemblies compiled statically. Added pure operation tests, saved-scene wiring assertions and PlayMode tests with a fake loader for pause/death/completion, retry, duplicate suppression, owner preservation and unload/arrival. Unity tests/build were not run by the assistant. Manual acceptance: restart from each screen, verify fresh loadout and working controls/audio, repeat rapidly. Full shared time/screen ownership remains Slice B.

## Original findings and responsibilities

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

### Slice B — concrete responsibilities and delivery order

#### B1 implementation status

- `Scripts/Flow/GameFlowContext.cs`: immutable phase/readiness/base-control snapshot supplied by existing entry/health/final systems and calculated access output. This is not another mutable run state machine.
- `Scripts/Flow/GameFlowPolicy.cs`: eligibility, owner reference identity, disposable tokens, idempotent release and pure access calculation. Duplicate acquisition by the same owner is rejected; independent owners may retain the same pause requirement. Pause/reward overlap is rejected.
- `TryChange` updates the same token only for entry menu → approach and finale animation → results. Loading uses a separate owner/token; releasing it on startup failure preserves the source screen. Death/completion reload requires the corresponding visible screen token.
- `Reset` invalidates all old tokens. A delayed old Dispose cannot affect a new run owner; a foreign policy's token cannot be removed. Unity disable/unload integration belongs to B2.
- `FreezeWorld`, `MuteAudio` and `BlockGameplay` are application requests, never commands to enable base permissions. `AllowsGameplay` also respects the supplied base permission. Baseline time/audio are not stored here. Loading captures cursor presentation only, never restores old time/control snapshots.
- `Tests/EditMode/Arenas/GameFlowPolicyTests.cs`: 23 cases covering side-effect-free rejection, out-of-order owner release, loading failure/retry, reward/death overlap, base permissions, atomic cinematic changes, reset and reference identity.

B1 delivery compiled runtime/EditMode assemblies statically without running Unity tests. B1 alone did not change scenes or existing controllers. Subsequent B2 wiring and validation status are recorded above.

Inspection also found cursor writes in `PlayerController.ApplyControlState/OnDisable`. `RewardSelectionController` captures/restores time and a single control boolean. `PlayerDeathController` changes base player/weapon/ability permissions from health events. Editing screen classes alone cannot complete ownership.

Proposed classes under `Scripts/Flow/`; names may be simplified during implementation:

| Part | Owns | Does not own |
| --- | --- | --- |
| `GameFlowPolicy` | Pure C# eligibility rules and effective time/audio/cursor/control requirements from active owners | Unity, loading, health changes, UI animation |
| `GameFlowController` | Scene bindings, ownership requests, reading existing death/final state and forwarding policy output | Another health/run model, rewards or enemies |
| `GameplayAccessController` | Applying calculated output to Unity globals and existing owner-based player locks | Transition decisions, button/camera presentation |
| Existing `RunRestartController` | Accepted reload operation and failure boundary | Pause/reward/finale presentation |

Flow: screen request → policy eligibility → scene-owned registration → effective access state → Unity application. Inspector bindings compose these parts; no additional manager above every system, global singleton or service locator is needed.

**Ownership contract:** Each screen acquires its own token; repeated release is safe. Release removes only that request, never restores a stale global snapshot. Freeze/mute requests combine: any active request keeps the respective restriction. Cursor priority is explicit: Loading > terminal screen/finale > entry > reward > pause > gameplay. Priority does not authorize otherwise invalid screens. Pause and reward are mutually exclusive. Other player lock owners and death-disabled base permissions remain intact.

**Initial behavior target:** Preserve presentation and gameplay.

| State | World time | Audio | Cursor / player |
| --- | --- | --- | --- |
| Gameplay | Session baseline scale | Normal policy | Locked/hidden; input permitted only by base permissions and other owners |
| Entry menu / approach | Frozen | No new global mute | Visible in menu, hidden during approach; player blocked |
| Pause | Frozen | Pause mute | Visible/unlocked; player blocked |
| Reward selection | Frozen | No new global mute | Visible/unlocked; player blocked |
| Death | World continues as currently | Unchanged | Visible/unlocked; death system disables base controls |
| Finale animation / results | Frozen | Preserve finale audio | Hidden/locked during animation, visible/unlocked for results; player blocked |
| Reload | Frozen | Muted | Retain source cursor presentation; screen/player interaction blocked |

Baseline time belongs to the scene session, not an old screen snapshot. Slow motion is outside scope. UI/camera transitions remain unscaled. Player locks do not block menu navigation/submit; preserve existing held-button release guards.

**File changes:**

- `DemoEntryController`: retain camera/view; report menu/approach/gameplay transitions, remove local time/cursor snapshots.
- `DemoPauseController`: retain cards, animation/input; forward open/close requests. Release ownership only after closing animation and input release complete.
- `RewardSelectionController`: retain offers/claims/multi-claim; remove direct time and `SetControlEnabled` restoration. Last claim/cancel releases once; death must not be undone by closing rewards.
- `DemoFinalController`: retain session and presentation/Cinemachine; move global effects to shared ownership and read completion from the existing session.
- `DemoOpeningController`: retain death menu; align visibility/restart eligibility with shared rules.
- `PlayerController`: stop independently writing cursor while shared management is bound. `PlayerDeathController` remains the health-event/base death-control authority; UI release cannot reverse it.
- `RunRestartController`: keep operation/loader. Acquire Loading first; startup failure releases only Loading, recomputing the prior pause/death/completion state.
- Wire both saved scenes and relevant builders/test fixtures. Inventory other scenes/prefabs/tests using reward selection and player control before requiring the shared layer. No silent auto-created managers or hidden demo-only dependencies.

**Small deliveries:**

1. B1: pure policy, ownership/token contract and conflict tests; existing screens remain unchanged.
2. B2: Unity application, scene coordination and migration of all global writers above together. A pause-only migration sharing globals with uncontrolled legacy writers is not an accepted intermediate release.
3. User tests/gameplay acceptance, then separate check-in. No visual redesign or new mechanic.

**Acceptance:** Out-of-order owner releases; repeated token release; UI release with disabled base permissions; rejected requests during entry/pause/reward/finale have no side effects; multi-claim focus; death during rewards; restart while pause closes; startup failure/retry; old-scene cleanup cannot overwrite new time/cursor; no dangling tokens after disable. Manually verify keyboard/mouse/gamepad, animations and finale audio. New run generations invalidate old tokens; explicitly test global application handoff between scenes.

Other test scenes without shared management must be explicitly supported during migration or migrated in the same check-in; document any remaining fallback. At B2 completion, audit code to verify one time/audio/cursor application owner in both target scenes.

A pure tests: acceptance, rejected sources, duplicate requests, loader failure/null operation, retry after failure. Integration: restart from pause/death/completion, entry bypass, fresh build/ammunition, controls blocked during loading, preservation of other owners.

The user runs Unity tests; investigate focused failures when needed. Report compilation, tests and builds separately. Use a test loader for failure simulation, never corrupt real scene assets.

No visual redesign, IMGUI-to-Canvas migration, meta, saves, XP/reward balance, enemies or geometry in A. Preserve current 1/3/5 settings. Exclude unrelated existing material changes and performance-file deletions.

Delivery: TR/EN TDD → Slice A implementation/tests/user acceptance/commit → refine Slice B. This plan does not authorize changing all flow in one increment.
