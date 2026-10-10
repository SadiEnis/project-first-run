# Demo HUD — Layout and Behaviour Contract

## Status and scope

Implementation plan based on the approved sketch; the basic HUD was accepted in gameplay; inventory, radial shading, slanted health and weapon motion are now implemented, pending new visual acceptance. Full-width XP at the top, abilities top-left, upgrades top-right with the key underneath, weapon/ammunition/health bottom-left. First integration targets WideCombat. Geometry, encounter density and game balance remain unchanged by HUD work.

The first checkpoint was documentation; the basic implementation slice is now added below. Meta progression, HUD settings, final icon artwork and automatic rollout to other scenes are out of scope. The free DOTween version is an option for the animation increment; Pro is not required. No package has been installed.

## Layout and authoring

| Area | Content |
| --- | --- |
| Top edge | Full-width XP with safe margins; level centred below |
| Top-left | Ability slots matching active capacity |
| Top-right | Upgrade slots matching active capacity; owned key below |
| Bottom-left | Active weapon card; magazine/reserve and health alongside |
| Centre | Minimal crosshair and contextual interaction prompt |

Use a saved, Inspector-editable Canvas prefab. Do not rebuild the scene at runtime; capacity-based slot instances are permitted. Anchors, scaling and margins must prevent overlap at different aspect ratios. Health includes current/maximum values. Missing icons have identifiable text/placeholder fallback, distinct from an empty slot.

## Capacity and inventory

Read PlayerBuildCapacity and current run inventory; presentation must not grant capacity/items. The current demo starts with 2 weapon, 3 ability and 5 upgrade slots; model maxima are 2/5/8. Do not hardcode these counts into the view. Future meta-derived run capacity uses the same contract.

Show active empty slots, not locked capacity. Acquisition fills a slot; level-up updates its level in place. Keep inventory order stable. Restart clears previous items, key and animation state.

## Weapon shading and reload

- Ready card: normal. Zero reserve alone does not shade a loaded magazine.
- Empty magazine and reserve: persistent translucent black overlay, readable 0/0.
- Automatic/manual reload, including a partially loaded magazine: start fully shaded, reveal clockwise from the top as the actual reload progresses.
- Partial reveal does not mean usable. Cancellation, switching and death follow runtime state.
- Overlay amount is remaining / actual operation duration, clamped to 0..1. Account for modified durations and zero duration; do not blindly use a base asset duration.
- Ordinary shot intervals and Minigun preparation are not reload indicators.

WeaponRuntimeState exposes ReloadTimeRemaining and IsReloading. Check whether the active operation duration needs a read-only presentation accessor. The HUD never independently completes a reload or fills ammunition.

## Ability shading

Shade after a successful use starts cooldown; reveal clockwise from the top. A failed use or missing target must not start a visual cooldown. An unshaded icon means ready by cooldown, not necessarily firing: automatic abilities may wait for a target.

AbilityRuntimeState exposes CooldownRemaining; committed duration includes the cooldown multiplier. Normalisation must use the actual active duration. AbilityRuntimeEntry.IsContinuous alone is insufficient to choose a timer: continuous executors may have periodic attacks. DroneRuntime maintains independent per-drone timers. Audit every ability in its presentation increment and expose read-only state as needed. Do not fabricate a single radial cooldown for a passive/continuous effect with no such timer. Multi-drone timing requires its own presentation decision.

## Clockwise weapon-card exchange

Cards travel clockwise around a shared arc. The waiting card enters from off-screen left along the upper arc; the former active card exits along the lower arc to the left. Mask the hidden portion. Keep text readable; slight tilt is optional, upside-down cards are not intended.

Initial tunable duration: 0.25–0.35 seconds. Health stays fixed; ammunition belongs to the actual active weapon. Switching happens in gameplay without waiting for the animation. Rapid Q presses retarget the latest real selection without a backlog. One owned weapon has no fictitious second card. An empty weapon retains its shaded state through movement.

## Data and lifecycle

Read-only presentation: subscribe to health, XP, inventory and selection changes where available; poll active remaining times lightly as needed. Wait safely for source initialisation. Explicitly assign/validate scene references; no per-frame scene-wide searches.

XP uses CurrentExperience / RequiredExperience, not TotalExperience. Multiple level gains settle on the real final level and remainder. Maximum-health changes update the health display.

Reward pause must not let cooldown/reload visuals advance independently. Decorative motion is separate from game timers; stop hidden HUD animations. Keep existing death/restart/quit functionality. Show HUD after opening control handoff, hide during final presentation. Restart cleans subscriptions/tweens and binds the fresh player.

Avoid duplicate OnGUI and Canvas HUD. Disable only the replaced health/ammo/XP/crosshair portions; preserve objective text, error reporting and death/restart/quit until explicitly migrated.

## Implementation slices and acceptance

1. Documentation checkpoint: this contract and indexes.
2. Basic prefab: static weapon, health, ammo, XP/level and capacity-driven empty slots; no animation. User evaluates layout.
3. Inventory binding: icons/fallbacks, levels, stable slot order and key.
4. Radial presentation: reload/empty ammo, then ability-specific cooldowns.
5. Weapon-card motion: repeated/rapid switching, one weapon, death/finale cleanup.
6. Integration: aspect ratios, reward pause, restart, finale and Windows build validation when needed.

Evaluate each meaningful slice; do not make every tiny operation a separate commit. Write EditMode/PlayMode coverage; user runs tests by preference, reported failures receive targeted attention.

Planned coverage: zero/default/max capacity; acquire/level-up slots; multi-level XP; max-health changes; loaded magazine with zero reserve; manual/automatic/interrupted reload; cooldown modifiers; failed use and target wait; pause; rapid switching; missing icons; death/finale/restart; no duplicate HUD or subscription leaks. Gameplay visual acceptance checks clockwise reveal and the sketched card trajectory.

## Delivery

The saved Assets/_Project/Prefabs/UI/DemoHUD.prefab Canvas is connected in WideCombat. It displays XP/level, current/maximum health, active weapon name/level, magazine/reserve and textual reload/empty-ammo state. Ability/upgrade slots reflect run capacity and fill in acquisition order with symbolic icons and levels. An owned key label appears below upgrades. Reload/cooldown shading and weapon-card motion are now connected.

DemoHudController validates scene references and resolves player components once. LateUpdate reads basic state; DemoHudView caches values to avoid rebuilding unchanged labels/bars. No gameplay mutation, independent timer or event subscriptions. HUD stays hidden before readiness, during reward modal, death and finale. Only the replaced debug information/crosshair is suppressed when a valid Canvas HUD is bound. Objective/error/restart/quit functionality stays intact; the original Opening scene retains legacy presentation with its null HUD reference.

EditMode DemoHudLayoutTests cover prefab references, zero/default/maximum capacity, visibility states and scene/prefab binding. PlayMode DemoHudPresentationTests cover health/XP/weapon, reload/empty-ammo text and hide/resume. Tests were written but not run. Full-run/restart/finale and aspect-ratio acceptance require gameplay verification. Static local-reference and whitespace checks do not establish Unity import/compilation or visual correctness.

Open the updated WideCombat scene from disk and enter Play Mode. No scene-generation menu or navigation bake is required. Edit DemoHUD in Prefab Mode; Demo HUD binding stores scene dependencies. Space at the bottom is reserved for the existing objective message.

## Icon and motion increment

Verification: runtime and both test assemblies compiled with the installed Unity Roslyn compiler against local Unity references. No compile errors; the existing PlayerChestInteractorTests.TearDown hiding warning remains unrelated. Saved prefab local references and parent links were checked. This is compilation/static verification, not Unity test execution or visual acceptance.

- DemoHudIcons is an Inspector-editable stable-ID catalog. Released weapons, abilities and upgrades have distinct code-native line glyphs; these are prototype symbols, not final artwork. Assign a Sprite in an entry to override its glyph. Missing entries show a question mark plus level rather than looking empty.
- HudSlantedImage draws both the health track and fill with a right-hand diagonal. Anchor width follows actual health; the leading edge retains its slope (narrow fills taper safely to zero). XP remains rectangular.
- HudRadialShade covers the rectangular icon/card and clears clockwise from twelve o'clock. Text levels remain readable above the shade. Full remaining = black translucent overlay; zero remaining = clear.
- AbilityRuntimeState.CommittedCooldownDuration and WeaponRuntimeState.CommittedReloadDuration snapshot successful operations. Reconfiguration mid-wait does not change their denominator or the timer itself.
- Standard abilities use the committed cast wait. Shuriken is clear while orbiting and shades its real between-orbit cooldown. Drone displays the soonest next drone's remaining/committed shot interval; one ready drone clears it. It does not imply all drones are ready or a target exists. No invented passive cooldown.
- HudWeaponCarousel uses two authored cards and a tunable 0.3-second smoothstep arc without DOTween. The incoming card follows the upper clockwise arc, the outgoing card the lower arc. Each card retains its own icon, level and reload/empty-ammo state. Screen clipping hides the off-screen position; captions stay upright. Health and ammo do not move.
- Rapid selection changes retarget from current angles toward the latest selected weapon, clockwise, without queuing. Only acquired/observed cards are shown. Hidden HUD motion snaps to the latest displayed selection; a fresh run instantiates fresh state. Time.deltaTime freezes decorative movement during pause; actual timers are only read.
- No regeneration or navigation bake required. WideCombat's explicit key reference is added to Demo HUD binding. Original Opening, scene geometry, balance and user encounter edits are untouched.

Targeted coverage was added to DemoHudLayoutTests and DemoHudPresentationTests for timer snapshots, radial direction, arc direction/retarget, catalog coverage, item acquisition/level/key, reload fraction and hidden-motion cleanup. Unity test execution remains with the user. Check partial manual reload, automatic empty reload, no-ammo state, ability target waits, rapid Q, reward pause, key pickup, finale and restart in gameplay.
