# Shuriken — content design

## Status and scope

Sixth ability on abilities-content, following GDD 12.5. The user accepted the proposed orbit/contact/bleed mechanics. This checkpoint is documentation only; implementation and validation are pending. No evolution gameplay or final art.

## Agreed behavior

- Automatically start one orbit set while ability control is enabled and the player is alive. No enemy is required: this is local protection rather than target acquisition.
- Each activation completes exactly two revolutions around the player's moving position, then removes its shurikens. Player aim rotation does not reset the orbital angle.
- Each shuriken can damage a given enemy once per revolution (at most twice per activation). Its second revolution resets its own hit ledger. Multiple colliders never multiply one hit.
- L6 adds a second shuriken at an opposite angular offset; each has independent hit accounting.
- Collision checks sweep orbital travel, subdividing angular steps and revolution boundaries as needed. Fast rotation, player movement and long frames must not silently skip targets. Teleports/map travel cancel the orbit rather than sweeping damage across the teleport path.
- World geometry blocks damage: require an unobstructed path from the player to the contact and from the local sweep to the target. Do not damage through walls. The visual orbit may temporarily clip geometry; it is not a solid physical projectile that pushes the player or stops the orbit.
- New activation waits until both revolutions finish, then the post-orbit cooldown elapses. Do not implement this as a cooldown measured from launch, and do not allow concurrent sets for one owner.
- Snapshot level settings and AbilityDamage scaling when a set starts; a mid-orbit upgrade affects the next set. Cancellation on map change/control loss removes visuals and contact state; preserve a normal post-orbit cooldown to avoid immediate recast exploits. No orbit survives player death.
- Pause freezes angle, orbit duration, cooldown, visuals and bleed timers. Map rebinding uses only the current registry. Enemy death/reuse clears hit/status state appropriately.

## Bleed at L8

Successful contact applies damage over time that continues after the enemy leaves the orbit. First tick after 0.5 s; subsequent ticks every 0.5 s for 2 s. Reapplication refreshes duration without stacking damage or postponing the next tick. Use an owner-specific bleed identity separate from fire/plasma burn; both statuses may coexist. Snapshot bleed damage at activation. Stop on target death/disable/reuse or map unload; already applied bleed can finish after the source dies.

## Provisional balance

Values not previously discussed (damage, L7 radius, bleed magnitude and contact radius) are initial implementation defaults, not final balance. Contact radius: 0.3 m; orbit is horizontal near torso height. Two revolutions at every level.

| Level | Contact damage | Seconds per revolution | Post-orbit cooldown s | Orbit radius m | Shurikens | Bleed |
| --- | --- | --- | --- | --- | --- | --- |
| 1 | 15 | 1.5 | 3 | 2.5 | 1 | None |
| 2 | 25 | 1.5 | 3 | 2.5 | 1 | None |
| 3 | 25 | 1 | 3 | 2.5 | 1 | None |
| 4 | 25 | 1 | 2 | 2.5 | 1 | None |
| 5 | 35 | 1 | 2 | 2.5 | 1 | None |
| 6 | 35 | 1 | 2 | 2.5 | 2 | None |
| 7 | 35 | 1 | 2 | 3.5 | 2 | None |
| 8 | 35 | 1 | 2 | 3.5 | 2 | 5 damage / 0.5 s for 2 s |

## Implementation and verification plan

1. Validated eight-level definition, runtime/factory and explicit orbit/post-orbit cooldown lifecycle; adapt existing ability hooks without changing cooldown semantics for other abilities.
2. Moving-center swept contact, per-shuriken/per-revolution hit ledgers, occlusion and readable temporary visuals.
3. Separate bleed identity, reusing timed-damage machinery only where refresh, pause and cleanup contracts match.
4. Authored asset, saved content arena/F1, ability/mixed pools and updated catalog expectations. Preserve starting equipment and ability slot capacity.
5. Author EditMode/PlayMode checks for progression, exactly two turns, independent hit limits, long-frame boundaries, movement/walls, post-orbit cooldown, upgrade snapshots, bleed refresh/coexistence and pause/death/reuse/map cleanup.

User runs tests and gameplay acceptance. Assistant only executes specifically requested missing/failing tests. Separate docs and implementation check-ins; stop for acceptance before Enchanted Staff.
