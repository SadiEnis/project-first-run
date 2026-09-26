# Acid Bottle — content implementation

Fourth ability in the agreed content order, on abilities-content. GDD 12.6 establishes random-target bottles, persistent acid and L8 slowing. User gameplay acceptance is complete with L8 slow adjusted to 50%. Values remain provisional for balancing.

## Agreed behavior

- Every 4 seconds, if eligible living current-map enemies are visible within 15 m, launch a bottle toward a randomly chosen enemy's ground position captured at launch. No homing; moving enemies can leave the landing point. No target means no cooldown.
- A short visible arc ends on world-geometry collision, ignoring actors in flight. Create acid only on a valid walkable ground surface verified against the NavMesh; no floating puddles on walls or ceilings. No bouncing or separate direct-hit/explosion damage.
- Puddles remain at their landing position. Only enemies inside the radius on the same floor with an unobstructed path from the puddle receive damage. First tick after .5 s, subsequent ticks every .5 s; leaving stops future ticks, not a carried burn.
- L5 launches two bottles in one activation/cooldown, choosing different targets when possible. Same-owner Acid Bottle puddles do not multiply tick damage or slow in overlapping space. Refresh/overlap must not repeatedly postpone damage.
- L8 reduces ground movement speed by 50% only while inside acid. Overlapping acid does not stack slow. Attack cooldowns are unchanged; movement is restored on exit, expiry, death or reuse. Charger movement uses the same modifier.
- Evaluate AbilityDamage for tick damage at launch. Pause freezes flight, lifetime and ticks. Player death/map unload cleans up owned bottles, areas and slow contributions. Rebind current-map registry. No evolution gameplay or final art.

## Initial values

| Level | Damage per .5 s tick | Duration s | Radius m | Bottles | Slow |
| --- | --- | --- | --- | --- | --- |
| 1 | 5 | 3 | 2 | 1 | None |
| 2 | 7 | 3 | 2 | 1 | None |
| 3 | 7 | 4 | 2 | 1 | None |
| 4 | 7 | 4 | 3 | 1 | None |
| 5 | 7 | 4 | 3 | 2 | None |
| 6 | 10 | 4 | 3 | 2 | None |
| 7 | 10 | 5 | 3 | 2 | None |
| 8 | 10 | 5 | 3 | 2 | 50% |

## Workflow

Runtime, eight-level asset, saved arena/F1 catalog and ability/mixed reward pools are integrated. EnemyMotor owns removable movement modifiers; the strongest modifier applies without multiplication, including Charger travel, without changing attack cooldowns. Primitive green bottles/puddles are temporary visuals.

Three EditMode checks cover authored progression, movement modifier ownership and invalid configuration. Existing catalog/pool expectations were updated; the ability-chest test no longer assumes a particular ability is always offered from a larger random pool. These changes have not been compiled or run in Unity by the assistant. User runs EditMode/PlayMode and commits/check-ins; the assistant only handles specifically reported missing/failing tests.
