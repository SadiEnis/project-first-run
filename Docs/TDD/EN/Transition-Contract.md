# Generic transition contract

## Implementation contract — 15 September 2026

This stage separates the transition decision from decoration and physical barriers. `ArenaTransitionController` continues to serve the current prototype; this contract is the decision core for new region/map travel.

Each direction of a connection is an independent `RegionTransition` definition:

- `SourceId` and `DestinationId` are stable identities independent of scene object names.
- `Free` eligibility does not wait for the source encounter.
- `EncounterCompleted` requires only the relevant region encounter; it does not query every enemy on the map.
- `Walk`, `Relocate` and `SceneLoad` describe traversal. All use the same decision contract.
- `Returnable` can be used repeatedly. `OneWay` is consumed after a successful use and cannot be used again in that direction.
- `TryUse` does not consume a failed attempt. Acceptance is atomic and occurs once.
- An unavailable destination rejects the request; this class does not spawn enemies, load scenes or move the player.

This stage does not subscribe directly to `RegionEncounterSession`; the caller passes `Status == Completed` explicitly. Free exits therefore never become implicitly coupled to encounter lifecycle.

## Acceptance tests

EditMode tests cover identity validation, free travel, incomplete/complete encounter gating, destination availability, returnable reuse, one-way consumption, failed-attempt state preservation and traversal metadata.

The existing `ArenaTransitionController` tests continue to enforce that physical triggers accept only the real player collider. Trigger, barrier presentation and scene target wiring connect to this contract in later increments.
