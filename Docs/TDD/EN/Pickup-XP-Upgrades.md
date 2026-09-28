# Pickup and XP upgrades

## Status

Attraction Core and Memory Crystal are implemented on upgrades-content. Unity compilation, test execution and gameplay acceptance are pending user validation.

PickupRadius (10) and ExperienceGain (11) were appended without renumbering existing stats. The collector evaluates its configured base radius for queries and runtime gizmos. PlayerExperienceController applies the XP multiplier once and stores a decimal fractional remainder; conversion from the evaluated float multiplier uses decimal precision rather than retaining binary float noise. Awards exceeding the existing integer-award limit fail before either state or remainder commits.

The existing F1 XP grant uses the gameplay bonus and now reports the actual integer XP gained alongside the base amount. Item-level controls are unchanged. Pickup notification-error handling checks both integer XP and the fraction before deciding whether an award committed.

Saved catalog and mixed pool now contain 21 items; the upgrade pool contains 8. No chest interaction code changed. Added PickupExperienceUpgradeTests and expanded CoreStatUpgradeTests cover progression, boundaries, fractions, collection-time bonuses, rejected awards, notification failures, multiple levels, scene relocation and fresh players. Scene relocation is not a full asynchronous loading test. Test execution remains with the user.

Both use the existing shared upgrade slots and five levels. Each level replaces its previous full effect set; ordinary percentages from different sources add. Numbers remain provisional balance.

| Item | Turkish name | L1 | L2 | L3 | L4 | L5 |
| --- | --- | --- | --- | --- | --- | --- |
| Attraction Core | Çekim Çekirdeği | +20% radius | +40% | +60% | +80% | +100% |
| Memory Crystal | Hafıza Kristali | +10% XP | +20% | +30% | +40% | +50% |

## Attraction Core

- Affects only the attraction radius of ground XP pickups.
- No chest attraction, opening range or automatic opening. Other pickup categories are outside this stage.
- Evaluate from the configured base radius; do not compound already modified radii. A 3-metre base becomes 6 metres at level five.
- Preserve attraction speed, physical collection behavior, pickup value and existing death/pause restrictions.
- Physics scanning, attraction eligibility and runtime range visualization must agree on the effective radius.
- Document the existing radius setter as a base-radius setter when integrating stats, and update affected tests intentionally.

## Memory Crystal

- Apply the current XP-gain multiplier when XP is awarded, not when the pickup spawns. Do not modify previously earned XP or level thresholds.
- Route ordinary run XP awards through one application point to avoid double bonuses. Explicit test-panel level controls must retain their documented behavior.
- Retain fractional XP per run: ten 1-XP awards at +10% must grant 11 XP in total. Avoid floating-point drift that loses this final point.
- Carry the fractional remainder across level changes and map transitions; never re-scale the already earned remainder when the multiplier changes. Discard it for a fresh run.
- Preserve existing invalid-input, overflow and reentrant-notification safeguards. Rejected awards must not consume the remainder or pickup; committed awards must not be repeated after notification errors.
- Faster leveling naturally invokes the existing level-up chest flow. Do not change chest generation rules.

## Integration and verification

Add two real assets to the saved content arena, scene builder, F1 catalog and upgrade/mixed pools. Preserve slot capacity and maximum-level exclusion.

Author tests for all five levels, additive stacking, full-effect replacement, radius boundaries/base changes, unchanged attraction speed, fractional accumulation, multiplier changes, multiple level-ups, rejected awards, persistence and fresh-run state. Update catalog counts. Do not add chest interaction behavior.

User owns Unity compilation, EditMode/PlayMode execution and manual acceptance. Assistant authors tests and only runs explicitly requested missing/failing tests. Stop for acceptance after implementation.
