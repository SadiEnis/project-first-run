# Generic transition contract

## Correction contract — 15 September 2026

RegionTransition is a two-ended connection. OneWay accepts SourceId → DestinationId only; Returnable accepts both directions. Independent singleUse defaults to false.

- CanBegin checks current region identity, the linked encounter requirement, destination readiness and pending work.
- TryBegin issues a distinct Attempt; requests in either direction are rejected while pending.
- Complete(attempt) commits only the current attempt and consumes single-use here.
- Cancel(attempt) allows retry without consumption. Stale/foreign attempts cannot complete or cancel newer work.
- Execution/load failures must call Cancel. Walk, Relocate and SceneLoad are metadata; actual scene loading belongs to roadmap stage 7.
- EncounterCompleted checks the explicitly linked encounter. The same requirement applies in both directions.

## Walking integration

RegionPassageController connects the contract to a player, encounter, generic trigger volume and optional physical blocker. Local +Z is destination, -Z source. Entry begins an attempt. Once all physical player colliders clear the safety volume, exiting the target side completes; backing out cancels. Success changes CurrentRegionId without map/run victory.

This component executes walking. Other methods need separate executors using the same Attempt protocol. Test_Waves retains its legacy adapter; PlayMode fixtures exercise the new component.

## Acceptance tests

Direction, independent single-use, failed traversal retry, duplicate requests, stale completion, wrong source and enum validation; real Unity collider fixtures for return trips, retreat, gated passages and one-way closure.
