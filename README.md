# Project First Run

A first-person roguelite arena shooter built with Unity 6. Features modular combat, weapon switching, abilities, and chest-based rewards.

An in-development gameplay prototype focused on explicit state ownership, data-driven content, and testable gameplay rules. Art and presentation currently use placeholders. The design documents also describe planned systems that are not yet implemented.

## Implemented foundations

- First-person movement, aiming, hitscan combat, ammunition, and reloading.
- Weapon acquisition and switching with ammunition state preserved per weapon.
- Player build ownership and capacity, abilities, stats, and upgrades.
- Run-owned item level state: level-one acquisition, configured maximum snapshots, one-step advancement and no extra slot consumption.
- Weapon level effects: snapshotted damage/capacity/fire-rate/reload profiles, preserving existing ammo and remaining timers. The two sample weapons have eight provisional levels, accessible through the runtime controller.
- Upgrade level effects: complete snapshotted modifier sets replace previous effects without stacking or consuming extra slots. Development Damage Boost uses three provisional levels (+20%, +30%, +40%).
- Fireball level effects: eight provisional snapshotted levels apply damage, cooldown and projectile speed through the existing automatic runtime. Projectile count and burn/evolution behavior remain separate follow-up work.
- Reward level-up choices: owned non-max items can appear as `LEVEL UP` choices; maxed items stay filtered, and claiming upgrades the existing runtime without consuming another slot.
- Evolution foundation: optional definitions can atomically replace a max-level owned item with an unowned same-category result in the same slot. No real evolution item assets or gameplay replacement behavior are connected yet.
- Enemy spawning, attacks, death handling, and wave progression.
- Enemy rank and behavior separation; normal/elite Chargers with direction-locked wind-up, obstacle-limited charge, one-hit sweep, recovery, and a visible warning.
- Reward eligibility, offers, single-use claims, and a modal selection UI.
- World chests: interaction opens a reward offer; claiming the reward consumes the chest.
- Level-up chest source: one chest per gained run level, including multi-level gains, queued until nearby supported and unobstructed placement is available.
- Enemy death chest drops: per-definition probability profiles, independent weighted chest selection, and queued placement near the death position without rerolls or duplicate death rewards.
- Chest tier foundation: validated rarity metadata and shared weighted selection tables, independently configured for level-up, normal and elite sources.
- Common Weapon, Ability and Upgrade chests: category-validated acquisition pools, up to three eligible choices, one claim, grey labelled placeholder presentation, and `NEW` / `LEVEL UP` choices.
- Advanced chest types: Green/Uncommon, Purple-working-name/Rare, Legendary and Boss definitions with mixed reward pools, explicit offer/claim counts, and multi-claim sessions for Legendary/Boss chests. Evolution and gold policies remain separate.
- Run XP and level rules: configurable increasing costs, carry-over XP, multi-level gains, and notifications.
- Enemy death XP drops, configurable range-based attraction, one-time collection, death/pause gating, and a development XP/level indicator in Test_Waves.

## Run the prototype

1. Clone this repository and add its root folder to Unity Hub.
2. Open it with **Unity 6000.3.9f1**, as recorded in `ProjectSettings/ProjectVersion.txt`.
3. Let Unity restore the packages and import the assets.
4. Open `Assets/_Project/Scenes/Tests/Test_Waves.unity` and enter Play mode.
5. The test fixture starts with three Common labelled chests (Weapon in front, Ability to its left, Upgrade to its right) and four advanced mixed chests for policy testing. Look at one and press **E** (gamepad South), then claim its reward to resume. Green/Purple close after one claim; Legendary/Boss require two claims when enough eligible choices exist. Current samples are Development Secondary, Fireball and Development Damage Boost; neither Fireball nor the upgrade is granted automatically in this scene. Use **Q** (gamepad North) to switch weapons after acquiring the secondary.
6. Kill chasers and approach their cyan XP orbs. Within a 3-metre radius, orbs fly toward you and grant 25 XP on arrival/contact; four orbs reach level 2. Follow level and XP at the bottom center of the screen.
7. Each level-up spawns an additional chest on nearby free ground. Look at it and press E to select a reward; level-up itself does not open the UI. The small development pool can offer either new items or level-up choices.
8. Normal enemies have a provisional 25% chest-drop chance; Elite Charger uses the existing 50% elite profile. Level-up, normal and elite tables currently select the three Common categories with equal weights. After acquiring a sample item, later chests can offer its next level until it reaches maximum.
9. Wave 2 mixes one Chaser, one orange Charger and one purple Elite Charger. A yellow ground line warns of the locked charge direction; move sideways during the warning. Charger grants 25 XP and Elite Charger 50 XP. Attack and loot values are provisional.

The chest bootstrap is an Editor-only development fixture. This scene demonstrates the gameplay foundations; it is not a packaged game or a complete run progression flow.

## Code and design

- `Assets/_Project/Scripts/`: gameplay rules, runtime controllers, UI, and development fixtures.
- `Assets/_Project/Data/`: ScriptableObject content definitions.
- `Assets/_Project/Prefabs/` and `Assets/_Project/Scenes/Tests/`: reusable composition and test scenes.
- `Assets/_Project/Tests/`: EditMode and PlayMode test suites.
- [English technical design](Docs/TDD/EN/README.md) / [Turkish technical design](Docs/TDD/TR/README.md).
- [Chest foundation](Docs/TDD/EN/Chest-Foundation.md) and [reward selection UI](Docs/TDD/EN/Reward-Selection-UI.md).
- [Experience and level foundation](Docs/TDD/EN/Experience-Level-Foundation.md).
- [Experience drops and collection](Docs/TDD/EN/Experience-Pickups.md).
- [Level-up chest source](Docs/TDD/EN/Level-Up-Chest-Source.md).
- [Enemy chest drop profiles and death source](Docs/TDD/EN/Enemy-Chest-Drops.md).
- [Chest rarity and shared-table foundation](Docs/TDD/EN/Chest-Tier-Foundation.md).
- [Common category chests and sample acquisitions](Docs/TDD/EN/Common-Chest-Types.md).
- [Item level state foundation and remaining integration](Docs/TDD/EN/Item-Level-Foundation.md).
- [Weapon level effects and runtime preservation](Docs/TDD/EN/Weapon-Level-Effects.md).
- [Upgrade level effects and modifier replacement](Docs/TDD/EN/Upgrade-Level-Effects.md).
- [Fireball level effects and runtime snapshot](Docs/TDD/EN/Fireball-Level-Effects.md).
- [Reward level-up choices and claim routing](Docs/TDD/EN/Reward-Level-Up-Choices.md).
- [Evolution foundation and same-slot replacement](Docs/TDD/EN/Evolution-Foundation.md).
- [Advanced chest types and multi-claim policies](Docs/TDD/EN/Advanced-Chest-Types.md).
- [Enemy variety, Charger and elite foundation](Docs/TDD/EN/Enemy-Variety-Foundation.md).
- [Game design and planned direction](Docs/GDD/GDD_EN.md).

Content configuration is separated from mutable runtime state. Pure C# objects own gameplay rules where practical; Unity components provide scene composition, input, presentation, and lifecycle integration.

## Verification

Charger foundation validation on **2026-09-14** passed **695 EditMode / 371 PlayMode** tests in the isolated Unity 6000.3.9f1 project, including direction locking, swept one-hit damage, obstacle/NavMesh limits, cancellation, warning presentation and normal/elite death rewards. Manual gameplay acceptance and balance tuning remain pending.

Run both suites through Unity's Test Runner window. Fireball level effects validation on **2026-09-14** passed **666 EditMode** and **351 PlayMode** tests in an isolated Unity 6000.3.9f1 project. Three new PlayMode cases cover cooldown progression, runtime snapshots, alias/duplicate boundaries and single-entry ownership. These are recorded automated results, not a live CI badge; manual gameplay acceptance of the `NEW` / `LEVEL UP` reward flow was also completed on this date. The preceding upgrade level effects passed 666 EditMode / 348 PlayMode; weapon level effects passed 666 EditMode / 340 PlayMode; item level state foundation passed 655 EditMode / 332 PlayMode.

The preceding Common chest stage passed 626 EditMode / 332 PlayMode tests and its three starter chest types were subsequently accepted in gameplay by the owner. Earlier XP, pickup attraction, level-up chest and enemy death drop stages were also manually validated.

Advanced chest types validation on **2026-09-14** passed **681 EditMode / 355 PlayMode** tests in the isolated Unity 6000.3.9f1 project. This includes mixed Uncommon/Rare/Legendary/Boss definitions, the six-choice UI capacity, multi-claim sessions, duplicate-choice boundaries, and the Test_Waves showcase wiring.

Ranger ranged enemy foundation validation on **2026-09-14** passed **700 EditMode / 378 PlayMode** tests in the isolated Unity 6000.3.9f1 project. This includes distance-band movement, navigation-valid retreat, line-of-sight gating, wind-up/cooldown timing, straight one-hit projectiles, wall occlusion, pause/cancellation cleanup and existing enemy/drop/wave regressions. Manual Ranger gameplay acceptance and balance tuning remain pending.

Arena and run flow foundation validation on **2026-09-14** passed **700 EditMode / 381 PlayMode** tests in the isolated Unity 6000.3.9f1 project. This includes final-wave victory, player-death defeat cleanup, terminal-result protection and fresh restart behavior.

## Development history

The owner verified all three Common starter chest types and the item-level `NEW` / `LEVEL UP` reward flow in gameplay on **2026-09-14** and approved integrating this milestone.

Development started in Unity Version Control (Plastic SCM). GitHub begins with a source snapshot of the chest-foundation milestone, based on **cs:138**. Earlier work remains in Plastic history; subsequent development will be recorded here as meaningful commits referencing the corresponding Plastic changesets.

See [the version-control workflow](Docs/Development/Version-Control.md) for synchronization, repository scope, and large-asset handling.
