# Ranged enemy foundation

## Status and scope

Owner-approved implementation contract for Ranger. Continues the enemy-variety stage on `feature/enemy-variety-foundation` after Charger commit `6f087ae`. That branch originally starts at integrated main `b8aa45f` / Plastic dev `cs:173`; this increment does not start a separate development stage.

The owner verified advanced chests, Chaser contact damage and Charger attacks in gameplay before approving Ranger. Ranger encourages lateral movement and target prioritization alongside those enemies.

## Accepted behavior

- Approach when outside the preferred distance band. Hold position inside it. Retreat when the player gets too close, using navigation-valid destinations and a margin between enter/exit thresholds to prevent constant oscillation.
- Attack only with an unobstructed line of sight and within firing range. Use a visible wind-up followed by one visible projectile and a cooldown.
- Lock the projectile direction when the shot is released. The projectile travels straight, does not home, and can be dodged. Damage occurs only on collision, once per projectile; walls block it and its lifetime is bounded.
- Retreat does not guarantee a route. If no safe navigation destination exists, remain on walkable ground and retry later; do not teleport through obstacles or leave the arena.
- Rank remains independent from behavior. The initial example is Normal Ranged; its stats, attack timing, projectile speed and preferred distance band are explicit configuration. Numerical balance is provisional until gameplay evaluation.

## Lifecycle and integration

Use explicit target/damageable injection and the existing registry, health, XP and drop lifecycle. Pause freezes movement, wind-up, cooldown and projectile lifetime. Enemy death or disable cancels uncommitted shots. A projectile already released remains a separately owned attack until impact or expiry; it must not depend on its shooter still existing. Player death prevents further damage, and projectiles belong to the arena so scene unload removes them.

Keep the first wave and total showcase size controlled. Wave 2 now adds one Normal Ranger to the existing Chaser/Charger/Elite Charger lineup (four enemies total). Elite advanced-chest weights and boss encounters remain separate mechanics decisions within the broader enemy milestone.

## Planned verification

EditMode: distance-band thresholds, wind-up/cooldown transitions, invalid configuration, bounded lifetime and one-hit commitment.

PlayMode: approach/hold/retreat on navigation, blocked retreat, wall occlusion, evadable projectiles, owner-independent projectile lifetime, pause, cancellation, cleanup and existing Charger/drop/wave regressions.

## Implementation values and decisions

Initial Ranger: 75 health, 10 damage, 25 XP, existing Normal chest profile. Preferred band 5–9 m with 1 m hysteresis; firing range 12 m; warning 0.6 s; cooldown 1.5 s; projectile speed 7 m/s, radius 0.15 m and lifetime 4 s. These are editable prototype values. Retreat is prioritized over starting a shot. A blocked sight line cancels wind-up and prevents firing. Holding enemies may wait behind cover; deliberate flanking is deferred.

Direction is sampled at release toward the target collider center (explicit target fallback: root + 1 m). Projectiles sweep their full movement segment, check initial overlaps, collide with the nearest solid obstacle or injected target, ignore shooter/enemy colliders and triggers, and resolve at most once. Lifetime limits movement even on a long frame. Projectile damage is separate from the release event; no instant damage occurs when shooting. Released projectiles are roots in the shooter's scene and remain independent of shooter cleanup.

The existing enemy prefab supplies composition. Ranger behavior logic is a separate runtime delegated by EnemyAttackController. EnemyMotor owns navigation commands and validates retreat paths. A blue identity tint and yellow wind-up cue distinguish Ranger. Automated validation passed 700 EditMode and 378 PlayMode tests on 2026-09-14 in the isolated Unity 6000.3.9f1 project. Ranger gameplay acceptance and balance tuning remain pending.
