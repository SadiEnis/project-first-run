# Transition barrier lifecycle

## Correction contract — 15 September 2026

TransitionBarrier tracks collider identities in a set. Duplicate identities are ignored; distinct colliders count separately, even when closed. Closing waits for the last identity to clear. Open cancels deferred closure; notifications fire once per state change.

RegionPassageController scans the assigned player's active non-trigger colliders. AABB overlap is conservative: irregular volumes can delay closure. The safety volume must contain the entire optional physical blocker, using distinct colliders. Invalid containment disables the blocker and rejects travel.

Add contacts before removing old ones so collider replacement never briefly reports empty. Destroyed/disabled colliders are removed on the next scan. Enable the blocker only when closed and empty. Disabling cancels pending work and leaves the blocker disabled; enabling rescans occupancy.

The trigger transform's local Z axis defines direction. Returning to the entry side cancels; fully clearing the target side completes. After OneWay completion, the destination region identity rejects reverse travel and closes the barrier. Opened/Closed events support presentation; animated collision geometry is outside this increment.

Safety covers a stationary blocker and correctly sized volume. PlayMode fixtures exercise compound colliders, retreat, death/pause, disabling and real physics-step traversal.

## Setup and limits

- Add RegionPassageController to the safety trigger's GameObject. Use a kinematic Rigidbody for the trigger and a separate GameObject/collider for the solid blocker.
- Assign Player, Health, endpoint identities, Direction and Requirement. Initial region is SourceId; local +Z must face the destination.
- For gated travel, scene composition binds its RegionEncounterSession before Start via BindEncounter or passes it to Configure.
- Opened/Closed UnityEvents can drive presentation. This component owns blocker activation; other scripts must not toggle the same collider.
- Scanning occurs at fixed physics steps; swept detection for movement that skips the whole volume in one step is not implemented. Size the volume for player speed.
- Usage/direction are per-connection state; shared map location across multiple passages and scene loading belong to later flow integration.

## Validation — 15 September 2026

EditMode: 733/733 passed. PlayMode: 408/408 passed, including 10 RegionPassageTests and the previous 3 RegionEncounterIntegrationTests. Sideways exits do not count as success. Local results: .codex-temp/xp-attraction/passage-review-edit.xml and passage-review-play.xml. Test_Waves gameplay is unchanged by this package.
