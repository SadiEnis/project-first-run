# Reward Foundation

## 1. Amaç

Reward Foundation, Project First Run'da chest/reward sistemlerinin oyuncuya sunabileceği item seçeneklerini data-driven ve deterministic biçimde üretmek için temel domain/runtime altyapısını tanımlar.

Bu branch'in ana amacı **reward offer generation ve eligibility**'dir.

Önemli sınır:

```text
Reward Foundation
→ hangi item'ların teklif edilebileceğini belirler

Category-specific acquisition runtime
→ seçilen item'ın gameplay'e nasıl kurulduğunu belirler
```

Bu branch Weapon, Ability veya Upgrade runtime'ını doğrudan kurmaz.

## 2. Neden Claim/Acquisition Bu Branch'te Değil?

Mevcut runtime'da:

```text
Upgrade
→ generic PlayerUpgradeController.TryAcquire(...)

Ability
→ PlayerAbilityController + concrete runtime factory

Weapon
→ active weapon initialization/runtime
```

Üç kategori henüz aynı generic acquisition contract'ına sahip değildir.

Reward Foundation'ın claim sırasında category switch veya concrete Fireball/weapon bilgisi taşıması istenmez.

Bu yüzden foundation önce doğru reward seçeneklerini üretir. Claim/application integration daha sonra category-specific acquisition path'leri hazır olduğunda bağlanır.

## 3. Temel Akış

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

Reward pool bir `ScriptableObject` content asset'i olabilir ve `ItemDefinition` referansları tutar.

```text
RewardItemPool
├── WeaponDefinition
├── AbilityDefinition
└── UpgradeDefinition
```

Pool item'ın nasıl acquire edildiğini bilmez; yalnızca candidate source'tur.

## 7. Eligibility Rules

Bir item reward candidate olabilmek için:

```text
definition null olmamalı
StableId geçerli olmalı
category desteklenmeli
PlayerBuild item'a zaten sahip olmamalı
ilgili category'de boş slot bulunmalı
```

İlk foundation sürümünde duplicate item teklif edilmez.

## 8. PlayerBuild Source of Truth

Reward Foundation kendi ownership veya capacity state'ini tutmaz.

Kontroller doğrudan:

```text
PlayerBuild.Contains(...)
PlayerBuild.HasFreeSlot(...)
```

üzerinden yapılır.

## 9. RewardOffer

`RewardOffer` plain C# immutable runtime object'tir.

Minimum veri:

```text
IReadOnlyList<ItemDefinition> Choices
```

Kurallar:

```text
null choice yok
aynı category + StableId duplicate yok
choice sırası korunur
external mutation yok
```

## 10. RewardOfferGenerator

Generator:

```text
eligible candidate'ları alır
requested choice count kadar unique seçim yapar
RewardOffer üretir
```

Şunlardan sorumlu değildir:

```text
item acquire etmek
UI göstermek
chest destroy etmek
rarity hesaplamak
currency vermek
```

## 11. Randomness

Unity `Random` doğrudan domain sınıfına gömülmez.

Minimal abstraction:

```text
IRandomSource
{
    int Next(int minInclusive, int maxExclusive);
}
```

Runtime adapter Unity random kullanabilir; testler deterministic/fake random kullanır.

## 12. Selection Semantics

İlk sürüm:

```text
uniform random selection
without replacement
```

Aynı offer içinde aynı item iki kez seçilmez.

## 13. Requested Choice Count

```text
requestedChoiceCount > 0
```

Eligible candidate sayısı daha azsa mevcut eligible count kadar choice döner. Eligible item yoksa empty offer normal sonuçtur.

## 14. Stable Identity

Candidate uniqueness:

```text
ItemCategory + StableId
```

üzerinden değerlendirilir.

## 15. Pool Validation

Programmer/configuration errors:

```text
null pool entries
missing/untrimmed StableId
unsupported category
duplicate same category + StableId entries
```

## 16. Development Validation

Development-only presenter offer'ı Game View'da gösterebilir:

```text
Reward Debug

Eligible: 4
Offer:
1. Fireball
2. Development Damage Boost
3. Plasma Rifle
```

Mevcut OnGUI debug presenter yaklaşımı kullanılır; TMP/Canvas gerekmez.

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

Reward Foundation concrete acquisition/runtime sınıflarını bilmez.

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
eligible count smaller than requested count
no eligible candidates
```

## 19. Performance

```text
FindObjectsByType yok
scene scan yok
ScriptableObject runtime mutate edilmez
candidate filtering explicit collection üzerinden yapılır
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

## 22. Sonraki Aşama

Reward offer generation tamamlandıktan sonra category-specific acquisition yolları tamamlanmalıdır.

Muhtemel sonraki işler:

```text
weapon loadout/acquisition
ability acquisition registry/factory
reward claim integration
```

Reward claim sistemi bu runtime'lar hazır olmadan category switch veya concrete content bilgisi taşımamalıdır.
