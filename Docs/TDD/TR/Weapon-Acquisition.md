# Weapon Acquisition

## 1. Amaç

Weapon Acquisition branch'i, bir `WeaponDefinition` seçildiğinde silahın oyuncunun run-scoped build ownership'ine eklenmesini, runtime loadout içinde saklanmasını ve gerekliyse aktif silah olarak `PlayerWeaponController` içine kurulmasını sağlayan generic acquisition katmanını tanımlar.

Hedef, reward/chest gibi üst seviye sistemlerin concrete weapon türlerini veya `PlayerWeaponController.Initialize(...)` detaylarını bilmeden silah kazandırabilmesidir.

```text
WeaponDefinition
      ↓
PlayerWeaponAcquisitionController
      ↓
PlayerBuildController
      ↓
PlayerWeaponLoadout
      ↓
PlayerWeaponController
```

## 2. Mevcut Runtime Gerçeği

Mevcut `PlayerWeaponController` yalnızca tek bir aktif weapon runtime state tutar:

```text
_activeDefinition
_runtimeState
```

`Initialize(WeaponDefinition)` çağrısı aktif silahı ve runtime state'i yeniden oluşturur.

Buna karşılık `PlayerBuildCapacity` bir oyuncunun ileride en fazla iki weapon ownership'ine sahip olabilmesini destekler.

Bu nedenle acquisition sistemi yalnızca `PlayerWeaponController.Initialize(...)` çağırarak tasarlanamaz.

Aksi halde ikinci weapon acquire edildiğinde:

```text
Weapon A active
↓ acquire Weapon B
Initialize(B)
↓
Weapon A runtime bilgisi kaybolur
```

ve oyuncunun sahip olduğu iki silahı temsil eden bir runtime loadout katmanı bulunmaz.

## 3. Temel Mimari Karar

Weapon ownership ile active weapon state ayrılır.

```text
PlayerBuild
→ ownership + capacity source of truth

PlayerWeaponLoadout
→ acquired WeaponDefinition runtime references
→ active weapon selection state

PlayerWeaponController
→ yalnızca aktif weapon'ın firing/ammo/reload runtime'ı
```

`PlayerWeaponLoadout` ikinci bir ownership sistemi değildir. Ownership kararı yine `PlayerBuildController` tarafından verilir. Loadout yalnızca başarılı acquisition sonrası runtime için gerekli concrete definition referanslarını tutar.

## 4. Scope

Bu branch:

```text
WeaponAcquireResult
PlayerWeaponLoadout
PlayerWeaponAcquisitionController
first-weapon auto-equip
second-weapon ownership + loadout storage
active weapon runtime initialization
existing starting weapon flow integration
Player prefab integration
EditMode / PlayMode tests
development validation
```

## 5. Out of Scope

```text
weapon switch input
weapon wheel UI
next/previous weapon controls
weapon drop/remove
weapon replacement UI
ammo persistence between weapon switches
per-weapon preserved magazine/reserve runtime state
weapon level-up
weapon evolution
save/load
meta progression
reward claim UI
```

Weapon switching bu branch'te input ile uygulanmaz. Branch yalnızca switching'in ileride kurulabilmesi için loadout state'i sağlar.

## 6. PlayerWeaponLoadout

`PlayerWeaponLoadout` pure C# runtime object'tir.

Minimum sorumluluklar:

```text
acquired WeaponDefinition referanslarını acquisition order ile saklamak
read-only erişim sağlamak
active definition bilgisini tutmak
duplicate runtime installation'ı engellemek
```

Capacity kararı burada verilmez. `AlreadyOwned` ve `CapacityReached` kuralları `PlayerBuild` sorumluluğudur.

## 7. Acquisition Semantics

Entry point:

```text
PlayerWeaponAcquisitionController.TryAcquire(WeaponDefinition)
```

Normal sonuçlar:

```text
Acquired
AlreadyOwned
CapacityReached
```

İlk weapon acquire edildiğinde:

```text
PlayerBuild add
→ PlayerWeaponLoadout add
→ active weapon yoksa auto-equip
→ PlayerWeaponController.Initialize(definition)
```

İkinci weapon acquire edildiğinde:

```text
PlayerBuild add
→ PlayerWeaponLoadout add
→ mevcut active weapon korunur
```

Acquisition ikinci weapon'ı otomatik olarak active weapon'ın üzerine yazmaz.

## 8. Active Weapon / Equip Boundary

`PlayerWeaponController` yalnızca aktif weapon runtime'ını yürütmeye devam eder.

Loadout tarafında explicit active selection API bulunabilir:

```text
SetActive / TrySetActive
```

Ancak bu branch'te gameplay input bağlanmaz.

İleride ayrı weapon switching katmanı:

```text
input
→ weapon selection
→ loadout active definition
→ PlayerWeaponController.Initialize(selected)
```

akışını kullanabilir.

## 9. Runtime State Reset Semantiği

Mevcut `PlayerWeaponController.Initialize(...)` yeni `WeaponRuntimeState` oluşturur.

Dolayısıyla weapon değiştirme şu anda:

```text
magazine/reserve state reset
```

anlamına gelir.

Bu branch weapon switching uygulamadığı için bu davranışı değiştirmez.

## 10. Transaction Boundary

Acquisition yarım state üretmemelidir.

İstenen invariant:

```text
PlayerBuild owns weapon
⇔ successful runtime loadout installation
```

Conceptual flow:

```text
1. definition null/stableId validate
2. runtime config oluşturulabilir mi validate
3. PlayerBuild preflight ownership/capacity
4. PlayerBuild.TryAdd(definition)
5. PlayerWeaponLoadout add
6. first weapon ise active/equip
7. WeaponAcquired event
```

`WeaponDefinition.CreateRuntimeConfig()` ownership mutation öncesinde çağrılarak malformed weapon content mümkün olduğunca erken yakalanmalıdır.

## 11. PlayerWeaponController Değişiklik Sınırı

`PlayerWeaponController` acquisition ownership bilmez.

Şu sorumlulukları korur:

```text
active WeaponDefinition
WeaponRuntimeState
fire
reload
ammo
hitscan resolution
WeaponDamage stat evaluation
```

Mevcut `Initialize(WeaponDefinition)` API'si aktif runtime installation sınırı olarak kullanılmaya devam eder.

## 12. Starting Loadout Integration

Mevcut starting weapon initialization akışı doğrudan `PlayerBuildController` ve `PlayerWeaponController.Initialize(...)` çağırıyorsa yeni acquisition/loadout path'ine adapte edilmelidir.

Exact refactor mevcut `PlayerStartingLoadoutInitializer` kodu görülmeden yapılmamalıdır.

Hedef:

```text
starting loadout
reward claim
development tooling
        ↓
PlayerWeaponAcquisitionController
```

## 13. Events

Minimum event:

```text
WeaponAcquired(WeaponDefinition)
```

`ActiveWeaponChanged` event'i ancak switching ihtiyacı geldiğinde loadout seviyesinde eklenir.

## 14. Dependency Direction

```text
WeaponDefinition
      ↓
PlayerWeaponAcquisitionController
      ↓
PlayerBuildController
PlayerWeaponLoadout
PlayerWeaponController
```

Reward sistemi acquisition'a bağımlı olabilir; weapon acquisition Reward/Chest/UI katmanlarını bilmez.

## 15. Error Handling

Programmer/configuration errors:

```text
null definition
invalid StableId
invalid WeaponRuntimeConfig
missing dependencies
loadout/build invariant violation
unexpected PlayerBuildAddResult
```

Normal runtime outcomes:

```text
AlreadyOwned
CapacityReached
Acquired
```

## 16. Tests

### EditMode — PlayerWeaponLoadout

```text
new loadout empty
add weapon preserves order
read-only collection
duplicate add rejected
active weapon initially null
active weapon must belong to loadout
null validation
```

### PlayMode — Acquisition

```text
first acquisition adds PlayerBuild ownership
first acquisition adds loadout entry
first acquisition initializes active PlayerWeaponController
second acquisition adds ownership/loadout
second acquisition does not replace active weapon
duplicate does not add twice
capacity reached does not mutate loadout/controller
event publishes once
invalid definition does not mutate ownership
Player prefab contains acquisition/loadout runtime
```

## 17. Development Validation

`Test_Waves` içinde:

```text
starting weapon acquisition normal
weapon fires/reloads normally
PlayerBuild weapon ownership correct
active weapon remains correct
second development weapon varsa acquire edilebilir
second weapon acquire active weapon'ı değiştirmez
Console clean
```

İkinci gerçek weapon content yoksa sırf manuel test için yapay production content oluşturulmaz.

## 18. Implementation Stages

### Stage 1 — Runtime Loadout

```text
PlayerWeaponLoadout
EditMode tests
```

Checkpoint:

```text
feat: add player weapon runtime loadout
```

### Stage 2 — Acquisition Runtime

```text
WeaponAcquireResult
PlayerWeaponAcquisitionController
PlayMode tests
```

Checkpoint:

```text
feat: add player weapon acquisition runtime
```

### Stage 3 — Existing Flow Integration

```text
starting loadout integration
Player prefab wiring
existing weapon test adaptation
```

Checkpoint:

```text
feat: integrate weapon acquisition with player loadout
```

### Stage 4 — Development Validation

```text
Test_Waves manual validation
debug visibility only if useful
```

Checkpoint only if new development code is required.

### Stage 5 — Final Regression

```text
EditMode Run All
PlayMode Run All
Test_Waves
Console clean
weapon-acquisition → dev
```

## 19. Sonraki Aşama

Weapon acquisition tamamlandıktan sonra üç category için acquisition yolu bulunmuş olur:

```text
Weapon
→ PlayerWeaponAcquisitionController

Ability
→ PlayerAbilityAcquisitionController

Upgrade
→ PlayerUpgradeController
```

Bundan sonra `reward-claim` branch'i seçilmiş `ItemDefinition`ı category-specific acquisition path'e yönlendiren üst seviye orchestration kurabilir.
