# Shotgun content

Current recoil behavior: [Recovering Weapon Recoil](Weapon-Recoil-Recovery.md). The user found the permanent 2.5° kick too weak; the new increment uses a temporary 6° kick at every level, delayed recovery and an accumulation cap. The 2.5° fix below is a historical checkpoint, not the current tuning.

## Recoil fix on content

- After the Minigun merge, the user requested mechanical aiming recoil for Shotgun. This increment opens the previously deferred camera recoil scope for Shotgun only; models, animation, audio and other weapons remain unchanged.
- Initial tuning is **2.5 degrees upward pitch per shot**, at all eight levels; this is not final balance. Reuse `WeaponFireProfile` and `PlayerLook`, without a separate camera system.
- Apply recoil once per successful trigger after resolving all pellets. Level eight's ten pellets do not multiply camera recoil. Missing still kicks; shots blocked by cooldown, reload, empty magazine, pause or death do not.
- Mouse/gamepad input can compensate and existing pitch limits apply. No automatic return, random horizontal kick or projectile ricochet. Damage, target push, spread, ammunition and level order stay unchanged.
- Verify all eight asset levels, single kick per trigger, no additional kick while holding or blocked, and independence from level-eight pellet count. User evaluation of the punchy feel follows automated verification.
- Implemented: `_recoilDegrees: 2.5` at all eight levels; no runtime code or scene changes. Isolated Unity verification: **805/805 EditMode, 484/484 PlayMode passed**. Added one PlayMode test and extended existing asset/input tests with recoil assertions. Reports: `.codex-temp/xp-attraction/shotgun-recoil-edit.xml` and `shotgun-recoil-play.xml`. All four changed asset/test files hash-match the tested copy; `git diff --check` is clean. User acceptance of the recoil feel remains pending; no commit/check-in was performed.

## Status and workflow

Implementation checkpoint — 24 September 2026; user gameplay acceptance — 25 September 2026. Git branch verified as `feature/content/shotgun`. The agreed Plastic branch is `/main/dev/content/shotgun`; its server state was not rechecked in this step. Runtime implementation, the Shotgun asset and arena connections are complete, and the user accepted the gameplay increment.

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

## Implementation notes

- `WeaponShotConfig` validates pellet count (1–64), half-angle (inclusive 0, exclusive 90) and push distance. `WeaponLevelData` and runtime entries snapshot every level's shot data; range/mask are also copied on acquisition. A parameterless constructor allows Unity to apply field defaults for legacy inline level records; an invalid zero pellet count is not accepted.
- `HitscanVolleyResolver` uses its own random source, completes physics queries and applies aggregate damage per target. `ShotFired` now publishes a complete `HitscanVolleyResult`; `DamageApplied` is emitted per successfully damaged target. The old `HitscanShotResolver` remains a single-ray API; the player uses the new resolver.
- `EnemyMotor` implements optional `IKnockbackReceiver`. Navigation boundaries and physics sweeps limit displacement without calling motor Stop/Resume or resetting attacks.
- `WD_Shotgun.asset` is added once to weapon and mixed pools. The arena contains five items. F1 → Shotgun Acquire → close panel → Q to equip → left mouse to fire / R to reload. PlasmaRifle occupies one of two weapon slots; acquiring Development Secondary first leaves no room for Shotgun in that run. Restart Play for a fresh test.
- `VolleyTracePresenter` holds at most 64 reusable LineRenderers; this Shotgun uses eight/ten traces, hidden after 0.12 game seconds. The HUD shows current per-pellet damage, pellet count and push distance. These are temporary test visuals.
- The Editor command `Update Content Arena Shotgun Connections` updates only saved catalog/trace wiring without rebuilding geometry or lighting. Full `Build Content Test Arena` also includes the new content but is not intended to preserve manual scene edits.

No tests were run for the original design-only checkpoint. Previous arena results remain 753 EditMode / 457 PlayMode; Shotgun implementation verification is recorded separately.

## Verification — 24 September 2026

- Unity 6000.3.9f1, isolated project copy: **EditMode 774/774**, **PlayMode 475/475** passed. Added 21 EditMode and 18 PlayMode tests; updated existing pool-size assertions for the added content.
- Verified real mouse presses/holding, reload, empty magazine, panel/death restrictions; per-target pellet totals; chest acquisition/levels/maximum eligibility; stats and preserved runtime state.
- Verified walls/player/NavMesh edges, stopped/prepared/dead enemies and an already committed Charger attack. Motor Stop establishes the stopped state after clearing its path; pushing preserves the previous stopped state.
- Test input isolation begins before asynchronous scene loading and ends after scene unloading. The second-press test explicitly checks release state and completed cooldown.
- Changed/new assets and code match the test copy; `git diff --check` is clean. Existing scene lighting data was preserved. No Windows build, commit/check-in or merge was performed.
- User gameplay evaluation was still pending at this automated verification checkpoint; numerical balance remains subject to feedback.

## Gameplay acceptance — 25 September 2026

- The user tested Shotgun and reported that it works well. This records overall gameplay acceptance, not separate confirmation of every manual test case above.
- No additional mandatory Shotgun mechanics remain in this increment. Custom models, animation, audio, camera recoil and final balancing remain future presentation/balance work; evolution remains out of scope.
- Next: record the implementation check-in/commit, merge Shotgun into **content**, then begin Minigun design from updated content. These version-control operations have not been performed by this documentation update.
