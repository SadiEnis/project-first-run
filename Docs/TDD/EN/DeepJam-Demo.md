# DeepJam demo — scope and scene design

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

The implementation contract, initial trial values and lost-sight movement/attack ownership are defined in [Enemy Perception](Enemy-Perception.md). Preserve the general principles below; initially investigate the last information point and return to idle at the current location after 3 seconds without new information. Not implemented yet.

- Preparation, detection and attack are separate stages. Enemies may already be prepared without automatically chasing or attacking an undetected player.
- Initial visual detection requires all three: detection range, viewing angle relative to enemy facing, and unobstructed line of sight. Proximity through walls/floors alone is insufficient. Range and angle are configurable; choose values through playtesting.
- Detection does not immediately deal damage. Preserve existing attack range, visibility, wind-up and cooldown rules.
- Surviving damage from the player or a player-owned ability/damage-over-time effect alerts the enemy even outside its initial detection cone/range. Unrelated damage does not automatically identify the player. This does not bypass attack visibility/range requirements.
- Leaving the detection boundary does not instantly deactivate the enemy. Settle short pursuit memory and last-seen-position behavior against existing tracking before implementation; do not grant unlimited precise tracking through walls. Duration, pursuit limits and waiting position after forgetting remain open tuning/design details.
- The key ambush can explicitly alert its encounter enemies, without allowing attacks through walls or alerting the whole map.
- Sound perception, patrols, sophisticated search and group alarm propagation are excluded. Do not silently change legacy test scenes' initial behavior; explicitly scope the new perception policy during integration.

Verify range/angle boundaries, targets behind enemies/walls, direct and owned timed-damage alerts, temporary loss of visibility, ambush activation, death/disable cleanup and preserved attack conditions. Manually evaluate overlook safety with the actual geometry; perception tests alone do not validate level design.

## Jumping and local recovery

The first version uses three fixed platforms and a safe landing. Make the first jump wide/teaching-oriented; later jumps demand slightly more attention without requiring sprint. Measure distances with the existing player's jump in greybox. No new puzzle system or tight spiral staircase is required.

Apply damage once per detected fall; the initial tuning value is 10% of maximum health, adjustable after playtesting. Briefly fade and return surviving players to the safe traversal start, clearing falling velocity. Multiple colliders/events for the same fall must not apply repeated damage. Lethal damage invokes ordinary run death, never resurrection through local recovery. Settle armor/incoming-damage modifier interaction before implementation.

This is not a run checkpoint: a survived fall does not reset XP, equipment, key or encounters; dying elsewhere does not respawn here. The lower floor may visually suggest fire/lava, but walking on it, periodic burn damage and climbing back are not part of version one. A collapsing third platform is optional follow-up after fixed-platform acceptance, not part of the initial scope.

Do not require reverse traversal after the key room. On ambush completion a separate exit door opens a short stair/ramp route down to the large arena and final door. Prevent approaching this shortcut from below to bypass the traversal/key: retain the initial closed door and final eligibility checks.

## Key and ambush contract

1. Final door starts locked, communicating its key requirement and the upstairs route.
2. The small arena initially has no visible enemies. Put the key away from the doorway so the player is fully inside when collecting it.
3. Pickup happens once: record run-owned key possession, safely close the entrance used by the player and start one ambush. The separate downward exit remains closed until encounter completion.
4. Enemies enter from occluded entrances or with readable spawn presentation, never silently on top of the player.
5. Wave/enemy counts are open. Two short groups are a starting proposal; tune during greybox testing. No new enemy type is needed.
6. Completion requires all scheduled groups to have spawned and no living enemies belonging to this encounter. A zero count between groups must not open the door; enemies in unrelated rooms do not hold it closed.
7. Completion opens the separate downward exit, not the original entrance, which stays closed. Descend to the large arena and proceed to the final door without reversing the jumping section. Re-entry cannot respawn the key/ambush; no general inventory system is required.

Traversal/gate tests: one damage application per fall, safe recovery pose/velocity, no revival on lethal falls, preserved run progress, entrance closure on pickup, both doors locked during the ambush, only the separate exit opening after the final group, and initial gate-state restoration on restart.
8. Final-sequence entry requires both key possession and ambush completion, including if physical barriers are bypassed. Barriers cannot trap the player. Escaped/unreachable enemies or spawn failure must not cause permanent lockout; design recovery against existing failure/cleanup contracts during implementation.
9. Ordinary death/restart resets the key, encounter, gates, enemies, drops, XP and run build. No permanent meta gain.

## Unletterboxed opening and FPS handoff

Desired presentation: an external camera shows the character emerging from earth, then blends into the real FPS eye pose. No black letterbox bars. Show the HUD when the camera is ready, then grant control. Movement, firing, automatic abilities and player damage are disabled during the opening.

A simple model is sufficient. Keep the existing FPS controller; the external body may be presentation-only. A model does not imply a ready-made emergence animation. Proposed short experiment: visual body, simple rise/straighten motion, dirt/dust and sound. No terrain deformation required.

October 4 inspection: Timeline is present in the manifest, Cinemachine is absent; filename searches found no Robot/StarterAssets or FBX files under Assets. Select compatible packages/models and record licenses/sources before integration; this checkpoint downloads none. Do not promise Cinemachine integration without checking version compatibility.

Match camera position, rotation and FOV and synchronize controller yaw/pitch. Avoid simultaneous camera control owners or AudioListeners. Hide/clamp the presentation mesh as needed to prevent head clipping without disabling the player root. Skipping must reach the same final HUD/control state.

User-approved fallback: FPS opening and ending if external-camera/model work becomes expensive. Proposed experiment budget: half a day; exact model/animation is open. Target a skippable 5–7-second first intro and a 1–2-second retry variant without repeated long cinematics. Keep intro-seen state across scene reloads within the application session; it is neither run progression nor a persistent save.

## Ending

Reveal the giant through silhouette/sound/motion on entry. Enter the ending once; stop player control, automatic attacks and damage. The giant strikes the floor; use sound, brief camera reaction, dust and controlled rubble. Obscure the view or imply a short fall, fade out, then show demo completion, replay and quit.

No physical room-destruction simulation or boss combat AI. FPS presentation is the reliable baseline; an external pullback is optional only if straightforward with the chosen model/sequence. This ending must not trigger ordinary Defeat/restart. Resolve simultaneous death/ending and repeated triggers through one authority; do not overwrite an already-started terminal outcome.

## Early build and acceptance

Build Windows from the first small greybox. Check correct startup scene, input/fire, HUD/rewards, NavMesh/enemies, death/restart, quitting and absence of leaked Editor dependencies. Add key/finale checks when implemented; do not mark unavailable checks passed. Prefer a clean extracted folder and another computer where available.

Final acceptance: no F1 required; first-attempt completion possible; separate optional route; correct key ambush gate; no early opening between waves; no duplicate pickup/re-entry; clean death reset; camera/HUD/control handoff and skip; no boss damage/incorrect restart at the ending; no traversal softlocks; working ending/replay. Author automated tests against these contracts; user executes them. Ammo supply and reward pacing remain to be settled during demo assembly; do not leave reserve exhaustion that blocks completion.
