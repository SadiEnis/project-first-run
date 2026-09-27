# Survival stat upgrades

## Status and scope

Mechanics approved for Second Heart, Ironhide and Lifesprout on upgrades-content. This is the pre-implementation documentation checkpoint. No runtime implementation or test execution is claimed.

Use the existing shared upgrade slots and five-level progression. Each level replaces the item's previous full effect set; other sources remain intact. Values are provisional balance.

| Item | Turkish name | L1 | L2 | L3 | L4 | L5 |
| --- | --- | --- | --- | --- | --- | --- |
| Second Heart | İkinci Kalp | +10% max health | +20% | +30% | +40% | +50% |
| Ironhide | Demir Deri | 5% damage reduction | 10% | 15% | 20% | 25% |
| Lifesprout | Yaşam Filizi | 0.5 health/s | 1 | 1.5 | 2 | 2.5 |

## Second Heart

- Evaluate maximum health from the configured base and current modifiers, never from the previously modified maximum.
- For a living player, a maximum-health increase also grants the positive difference in current health: 60/100 becomes 70/110. It does not fully heal.
- On maximum-health decrease, keep current health unless it exceeds the new maximum; clamp only the excess. This is not damage and must not trigger damage reactions.
- Re-evaluating unchanged modifiers or changing maps must not repeatedly grant health.
- Maximum-health changes must not resurrect a dead player. Explicit new-run/reset behavior remains separate.

## Ironhide

- Add ordinary damage-reduction percentages from applicable sources, then clamp the total to 0–75%.
- Applied incoming damage = incoming amount * (1 - effective reduction), before limiting actual health loss to remaining health.
- Fractional damage is supported; do not round or enforce an arbitrary minimum hit of one.
- Apply once at the player's shared incoming-damage boundary, covering existing enemy attacks and other damage routed through that boundary. Do not apply the player's armor to enemies.
- Keep original incoming damage information distinguishable from actual health lost for events and future reactions. Healing and maximum-health clamping are not damage.
- Later trait interactions require their own agreed rules; this does not implement Iron Oath or Glass Heart.

## Lifesprout

- Heal once per second of active, scaled gameplay time; each tick restores the current level's health-per-second value.
- Works during combat as well as exploration. Stop during pause/reward selection and after death/run termination.
- Clamp at maximum health, never resurrect and never bank unused healing at full health.
- Level changes must not grant an immediate bonus tick or duplicate a regeneration loop.
- Avoid scene-transition/loading time creating healing bursts. Preserve run stats without duplicating timers or stat subscriptions.

## Implementation boundaries

The current HealthState maximum is immutable and no healing operation exists. Extend the shared health model with validated healing and live maximum-health changes; do not use spawn initialization/reset as an upgrade effect, since that would replace live health state.

Keep player-stat integration separate from enemy health configuration and pooled enemy initialization. Preserve existing damage/death events and exactly-once death behavior. Publish health changes when current or maximum health changes, so debug/UI consumers remain accurate.

Add three real assets, saved content arena/F1 integration, scene-builder integration and upgrade/mixed reward-pool integration. Preserve maximum-level exclusion and existing slot limits. No special frame glow, gold, meta progression, evolution or build work in this group.

## Verification plan

- Author tests for five-level tables, full-effect replacement, additive stacking and maximum-level rejection.
- Maximum health: injured/full/dead states, increase/decrease, unchanged re-evaluation, invalid values and event behavior.
- Mitigation: zero armor, each level, multiple sources, 75% cap, fractional damage, lethal damage and no double application.
- Regeneration: tick cadence, level changes, overheal, no stored healing, pause/reward screen, death and prevention of duplicate loops.
- Cover persistence across map transitions and fresh-run reset without carrying previous-run modifiers.
- Update catalog/pool expectations and test the three upgrades together.

User runs Unity compilation, EditMode/PlayMode and gameplay acceptance. Assistant authors tests but runs only specifically requested missing/failing tests. Stop for manual acceptance before implementing the next group.
