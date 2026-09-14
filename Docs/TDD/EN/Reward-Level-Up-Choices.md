# Reward level-up choices

Reward offers now use the same `ItemDefinition` choice identity for both new acquisitions and level-ups. `RewardCandidateFilter` includes an owned item only while its run snapshot is below maximum; full categories do not block such a level-up. Maximum-level items are excluded.

Claim handlers preflight the level-up path and fall back to acquisition only when the item is not owned. A successful level-up returns the existing `Claimed` result, consumes the reward session once, does not consume a slot and does not publish an acquisition event. The UI labels choices `NEW`, `LEVEL UP` or `MAX` from the current build snapshot.

No reroll, gold fallback, multi-select, save migration or automatic level-up is introduced. Ability/weapon/upgrade runtime controllers remain responsible for applying effects atomically.

## Validation — 2026-09-14

The full isolated Unity 6000.3.9f1 suites passed: 666 EditMode and 351 PlayMode tests. The updated showcase covers a new acquisition followed by a level-up choice in all three categories.
