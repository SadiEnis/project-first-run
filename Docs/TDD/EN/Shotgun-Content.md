# Shotgun content

## Status and workflow

Design checkpoint — 24 September 2026. Git branch verified as `feature/content/shotgun`. The agreed Plastic branch is `/main/dev/content/shotgun`; its server state was not rechecked in this step. No Shotgun runtime implementation or asset has been added yet.

Content workflow: Git `main → content → feature/content/shotgun`, Plastic `dev → content → shotgun`. Record the docs checkpoint, then a coherent implementation checkpoint. After automated verification and user gameplay acceptance, merge into **content** and start the next item from updated content. Deliver playable content packages from content to dev/main. This is the agreed content-period exception to starting every stage from dev/main.

Implement the close-range burst damage/crowd-control role from GDD 11.1 through existing acquisition, levels, ammunition and rewards. The details below are the initial implementation proposal; numerical values are playtest starting points, not final balance.

## Firing contract

- Stable ID `weapon.shotgun`, display name `Shotgun`, Weapon category, eight levels. Do not grant it as a free starting weapon.
- Semi-automatic: each physical press produces at most one shot; holding does not repeat. Existing cooldown, reload, death, panel and reward UI restrictions apply.
- A successful trigger consumes **one** shell and **one** cooldown even on a miss. Pellets are not separate shots. Sound/firing notifications must not repeat per pellet; the result model must carry all pellet traces and target results.
- Pellets are hitscan, not projectile objects. Preserve camera aiming and muzzle obstruction checks per pellet. Non-damageable walls still block; ignore triggers and the player's own colliders. No penetration or ricochet.
- Cone half-angle 6 degrees (12 degrees total), maximum range 30 m. No separate distance damage multiplier initially: fewer pellets hitting the same target reduce effectiveness at range.
- Use a circular cone distribution, not a square pattern or coincident rays. Tests must control the random source; do not reseed Unity's global random state and affect unrelated systems.
- Complete all pellet physics queries before applying damage/displacement, so an early kill or push cannot change later pellet geometry.
- Each pellet stops at its first obstruction. Aggregate pellets by damage receiver; multiple colliders must not multiply one pellet's damage. Apply one total `DamageInfo` per target per shot, with separate results for different targets.
- Evaluate WeaponDamage once on the per-pellet base damage; sum the pellets that hit. Eight level-one hits without modifiers deal 64 damage. UI must distinguish per-pellet damage from theoretical full-volley damage.

## Ammunition and eight levels

Starting reserve: 48 shells. Fire rate: 1.2 shots/s at every level. Use the existing magazine reload model: transfer the required shells from reserve in one operation. No individual shell loading.

| Level | Pellet damage | Pellets | Magazine | Reload (s) | Push distance (m) | Change |
| --- | --- | --- | --- | --- | --- | --- |
| 1 | 8 | 8 | 6 | 2.0 | 0.6 | Base weapon |
| 2 | 10 | 8 | 6 | 2.0 | 0.6 | Damage |
| 3 | 10 | 8 | 8 | 2.0 | 0.6 | Capacity |
| 4 | 10 | 8 | 8 | 2.0 | 1.0 | Knockback |
| 5 | 12 | 8 | 8 | 2.0 | 1.0 | Damage |
| 6 | 12 | 8 | 8 | 1.6 | 1.0 | Reload |
| 7 | 12 | 8 | 10 | 1.6 | 1.0 | Capacity |
| 8 | 12 | 10 | 10 | 1.6 | 1.0 | Pellet count |

Preserve the GDD progression order. Level eight only increases pellet count in this increment; add no evolution recipe, asset, reward or automatic transformation.

Preserve the existing level contract: validate and snapshot all levels before acquisition; later asset changes do not alter acquired weapons. Level-up keeps the same active/inactive runtime entry, existing ammo and remaining cooldown/reload seconds. More capacity gives no free ammunition. New pellet/push values apply on the next shot.

## Knockback boundary

- Apply through an optional combat receiver; do not couple the shot resolver directly to EnemyController. Targets without push support still receive normal damage.
- After accepted damage, push a surviving target horizontally at most once per trigger, away from the muzzle. Pellet count does not multiply distance; do not start movement after death.
- Initially use a single obstruction-limited displacement, not ragdoll, launch, timed stun or a separate status system. Do not cross NavMesh edges, walls or closed barriers. The physics sweep also treats the player as an obstruction and ignores the receiver's own colliders.
- Respect EnemyMotor navigation/attack movement ownership. The initial push does not reset attack cooldowns, remove a Ranger's emitted projectile, or cancel a Charger's attack as a stun would. Normal behavior resumes from the new position on its next update. Preserve existing collision-limited charge behavior.
- Reject displacement for dead, disabled, unprepared or off-NavMesh enemies. Do not retain a pending push across reactivation.

## Integration with existing code

- Inspected WeaponDefinition currently represents single-ray hitscan; WeaponLevelData carries damage/capacity/rate/reload. Add immutable runtime shot data for pellets, cone and push. Defaults of one pellet, zero spread and zero push must preserve existing PlasmaRifle/Development Secondary assets and behavior.
- Keep ammunition/cooldown/reload rules in WeaponRuntimeState; no parallel loadout or Shotgun acquisition path. A small spread calculator, multi-hit resolution and optional push receiver suffice; do not rewrite a general weapon framework.
- Extend current single-hit notifications to represent the whole shot, updating debug/presentation consumers and tests together. One trigger means one firing notification; actual damage notifications are per target. Preserve single-ray weapon behavior.
- Add the Shotgun asset and eight levels, updating both the ContentArena catalog and Editor scene builder. Use the saved scene; do not introduce Play-time scene construction.
- Add Shotgun once to the existing weapon reward pool and suitable mixed pool without changing chest types or weighting algorithms. New-weapon/level choices, two-slot limits and maximum levels use existing eligibility rules. Keep Development Secondary in this increment.
- Provide lightweight, short-lived pellet trace/impact feedback without unbounded per-shot GameObject accumulation. Original weapon models, animation, audio and camera recoil polish are outside this mechanical delivery.

## Tests and acceptance

1. EditMode: eight-level data; invalid/NaN/infinite values; pellet/angle bounds; deterministic output from controlled random inputs; normalized cone directions; single-pellet compatibility; runtime snapshots and maximum levels.
2. PlayMode: one shell/cooldown/notification per press; holding, empty magazine, reload and death; target pellet totals, multiple targets/colliders, wall/muzzle obstruction, range and trigger filtering.
3. Knockback: once per target, level-based distance, death/rejected damage/unsupported receivers; walls, player, closed barriers and NavMesh edges; continued Chaser/Charger/Ranger behavior.
4. Active/inactive Shotgun level-up, Q switching, ammo/cooldown/reload preservation, stat effects, isolation between players and source assets.
5. Saved ContentArena panel acquisition/all levels; real chest new-Shotgun/level claims, full-slot/maximum limits and existing weapon regression tests.
6. User tests close/mid/far targets, crowds, knockback, magazine/reload feel and all eight levels. Do not implement Minigun or other content before acceptance. Do not resume Windows build work.

No new automated tests were run for this docs-only checkpoint. Previous content arena validation was 753 EditMode / 457 PlayMode; those results do not validate Shotgun.
