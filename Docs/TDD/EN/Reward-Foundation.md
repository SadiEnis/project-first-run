# Reward Foundation

## 1. Purpose

Reward Foundation defines the domain/runtime layer used by chest and reward systems in Project First Run to generate data-driven, deterministic item offers.

The main responsibility of this branch is **reward offer generation and eligibility**.

Important boundary:

```text
Reward Foundation
→ decides which items may be offered

Category-specific acquisition runtime
→ decides how a chosen item is installed into gameplay
```

This branch does not directly install Weapon, Ability or Upgrade runtime state.

## 2. Why Claim/Acquisition Is Not Part of This Branch

The current runtime has different acquisition shapes:

```text
Upgrade
→ generic PlayerUpgradeController.TryAcquire(...)

Ability
→ PlayerAbilityController + concrete runtime factory

Weapon
→ active weapon initialization/runtime
```

The three categories do not yet share one generic acquisition contract.

Reward Foundation must not compensate with category switches or concrete Fireball/weapon knowledge.

Therefore the foundation first produces correct offers. Claim/application integration is added later when category-specific acquisition paths are ready.

## 3. Core Flow

```text
Reward Item Pool
      ↓
RewardCandidateFilter
      ↓
PlayerBuild ownership/capacity
      ↓
Eligible Candidates
      ↓
RewardOfferGenerator
      ↓
RewardOffer
```

## 4. Scope

```text
RewardItemPool
RewardCandidateFilter
RewardOffer
RewardOfferGenerator
IRandomSource
deterministic test random implementation
PlayerBuild eligibility integration
development reward debug presenter
EditMode tests
PlayMode development validation
```

## 5. Out of Scope

```text
Chest world objects
Chest opening interaction
Reward selection UI
Chosen reward claim/application
Weapon runtime installation
Ability runtime installation
Upgrade runtime installation
Item level-up
Evolution
Reroll
Currency fallback
Weighted rarity
Chest color rules
Save serialization
Meta progression
Network synchronization
```

## 6. Reward Item Pool

A reward pool may be a ScriptableObject content asset containing `ItemDefinition` references.

```text
RewardItemPool
├── WeaponDefinition
├── AbilityDefinition
└── UpgradeDefinition
```

The pool does not know how items are acquired; it is only a candidate source.

## 7. Eligibility Rules

An item may be offered when:

```text
definition is not null
StableId is valid
category is supported
PlayerBuild does not already own it
the category has a free slot
```

The initial foundation does not offer already-owned duplicates.

## 8. PlayerBuild Source of Truth

Reward Foundation does not maintain separate ownership or capacity state.

It delegates to:

```text
PlayerBuild.Contains(...)
PlayerBuild.HasFreeSlot(...)
```

## 9. RewardOffer

`RewardOffer` is an immutable plain C# runtime object.

Minimum data:

```text
IReadOnlyList<ItemDefinition> Choices
```

Rules:

```text
no null choices
no duplicate category + StableId identity
choice order is preserved
no external mutation
```

## 10. RewardOfferGenerator

Responsibilities:

```text
obtain eligible candidates
select up to requested choice count
select unique items
produce RewardOffer
```

It does not acquire items, display UI, destroy chests, calculate rarity or grant currency.

## 11. Randomness

Unity `Random` is not embedded directly in the domain class.

Minimal abstraction:

```text
IRandomSource
{
    int Next(int minInclusive, int maxExclusive);
}
```

Runtime may use Unity random; tests use deterministic/fake random.

## 12. Selection Semantics

Initial selection:

```text
uniform random
without replacement
```

The same item cannot appear twice in one offer.

## 13. Requested Choice Count

```text
requestedChoiceCount > 0
```

If fewer eligible candidates exist, return all available eligible candidates. No eligible candidates may produce an empty offer.

## 14. Stable Identity

Candidate uniqueness uses:

```text
ItemCategory + StableId
```

## 15. Pool Validation

Programmer/configuration errors:

```text
null pool entries
missing/untrimmed StableId
unsupported category
duplicate category + StableId entries
```

## 16. Development Validation

A development-only presenter may show generated offers in Game View:

```text
Reward Debug

Eligible: 4
Offer:
1. Fireball
2. Development Damage Boost
3. Plasma Rifle
```

Use the existing OnGUI debug presenter approach; TMP/Canvas is not required.

## 17. Dependency Direction

```text
Items
  ↓
Reward Pool / Offer
  ↓
Reward Candidate Filter
  ↓
PlayerBuild

Random Source
  ↓
RewardOfferGenerator
```

Reward Foundation does not know concrete acquisition/runtime classes.

## 18. Error Handling

Programmer/configuration errors:

```text
null dependencies
invalid requested choice count
invalid pool content
invalid stable IDs
unsupported category
invalid random result
```

Normal outcomes:

```text
candidate already owned
category full
eligible count below requested count
no eligible candidates
```

## 19. Performance

```text
no FindObjectsByType
no scene scanning
no runtime ScriptableObject mutation
candidate filtering uses explicit collections
```

## 20. Testing Strategy

### EditMode

```text
pool validation
already-owned item filtered
full category filtered
different categories handled independently
eligible item preserved
duplicate pool identity rejected
offer choices are read-only
requested count validation
selection without replacement
deterministic random behavior
eligible count below requested count
empty eligible set
```

### PlayMode / Development

```text
real PlayerBuild eligibility source
development offer generation
debug presenter displays choices
existing gameplay unaffected
```

## 21. Implementation Stages

### Stage 1 — Reward Domain

```text
RewardOffer
IRandomSource
RewardCandidateFilter
EditMode tests
```

Changeset:

```text
feat: add reward selection domain
```

### Stage 2 — Reward Content Pool

```text
RewardItemPool
pool validation
EditMode tests
```

Changeset:

```text
feat: add reward item pool
```

### Stage 3 — Offer Generation

```text
RewardOfferGenerator
UnityRandomSource
deterministic tests
```

Changeset:

```text
feat: add reward offer generation
```

### Stage 4 — Development Validation

```text
development reward bootstrap/presenter
Game View offer visibility
Test_Waves validation
```

Changeset:

```text
feat: add development reward offer validation
```

### Stage 5 — Final Regression

```text
EditMode Run All
PlayMode Run All
manual Test_Waves validation
Console clean
reward-foundation → dev
```

## 22. Next Layer

After offer generation, category-specific acquisition paths should be completed before chosen rewards are applied.

Likely follow-up work:

```text
weapon loadout/acquisition
ability acquisition registry/factory
reward claim integration
```

Reward claim should not contain category switches or concrete content knowledge simply because those runtime paths are not ready yet.
