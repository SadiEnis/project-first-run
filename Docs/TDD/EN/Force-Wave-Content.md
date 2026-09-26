# Force Wave — agreed design and implementation

Second ability in Abilities Content Plan, on the existing abilities-content branch. User approved the sector, conditional activation and initial values on 2026-09-26, with later gameplay revision allowed. GDD 12.1 defines frontal crowd control and the upgrade order, not the numerical values below.

## Proposed mechanic

- Automatic, instantaneous frontal sector: 100 degrees total width, initially 4 m reach. Facing follows the player's horizontal facing direction, not the nearest enemy. Vertical aim does not tilt the wave into the ground.
- Activate only when a living, active enemy in the current map registry is inside the sector and has world line of sight. No eligible target means no cooldown. Use an explicit vertical tolerance (initially 2 m) to avoid hitting another floor.
- Evaluate collider-center positions consistently for range, sector and cover. Ignore source colliders, actors and triggers as cover; solid world geometry blocks the effect.
- One cast damages each eligible receiver once, even with multiple colliders. Apply AbilityDamage once to base damage. Accepted non-lethal hits attempt horizontal push away from the player through IKnockbackReceiver; push failure does not cancel applied damage. Revalidate lifetime and source health between receivers.
- Reuse EnemyMotor's collision-safe knockback. Do not teleport through walls or add a separate movement system. Do not invent boss immunity or rank multipliers in this increment.
- Single cooldown per performed wave. Preserve remaining cooldown on level-up. Pause, player death and map transition prevent new casts; preserve registry rebinding.
- Temporary expanding arc feedback may animate after impact, but it does not repeatedly apply damage. No projectile, stun, burn, evolution gameplay, gold or final VFX/audio.

## Proposed playtest defaults

| Level | Damage | Reach m | Cooldown s | Push m |
| --- | --- | --- | --- | --- |
| 1 | 20 | 4 | 5 | 2 |
| 2 | 30 | 4 | 5 | 2 |
| 3 | 30 | 5 | 5 | 2 |
| 4 | 30 | 5 | 3 | 2 |
| 5 | 30 | 5 | 3 | 3 |
| 6 | 40 | 5 | 3 | 3 |
| 7 | 40 | 6 | 3 | 3 |
| 8 | 40 | 6 | 1.5 | 3 |

## Gameplay acceptance — 2026-09-26

User accepted the mechanic and personally tested revised cooldowns: L1–3 5 s, L4–7 3 s, L8 1.5 s. The table above and automated test expectations match the user-authored asset. These revisions were not rerun automatically at the user's request. Earlier test counts below/above describe the pre-tuning run only; the outstanding full-suite issues are retained for later regression work. Overall ability balance and clearer progression feedback will be revisited after content expansion.

Width stays fixed; area upgrades increase reach. Values are approved initial playtest defaults, not final balance.

## Implementation checkpoints

1. Agree mechanic and commit EN/TR docs.
2. Runtime sector/cover selection, damage, push and eight-level configuration with focused tests.
3. Saved ability asset, acquisition/reward/F1 catalog integration and temporary presentation. Preserve the authored arena; do not reconstruct it at runtime.
4. Full EditMode/PlayMode regressions, then stop for gameplay acceptance before Drone.

Verification scope includes sector/range/height boundaries, no target/no cooldown, facing versus nearest target, world cover, multiple colliders, lethal hits, refused push, wall-safe movement, AbilityDamage evaluation, level changes, pause/death, registry rebinding, max-level rewards and saved arena acquisition.

## Implementation

ForceWaveDefinition validates eight authored levels. ForceWaveRuntime shares its target collection between availability and execution, applies damage once per receiver and delegates survivor push to IKnockbackReceiver. ForceWaveRuntimeFactory preserves cooldown through the standard acquisition/level path. The existing development bootstrap registers both Fireball and Force Wave factories.

AD_ForceWave uses stable identity ability.force-wave. It is appended to the saved content arena/F1 catalog and added to ability and mixed reward pools; existing catalog indices, starting weapon and scene geometry are preserved. A short, map-owned LineRenderer arc uses the existing plasma projectile material as temporary presentation. It has no collider or damage logic and expires after .25 scaled seconds or source death. No evolution implementation or Windows build is included.

## Validation status — 2026-09-26

EditMode: 889/889 passed (force-wave-final-EditMode.xml). All three new ForceWaveArenaTests passed, including actual damage/push, wall blocking, cooldown-preserving upgrades and chest eligibility. Full PlayMode: 534/536 passed (force-wave-final-PlayMode.xml). The legacy common-chest test expected one ability instead of the expanded pool's two; its assertion has been corrected but not rerun. MinigunArenaTests.HoldPreparesThenConsumesOneRoundPerShotAndRecoilAccumulates failed its released-input assertion; the cause is not established. The follow-up full run was not authorized, so a fully green regression result is pending. Reports are under .codex-temp/xp-attraction. User Force Wave gameplay acceptance is pending.
