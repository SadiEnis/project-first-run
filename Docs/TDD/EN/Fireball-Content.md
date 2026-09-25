# Fireball content — design draft

First ability in [Abilities Content Plan](Abilities-Content-Plan.md). Keep the existing `ability.fireball` asset/identity and reward membership. Current code fires one straight projectile at the nearest registered enemy, uses trigger collision, and has provisional damage/cooldown/speed levels. It has no burn or multi-projectile sequence. This increment replaces that provisional content with GDD 12.2 behavior; it is not a new ability or evolution.

## Proposed mechanics (confirm before implementation)

- Automatically activate when an eligible target exists within 20 m of the cast origin. Only alive, active, prepared enemies from the current map registry qualify. Exclude world-occluded targets using an explicit world mask; actors do not count as cover. No target means no projectile and no cooldown consumption.
- Fire the whole volley simultaneously. Choose targets randomly without replacement until all eligible enemies have been used; repeat shuffled cycles if fewer enemies than projectiles. A single enemy can receive the entire volley. Random selection must be injectable for tests and must not consume Unity global random state. Revalidate candidates before spawning and preserve registry rebinding across maps.
- Projectiles aim at each selected enemy's collider center at launch, then travel straight. No homing, retargeting, explosion, piercing or knockback. Each projectile can damage one receiver once; another enemy or world wall in its path can intercept it. Multiple volley projectiles may each damage the same enemy.
- Use swept collision including initial overlap, source exclusion, nearest obstruction and source-to-spawn obstruction. Do not rely solely on trigger callbacks. Proposed radius .12 m, maximum travel 60 m, lifetime 5 s; clamp movement to remaining range/time and expire without damage. Ignore unrelated triggers. Saved visuals must not obstruct sibling projectiles.
- Snapshot level and stat-adjusted direct/burn damage at launch; apply AbilityDamage once to each base value. Switching weapons or leveling does not alter in-flight casts. Pause freezes travel and burn. Player death/destruction and owning-map unload disarm effects; no post-death rewards. Keep effects owned by the map/target, not a persistent player hierarchy.
- One cooldown per successful volley, not per projectile. Validate the whole cast before spawning; roll back partially created projectiles on construction failure. Preserve the existing acquisition/level runtime and remaining cooldown on level-up.

## Proposed levels

Numeric values are playtest defaults. Projectile counts and upgrade order follow the GDD.

| Level | Direct damage per projectile | Cooldown s | Projectiles | Speed m/s | Burn duration s |
| --- | --- | --- | --- | --- | --- |
| 1 | 25 | 2 | 2 | 12 | 1.5 |
| 2 | 30 | 2 | 2 | 12 | 1.5 |
| 3 | 30 | 1.5 | 2 | 12 | 1.5 |
| 4 | 30 | 1.5 | 3 | 12 | 1.5 |
| 5 | 35 | 1.5 | 3 | 12 | 1.5 |
| 6 | 35 | 1.5 | 3 | 16 | 1.5 |
| 7 | 35 | 1.5 | 4 | 16 | 1.5 |
| 8 | 35 | 1.5 | 4 | 16 | 3 |

## Burn proposal and explicit assumptions

GDD increases burn duration at L8 but does not specify when burn begins. Proposal: burn starts at L1; L8 doubles its duration. Accepted non-lethal direct hits apply 5 base damage per .5 s tick, first tick after .5 s: three ticks at L1–7, six at L8 without refresh.

Same source/receiver/Fireball effect does not stack; reapplication refreshes duration, replaces damage with the new launch snapshot and preserves tick phase. Multiple balls do not multiply burn stacks or reset the pending tick timer. Proposal: Plasma and Fireball burns coexist as two separately keyed effects, each using its own weapon/ability stat snapshot. Target death, disable/reuse and source/map cleanup clear appropriate effects.

Extract only the shared timed-damage lifecycle required by these two consumers; preserve Plasma behavior and run its regressions. Do not directly use the Plasma-specific WeaponDamage path for abilities. No ground fire or burn spread. Temporary readable feedback is enough; final VFX/audio are deferred.

## Implementation checkpoints and verification

1. Design agreement and EN/TR docs checkpoint, including the ability order and corrected Plasma acceptance.
2. Random ranged targeting, atomic multi-projectile casting and swept projectile lifecycle, with tests; take a coherent checkpoint if independently verified.
3. Shared timed-damage support, Fireball burn, eight authored levels and saved arena/F1/reward integration. No scene reconstruction, catalog duplicate or unrelated retuning.
4. Full EditMode/PlayMode validation, results documentation, then user gameplay test before Force Wave.

Cover zero/one/many targets, random cycles and range/cover boundaries, registry rebinding, 2/3/4 projectiles with one cooldown, stat snapshots, wall/initial/high-speed collisions, no double resolution, cast rollback, burn refresh/coexistence/expiry/lethal rewards, pause/death/reuse/map cleanup, max/full-slot reward eligibility and existing weapon/ability regressions. Values and target behavior remain a proposal until discussed. No code or test execution in this design checkpoint.
