# Experience and Level Foundation

The original foundation below is extended on the same branch by [Experience Pickups](Experience-Pickups.md). See that document for current scene integration.

## Scope and ownership

Run experience is separate from item levels, build ownership, and permanent progression. `ExperienceState` is the authoritative pure C# run state. `PlayerExperienceController` is a scene-facing adapter; it can bind an existing state when a future run coordinator recreates the player. No singleton or `DontDestroyOnLoad` is introduced.

This stage covers the XP curve, state, gain results, controller notifications, and tests. Enemy XP values, pickups, HUD, chest spawning, save data, and meta progression are subsequent layers. No existing prefab or scene is changed in this stage.

## Curve

Use positive integer XP. Start at level 1 with zero XP. A configurable linear curve defines the cost of advancing from level L:

`required XP = first-level cost + (L - 1) * cost increase per level`

Both curve parameters must be positive. The increasing cost also bounds the number of level transitions in a single integer-sized XP award. Initial authoring defaults are 100 and 50; these are provisional, not final balance. No gameplay maximum level is introduced.

`ExperienceDefinition` stores authoring values and creates an immutable `ExperienceCurve`. Editing an asset must not change an active run's curve. Threshold arithmetic uses `long`.

## Gain contract

- `GainExperience(int amount)` rejects negative values; zero is a no-op.
- Preserve overflow XP after crossing a threshold, including multiple levels in one gain.
- Keep current-level XP and lifetime run total separate; total uses checked `long` arithmetic.
- Calculate the entire result before committing changes, so arithmetic errors leave state unchanged.
- An immutable `ExperienceGainResult` records amount, previous/current level, remaining XP, total XP, next threshold, and `LevelsGained`.
- Numeric representation overflow fails explicitly; this is not a gameplay level-cap policy.

## Notifications and lifecycle

The controller requires explicit initialization. Null or repeated initialization fails without replacing its existing state. It exposes read-only progress properties and forwards awards to the bound state.

For each positive award, publish `ExperienceChanged` once after committing the state. If levels were gained, publish `LevelChanged` once with the same result; `LevelsGained` gives the exact reward count. Example: 500 XP with costs 100, 150, 200 reaches level 4 with 50 XP; the level notification reports 3 gained levels. Consumers must use the immutable result rather than assume exactly one level per callback.

Nested awards during notification are rejected to preserve event ordering. Event subscribers must not throw. Death/control/pause eligibility belongs to the future XP collection boundary; merely opening a reward modal must not erase progression.

## Validation

EditMode tests cover curve validation, asset snapshots, exact thresholds, carry-over, multi-level gains, zero/negative awards, large awards, independent runs, result snapshots, initialization, committed-state notifications, reentrancy, and binding an existing run state. Later pickup tests will cover one-time collection and death gating.

## Next stages

1. Enemy XP values, pickup spawning and collection, simple XP/level HUD.
2. A level-up chest source consuming `LevelChanged` and `LevelsGained`.
3. Other chest sources (normal/elite drop probability and boss rewards).
