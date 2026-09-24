# Minigun content

## Status and workflow

Design checkpoint — 25 September 2026. Git branch verified: `feature/content/minigun`; the user reports creating and switching both branches. Agreed Plastic branch: `/main/dev/content/minigun`; server state not independently checked. This document is an implementation proposal, not a record of implemented or tested mechanics.

Follow GDD 11.2. Commit this bilingual design checkpoint before implementation. Merge the accepted weapon into content; reserve the content → main pull request for the content package. Stop for user gameplay acceptance after this weapon, before another weapon/ability.

## Proposed firing contract

- Stable ID `weapon.minigun`, automatic hitscan, eight levels, not granted at run start. One bullet, one ammo cost and one shot notification per successful shot. No Shotgun push, penetration, explosive damage or heat system.
- Initial tuning: 40 m range, fixed 1-degree cone half-angle, 240 reserve rounds. Reuse the volley resolver with one pellet and its self/trigger filtering, muzzle obstruction and range checks.
- Holding fire prepares the barrel for 0.35 scaled seconds, then fires at the configured rate. This first prototype uses a preparation delay, not a continuously accelerating rate curve. Preparation consumes no ammunition and causes no recoil.
- Releasing fire, starting reload, switching weapons, losing weapon control or dying clears preparation. Empty weapons do not prepare; reloading requires preparation again. Paused time cannot advance preparation or produce shots; existing panel/reward control restrictions remain authoritative. No deferred burst when control resumes.
- Preserve ammunition and remaining reload/cooldown across switching and level-up. Do not grant ammunition when capacity increases. Preparation is transient and deliberately resets on switching, unlike ammo. Level-up does not restart in-progress preparation; the duration is unchanged across levels.
- Do not silently change all weapons' firing cadence. Measure sustained cadence at 30/60/120 simulated FPS; the Minigun scheduler must preserve elapsed-time remainder, with no unbounded catch-up burst after a stall. Choose and document a bounded catch-up policy during implementation and test its maximum shot count. Ordinary-frame count deviation should be at most one shot over a fixed interval after preparation.

## Critical hits and recoil

- Each successful shot rolls critical chance once, using an injectable random source independent of Unity global random. Zero chance never crits; full chance always crits. A critical multiplies stat-adjusted bullet damage by 2; no extra damage application or second ammo cost. A miss still consumes the shot. Surface the critical result for test/debug feedback.
- Keep this a weapon-local critical profile; do not add unrelated ability criticals, global critical upgrades or new stat categories in this increment. Existing weapons default to zero chance and retain their behavior.
- Mechanical recoil: after resolving a successful bullet, apply an upward pitch kick through the existing PlayerLook pitch owner. Do not add a second script that overwrites camera transforms. Player mouse/gamepad input can compensate; clamp at the existing pitch limits. No automatic aim return in the first prototype, no random yaw, camera shake or weapon animation.
- Fixed spread is not recoil and does not increase during firing. Level eight reduces the actual pitch kick. Recoil affects the next shot, not the bullet just resolved; blocked shots and preparation produce none. Existing weapons default to zero recoil. Camera reset on a new run follows the existing player lifecycle.

## Proposed eight-level tuning

Numbers are initial playtest values, not values specified by the GDD or final balance. Preserve its upgrade order.

| Level | Damage | Crit chance | Magazine | Shots/s | Reload s | Pitch kick/shot |
| --- | --- | --- | --- | --- | --- | --- |
| 1 | 8 | 5% | 80 | 12 | 3.0 | 0.35° |
| 2 | 10 | 5% | 80 | 12 | 3.0 | 0.35° |
| 3 | 10 | 10% | 80 | 12 | 3.0 | 0.35° |
| 4 | 10 | 10% | 100 | 12 | 3.0 | 0.35° |
| 5 | 10 | 10% | 100 | 15 | 3.0 | 0.35° |
| 6 | 12 | 10% | 100 | 15 | 3.0 | 0.35° |
| 7 | 12 | 10% | 100 | 15 | 2.4 | 0.35° |
| 8 | 12 | 10% | 100 | 15 | 2.4 | 0.20° |

Level eight adds no evolution asset, recipe, offer or transformation. GDD evolution eligibility remains a future integration, not a promised gameplay effect here.

## Integration and acceptance

- Snapshot and validate all new level/profile data before acquisition: finite numbers, chance in [0,1], critical multiplier at least 1, non-negative preparation/recoil. Legacy serialized assets must retain valid defaults. Invalid profiles must fail before consuming ammo or replacing an existing loadout entry.
- Reuse WeaponRuntimeState, PlayerWeaponRuntimeEntry, stat evaluation and existing acquisition/level/reward rules; avoid a parallel loadout. Introduce only the small testable state/profile needed for preparation, criticals and cadence.
- Add the asset once to weapon/mixed pools and the saved ContentArena catalog. Update the Editor builder and provide incremental scene wiring that preserves authored geometry and lighting. Two weapon slots remain; use a fresh run with PlasmaRifle and acquire Minigun before another weapon.
- Reuse bounded traces; expose preparation state, damage, critical chance and rate in the test panel/HUD. Do not construct the arena at Play time. No custom models/audio/animation, gold/meta, Windows build or new evolution work.
- EditMode: eight levels, validation/legacy defaults, deterministic critical boundaries, preparation transitions, cadence at different frame steps/stalls, snapshot isolation and level-up preservation.
- PlayMode: saved scene and actual held input; preparation/release/reload/switch/pause/death restrictions; damage/crit/recoil direction and limits; mouse/gamepad compensation; ammo/events, real chest claims/full slots/max levels; Shotgun and PlasmaRifle regressions.
- User acceptance: sustained fire and short taps, aim compensation, reload/ammunition pressure, eight-level differences and chest/panel acquisition. Stop for this evaluation after automated tests.

No implementation or test execution in this design-only checkpoint. Previous Shotgun results (774 EditMode / 475 PlayMode) do not validate Minigun.
