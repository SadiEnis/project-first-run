# Advanced chest types

## Scope

This increment extends the existing Common category chests with mixed advanced chest definitions. It is intentionally independent from evolution gameplay, gold rewards, Golden Chest behavior, and boss spawn sources.

The increment covers Uncommon (Green), Rare (Purple working name), Legendary, and a Boss Chest definition. Existing Common Weapon, Ability, and Upgrade chests remain unchanged.

## Reward policy

Each chest definition explicitly owns its requested offer count and maximum number of selections. Rarity is metadata and presentation identity; it does not silently change drop probability, item level, or candidate eligibility.

| Chest | Rarity | Offer choices | Claims | Pool |
| --- | --- | ---: | ---: | --- |
| Green | Uncommon | 3 | 1 | Mixed |
| Purple (working name) | Rare | 4 | 1 | Mixed |
| Legendary | Legendary | 5 | 2 | Mixed |
| Boss | Legendary | 6 | 2 | Mixed |

If fewer eligible items exist than the configured offer count, the offer contains the available unique candidates. The effective claim count is the smaller of the configured claim count and the generated offer size. An empty offer preserves the existing `NoEligibleRewards` behavior: the chest remains available and no modal opens.

The existing eligibility filter continues to remove owned max-level items, full categories, and other invalid candidates. A successful claim still routes through the existing category acquisition/level-up handlers. A chest is consumed only after all effective claims are completed.

## Multi-claim session

The reward claim session remains the authoritative owner of claim state. It records selected definitions, rejects duplicate selections, exposes selections remaining, and closes only when the configured effective claim count is reached. Single-claim Common and Green/Purple chests retain their current behavior.

The selection UI shows the remaining selection count, disables a choice after a successful claim, and keeps the modal open for an incomplete multi-claim session. Player control and time-scale restoration occur only when the session closes.

## Content and sources

Advanced definitions use the existing chest prefab and existing development item assets. A mixed reward pool may contain Weapon, Ability, and Upgrade definitions; its category boundary is validated by `ChestDefinition` and `RewardItemPool`.

Advanced chest definitions are added to a dedicated development source table for deterministic content tests and showcase wiring. Existing level-up and enemy source tables keep their current Common-only weights in this increment so the previously accepted run loop remains stable. Connecting advanced definitions to rarity/drop probabilities is a later balancing step.

The Boss Chest definition is validated and available as content, but no enemy or boss source spawns it yet. No evolution option, evolution probability, gold fallback, Golden Chest, or persistent currency is implemented here.

## Presentation

The existing placeholder chest view displays the definition name and rarity label. Colors remain development presentation only; gameplay rules read serialized rarity/policy fields rather than display color or working names.

## Verification

EditMode tests cover advanced definition validation, mixed-pool boundaries, explicit rarity/policy values, offer/claim count constraints, and unchanged Common compatibility. PlayMode tests cover four-, five-, and six-choice offers, multi-claim completion, duplicate-choice rejection, chest consumption only after the final claim, and regression of single-claim behavior.

Manual validation uses the development showcase to open Green, Purple, Legendary, and Boss definitions. The first two close after one valid selection; Legendary and Boss remain open after the first claim and close after the second. No evolution or gold option is expected.

Validation on 2026-09-14: **681 EditMode / 355 PlayMode** tests passed in the isolated Unity 6000.3.9f1 project. The Test_Waves fixture contains the three Common starters plus the four advanced definitions; source tables remain Common-only so the accepted run loop is not randomized by this increment.
