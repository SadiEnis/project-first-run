# Rocket Launcher content

## Status and workflow

Design checkpoint. Git branch verified as feature/content/rocket-launcher, from the content integration workflow; Plastic branch /main/dev/content/rocket-launcher is user-reported, not independently verified. Follow GDD 11.3. This is a proposal, not implemented gameplay. Record the bilingual docs checkpoint before code; stop for user playtest after the weapon is implemented and verified. No evolution content or ammo sources.

## Proposed mechanics

- Stable ID weapon.rocket-launcher, Weapon category, eight levels. Semi-automatic: one press launches one rocket and costs one magazine round/cooldown. No spin-up, critical chance, homing, gravity, ricochet or penetration initially.
- A visible projectile travels from the muzzle toward the camera aim point. Preserve camera-to-aim and muzzle obstruction checks. A nearby wall cannot be bypassed by spawning on its far side.
- Proposed defaults: 3-round magazine, 12 reserve rounds, .8 shots/s, 3 s reload, 25 m/s rocket, .15 m collision radius, maximum travel 60 m and lifetime 4 s. Reaching range/lifetime without impact despawns without damage. Clamp each movement step to the remaining travel/time.
- Sweep the projectile volume over its movement segment and resolve the nearest solid obstruction, including high-speed and initial-overlap cases. Ignore the firing hierarchy and unrelated trigger volumes, but not walls/closed barriers. Do not rely solely on OnTriggerEnter.
- First impact produces exactly one area explosion; no additional direct-hit damage. Initial proposal: 80 damage to each exposed damage receiver within a 3 m radius, with no distance falloff. Use closest collider surface for distance and deduplicate receivers across multiple colliders.
- World cover blocks blast exposure. Establish exposure for all candidates before damage can remove colliders. Other damageable actors do not act as world cover. Handle the impact surface with a small, validated free-space origin; do not offset the blast or fragments through thin walls. A direct-hit target must not incorrectly occlude its own explosion damage.
- Proposed self-damage rule: ignore the firing actor in rocket, blast and fragments. No enemy push, status, stun or rocket jump in this increment. This is an explicit provisional gameplay choice, not a GDD requirement.
- Snapshot launch-time stat-adjusted damage and the complete level profile. Switching/leveling afterwards does not modify rockets already in flight. No extra ammo/recoil per impact or fragment.
- Reuse automatic empty-magazine reload and recovering recoil. Initial recoil at every level: 5° kick, .10 s delay, .35 s return, 10° cap. Manual R and existing pause/death/control rules remain.
- Travel and effects use scaled game time. Cancel/disarm projectiles when the firing player dies or the owning map unloads; no post-death reward generation. Keep projectile ownership in the firing map, not a persistent player hierarchy.

## Eight-level proposal

Values are playtest starting points; only the upgrade sequence is prescribed by the GDD.

| Level | Blast damage | Speed m/s | Magazine | Reload s | Blast radius m | Fragments |
| --- | --- | --- | --- | --- | --- | --- |
| 1 | 80 | 25 | 3 | 3 | 3 | 0 |
| 2 | 100 | 25 | 3 | 3 | 3 | 0 |
| 3 | 100 | 35 | 3 | 3 | 3 | 0 |
| 4 | 100 | 35 | 4 | 3 | 3 | 0 |
| 5 | 100 | 35 | 4 | 2.4 | 3 | 0 |
| 6 | 120 | 35 | 4 | 2.4 | 3 | 0 |
| 7 | 120 | 35 | 4 | 2.4 | 4 | 0 |
| 8 | 120 | 35 | 4 | 2.4 | 4 | 8 |

## Level-eight fragmentation

- One impact explosion emits eight visible physical fragments, not a second explosion and not an evolution.
- Initial pattern: evenly spaced horizontal radial directions from the validated explosion origin. No target seeking. Pieces directed into nearby geometry are blocked rather than spawned beyond it.
- Proposed fragment values: 20 base damage, 20 m/s speed, .05 m collision radius, 8 m travel / .4 s lifetime. Apply the launch-time WeaponDamage multiplier once to fragment base damage, independently of blast base damage.
- Fragments collide via swept movement, do not penetrate, explode or create children. Each fragment resolves once. Across one rocket's fragment batch, at most one fragment may damage the same receiver; the initial blast may also damage that receiver. Keep this shared hit record local to the batch, with bounded lifetime.
- Fragment impacts do not consume magazine rounds or emit another weapon-fired/recoil notification. Expose delayed damage results for debug feedback separately.

## Integration boundaries

- The current weapon controller routes shots to a hitscan volley resolver; Fireball is a single-target trigger projectile and EnemyRangedProjectile has swept single-target movement. Reuse applicable collision/lifecycle patterns without changing these existing gameplay contracts.
- Extend weapon configuration with a validated delivery mode and immutable rocket/fragment level data. Hitscan remains the default for existing assets. Validate required projectile prefab/components and all levels before acquisition/firing mutates ammo.
- Distinguish launch notification from delayed impact/damage. Update all shot-event consumers/tests coherently; do not fake an instantaneous hitscan trace for a moving rocket. Reuse one ammunition, acquisition, stat and loadout pipeline.
- Add saved rocket/fragment prefabs with simple temporary visuals and a bounded-duration blast effect. Explicit limits on lifetime and eight fragments per explosion; no permanent trail/material/object accumulation. Pooling is not a prerequisite, but measure before adding it.
- Add the asset once to weapon/mixed reward pools, saved ContentArena catalog and Editor builder with incremental wiring that preserves authored geometry and lighting. The two-slot limit stays unchanged; test from a fresh run with PlasmaRifle plus Rocket Launcher.
- Other weapons, Fireball, scene geometry, gold/meta, new ammo drops, final audio/model polish and Windows builds stay out of scope. PlasmaRifle's planned projectile/piercing/burn expansion is a later item, not part of this branch.

## Verification and acceptance

EditMode: eight levels, finite positive bounds, mode/prefab/profile consistency, immutable snapshots, no free ammo on level-up, legacy defaults and unique pool/catalog membership.

PlayMode: real input, one launch/ammo/recoil per trigger, wall/muzzle/initial-overlap/high-speed collisions, single detonation, radius/cover/self filtering, multiple-collider deduplication, no direct-hit double damage, bounded fragmentation and batch deduplication, expiration, pause/death/map cleanup, switching/leveling in-flight snapshots, automatic/manual reload, reward acquisition/max/full-slot rules and existing weapon regressions.

User playtest checklist: close/mid/far launch, grouped enemies and cover, level-three speed, level-four capacity, level-seven radius, level-eight fragments, recoil and automatic reload. The user reported successful gameplay and accepted this increment; individual checklist cases were not separately reported. Numerical balance remains provisional.

Deferred evolution idea: fragments could explode on impact and leave temporary burning areas. This is an idea for later design, not an implemented or approved evolution mechanic.

## Implementation checkpoint — 2026-09-25

- `WeaponDeliveryMode` defaults to Hitscan; Rocket profiles are validated and snapshotted with the existing runtime entry. Fireball and other weapon assets are unchanged.
- `RocketProjectile` sweeps its sphere and clamps travel to remaining range/lifetime. The swept center is the blast origin; cover exposure is collected before applying damage. Fragments share one hit record per explosion.
- `PlayerWeaponController.ProjectileLaunched` reports a successful rocket launch. `ShotFired` retains the hitscan volley contract; `DamageApplied` reports delayed accepted blast/fragment hits. Trace/debug consumers handle the separate launch event, with no fake hitscan line.
- Saved assets: `WD_RocketLauncher`, `RocketProjectile`, `RocketFragment`, `RocketBlast`, and three shared materials. The Editor-only builder creates missing content and incrementally updates pools/catalog; Play mode does not generate the scene.
- The saved arena has seven catalog items. Start a fresh run, press F1, acquire Rocket Launcher, close the panel and switch with Q. Level it through F1; L8 adds eight radial fragments. Weapon/mixed chests also use the existing acquisition/level-up claim flow.
- No pooling/performance claim or Windows build validation is included. Visuals are temporary; final sound/model work and ammo drops remain deferred.
- Automated verification: **843/843 EditMode, 500/500 PlayMode passed** in the isolated Unity 6000.3.9f1 project. Reports: `.codex-temp/xp-attraction/rocket-final-EditMode.xml` and `rocket-verified-PlayMode.xml`. The 19 new EditMode and 11 new PlayMode cases cover authored profiles, legacy modes, snapshots, real-input launch/auto-reload, swept/initial/muzzle collisions, cover, deduplication, fragmentation, expiration, pause/death and map cleanup. The existing arena showcase teardown now stops queued drop producers before asynchronous unload; its first full-suite run exposed that test-lifecycle race. Changed/new source assets match the tested copy.
