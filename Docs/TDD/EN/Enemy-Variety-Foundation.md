# Enemy variety foundation

## Status and baseline

Mechanics accepted by the owner; implementation contract for the Charger increment. Gameplay acceptance will be recorded separately after testing.

Plastic `/main/dev/enemy-variety-foundation` starts explicitly at integrated dev `cs:173`. Git `feature/enemy-variety-foundation` starts at integrated main `b8aa45f`. Advanced chests were merged with automated validation; owner gameplay review remains pending.

GDD section 17 requires tactical enemy variety but leaves families and boss mechanics undecided. This increment implements the agreed Charger family.

## Accepted mechanics

- Keep the existing close-range Chaser as the baseline.
- Charger approaches, locks direction at the start of a visible wind-up, charges along that direction, then recovers before pursuing again. Moving sideways during the warning lets the player evade it. Wind-up, charge speed, distance and recovery are configurable; initial numbers need playtesting.
- Separate rank (`Normal`, `Elite`, `Boss`) from behavior. An Elite can belong to the Chaser or Charger family. Rank alone does not multiply stats or choose loot; definitions explicitly configure health, damage, movement, XP and drop profile.
- Start the playable elite example with Elite Charger, identifiable through placeholder presentation and configurable timings. It reuses the same behavior contract rather than duplicating the attack code.
- Reserve Boss classification for the foundation. Design a dedicated boss encounter and its attacks separately before adding a playable boss.

## Technical boundaries

Use the existing EnemyDefinition, explicit target injection, EnemyRegistry, HealthComponent, EnemyMotor and death lifecycle. Keep navigation behind the motor; charge movement must remain navigation-aware and stop at obstacles. A charge cannot repeatedly damage the same target during one charge. Pause, death, disable and invalid targets must cancel or suspend attack processing safely, without delayed damage after cancellation.

Use the existing EnemyChestDropProfile and weighted ChestDropTable. Elite content may receive a dedicated table containing advanced chests; chance and weights will be decided with the owner, not inferred from rank. Preserve one death notification, one XP reward and at most one chest-drop attempt. Boss reward wiring follows the boss encounter design.

## Implementation

EnemyChargeState owns timing and one-hit commitment; EnemyAttackController selects Chaser or Charger behavior from the definition. EnemyMotor handles straight navigation-aware displacement. EnemyChargeView owns the warning line, camera-facing label and placeholder tint. Existing EnemySpawner composes both behaviors through its existing initialization route. EnemyDefinition and EnemySpawnRequest reject unknown rank/behavior data before registration or instantiation.

ED_Charger and ED_EliteCharger reuse the existing enemy prefab and the existing explicit drop profiles. The original Common-only elite table is retained pending the advanced rarity balance decision.

## Verification coverage

EditMode: default/invalid rank data, configuration limits, attack transitions, locked direction, cooldown and once-per-charge damage.

PlayMode: target injection, obstacle handling, pause/death/disable cancellation, registry cleanup, normal and elite spawning, exactly-once XP/drop behavior and existing wave/chest regressions.

## Charger implementation contract

Direction locks at the START of wind-up, making the complete warning a dodge opportunity. Scaled time suspends wind-up, movement and recovery during pause. Death, disable, Stop and missing/inactive/dead targets cancel the attack; reactivation requires a fresh wind-up. Damage is swept across the actual movement segment, at most once per charge, including large frame steps. NavMesh boundaries and solid obstacles limit movement; the charger never navigates around an obstacle during the committed attack.

Provisional normal settings: 100 health, 15 damage, 25 XP, activation range 7, wind-up 0.8 s, charge speed 10, distance 8, hit radius 0.7, recovery 1.2 s. Elite example: 200 health, 20 damage, 50 XP, wind-up 0.65 s, speed 12, recovery 1.0 s. Rank does not apply hidden multipliers. Existing normal/elite drop profiles (25%/50%, Common tables) remain the explicit initial assignments; advanced rarity weights require a later balancing decision.

Test_Waves keeps its first wave; its second wave contains one Chaser, one Charger and one Elite Charger. A dedicated presentation component shows rank/color and a ground direction warning without scaling colliders. Boss is metadata only. Evolution gameplay, gold, meta progression and final balance remain subsequent work.

Validation on 2026-09-14: **695/695 EditMode** and **371/371 PlayMode** tests passed in the isolated Unity 6000.3.9f1 project. Coverage includes camera-facing warning presentation, swept damage, lateral evasion, solid obstacles, NavMesh edges, pause/cancellation, normal/elite spawner composition and exactly-once XP/chest drops. The former Chaser-only scene assertion now checks rank-specific drop wiring. A missing generic-collections import in AdvancedChestContentTests from the preceding increment was restored. Manual gameplay acceptance and final balancing remain pending.
