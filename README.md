# Project First Run

A first-person roguelite arena shooter built with Unity 6. Features modular combat, weapon switching, abilities, and chest-based rewards.

An in-development gameplay prototype focused on explicit state ownership, data-driven content, and testable gameplay rules. Art and presentation currently use placeholders. The design documents also describe planned systems that are not yet implemented.

## Implemented foundations

- First-person movement, aiming, hitscan combat, ammunition, and reloading.
- Weapon acquisition and switching with ammunition state preserved per weapon.
- Player build ownership and capacity, abilities, stats, and upgrades.
- Run-owned item level state: level-one acquisition, configured maximum snapshots, one-step advancement and no extra slot consumption.
- Weapon level effects: snapshotted damage/capacity/fire-rate/reload profiles, preserving existing ammo and remaining timers. The two sample weapons have eight provisional levels, accessible through the runtime controller. Chest level-up rewards/UI and ability/upgrade level effects are not connected yet.
- Enemy spawning, attacks, death handling, and wave progression.
- Reward eligibility, offers, single-use claims, and a modal selection UI.
- World chests: interaction opens a reward offer; claiming the reward consumes the chest.
- Level-up chest source: one chest per gained run level, including multi-level gains, queued until nearby supported and unobstructed placement is available.
- Enemy death chest drops: per-definition probability profiles, independent weighted chest selection, and queued placement near the death position without rerolls or duplicate death rewards.
- Chest tier foundation: validated rarity metadata and shared weighted selection tables, independently configured for level-up, normal and elite sources.
- Common Weapon, Ability and Upgrade chests: category-validated acquisition pools, up to three eligible choices, one claim, and grey labelled placeholder presentation. Item-level rewards and advanced chest types remain planned.
- Run XP and level rules: configurable increasing costs, carry-over XP, multi-level gains, and notifications.
- Enemy death XP drops, configurable range-based attraction, one-time collection, death/pause gating, and a development XP/level indicator in Test_Waves.

## Run the prototype

1. Clone this repository and add its root folder to Unity Hub.
2. Open it with **Unity 6000.3.9f1**, as recorded in `ProjectSettings/ProjectVersion.txt`.
3. Let Unity restore the packages and import the assets.
4. Open `Assets/_Project/Scenes/Tests/Test_Waves.unity` and enter Play mode.
5. The test fixture starts with three labelled chests: Weapon in front, Ability to its left, Upgrade to its right. Look at one and press **E** (gamepad South), then claim its reward to resume. Current samples are Development Secondary, Fireball and Development Damage Boost; neither Fireball nor the upgrade is granted automatically in this scene. Use **Q** (gamepad North) to switch weapons after acquiring the secondary.
6. Kill chasers and approach their cyan XP orbs. Within a 3-metre radius, orbs fly toward you and grant 25 XP on arrival/contact; four orbs reach level 2. Follow level and XP at the bottom center of the screen.
7. Each level-up spawns an additional chest on nearby free ground. Look at it and press E to select a reward; level-up itself does not open the UI. The small development pool can run out of eligible rewards; gold fallback and item-level rewards are not implemented yet.
8. Chasers also have a provisional 25% chance to drop a chest near their death position, independently of XP collection. For a deterministic test, set `CDP_DevelopmentNormal`'s Chance Basis Points to 10000 before Play, then restore 2500. Level-up, normal and elite tables currently select the three Common categories with equal weights; the elite profile still has no spawned elite enemy. After acquiring the sample items, their category pools are exhausted: later chests stay available without opening an empty modal until item-level rewards/fallback are implemented.

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
- [Game design and planned direction](Docs/GDD/GDD_EN.md).

Content configuration is separated from mutable runtime state. Pure C# objects own gameplay rules where practical; Unity components provide scene composition, input, presentation, and lifecycle integration.

## Verification

Run both suites through Unity's Test Runner window. Weapon level effects validation on **2026-09-14** passed **666 EditMode** and **340 PlayMode** tests in an isolated Unity 6000.3.9f1 project. This increment adds 11 EditMode and eight PlayMode cases for configuration validation, active/inactive weapons, damage modifiers, progression snapshots and ammo/timer preservation. All 13 new/changed Unity asset, source, test and meta files matched the test copy by SHA-256. These are recorded automated results, not a live CI badge or gameplay acceptance of level-up rewards, which are not connected yet. The preceding item level state foundation passed 655 EditMode and 332 PlayMode tests.

The preceding Common chest stage passed 626 EditMode / 332 PlayMode tests and its three starter chest types were subsequently accepted in gameplay by the owner. Earlier XP, pickup attraction, level-up chest and enemy death drop stages were also manually validated.

## Development history

The owner verified all three Common starter chest types in gameplay on **2026-09-14** and approved integrating this milestone. Item-level rewards remain planned; the automated results above record the implementation before that gameplay acceptance.

Development started in Unity Version Control (Plastic SCM). GitHub begins with a source snapshot of the chest-foundation milestone, based on **cs:138**. Earlier work remains in Plastic history; subsequent development will be recorded here as meaningful commits referencing the corresponding Plastic changesets.

See [the version-control workflow](Docs/Development/Version-Control.md) for synchronization, repository scope, and large-asset handling.
