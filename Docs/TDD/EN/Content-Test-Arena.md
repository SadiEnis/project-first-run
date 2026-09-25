# Content test arena

## Status and purpose

Implementation checkpoint — 24 September 2026. Branches: Plastic `/main/dev/content-test-arena`, Git `feature/content-test-arena`, based on the merged map foundation. The saved scene and test panel are implemented; the user tested the panel and features and accepted the arena for content testing.

Provide repeatable testing of each weapon/ability with the actual player, enemies and progression systems. This is not the final demo map or a new run manager. Pause after each weapon/ability implementation for the user's gameplay evaluation.

## Scene contract

- Target: `Assets/_Project/Scenes/Tests/Test_ContentArena.unity`. Preserve PlayableMapFixture, PlayableMapDestination and Test_Waves. Reuse existing prefabs/services rather than introducing parallel combat/reward systems.
- Author and save floors, boundary walls, near/mid/far distance markers, spawn points, NavMesh and component wiring in the Editor. Play does not generate geometry, service hierarchies or missing Inspector references. Dynamic gameplay objects such as enemies, projectiles, XP and chests still spawn normally at runtime.
- One open combat space and an attached starting/test bay without enemy spawn points are enough. No region passages, dungeons or scene exits are required initially. The starting bay does not grant invulnerability.
- Exactly one player, enemy registry, XP initializer and reward UI/service group. Reuse the map fixture's wiring approach without initializing services twice.
- Assign existing Chaser, Charger and Ranger prefabs/definitions. A small Inspector-configured starting group uses actual targeting, navigation and attacks. Support rebuilding a single-family or mixed group without unbounded stacking.
- Start with the existing player loadout. No free ability, upgrade or automatically opened reward selection. Enemy counts and distances are test data, not balance decisions.

## Real progression and rewards

- Enemy deaths follow the actual damage/death path, including XP and assigned enemy chest-drop profiles. Random chest drops are not expected on every death.
- Existing attraction/collection gathers XP; the current curve raises the player level and produces level-up chests.
- Chest interaction opens the actual reward screen. Preserve new-item acquisition, owned-item level-up, slot limits, maximum level, remaining selections and pause behavior.
- Existing weapon/ability/upgrade and advanced chest definitions can be assigned. Do not reopen gold/evolution scope or add evolution assets or gold fallback.

## Development panel

- A toggleable test tool separate from normal HUD. Opening it releases the cursor and stops movement/firing/ability control; closing respects death, reward UI and previous control state. Its input/pause ownership must not accidentally release another UI's restrictions.
- Do not manage the panel and reward selection simultaneously. Reject test mutations and group replacement while a reward screen is open. Reject item/XP/group commands for dead or uninitialized players.
- Acquire from an explicitly assigned item catalog or increase an owned item's level by one. Use real acquisition/level controllers, not direct build-list or asset mutation. Display full-slot and maximum-level outcomes. Each later content package adds its catalog entry and necessary runtime factory wiring; do not list unimplemented items.
- XP grants use the normal `GainExperience` path; producing a level-up chest is intentional. Spawning a selected existing chest uses the real ChestSpawner, allowing reward testing without waiting for a random drop.
- Replacing a group removes only surviving instances owned by that test group and creates the selected group. Test removal produces no kill/XP/drop rewards. Preserve existing earned XP/chests, player health/ammo and build.
- Display the latest command outcome and instructions. Show readable health, ammo, run level/XP and item levels without unnecessary overlapping debug overlays.
- This is not a full run reset, revival or save/load tool. Stop and restart Editor Play for a fresh test.

## Verification and delivery boundary

1. EditMode: saved scene/prefab/asset references, unique services, enemy definitions, spawn points, baked NavMesh and catalog validity.
2. PlayMode: saved-scene startup; initial loadout and no unearned reward; correct setup and navigation for all three enemy families.
3. Actual enemy death → pickup → XP → level → chest → acquisition/level-up, including selection pause/control behavior.
4. Panel acquisition/level-up, capacity/max-level boundaries, invalid requests and preservation of live runtime state.
5. Group replacement cleans registry/instances without free kill rewards and preserves existing loot/build.
6. Panel/reward UI exclusion, no accidental control restoration after death, and repeated open/close safety.
7. User evaluation of movement/combat, drops, chest selection and direct content controls. Do not begin Shotgun implementation before this acceptance.

This Editor-focused delivery does not resume Windows build investigation or claim standalone acceptance. EditMode/PlayMode results are recorded below.

## Implementation and use

- Open `Test_ContentArena` in the Editor and enter Play. The floor, cover, 10/20/30/40 m markers and NavMesh are saved. `ContentArenaSceneBuilder` rebuilds only this fixture through an explicit Editor menu command, never on Play. Rebuilding replaces manual scene edits.
- Four mixed enemies start: two Chasers, one Charger and one Ranger. The starting bay is not invulnerable. F1 pauses the game and opens the panel; closing restores previous control/time state. Stop and restart Play after death.
- PlasmaRifle is the starting weapon. The existing `WeaponSwitchingDevelopmentBootstrap` provides two weapon slots only for this test player, without changing production defaults or the prefab. Three ability and five upgrade slots remain. Acquire Development Secondary, Fireball and Development Damage Boost through the normal acquisition path; Q switches weapons.
- Commands include Acquire, Level +1, Grant 100 XP, single-family/mixed enemy replacement and seven chest types. Replacement preserves existing loot, health, ammo and build without kill rewards. Failed chest placement reports that nearby space is unavailable.
- Panel XP immediately updates the level; the regular level-up chest source waits until the panel closes and gameplay resumes. Direct chest spawning places a chest while the panel is open. Close it, approach and press E to use the actual reward screen.
- The original arena delivery added no new weapon/ability or evolution content. Following user acceptance, the [Shotgun increment](Shotgun-Content.md) adds the fifth catalog item and pellet trace/HUD wiring, without opening evolution scope.
- The [Minigun increment](Minigun-Content.md) adds the sixth catalog item and preparation/rate/critical HUD feedback. Its separate verification is 805 EditMode / 483 PlayMode passed; user gameplay acceptance is complete, with balance still provisional. Content branches now start from the agreed integration branch `content`, with the package PR deferred until content → main.

## Verification — 24 September 2026

- Unity 6000.3.9f1, isolated project copy, saved scene: **EditMode 753/753**, **PlayMode 457/457** passed.
- Added one authored-scene wiring test and eight gameplay/panel tests. The initial single-weapon-slot wiring was corrected; panel XP chest creation was verified after pause release, matching the normal pipeline.
- Scene/navigation assets and script metadata were copied back from the verified project. User gameplay acceptance is complete; the user will perform commit/check-in and merge. No Windows build was attempted.

## Checkpoints

The [Plasma Rifle increment](Plasma-Rifle-Content.md) upgrades the existing catalog entry to physical piercing projectiles and L8 target burn. F1 shows pierce/burn state. The catalog remains at seven items and the authored starting weapon is preserved; acquire Plasma through F1 if needed. User gameplay acceptance is pending.

Rocket Launcher adds the seventh catalog item, blast/radius/fragment HUD data, and the existing weapon/mixed chest flow. See [Rocket Launcher](Rocket-Launcher-Content.md) for levels and verification. User gameplay acceptance is complete; balance remains provisional. Geometry and lighting are unchanged.

Record design changes as a docs checkpoint, then a coherent implementation/test checkpoint. The content arena PR was merged. Current weapon/ability branches start from the agreed `content` integration branch; the user performs normal weapon merges into content, with a package PR at content → main closure.
