# Conditional and trade-off upgrades

## Status and workflow

Mechanics approved on upgrades-content. This is the documentation checkpoint before implementation; no assets, runtime changes or test execution are claimed here.

### Last Stand implementation update

Last Stand is now implemented; Iron Oath, Blood Pact and Glass Heart remain pending. Unity compilation, automated execution and manual acceptance of Last Stand are not yet confirmed.

Its three-level asset stores a flat LowHealthDamageBonus (new stat 12) of 0.20/0.30/0.40. PlayerLastStandController on the saved player prefab subscribes to health/stat changes and owns two separate runtime.last-stand additive damage modifiers. It replaces both before notification and removes only its own effects on disable; unchanged bonus values are no-ops, preventing notification recursion and repeated stacking. Maximum-health updates reuse the existing survival controller; no projectile or timed-effect consumers were changed.

Added the item to the saved arena, builder and reward pools (catalog/mixed pool 23; upgrade pool 10). LastStandUpgradeTests covers equality/above/below threshold, healing, death/reset, injured acquisition, all three levels, other-source preservation, maximum-health interactions and disable/re-enable. Existing snapshot behavior remains unchanged; these new tests do not claim full damage-delivery or scene-loading coverage. Tests have not been run by the assistant.

Implement in order: Last Stand, Iron Oath, Blood Pact, Glass Heart. Stop after each item for user gameplay acceptance. Keep the existing branch and shared upgrade slots; each trait has three levels. Each level replaces its own previous effects without accumulating earlier levels or removing unrelated sources. Values are provisional balance.

| Item | Turkish name | L1 benefit | L2 benefit | L3 benefit | Condition or fixed cost |
| --- | --- | --- | --- | --- | --- |
| Last Stand | Son Direniş | +20% damage | +30% | +40% | Living player at or below 30% current maximum health |
| Iron Oath | Demir Yemin | +10 percentage points damage reduction | +15 | +20 | -10% movement speed at every level |
| Blood Pact | Kan Ahdi | +15% damage | +25% | +35% | -20% maximum health at every level |
| Glass Heart | Cam Kalp | +20% damage | +35% | +50% | +25% incoming damage at every level |

All outgoing damage bonuses apply equally to WeaponDamage and AbilityDamage, adding to ordinary bonuses such as Wrath Seal. Costs do not grow with level. These traits do not introduce separate rarity or capacity rules.

## Last Stand

- Active only while alive and current health <= 0.30 * current maximum health; healing above the threshold removes the bonus.
- Re-evaluate on acquisition, level change, damage, healing, maximum-health changes and reset. Buying the item while already below the threshold must work immediately.
- Use dedicated ownership for conditional modifiers. Repeated notifications and threshold crossings must not duplicate bonuses, remove another upgrade's modifiers or recursively mutate health.
- Preserve existing damage snapshot timing: already launched projectiles/started effects are not rewritten. Future damage snapshots use the current bonus.
- Death does not activate the bonus merely because health is zero; no resurrection behavior is added.

## Iron Oath

- Add damage-reduction percentage points to Ironhide and other applicable sources; preserve the shared 75% reduction cap.
- The fixed -10% MoveSpeed adds to ordinary movement bonuses. With Windweave +25%, net movement is +15%, not a separate multiplication.
- Affect configured walk and sprint speeds, not gravity or jump behavior. Do not add a new reload movement penalty.
- Existing movement validation remains; any future combination requiring a new minimum-speed rule needs a separate decision.

## Blood Pact

- Apply -20% MaxHealth and the outgoing damage bonus together as one item's full effect set.
- Evaluate maximum health from the original configured base and all modifiers. Second Heart +50% and Blood Pact -20% produce +30% net maximum health.
- Reuse the accepted maximum-health decrease rule: 100/100 becomes 80/80, while 60/100 becomes 60/80. Clamping is not damage and must not trigger hit reactions.
- Re-evaluate Last Stand against the resulting current/maximum health. A maximum-health change alone can change eligibility.
- No repeated penalty on level-up; the cost remains -20%. Do not reset/refill the player to implement the item.

## Glass Heart

- Incoming damage is multiplied by 1.25 before armor reduction; actual health loss is then limited to remaining health.
- Example: a 20-point hit with 20% reduction produces 20 * 1.25 * 0.80 = 20 health loss.
- Preserve original incoming DamageInfo and distinguish actual applied damage in DamageResult/events. Apply amplification once, only to the player.
- Healing and maximum-health clamping are not incoming damage. No new hit reactions or enemy damage changes are introduced.

## Integration and verification

For each implemented item add its real asset, saved content arena/F1 and scene-builder entries, and upgrade/mixed reward pool entries; retain maximum-level exclusion. Do not expose the remaining unimplemented items as selectable rewards.

Author tests for three-level values, ownership/replacement, ordinary additive stacking, fixed costs, caps, both damage stats, Last Stand threshold equality/above/below/death/healing, maximum-health interactions, no duplicate subscriptions, and Glass Heart order/original-versus-applied damage. Preserve run state across map travel and isolate fresh-run state. Reuse existing snapshot tests and add targeted regressions where consumers change.

User runs Unity compilation, EditMode/PlayMode and gameplay acceptance. Assistant writes tests and executes only explicitly requested missing/failing tests. Manual acceptance is not automated test evidence.

Fluid Mechanism remains deferred without changing its identity. Special inward-glowing card frames, gold/meta, evolution and final balance remain outside this implementation group.
