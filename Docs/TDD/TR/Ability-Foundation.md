# Ability Foundation

## 1. Amaç

Ability Foundation, oyuncunun sahip olduğu otomatik kullanılan yeteneklerin ortak runtime altyapısını tanımlar.

Bu sistemin hedefi, Vampire Survivors benzeri otomatik ability kullanımını Project First Run'ın mevcut FPS combat yapısıyla birleştirmektir.

İlk concrete ability Fireball olacaktır; ancak bu branch yalnızca Fireball'a özel bir sistem kurmayacaktır. Fireball, generic ability runtime'ın ilk gerçek kullanımı olacaktır.

Temel akış:

```text
PlayerBuild
    ↓ ownership
Ability Runtime
    ↓ cooldown
Target Selection
    ↓
Auto Cast
    ↓
Ability Behaviour
    ↓
Damage / Effect
```

## 2. Tasarım Hedefleri

Ability sistemi:

1. Ability verisini runtime state'ten ayırmalıdır.
2. Ability ownership bilgisini PlayerBuild'den alabilmelidir.
3. Otomatik kullanım için cooldown tabanlı bir runtime model sağlamalıdır.
4. Target seçimini ability davranışından ayırmalıdır.
5. Ability davranışlarının birbirinden farklı olabilmesine izin vermelidir.
6. Fireball, Drone, Lightning Staff, Shuriken ve diğer gelecekteki ability'leri aynı foundation üzerinde destekleyebilmelidir.
7. AbilityController benzeri tek bir monolitik sınıfa bütün sorumlulukları yüklememelidir.
8. Unity lifecycle gerektirmeyen kuralları plain C# sınıflarında tutmalıdır.
9. Combat foundation'daki `DamageInfo`, `DamageResult` ve `IDamageable` sistemleriyle uyumlu olmalıdır.
10. İleride stat modifier, level-up ve evolution sistemlerinin eklenmesine uygun olmalıdır.

## 3. Scope

Bu branch'in hedefi generic ability runtime foundation ve ilk gerçek entegrasyon için gerekli altyapıdır.

Planlanan ana parçalar:

```text
AbilityDefinition
AbilityRuntimeConfig
AbilityRuntimeState
PlayerAbilityController
Ability target selection contract
Auto-cast orchestration
Ability execution contract
```

İlk concrete doğrulama Fireball ile yapılacaktır.

## 4. Out of Scope

Bu branch kapsamında aşağıdakiler tamamlanmayacaktır:

```text
Chest / Reward sistemi
Ability reward selection
Ability level-up sistemi
Evolution sistemi
Global stat modifier sistemi
Meta progression
Save sistemi
Ability UI
Cooldown UI
Full VFX / SFX polish
Object pooling
Addressables integration
Boss-specific ability rules
```

Fireball yalnızca foundation'ın gerçek gameplay doğrulaması için minimum concrete implementation olarak kullanılacaktır.

## 5. Ability Definition

Ability'ler data-driven olmalıdır.

`AbilityDefinition`, mevcut `ItemDefinition` yapısından türemelidir.

```text
ItemDefinition
    ↓
AbilityDefinition
```

`AbilityDefinition.Category` değeri `ItemCategory.Ability` olacaktır.

Foundation aşamasında definition en az şu verileri taşımalıdır:

```text
Stable ID
Display Name
Cooldown
```

Concrete ability'ye özel alanlar generic definition'a doldurulmamalıdır. Örneğin Fireball daha sonra damage, projectile speed, lifetime veya projectile prefab gibi kendi verilerine ihtiyaç duyabilir.

## 6. Definition ve Runtime State Ayrımı

ScriptableObject definition yalnızca konfigürasyon verisidir. Runtime sırasında değişen state ayrı tutulmalıdır.

```text
AbilityDefinition
├── stableId
├── displayName
└── cooldown

AbilityRuntimeState
├── cooldown remaining
└── ready state
```

Runtime sırasında ScriptableObject üzerindeki veriler mutate edilmez.

## 7. Ability Runtime Config

Definition'dan saf runtime konfigürasyonu üretilebilmelidir:

```text
AbilityDefinition
      ↓
CreateRuntimeConfig()
      ↓
AbilityRuntimeConfig
```

`AbilityRuntimeConfig` immutable olmalıdır. İlk sürümde en az cooldown bilgisini taşır.

## 8. Ability Runtime State

`AbilityRuntimeState` plain C# sınıfı olmalıdır.

Sorumlulukları:

```text
Cooldown takibi
Ready kontrolü
Cast commit
Cooldown reset
```

Akış:

```text
Ready
  ↓ successful cast
CoolingDown
  ↓ Tick(deltaTime)
Ready
```

Normal cast isteği sonucu explicit bir sonuçla ifade edilebilir:

```text
Performed
OnCooldown
```

Target bulunamaması runtime cooldown state'in sorumluluğu değildir.

## 9. Ownership

Ability ownership'ın source-of-truth'u `PlayerBuild` olacaktır.

```text
PlayerBuild
└── ability.fireball
```

Ability runtime ise aktif gameplay state'ini yönetir.

```text
PlayerBuild
→ Oyuncu hangi ability'ye sahip?

Ability Runtime
→ Ability şu an kullanılabilir mi?
→ Cooldown ne kadar?
→ Nasıl execute edilir?
```

Ability runtime PlayerBuild koleksiyonlarını değiştirmez.

## 10. Runtime Ownership

Oyuncuya ait aktif ability runtime state'lerinin tek bir runtime sahibi olmalıdır.

İlk hedef yapı:

```text
Player
└── PlayerAbilityController
      ├── Ability Runtime Entry
      ├── Ability Runtime Entry
      └── Ability Runtime Entry
```

Controller'ın görevi concrete ability davranışlarını içine yığmak değildir. Runtime entry'leri tutar, tick eder ve cast orchestration'ını yürütür.

## 11. Ability Runtime Entry

Her aktif ability runtime entry şu ilişkiyi temsil eder:

```text
Ability Definition
+
Ability Runtime State
+
Ability Execution Behaviour
```

İleride item progression geldiğinde level bilgisi eklenebilir. Foundation aşamasında level eklenmeyecektir.

## 12. Auto-Cast

Ability'ler manuel input'a bağlı olmayacaktır.

```text
Ability ready?
      ↓ yes
Target gerekli mi?
      ↓
Target Selection
      ↓
Valid target bulundu mu?
      ↓ yes
Execute
      ↓
Commit cooldown
```

Önemli kural:

```text
No target
→ No cast
→ No cooldown reset
```

Cooldown yalnızca başarılı execution sonrasında başlatılır.

## 13. Target Selection

Target seçimi ability execution'dan ayrılmalıdır. Bütün ability'ler aynı targeting kuralına sahip olmak zorunda değildir.

```text
Fireball       → nearest valid enemy
Lightning      → one or more valid enemies
Shuriken Ring  → target gerektirmeyebilir
Drone          → kendi targeting davranışına sahip olabilir
```

İlk Fireball entegrasyonu nearest-enemy selector kullanabilir.

## 14. Enemy Kaynağı

Target selector mevcut `EnemyRegistry` üzerinden aktif enemy adaylarını okuyabilir.

```text
EnemyRegistry
    ↓ active candidates
Target Selector
    ↓ selected target
Ability Execution
```

Ability sistemi registry'yi değiştirmez. Ölü veya registry'den çıkmış enemy target seçilmemelidir.

## 15. Target Selection Contract

Target selection küçük ve açık bir contract kullanmalıdır.

Temel sorumluluk:

```text
Verilen origin için uygun target bul
```

Target selector cooldown yönetmez, damage uygulamaz, projectile spawn etmez ve ownership yönetmez.

## 16. Ability Execution

Concrete ability behaviour ayrı execution implementation'ları üzerinden genişletilmelidir.

Kaçınılacak yapı:

```text
switch (abilityType)
{
    case Fireball:
    case Drone:
    case Lightning:
    ...
}
```

Tercih edilen yapı:

```text
FireballAbilityExecutor
LightningAbilityExecutor
ShurikenAbilityExecutor
```

Orchestration katmanı concrete execution detayını bilmemelidir.

## 17. Fireball İlk Concrete Ability

İlk gerçek doğrulama Fireball olacaktır.

```text
Cooldown hazır
    ↓
Nearest enemy seç
    ↓
Projectile spawn
    ↓
Projectile target yönüne ilerler
    ↓
Enemy hit
    ↓
IDamageable.ApplyDamage
```

Bu branch'teki amacı content polish değil, generic ability pipeline'ın çalıştığını kanıtlamaktır.

## 18. Fireball ve Projectile Ownership

Projectile behaviour ability controller içine yazılmamalıdır.

```text
PlayerAbilityController
    ↓ cast orchestration

Fireball Executor
    ↓ spawn

Fireball Projectile
    ↓ movement / hit

IDamageable
    ↓ damage
```

Projectile movement, hit ve lifetime sorumlulukları ayrı tutulur.

## 19. Cooldown Semantics

İlk sürümde cooldown:

```text
seconds between successful casts
```

olarak yorumlanır.

Cooldown yalnızca başarılı cast sonrasında resetlenir. `CooldownRemaining >= 0` invariant'ı korunur ve negatif delta time kabul edilmez.

## 20. Player Death

Player öldüğünde ability auto-cast devam etmemelidir.

Hedef entegrasyon:

```text
PlayerDeathController
      ↓
PlayerAbilityController.SetAbilityControlEnabled(false)
```

Revive/reset mevcut player lifecycle kurallarıyla uyumlu olmalıdır.

## 21. Arena ve Wave Sistemleri

Ability sistemi wave progression'ın sahibi değildir.

```text
Ability
→ Enemy Health
→ EnemyController.Died
→ WaveEnemyTracker
→ Wave progression
```

Wave sistemi ability'ye özel bilgiye ihtiyaç duymamalıdır.

## 22. PlayerBuild Integration

Gelecekte reward akışı yaklaşık şöyle olacaktır:

```text
Reward
    ↓
PlayerBuild.TryAdd(AbilityDefinition)
    ↓ Added
PlayerAbilityController.AddAbility(...)
```

Bu branch'te chest/reward orchestration yazılmayacaktır.

Fireball development entegrasyonu için starting/test ability development configuration üzerinden sağlanabilir.

## 23. Error Handling

Configuration/programmer hataları exception veya validation error olabilir:

```text
Null definition
Invalid cooldown
Missing execution behaviour
Missing projectile prefab
Invalid runtime dependency
```

Normal gameplay sonuçları exception olmamalıdır:

```text
Ability on cooldown
No valid target
Cast not performed
```

## 24. Performance İlkeleri

```text
Her frame FindObjectsOfType kullanılmaz.
Target candidate kaynağı olarak EnemyRegistry kullanılır.
Hot path'lerde gereksiz LINQ allocation'larından kaçınılır.
Definition assetleri runtime sırasında mutate edilmez.
```

Object pooling bu branch'in scope'u dışındadır. İlk Fireball `Instantiate/Destroy` kullanabilir.

## 25. Testing Strategy

### EditMode

```text
AbilityRuntimeConfig validation
AbilityRuntimeState initial state
Cooldown tick
Successful cast commit
OnCooldown result
Cooldown completion
Invalid delta time
Target selector rules where practical
```

### PlayMode

```text
Player ability runtime initializes
Auto-cast occurs
No target → no cast
Nearest enemy selected
Projectile spawns
Projectile damages enemy
Ability kill participates in wave progression
Player death disables auto-cast
```

## 26. Architecture

```text
PlayerBuild
    ↓ ownership

PlayerAbilityController
    ↓ runtime orchestration

AbilityRuntimeState
    ↓ cooldown

Target Selector
    ↓ target

Ability Executor
    ↓ concrete behaviour

Projectile / Effect
    ↓

Combat Foundation
    ↓

Enemy
```

Hiçbir sınıf bütün pipeline'ın sorumluluğunu tek başına taşımamalıdır.

## 27. Implementation Stages

### Stage 1 — Ability Domain Foundation

```text
AbilityDefinition
AbilityRuntimeConfig
AbilityRuntimeState
Cast result
EditMode tests
```

Suggested changeset:

```text
feat: add ability runtime foundation
```

### Stage 2 — Targeting Foundation

```text
Target selection contract
Nearest enemy selector
EnemyRegistry integration
Tests
```

Suggested changeset:

```text
feat: add ability target selection
```

### Stage 3 — Auto-Cast Runtime

```text
PlayerAbilityController
Ability runtime entries
Execution contract
Auto-cast orchestration
PlayMode tests
```

Suggested changeset:

```text
feat: add automatic ability runtime
```

### Stage 4 — Fireball Integration

```text
Fireball definition
Fireball executor
Projectile behaviour
Player / development integration
Manual scene validation
```

Suggested changeset:

```text
feat: add fireball ability integration
```

### Stage 5 — Regression and Merge

```text
EditMode Run All
PlayMode Run All
Manual combat validation
Console clean
ability-foundation → dev
```

## 28. Gelecek Sistemlerle İlişki

Ability Foundation daha sonra şu sistemlerin tabanı olacaktır:

```text
Item Progression
Stat / Upgrade System
Chest / Reward System
Evolution System
More Ability Content
```

Evolution sistemi yalnızca sayı artırmak zorunda olmayacaktır. Concrete execution davranışını değiştirebilmelidir.

```text
Fireball
    ↓ evolution
Meteor Rain
```

örneğinde tamamen farklı execution behaviour kullanılabilir.
