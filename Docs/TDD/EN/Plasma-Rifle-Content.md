# Plasma Rifle content

## Stage and scope

Design checkpoint on `feature/content/plasma-rifle`, branched from the content integration branch. Implement only after design acceptance and the docs checkpoint. This document supersedes the provisional Plasma Rifle progression in Weapon-Level-Effects; GDD section 11.4 defines the upgrade order. All numeric values below are proposed playtest defaults, not final balance.

Keep `weapon.plasma-rifle`, the existing asset, starting-weapon references, reward membership and eight-level ownership. Do not add a second Plasma Rifle or change the saved arena layout. No evolution gameplay, burn areas, rocket changes, ammo drops, gold/meta, final audio/models or Windows build.

## Proposed mechanics

- Automatic physical energy projectiles, one projectile and ammunition round per accepted shot. No spin-up, spread, critical hits, explosion, homing, gravity or knockback. Reuse the existing reload, weapon control and recovering recoil pipeline.
- Proposed projectile: 60 m/s, .08 m collision radius, 100 m maximum travel, 2 s lifetime. Clamp each step to remaining travel/time. Expiration deals no damage.
- Aim from the muzzle toward the camera aim point; camera-to-muzzle obstruction must prevent spawning beyond cover. Swept movement includes initial overlaps and processes solid hits in distance order, including multiple enemies within one frame. Ignore the source hierarchy and trigger volumes, not walls or closed barriers.
- Piercing counts additional distinct damage receivers: zero means one target total; one means two; two means three. A projectile damages each receiver at most once, regardless of collider count. Full direct damage at each target, without falloff. Damageable contact consumes its target budget even when damage is rejected; only accepted, non-lethal damage can apply burn. World geometry always stops it. Previously hit receivers must not trap the projectile on subsequent frames.
- Snapshot the level profile and stat-adjusted direct/burn damage at launch. Switching or leveling cannot alter an in-flight projectile. Use scaled time; source death/destruction and map unload cancel pending projectile/burn damage. Weapon switching alone does not cancel them.
- Proposed recoil for all levels: .3 degree kick, .10 s recovery delay, .22 s return, 6 degree cap. One kick per shot, none per pierced target or burn tick.

## Proposed progression

Reserve 48 and reload 1.5 s at every level; keep the starting magazine of 12. Capacity increases do not grant free ammunition.

| Level | Direct damage | Shots/s | Magazine | Additional pierced targets | Burn |
| --- | --- | --- | --- | --- | --- |
| 1 | 25 | 5 | 12 | 0 | No |
| 2 | 30 | 5 | 12 | 0 | No |
| 3 | 30 | 6 | 12 | 0 | No |
| 4 | 30 | 6 | 12 | 1 | No |
| 5 | 35 | 6 | 12 | 1 | No |
| 6 | 35 | 6 | 20 | 1 | No |
| 7 | 35 | 6 | 20 | 2 | No |
| 8 | 35 | 6 | 20 | 2 | Yes |

## Proposed burn contract

- L8 applies a target-local burn, not a ground area: 5 base damage per .5 s tick for 3 s (six ticks / 30 base damage without refresh). First tick occurs after .5 s, not immediately. Apply the launch-time WeaponDamage modifier once, independently of direct damage.
- One Plasma burn per source/receiver, no stacking. Accepted reapplication refreshes the 3 s remaining duration and replaces tick damage with that new shot's snapshot, while preserving the pending tick timer. Continuous fire must neither prevent ticks nor create extra immediate ticks. Expiration's final scheduled tick is included.
- Damage uses the existing health/death/reward pipeline. Lethal hits, dead/destroyed targets, source death and map cleanup leave no lingering damage or duplicate rewards. Clear burn state on target disable/reuse so pooled targets cannot inherit it. Pause freezes both duration and tick timers. No off-target spread or status synergy framework in this increment.
- Delta-time handling must preserve tick totals at ordinary frame rates and bounded lifetime catch-up, without an unbounded backlog. Add a small temporary target feedback effect; do not allocate a new permanent material each tick.

## Integration and verification plan

- Add a validated Plasma delivery profile while retaining existing Hitscan/Rocket serialized enum values. Validate the complete progression and saved prefab before acquisition/firing changes ammo.
- Generalize or extend projectile-launch notification coherently: the current event is Rocket-typed. Update every consumer and Rocket regression test as needed; neither projectile weapon may display a fake hitscan trace. Distinguish launch, direct impact and periodic burn feedback without extra fired/ammo/recoil events.
- Use saved projectile/feedback assets and incrementally update the existing weapon; preserve arena geometry, starting-weapon selection and reward order. The catalog count does not grow. F1 must expose relevant pierce/burn data.
- Audit tests that use the production Plasma asset as a hitscan fixture. Update production-content expectations to this GDD progression; make explicit synthetic hitscan fixtures for tests of hitscan behavior rather than weakening their assertions.
- EditMode: all levels, invalid profiles/prefabs, snapshot independence, legacy modes, unchanged identity/catalog membership, burn timer/refresh boundaries and ammo preservation.
- PlayMode: real automatic input and recoil/reload, camera/muzzle obstruction, thin walls, initial overlap, same-frame multi-target piercing, receiver deduplication, rejected/lethal hits, burn refresh/pause/kill/reuse cleanup, launch-time stat snapshots, switching, player death, map unload and reward integration. Run full suites, then stop for user playtest.

## Implementation checkpoint

- The accepted starting values above are authored into the existing `WD_PlasmaRifle` asset without changing its GUID or stable ID. Hitscan/Rocket retain enum values 0/1; Plasma is 2. The arena catalog, reward pools, scene layout and user's starting-weapon selection are untouched.
- `PlasmaProjectile` resolves ordered swept contacts with a per-projectile receiver set. `PlasmaBurnState` owns tick/refresh timing; `PlasmaBurn` attaches target-local source-specific damage and clears on target disable/death, source death or destruction. Burn feedback is a temporary orange marker above the target, not final flame VFX or a ground area.
- `PlasmaLaunched` was added alongside the existing Rocket-typed `ProjectileLaunched`. `ShotFired` remains hitscan-only; both projectile paths report accepted delayed damage through `DamageApplied`. Trace and debug consumers handle both launch types. Rocket event compatibility is preserved.
- Saved Plasma projectile/burn visual prefabs share two materials. The Editor menu **Apply Plasma Rifle Content Defaults** explicitly reapplies the provisional balance table to the existing weapon; do not run it to preserve custom tuning. No runtime scene construction is added.
- Existing content tests now expect GDD level values. Minigun acquisition/switch tests arrange a two-weapon loadout via normal acquisition even when Minigun is the scene's starting weapon; they do not rewrite the saved scene.
- User gameplay acceptance completed on 2026-09-25: the user confirmed physical projectiles, piercing, burn effects and levels work. This is mechanical acceptance, not final balance or final visual approval. The automated checkpoint below predates that acceptance.
- Automated validation on 2026-09-25: **864/864 EditMode and 516/516 PlayMode passed** in isolated Unity 6000.3.9f1. Reports: `.codex-temp/xp-attraction/plasma-final-EditMode.xml` and `plasma-complete-PlayMode.xml`. Added 21 EditMode and 16 PlayMode cases. The full suite also covers Rocket, Shotgun, Minigun and existing progression; legacy Minigun release input was aligned to a fresh frame after a timed yield to eliminate a test timing failure. All 36 changed/new Assets files match the verified copy. No Windows build or manual visual acceptance was performed.
