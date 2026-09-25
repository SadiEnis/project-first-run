# Minigun content

## Status and workflow

Design and implementation checkpoints — 25 September 2026. Git branch verified: `feature/content/minigun`; the user reports creating and switching both branches. Agreed Plastic branch: `/main/dev/content/minigun`; server state not independently checked. The mechanics below are implemented and automatically verified; user gameplay acceptance is complete. Numerical balance remains provisional.

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

The original design-only checkpoint did not include implementation or test execution. Minigun verification is recorded separately below.

## Implementation and verification

- `WeaponFireProfile` snapshots validated preparation, critical and recoil data for each level. Existing weapons retain zero preparation, critical chance and recoil; their firing cadence remains on the existing path.
- `PreparedAutomaticFire` advances the existing ammo/cooldown state chronologically. It allows at most three shots per tick, then discards excess stalled-frame time while retaining the final shot's cooldown; no catch-up debt is carried into the next frame. Sustained cadence was checked at 30/60/120 simulated FPS for both 12 and 15 shots/s.
- Critical results are exposed by `HitscanVolleyResult.IsCritical`. Recoil is applied through `PlayerLook` after resolving the bullet, using the existing pitch limits and input ownership. Pause clears preparation rather than banking it.
- `WD_Minigun.asset` is connected once to weapon/mixed pools and the saved arena's six-item catalog. Geometry and lighting were not rebuilt. The Editor builder includes Minigun and provides `Update Content Arena Minigun Connections` for incremental wiring.
- Unity 6000.3.9f1, isolated project copy: **805/805 EditMode and 483/483 PlayMode passed**. Added 31 EditMode and eight PlayMode tests. Reports: `.codex-temp/xp-attraction/minigun-edit.xml` and `minigun-play.xml`.
- Coverage includes real held mouse input, preparation/reset restrictions, reload/empty/panel/pause/death, critical damage with stats, pitch limits and compensation through mouse/gamepad look calculations, chest acquisition/levels/max eligibility, slot limits and preserved inactive state. Existing regression tests also passed.
- The expanded weapon pool contains four weapons but a common chest offers three. The Shotgun acquisition test now controls reward randomness so it does not incorrectly assume every weapon must appear in every offer.
- No Windows build, evolution content, implementation commit/check-in or merge was performed by the agent. User gameplay acceptance was pending at the automated verification checkpoint and is now recorded below.

## Gameplay acceptance and follow-up

- The user reports that Minigun behaves as intended and its levels work. This is mechanical acceptance, not final balance approval or separate confirmation of every manual test case.
- Damage, rate, recoil and other tuning values may be revisited when the broader combat/content balance can be evaluated.
- Agreed next step: commit/check-in this increment and merge Minigun into content, without a PR at this stage. Afterwards, update TDD and extend felt recoil to the other weapons directly on content, particularly a heavier, punchier Shotgun kick. Do not change these weapons in the Minigun closing checkpoint.
- Spread and recoil are separate: Minigun currently uses a fixed random cone and upward aim kick; projectile ricochet is not implemented. Shotgun recoil must be once per trigger, not once per pellet. Discuss exact feel/tuning before implementing the follow-up; reserve the package PR for content → main.

## User playtest

Open `Assets/_Project/Scenes/Tests/Test_ContentArena.unity` and start a fresh Play session. F1 → Minigun Acquire → close the panel → Q to equip. Do not acquire another secondary weapon first; the loadout still has two slots.

Hold left mouse: preparation reaches 0.35 seconds before firing; short taps should not fire. Try counteracting upward recoil, R reload, Q switching and F1 level increases. Compare level five's rate, level seven's reload and level eight's recoil. The HUD shows preparation, rate, critical chance and the last shot's critical result. Numbers remain provisional until playtest feedback.
