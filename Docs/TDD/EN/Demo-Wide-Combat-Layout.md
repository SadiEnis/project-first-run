# Alternative wide combat layout

Separate greybox experiment requested by the user, not a replacement for the accepted `DeepJam_Opening` scene. Target scene: `DeepJam_WideCombat`, with independent navigation data.

## Wide-area encounter density

### Front-floor addition

Add six Chasers between the stairs at X = ±7, Z = 110/120/130, Y = -19.9, preserving rear positions. Main arena total becomes 24 (15 Chasers, 6 Rangers, 3 Chargers). Append anchors to ArenaLeft; no additional wave/trigger or perception changes.

Authoring: change Entries/Count in the encounter's Group asset, create empty child Transforms on navigation, then assign them to Spawn Points. Entries consume anchors in order; insufficient anchors wrap around and overlap enemies. Adding only an anchor does not increase counts. Edit outside Play Mode and save.

- WideCombat-only encounter assets: second room 6 (4 Chasers, 2 Rangers); main arena 18 (left 9 Chasers; right 6 Rangers and 3 Chargers). Existing composition ratios are preserved.
- One distinct grounded spawn anchor per enemy, outside cover and doorways. Preserve preparation/perception; no extra waves or mandatory clear conditions.
- Corridor 4, optional room 3 and key ambush 3+3 remain unchanged. Shared assets and the accepted demo are untouched.
- Health, damage, XP and drop rates stay unchanged; higher total XP/drop opportunities require gameplay evaluation. Counts are an initial density pass.
- Tests cover isolated assets, composition/counts and unique spawn anchors. Unity test execution remains with the user.

Implemented three EW_Wide assets and 15 additional spawn anchors. Geometry/navigation were not changed; no additional bake is needed for counts alone if the previous layout has already been baked. Unity tests were not run.

## User-authored layout adaptation

- Preserve the narrowed corridor, smaller reward room and unchanged second room. Move the reward anchor inside its floor.
- The arena is now 48 × 71.3016 m with its walking surface at Y = -19.9; retain the overlook at -3. Both stairways use 68 risers below the player's 0.3 m step offset.
- Align optional room, parkour, key room, return route and finale floors to the arena. Move side connections past the longer stairs, preserving jump gaps and fall recovery. Former short key ascent/descent sections become level connections.
- Update perimeter walls, cover, spawn anchors and trigger volumes. Preserve the user's relocated finale room; trim the overlapping approach corridor to start at the arena end. Relocate the gate, entry volume and presentation targets accordingly.
- Enemy counts, rewards and damage are unchanged. The accepted original scene is untouched. Rebake the independent WideCombat navigation after this edit; the old bake is stale.

## Adaptation delivery

The saved scene now follows the revised layout: 68 risers per stairway (approximately 0.249 m rise / 0.55 m tread), side entrances at Z = 145.2 and final gate at the arena end Z = 170.5015. The former 2 m key-route elevation is removed; connected walking surfaces share Y = -19.9. User-authored main floor dimensions and second-room geometry are preserved.

Seven EditMode checks cover floor alignment/references, parkour spacing, stair continuity, reward/finale anchors, balcony occlusion, independent navigation and baked spawn/route reachability. Static validation found no missing local references or duplicate fileIDs. Unity tests/play validation were not run; navigation has not yet been rebaked after this edit.

Reload the current saved scene from disk. With WideCombat active, run `Project First Run → Demo → Rebake Demo Navigation` and save; do not overwrite disk edits by saving a stale open scene. Then exercise both stairs, side routes, key ambush and finale. Build startup is unchanged.

## Initial design (before user layout edits)

- Main arena grows from 30×28 m to approximately 48×36 m, with perimeter circulation, two stair approaches and cross-routes between cover.
- Opening corridor approximately 13×30 m; second combat room approximately 23.5×24 m. Preserve readable doorways; move geometry and trigger bounds together.
- Keep balcony elevation. Staggered tall screens conceal some enemies from the balcony without obscuring the entire arena. Keep stairs and route entrances clear.
- Do not change enemy counts/definitions, health/damage, reward tables or XP. Distribute arena spawns among the new cover pockets.
- Preserve relative parkour platform dimensions/gaps, fall damage and return anchor. Translate the key room and ambush pockets as a unit. Keep the finale's internal dimensions.
- Preserve optional room, key route, return corridor and final gate connections. The result is a directly editable saved scene, not a runtime map generator.

## Acceptance

Open the alternative, bake its own NavMesh and save. Walk the entire route through ending and replay. Assess balcony and floor separately: incentive to descend, circulation around cover, enemy movement, clear doorways and stairs. Evaluate whether the same enemy count makes the larger space feel empty; this experiment does not imply balance acceptance.

Static/editor checks cover unchanged original scene/navigation, preserved parkour dimensions and resolved alternative gameplay references. Unity gameplay and bake verification remain required. Do not automatically replace the build startup scene with this alternative.

## Delivery status

Created the saved alternative. Main arena: 48×36 m; second room: 23.5×24 m. Staggered tall cover and repositioned six arena spawns. Earlier rooms/transitions are widened horizontally while retaining elevations. Parkour/key area is translated by (+9, 0, +26.2), preserving jump gaps and ambush room scale. Original scene and OpeningNavigation.asset are unchanged.

Use `Project First Run → Demo → Open Wide Combat Greybox`, then `Rebake Demo Navigation` and save. The first bake creates `WideCombatNavigation.asset`; this scene initially does not share the old NavMesh. Bake before Play. Existing Build Windows Opening still builds the original; the alternative is not automatically included in the submission build.

DemoWideCombatLayoutTests contains four EditMode checks: dimensions/gameplay references, preserved parkour dimensions, cover hiding at least one spawn per lane from the balcony, and independent navigation data. The last check requires the first bake. Unity tests and gameplay were not run for this delivery. Static validation found no duplicate scene fileIDs; user settings files were untouched. Evaluate whether unchanged enemy counts make the enlarged space feel empty during gameplay.
