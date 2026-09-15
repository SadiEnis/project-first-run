# Region preparation

## Contract — roadmap 5

PreparedRegionEncounter prepares one regional enemy group using existing EnemySpawnRequest/EnemySpawner services. WaveController remains for wave-sequence encounters; this increment does not prewarm those sequences.

- An approach trigger calls RequestPreparation; repeated calls do not duplicate work.
- Each Update creates at most MaxEnemiesPerFrame enemies and checks a time budget after each enemy. A single Instantiate cannot be interrupted, so the millisecond budget is a soft limit.
- Instantiate and initialize beneath an inactive parent. Disable EnemyController, motor, attack and physical colliders, then move the root into the active scene so Awake/preparation work is spread across frames. Dormant enemies never join the registry or move/attack.
- Prepared models must remain out of sight through level design: place the approach trigger before an occluding corridor/door. This is not automatic occlusion or shader precompilation.
- The activation trigger enters RegionEncounterSession when ready. Early entry waits; leaving cancels the intent. Entry also requests preparation if the approach trigger was skipped.
- RegionPassageController binds destination preparation. Its barrier waits for readiness; Ready/Running/Victory allow travel. Readiness is distinct from encounter completion.
- Activation enables AI, colliders and registry membership on existing instances, without spawning again. Measure activation separately; large-group activation cost needs profiling.
- Death or disabling/destroying the preparation owner cancels work and cleans owned enemies without simulating deaths/drops. Failure/cancellation is terminal; a new run constructs a new session.
- Reentry preserves enemies/health. Leaving an active area does not suspend combat. Unvisited-region eviction remains a later decision.

## State and setup

Preparation: Idle → Preparing → Ready; errors/cancellation → Failed/Cancelled. IArenaSession Ready/Running/Victory/Defeat describes combat. Bind the regional session only after preparation is Ready.

Inspector supports an existing EnemyWaveDefinition used as one group, spawn points, EnemySpawner, EnemyRegistry, player/health and region identity. Code Initialize uses the same path. RegionPreparationTrigger takes a generic trigger Collider, player root and purpose (Prepare/Activate). Use one activation volume per region; multiple preparation volumes are allowed. Scanning covers initial occupancy and compound colliders. Size volumes for player speed to avoid skipping them within one physics step.

## Acceptance and measurement

Real-enemy tests cover item budget, dormancy, registry/damage, early entry/retreat, unrelated colliders, readiness barriers, partial-failure cleanup, death, cancellation and reentry. Profiler markers: Region.PrepareStep and Region.Activate. Last/MaxPreparationStepMilliseconds and LastActivationMilliseconds provide development measurements; headless tests do not guarantee target-device FPS. Pooling is deferred until measurements and the playable scene justify it.

## Validation — 15 September 2026

- EditMode: 733/733; PlayMode: 420/420 passed. The 12 new preparation PlayMode tests use a real NavMesh.
- The final four-enemy sample measured maximum preparation step 0.4034 ms and activation 0.1689 ms. Previous measurements were 0.3451 ms and 0.0674 ms respectively; these depend on environment/load and are not performance targets.
- Sphere triggers require actual shape overlap, not just AABB intersection. Destroying a readiness source does not mark its passage ready.
- Local reports: .codex-temp/xp-attraction/preparation-edit-final.xml and preparation-play-verified.xml.
- Test_Waves is unchanged. Playable corridors/rooms are scheduled for roadmap 9; this increment integrates runtime code and automated fixtures.
