# Transition barrier lifecycle

## Implementation contract — 15 September 2026

This stage defines barrier state independently from physical/visual presentation. `TransitionBarrier` knows nothing about a door model, animation or collider shape; a view layer listens to `Opened` and `Closed`.

- The barrier starts `Closed`; the first `Open` publishes once.
- A closed barrier rejects entry. A duplicate collider entry from the same player does not create another operation.
- A close request while the player is inside leaves the barrier open and defers the request.
- Once the player is fully clear, the deferred close is applied once, preventing the player being trapped in the passage.
- With no player inside, close applies immediately. Repeated close on a closed barrier publishes nothing.
- Reopening cancels a deferred close without publishing a duplicate open event.
- This class does not own transition eligibility, teleportation, scene loading or animation duration.

## Acceptance tests

EditMode covers idempotent opening, closed/duplicate entry, deferred close while occupied, close after the player clears, immediate close when empty, duplicate close and reopening that cancels a pending close.
