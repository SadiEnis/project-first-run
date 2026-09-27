# Enemy health bars

Scope: readable temporary health feedback for content testing, including delayed bleed/burn damage. No status icons, damage numbers, combat or balance changes.

- Attach EnemyHealthBar to the shared enemy prefab used by Chaser, Charger and Ranger.
- A small world-space canvas above the enemy faces Camera.main (optional camera override). No camera, uninitialized/dead enemy or disabled component means hidden UI.
- Show current/maximum health as a left-anchored fill, including full health. Update immediately on HealthChanged; synchronize in LateUpdate as Initialize does not emit that event.
- Build the tiny UI once per enemy instance, reuse it on enable/reset, and destroy it with the enemy. No raycast targets or GraphicRaycaster.
- Offset, dimensions and colors are Inspector settings. The health component on the same object is authoritative.
- User runs Unity tests and gameplay acceptance. Bleed gameplay remains unverified until health loss after leaving the orbit is observed; do not infer acceptance from implementation.
