# Recovering weapon recoil

## Contract on content

This increment replaces permanent aim displacement with a temporary upward recoil offset. It supersedes the no-return behavior in the original Shotgun/Minigun checkpoints. No new weapon, ricochet, model animation or sound is included.

- PlayerLook owns base aim and final camera pitch. Mouse/gamepad change base aim; recoil never writes the base aim. Final pitch is base aim minus recoil offset, within existing pitch limits.
- Resolve the bullet/whole volley first, then apply one immediate kick per successful trigger. Misses kick; blocked shots do not.
- Each new kick adds to the remaining offset, caps accumulation, restarts the delay, and then smoothly returns to zero using a finite-duration smoothstep curve. A shot during recovery starts from the current offset, never an old target angle.
- Recovery uses scaled game time. Paused time and disabled player control freeze recovery; resuming continues it without a catch-up jump. Reload and weapon switching do not snap the camera back. The last actual shot supplies recovery timing. Switching to a lower-cap weapon must not abruptly shrink existing recoil; it cannot add more until below its cap.
- Input compensation changes base aim normally. Recovery removes only recoil, so the camera settles at the player's new aim, not the world direction saved before firing. Looking toward the upper pitch limit trims hidden recoil; there is no invisible accumulated kick beyond the limit.
- Disabling/destroying the look component clears its temporary offset. New player instances start without recoil. No automatic yaw kick or aim assistance.
- Inspector values are validated and snapshotted for all levels before acquisition: kick and delay finite/non-negative; recovery duration and maximum offset finite/positive; kick cannot exceed maximum. Legacy no-recoil weapons keep valid defaults and zero kick.

## Initial tuning

| Weapon | Kick | Delay | Return duration | Accumulation cap |
| --- | --- | --- | --- | --- |
| Shotgun, all levels | 6° | 0.08 s | 0.30 s | 12° |
| Minigun, levels 1–7 | 0.35° | 0.12 s | 0.25 s | 8° |
| Minigun, level 8 | 0.20° | 0.12 s | 0.25 s | 8° |

Values are provisional. Shotgun normally recovers before its next legal shot; Minigun's interval is shorter than its recovery delay and therefore builds recoil during sustained fire. Other weapons keep zero kick for now, but can use the same data-driven mechanism.

## Verification plan

Pure model tests cover delay crossing, exact return, frame subdivision, accumulation during delay/recovery, caps, new profiles, pitch headroom, reset, zero time and invalid values. Asset tests cover all eight levels and legacy defaults/snapshots. Scene tests cover a real Shotgun volley, no pellet multiplication, Minigun held fire/release, input compensation, pause, switching and existing blocked-fire behavior.

Update EN/TR docs, implement and run automated tests, then stop for user evaluation. User performs check-in/commit; no PR or merge is needed for this direct content increment.

## Implementation and verification

- Implemented with validated `WeaponRecoilConfig`, pure `WeaponRecoilState`, per-level snapshotted data and a single PlayerLook camera owner. Recovery uses smoothstep over the last shot's duration, consuming any delay before advancing the curve. Kick is immediate; only the return is interpolated.
- Shotgun uses 6°/.08s/.30s/12° at all levels. Minigun retains .35° (.20° at level eight), with .12s/.25s/8°. No scene rebuild, new weapon, change to damage/ammo or other weapon's kick.
- Isolated Unity 6000.3.9f1 verification: **824/824 EditMode, 485/485 PlayMode passed**. Reports: `.codex-temp/xp-attraction/recoil-recovery-edit.xml` and `recoil-recovery-play.xml`. The recovery increment added 19 EditMode cases and one PlayMode case, extending existing asset/scene tests; the earlier uncommitted Shotgun kick increment also added one PlayMode case.
- Tests cover return timing, re-fire during recovery, caps, invalid profiles, snapshots, legacy defaults, real firing, mouse/gamepad look compensation, pause/switch/disable behavior and no extra kick for blocked shots or multiple pellets. Full existing suites passed as well.
- Changed/new asset and code files match the tested copy by hash; `git diff --check` is clean. No Windows build or version-control writes. User feel acceptance is pending.

## Playtest

In a fresh Test_ContentArena run, acquire Shotgun via F1 and equip with Q. Fire once: a stronger upward kick should settle after .08 + .30 seconds without moving the underlying aim. Move the mouse during recovery to confirm it settles at the new aim. Check levels one and eight for the same kick despite different pellet counts.

Restart Play to free the second weapon slot, acquire Minigun and hold fire: small kicks accumulate up to 8°, then fade after release. Release briefly and fire again while recovery remains in progress to feel the additive behavior. Evaluate strength, delay and return speed before final tuning/commit.
