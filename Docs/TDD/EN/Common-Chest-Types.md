# Common category chests

## Scope and branch agreement

Continue on `/main/dev/chest-tier-foundation` / `codex/chest-tier-foundation`, after rarity/table implementation **cs:156 / 01b1d29**. The owner explicitly chose to complete the foundation and its first chest types on the same branch, then merge them together after testing. No new branch or intermediate merge is needed.

This increment implements the acquisition path for Weapon, Ability and Upgrade chests from GDD 15.1–15.3. All are Common, request up to three distinct eligible choices and allow one claim. The current content pool is small: weapons contain Plasma Rifle and Development Secondary (the starting rifle is already owned), abilities contain Fireball, upgrades contain Development Damage Boost. Fewer than three eligible choices is valid. Item-level improvements are not implemented, so these are the initial category-specific acquisition chests, not the complete GDD reward progression.

## Category contract

`ChestRewardCategory` has explicit serialized values Mixed=0, Weapon=1, Ability=2, Upgrade=3. Mixed preserves the old development definition. `ChestDefinition` stores category and optional display name independently of rarity, reward pool and requested choice count. A missing display name falls back to StableId.

Typed definitions reject any pool item outside the declared category. This is a content error, not a reason to silently remove an incorrectly configured candidate. Validation runs at spawn/initialization and again before generating an offer, so changing a pool after spawning cannot bypass the category boundary. Invalid data leaves the chest Available without opening a modal. The existing eligibility filter still excludes owned items and full categories; the existing claim/session/UI code remains the authority for acquisition and completion.

## Content and test scene

- Three chest definitions and three category-only reward pools are stored in the existing Chests/Dev and Reward/Dev folders. They share the existing chest prefab.
- Each independent source table now contains Weapon/Ability/Upgrade definitions with equal provisional weights 1:1:1. Level-up still guarantees one chest. Enemy chance stays 25%, with the unused elite example at 50%. These weights are test configuration, not final balance.
- `ChestView` applies a grey per-instance tint and one camera-facing name label to Common typed chests. Shared materials are not changed. The label belongs to the existing visual root and disappears with it after claim. It follows the main camera in LateUpdate and retains its orientation if no main camera exists. A render check exposed mirrored double-sided text with front/back labels, so the single-label presentation replaces that draft. Mixed development chests keep their previous presentation. This is placeholder presentation, not final art or localization.
- The Editor-only starter fixture spawns a Weapon chest in front, an Ability chest to its left and an Upgrade chest to its right. Extra fixture chests are configured on the existing bootstrap; empty additional lists preserve its previous single-chest behavior. It rejects invalid definitions before spawning, prevents repeated spawns and cleans up a partial batch on failure.
- Disable the automatic `UpgradeDevelopmentBootstrap` only in Test_Waves; otherwise the sole upgrade is already owned before its chest opens. Acquiring it from a chest now supplies the same 20% weapon/ability damage bonus. Other attack/test scenes are not changed.

## Limits and next work

Test_Waves sets `FireballDevelopmentBootstrap.Grant Starting Ability` to false: the factory registry is still installed, but Fireball is acquired through its chest. The new option defaults to true, preserving other scenes. The starter fixture retains its existing two-slot weapon loadout; the default one-slot player build is unchanged.

Opening is still explicit with E/gamepad South. Empty offers leave the chest available with no modal; there is no gold fallback or reroll into a different category. After the three sample acquisitions, many subsequent chests legitimately have no eligible rewards. Do not interpret this as a drop-source bug.

Item-level rewards, additional item content, mixed Green/Purple content, evolution, Legendary/Boss multi-claim rules, Golden Chest and gold fallback remain separate increments. Neither enum nor grey tint adds these behaviors.

## Verification

An isolated Unity render preview was visually inspected on 2026-09-14: all three grey placeholders show one readable name label each, without the earlier mirrored duplicate. This was a presentation check, not a Test_Waves gameplay session.

Automated tests cover category validation (including cross-category data), legacy Mixed compatibility, real content/table/scene wiring, opening category-pure offers and claiming once, post-spawn pool corruption, placeholder labels/tint, and starter-fixture batch behavior. Run both full suites before check-in.

Validation on 2026-09-14: full suites passed, **626 EditMode / 332 PlayMode**, in the isolated Unity 6000.3.9f1 test project. The increment adds 22 EditMode and 6 PlayMode tests. SHA-256 matched all 29 new/changed Unity files to the test copy. The showcase test uses the scene's two-slot weapon capacity and installs the Fireball factory without granting ownership; it claims all three categories through the actual selection buttons. Default Fireball granting remains covered for other scenes. These results do not replace the owner's gameplay acceptance.

Manual Test_Waves: start a fresh run. Claim each of the three labelled starter chests; expect Development Secondary, Fireball and Development Damage Boost respectively. Q switches the acquired weapon; the upgrade should no longer be installed before claiming. Kill enemies and collect XP to check that generated chests use the three categories, with unchanged enemy drop chance and one guaranteed chest per level. Repeatedly opening an exhausted category must not pause or consume it. Record manual acceptance separately from automated results.
