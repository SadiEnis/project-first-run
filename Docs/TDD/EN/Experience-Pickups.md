# Experience pickups

## Scope

This extends [Experience and Level Foundation](Experience-Level-Foundation.md) on the same XP branch. Enemy death produces world XP, walking into a pickup awards it once, and Test_Waves displays run progress. Level-up chest spawning remains the next stage; this layer does not spawn chests.

## Ownership and flow

- `EnemyDefinition.ExperienceReward` is non-negative; zero means no drop. The development chaser grants 25 XP (provisional balance).
- `EnemyExperienceDrop` listens to the initialized enemy's actual `Died` event and creates one pickup above the death position, copying the reward amount. Disable/destruction alone never grants XP. Pooling/reinitialization is not supported yet, matching the current destroy-on-death lifecycle.
- `ExperiencePickup` owns its amount and one-time consumption flag. Its trigger only accepts `PlayerExperienceCollector`. A kinematic Rigidbody supports the player's CharacterController; Ignore Raycast prevents blocking shots or chest interaction.
- `PlayerExperienceCollector` requires initialized XP, active components, living health, and positive time scale. Rejection leaves the pickup available. Trigger stay allows collection when an overlapping player becomes eligible again. No collection key, timeout, pooling, or cross-scene persistence is introduced.
- Reserve consumption before XP notifications to prevent duplicate/reentrant collection. A failure before XP commits releases the reservation; a throwing observer after commit must not make the pickup collectible again. Observers must still obey the foundation's no-throw contract.
- `ExperienceRunBootstrap` binds a new run state from an explicit serialized definition to the scene's player in Awake. Future run coordination replaces this bootstrap; the controller can still bind an existing state.
- `ExperienceDebugPresenter` is an Editor-only, read-only level/current/required/total XP indicator with a progress bar, consistent with the existing development overlays. It is not final HUD art.

## Composition and validation

### Attraction extension

The collector scans nearby pickup colliders each physics step using a reused overlap buffer, growing it on saturation rather than dropping results. The default world-space attraction radius is **3 metres**, centered at a local offset of `(0, 0.75, 0)` on the player. Check the pickup's center against the radius, not just its collider overlap. The Ignore Raycast pickup layer is included explicitly; other colliders are ignored.

In-range pickups move toward the player's current collection point through their kinematic Rigidbody at **8 metres/second**. Movement is clamped to prevent overshoot. Entering the radius does not immediately award XP: arrival or ordinary contact invokes the existing exactly-once collection contract. There is no persistent target lock; leaving the radius stops attraction, and returning resumes it. Death, disabled components, uninitialized XP and pause block attraction as well as collection. There is no line-of-sight or obstacle avoidance for these non-solid XP orbs.

`PlayerExperienceCollector.SetAttractionRadius` accepts finite non-negative values and applies changes to existing drops on the next physics step. Zero disables attraction but preserves ordinary contact collection. The radius and collection point are Inspector-authored; selecting the collector shows a range gizmo. Pickup speed is authored on the pickup prefab. Future upgrades may pass an evaluated radius through this boundary; no new stat type, modifier or upgrade is added in this increment.

Attraction tests cover the exact radius and collider-edge boundary, runtime radius changes, zero/invalid values, travel before award, approaching/leaving/re-entering range, death mid-flight, pause/disable gating, and piles larger than the initial query buffer.

Player prefab: XP controller and collector. Chaser prefab: drop adapter referencing the pickup prefab. Test_Waves: run bootstrap and development indicator. Curve asset: 100 initial XP, +50 per subsequent level. Other scenes do not implicitly start an XP run.

Automated coverage: validation, exactly-once awards, unrelated colliders, dead/uninitialized/paused/disabled players, resuming while overlapping, multi-level pickups, observer failure, death versus disable/destruction, and asset/scene wiring. Run full EditMode and PlayMode suites after composition changes.

Manual Test_Waves check: kill a chaser outside pickup range, observe a stationary cyan orb, then approach within 3 metres of its center and watch it fly toward you before receiving 25 XP. Four orbs reach level 2; the next level costs 150 XP. Chest rewards and weapon switching preserve XP. Death stops flight and collection. Level-up does not yet spawn a chest.
