# Loaded Dice

## Status and scope

Implemented and accepted by the user. The user reports EditMode/PlayMode without errors and an L3 gameplay sample of 20 level-up chests: four common and two legendary, with the rest green/purple. This is compatible with the expected 21.13% common probability at L3, not statistical proof or final balance. Loaded Dice (Hileli Zar) uses the shared upgrade slots and three levels. Each level replaces the previous full effect, rather than stacking earlier levels.

## Implementation notes

Luck uses AdditivePercent modifiers 0.2/0.4/0.6 evaluated against base 1. Both scene sources read the player's current stats at selection time; cached pending definitions retain their identity. Missing/uninitialized stats use 1. Shared assets are not mutated. ChestDefinition.AffectedByLuck is explicit opt-in: only Green/Purple/Legendary assets enable it; Boss and other definitions default to false even when their rarity is Legendary.

The multiplier-1 path preserves existing integer random ranges. Boosted selection keeps weights in double precision and uses a single integer random sample in [0, 1,000,000,000) against normalized cumulative boundaries. This discretizes probability to approximately one-billionth without rounding individual weights. Multipliers below 1 or non-finite values are rejected. Profile generation rolls retain their original [0,10000) range and threshold.

Catalog/mixed pool now contain 28 items, upgrade pool 15. Added LoadedDiceChestTests and LoadedDiceUpgradeTests; extended source retry tests and content checks. Coverage includes selection boundaries, exclusions, generation thresholds, ownership/level replacement and fresh-player stats. A full asynchronous scene-travel run and manual probability balancing have not been executed or newly verified by these tests.

| Level | Green/Purple/Legendary weight multiplier |
| --- | --- |
| Unowned | 1.00 |
| 1 | 1.20 |
| 2 | 1.40 |
| 3 | 1.60 |

These are initial balance values, not a claim of final balance. Describe the bonus as increased selection weight, not percentage points or a guaranteed increase of the same percentage in final probability.

## Probability contract

- Use the GDD 15.9 source-specific baseline. Multiply only Green, Purple and Legendary weights by the current multiplier, then normalize over all eligible entries. Common Weapon/Ability/Upgrade weights remain unchanged and equal to one another in the baseline tables.
- Apply to normal-enemy, elite-enemy and level-up chest type selection. Normal/elite chest-generation chances stay 25%/50%; each level-up still grants one chest.
- Read the player's current luck when selecting the chest definition. An already selected, spawned or pending-placement chest does not reroll when the upgrade level changes or placement is retried.
- Do not mutate shared ScriptableObject weights. Preserve existing unmodified callers and random-source injection. Avoid rounding adjusted entries individually into biased integer weights. Document and test the chosen numeric selection scheme during implementation.
- Integrate with the existing Luck stat after auditing its consumers; missing player stats mean the unmodified baseline. Validate non-finite/invalid inputs explicitly.
- Boss/Golden rewards, evolution chances, reward choices inside a chest, extra chest counts and bad-luck protection are outside this item's scope. Special tables containing excluded entries must not accidentally boost those entries.
- Preserve ownership across map travel; fresh runs reset ownership. Add the asset to the content arena, builder, F1 catalog and upgrade/mixed pools, preserving capacity and maximum-level exclusion.

For baseline advanced probability A and multiplier m, the new combined advanced probability is A*m / (1-A+A*m). Individual advanced types retain their relative proportions.

| Source | Unowned | L1 | L2 | L3 |
| --- | --- | --- | --- | --- |
| Normal: Green or better, given a chest | 35% | 39.25% | 42.98% | 46.28% |
| Elite: Green or better, given a chest | 60% | 64.29% | 67.74% | 70.59% |
| Level-up: Green or better | 70% | 73.68% | 76.56% | 78.87% |

## Balance evaluation, not presentation polish

Chest quality is part of the core reward economy. Separate baseline reward pacing, this optional upgrade's value, and protection from prolonged unlucky streaks. Do not make owning Loaded Dice a requirement for satisfying rewards.

For normal enemies, L1/L3 improve conditional quality by about 4.25/11.28 percentage points, not 4/15. Including the unchanged 25% drop chance, Green-or-better probability per kill moves from 8.75% to about 9.81%/11.57%. Per 100 normal kills, that is an expected additional 1.06/2.82 advanced chests, not guaranteed drops. This also does not measure the utility of their contents.

Evaluate real run length, chest sources and counts, acquisition time, remaining reward opportunities, time between valuable rewards, reward usefulness, build strength and the upgrade slot/three-selection opportunity cost. Early acquisition has longer to pay off; late acquisition may be weak. Compare runs with and without the item, not only repeated F1 level-ups. Three small samples cannot establish final balance.

Keep 20/40/60% weight bonuses initially. Adjust one balance lever at a time after measurements. Level/region-dependent tables and bad-luck protection are separate future design decisions, not silently included here. If introduced later, specify their ordering relative to luck and test the combined distribution. No telemetry system is required by this documentation step.

## Verification plan

Author deterministic tests for unowned/three-level probabilities across all three sources, selection boundaries, equal common shares, preserved advanced relative proportions, excluded Boss/Golden entries, invalid input, unchanged shared assets, preserved generation chances, level replacement, maximum level, missing stats, pending-placement retention and run/map ownership. Do not use small random samples as pass/fail probability assertions.

The user reports successful EditMode/PlayMode and manual acceptance. Tests were not run by the assistant. This increment is ready for the implementation check-in; broader reward-economy balancing remains open.
