# Run outcome and restart — stage 8

## Contract

- A run is `Ready`, `Running`, `Transition`, `Victory` or `Defeat`. Death from combat, exploration or a scene-travel attempt publishes one `Defeat` and leaves the run terminal. A later arena victory, passage completion or late death event cannot change that outcome.
- Final run victory is emitted once after the configured final arena session completes. A normal region exit or a scene arrival never emits run victory; a future final-objective adapter may call the run outcome explicitly.
- Restart is an explicit operation after `Victory` or `Defeat`: the caller supplies one atomic world/player reset callback. The callback must remove/reset encounters and transient world objects, reset player health/control and prepare every arena session in `Ready`. If it throws or leaves a session/player invalid, the old terminal state remains and no new arena begins.
- Once reset succeeds, the controller creates a fresh run state, begins only the first session, and emits `SessionRestarted`/`SessionStarted`. No checkpoint, save, meta currency, or old map restoration is implied. The caller chooses whether the scene is reloaded, the existing map is rebuilt, or an authored reset is used.
- The existing player object may be reused by the reset callback, but the callback owns clearing build/XP/item state. This stage does not decide permanent meta progression.

## Verification

Tests cover death during an arena and transition, exactly-once defeat, terminal-event immunity, successful callback-driven restart, callback failure rollback, invalid player/session state, and fresh first-arena start. Scene travel cancellation remains covered by Scene-Travel; the restart coordinator consumes its terminal outcome but does not own scene loading.

Verified: **747/747 EditMode**, **445/445 PlayMode**, no failures. Local reports: `.codex-temp/xp-attraction/run-outcome-final-EditMode.xml` and `run-outcome-final-PlayMode.xml`.
