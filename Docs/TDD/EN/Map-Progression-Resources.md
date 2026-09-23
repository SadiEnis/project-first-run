# Map-local progression and resources

## Stage 6 contract

One map owns one `MapTraversalSession`, exposed by an Inspector-configured `MapTraversalController` (initial region and unique region IDs). Every walking passage in that map references the same controller. Region IDs are case-sensitive. The location changes only after the whole player clears the destination side; entering, backing out, pausing, dying or cancelling does not commit progression. Only one passage may have a pending traversal for that map. Unknown endpoints fail configuration. Standalone two-region fixtures may omit the controller; this fallback is not appropriate for a multi-passage map.

Returnable passages support optional side paths; one-way passages disallow their reverse traversal without globally deleting previous regions. Free passage does not require defeating remaining enemies. Explicit encounter-gated routes retain that requirement. No traversal awards encounter victory, run victory or checkpoint/save state.

## Ownership and lifetime

- Keep the same player instance, build, item levels, XP state and weapon runtime entries. Do not serialize/recreate the player on a local crossing. Health and ammunition are not reset; gameplay timers continue normally.
- Region leave does not dispose or disable its encounter. Surviving enemies and their current health remain; pursuit boundaries and suspension are deferred.
- Uncollected XP and unopened chests have no region-exit timeout. They remain independent world objects until collected/consumed or their owning map scene unloads. Neither one-way travel nor encounter cleanup grants or deletes them.
- Spawned loot belongs to the source object's scene, not whichever scene happens to be active. Keep it at scene root, outside enemy/preparation ownership. This also defines safe ownership for the later additive-scene work without implementing scene travel now.
- Prepared but unvisited groups remain until their owner is explicitly disabled/destroyed or its scene unloads. No distance-based eviction in this stage.

## Verification and scope

EditMode covers shared route eligibility, forward/return/one-way travel, global in-flight exclusion, cancellation, stale attempts and invalid region configuration. PlayMode covers actual collider-driven passages sharing location, cancellation/death releasing the map lock, resource continuity with the existing player prefab, source-scene loot ownership/lifetime, and leaving a running encounter through a free passage without victory or despawn. Existing preparation tests cover living enemies retained across leave/reentry.

Verified: **739/739 EditMode**, **427/427 PlayMode**, no failures. This increment adds 6 EditMode and 7 PlayMode cases. Local reports: `.codex-temp/xp-attraction/map-resources-edit.xml` and `map-resources-play.xml`. Tests run in the isolated Unity project; no manual scene-layout or performance claim is implied.

No playable scene changes, cross-scene player transfer, teleport integration, checkpoints, loot catch-up, pooling or boundary AI are included. Scene travel is stage 7; the handcrafted playable map is stage 9. `Test_Waves` remains the legacy fixture.
