# Weapon level effects

## Validation — 2026-09-14

The isolated Unity 6000.3.9f1 project passed 666 EditMode and 340 PlayMode tests, including 11 new EditMode and eight new PlayMode cases. Coverage includes active/inactive weapons, damage modifiers, acquisition-time snapshots, malformed progression, maximum level, inconsistent ownership and ammo/timer preservation. This is automated runtime validation, not manual acceptance of chest level-up rewards or UI, which are not implemented yet.

Continuation on `/main/dev/item-level-foundation` / `codex/item-level-foundation` after state foundation cs162 / fd6fd4b. This increment implements weapon effects first; ability/upgrade effects and chest level-up choices remain subsequent work on this branch.

## Contract

- Level 1 uses existing weapon fields. Each additional level supplies absolute base damage, magazine capacity, shots per second and reload duration. The authored list must contain exactly MaximumLevel minus one entries; every entry must be valid and capacities cannot decrease. Validate the complete progression before acquisition.
- A runtime entry snapshots level configurations at acquisition. Changing shared authoring assets does not alter an already acquired progression. Player stats apply on top of the current level's base damage.
- `PlayerWeaponAcquisitionController.TryLevelUp` is the runtime-facing authority: require matching build/loadout ownership, exact definition, current level and maximum. Preflight before either state changes. Missing ownership returns NotOwned, capped items return MaximumLevelReached; mismatches throw without mutation.
- Preserve the same runtime entry and ammo state for active and inactive weapons. Magazine and reserve counts stay unchanged. Increasing capacity adds empty space, not ammunition. Existing fire/reload remaining seconds stay unchanged; new rates/durations apply to the next action. An ongoing reload fills toward the new capacity on its original completion time using available reserve. Reset remains an explicit separate operation.
- No automatic equip, duplicate acquisition or acquire event occurs on level-up. Switching retains the upgraded entry. Do not call the pure build-level method directly for live weapons.

## Sample content and limits

Historical checkpoint: the Plasma Rifle sample progression below is superseded by [Plasma Rifle Content](Plasma-Rifle-Content.md), which implements the GDD's projectile/piercing/burn sequence. The generic ammo/timer preservation contract above still applies.

Plasma Rifle and Development Secondary get eight provisional cumulative test levels: damage at 2, capacity at 3, rate at 4, damage at 5, reload duration at 6, capacity at 7, damage at 8. Values are development balance, not completed GDD weapon designs or evolution eligibility. Trigger mode, range and damage mask are unchanged. Fireball and upgrades remain single-level until their own effects are implemented.

There is no new key binding, level-up reward or UI in this increment. Automated tests call the runtime-facing controller; the existing chest acquisition behavior remains unchanged. Validate full suites plus active/inactive upgrades, modifiers, ammo/timer preservation, malformed content, maximum boundaries and inconsistent ownership. Continue with ability/upgrade effects and then explicit reward choices; do not merge the entire item-level stage yet.
