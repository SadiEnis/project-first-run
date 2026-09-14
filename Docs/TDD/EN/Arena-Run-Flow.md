# Arena and run flow foundation

## Status and scope

This increment extends the existing single-arena session into a safe run lifecycle. It deliberately does not add scene transitions, arena modifiers, bosses, gold, meta progression or evolution gameplay.

## Accepted behavior

- A ready arena starts its configured wave sequence exactly once.
- Completing the final wave produces one terminal Victory state.
- Player death while the session is running stops the active wave, removes active enemies and produces one terminal Defeat state.
- A terminal result cannot be overwritten by a late wave completion or a second death notification.
- A finished session may restart only when the player is alive. Restart creates a fresh wave sequence and publishes a new session start event.
- Restart while running, before initialization, or while the player is dead is rejected.

## Technical boundaries

ArenaSessionController owns session state and terminal result events. WaveController owns wave sequencing and exposes stop/restart lifecycle operations. Stopping a running sequence unsubscribes tracked enemies, disables and destroys active wave instances, and marks the sequence Failed. Runtime systems remain explicitly injected; no global singleton or scene search is introduced.

The current development bootstrap remains the composition root for the test scene. Multi-arena orchestration and production scene loading are subsequent increments.

## Verification

EditMode covers state transition invariants. PlayMode covers start, final-wave victory, defeat cleanup, terminal-result protection and restart freshness, alongside the existing wave, chest, XP and enemy regressions.

Validation on 2026-09-14 passed 700/700 EditMode and 381/381 PlayMode tests in the isolated Unity 6000.3.9f1 project.
