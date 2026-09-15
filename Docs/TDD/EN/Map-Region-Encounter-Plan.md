# Map, region and encounter progression plan

## Status and scope

15 September 2026: Accepted design direction and staged delivery plan, not an implemented system. The existing multi-arena prototype still unlocks a same-scene relocation after wave completion.

This plan supersedes the assumptions that every arena is a separate scene and every progression requires clearing all waves. Gold, meta progression, new evolution content and procedural generation are out of scope.

## Concepts and accepted mechanics

| Concept | Responsibility |
| --- | --- |
| Map | For the first prototype, an explorable Unity scene containing multiple regions and potentially exits. |
| Region | A main area, side room or cave inside a map; not necessarily a scene. |
| Encounter | An enemy group or wave sequence; completion does not automatically complete the map. |
| Transition | A request to advance between source and destination; eligibility, traversal and door presentation are separate. |
| Run | Player progression across maps and the final outcome. |

- Players may skip optional combat and reach an exit, trading away XP and development opportunities. Balance will be evaluated later.
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

Discuss mechanics and update TDD before each stage. Stage 1 is documented, stage 2 has its lifecycle core, stages 3–4 have the corrected transition contract and walking/barrier integration, stage 5 supports single-group preparation, stage 6 adds shared map-local traversal/resource ownership, and stage 7 adds cross-scene travel with preserved player runtime and explicit map bindings. Validation scope is recorded in the respective documents. Stages 8–10 remain planned.

1. **Design checkpoint — this document:** Record GDD direction, terminology, migration inventory and sequence. No runtime changes.
2. **Region and encounter lifecycle:** Define preparation, activation, completion and reentry. Test that encounter completion does not finish the map.
3. **Generic transition contract:** Free/encounter-gated eligibility, destination, one-way/returnable direction and walking/relocation/scene-loading separation. Test incorrect players and duplicate requests.
4. **Passage barrier:** Presentation-independent opening/closing signals; test full player clearance before closing, trapping prevention and repeated triggers.
5. **Prewarming — single-group integration implemented:** Separate preparation/activation volumes, count/time budgets, deferred entry and readiness gating are implemented. Measurements and validation are in Region-Preparation. Wave-sequence prewarming and pooling are not implemented.
6. **Resource continuity — implemented:** Shared current-region state and a map-wide traversal lock; player state remains in place. Loot belongs to its source map scene and survives region/enemy cleanup. Return, one-way traversal and skipped combat are covered in Map-Progression-Resources.
7. **Cross-scene travel — foundation implemented:** Inspector-configured scene path and entry identity, additive loading, original player transfer, destination dependency binding, source cleanup and failure/retry behavior. See Scene-Travel for contracts and test scope; packaged-build verification remains part of playable-map integration.
8. **Run outcome and restart:** Death while exploring/travelling, final victory and clean restart. Checkpoints/saving are not implicitly included.
9. **Playable fixture:** Main area, two returnable optional areas, preparation connector, one-way second main area and exit to a small second scene. Placeholder geometry is sufficient.
10. **Gameplay and performance validation:** Compare short/exploration routes; measure transition frame spikes, visible spawning and resource loss.

Initial design and adaptation stay on `feature/multi-arena-run`. Each numbered stage does not require its own branch or commit. Future independent stages start from Plastic dev / Git main; the user performs merges.

## Decisions and remaining design topics

- Decided: reentry preserves existing encounter state and surviving enemies' health without respawning.
- Can enemies pursue outside their region, and how do ownership and pursuit boundaries differ?
- Decided: an active encounter keeps running when the player leaves; suspension optimization is deferred.
- Decided for the prototype: prepared but unvisited groups remain until their owner is explicitly disabled/destroyed or the map scene unloads; no distance-based eviction.

Pursuit boundaries remain open. Accepted lifecycle and resource rules are documented in Region-Encounter-Lifecycle and Map-Progression-Resources.

## Verification and versioning

Create a docs check-in/commit for a coherent design package, then meaningful runtime/test increments. Do not commit every small file operation separately. This checkpoint changes documentation only; earlier Unity test results do not validate the proposed system.
