# Enchanted Staff — content design

## Status and scope

Seventh ability on abilities-content; GDD 12.7. The user accepted the Runetracer-inspired revision: random launch directions, enemy piercing and reflection from real world obstacles. No random mid-flight turns, screen-edge bounce or homing. This is a documentation checkpoint; runtime, content and tests are not yet implemented.

## Agreed mechanics

- Automatically emit short travelling energy beams in random horizontal directions; no target is required. They are not continuous lasers attached to the player.
- Pass through enemies without changing direction or consuming penetration. Each beam has independent per-enemy hit timing: at least 0.5 s between hits on the same enemy. Neither bouncing nor hitting another enemy resets that protection. Multiple colliders count as one target.
- Reflect direction from solid world surfaces using the collision normal. Actual map geometry, not camera/screen boundaries, defines bounces. Do not steer randomly between collisions.
- Beams expire after their lifetime, even if they repeatedly bounce. A new cast does not wait for older beams to expire; each keeps its own lifetime and hit records.
- L5 emits two independently directed beams. Snapshot damage, lifetime and other settings at launch; later upgrades affect future casts.
- Scale damage through AbilityDamage, not weapon stats. Pause freezes movement, lifetime, cooldown and hit timers. Player death clears owned beams; map rebinding clears old-map beams and uses the new registry.
- Wall collision ordering must prevent hitting enemies behind the first obstruction on a segment. Sweep travel to prevent tunnelling; consume remaining frame travel after reflection.
- Use a small outward separation after contact and a bounded collision-iteration budget for corners. If safe travel cannot be resolved within the budget, stop travel for that frame; lifetime still advances. Never skip obstacles to exhaust travel.
- Spawn inside solid geometry must not allow escape through walls. Cancel that beam safely. Exclude source, pickups, triggers and presentation objects from world collisions.
- Initial prototype travels horizontally on a fixed launch plane. Derive horizontal reflection from wall normals; terminate on floor/ceiling contacts that cannot produce a valid horizontal reflection. This is not terrain-following behavior; stairs/slopes require manual evaluation.
- Visible short luminous bodies/trails and bounce feedback are temporary presentation. No evolution, explosions, status effect, final art or build work.

## Provisional starting values

Previously proposed speed 8 m/s, lifetime 2/3/4 s and cooldown 5/4/3 s remain initial balance values. Contact radius 0.15 m and damage 20/30/40 are provisional implementation defaults. Cooldown is measured from emission, unlike Shuriken's post-orbit cooldown.

| Level | Damage | Lifetime s | Cooldown s | Beams |
| --- | --- | --- | --- | --- |
| 1 | 20 | 2 | 5 | 1 |
| 2 | 30 | 2 | 5 | 1 |
| 3 | 30 | 3 | 5 | 1 |
| 4 | 30 | 3 | 4 | 1 |
| 5 | 30 | 3 | 4 | 2 |
| 6 | 40 | 3 | 4 | 2 |
| 7 | 40 | 4 | 4 | 2 |
| 8 | 40 | 4 | 3 | 2 |

## Implementation and verification plan

1. Validated level definition and runtime factory; preserve cooldown progress on upgrade.
2. Swept beam movement, ordered wall/enemy contacts, reflection and per-beam/per-enemy hit protection. Track target reuse separately from an old enemy life.
3. Temporary visuals; authored asset and saved content arena/F1 plus ability/mixed reward pools. Keep existing slot limits and starting equipment.
4. Author tests for levels, reflection, piercing, collider deduplication, independent beams, repeat-hit timing across bounces, expiry, corners/initial overlaps, large frames, pause/death/map cleanup and upgrades.
5. User runs EditMode/PlayMode and gameplay acceptance. Assistant runs only specifically requested missing/failing tests. No automatic VCS writes.

Stop for user acceptance before Sniper Bomb. Docs and implementation have separate check-in checkpoints.
