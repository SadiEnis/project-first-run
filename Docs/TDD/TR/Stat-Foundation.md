# Stat Foundation

## 1. Amaç

Stat Foundation, Project First Run'daki player-facing sayısal değerlerin upgrade, buff, debuff ve gelecekteki meta progression sistemleri tarafından güvenli ve tutarlı biçimde değiştirilebilmesi için ortak runtime altyapısını tanımlar.

Bu branch doğrudan Upgrade sistemini kurmaz. Önce Upgrade sisteminin üzerine oturacağı stat katmanını bağımsız olarak tamamlar.

Ana fikir:

```text
Domain Base Value
      ↓
PlayerStatCollection
      ↓ modifiers
Final Runtime Value
```

Base değerlerin ownership'i mevcut domain sistemlerinde kalır.

Örnek:

```text
WeaponDefinition.BaseDamage
        +
Player stat modifiers
        ↓
Final Runtime Damage
```

Stat sistemi weapon, ability veya movement verisinin source-of-truth'u olmayacaktır.

## 2. Temel Tasarım Kararı

Stat sistemi kendi içinde yeni base player value kopyaları tutmayacaktır. İlgili sistem kendi base değerini verir:

```text
stats.Evaluate(PlayerStatType.WeaponDamage, weaponBaseDamage)
```

Böylece aynı değer iki farklı yerde tutulmaz.

Örnek sahiplik:

```text
WeaponDefinition → base weapon damage
Concrete AbilityDefinition → base ability values
PlayerMotor / movement config → base movement values
Health config → base health value
PlayerStatCollection → yalnızca modifier'lar
```

## 3. Scope

```text
PlayerStatType
StatModifierOperation
StatModifier
PlayerStatCollection
PlayerStatsController
EditMode tests
PlayMode owner tests
ilk consumer integration noktaları
```

## 4. Out of Scope

```text
UpgradeDefinition
Upgrade acquisition
Chest / Reward
Upgrade UI
Upgrade level-up
Evolution
Meta progression
Save
Temporary timed buffs
Conditional perks
Kill-triggered perks
Complex proc systems
```

## 5. PlayerStatType

İlk sürümde yalnızca yakın gelecekte gerçekten kullanılacak stat türleri bulunur:

```text
WeaponDamage
AbilityDamage
MoveSpeed
WeaponFireRate
ReloadSpeed
AbilityCooldown
MaxHealth
Luck
```

Yeni stat yalnızca gerçek consumer ihtiyacı oluştuğunda eklenir.

## 6. Modifier Operations

Foundation üç operasyon destekler:

```text
Flat
AdditivePercent
MultiplicativePercent
```

Percent değerleri fractional format kullanır:

```text
+10% → 0.10
-20% → -0.20
```

Hesap sırası deterministiktir:

```text
base
+ flat toplamı
× (1 + additive percent toplamı)
× her multiplicative percent için (1 + value)
```

Örnek:

```text
Base = 100
Flat = +20
Additive = +25%
Multiplicative = +10%

(100 + 20) × 1.25 × 1.10 = 165
```

## 7. StatModifier

`StatModifier` immutable plain C# value object olmalıdır.

Minimum veri:

```text
PlayerStatType
StatModifierOperation
Value
SourceId
```

Örnek source ID:

```text
upgrade.glass_cannon
buff.arena_frenzy
meta.damage_01
```

Source ID daha sonra bir kaynağa ait modifier'ların topluca kaldırılmasını sağlar.

## 8. Source Identity

```text
null / empty source id → invalid
leading/trailing whitespace → invalid
comparison → ordinal, case-sensitive
```

Unity object reference modifier kimliği olarak kullanılmaz.

## 9. PlayerStatCollection

Plain C# runtime owner'dır.

Sorumlulukları:

```text
modifier eklemek
modifier kaldırmak
source'a göre kaldırmak
stat evaluate etmek
stat modifier'larını read-only sunmak
```

Şunlardan sorumlu değildir:

```text
PlayerBuild ownership
Upgrade acquisition
ScriptableObject loading
UI
Save
Ability execution
Weapon firing
Movement
Health lifecycle
```

## 10. Evaluation

Merkez API:

```text
float Evaluate(PlayerStatType statType, float baseValue)
```

`baseValue` finite olmalıdır. Sonuç NaN veya Infinity üretiyorsa configuration/programmer error kabul edilir.

Generic stat layer sonucu otomatik pozitif clamp etmez. Minimum fire rate gibi domain invariant'ları consumer sistemin sorumluluğundadır.

## 11. Duplicate Modifiers

Aynı source birden fazla modifier üretebilir:

```text
upgrade.example
├── WeaponDamage +20%
└── AbilityDamage +20%
```

Upgrade duplicate ownership daha sonra Upgrade Foundation tarafından yönetilir.

## 12. Removal

```text
Remove(modifier)
RemoveBySource(sourceId)
```

Olmayan modifier/source'u kaldırmak normal runtime sonucudur ve exception gerektirmez.

## 13. PlayerStatsController

Unity runtime owner:

```text
Player
└── PlayerStatsController
      └── PlayerStatCollection
```

Controller collection oluşturur ve erişim sağlar. Upgrade davranışlarını veya concrete effect'leri içine almaz.

## 14. Existing Systems Integration

### Weapon

```text
WeaponDefinition.BaseDamage
      ↓
Evaluate WeaponDamage
      ↓
DamageInfo
```

### Ability

```text
FireballDefinition.Damage
      ↓
Evaluate AbilityDamage
      ↓
FireballProjectile
```

### Ability Cooldown

Daha sonra:

```text
AbilityDefinition.Cooldown
      ↓
Evaluate AbilityCooldown
      ↓
AbilityRuntimeConfig
```

### Movement / Health

MoveSpeed ve MaxHealth integration gerçek Upgrade ihtiyacı geldiğinde ayrı checkpoint'te yapılabilir.

## 15. Dependency Direction

```text
Combat / Weapon / Ability / Player systems
            ↓
      Stat Foundation
```

Stat Foundation concrete weapon, Fireball, enemy veya upgrade sınıfını bilmez.

Upgrade Foundation daha sonra:

```text
UpgradeDefinition
       ↓
StatModifier
       ↓
PlayerStatCollection
```

şeklinde Stat Foundation'a bağımlı olacaktır.

## 16. Error Handling

Programmer/configuration errors:

```text
invalid source id
NaN / Infinity modifier value
NaN / Infinity base value
unsupported operation
non-finite evaluation result
```

Normal runtime outcomes:

```text
modifier bulunamadı
source için modifier yok
```

## 17. Performance

```text
hot path içinde FindObjectsByType yok
LINQ allocation yok
ScriptableObject runtime mutate edilmez
her frame yeni modifier allocation yapılmaz
```

Caching yalnızca profiling ihtiyaç gösterirse eklenir.

## 18. Testing Strategy

### EditMode

```text
modifier validation
flat evaluation
additive percent evaluation
multiplicative percent evaluation
mixed operation order
multiple modifiers
stat-type isolation
remove exact modifier
remove by source
invalid base value
non-finite result protection
read-only exposure
```

### PlayMode

```text
PlayerStatsController owns one collection
normal Unity lifecycle
real player prefab contains PlayerStatsController
consumer integration uses evaluated value
```

## 19. Implementation Stages

### Stage 1 — Pure Stat Domain

```text
PlayerStatType
StatModifierOperation
StatModifier
PlayerStatCollection
EditMode tests
```

Changeset:

```text
feat: add player stat runtime foundation
```

### Stage 2 — Unity Runtime Owner

```text
PlayerStatsController
Player prefab integration
PlayMode tests
```

Changeset:

```text
feat: add player stat runtime owner
```

### Stage 3 — Combat Consumer Integration

İlk doğrulama:

```text
WeaponDamage
AbilityDamage
```

Existing weapon ve Fireball base damage değerleri korunur.

Changeset:

```text
feat: integrate combat damage stats
```

### Stage 4 — Regression

```text
EditMode Run All
PlayMode Run All
manual weapon damage test
manual Fireball damage test
Console clean
stat-foundation → dev
```

## 20. Sonraki Branch

```text
upgrade-foundation
```

Upgrade Foundation şu parçaları Stat Foundation üzerine kurar:

```text
UpgradeDefinition
stat modifier data
PlayerBuild ownership
upgrade runtime application
duplicate/capacity rules
first concrete upgrade validation
```
