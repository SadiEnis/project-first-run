# Sniper Bomb — content design

## Status

Eighth and final planned base ability on abilities-content, following GDD 12.8. The user accepted the targeting, homing and retarget rules below; revisions remain possible after gameplay testing. This checkpoint is documentation only. No implementation or test execution yet; no evolution.

## Agreed behavior

- Acquire a living, visible current-map enemy within 20 m of the player. Priority is Boss, then Elite, then Normal; choose the nearest within the highest available rank. No eligible target means no cooldown consumption.
- Fly at 10 m/s, continuously steering toward the locked target. No navigation/pathfinding around obstacles. Sweep movement to prevent tunnelling.
- Contact with an enemy or solid world obstacle detonates the bomb at that contact point. Enemy contact does not add separate direct-hit damage: explosion damage is applied once per eligible enemy.
- Explosion radius is measured from the actual detonation, with line-of-sight against world cover. Multiple colliders do not multiply damage; walls block damage to enemies behind them.
- L5 launches two bombs in the same activation/cooldown, both initially locked to the same priority target. Each bomb has its own flight, explosion and target lifetime tracking; both can damage one enemy.
- Before L7, if the target dies or becomes invalid, continue to its last known position and detonate there.
- From L7, when the target becomes invalid, attempt a new acquisition using the same rank priority and nearest-visible rule, within 20 m of the bomb's current position. A surviving target is not replaced just because a higher rank appears. If no replacement exists, commit to the last known position; do not scan indefinitely. Replacement targets can themselves trigger a later replacement if they die.
- Initial acquisition is player-relative; reacquisition is bomb-relative. Do not retarget across registries/maps or mistake a reused enemy instance for the original target.
- Provisional flight lifetime is 5 s from launch, not reset by retargeting. On timeout detonate at the current position; no teleport to the target.
- Snapshot level configuration and AbilityDamage-scaled damage at launch. Active bombs are unchanged by later level upgrades. Existing cooldown progress is preserved.
- Pause freezes flight, lifetime, cooldown and explosion feedback. Player death/map change clears owned bombs without explosions. Ignore source, pickups, trigger-only and presentation colliders.
- Spawn inside solid cover must not permit escape through walls. Safely cancel an invalid launch rather than producing damage through cover.
- Readable temporary bomb and expanding explosion visuals; final art/audio, evolution and builds are out of scope.

## Provisional balance

The previously discussed 20 m acquisition range, 10 m/s speed, 6/4 s cooldown and 2/3/4 m explosion radius are initial values. Damage 50/70/100, 0.2 m projectile radius and 5 s flight lifetime are additional implementation defaults, not final balance.

| Level | Explosion damage | Radius m | Cooldown s | Bombs | Retarget |
| --- | --- | --- | --- | --- | --- |
| 1 | 50 | 2 | 6 | 1 | No |
| 2 | 70 | 2 | 6 | 1 | No |
| 3 | 70 | 3 | 6 | 1 | No |
| 4 | 70 | 3 | 4 | 1 | No |
| 5 | 70 | 3 | 4 | 2 | No |
| 6 | 100 | 3 | 4 | 2 | No |
| 7 | 100 | 3 | 4 | 2 | Yes |
| 8 | 100 | 4 | 4 | 2 | Yes |

## Implementation and verification plan

1. Validated level data, priority selector and runtime factory.
2. Swept homing flight with obstacle handling, last-known-position fallback, target reuse checks and L7 reacquisition.
3. Once-per-enemy, cover-aware explosion; independent two-bomb behavior; temporary visuals and lifecycle cleanup.
4. Authored asset, saved content arena/F1 and ability/mixed reward pools; preserve slot capacity and starting equipment.
5. Author tests for ranks/distance/visibility, empty acquisition, tracking, target death/reuse, fallback/retarget, wall ordering, explosion deduplication/cover, timeout, two bombs, level snapshots, pause/death/map cleanup.

User runs EditMode/PlayMode and gameplay acceptance; assistant only executes specifically requested missing/failing tests. No automatic commits or merges. Separate docs and implementation check-ins. Stop for acceptance before assessing completion of the abilities-content stage.
