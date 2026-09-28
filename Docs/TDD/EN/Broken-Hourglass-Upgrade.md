# Broken Hourglass

## Status and scope

Approved mechanics on upgrades-content. Documentation checkpoint before implementation; no runtime changes or test execution claimed.

Broken Hourglass (Kırık Kum Saati) uses the shared upgrade slots and five levels. Full effects replace the previous level; percentages from applicable sources add. Balance values are provisional.

| Level | Cooldown reduction |
| --- | --- |
| 1 | 5% |
| 2 | 10% |
| 3 | 15% |
| 4 | 20% |
| 5 | 25% |

## Timing contract

- Effective cooldown = current ability level's base cooldown * (1 - total reduction).
- Clamp total reduction to 0–75%; this item alone reaches 25%.
- Affect ordinary ability recast cooldowns and Shuriken's waiting period after its orbit ends.
- Do not affect Drone firing cadence, weapon fire/reload, projectile speed, active effect/orbit duration, volley spacing, damage-over-time ticks or stun/slow durations.
- Capture the current reduction when a new cooldown starts. Acquiring or leveling this upgrade must not shorten, reset or rescale an already running cooldown, nor grant an immediate cast.
- Preserve targeting requirements, failed-cast behavior, pause/death/control gates and existing ability-level semantics.
- For Shuriken, calculate the next wait at the existing orbit-end cooldown-start point, from its applicable base cooldown. Apply once; do not accelerate its entire continuous runtime.
- Re-evaluate from unmodified base values to avoid compounding reductions. Scene travel must preserve running waits; a fresh run must not retain previous-run modifiers.

## Implementation and verification plan

Audit AbilityRuntimeState, AbilityRuntimeEntry and Shuriken's independent clock before changing consumers. Keep the new stat interpretation explicit: the existing AbilityCooldown enum member alone does not establish a runtime formula.

Add a five-level real asset to the saved content arena, its builder, F1 catalog and upgrade/mixed pools. Preserve slot limits and maximum-level exclusion.

Author tests for all levels, additive stacking/cap, correct interval math, unchanged ongoing waits, failed casts, Shuriken post-orbit timing and unaffected active durations/Drone cadence. Update catalog assertions and exercise representative ordinary abilities. User runs Unity compilation, EditMode/PlayMode and manual acceptance; assistant only executes explicitly requested missing/failing tests.

## Deferred work

Fluid Mechanism retains its reload movement-penalty identity. There is currently no such movement penalty to reduce; do not substitute reload-speed bonuses or introduce a penalty merely to enable this item.

Finish upgrades that can be implemented with the available systems first. Revisit items requiring additional systems/design decisions individually afterward. Fluid Mechanism's asset and reward-pool integration remain deferred, not silently completed or dropped from the sixteen-item scope.
