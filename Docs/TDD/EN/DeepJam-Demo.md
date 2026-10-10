# DeepJam demo — scope and scene design

## Opening onboarding and gameplay feedback — 10.10.2026

Current policy supersedes the historical opening reward notes below. The four corridor Chasers use the demo-only ED_DemoOpeningChaser definition: unchanged combat stats, no random chest profile and 10 XP each (40 total, instead of the normal 25 each). Their only chest reward is the existing guaranteed Ability Chest after all four die. The reduced XP stays below the first level threshold of 100, preventing an early level-up chest from introducing abilities before this reward. Other enemies, drop tables, XP curves and scenes outside the two demo variants are unchanged. Both Opening and WideCombat share EW_DemoOpening; the creation tool also references the dedicated definition. Do not regenerate either scene.

Planned onboarding presentation: starting weapon explanation during the corridor; acquire the first ability at the guaranteed reward; explain automatic ability use after the reward is actually accepted; then briefly tease discoverable upgrades. Tutorial message UI/timing and device-specific prompts are not implemented by this data change. Skipping the corridor still remains allowed; there is no new mandatory combat gate.

User acceptance feedback: a full run took 2m50s, ending at level 6 with 67/350 XP; 21 chests were opened (1 weapon, 9 ability, 2 upgrade, 4 green, 5 purple). Final build: Minigun L1, Rocket Launcher L4, Sniper Bomb L3, Lightning L8, Fireball L1, four upgrades (three L1, one L2). Minigun ran out at 74s; eight rockets remained at the end. Content-rich loot is provisionally accepted for the demo, not approved as final-game economy. These are observations from one run, not measured probabilities. Chest counts include guaranteed rewards as well as level-up and enemy sources.

A minimal-combat run reached the finale after fighting only the key ambush. The user accepts this freedom and the local mandatory ambush. The balcony exposes at most roughly 6–7 enemies and no longer needs an immediate anti-sniping adjustment. Optional-room challenge, key-room staging and parkour extensions are deferred until after visual/audio work; do not increase enemy counts now.

Current demo capacity remains 2 weapons / 3 abilities / 5 upgrades. Model maxima are 2 / 5 / 8, not evidence of an implemented meta unlock progression. Exposing maximum slots for a content showcase is a proposal, not applied in this increment; more distinct items also spreads upgrades across more items.

Known separate input issue: finale Replay/Quit buttons do not work with gamepad. The current ending menu uses OnGUI buttons with no explicit gamepad selection/submit handling. Track as a pre-delivery controller-accessibility fix, not environment polish; no change in this increment.

Verification: add asset/scene regression checks to DemoOpeningLayoutTests and extend DemoOpeningTests to assert 40 XP after collecting all corridor pickups, no level-up/random chests and exactly one guaranteed reward. The user runs Unity tests and gameplay checks.

## Status and order — 2026-10-04

Git: `feature/deepjam-demo`. The opening slice was committed as `12a2dd0`; the user verified its combat/reward flow and Windows build startup. Full standalone gameplay acceptance and automated test results remain separate. Application deadline is October 11; October 10 is the proposed buffered delivery target. Aim for roughly 8–12 minutes, not a fixed acceptance requirement.

Order: TDD/docs checkpoint → small playable greybox and early Windows build → complete route/game loop → presentation and finishing touches. Do not wait for the finished map or cinematics before the first build. The user runs builds and Unity tests; the assistant authors implementation/tests and instructions. Check-ins and merges remain user-owned.

## First implementation slice — start / corridor / reward

- Unity menu: `Project First Run > Demo > Create Opening Greybox`. Copy the existing `Test_ContentArena` wiring into a separate file without editing the source scene, prefabs or test NavMesh. If the demo already exists, only open it; never overwrite user-authored layout changes.
- Destination: `Assets/_Project/Scenes/Demo/DeepJam_Opening.unity`. Geometry, four spawn points, activation volume and reward anchor are visible and serialized in Edit Mode. Bake navigation during authoring. Play Mode instantiates ordinary enemies/loot, not the entire map.
- This first scale/flow rehearsal is flat: safe start room → screened entrance → wide corridor with cover → first reward room. Descending stairs, the full dungeon and cinematics are not implemented yet.
- Prepare four Chasers before entry; entering the corridor activates the existing encounter. The new view-cone/LOS perception policy is not implemented in this slice; existing enemy behavior remains.
- Clearing this group creates one guaranteed Ability Chest at the authored reward anchor. Existing enemy/level-up drops remain independent and random. This introduces rewards, not a universal kill-all gate on the main route.
- No F1 panel. Temporary health/ammo/XP/objective HUD and crosshair; E opens a chest. Claiming the guaranteed reward marks only this slice complete, not the demo finale or run Victory.
- On death, Enter or Retry cleanly reloads the same scene and resets temporary run progression. Earth-emergence animation is not included. Retry requests while alive are ignored.
- Two starting weapon slots work in standalone builds too; remove the Editor-only switching bootstrap from the demo copy. Retain the existing ability installation service for this slice.
- Windows menu: `Project First Run > Demo > Build Windows Opening`. Build only this scene to `Builds/DeepJamOpening/ProjectFirstRun.exe` without changing the existing Build Settings scene list. Launch the executable manually afterward. Alt+F4 exits during gameplay; the death screen also has Quit.
- Tests: `DemoOpeningLayoutTests`, `DemoOpeningTests`. Missing generated scene explicitly skips acceptance, not passes it. Cover safe start, the actual activation volume, a single guaranteed reward and death cancellation. Automated test results have not been reported. The user verified Windows build creation and startup.
- The user observed three chests in the short opening: guaranteed reward, level-up and random enemy drop. The sources are independent; balance was not changed in response.
- First acceptance: no pursuit before corridor entry; four enemies activate on entry; killing them yields XP and the guaranteed chest; E/claim returns to gameplay; death/retry resets the run. Ammo supply and longer-run balance remain separate demo work.

## Second implementation slice — walkable map greybox

Status: geometry was authored into the existing `DeepJam_Opening.unity`. The user reported that rebaking resolved the navigation-test issue and accepted the room layout provisionally. Small room sizes will be evaluated with encounters later; dimensions were not changed in this checkpoint. This does not claim a full automated test pass or complete standalone gameplay acceptance. No new map/test scene was created. Do not rerun the opening-scene creation menu. Preserve the start, corridor encounter, player/service wiring and chest rules.

### Layout and elevation plan

1. **Corridor end → descent:** add a wide staircase with protected edges. Prefer a readable straight or landing-based flight over a tight spiral. Initial target: approximately 3 m descent and at least 3 m clear passage, validated against the player collider and actual walking.
2. **First reward room:** preserve the existing room's purpose and guaranteed chest; adapt it to the lower landing. Move the reward anchor with the floor so the chest neither floats nor intersects the ground. The layout change does not add a second guaranteed reward.
3. **Second room:** a short connection leads to the future Ranger/cover teaching area. Provide at least two walkable approaches and sight-breaking cover. Do not add a Ranger or any new encounter in this slice.
4. **Large arena overlook:** the second room leads onto a landing approximately 4 m above the arena. Retain player camera control. The floor and three route entrances should read from here. Initial floor target is approximately 30 × 28 m, not a final balance requirement.
5. **Two descents:** stairs on each side of the overlook connect continuously to the arena floor. Use cover to make one approach more protected and the other more exposed. Both flights remain walkable in reverse; no one-way lock in this slice.
6. **Future routes:** reserve a distinct optional-room entrance/volume, an ascending entrance for the mandatory key route and a recognizable final-door location. Clearly temporary physical barriers terminate unfinished key/finale routes. Do not present these as functioning key/lock mechanics or block safe circulation through the main layout.

### Implementation boundaries

- Group geometry by section in the Hierarchy and serialize it in the scene, never generate it on Play. Do not delete/rebuild existing objects wholesale; edit only necessary floors, boundary walls, connections and the reward anchor.
- Stair and doorway dimensions must work with the current CharacterController without jumping, sprinting or teleporting. Choose step height against its configured step offset. Leave no exposed map-edge gaps through which the player can fall out.
- Floors retain the existing ground-layer convention so elevation changes do not break XP/chest placement. Recheck the corridor activation volume and references when moving the reward room; do not duplicate a broad trigger across the expansion.
- Rebake navigation against the changed geometry. Preserve corridor enemy navigation; do not wire new enemy spawn points or encounters into the new sections yet.
- Use simple colors, door frames and temporary labels to distinguish the optional room, key route and finale. No final materials/models, lighting pass, HUD redesign or cinematics.
- Update the opening-only completion HUD text so the additional walkable area does not appear to be beyond the end of the game.
- Range/cone/LOS perception, key ambush, platforming, separate return exit and finale remain outside this implementation checkpoint. Accept the walkable layout first, then connect mechanics in separate increments.

### Acceptance and next step

Initial authored dimensions: 15 steps at 0.2 m descend 3 m after the corridor; 16 × 14 m reward room; 16 × 16 m second room; 30 × 4 m overlook; 30 × 28 m arena floor. Each arena staircase has twenty 0.2 m steps, 4 m tread width and 0.6 m tread depth. The current player step offset is 0.3 m. A 12 × 12 m optional room sits on the left; a short ascending key-route entrance terminates at a temporary barrier on the right; another temporary barrier marks the finale opposite the overlook. Route colors: green optional, yellow key, cyan finale. These areas do not yet contain reward/ambush/key mechanics.

Reopen the scene in Unity, then run `Project First Run > Demo > Rebake Demo Navigation`. This updates only the scene's existing navigation asset and saves the scene; it does not generate geometry or rebuild user-authored layout. Include the resulting `OpeningNavigation.asset` in the next check-in. Until rebaked, the old navigation data does not represent the lower floors; `DemoMapLayoutTests.BakedNavigationReachesNewElevationsAndOptionalRoom` reports that omission as a failure.

Authored acceptance code: `DemoMapLayoutTests` checks hierarchy, reward height, stairs and post-bake route reachability. `DemoOpeningTests.ExpandedMapCanBeWalkedDownAndBackUpWithoutJumpOrTeleport` teleports only for initial placement, then walks the main route and both arena stairs using the existing PlayerMotor. Tests have not been executed; this is not a pass report. Local fileID references and uniqueness in the saved scene were checked textually, not as a substitute for Unity visual/physics validation.

- Preserve start-to-first-reward gameplay and chest counts/probabilities.
- Walk down the first descent, through the intermediate rooms and onto the overlook; descend and return via each arena staircase.
- All three route locations read from the overlook; the optional area is not mistaken for the key route. Cover prevents a single safe firing line over the entire floor; this is not a substitute for enemy perception acceptance.
- Check floor continuity, head clearance, snagging and map escape at every connection. Unfinished boundaries are visible and do not open into voids.
- Extend EditMode scene checks for serialized hierarchy/references and navigation connections, and PlayMode checks for opening-flow regression and key walking connections. The user runs tests; authored tests alone are not a pass.
- After layout acceptance, implement enemy perception and connect new encounters. Do not expand into key/platforming/cinematics before this acceptance.

### Working copies and build output

The desktop checkout is the development workspace; `C:\Project\ProjectFirstRun` is a build-only copy. Check matching commits and required project settings before builds; do not develop the two copies independently. Pre-sync desktop settings remain preserved in the stash and are not restored wholesale during this map stage. Keep `Builds` excluded from Git and Plastic check-ins.

## Third implementation slice — room encounters (2026-10-05)

Perception was accepted and the user reported taking the check-in/commit. The user approved implementation of the distribution below. It is now wired into the saved scene, not final balance. Unity compilation, tests and gameplay acceptance are pending; the assistant did not execute Unity tests.

The scene's `Demo room encounters` root contains four independent encounters; including the opening there are five encounters and 16 enemies. `EW_DemoSecondRoom`, `EW_DemoArenaLeft`, `EW_DemoArenaRight` and `EW_DemoOptionalRoom` are demo-only assets. Preparation volumes begin at z=30 on the descent; activation begins at z=39 for the second room and z=49 for the arena/optional groups, extending over the subsequent route. These are early preparation/activation volumes, not gates or region borders. Perception still governs sight and attack permission. The demo HUD exposes preparation/reward failures.

Geometry and navigation are unchanged; reopen the saved scene without rerunning generation. `DemoMapLayoutTests` checks spawn reachability against the existing bake. `DemoOpeningTests` now covers preparation/activation, re-entry, independent once-only rewards, cancellation and death; its geometry-walking test cancels the new encounters too. Fresh restart behavior and both approach routes still require manual acceptance.

| Area | Initial trial group | Purpose / traversal |
| --- | --- | --- |
| Opening corridor | Existing 4 Chasers | Unchanged, including the guaranteed Ability Chest. |
| First reward room | No new enemies | Reward selection and breathing space; no invisible barrier against pursuing enemies. |
| Second room | 2 Chasers + 1 Ranger | Cover and approaching a Ranger under Chaser pressure; free exit. |
| Large arena | Total 3 Chasers + 2 Rangers + 1 Charger | Two small placement groups supporting different stair approaches. Individual detection, not a simultaneous six-enemy alarm. No kill-all exit requirement. |
| Optional room | 2 Chasers + 1 Charger | Close-range risk/reward with free entry and retreat. One guaranteed Green Chest after clearing only this group. |

### Integration and boundaries

Gameplay feedback (2026-10-05): the user confirmed timely activation and player damage alerting enemies outside acquisition range. All arena enemies could be killed from the balcony; this is not final layout acceptance. Enlarge the arena during the resizing pass and evaluate a higher overlook as an option. Height alone does not prevent safe clearing: reassess blind space below the landing, full-height cover, enemy placement and both stair approaches together. Do not add invisible shot blockers or damage immunity. The user plans manual scene resizing; regeneration must not overwrite these edits. Geometry changes require navigation, spawn/reward anchors and preparation/activation volumes to be rechecked. This feedback does not claim new automated tests passed; dimensions remain unchanged.

- Use the same saved `DeepJam_Opening` scene, explicit room spawn points, separate group definitions and existing `PreparedRegionEncounter`. The two arena groups total six enemies, not six each. Re-entry never respawns them.
- Start preparation in the preceding connection, before enemies become visible. Arena enemies must be active before the overlook offers a shot; avoid visible but invulnerable prepared enemies. Activation is not an alarm: the accepted perception profile governs pursuit and attacks.
- If preparation lags, adjust lead-in distance/timing rather than spawning visibly or adding a kill-all gate to the main route. Report preparation failures explicitly; a failed group is not cleared and grants no completion reward.
- Author facing, Ranger sightlines and Charger space against existing geometry. Overlook entry does not alert everyone, but exposed players may be seen. Enemies leaving a room are not deleted/healed and remain owned by their encounter.
- The optional reward spawns once at its saved room anchor when its own encounter completes. Living enemies elsewhere do not block it. `EncounterChestReward` does not regenerate a removed/opened chest; death or cancellation gives no reward. Preserve enemy/level-up drop tables; add no guaranteed chests to the other new rooms.
- Key route, platforming, key ambush, gate locks and finale are excluded. No new enemy type, elite, ammo source or global balance change. Record ammunition shortages during playtesting for the subsequent resupply decision.

### Acceptance

EditMode: saved group counts, prefab/definition/profile/player/registry dependencies, spawn-point navigation reachability and reward references. PlayMode: preparation versus activation, no re-entry duplication, independent encounter completion, a single optional reward, death/restart cleanup and opening regressions. The user executes tests.

Manual: try both arena stairs separately; reach the key-route entrance without clearing the arena; skip the optional room, then enter and retreat in another attempt; evaluate Ranger cover and Charger dodge space. This checkpoint does not claim full-map or key/finale acceptance.

## Experience and scope

Show manual FPS combat, automatic abilities, chest-driven builds and risk/reward routes. First-attempt completion is allowed; no forced death or hidden first-run gate. Completing the demo need not mean the protagonist wins narratively.

The user's dungeon sketch is the map basis. The earlier energy-facility suggestion is not a confirmed theme. Overall Hell/surface travel direction and final art style remain open; emerging from earth does not settle them.

Prefer one saved scene with Edit Mode-authored geometry and explicit Inspector references. Do not generate the whole map at Play time. Preserve test scenes. New weapons/abilities, Fluid Mechanism, gold/meta, playable evolutions, procedural maps and a full boss system are out of scope.

## Route and room purposes

| Area | Purpose |
| --- | --- |
| Start | Emergence, orientation and control handoff |
| Combat corridor | Starting weapon/Chaser introduction with lateral movement space |
| Descent and first room | Early reward and trying the new power |
| Second room | Ranger/cover relationship and main-arena preparation |
| Large arena | Combine enemy behaviors; expose optional and key routes |
| Separate optional room | Extra risk/reward, not the mandatory key |
| Upstairs entrance room | Short fight on the mandatory key route |
| Enemy-free jumping section | Three fixed platforms and a safe landing; damaging local recovery on a fall |
| Small key arena | Initially empty, quiet room with a lone glowing key; pickup triggers ambush |
| Final room | Key-gated giant reveal and ending, not a boss fight |

The key route is mandatory; the other side room remains optional. Do not impose universal kill-all gates on the main route/large arena. The key ambush is a deliberately gated encounter within the broader free-exploration design.

## Overlook entrance to the large arena

The previous room leads to an upper landing overlooking the arena. Retain player camera control; no mandatory panorama cinematic. Make the final door, stairs to the key route and separate optional-room entrance distinguishable. Allow a brief opportunity to read the space; entrance does not alert every enemy simultaneously.

Stairs descend on both sides. Aim for different approach angles rather than decorative symmetry: one may be more sheltered/close-range and the other more exposed. Choose actual cover positions in greybox. The landing must not enable risk-free clearing of the whole arena; use cover and sightlines to conceal some enemies, not invisible shot blockers.

## Enemy perception and combat engagement

The implementation contract, initial trial values and lost-sight movement/attack ownership are defined in [Enemy Perception](Enemy-Perception.md). Preserve the general principles below; initially investigate the last information point and return to idle at the current location after 3 seconds without new information. The user confirmed perception gameplay and green corrected tests for the opening group on 2026-10-05. The same profile is now wired to the new room groups; their acceptance is pending.

- Preparation, detection and attack are separate stages. Enemies may already be prepared without automatically chasing or attacking an undetected player.
- Initial visual detection requires all three: detection range, viewing angle relative to enemy facing, and unobstructed line of sight. Proximity through walls/floors alone is insufficient. Range and angle are configurable; choose values through playtesting.
- Detection does not immediately deal damage. Preserve existing attack range, visibility, wind-up and cooldown rules.
- Surviving damage from the player or a player-owned ability/damage-over-time effect alerts the enemy even outside its initial detection cone/range. Unrelated damage does not automatically identify the player. This does not bypass attack visibility/range requirements.
- Leaving the detection boundary does not instantly deactivate the enemy. Settle short pursuit memory and last-seen-position behavior against existing tracking before implementation; do not grant unlimited precise tracking through walls. Duration, pursuit limits and waiting position after forgetting remain open tuning/design details.
- The key ambush can explicitly alert its encounter enemies, without allowing attacks through walls or alerting the whole map.
- Sound perception, patrols, sophisticated search and group alarm propagation are excluded. Do not silently change legacy test scenes' initial behavior; explicitly scope the new perception policy during integration.

Verify range/angle boundaries, targets behind enemies/walls, direct and owned timed-damage alerts, temporary loss of visibility, ambush activation, death/disable cleanup and preserved attack conditions. Manually evaluate overlook safety with the actual geometry; perception tests alone do not validate level design.

## Jumping and local recovery

The first version uses three fixed platforms and a safe landing. Make the first jump wide/teaching-oriented; later jumps demand slightly more attention without requiring sprint. The current PlayerMotor only provides movement and gravity: implement jumping first, then measure distances with the actual controller in greybox. No new puzzle system or tight spiral staircase is required.

Apply damage once per detected fall; the initial tuning value is base damage equal to 10% of current maximum health, adjustable after playtesting. Route it through HealthComponent, preserving incoming damage increases/reductions and existing survival rules. Do not subtract health directly or attribute it as a player attack/kill. Briefly fade and return surviving players to the safe traversal start, clearing falling velocity. Multiple colliders/events for the same fall must not apply repeated damage. Lethal damage invokes ordinary run death, never resurrection through local recovery.

This is not a run checkpoint: a survived fall does not reset XP, equipment, key or encounters; dying elsewhere does not respawn here. The lower floor may visually suggest fire/lava, but walking on it, periodic burn damage and climbing back are not part of version one. A collapsing third platform is optional follow-up after fixed-platform acceptance, not part of the initial scope.

Do not require reverse traversal after the key room. On ambush completion a separate exit door opens a short stair/ramp route down to the large arena and final door. Prevent approaching this shortcut from below to bypass the traversal/key: retain the initial closed door and final eligibility checks.

### Fourth implementation increment: jumping and fixed traversal

Implementation status: jumping was accepted in user gameplay and committed. Three fixed platforms, a landing and ParkourRecovery are now wired into the saved demo scene; Unity tests and gameplay acceptance for this new section are pending. The items below define acceptance for the complete increment.

Resolved input conflict: gamepad south is now reserved for jumping; existing chest interaction moves to the unused right shoulder button. Keyboard interaction remains E. The chest interaction test uses the updated binding.

- **Input and motor:** Use Input Actions with Space / gamepad south; update the generated C# wrapper with the asset. A fresh press while grounded initiates one jump; holding cannot automatically repeat. No double jump, dash or replacement movement system. Expose jump height in the Inspector, initially 1.2 m for trial, deriving velocity from existing gravity. Preserve horizontal air movement; ceiling contact cancels upward velocity.
- **Control ownership:** Rewards, pause, death and control locks suppress jumping; queued presses cannot jump on unlock. Releasing the recovery-owned movement/fire lock must not release another system's lock. Do not reset automatic abilities or other encounters; introduce no new invulnerability rule.
- **Fall transaction:** Only the explicitly assigned volume beneath traversal initiates local recovery; do not change map-wide fall behavior. Additional collider entries during recovery cannot start another damage/teleport transaction. Rearm for the next fall after safe return completes and the player leaves the hazard volume. Death during the fade takes priority; recovery cannot restore control or health.
- **Safe pose:** Serialize a return anchor at traversal start, outside the hazard with floor support and headroom. Safely teleport the CharacterController, clear vertical velocity, synchronize root facing and PlayerLook with the starting direction, and clear residual camera recoil. Invalid references/placement must surface as validation errors rather than silently teleporting to world origin.
- **Scene scope:** Add three fixed platforms, landing, fall volume and return anchor along the key route in the existing saved demo scene. Do not regenerate the scene or overwrite user geometry edits. Jumps must work at unupgraded walking speed. Enemy navigation must not create automatic walking paths across gaps. Rebake NavMesh after geometry changes and check existing room routes.
- **Next increment:** Key pickup, ambush, gates and the downward shortcut are separate implementation work. Do not present this landing as a completed demo; clearly label the temporary test endpoint and allow walking/jumping back out. The final contract still avoids reverse traversal after the ambush.

Author tests for the user to run: one grounded press, no airborne/held repeat, clearing input under control locks, ceiling contact; one damage application per fall, maximum-health scaling and incoming damage modifiers, safe pose/velocity/look, lethal fall and death during fade, repeated separate falls, and preserved run progression. Manually check all three jumps at walking speed, safe recovery from every gap, reward/pause overlap and existing-room regressions. This checkpoint runs neither Unity tests nor a Windows build.

### Traversal integration and trial route

- Take the key stairs on the right of the large arena. Start landing: (22.5, -5, 97); platform centers: X=26.5 / 30.5 / 34.5; safe destination: X=39.25. There are four gaps including entry onto the three platforms and exit onto the landing. The first gap is 1.25 m, the others 1.5 m; all top surfaces are Y=-5. Sprint is unnecessary; approach each edge before jumping.
- Geometry, hazard volume and return anchor are editable under the saved scene root `Demo parkour`. The temporary endpoint barrier moved from X=24 to X=41.5. The landing states that the key room is not ready; temporarily jump back to leave.
- Traversal geometry stays outside the enemy NavMeshSurface children; jumping gaps are not enemy walking routes. Because the existing key-entry barrier moved, run `Project First Run → Demo → Rebake Demo Navigation` and save the scene. The assistant has not performed this bake; existing room-path bake tests remain intact.
- A fall applies one HealthComponent damage request, fades out for 0.15 seconds, teleports safely, then fades in for 0.15 seconds. Clear vertical velocity, pitch and camera recoil; face the platforms. Pause stops the recovery clock; death cancels recovery. XP, magazine contents, abilities and encounters are not reset.
- Temporary input blocks are owner-based; PlayerInputReader separates requested control state from temporary blockers. The motor likewise removes only recovery's own suspension. Reward/death locks are not overwritten; the development panel and scene travel do not capture a temporarily blocked input map as a permanent preference.
- The anchor must be outside the hazard, clear for the player capsule and within 0.35 m of supporting floor. Validate at startup and before teleport. Invalid references/placement show a configuration error rather than teleporting to origin or repeatedly damaging the player. This initial version requires unit player scale.
- New coverage: `DemoMapLayoutTests.ParkourHasThreeFixedPlatformsAndExplicitRecoveryReferences` and seven `Parkour...` tests in `DemoOpeningTests`. They cover walking-speed completion of all four gaps, damage, separate falls, preserved state, death/pause/control locks and an unsafe anchor. Tests are authored, not claimed executed or passed.

## Key and ambush contract

1. Final door starts locked, communicating its key requirement and the upstairs route.
2. The small arena initially has no visible enemies. Put the key away from the doorway so the player is fully inside when collecting it.
3. Pickup happens once: record run-owned key possession, safely close the entrance used by the player and start one ambush. The separate downward exit remains closed until encounter completion.
4. Enemies enter from occluded entrances or with readable spawn presentation, never silently on top of the player.
5. Initially use two groups: 3 Chasers, followed by 2 Chasers + 1 Ranger. Start the second only after the first is fully defeated. These are initial tuning values subject to gameplay acceptance; no new enemy type is added.
6. Completion requires all scheduled groups to have spawned and no living enemies belonging to this encounter. A zero count between groups must not open the door; enemies in unrelated rooms do not hold it closed.
7. Completion opens the separate downward exit, not the original entrance, which stays closed. Descend to the large arena and proceed to the final door without reversing the jumping section. Re-entry cannot respawn the key/ambush; no general inventory system is required.

Traversal/gate tests: one damage application per fall, safe recovery pose/velocity, no revival on lethal falls, preserved run progress, entrance closure on pickup, both doors locked during the ambush, only the separate exit opening after the final group, and initial gate-state restoration on restart.
8. Final-sequence entry requires both key possession and ambush completion, including if physical barriers are bypassed. Barriers cannot trap the player. Escaped/unreachable enemies or spawn failure must not cause permanent lockout; design recovery against existing failure/cleanup contracts during implementation.
9. Ordinary death/restart resets the key, encounter, gates, enemies, drops, XP and run build. No permanent meta gain.

### Fifth implementation increment: key room and two-group ambush

Implementation status: KeyAmbushSession and DemoKeyAmbushController are wired into the saved DeepJam_Opening scene. Added the room, occluded spawn compartment, two independent group assets, entrance/exit gates and separate downward route. After rebaking navigation, the user reported successful Unity tests and gameplay acceptance for both groups and the return exit. The user also verified jumping, 10% base fall damage and safe recovery in the previous traversal increment.

Navigation guard follow-up: before either group begins, every prepared enemy must have an active, enabled NavMeshAgent bound to the NavMesh, regardless of distance from the player. Missing navigation fails the encounter with a rebake/save instruction, cleans up both groups and leaves the exit locked; it never grants completion. This avoids position-dependent success with stationary enemies. Two additional PlayMode regressions cover removed scene navigation with distant spawns and a disabled agent in group two. These new tests await user execution; the earlier acceptance does not cover this follow-up. This guard validates agent binding, not all possible path connectivity; baked-room paths remain covered by the layout tests.

Foundation coverage: KeyAmbushSessionTests cover safe/duplicate pickup, ordering, intermission timing, failure, death and fresh-run state. Two shared world-interaction consumption tests were added to PlayerJumpTests; key and chest cannot consume the same press twice. DemoMapLayoutTests adds reference/composition/baked-room-path coverage; DemoOpeningTests adds five tests for sequential groups/gates, pickup guards, death, encounter failure and the walkable return route. These tests are authored, not executed in Unity.

Scene trial: reopen the updated scene, run `Project First Run → Demo → Rebake Demo Navigation` and save; do not regenerate the scene. Cross the platforms into the key room, look at the central yellow/lit object and collect with E / RB-R1. Clear 3 Chasers, then 2 Chasers + 1 Ranger. The yellow north exit opens; descend and follow the outer corridor to the rear-right side of the large arena. The original traversal entrance remains closed. Final eligibility is exposed as a condition; the giant/finale sequence and opening the final door are not implemented in this scene yet.

Layout: room X=41.5–61.5 / Z=87–107 at Y=-5; key at (49,-3.6,97). Return stairs run along (49,107–113), descending to Y=-7; the corridor follows (49,115) → (19,115) → (19,105) → large arena. Only the connecting opening was cut in the arena's rear-right wall; arena dimensions are unchanged. Gates are closed during baking: enemies have no walking route into traversal or the locked exit; the player uses physical stairs after the exit opens.

- **Scene:** Extend beyond the temporary traversal-end barrier with the key room. Preserve the existing authored scene; do not regenerate it. Wire the entrance, key, both groups' spawn points and separate downward exit through Inspector references. This increment adds neither a new pre-traversal fight nor a puzzle.
- **Pickup:** Use a glowing placeholder key and short interaction prompt. Look at it and press E / gamepad RB-R1 within an initial 2 m range. Prevent collection through walls or from the doorway; require the player fully inside the room and an empty entrance closure volume. Pause, rewards, death and temporary input locks suppress pickup. Collect once; the pickup input cannot also perform another world interaction in the same frame.
- **Ownership and gates:** A demo flow controller owns key possession and ambush phase; no general inventory or save system. Initially the entrance is open and the downward exit closed. Safe pickup hides the key, closes the entrance and starts the ambush. Never enable a door collider inside the player; if entrance closure is obstructed, do not consume the key or start combat.
- **Preparation and presentation:** Reuse PreparedRegionEncounter and enemy tracking, sequencing two separate groups through a demo ambush controller. Prepared enemies may already render, so place spawn pockets out of sight. No silent appearances beside the player. Preparation, activation and awareness remain separate; alert ambush enemies once using the player's position at activation, then retain existing sight/memory/attack rules.
- **Sequence:** After the first group's Victory, allow a short 1-second scaled-time interval. Do not activate group two until ready; both gates remain closed while waiting for preparation. Finishing group one, zero enemies between groups or clearing unrelated rooms does not complete the ambush. Death/cancellation/failure is not Victory. Duplicate events cannot start group two again.
- **Exit and finale:** Completing group two opens only the downward exit; the entrance stays closed. Stairs/ramp return physically to the large arena, without teleporting or allowing the key route to be bypassed by entering from below initially. Final eligibility requires `key collected AND ambush complete`. The giant/finale cinematic is a later increment; do not present this step as the finished demo.
- **Resources:** Preserve existing enemy XP and chest drop rules. Add no extra random reward, guaranteed chest or gold for the key. Reward modals also pause the inter-group interval. Key/ambush/gates belong to the run and reset on ordinary death/restart.
- **Failures:** Spawn/activation failure or a missing living enemy must not silently become success. Try returning an escaped enemy to a safe baked point inside the room; if impossible, surface encounter failure. Failure grants no final eligibility, cleans up the encounter and offers an explicit run restart. No timeout kills enemies or opens the gate simply because combat took too long.

Coverage: pickup range/visibility and gate clearance; duplicate pickup/events; first-then-second ordering; locked exit between groups; unrelated enemies; ambush alert without attacks through walls; final eligibility requiring both key and completion; pause, death, failure and restart cleanup. Scene tests verify gate/group/key/path references; gameplay checks spawn presentation and shortcut bypass prevention. Author tests during implementation; the user runs Unity tests. Rebake navigation after geometry changes.

Navigation regression follow-up (2026-10-08): reproduced the missing-navigation test failure in an isolated project copy. Dormant agents retained `isOnNavMesh` after surface data removal, so that flag alone did not establish missing navigation. Runtime validation now also samples current navigation at the spawn using the agent type and area mask. The additive-scene test temporarily removes registered surfaces, verifies missing data directly and restores surfaces in `finally`. The targeted `KeyAmbushMissingNavigationFailsEvenWhenPlayerIsFarFromSpawns` PlayMode rerun passed **1/1**; no full suite or second regression was run in this follow-up. Saved scene geometry and baked navigation were unchanged.

## Unletterboxed opening and FPS handoff

Desired presentation: an external camera shows the character emerging from earth, then blends into the real FPS eye pose. No black letterbox bars. Show the HUD when the camera is ready, then grant control. Movement, firing, automatic abilities and player damage are disabled during the opening.

A simple model is sufficient. Keep the existing FPS controller; the external body may be presentation-only. A model does not imply a ready-made emergence animation. Proposed short experiment: visual body, simple rise/straighten motion, dirt/dust and sound. No terrain deformation required.

October 4 inspection: Timeline is present in the manifest, Cinemachine is absent; filename searches found no Robot/StarterAssets or FBX files under Assets. Select compatible packages/models and record licenses/sources before integration; this checkpoint downloads none. Do not promise Cinemachine integration without checking version compatibility.

Match camera position, rotation and FOV and synchronize controller yaw/pitch. Avoid simultaneous camera control owners or AudioListeners. Hide/clamp the presentation mesh as needed to prevent head clipping without disabling the player root. Skipping must reach the same final HUD/control state.

User-approved fallback: FPS opening and ending if external-camera/model work becomes expensive. Proposed experiment budget: half a day; exact model/animation is open. Target a skippable 5–7-second first intro and a 1–2-second retry variant without repeated long cinematics. Keep intro-seen state across scene reloads within the application session; it is neither run progression nor a persistent save.

## Ending

Reveal the giant through silhouette/sound/motion on entry. Enter the ending once; stop player control, automatic attacks and damage. The giant strikes the floor; use sound, brief camera reaction, dust and controlled rubble. Obscure the view or imply a short fall, fade out, then show demo completion, replay and quit.

No physical room-destruction simulation or boss combat AI. FPS presentation is the reliable baseline; an external pullback is optional only if straightforward with the chosen model/sequence. This ending must not trigger ordinary Defeat/restart. Resolve simultaneous death/ending and repeated triggers through one authority; do not overwrite an already-started terminal outcome.

### Sixth implementation increment: final gate and demo ending

Status: the user accepted and committed the first sub-increment's gate and ending-entry gameplay. The second sub-increment's visual sequence and completion screen are implemented; Unity tests and gameplay acceptance for these additions are pending. The key ambush and return route are unchanged. Only the final room was extended to Z=109–125 for presentation space, retaining 6 m width with 10 m walls. The large arena and earlier rooms were not resized.

Current presentation: DemoEndingTimeline uses unscaled time while gameplay is frozen: 1.8 s reveal/camera orientation → 1.2 s arm windup → 0.35 s strike → 1.2 s aftermath → 1 s fade (5.55 s total). Timings and camera response are Inspector-configurable. DemoEndingPresentation animates the scene-authored block giant's arm and three collider-free rubble pieces, with restrained camera shake and a short dust-colored screen wash. This is greybox presentation: no new model, sound, particle package or destruction physics. Keep the FPS camera; no second camera/AudioListener or letterbox bars.

Outcome: the temporary “Final entry reached” screen is removed. After fully fading, enter Completed and show “DEMO COMPLETE”, Play again and Quit with an unlocked cursor. Accept replay only once from Completed; if scene loading cannot start, show the error and allow retry. Reload the same scene as a fresh run without checkpoints/persistence. Existing owner-based input/motor/weapon/ability/damage locks remain throughout. On disable, restore the camera pose and release only this controller's locks.

New coverage: alongside the prior entry tests, DemoEndingTimelineTests covers timing, one impact, large frame steps, fading and invalid durations. DemoFinalSessionTests covers single replay-request ownership. DemoMapLayoutTests checks presentation references and collider/enemy-free visuals. DemoOpeningTests covers completion at Time.timeScale=0, one impact, full fade, damage protection, cursor and camera restoration. Actual scene reload is not executed in that test to avoid destroying the runner's additive fixture; verify Play again and Quit manually/in a build. Unity tests were not run for this increment; static diff, new scene fileID references and collider-free visuals were checked.

Gameplay trial: reopen the updated scene, run Demo → Rebake Demo Navigation after extending the final room and save; do not regenerate the scene. Without the key the gate stays closed. Collect the key, clear both groups, return to the arena and enter the final gate. Control/HUD stop, the camera faces the giant, the arm strikes, rubble/camera react, and the view fades to the completion screen. Also enter facing backwards. Play again must start a clean run with reset key/gate state. Remaining arena enemies must not prevent the ending. Windows build verification for this increment is still pending.

- **Gate condition:** Replace the temporary final barrier with a gate that opens for a living player when `key collected AND both ambush groups completed`. Clearing the large arena or optional room is not required. Do not consume the key. Opening the gate does not start the ending.
- **Entry:** Recheck eligibility when the player enters the interior trigger beyond the gate; begin exactly once. Enemy/projectile colliders, repeat entry, a dead player, an open reward modal or pause cannot start the ending. Do not trap the player in the doorway; closing the entrance behind them is unnecessary for the first version.
- **Outcome ownership:** Flow is `Playing → Ending → Completed` or `Playing → Dead`. An accepted terminal flow cannot switch to the other. If already dead at final acceptance, death wins; after Ending begins, further damage cannot kill the player. DemoOpeningController death/retry presentation must not compete with the ending screen.
- **Control and combat:** Ending stops movement, look, firing, reload, interaction and automatic abilities; existing projectiles and damage-over-time cannot hurt the player. Blocking input alone is insufficient. Hide gameplay HUD. Locks respect existing ownership and never release another system's lock. Missing required references produce an actionable initialization error rather than stranding the player mid-sequence.
- **Initial presentation:** Use the FPS camera baseline without downloading new models/packages or requiring Cinemachine. Author an editable simple giant silhouette and presentation anchors in the scene. Brief reveal/motion, ground strike, restrained camera response and dust/rubble impression lead into a fade. No destruction simulation, boss AI, boss health bar or boss reward. Expose timing through the Inspector; no letterbox bars.
- **Completion and replay:** After fading, show demo completion, Replay and Quit with an unlocked cursor. Replay reloads the same scene once as a fresh run, resetting health, XP, inventory, key, groups and gates. Quit closes the player build. Time scale and temporary locks must not prevent a clean restart.
- **Scope boundary:** Do not change the opening cinematic, arena dimensions, balcony advantage, ammo supply or reward balance here. Preserve user-authored layout; never regenerate the whole scene. Rebake navigation if geometry changes.

Implementation order: (1) gate eligibility, one-shot ending entry and death/ending separation; (2) simple FPS ending presentation, completion screen and replay. These are meaningful intermediate checkpoints, not one commit per small edit.

Test contract: gate remains closed without a key and with a key alone; opens after both groups; unrelated living enemies do not block it; player-only one-shot entry; pause/reward/death guards; damage and automatic attacks suppressed after entry; terminal outcome precedence; completion UI and duplicate restart requests causing only one load. Author tests; the user normally executes Unity tests. After both increments are integrated, also check the full route and replay in a Windows build. This document claims no test or build success.

## Early build and acceptance

Build Windows from the first small greybox. Check correct startup scene, input/fire, HUD/rewards, NavMesh/enemies, death/restart, quitting and absence of leaked Editor dependencies. Add key/finale checks when implemented; do not mark unavailable checks passed. Prefer a clean extracted folder and another computer where available.

Final acceptance: no F1 required; first-attempt completion possible; separate optional route; correct key ambush gate; no early opening between waves; no duplicate pickup/re-entry; clean death reset; camera/HUD/control handoff and skip; no boss damage/incorrect restart at the ending; no traversal softlocks; working ending/replay. Author automated tests against these contracts; user executes them. Ammo supply and reward pacing remain to be settled during demo assembly; do not leave reserve exhaustion that blocks completion.
