# Automatic empty-magazine reload

## Fix contract

On content, an active weapon with zero magazine rounds and positive reserve automatically starts the existing reload, without an R press or another fire attempt. Manual R remains available for partially empty magazines. No ammunition source, infinite ammo, balance change or new animation is introduced.

- Check empty magazines during normal controlled updates (including after equip/resume) and immediately after the final successful shot's firing work.
- Reuse the existing reload validation, duration and transfer logic. Emit ReloadStarted once, reset Minigun preparation, and emit the existing completion/ammo notifications when the timer finishes.
- No reserve means no reload or repeated start events. Partial reserve transfers only available rounds. No refill before the timer completes.
- Reload cannot start/progress while weapon control is disabled or time is paused; death follows the existing disabled-control rule. Inactive weapons retain their state; no background reload is added.
- Holding automatic fire resumes firing after reload. Minigun must prepare again; semi-automatic weapons require a new press. Switching weapons preserves the existing remaining reload time.
- Verify existing behavior plus real last-round shots on PlasmaRifle, Shotgun and Minigun, idle empty magazines, repeated R, held Minigun, no reserve, partial reserve, pause/control/death and switching.
- Commit intent: `fix: auto-reload empty magazines when reserve ammo is available`. User handles check-in/commit on content; no merge/PR is needed for this increment.

## Verification and user observation

- PlayerWeaponController checks automatic reload before controlled processing and after firing, sharing StartReload with manual R. WeaponRuntimeState ammo transfer and reload durations remain unchanged.
- Unity 6000.3.9f1, isolated copy: **824/824 EditMode and 489/489 PlayMode passed**. Reports: `.codex-temp/xp-attraction/auto-reload-EditMode.xml` and `auto-reload-PlayMode-v2.xml`.
- Four PlayMode cases added: input-free reload with partial reserve/repeated R; real last-round shots across PlasmaRifle/Shotgun/Minigun; held Minigun re-preparation and reserve exhaustion; pause/control/death restrictions. Existing manual reload and switching regressions also passed. The multi-weapon test needed fresh input frames between presses; no additional runtime change was needed for that test correction.
- The user observed that reaching zero rounds activates reload. This confirms the requested start behavior, not separate manual confirmation of every edge case or the full transfer lifecycle.
- Changed code/tests hash-match the tested copy; `git diff --check` is clean. No ammo-drop system, build, commit/check-in or merge was performed.
