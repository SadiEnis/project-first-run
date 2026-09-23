# Playable map fixture — stages 9–10

## Layout contract

The prototype map is one authored scene with placeholder geometry and a small target scene. The main area starts the player and contains two optional returnable side routes. Preparation corridors request groups before their activation areas. The second main area is reached through a reusable one-way passage; its reverse route is absent by contract. A final exit uses the stage 7 scene-loading adapter and leads to PlayableMapDestination.

The map is an authored graph, not a wave-clear corridor: the player may leave an encounter and use a free exit while enemies remain. Encounter-gated exits are available for future rooms but are not applied to the ordinary side routes. Returning to a side route preserves its encounter instances and loot. The one-way connection does not become a checkpoint.

## Fixture composition

- The scene contains deterministic placeholder floors, walls, labels and triggers under one content root. `PlayableMapFixtureBootstrap` is a debug presenter only; it never creates gameplay objects at runtime.
- References are serialized: one player, one map session/registry, two returnable side passages, three preparation/activation pairs, one one-way passage and a scene exit.
- Existing enemy, XP and level-up chest services are connected. The fixture wave contains four Basic Chasers: they drop XP but have no enemy chest-drop profile. Level-up chests use existing pools.
- The destination contains metadata, inactive content, a known entry, floor/walls/NavMesh, registry and reward services. It is not a second combat area.
- Exit and trigger colliders are sized for the player body and do not spawn at the player. Labels communicate source/destination IDs and whether a route returns or is one-way.

In the temporary view, gray/blue is the main floor, cyan is the north returnable side area, green is the south returnable side area, and yellow is the second main area. Gray connector floors bridge the visible gaps. The colored areas are floors, not locked walls; the thin gateway at each connector is the passage trigger/barrier.

## Acceptance tests

Open the fixture from the Unity Editor and run it: the player starts in the main area, can visit either side route and return, sees the preparation/activation route, and can use the one-way route without a reverse passage. The final exit loads the target scene and places the same player at its entry. Test both a short route and an exploration route. Confirm no second player, visible target spawn before arrival, or automatic chest/XP award on crossing.

This fixture is intentionally small and deterministic. It does not represent the final art, procedural generation, enemy variety, performance budget or restart UI. Those remain later polish/validation work.

The target is editor-loadable by asset path. The validation build tool includes both scenes through an explicit scene list without modifying the default Build Profile. Include these scenes in the list when building manually with another configuration.

## Stage 10 implementation and acceptance — 20 September 2026

- Geometry, triggers, baked NavMesh and service references are authored and saved in the Editor. Play does not rebuild the map. The rebuild tool only owns these fixture scenes.
- Both side areas and the second main area use four existing Basic Chasers each. Each group awards 100 XP, using the existing 100/150/... curve and level-up chest source. These are test values, not balance decisions.
- Walls occlude spawns from preparation locations. Preparation precedes the passage and activation occurs inside the room. Passages wait for readiness, not enemy deaths. Barrier rendering and navigation obstacles follow the physical collider. Reentry preserves enemies and loot.
- Wire XP startup, the existing reward UI prefab, chest spawner and Fireball acquisition factory. No automatic opening reward or free ability. Preserve the actual player, build and XP across scene travel.
- A separate playable arrival fixture provides floor, local registry and reward services; leave Test_SceneTravelTarget unchanged. Arrival is neither victory nor a checkpoint.
- PlayMode tests load saved scenes and cover preparation, navigation, XP → level → chest → claim, reentry and real travel. Verify a standalone build with an explicit scene list without changing the user's default Build Profile.
- Display preparation/activation costs and frame time. Batch timings do not establish visual FPS acceptance. Manually verify one-way boundaries, hidden spawning and short/exploration routes.
- Hub/meta/gold remain excluded. Map-based final outcomes and actual new-run setup remain a separate integration item; the arena reset callback does not complete that work.

## Automated evidence — 20 September 2026

- Full isolated-project suites: **752/752 EditMode, 449/449 PlayMode**, no failures or skipped tests. These include four tests against the saved fixture and its real prefab/service dependencies, not only synthetic encounter objects.
- Saved-scene coverage: hidden preparation and baked navigation; physical one-way closure; side reentry preserving enemy identity/health; four kills producing 100 XP and a claimable level-up chest; leaving living enemies behind; actual scene loading preserving the player/build/weapon/XP and binding destination reward services.
- A separate 32-enemy probe verifies at most two instances created per frame and activation of the same prepared instances. Maximum preparation step: **2.4892 ms**; activation: **0.9747 ms**. Whole-probe managed heap difference: **466,944 bytes**; this is not an allocation counter or a per-frame GC measurement.
- A prior cold saved-scene run reached **25.9395 ms** for a preparation step; later fixture runs peaked at **1.1808 ms** or less. The 2 ms budget is soft: an individual Instantiate cannot be interrupted. These observations do not prove hitch-free play or justify declaring pooling unnecessary. Defer a pooling change until graphical-player profiling identifies the recurring cost and required target budget.
- Batch mode does not validate camera occlusion from every playable angle, GPU frame time, perceived stutter, or the fun/balance of the two routes. Those remain manual acceptance items.

### Manual route checklist

1. Open `Assets/_Project/Scenes/Playable/PlayableMapFixture.unity`; do not rebuild it at runtime. The starting area is gray.
2. Exploration route: enter the cyan north or green south corridor. Preparation occurs before the passage. Inside the room, go around the **right end** of the sight-screen wall to reach four Chasers. Collect their XP, open the level-up chest and claim a reward. Leave and return; surviving enemies must retain health and must not duplicate.
3. Short route: ignore both side rooms and use the east passage into the yellow area. Once fully across, its gate closes behind you. Go around the **north end** of the sight-screen wall; enemies activate, but killing them is not required for leaving.
4. Use the exit near the yellow area's northeast corner. The playable destination has a floor and preserves player state; it does not award free XP/chests or declare victory. It is a travel-validation endpoint, not a complete second level.
5. Check preparation visibility, route readability and hitches in normal rendered play. Map-based death/final outcome/new-run integration is still separate; this checklist does not certify the hub loop.

## Windows package result — distribution acceptance blocked, work deferred

- Unity **6000.3.9f1**, URP **17.3.0**, explicit two-scene Windows x64 Development build in the isolated test project. No default Build Profile, engine version or package dependency changes were made in the source project.
- The first build failed in Input System's linker callback with an assembly CodeBase string conversion error. Launching Unity with the existing ASCII Windows short path passed that step. A clean build then completed (`STAGE10_BUILD_PASS`, 170,735,586 bytes). Build products remain outside version control.
- **Standalone traversal has NOT passed.** Before the smoke check reaches the map, the player reports different serialization layouts for `UniversalRenderPipelineAsset` (448 read / 596 expected) and `UniversalRendererData` (260 / 292), attempts an invalid approximately 63.8 GB allocation, and crashes. This is not evidence that map preparation consumes that amount of memory.
- Reproduced after `BuildOptions.CleanBuildCache`, both with `-batchmode -nographics` and with `-batchmode -force-d3d11`. Changing working directory to the package folder did not help. These attempts do not rule out every import-cache or asset issue; the root cause is not yet established.
- Unity issue [UUM-146262](https://issuetracker.unity.com/issues/23231/urp-asset-serialization-error-and-crash-on-android-when-moving-asemdef-and-scripts-as-part-of-pre-build-process) reports a similar renderer-layout error under a different Android/build-hook reproduction. It is a diagnostic lead, **not proof that this project has the same defect** and not approval to upgrade Unity.
- When resumed: isolate import/type-data caches and URP assets in the disposable copy; if needed compare with an independently imported copy or an explicitly agreed engine version. Do not upgrade the source project automatically. Then rerun `--validate-map` and distribution acceptance. The 24 September scope decision removes this investigation as a prerequisite for the current merge; the fault is still open.
- Local evidence: `.codex-temp/xp-attraction/stage10-all-edit.xml`, `stage10-all-play.xml`, `stage10-build-clean.log`, `stage10-player-clean.log`, and `stage10-player-d3d11.log`. Logs and generated players are temporary; this document records the durable result.

## User acceptance and scope closure — 24 September 2026

The user reported working regions/side areas, the closing passage, enemy spawning/death, chest/XP/level progression and scene travel. This functional acceptance does not establish hidden spawning from every camera angle or hitch-free performance. The user deferred Windows build investigation and chose to connect real-map death/final outcome/fresh-run behavior during demo work, or earlier if needed. The branch can close as the map foundation with those open items recorded; next is a content test scene and individual weapon/ability expansion. This documentation update changes no runtime code and includes no new automated test run.
