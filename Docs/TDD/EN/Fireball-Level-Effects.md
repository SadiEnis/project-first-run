# Fireball level effects

Fireball continues the item-level-foundation branch after weapon and upgrade effects. The acquired runtime snapshots eight provisional configurations. This increment applies base damage, cooldown and projectile speed; it preserves one runtime entry and uses the existing automatic target/execution pipeline.

Level 1 retains the existing asset values. Levels 2–8 use complete absolute values, validated before acquisition. Asset edits after acquisition do not retune the runtime. A level-up requires matching PlayerBuild ownership, the exact definition reference and runtime level; missing ownership and maximum level are normal results, while mismatches fail without mutation.

Projectile count, burn duration, evolution eligibility, UI and chest reward selection are intentionally not connected here. They require separate behavior/evolution decisions.

## Validation — 2026-09-14

The isolated Unity 6000.3.9f1 project passed 666 EditMode and 351 PlayMode tests. Three new PlayMode tests cover cooldown progression, snapshot behavior, duplicate/alias protection and single-entry ownership. No SCM operation is performed by the agent.
