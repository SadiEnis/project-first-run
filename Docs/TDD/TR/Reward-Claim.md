# Reward Claim

## Amaç

`RewardOffer` içinden seçilen `ItemDefinition`'ı doğru acquisition yoluna yönlendirmek ve başarılı claim sonrasında offer'ın yalnızca bir kez tüketilmesini sağlamak.

```text
RewardOffer
    ↓ selected ItemDefinition
RewardClaimSession
    ↓
PlayerRewardClaimController
    ↓
RewardClaimHandlerRegistry
    ├── WeaponRewardClaimHandler
    ├── AbilityRewardClaimHandler
    └── UpgradeRewardClaimHandler
```

Concrete Fireball, concrete weapon veya concrete upgrade bilgisi Reward Claim katmanına sızmaz.

## Mevcut Acquisition Yolları

```text
Weapon  → PlayerWeaponAcquisitionController
Ability → PlayerAbilityAcquisitionController
Upgrade → PlayerUpgradeController
```

Reward Claim bu sistemleri tekrar etmez; yalnızca orchestration yapar.

## Temel Karar

Merkezi `ItemCategory switch` yerine `IRewardClaimHandler` kullanılır.

Her handler yalnızca kendi acquisition sistemini bilir ve sonucu ortak `RewardClaimResult` contract'ına map eder.

## Scope

```text
RewardClaimResult
RewardClaimSession
IRewardClaimHandler
RewardClaimHandlerRegistry
WeaponRewardClaimHandler
AbilityRewardClaimHandler
UpgradeRewardClaimHandler
PlayerRewardClaimController
Player prefab wiring
tests
development validation
```

## Out of Scope

```text
final reward UI
chest interaction/lifecycle
rarity
weighted rewards
reroll
currency fallback
item level-up/evolution
save/meta
weapon switching
```

## RewardClaimSession

`RewardOffer` immutable kalır. Claim state ayrı plain C# object'te tutulur:

```text
RewardClaimSession
├── Offer
├── IsClaimed
└── ClaimedDefinition
```

Session:
- seçimin gerçekten offer içinde olduğunu doğrular,
- başarılı claim'i commit eder,
- ikinci başarılı claim'i engeller.

Selection yalnızca offer'daki **exact `ItemDefinition` instance** ise kabul edilir. Aynı StableId'ye sahip başka instance reddedilir.

## RewardClaimResult

```text
Claimed
AlreadyClaimed
AlreadyOwned
CapacityReached
```

Semantik:
- `Claimed`: acquisition başarılı, session consumed.
- `AlreadyClaimed`: session daha önce consumed, acquisition yapılmaz.
- `AlreadyOwned`: state offer üretildikten sonra değişmiş olabilir, session açık kalır.
- `CapacityReached`: state değişmiş olabilir, session açık kalır.

Normal claim sonucu olmayan durumlar exception'dır:

```text
null selection
selection not in offer
missing handler
multiple matching handlers
missing dependency
unsupported handler result
invalid runtime configuration
```

## Handler Contract

```text
IRewardClaimHandler
{
    bool Supports(ItemDefinition definition);
    RewardClaimResult TryClaim(ItemDefinition definition);
}
```

Handler session veya UI bilmez.

### Weapon mapping

```text
WeaponAcquireResult.Acquired         → Claimed
WeaponAcquireResult.AlreadyOwned     → AlreadyOwned
WeaponAcquireResult.CapacityReached  → CapacityReached
```

### Ability mapping

```text
AbilityAcquireResult.Acquired         → Claimed
AbilityAcquireResult.AlreadyOwned     → AlreadyOwned
AbilityAcquireResult.CapacityReached  → CapacityReached
```

### Upgrade mapping

```text
UpgradeAcquireResult.Acquired         → Claimed
UpgradeAcquireResult.AlreadyOwned     → AlreadyOwned
UpgradeAcquireResult.CapacityReached  → CapacityReached
```

## Handler Registry

`RewardClaimHandlerRegistry` exactly one matching handler resolve eder.

Kurallar:

```text
null registration yok
same instance duplicate yok
no match → configuration error
multiple matches → ambiguity error
```

Concrete category switch içermez.

## PlayerRewardClaimController

Akış:

```text
1. session validate
2. selection offer içinde mi?
3. session already claimed mi?
4. handler resolve
5. category acquisition çağır
6. Claimed ise session commit
7. RewardClaimed event publish
```

Önemli invariant:

```text
acquisition failure → session açık
successful claim    → session consumed
```

Event:

```text
RewardClaimed(ItemDefinition)
```

yalnızca başarılı claim'de publish edilir.

## Dependency Direction

```text
Rewards
  ↓
Reward Claim
  ↓
Claim Handlers
  ↓
Weapon / Ability / Upgrade Acquisition
```

Acquisition katmanları Reward'a bağımlı olmaz.

## Player Composition

```text
Player
├── PlayerWeaponAcquisitionController
├── PlayerAbilityAcquisitionController
├── PlayerUpgradeController
└── PlayerRewardClaimController
```

Ability factory registry initialization Reward Claim'in sorumluluğu değildir.

## Tests

### EditMode

```text
RewardClaimSession
- new session not claimed
- exact offered instance accepted
- non-offered selection rejected
- same StableId different instance rejected
- successful commit stores choice
- second commit rejected

RewardClaimHandlerRegistry
- registration
- null rejection
- duplicate instance rejection
- matching resolution
- no match
- ambiguous match
```

### PlayMode

```text
weapon handler result mapping
ability handler result mapping
upgrade handler result mapping

PlayerRewardClaimController
- weapon choice claims
- ability choice claims
- upgrade choice claims
- successful claim consumes session
- event publishes once
- second claim → AlreadyClaimed
- selection outside offer rejected
- AlreadyOwned does not consume session
- CapacityReached does not consume session
- missing handler does not consume session
- Player prefab contains controller
```

## Development Validation

`Test_Waves` içindeki mevcut Reward Debug offer kullanılabilir.

```text
offer görünür
gerçek offered item claim edilir
PlayerBuild ownership değişir
ilgili acquisition runtime etkisi görülür
aynı session ikinci kez claim edilmez
Console clean
```

Final UI bu branch'te yapılmaz. Gerekirse geçici OnGUI claim controls yalnızca development tooling olarak eklenir.

## Implementation Stages

### Stage 1 — Claim Session Domain
```text
RewardClaimResult
RewardClaimSession
EditMode tests
```
Checkpoint:
```text
feat: add reward claim session domain
```

### Stage 2 — Handler Contract + Registry
```text
IRewardClaimHandler
RewardClaimHandlerRegistry
EditMode tests
```
Checkpoint:
```text
feat: add reward claim handler registry
```

### Stage 3 — Category Acquisition Adapters
```text
WeaponRewardClaimHandler
AbilityRewardClaimHandler
UpgradeRewardClaimHandler
tests
```
Checkpoint:
```text
feat: add reward acquisition claim handlers
```

### Stage 4 — Player Claim Orchestration
```text
PlayerRewardClaimController
runtime composition
Player prefab wiring
tests
```
Checkpoint:
```text
feat: add player reward claim orchestration
```

### Stage 5 — Development Validation
Checkpoint only if new development tooling is added.

### Stage 6 — Final Regression
```text
EditMode Run All
PlayMode Run All
Test_Waves
Console clean
reward-claim → dev
```

## Sonraki Sıra

```text
1. weapon-switching
2. reward-selection-ui
3. chest-foundation
```
