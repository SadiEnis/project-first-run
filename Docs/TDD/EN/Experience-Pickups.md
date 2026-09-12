# Experience pickups

## Scope

This extends [Experience and Level Foundation](Experience-Level-Foundation.md) on the same XP branch. Enemy death produces world XP, walking into a pickup awards it once, and Test_Waves displays run progress. Level-up chest spawning remains the next stage; this layer does not spawn chests.

## Ownership and flow

- `EnemyDefinition.ExperienceReward` is non-negative; zero means no drop. The development chaser grants 25 XP (provisional balance).
- `EnemyExperienceDrop` listens to the initialized enemy's actual `Died` event and creates one pickup above the death position, copying the reward amount. Disable/destruction alone never grants XP. Pooling/reinitialization is not supported yet, matching the current destroy-on-death lifecycle.
- `ExperiencePickup` owns its amount and one-time consumption flag. Its trigger only accepts `PlayerExperienceCollector`. A kinematic Rigidbody supports the player's CharacterController; Ignore Raycast prevents blocking shots or chest interaction.
- `PlayerExperienceCollector` requires initialized XP, active components, living health, and positive time scale. Rejection leaves the pickup available. Trigger stay allows collection when an overlapping player becomes eligible again. No magnet, collection key, timeout, pooling, or cross-scene persistence is introduced.
- Reserve consumption before XP notifications to prevent duplicate/reentrant collection. A failure before XP commits releases the reservation; a throwing observer after commit must not make the pickup collectible again. Observers must still obey the foundation's no-throw contract.
- `ExperienceRunBootstrap` binds a new run state from an explicit serialized definition to the scene's player in Awake. Future run coordination replaces this bootstrap; the controller can still bind an existing state.
- `ExperienceDebugPresenter` is an Editor-only, read-only level/current/required/total XP indicator with a progress bar, consistent with the existing development overlays. It is not final HUD art.

## Composition and validation

Player prefab: XP controller and collector. Chaser prefab: drop adapter referencing the pickup prefab. Test_Waves: run bootstrap and development indicator. Curve asset: 100 initial XP, +50 per subsequent level. Other scenes do not implicitly start an XP run.

Automated coverage: validation, exactly-once awards, unrelated colliders, dead/uninitialized/paused/disabled players, resuming while overlapping, multi-level pickups, observer failure, death versus disable/destruction, and asset/scene wiring. Run full EditMode and PlayMode suites after composition changes.

Manual Test_Waves check: kill a chaser, observe a small cyan orb, walk into it and receive 25 XP. Four orbs reach level 2; the next level costs 150 XP. Chest rewards and weapon switching preserve XP. Death stops collection. Level-up does not yet spawn a chest.
