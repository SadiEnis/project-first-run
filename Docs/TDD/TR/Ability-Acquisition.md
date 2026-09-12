# Ability Acquisition

## Amaç
Bu branch, bir `AbilityDefinition` seçildiğinde ability'nin `PlayerBuild` ownership'ine eklenmesini ve doğru runtime entry'nin `PlayerAbilityController` içine kurulmasını sağlayan generic acquisition katmanını tanımlar.

```text
AbilityDefinition
      ↓
PlayerAbilityAcquisitionController
      ↓
PlayerBuildController
      ↓
AbilityRuntimeFactoryRegistry
      ↓
PlayerAbilityController
```

## Problem
Mevcut sistemde `PlayerAbilityController` runtime entry kabul ediyor ve Fireball için concrete runtime factory mevcut. Ancak üst seviye bir reward/chest sistemi henüz generic biçimde sadece `AbilityDefinition` vererek ability acquire edemiyor.

Hedef:

```text
Reward / Chest / Debug
        ↓
AbilityDefinition
        ↓
generic acquisition
```

## Scope
```text
IAbilityRuntimeFactory
AbilityRuntimeFactoryRegistry
AbilityAcquireResult
PlayerAbilityAcquisitionController
FireballAbilityRuntimeFactory generic contract integration
Player prefab integration
EditMode / PlayMode tests
development validation
```

## Out of Scope
```text
Reward claim UI
Chest interaction
Ability level-up
Ability evolution
Ability replacement/removal
Save/load
Meta progression
Rarity / weighted selection
```

## Ownership Source of Truth
Ownership ve capacity tekrar tutulmaz. `PlayerBuildController.TryAdd(...)` duplicate ve capacity için source of truth'tur.

Normal sonuçlar:
```text
Acquired
AlreadyOwned
CapacityReached
```

Factory bulunamaması veya invalid runtime output gibi durumlar configuration/programmer error olarak ele alınmalıdır; exact davranış mevcut factory API incelendikten sonra implementation sırasında kilitlenecektir.

## Runtime Factory Contract
`IAbilityRuntimeFactory` belirli bir `AbilityDefinition`ı destekleyip desteklemediğini söylemeli ve uygun `AbilityRuntimeEntry` oluşturmalıdır.

Factory şunları bilmez:
```text
PlayerBuild ownership
capacity
reward logic
UI
```

## Factory Registry
`AbilityRuntimeFactoryRegistry` definition için doğru factory'yi bulur.

```text
AbilityDefinition
      ↓
Registry
      ↓
matching factory
      ↓
AbilityRuntimeEntry
```

Concrete type switch kullanılmaz. Factory kendi support contract'ını sağlar.

## Fireball Integration
Mevcut `FireballAbilityRuntimeFactory` generic factory contract'ına adapte edilir. Fireball-specific bilgiler factory içinde kalır.

Acquisition controller şunları bilmez:
```text
FireballProjectile
EnemyRegistry
FireballAbilityExecutor
NearestEnemyTargetSelector
```

## Transaction Boundary
Hedef yarım state üretmemektir:

```text
ownership var, runtime yok   ✗
runtime var, ownership yok   ✗
```

Conceptual flow:
```text
1. definition validate
2. matching factory resolve
3. runtime creation preconditions validate
4. PlayerBuild.TryAdd
5. Added ise runtime entry install
6. AbilityAcquired event
```

Factory creation side-effect üretiyorsa exact sıra mevcut implementasyon incelendikten sonra belirlenmelidir.

## Event
```text
AbilityAcquired(AbilityDefinition)
```

## Dependency Direction
```text
Items / AbilityDefinition
        ↓
Ability Acquisition
        ↓
PlayerBuildController
PlayerAbilityController
AbilityRuntimeFactoryRegistry
        ↓
Concrete Ability Runtime Factories
```

Reward sistemi acquisition'a bağımlı olabilir; acquisition reward sistemine bağımlı olmaz.

## Tests
### EditMode
```text
factory registration
factory resolution
duplicate/ambiguous registration
unsupported definition
null validation
```

### PlayMode
```text
successful acquisition adds build ownership
successful acquisition adds runtime entry
duplicate does not add runtime twice
capacity reached does not add runtime
event publishes once
Fireball uses real factory
Player prefab contains acquisition controller
```

## Development Validation
`Test_Waves` içinde Fireball acquisition manuel olarak doğrulanır:

```text
Fireball initially unowned
acquire
PlayerBuild ownership added
PlayerAbilityController runtime added
Fireball auto-cast works
Console clean
```

## Implementation Stages
### Stage 1 — Factory Contract + Registry
```text
IAbilityRuntimeFactory
AbilityRuntimeFactoryRegistry
registry tests
```
Checkpoint:
```text
feat: add ability runtime factory registry
```

### Stage 2 — Acquisition Runtime
```text
AbilityAcquireResult
PlayerAbilityAcquisitionController
tests
```
Checkpoint:
```text
feat: add player ability acquisition runtime
```

### Stage 3 — Fireball Integration
```text
FireballAbilityRuntimeFactory generic contract
real acquisition path
Player prefab wiring
tests
```
Checkpoint:
```text
feat: integrate fireball ability acquisition
```

### Stage 4 — Development Validation
```text
Test_Waves manual validation
debug visibility if needed
```
Checkpoint:
```text
feat: add development ability acquisition validation
```

### Stage 5 — Final Regression
```text
EditMode Run All
PlayMode Run All
Test_Waves
Console clean
ability-acquisition → dev
```

## Sonraki Aşama
Muhtemel sonraki branch:
```text
weapon-acquisition
```

Ardından reward claim, Weapon / Ability / Upgrade acquisition yollarını concrete content bilgisi taşımadan orkestre edebilir.
