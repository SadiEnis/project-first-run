# Project First Run

A first-person roguelite arena shooter built with Unity 6. Features modular combat, weapon switching, abilities, and chest-based rewards.

An in-development gameplay prototype focused on explicit state ownership, data-driven content, and testable gameplay rules. Art and presentation currently use placeholders. The design documents also describe planned systems that are not yet implemented.

## Implemented foundations

- First-person movement, aiming, hitscan combat, ammunition, and reloading.
- Weapon acquisition and switching with ammunition state preserved per weapon.
- Player build ownership and capacity, abilities, stats, and upgrades.
- Enemy spawning, attacks, death handling, and wave progression.
- Reward eligibility, offers, single-use claims, and a modal selection UI.
- World chests: interaction opens a reward offer; claiming the reward consumes the chest.
- Run XP and level rules: configurable increasing costs, carry-over XP, multi-level gains, and notifications.
- Enemy death XP drops, one-time proximity collection, death/pause gating, and a development XP/level indicator in Test_Waves. Level-up chest spawning is not connected yet.

## Run the prototype

1. Clone this repository and add its root folder to Unity Hub.
2. Open it with **Unity 6000.3.9f1**, as recorded in `ProjectSettings/ProjectVersion.txt`.
3. Let Unity restore the packages and import the assets.
4. Open `Assets/_Project/Scenes/Tests/Test_Waves.unity` and enter Play mode.
5. Look at the chest in front of the player and press **E** (gamepad South) to open reward selection. Claim a reward to resume gameplay. Use **Q** (gamepad North) to switch weapons after acquiring a second weapon.
6. Kill chasers and walk into their cyan XP orbs. Each grants 25 XP; four orbs reach level 2. Follow level and XP at the bottom center of the screen.

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
- [Game design and planned direction](Docs/GDD/GDD_EN.md).

Content configuration is separated from mutable runtime state. Pure C# objects own gameplay rules where practical; Unity components provide scene composition, input, presentation, and lifecycle integration.

## Verification

Run both suites through Unity's Test Runner window. XP pickup validation on **2026-09-12** passed **556 EditMode** and **260 PlayMode** tests in an isolated Unity 6000.3.9f1 project. This includes 29 new pickup/composition/regression tests. These are recorded automated results, not a live CI badge or a manual visual playtest.

## Development history

Development started in Unity Version Control (Plastic SCM). GitHub begins with a source snapshot of the chest-foundation milestone, based on **cs:138**. Earlier work remains in Plastic history; subsequent development will be recorded here as meaningful commits referencing the corresponding Plastic changesets.

See [the version-control workflow](Docs/Development/Version-Control.md) for synchronization, repository scope, and large-asset handling.
