# Drone content
Discrete raycast shots with a short tracer; no physical projectiles. Each drone independently selects the nearest visible, alive current-map enemy. Walls block shots; the first enemy hit receives AbilityDamage once. No ammo, reload, spin-up, piercing, burn or evolution gameplay.

| Level | Damage | Shots/s per drone | Range m | Drones | Boss rate multiplier |
| --- | --- | --- | --- | --- | --- |
| 1 | 8 | 2 | 12 | 1 | 1 |
| 2 | 12 | 2 | 12 | 1 | 1 |
| 3 | 12 | 3 | 12 | 1 | 1 |
| 4 | 12 | 3 | 18 | 1 | 1 |
| 5 | 16 | 3 | 18 | 1 | 1 |
| 6 | 16 | 3 | 18 | 2 | 1 |
| 7 | 16 | 4 | 18 | 2 | 1 |
| 8 | 16 | 4 | 18 | 2 | 1.5 |

Damage/range are provisional defaults. L8 applies to Boss rank only. The second drone starts half an interval later; independent targeting does not guarantee permanent alternation. Upgrades preserve remaining shot delay; no-target frames do not consume a new cooldown and stalled frames do not cause catch-up bursts.

A narrow continuous-ability tick hook leaves ordinary cast abilities on their original path. Drone owns per-drone timers, not the entry's one-shot cooldown. Map-owned shoulder visuals have no colliders, use reusable tracers and rebuild after registry rebinding. World obstruction constrains placement; pause freezes timers; disabled control prevents fire; death/source destruction removes visuals. Both drones occupy one ability slot. Factory construction has no scene side effects before successful acquisition.

AD_Drone is appended to F1, ability/mixed reward pools and the editor builder without rebuilding the arena. Temporary geometry/materials are feedback, not final art.

## Validation
Acceptance update: user confirmed Drone gameplay, both drones and target-facing presentation. User reported EditMode 889/889 and other PlayMode tests green except four weapon-arena fixtures not yet run. Those four fixtures alone subsequently passed: Minigun 13/13, Plasma 2/2, Rocket 2/2, Shotgun 9/9 (26/26, weapon-arenas-only.xml). No assistant full-suite run or dedicated L8 boss acceptance is claimed. The paragraph below records the original handoff.

Per user request, EditMode/PlayMode and builds are skipped for this increment; compilation/runtime success is not claimed. Catalog-count assertions are updated mechanically. Manual acceptance: acquire via F1, check follow/targeting/wall blocking, compare L3/L7 speed, L4 range and L6 second drone, then pause, die and change map. L8 requires a Boss-ranked target. Previous full-suite follow-up remains deferred.
