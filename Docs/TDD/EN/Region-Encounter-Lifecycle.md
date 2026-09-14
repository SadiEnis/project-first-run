# Region and encounter lifecycle

## Implementation contract — 15 September 2026

This is stage two of the [progression plan](Map-Region-Encounter-Plan.md). Add a testable lifecycle first; physical preparation/entry triggers, pursuit bounds, new transition eligibility and the playable scene follow later. Keep the existing `Test_Waves` flow unchanged.

- `RegionEncounterSession` owns a stable region identity and one local encounter in this increment.
- `Prepare` binds an already initialized, Ready `IArenaSession`. Preparation currently means dependency readiness, not enemy pre-instantiation or pooling.
- States: Unprepared → Prepared → Active → Completed or Failed. Player presence is a separate `IsPlayerInside` value.
- First `Enter` begins the encounter once. Duplicate entry never respawns. `Leave` only updates presence; it does not stop, destroy or heal enemies.
- Reentry resumes the same active encounter. Completed encounters never restart on entry. Failed sessions cannot restart; a new run must construct new sessions.
- Active encounters may complete or fail on player death while the player is outside. Publish terminal notification once.
- Completion is local. This class never calls `RunSessionController`, advances a map or opens a door. Future transition logic subscribes explicitly.
- The region session exclusively owns encounter lifecycle: do not also bind its encounter to the legacy run list or externally Begin/Restart it.
- `Dispose` removes subscriptions without destroying world objects. The scene owner disposes the session; enemy/scene cleanup remains separate.

## Acceptance tests

EditMode: invalid preparation, preparation without starting, entry requires preparation, duplicate entry, leave/reentry, duplicate terminal notifications, startup failure, synchronous completion and subscription cleanup.

PlayMode: real `ArenaSessionController` / `WaveController` preserve spawn count and wounded enemy identity/health on reentry; completion while outside; no respawn after completion; player death while outside.

Pursuit is not coupled to region presence; existing AI continues unchanged. Distant-region suspension, enemy prewarming and resource release are not performance claims of this increment.
