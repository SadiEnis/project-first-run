# Lightning Staff — content design

## Status

Fifth ability on abilities-content, following GDD 12.4. The user agreed to random targeted area strikes, L6 stun and L8 one-hop chains. Documentation only at this checkpoint; implementation and validation remain pending.

## Mechanics

- Random living, visible current-map enemies within 15 m are primary targets. No target means no cooldown. Prefer distinct targets; remaining strikes may reuse targets if there are fewer enemies than strikes.
- Instant strikes at target positions, with a temporary overhead bolt and impact feedback. No projectile travel or persistent damage field.
- Each strike damages every eligible enemy within 2 m once, regardless of collider count. Require the same floor and unobstructed impact-to-target visibility. Independent overlapping strikes may damage the same enemy.
- Apply AbilityDamage scaling. L6 adds 0.5 s stun: suspend voluntary movement and attack progression, including Charger motion and Ranger firing. Preserve attack state/timers; resume without an extra attack or resetting cooldowns. Incoming damage and other status durations continue.
- Reapplied stun refreshes remaining duration to at least 0.5 s, not additive duration. Restore control without overriding acid slow or other owners. Death/reuse clears stun.
- L8: each strike chains once to the nearest living enemy within 4 m of its impact, excluding enemies already damaged by that strike. Require the same floor and clear visibility. Same damage/stun, single target only; no recursive chain or secondary area explosion. No candidate means no chain. Independent strikes may share recipients.
- Prototype stun affects all ranks. Boss resistance/immunity is deferred to an explicit balance decision.
- Pause freezes cooldowns, stun and visual lifetime. Player death prevents new casts; existing stun expires normally. Rebind registry on map change and clean up map-owned visuals/status objects.

## Initial balance

Acquisition range 15 m and impact radius 2 m throughout. Damage values and 4 m chain range are provisional implementation defaults; balance remains revisable.

| Level | Strike/chain damage | Cooldown s | Strikes | Stun s | Extra chain targets per strike |
| --- | --- | --- | --- | --- | --- |
| 1 | 20 | 4 | 1 | 0 | 0 |
| 2 | 30 | 4 | 1 | 0 | 0 |
| 3 | 30 | 3 | 1 | 0 | 0 |
| 4 | 30 | 3 | 2 | 0 | 0 |
| 5 | 40 | 3 | 2 | 0 | 0 |
| 6 | 40 | 3 | 2 | 0.5 | 0 |
| 7 | 40 | 3 | 3 | 0.5 | 0 |
| 8 | 40 | 3 | 3 | 0.5 | 1 |

## Implementation and verification

1. Validated level configuration, runtime factory, current-map targeting and existing damage scaling.
2. Timed stun integrated with movement and attack ownership. Zero navigation speed alone is insufficient; explicit charge movement and attacks must also pause.
3. Strike/chain feedback, authored asset, saved arena/F1 and ability/mixed reward pools. Preserve starting equipment and slot limits.
4. Author tests for progression, eligibility/occlusion, collider deduplication, independent overlaps, chain exclusions, stun refresh/expiry, acid coexistence, Charger/Ranger suspension, pause/death/reuse and map rebinding. Update catalog/pool expectations.
5. User runs EditMode/PlayMode and gameplay acceptance; assistant only runs specifically requested missing/failing tests. No evolution, final art, builds or VCS actions.

Separate docs and implementation check-ins. Stop for gameplay acceptance before Shuriken.
