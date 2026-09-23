# Map, region and encounter progression plan

## Status and scope

20 September 2026: Foundations for stages 1–8 and an authored traversal fixture for stage 9 exist as scoped below. The user observed returnable side passages, one-way traversal, arrival in the target scene and Ready–Ready → Running–Ready preparation/activation. This acceptance does not establish a complete gameplay loop or current automated test/build validation. Stage 10 remains incomplete; the older wave-based fixture remains a separate test flow.

This plan supersedes the assumptions that every arena is a separate scene and every progression requires clearing all waves. Gold and meta progression belong to the intended game loop, but their implementation is deferred during this map stage. New evolution content and procedural generation are also out of scope.

## Concepts and accepted mechanics

| Concept | Responsibility |
| --- | --- |
| Map | For the first prototype, an explorable Unity scene containing multiple regions and potentially exits. |
| Region | A main area, side room or cave inside a map; not necessarily a scene. |
| Encounter | An enemy group or wave sequence; completion does not automatically complete the map. |
| Transition | A request to advance between source and destination; eligibility, traversal and door presentation are separate. |
| Run | Player progression across maps and the final outcome. |

- Players may skip optional combat and reach an exit, trading away XP and development opportunities. Balance will be evaluated later.
- This freedom is not limited to side dungeons: main areas with free exits also allow leaving enemies behind and crossing a passage that closes afterward. One-way traversal is independent of encounter completion requirements.
- Intended outer loop: wake in hub/meta area → enter run → death returns to the same hub/upgrade area → spend retained run gold on permanent improvements → new run. This decision does not redefine gold retention rates or hub presentation; existing economy proposals need separate design.
- Encounter completion gates only explicitly configured special passages. Remaining enemies do not globally lock map exits.
- Returnable side areas and one-way connections will be supported. One-way travel does not imply a checkpoint or save.
- Walking through a corridor/stairway can be a same-scene transition; teleporting is optional. Scene loading is a separate traversal method.
- Keep transitions generic rather than introducing decoration-specific `GateTrigger` / `PitTrigger` classes.
- Separate preparation from combat activation. Prepare enemies before they become visible; verify hitch targets through profiling, not merely an async API.
- Region changes do not reset health, equipment, ammunition, XP or cooldowns. Cross-scene data transfer requires its own design.
- Ordinary encounter completion and map exit are not run victory. The final objective is separate.
- Begin with one handcrafted layout; multiple variants are not required.

## Migration inventory

| Component | Retain / change |
| --- | --- |
| `WaveController`, `WaveEnemyTracker` | Retain local encounter spawning/completion tracking; not the sole authority for map progression. |
| `ArenaSessionController` | Treat wave lifecycle as encounter scope and separate automatic map completion. Renaming is not yet decided. |
| `RunSessionController` | Revisit the fully initialized sequential session list and victory-driven advancement; distinguish map exits from encounter completion. |
| `ArenaTransitionTrigger` | Retain generic colliders and explicit player identity; adapt source arena index to the destination model. |
| `ArenaTransitionController` | Separate eligibility, traversal and destination resolution; free exits must not require arena victory. |
| `PlayerMotor.Teleport` | Retain optional local relocation; walking connectors need not call it. |
| Development bootstrap / `Test_Waves` | Retain the existing verification fixture; it is not the explorable prototype scene. |

## Delivery order

Discuss mechanics and update TDD before each stage. Stages 1–8 are scoped below; stage 8's terminal/restart foundation in the sequential arena controller is not a hub-return integration in the new map fixture. Stage 9 has a technical traversal fixture; stage 10 covers integration gaps and validation. Reentry and continuously active encounter decisions are recorded below.

1. **Design checkpoint — this document:** Record GDD direction, terminology, migration inventory and sequence. No runtime changes.
2. **Region and encounter lifecycle:** Define preparation, activation, completion and reentry. Test that encounter completion does not finish the map.
3. **Generic transition contract:** Free/encounter-gated eligibility, destination, one-way/returnable direction and walking/relocation/scene-loading separation. Test incorrect players and duplicate requests.
4. **Passage barrier:** Presentation-independent opening/closing signals; test full player clearance before closing, trapping prevention and repeated triggers.
5. **Prewarming — single-group integration implemented:** Separate preparation/activation volumes, count/time budgets, deferred entry and readiness gating are implemented. Measurements and validation are in Region-Preparation. Wave-sequence prewarming and pooling are not implemented.
6. **Resource continuity — implemented:** Shared current-region state and a map-wide traversal lock; player state remains in place. Loot belongs to its source map scene and survives region/enemy cleanup. Return, one-way traversal and skipped combat are covered in Map-Progression-Resources.
7. **Cross-scene travel — foundation implemented:** Inspector-configured scene path and entry identity, additive loading, original player transfer, destination dependency binding, source cleanup and failure/retry behavior. See Scene-Travel for contracts and test scope; packaged-build verification remains part of playable-map integration.
8. **Run outcome and restart — foundation implemented:** Death while exploring/travelling, final victory and clean restart through an atomic reset callback. Checkpoints/saving are not implicitly included; see Run-Outcome-Restart.
9. **Playable fixture — authored integration exists:** Main area, two returnable side areas, preparation/activation volumes, one-way second main area and exit to a small second scene. Existing enemy/XP/level-up chest services, baked navigation, boundaries and sight-screen walls are wired. This is still a validation fixture, not the final demo or a complete run loop.
10. **Gameplay and performance validation — in progress:** Full suites pass (752 EditMode / 449 PlayMode), including saved-scene traversal/reentry/reward integration and a 32-enemy preparation probe. See Playable-Map-Fixture for measurements and their limitations. Rendered short/exploration route evaluation and cold-start profiling remain; pooling is not decided from warm batch timings alone. Packaged-build evidence is recorded separately there. Map-based death/final outcomes must be addressed independently of the old arena list; callback restart is not a completed hub loop.

Initial design and adaptation stay on `feature/multi-arena-run`. Each numbered stage does not require its own branch or commit. Future independent stages start from Plastic dev / Git main; the user performs merges.

## Decisions and remaining design topics

### Order after map work — 20 September 2026

1. Close arena/map integration gaps and stage 10 acceptance criteria.
2. Expand content: add mechanically distinct weapons, abilities and upgrades in small packages. Random chest offers and player choices should create varied builds; numeric variation alone is insufficient. Discuss mechanics and TDD before each content package.
3. Design the playable demo map: arrange the main route, optional dungeons/side paths, encounters and growth opportunities using this content; evaluate time–risk–reward balance. The current technical fixture is not the final demo map.

A boss is not a mandatory stage before content expansion; its scope will be decided separately against demo needs. Gold/meta implementation and evolution gameplay remain deferred. Whether the demo includes the hub/economy needs a separate scope decision; technical validation does not imply those systems are ready.

### Lifecycle decisions

- Decided: reentry preserves existing encounter state and surviving enemies' health without respawning.
- Can enemies pursue outside their region, and how do ownership and pursuit boundaries differ?
- Decided: an active encounter keeps running when the player leaves; suspension optimization is deferred.
- Decided for the prototype: prepared but unvisited groups remain until their owner is explicitly disabled/destroyed or the map scene unloads; no distance-based eviction.

Pursuit boundaries remain open. Accepted lifecycle and resource rules are documented in Region-Encounter-Lifecycle and Map-Progression-Resources.

## Verification and versioning

Create a docs check-in/commit for a coherent design package, then meaningful runtime/test increments. Do not commit every small file operation separately. The original checkpoint was documentation-only; subsequent implementation and its dated verification evidence are recorded in the corresponding stage documents. Do not treat an older test result as validation of a later change.
