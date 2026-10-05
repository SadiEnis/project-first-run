# Enemy perception — demo foundation

## Status and scope

Implemented on `feature/deepjam-demo`. On 2026-10-05 the user confirmed wall occlusion, detection on entering view, last-known-position investigation and waiting after memory expiry, and reported green tests after the corrections below. This is user-reported acceptance, not an assistant-executed test run or a full standalone regression claim. The optional profile is connected to the saved opening scene, with spawn points facing the entrance. Room dimensions, navigation bake and chest values remain unchanged. New room encounters follow separately.

Implementation: `EnemyPerceptionProfile` owns settings, `EnemyPerceptionState` owns memory, and `EnemyPerceptionController` samples visibility/damage. The attack controller advances perception before family decisions; the motor updates afterward and cannot read the live target during investigation. Attack commitment also checks current LOS between scheduled sight samples. No new scene-generation step is required.

Validation authored: `EnemyPerceptionStateTests`, `EnemyPerceptionTests`, and additions to `DemoOpeningLayoutTests` / `DemoOpeningTests`. Includes actual Fireball burn/Shuriken bleed ownership, disabled/reinitialized instances, prepared opening enemies, three attack families and legacy opt-out. Tests have not been executed by the assistant. Check partial/unreachable paths and final movement feel in Unity as part of gameplay acceptance.

Manual check: open the existing `DeepJam_Opening` scene. Start remains safe; entering the corridor activates the encounter but each enemy detects independently. Hide behind full-height cover: enemies investigate the last known point and idle after memory expires. Owned damage alerts an unseen enemy without permitting blind attacks. Select a spawned enemy to inspect awareness/memory/gizmos; edit `Scenes/Demo/EP_Demo.asset` for trial settings. No navigation rebake is needed for this checkpoint.

## Existing integration points

### Test correction note — 2026-10-05

The user reported two failures among the 16 `EnemyPerceptionTests`. The arrival test was reacquiring the unobstructed player after a damage alarm; it now moves the player outside tracking range while retaining the old investigation point. The hidden-pursuit/reacquisition test now uses a coroutine to await navigation completion and holds the enemy position fixed during those waits. It verifies both the frozen memory and navigation destination while hidden, then the new destination on reacquisition. These changes are limited to test scenarios; gameplay rules are unchanged. The user subsequently reported the corrected tests green on 2026-10-05.

- `EnemyController.Initialize` starts the motor; `OnEnable` can resume it. `EnemyMotor` periodically reads the live target Transform. Lost-sight pursuit must stop this path from leaking the player's current position.
- `EnemyAttackController` owns Chaser/Charger/Ranger behavior. Ranger retreat uses destination overrides; Charger commits a direction. Perception must not become a competing movement owner.
- Ranger has a LOS check; basic Chaser attacks currently use planar distance. Under the opt-in demo policy, a new melee strike needs unobstructed sight and real 3D attack distance, preventing damage through walls/floors. Do not silently change legacy enemies.
- `HealthComponent.Damaged` provides applied damage and `DamageInfo.Source`, before `Died` on lethal hits. Alarm needs explicit alive/nonlethal checks. Drone/burn/bleed can preserve player ownership through Source; do not infer ownership from projectile names or tags.

## Configuration and compatibility

An optional perception-profile reference on `EnemySpawner` enables the policy. Null preserves existing automatic pursuit/attacks. A separate demo profile avoids globally opting shared enemy prefabs/definitions in. The profile is immutable configuration; awareness and memory belong to each runtime enemy.

Initial trial values, not final balance:

| Setting | Initial value | Meaning |
| --- | --- | --- |
| Acquisition range | 18 m | Real 3D distance |
| Horizontal acquisition cone | 120° total | 60° either side of forward |
| Alert visual tracking range | 24 m | No cone while alerted; range and LOS still required |
| Sight query interval | 0.1 s | Scaled gameplay time; paused with the game |
| Information memory | 3 s | Since last confirmed sight or valid damage |
| Last-point arrival tolerance | max(0.75 m, stoppingDistance + 0.1 m) | Compatible with motor stopping distance |

Expose these values, eye/body offsets and obstruction layer mask in the Inspector. Initially sample approximately 1 m above each root. Ignore self/target colliders and triggers as obstructions; other solid colliders obstruct by default. Zero horizontal separation passes the cone check, but still needs 3D distance and LOS. Visibility over low cover depends on this single body sample; multi-point visibility is outside the first scope.

## State contract

1. **Idle:** once the encounter activates, wait in place without patrol or scanning turns. Acquisition requires range, cone and LOS together. Author spawn facing toward the intended entrance/watch direction.
2. **Visible pursuit:** use the live position for movement decisions only while sight is confirmed. Alert tracking needs LOS and 24 m range but not the initial cone; moving behind an alerted enemy does not instantly erase awareness. Preserve each family's pursuit, distance-keeping and attack-preparation rules.
3. **Investigating last information:** snapshot the last position on lost sight. Navigate toward that snapshot, never update retreat/pursuit from the hidden target Transform. If reached early, wait for the remaining memory. Partial/invalid paths stop at an accessible position; no teleporting or indefinite retries.
4. **Forget:** after 3 s without new information, stop and resume the idle/cone policy at the current location. No spawn return, region leash, healing or respawn in this version. Do not reset health/cooldowns; physical region boundaries remain separate.

## Damage alarm

Only applied, nonlethal damage credited to the assigned player or its hierarchy alerts the enemy. Null/environment/other-enemy sources do not identify the player. Owned projectiles and abilities must preserve the player Source.

An idle enemy first hit by an unseen player captures the player's position once at that event as an investigation point; acquisition range/cone do not prevent the alarm. Alarm alone does not permit new attacks without visual confirmation. Subsequent damage while already alerted refreshes memory but does not continuously update an unseen player's position. Burn/bleed can therefore extend alertness without becoming wall-tracking. Reacquired vision updates the position normally.

## Attack, motor and lifecycle contract

- Perception supplies movement/attack permission and last-known destination, not competing per-frame Stop/Resume/SetDestination writes. Hidden-target Ranger retreat and new Charger windups are prohibited.
- Lost sight cancels unreleased Ranger shots and Charger windups before the charge starts. Already-fired physical projectiles live normally. A committed charge completes its fixed-direction sweep/hit/recovery under existing rules without hidden-target re-aiming. Perception governs movement again after recovery.
- Preserve cooldown and stun. Do not toggle components or reset attack state with Stop every frame as awareness changes. Reacquisition grants no free attack/cooldown reset.
- Prepared encounters must not activate combat through perception/damage during staging. Install the policy before the first enabled movement/attack tick; no one-frame omniscient pursuit.
- Idle active enemies remain registered and eligible for automatic ability targeting. Preparation invulnerability remains owned by the existing encounter lifecycle.
- Stop movement/new attacks on player death or missing target. Clear memory, subscriptions and pending permissions on enemy death, disable and reinitialization. Pool/re-enable must not retain previous awareness; disabling only the perception component must not make an opted-in instance automatically aggressive.
- A future explicit ambush alarm entry point may require an active encounter and still preserve LOS/damage constraints. This stage does not implement keys or ambush gameplay.

## Acceptance and implementation order

1. Pure state/math tests: range/cone boundaries, wider alerted range, memory renewal/expiry, unseen damage and invalid settings.
2. Physics/PlayMode: walls/floors, trigger/self/target filtering, owned damage outside the cone, fixed hidden position during burn/bleed, pause, death and reuse.
3. Chaser/Ranger/Charger integration: no first-frame leak, genuinely fixed last-known position, no new hidden-target attack, committed-charge behavior and preserved cooldowns.
4. Legacy scenes without profiles and prepared-encounter regressions. Opening tests must separate entering the activation volume from being detected.
5. Connect profile and authored facing to the demo opening group; expose range/cone/state/last-known point through Inspector/gizmos. Populate new rooms in a later checkpoint.

The assistant authors tests; the user runs them. Do not report unexecuted tests as passing. Sound, patrols, group alerts, advanced searching and boss AI remain outside scope.
