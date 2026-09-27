# Core stat upgrades — first content group

## Status and scope

Mechanics accepted for Wrath Seal, Windweave and Fire Rhythm on upgrades-content. This is the pre-implementation documentation checkpoint; no new gameplay implementation or test execution is claimed.

Shared progression: nine stat upgrades have five levels; seven traits have three. Ordinary percentage bonuses to the same stat add together. Each upgrade level replaces that upgrade's previous full effect set; it does not add every earlier level again. Other sources remain intact. Trait costs/caps and remaining item rules will be discussed separately.

## Approved starting progression

| Item | Turkish name | L1 | L2 | L3 | L4 | L5 |
| --- | --- | --- | --- | --- | --- | --- |
| Wrath Seal | Hiddet Mührü | +10% | +20% | +30% | +40% | +50% |
| Windweave | Rüzgâr Örgüsü | +5% | +10% | +15% | +20% | +25% |
| Fire Rhythm | Ateş Ritmi | +10% | +20% | +30% | +40% | +50% |

Values remain provisional balance, not final tuning.

### Wrath Seal

- Apply equal additive percentages to WeaponDamage and AbilityDamage.
- Audit all actual damage consumers, including fragments, secondary explosions and timed effects. Apply the bonus once, not once at launch and again at impact.
- Preserve existing launch/activation snapshots for active projectiles/effects; later stat changes affect subsequent damage snapshots. Do not rewrite established ability lifecycles merely to make this upgrade live-updating.
- Replace the development damage item in playable catalogs/reward pools rather than offering two equivalent damage upgrades. Before implementation, inspect references and choose an explicit asset/identity migration; preserve existing test fixtures intentionally.

### Windweave

- Connect MoveSpeed to ground movement. Jump height and gravity remain unchanged.
- Evaluate from the original configured base speed, not a previously modified result. Preserve input normalization and existing movement restrictions/penalties.
- New acquisitions, level changes, reset and map transitions must not compound multipliers or leave stale speed.

### Fire Rhythm

- Connect WeaponFireRate to shots per second for applicable player weapons. Interval = 1 / evaluated fire rate: +50% rate divides the interval by 1.5, not by 2.
- Preserve automatic/semi-automatic input behavior, magazine consumption and reload rules. A semi-automatic gun still needs a new trigger press for each shot.
- Do not speed up reload, ability cooldowns or Drone firing; do not change projectile speed, pellet/fragment count or burst identity.
- Preserve a shot's already-running wait without resetting it or granting a free shot on acquisition/upgrade/switch. New shot intervals use the updated rate. Validate this against existing weapon state ownership before editing.
- Switching weapons must not lose per-weapon ammunition or timing state.

## Integration and verification plan

1. Audit stat consumers and existing development-damage references before deciding asset migration.
2. Document/fix missing MoveSpeed and WeaponFireRate consumers without changing base movement or weapon semantics.
3. Add five-level real assets and saved content arena/F1 plus upgrade/mixed reward pool integration.
4. Author checks for full-effect replacement, additive stacking, maximum-level exclusion, all three level tables, movement without jump/gravity changes, fire-rate math/input behavior, reload isolation, damage snapshot coverage, reset and cross-map preservation.
5. User runs EditMode/PlayMode and manual acceptance. Assistant runs only requested missing/failing tests. No automatic VCS writes.

After implementation stop for gameplay evaluation of these three; do not implement the next group or special card glow yet.
