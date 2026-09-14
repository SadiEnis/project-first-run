# Multi-arena run foundation

## Universal transition increment (implementation contract)

`ArenaTransitionTrigger` is independent of environment art. Any trigger collider shape can represent an exit. It references an `ArenaTransitionController` and a zero-based source arena index. Only the explicitly assigned player's colliders may request a transition; enemies and unrelated colliders are ignored. An exit is available only while its own arena is completed and the run awaits transition. Final victory, defeat, pause, disabled components and duplicate entries cannot advance the run. Standing inside a locked exit requires leaving and re-entering once unlocked.

The transition controller owns the ordered destination entry points and relocates the existing player before starting the next arena. CharacterController enable state is preserved, falling velocity is reset, and entry yaw is applied. Health, XP, equipment and item levels remain on the same player instance. A failed arena start ends the run as Defeat rather than leaving a phantom Running state. Death while waiting at an exit also ends the run.

This increment uses same-scene destinations; asynchronous scene loading and art-specific effects remain future work. Test_Waves demonstrates two logical arena sessions using the existing layout, with a marked generic exit and a second entry point. It is a transition showcase, not two finished arena layouts. Existing drops remain in the scene; cross-scene loot cleanup policy is deferred. Code, tests and scene wiring form one meaningful implementation checkpoint.

### Scene authoring

1. Initialize the arena sessions, then initialize RunSessionController with their ordered list and the player's death source. Pass the death source to observe deaths during the between-arena wait.
2. Assign the run, player root, player health and ordered entry-point transforms to ArenaTransitionController (Inspector or Configure). Entries must match the run's arena count. Entry yaw controls arrival orientation; camera pitch is retained.
3. Add ArenaTransitionTrigger to the same object as an `Is Trigger` collider. Assign the transition controller and source arena index. Box, sphere, capsule or a Unity-supported trigger mesh can be used. Standard Unity physics trigger requirements apply; a kinematic Rigidbody on the trigger works with the showcase player's CharacterController.
4. Keep the entry clear of obstacles and other exit volumes. The exit sends only the source index; the ordered run selects the destination. Multiple exits for one arena share this routing and cannot skip arenas.

Test_Waves enables the optional showcase on ArenaSessionDevelopmentBootstrap via `_secondArenaDefinition`. The cube at world (0, 1, 7) turns green after both waves; entering places the player at (0, 0.1, -5) and starts the second session. The Run panel shows arena index and final result. This fixture uses Editor development bootstraps; production composition is still separate work.

Transition validation completed across 2026-09-14/15: 706/706 EditMode and 395/395 PlayMode passed in isolated Unity 6000.3.9f1. Nine new PlayMode cases cover a real physics sphere trigger, the actual Test_Waves two-session flow, player identity and arrival ordering, health preservation, duplicate/wrong-source rejection, pause and missing destinations, death during transition, failed arena start, CharacterController fall reset, and retry after invalid run initialization. Gameplay acceptance by the owner remains pending.

## Status and scope

This increment adds run-level orchestration above the existing single-arena session. It does not load scenes, add arena modifiers, bosses, gold or meta progression.

## Accepted behavior

- A run receives an ordered, non-empty list of already initialized arena sessions.
- Beginning a run starts only arena zero and publishes the run/session start events.
- Victory from the active arena advances the run to a Transition state and publishes the completed arena index.
- The next arena starts only after an explicit `AdvanceToNextArena()` call.
- Victory from the final arena completes the run exactly once.
- Defeat from the active arena completes the run as Defeat; stale events from inactive or finished arenas are ignored.

## Technical boundaries

`RunSessionState` owns pure run progression and arena indexing. `RunSessionController` owns injected `IArenaSession` orchestration and transition events. Arena implementations remain responsible for their own wave lifecycle. Scene loading, transition presentation and production composition are subsequent increments.

## Verification

EditMode covers run-state transitions and invalid order. PlayMode covers active-arena routing, explicit transitions, final victory, defeat protection and the existing gameplay regressions.

Validation on 2026-09-14 passed 706/706 EditMode and 386/386 PlayMode tests in the isolated Unity 6000.3.9f1 project.
