# Upgrades content plan

## Scope and workflow

Git branch: `feature/content/upgrades-content`, based on `content`. Intended Plastic branch: `/main/dev/content/upgrades-content`. All 16 upgrades share this branch. User owns branch operations, check-ins, commits and merges; do not create per-item branches.

Accepted scope is all nine GDD 13.1 stat upgrades plus all seven GDD 13.2 traits, replacing the former ten-upgrade target. Names below are approved working display names and can change with the game's theme. Stable technical IDs must not depend on localized display text; concrete IDs will be assigned with implementation.

This checkpoint is documentation only. Do not claim new upgrade assets, runtime integration or validation.

## Accepted roster

| Group | English name | Turkish name | Mechanical identity |
| --- | --- | --- | --- |
| Stat | Wrath Seal | Hiddet Mührü | General weapon/ability damage |
| Stat | Ironhide | Demir Deri | Damage mitigation |
| Stat | Second Heart | İkinci Kalp | Maximum health |
| Stat | Lifesprout | Yaşam Filizi | Health regeneration |
| Stat | Attraction Core | Çekim Çekirdeği | Pickup attraction radius |
| Stat | Memory Crystal | Hafıza Kristali | Experience gain |
| Stat | Broken Hourglass | Kırık Kum Saati | Ability cooldown |
| Stat | Windweave | Rüzgâr Örgüsü | Movement speed |
| Stat | Fire Rhythm | Ateş Ritmi | Applicable weapon fire rate |
| Trait | Last Stand | Son Direniş | Low-health damage (Berserker) |
| Trait | Iron Oath | Demir Yemin | Armor for movement cost (Heavy Armor) |
| Trait | Blood Pact | Kan Ahdi | Damage for maximum-health cost |
| Trait | Glass Heart | Cam Kalp | Large damage for survivability cost (Glass Cannon) |
| Trait | Loaded Dice | Hileli Zar | Luck/reward probabilities (Lucky) |
| Trait | Fluid Mechanism | Akıcı Mekanizma | Reduced reload movement penalty (Ammo Expert) |
| Trait | Crimson Fang | Kızıl Diş | Health from kills (Vampire) |

## Proposed implementation order

1. Shared rules and consumer audit: choose level counts, stacking formula, caps, timing of stat changes and health adjustment policies. Existing stat enum entries are not proof of runtime integration.
2. Wrath Seal, Windweave, Fire Rhythm: basic offensive/movement consumers.
3. Second Heart, Ironhide, Lifesprout: maximum health, mitigation and healing.
4. Attraction Core, Memory Crystal: pickups and XP.
5. Broken Hourglass: scoped cooldown reduction per [Broken Hourglass](Broken-Hourglass-Upgrade.md). Fluid Mechanism is deferred unchanged until reload movement-penalty design exists; do not replace it with reload speed.
6. Last Stand, Iron Oath, Blood Pact, Glass Heart: conditional and trade-off effects.
7. Crimson Fang, Loaded Dice: kill attribution/healing and precisely scoped probability consumers.
8. Reward/F1 integration and interactions review, followed by user acceptance and branch closure.

Discuss each group's mechanics before implementing. Record EN/TR TDD before code. Split docs and coherent implementation checkpoints without fragmenting every file change. User runs EditMode/PlayMode and manual checks; assistant authors tests and handles only specifically requested missing/failing runs.

## Shared contracts and unresolved decisions

- All 16 use the existing shared upgrade slot pool; stat/trait grouping is not a new category or separate capacity.
- Accepted progression: five levels for the nine stat upgrades and three for the seven traits. Ordinary percentages add; each level replaces its previous full effect set. The first group's values and contracts are in [Core Stat Upgrades](Core-Stat-Upgrades.md); other item values remain open.
- Each level represents the full effect set, not an accumulating increment. Respect existing upgrade ownership, replacement and maximum-level exclusion.
- Verify damage coverage for weapon/ability hits, fragments and timed effects; document snapshot versus live recalculation.
- Maximum-health gain/loss, damage-reduction formula/cap and regeneration are implemented per [Survival Stat Upgrades](Survival-Stat-Upgrades.md); user validation is pending.
- Approved cooldown scope: ordinary recast waits and Shuriken post-orbit waits only; no Drone cadence or active-effect acceleration. See [Broken Hourglass](Broken-Hourglass-Upgrade.md). Implement available upgrades first, then revisit additional-system dependencies individually.
- Last Stand, Iron Oath, Blood Pact and Glass Heart mechanics are approved in [Conditional and Trade-off Upgrades](Conditional-Tradeoff-Upgrades.md). Implement individually with acceptance after each; Glass Heart uses incoming damage amplification before armor.
- Fluid Mechanism remains deferred without changing its reload movement-penalty identity. Do not invent a penalty solely to make the item useful without agreement.
- Crimson Fang is implemented: guaranteed 2/5/7 health per player-owned kill, including timed damage; no overheal or resurrection. See [Crimson Fang](Crimson-Fang-Upgrade.md). User validation is pending.
- Loaded Dice boosts Green/Purple/Legendary selection weights by 20/40/60%, not chest counts or generation chances. See [Loaded Dice](Loaded-Dice-Upgrade.md) for scope, probability math and balance criteria. Implemented; user reports successful EditMode/PlayMode and gameplay acceptance. Evolution, gold/meta and bad-luck protection remain excluded.
- Preserve run ownership, reset and cross-map behavior; test combinations, not only isolated acquisitions.
- The development damage item must have an explicit replacement/migration decision; avoid accidental duplicate real damage upgrades in pools.

## Special reward-card presentation

The seven traits should eventually be visually distinguished by a bright frame with glow directed inward, not spilling outward. User may provide a visual reference. Record as presentation work pending reference/design; do not implement generic exterior bloom now. This does not imply higher rarity or a separate slot pool.

Gold/meta progression, new evolution content, final theme/art and Windows builds remain out of scope. Earlier ability validation gaps (including Shuriken bleed gameplay and unreported automated results) remain open, not implicitly closed by this stage.
