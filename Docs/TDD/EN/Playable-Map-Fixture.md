# Playable map fixture — stage 9

## Layout contract

The prototype map is one authored scene with placeholder geometry and a small target scene. The main area starts the player and contains two optional returnable side routes. A preparation corridor requests the next group before its activation area. The second main area is reached through a reusable one-way passage; its reverse route is absent by contract. A final scene exit uses the stage 7 scene-loading adapter and leads to the technical target scene.

The map is an authored graph, not a wave-clear corridor: the player may leave an encounter and use a free exit while enemies remain. Encounter-gated exits are available for future rooms but are not applied to the ordinary side routes. Returning to a side route preserves its encounter instances and loot. The one-way connection does not become a checkpoint.

## Fixture composition

- The scene contains deterministic placeholder floors, walls, labels and triggers under one content root. `PlayableMapFixtureBootstrap` is a debug presenter only; it never creates gameplay objects at runtime.
- All component references are serialized in the scene: one player instance, one map session, local registries, two returnable side passages, one preparation/activation pair, one one-way passage and a scene exit.
- Existing enemy, XP and chest services are used where references are available; the fixture adds only one fixture-scoped wave asset with a no-drop enemy so preparation can be exercised without coupling to chest-drop setup.
- The target scene contains only metadata, an inactive content root, a known entry and a registry. It is intentionally not a second arena.
- Exit and trigger colliders are sized for the player body and do not spawn at the player. Labels communicate source/destination IDs and whether a route returns or is one-way.

In the temporary view, gray/blue is the main floor, cyan is the north returnable side area, green is the south returnable side area, and yellow is the second main area. Gray connector floors bridge the visible gaps. The colored areas are floors, not locked walls; the thin gateway at each connector is the passage trigger/barrier.

## Acceptance tests

Open the fixture from the Unity Editor and run it: the player starts in the main area, can visit either side route and return, sees the preparation/activation route, and can use the one-way route without a reverse passage. The final exit loads the target scene and places the same player at its entry. Test both a short route and an exploration route. Confirm no second player, visible target spawn before arrival, or automatic chest/XP award on crossing.

This fixture is intentionally small and deterministic. It does not represent the final art, procedural generation, enemy variety, performance budget or restart UI. Those remain later polish/validation work.

The target scene is editor-loadable by asset path for this prototype. Before a player build, both scenes must be added to the active Build Profile.
