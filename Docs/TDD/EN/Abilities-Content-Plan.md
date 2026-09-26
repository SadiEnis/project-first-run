# Abilities content plan

## Workflow

Git: `content → feature/content/abilities-content`; Plastic: `/main/dev/content → abilities-content`. One branch for all eight abilities. The user creates branches, commits/checks in and merges. Do not create per-ability branches or implement directly on content.

Each ability: discuss mechanics → EN/TR TDD and docs checkpoint → coherent implementation increments → automated EditMode/PlayMode validation → saved arena/reward/F1 integration → stop for user gameplay acceptance. A single branch does not imply one commit per ability: split at meaningful independently verifiable boundaries, not every file change. No automatic commits or merges.

## Order

| Order | Ability | Main scope | Status |
| --- | --- | --- | --- |
| 1 | Fireball | Complete existing content: random ranged targets, 2/3/4 projectiles, burn, GDD levels | Implemented; user confirmed burn, broader level/balance acceptance pending |
| 2 | Force Wave | Frontal short-range area damage and knockback | Planned |
| 3 | Drone | Player-following drone, targeting/fire cadence, second drone, boss rate bonus | Planned |
| 4 | Acid Bottle | Random-target bottles, lasting damage areas, L8 slow | Planned |
| 5 | Lightning Staff | Random strikes, additional strikes, stun, L8 chain | Planned |
| 6 | Shuriken | Two rotations around player, controlled repeat hits, second shuriken, bleed | Planned |
| 7 | Enchanted Staff | Piercing beams with limited lifetime and random direction changes | Planned |
| 8 | Sniper Bomb | Elite/boss priority, homing explosions, second bomb, retarget on target death | Planned |

All have eight base levels. GDD upgrade order is authoritative; numerical balance and unspecified targeting/status rules are discussed before each ability. Reuse validated behavior where suitable, extracting shared code only when a concrete second consumer requires it. Do not couple ability damage to weapon damage. Preserve cross-map registry rebinding and existing run lifecycle.

Evolution gameplay, gold/meta, ammunition sources, final presentation and Windows builds are excluded. Temporary readable feedback is included. Boss-specific rules may use automated rank fixtures where playable boss content is unavailable.

Initial checkpoint: Plasma gameplay acceptance recorded; [Fireball design](Fireball-Content.md) prepared. No new ability implementation or test run at this checkpoint.
