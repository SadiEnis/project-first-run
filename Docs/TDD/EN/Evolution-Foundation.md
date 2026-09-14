# Evolution foundation

Evolution is a replacement, not an additional item level. An `EvolutionDefinition` references a source item, a result item and optional item requirements. The source must be owned at maximum level; the result must be unowned and share the source category. `PlayerEvolutionController` validates every condition before atomically replacing the source stable ID with the result in the same `PlayerBuild` slot.

The result starts at level 1 with its authored maximum. No new gameplay item assets are introduced in this foundation. Runtime weapon/ability behavior replacement, evolution reward chest selection, final content balance and evolution-specific visuals remain subsequent work. Optional requirements can represent a future “required upgrade at maximum” rule without forcing that rule globally today.

## Validation — 2026-09-14

The isolated Unity 6000.3.9f1 project passed 668 EditMode and 354 PlayMode tests. Temporary ScriptableObjects verify maximum-level gating, optional requirements, same-category validation, result ownership conflicts, same-slot replacement and failure atomicity.
