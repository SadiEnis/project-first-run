# Item level foundation

## Baseline and sequence

Starts from integrated Plastic `/main/dev` **cs:160** and Git `main` **80699d2**, after owner acceptance and integration of the three Common chests. Branches: `/main/dev/item-level-foundation` and `codex/item-level-foundation`. Neither starts from the previous feature branch.

Implement in dependency order:

1. Run-owned level state, configurable maximum, validation and compatibility (this increment).
2. Level-specific runtime effects and content for the implemented weapon/ability/upgrade samples, preserving ammunition, reload/cooldown and slot ownership.
3. Explicit acquire/level-up reward choices and transactional claims, current/next-level UI, and max-level eligibility.

Only step 1 is implemented in this increment. Existing chests still offer acquisitions, not level-ups. Do not merge or describe the entire item-level reward feature as complete after the state foundation alone.

## State contract

`ItemDefinition.MaximumLevel` is authoring data, defaulting to 1 for existing content until actual level effects are configured. Invalid nonpositive maxima are rejected, not silently clamped. GDD base weapons and abilities target eight levels, but that is not a reason to expose seven currently unimplemented rewards. Upgrade level counts are still an open design decision; no global count is invented.

`PlayerBuild` owns one `PlayerBuildItem` per (category, ordinal stable ID). Acquisition snapshots the maximum and starts at level 1. Definition assets never store mutable run levels. Separate builds have independent state; changing an asset after acquisition does not change an existing item's maximum. Existing string-list queries and acquisition results remain compatible. Legacy ID-only additions default to a single-level item.

Read-only entry properties expose category, stable ID, current level, maximum and whether the maximum has been reached. Missing queries return false (or level 0). The domain-level `TryLevelUp` advances exactly once, returns NotOwned or MaximumLevelReached without mutation, and never consumes another slot. Reacquiring an owned ID returns AlreadyOwned; it does not upgrade, reset or replace the item's configured maximum. Invalid categories/IDs/maxima fail before mutation.

`PlayerBuildController.TryAdd(ItemDefinition)` validates and snapshots the authored maximum. The level mutation API intentionally remains on the pure build model for now; no chest/claim/debug shortcut calls it before runtime effects can be updated atomically.

## Verification and exclusions

2026-09-14 verification: **655 EditMode / 332 PlayMode passed** in the isolated Unity 6000.3.9f1 project. The 29 new EditMode cases cover this state foundation; the full PlayMode suite remains unchanged and passes. All nine new/changed Unity source, test and meta files matched the tested copy by SHA-256. This is automated validation of the state increment, not gameplay acceptance of level-up rewards.

Test all three categories, initial and missing states, maximum boundaries, full slots, duplicate acquisition, independent runs, immutable public entries, read-only ownership lists, definition validation and maximum snapshots. Run both full suites for acquisition, weapon-switching, ability, upgrade and chest regressions.

No new level effects, eight-level sample balance, item-level UI, reward eligibility changes, evolution, gold fallback, save migration or level debug buttons are part of this increment. No gameplay change is expected yet. Continue with runtime effect application before exposing upgrades as rewards.
