# Chest tier foundation

Same-branch continuation: the owner chose to implement [Common Category Chests](Common-Chest-Types.md) before merging this branch. That increment replaces the single development entries with three category definitions and adds placeholder presentation. The sections below record the preceding foundation increment.

## Baseline and scope

Starts from integrated Plastic `/main/dev` **cs:154** and Git `main` **204745f**, after the owner validated enemy death drops. This is the foundation before implementing the eight chest types in GDD section 15, not the complete chest-type milestone.

`ChestRarity` records **Common, Uncommon, Rare, Legendary** on `ChestDefinition`, with explicit serialized values 0–3. Missing data defaults to Common for the existing development chest. Unknown enum values fail validation. Rarity is metadata, not a numeric item level, drop probability, reward category, or automatic quality multiplier. Weapon/Ability/Upgrade chests may all be Common; Boss is legendary-tier but has a distinct reward policy. Golden Chest will use a dedicated gold reward path, not an invented fifth rarity.

## Shared weighted selection

`ChestDropTable` is a ScriptableObject holding positive integer weights and `ChestDefinition` references. It answers **which chest**, never **whether a chest drops**. Selection uses one `IRandomSource.Next(0, totalWeight)` call and exclusive cumulative intervals. Weights need not sum to 100 and rarity does not silently modify them. Null/empty entries, missing definitions, nonpositive weights, invalid definitions, overflowing totals and out-of-range random output fail explicitly. Assets do not hold runtime random state.

`EnemyChestDropProfile` retains the chance in basis points and references a table. Zero chance requires no table; positive chance requires a valid table. Death probability and subsequent weighted selection remain separate rolls. The selected definition remains in the existing corpse-independent queue; retries never reroll it.

`LevelUpChestSource` now references a table instead of a single definition. Every level still guarantees one chest; there is no level-up drop-chance roll. Selection happens at the first eligible processing attempt, outside the XP event. The chosen definition is retained across blocked placement, pause/death, disable/re-enable and spawn errors. It clears only after a successful spawn, so the next entitlement gets its own selection. A destroyed selected definition is an error, not permission to reroll. The source validates every configured prefab's placement contract at initialization.

The existing tracker still owns entitlement counts. The source owns only the current pending selection. Player-relative placement, bounded processing, modal behavior and single-claim completion do not change.

## Content migration and unchanged gameplay

- `CDT_DevelopmentLevelUp`, `CDT_DevelopmentNormal`, and `CDT_DevelopmentElite` are **separate assets** so future weights can be edited independently. All currently contain only `CD_DevelopmentChest` with weight 1.
- The two enemy profiles migrate their former embedded entries to the matching table references. Their **25% / 50%** provisional chances are unchanged; the elite example still has no spawned elite content.
- `Test_Waves` level-up source migrates its old single-definition reference to the level-up table. The starter chest still references the development definition directly.
- All tracked profiles, scene references and test fixtures are migrated together. Untracked external/custom profiles using the old embedded field need manual table creation and assignment; no silent runtime fallback masks missing configuration.
- No new colored chest prefabs, reward pools, drop-frequency balancing or rank-based enemy behavior are introduced. Existing chests remain visually and functionally the same in this stage.

## Next chest-type stage

Implement the three common category chests first (Weapon, Ability, Upgrade), then mixed Green/Purple and the Legendary/Boss policies as the required reward capabilities become available. Keep rarity, category filtering, number of offered choices, number of permitted claims, evolution policy and presentation separate. The GDD working name “Purple Chest” currently means Rare with a planned blue visual; Legendary is purple. Do not infer semantics from colors or names.

Item-level rewards, evolutions, two-choice claim sessions and gold fallback/Golden Chest remain unimplemented. Adding rarity metadata must not imply those behaviors already work.

## Verification

Automated checks cover rarity validation; shared-table intervals and bad data; unchanged enemy chance boundaries; independent source-table assets; weighted selection for multiple level gains; retained selection after failed placement/disable; and existing XP, death, spawn and chest interaction regressions.

Manual `Test_Waves` regression should look unchanged: XP attracts, levels guarantee chests, enemy deaths sometimes add chests, and E still opens/claims an eligible reward. Inspector verification: inspect Rarity on the development definition and the three independent table assets. Changing a rarity alone must not change drop chance or reward behavior. Record actual suite results in README; this foundation's manual acceptance remains separate from the already validated enemy-drop milestone.
