# Upgrade Foundation

## 1. Amaç

Upgrade Foundation, Project First Run'da run sırasında edinilen upgrade'ların data-driven biçimde tanımlanmasını, `PlayerBuild` ownership kurallarına göre alınmasını ve `PlayerStats` üzerine modifier olarak uygulanmasını sağlar.

Temel zincir:

```text
UpgradeDefinition
      ↓
PlayerUpgradeController
      ├── PlayerBuildController
      └── PlayerStatsController
                ↓
          StatModifier
```

Upgrade sistemi weapon veya ability runtime'ını doğrudan değiştirmez. Sayısal etkiler `PlayerStats` üzerinden consumer sistemlere ulaşır.

## 2. Temel Tasarım İlkeleri

Ownership ve effect ayrı sorumluluklardır:

```text
PlayerBuild → ownership
PlayerStats → runtime numeric effects
PlayerUpgradeController → acquisition orchestration
```

`UpgradeDefinition` yalnızca content/configuration verisidir; runtime state değildir. ScriptableObject runtime sırasında mutate edilmez.

Foundation controller concrete upgrade davranışlarını stable ID switch'leriyle hard-code etmez.

## 3. Scope

```text
UpgradeDefinition
UpgradeStatModifierData
UpgradeAcquireResult
PlayerUpgradeController
Player prefab integration
EditMode tests
PlayMode integration tests
development upgrade content
weapon + ability stat effect validation
```

## 4. Out of Scope

```text
Chest / Reward generation
Upgrade seçim UI
Upgrade level-up
Evolution
Temporary buffs
Conditional effects
On-kill / on-hit proc effects
Health-threshold effects
Save serialization
Meta progression
Reroll
Upgrade removal / replacement UI
```

## 5. UpgradeDefinition

`UpgradeDefinition`, `ItemDefinition` türevidir.

```text
Category → Upgrade
StableId
DisplayName
Stat Modifiers
```

İlk foundation etkileri stat modifier datası üzerinden ifade edilir.

Örnek:

```text
upgrade.development_damage_boost
├── WeaponDamage +20%
└── AbilityDamage +20%
```

## 6. UpgradeStatModifierData

Unity Inspector'da serialize edilebilen küçük bir data yapısıdır.

Alanlar:

```text
PlayerStatType
StatModifierOperation
Value
```

`SourceId` ayrıca asset üzerinde yazılmaz. Runtime `StatModifier.SourceId`, doğrudan `UpgradeDefinition.StableId` değerini kullanır.

## 7. Validation

Configuration error örnekleri:

```text
empty stable ID
invalid stat type
invalid operation
NaN / Infinity value
```

Stat-specific domain invariant'ları generic definition tarafından zorlanmaz.

## 8. Acquisition Result

```text
Acquired
AlreadyOwned
CapacityReached
```

Normal gameplay sonuçları exception değildir. Programmer/configuration hataları exception'dır.

## 9. PlayerUpgradeController

Dependencies:

```text
PlayerBuildController
PlayerStatsController
```

Ana API:

```text
UpgradeAcquireResult TryAcquire(UpgradeDefinition definition)
```

Akış:

```text
1. definition validate edilir
2. runtime StatModifier listesi hazırlanır
3. PlayerBuild.TryAdd(definition)
4. Added değilse modifier uygulanmaz
5. Added ise modifier'lar PlayerStats'a eklenir
6. UpgradeAcquired event yayınlanır
```

Modifier'lar ownership mutation'dan önce oluşturulur; invalid modifier config, PlayerBuild değişmeden önce hata verir.

## 10. Duplicate Upgrade

Existing `PlayerBuild` stable ID ownership source-of-truth olmaya devam eder.

```text
first acquire → Acquired → modifiers applied once
same StableId again → AlreadyOwned → no stacking
```

## 11. Capacity

PlayerBuild capacity kuralları authoritative kalır.

```text
capacity full
→ CapacityReached
→ PlayerStats unchanged
```

Upgrade system ikinci bir capacity sayacı tutmaz.

## 12. Runtime Modifier Source

Bir upgrade'ın tüm modifier'ları aynı source ID'yi kullanır:

```text
UpgradeDefinition.StableId
```

Bu ileride `RemoveBySource` ile uyumludur.

## 13. Events

Minimum event:

```text
UpgradeAcquired(UpgradeDefinition definition)
```

Başarısız normal acquisition sonuçları için event zorunlu değildir.

## 14. Player Prefab

```text
Player
├── PlayerBuildController
├── PlayerStatsController
└── PlayerUpgradeController
```

## 15. Development Integration

Development-only bir upgrade:

```text
Development Damage Boost
├── WeaponDamage +20%
└── AbilityDamage +20%
```

Bu final içerik değildir. Mevcut iki stat consumer'ı aynı anda doğrular.

## 16. Dependency Direction

```text
Items
  ↓
UpgradeDefinition
  ↓
PlayerUpgradeController
  ├── Builds
  └── Stats

Weapons / Abilities
       ↓
     Stats
```

Upgrade Foundation concrete weapon veya Fireball sınıfını bilmez.

## 17. Error Handling

Programmer/configuration errors:

```text
null definition
invalid definition data
PlayerBuildController not initialized
PlayerStatsController not initialized
unexpected PlayerBuild result
```

Normal gameplay outcomes:

```text
AlreadyOwned
CapacityReached
```

## 18. Performance

```text
FindObjectsByType yok
LINQ zorunlu değil
runtime ScriptableObject mutation yok
modifier allocation yalnızca acquisition sırasında
```

## 19. Testing Strategy

### EditMode

```text
UpgradeDefinition category
modifier data validation
runtime modifier creation
source ID from UpgradeDefinition.StableId
multiple modifiers supported
```

### PlayMode

```text
successful acquisition adds ownership
successful acquisition applies modifiers
duplicate acquisition does not stack
capacity reached does not apply modifiers
event publishes once
real Player prefab contains controller
weapon/ability damage reflect acquired modifier
```

## 20. Implementation Stages

### Stage 1 — Upgrade Content Model

```text
UpgradeStatModifierData
UpgradeDefinition
EditMode tests
```

Changeset:

```text
feat: add upgrade content foundation
```

### Stage 2 — Upgrade Runtime Acquisition

```text
UpgradeAcquireResult
PlayerUpgradeController
PlayMode tests
Player prefab integration
```

Changeset:

```text
feat: add player upgrade acquisition runtime
```

### Stage 3 — Development Gameplay Integration

```text
development damage upgrade asset
development bootstrap
weapon damage validation
Fireball damage validation
duplicate/capacity validation
```

Changeset:

```text
feat: integrate development upgrade gameplay
```

### Stage 4 — Final Regression

```text
EditMode Run All
PlayMode Run All
manual Test_Waves validation
Console clean
upgrade-foundation → dev
```

## 21. Sonraki Aşama

En mantıklı sonraki üst sistem:

```text
reward-foundation
```

Reward/chest sistemi artık üç item kategorisini kullanabilir:

```text
Weapon
Ability
Upgrade
```

Ownership ve capacity kurallarını tekrar yazmak yerine mevcut PlayerBuild ve acquisition runtime'larına delegasyon yapmalıdır.
