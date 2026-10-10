# Demo HUD — Layout and Behaviour Contract

## Status and scope

Implementation plan based on the approved sketch; HUD is not implemented yet. Full-width XP at the top, abilities top-left, upgrades top-right with the key underneath, weapon/ammunition/health bottom-left. First integration targets WideCombat. Geometry, encounter density and game balance remain unchanged.

This increment is documentation only. Meta progression, HUD settings, final icon artwork and automatic rollout to other scenes are out of scope. The free DOTween version is an option for the animation increment; Pro is not required. No package is installed in this increment.

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

Documentation only. No scene, gameplay code or package changes; no Unity tests run.
