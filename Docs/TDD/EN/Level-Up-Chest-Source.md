# Level-up chest source

## Scope

One world chest is guaranteed per gained run level, matching the GDD. This stage connects the existing XP and chest foundations; it does not add weighted chest types, enemy/elite/boss drops, item-level rewards, gold fallback, or automatic reward popups. Test_Waves uses the existing development chest definition and reward pool.

Branch baseline: Plastic `/main/dev` **cs:148**, Git `main` **cad6518**. This is a new stage, not a continuation under chest-foundation.

## Ownership

- `LevelUpChestTracker` owns the highest observed run level, pending chest count and successful spawn count. Level 1 gives no chest. Repeated/stale levels cannot duplicate rewards; a jump from 1 to 4 earns three chests.
- `LevelUpChestSource` observes `PlayerExperienceController.LevelChanged` without spawning inside XP notifications. It also reconciles the current level when processing/resuming, so disabled periods do not lose earned levels. Initial binding starts at the player's current level (or 1 before XP initialization); it does not retroactively pay levels earned before the source existed.
- The source processes at most one chest per Update. It waits for initialized XP/spawner, living player and positive time scale. A successful spawn consumes one pending entitlement; no valid location leaves it pending. Retry placement at bounded intervals. A configuration/spawn exception is logged once and disables automatic processing without consuming the entitlement.
- The tracker survives disable/re-enable, but is run-scoped. Destroying/recreating the source, save/load and cross-scene persistence are not supported by this increment.
- `ChestSpawnerBootstrap` initializes the shared spawner after build initialization. Level-up spawning does not rely on the Editor-only starter-chest bootstrap; the starter chest remains a separate test fixture.

## Placement

`ChestSpawnPlacement` searches three rings near the player's current position (2.5, 4.25, 6 metres), beginning in front. Eight candidates per ring are bounded work, not an unbounded search. Use the current upright, root-BoxCollider chest prefab's scaled dimensions. Ground sampling covers the center and footprint corners; reject missing/uneven support, blocked volumes, and direct paths obstructed by solid colliders. Ground uses the Environment layer; obstruction checks ignore triggers/XP orbs. Publish new colliders to physics before processing another chest to prevent overlaps in a multi-level burst.

This is a flat-arena placement policy, not a general navigation/pathfinding system. If all locations are blocked, the player can move or open existing chests; the pending reward is retained and placement retries near the player's new position. Death/pause defers processing rather than deleting rewards.

## Existing reward behavior

Spawning a chest does not open its reward screen. The player still looks at it and presses E / gamepad South. With no eligible rewards the chest stays available, as in chest foundation; this source does not invent a gold fallback. Once the small development reward pool is exhausted, later chests may therefore have nothing to offer until reward progression is expanded.

## Verification

Automated tests cover multi-level accounting and duplicate observations, initialization and pause/death/disable handling, late spawner readiness, successful versus failed placement/spawning, retry after movement or newly available space, and ground/obstruction/footprint checks. Scene tests verify shared spawner, XP/player references and definition wiring.

Manual Test_Waves: the initial development chest is unchanged. Collect four 25-XP orbs to reach level 2; one additional chest should appear on nearby free ground, without opening the UI. Open it with E. Gain another 150 XP for the next chest. Check death/pause and placement near an arena edge or an existing chest. Multi-level gains must yield one distinct chest per level, not one per callback.
