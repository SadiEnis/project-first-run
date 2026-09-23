# Cross-scene map travel — stage 7

## Mechanics

- Each generic map exit holds an Inspector-editable full scene asset path and entry ID. Routes define map order and may branch; there is no mandatory global linear map list. Paths must be enabled in the build. This exit is a scene-loading adapter, not a gate/pit-specific mechanic.
- `MapSceneRoot` defines the target map, inactive content root and named entry transforms with region IDs. Entry resolution is limited to the loaded scene; missing/duplicate IDs and unknown regions fail before transfer. The selected region becomes the map's initial location.
- Keep the existing root player GameObject and all its child objects/runtime components. No new player prefab, snapshot reconstruction, XP reset, loadout initialization or automatic reward claim. The player's camera, weapon/ability origins and persistent runtime dependencies must belong to that hierarchy. Scene-local UI/spawners belong to the map and bind to the incoming player.
- Ability target selectors and factories have a map-registry binding contract: rebind existing entries and future acquisitions to the destination enemy registry only at commitment. Preserve ability levels/cooldowns and upgrade modifiers. Maps with initialized ability acquisition or owned abilities require exactly one local enemy registry. Future abilities with additional map-local dependencies must extend this contract explicitly.
- One `SceneTravelController` lives on the player root. It reserves the source map against walking/other exits, freezes scaled gameplay and player input/action updates, then loads the destination additively. The old map remains until target validation/binding succeeds. Time scale and component enabled states are restored, including a previously disabled component.
- Target content is authored inactive: no target enemies, UI or bootstrap may run before binding. The root holds only metadata; all gameplay belongs beneath content. Known region/preparation/chest-bootstrap components receive the existing player before activation. Legacy development bootstraps and a second player must not be present in destination maps.
- After validation, move/teleport the player to the chosen entry, select the target as active scene, activate content, then retire/unload the old scene. Old enemies, projectiles and unclaimed loot are not transferred. Unclaimed loot is lost without compensation when its map unloads. Map arrival does not award victory or save a checkpoint.

## Failure and lifecycle

Reject dead/paused players, pending local traversal, duplicate requests, invalid paths, already-loaded targets and same-scene requests before loading. Before commitment, cancellation/death/disabled owner or bad target returns to the source and unloads only the newly loaded destination. Unity scene loads are not forcibly cancelled: finish the operation, then dispose the unwanted scene. No timeout that abandons a still-running load.

Once the player is committed, old-scene unload failure cannot roll back destroyed content: keep the player in the destination, deactivate old roots, report cleanup failure and block further travel until explicit cleanup retry succeeds. A failed precommit cleanup similarly requires retry before a new load. Explicit cancellation applies only before commitment. Application shutdown and externally unloading owned scenes are outside the normal transaction contract.

## Scene authoring

For a destination, create two roots: an active metadata object with `MapSceneRoot`, and an **inactive** content object containing `MapTraversalController`, geometry, a map-local `EnemyRegistry`, entries, region components, exits and UI. Assign the content/map/entry array on the metadata object. Entry transforms must be descendants of content. Assign local geometry/group/spawner/view references normally; leave incoming player references for runtime binding. An exit's source region must exist in the local map; its destination path and entry ID identify the next map. Add target scenes to the build before using the runtime loader.

The initial scene keeps its existing root player and initialized build/acquisition systems; its content is active and source exits reference that player’s `SceneTravelController` and the local `MapSceneRoot`. Destination maps must not run starting-loadout/XP/development bootstraps again. If a needed destination component cannot be bound, reject the destination rather than starting partially configured gameplay.

The destination's `LevelUpChestSource` binds to current player XP and starts at the current level: old levels are not awarded twice, later levels enqueue rewards normally. Scene-local pending drop queues are not transferred with the player; pending-reward persistence is not implemented.

`Test_SceneTravelTarget` is a minimal technical fixture with inactive content and an entry, not an arena layout. The real-load test uses Unity's editor additive-loading entry point so the user's build scene list need not change. Packaged-build loading and platform profiling still require build verification at playable-map integration.

## Verification scope

This is a single-player foundation, not background corridor streaming or a zero-stutter guarantee. Loading UI/fades, save/restart, final victory, NavMesh/lightmap authoring and the playable map are later stages. New targets use the scene contract; `Test_Waves` is not silently migrated. Fixture tests cover real Unity scenes, player identity/state, entry selection, duplicate requests, invalid destinations, cancellation/death, cleanup/retry and initial map locking. A real additive-load adapter test complements controlled operation tests. Record actual results after execution.

Unity reference: [asynchronous scene loading](https://docs.unity3d.com/6000.3/Documentation/ScriptReference/SceneManagement.SceneManager.LoadSceneAsync.html), [root-object scene transfer](https://docs.unity3d.com/6000.3/Documentation/ScriptReference/SceneManagement.SceneManager.MoveGameObjectToScene.html).
