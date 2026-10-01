# Crimson Fang

## Status and scope

Mechanics approved on upgrades-content. Pre-implementation documentation checkpoint; no runtime implementation or test execution is claimed.

Crimson Fang (Kızıl Diş) uses the existing shared upgrade slots and three levels. Each level replaces the previous full effect; no accumulated earlier-level healing.

| Level | Health per credited enemy kill |
| --- | --- |
| 1 | 2 |
| 2 | 5 |
| 3 | 7 |

These are provisional gameplay-test values. At level three, ten eligible kills can restore up to 70 health; evaluate dense encounters during later balance work rather than silently adding a chance, cooldown or rank multiplier now.

## Kill and healing contract

- Guaranteed healing for an enemy killed by the player while the player is alive; no random roll.
- Credit the owner of the lethal damage, not everyone who previously damaged the enemy.
- Player weapon, ability, burn and bleed kills are eligible. Inspect existing damage-source ownership, including projectile/drone/effect sources, before choosing the integration boundary. Missing or unrelated owners must not be credited to the player.
- Each enemy death grants at most one heal. Handle pooled enemy reuse as a new spawn, not as the old death. Despawn, disable, unregister and scene cleanup are not kills.
- Normal, elite and boss enemies grant the same amount for now.
- Apply the current owned upgrade level at the time of the kill. Existing effects may kill later; retaining their damage snapshots does not freeze the healing level at launch time.
- Clamp healing at maximum health; discard excess. Full-health kills do not bank healing.
- Player death prevents healing/resurrection from lingering effects. A simultaneous trade that leaves the player already dead when the kill is handled must not revive them.
- Healing uses the existing health API, not a reset/refill. Incoming-damage amplification and armor do not modify healing.
- Health notifications re-evaluate Last Stand: healing above 30% removes its conditional damage bonus.

## Integration plan

Audit EnemyController death ordering, EnemyRegistry removal and DamageInfo.Source before implementation. Avoid unsubscribing on unregister before the death callback can be processed. Keep ownership explicit rather than granting rewards from a global enemy-count change.

Bind current and newly spawned enemies without duplicate subscriptions; detach old map bindings and preserve player upgrade ownership during scene travel. Fresh runs must not inherit previous-run ownership or deduplication state. Keep any kill-attribution support narrowly scoped; do not implement gold or other kill rewards.

Add one three-level real asset to the saved content arena, scene builder, F1 catalog and upgrade/mixed pools. Preserve slot limits and maximum-level exclusion. No trait-card glow or other pending upgrades in this step.

## Verification plan

Author tests for three-level values/replacement, duplicate acquisition and maximum level, direct weapon/ability and timed-effect ownership, unrelated/missing source rejection, single healing per death, pooled reuse, despawn/cleanup exclusion, full-health clamping, player death, level-at-kill behavior, Last Stand interaction and map rebind/cleanup.

User runs Unity compilation, EditMode/PlayMode and manual acceptance. Assistant writes tests and runs only explicitly requested missing/failing tests. Stop for gameplay acceptance after implementation; final balance remains deferred.
