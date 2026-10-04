# DeepJam demo — scope and scene design

## Status and order — 2026-10-04

Git: `feature/deepjam-demo`. The user created the demo branches. This checkpoint is documentation only: no scene, model, package or runtime change. Application deadline is October 11; October 10 is the proposed buffered delivery target. Aim for roughly 8–12 minutes, not a fixed acceptance requirement.

Order: TDD/docs checkpoint → small playable greybox and early Windows build → complete route/game loop → presentation and finishing touches. Do not wait for the finished map or cinematics before the first build. The user runs builds and Unity tests; the assistant authors implementation/tests and instructions. Check-ins and merges remain user-owned.

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
