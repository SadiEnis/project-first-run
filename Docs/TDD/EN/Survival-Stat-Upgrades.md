# Survival stat upgrades

## Status and scope

Second Heart, Ironhide and Lifesprout are implemented on upgrades-content. Unity compilation, automated test execution and manual gameplay acceptance are pending user validation.

Implementation adds validated healing/live maximum changes to HealthState/HealthComponent and a PlayerSurvivalController on the saved player prefab. DamageReduction (8) and HealthRegeneration (9) extend the stat enum without renumbering existing values. Reduction is stored as additive percentage points via Flat modifiers against zero; regeneration is flat health/second. MaxHealth uses the existing percentage operation.

The controller subscribes once to stat changes, preserves the configured base maximum, and drives scaled-time regeneration. Pauses/control locks reset the partial tick, so resuming needs a full second. Full health, death and disable also discard partial ticks; level changes do not. The optional run reference (Inspector or BindRun) stops healing on Victory/Defeat and is connected by the existing two-session development bootstrap. Custom run hosts must also bind their run; map scenes without a run controller use player control/death/time gates.

The existing scene-travel flow moves the same player object and freezes gameplay, preserving these stats and health. Fresh player creation starts clean; ResetHealth is still a revive/refill operation, not an upgrade/build reset.

The saved content catalog now contains 19 items; upgrade/mixed pools contain 6/19. Added SurvivalHealthStateTests and SurvivalUpgradeTests cover health boundaries, mitigation, progression, regeneration, enable cycles, scene relocation and a fresh player. Scene relocation is not a full asynchronous loading test. Catalog tests were updated. No Unity test pass or compilation is claimed.

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
