# Weapon Switching

## 1. Amaç

`weapon-switching` branch'i, oyuncunun sahip olduğu birden fazla weapon arasında runtime state kaybetmeden geçiş yapmasını sağlar.

Mevcut acquisition katmanı iki weapon ownership'ini destekler; ancak `PlayerWeaponController` yalnızca tek aktif runtime yönetir. Bu branch'in hedefi ikinci weapon'ı gerçekten gameplay içinde kullanılabilir hale getirmektir.

```text
PlayerWeaponLoadout
        ↓
active runtime entry
        ↓
PlayerWeaponSwitcher
        ↓
PlayerWeaponController
        ↓
fire / ammo / reload
```

## 2. Ana Problem

Mevcut `PlayerWeaponController.Initialize(WeaponDefinition)` yeni bir `WeaponRuntimeState` oluşturur.

Bu yüzden her switch'te yeniden Initialize çağrılırsa ammo/reload/cooldown state'i resetlenir ve exploit oluşur.

```text
Weapon A magazine = 0
switch → B
switch → A
Initialize(A)
→ magazine yeniden full
```

Bu branch switching eklerken state-reset problemini de çözmelidir.

## 3. Tasarım Kararı

Her acquired weapon kendi kalıcı runtime state'ine sahip olur.

```text
PlayerWeaponRuntimeEntry
├── WeaponDefinition
└── WeaponRuntimeState
```

`PlayerWeaponLoadout` artık acquired weapon runtime entry'lerinin sahibi olur.

```text
PlayerWeaponLoadout
├── Entry A
│   ├── Definition A
│   └── RuntimeState A
└── Entry B
    ├── Definition B
    └── RuntimeState B
```

Switch sırasında yeni state oluşturulmaz; mevcut entry equip edilir.

## 4. Sorumluluk Ayrımı

### PlayerWeaponRuntimeEntry

Plain C# runtime object.

Sorumluluk:

```text
one acquired WeaponDefinition
+
its persistent WeaponRuntimeState
```

### PlayerWeaponLoadout

Sorumluluk:

```text
acquired runtime entries
active entry
acquisition order
lookup
active selection
```

Capacity hâlâ `PlayerBuild` tarafından yönetilir.

### PlayerWeaponController

Yalnızca active weapon combat orchestration:

```text
fire
reload
ammo
hitscan
damage evaluation
```

### PlayerWeaponSwitcher

Küçük Unity orchestration component'i:

```text
switch request
→ next loadout entry
→ active selection
→ PlayerWeaponController equip
```

## 5. Scope

```text
PlayerWeaponRuntimeEntry
PlayerWeaponLoadout runtime-state migration
PlayerWeaponController existing-state equip boundary
PlayerWeaponSwitcher
SwitchWeapon input action integration
first/second weapon switching
ammo/reload/cooldown state preservation
events
Player prefab integration
EditMode tests
PlayMode tests
Test_Waves development validation
```

## 6. Out of Scope

```text
weapon selection UI
weapon wheel
direct slot-number selection
drop/remove/replacement
world pickups
weapon level-up/evolution
rarity
weapon animation polish
save serialization
meta progression
```

## 7. PlayerWeaponRuntimeEntry

Önerilen minimum contract:

```text
PlayerWeaponRuntimeEntry
{
    WeaponDefinition Definition
    WeaponRuntimeState RuntimeState
}
```

Creation:

```text
definition.CreateRuntimeConfig()
→ new WeaponRuntimeState(config)
→ PlayerWeaponRuntimeEntry
```

## 8. PlayerWeaponLoadout Migration

Mevcut davranışlar mümkün olduğunca korunur:

```text
WeaponCount
HasActiveWeapon
ActiveDefinition
Contains(...)
```

Yeni runtime-facing API:

```text
ActiveEntry
Entries
GetEntry(...)
SetActive(...)
GetNextEntry()
```

Acquisition order switching order olarak kullanılır.

```text
[A, B]
A → Next → B
B → Next → A
```

Tek weapon varsa effective switch yapılmaz.

## 9. PlayerWeaponController Equip Boundary

Switching için açık bir boundary gerekir:

```text
Equip(PlayerWeaponRuntimeEntry entry)
```

Controller:

```text
_activeDefinition = entry.Definition
_runtimeState = entry.RuntimeState
```

şeklinde mevcut state'i kullanır ve yeni `WeaponRuntimeState` üretmez.

## 10. Acquisition Integration

İlk weapon:

```text
validate
→ PlayerBuild add
→ create/add runtime entry
→ set active
→ equip
```

İkinci weapon:

```text
validate
→ PlayerBuild add
→ create/add runtime entry
→ mevcut active entry korunur
```

İkinci acquisition aktif weapon'ı değiştirmez.

## 11. Switching Rules

```text
0 weapon → no-op
1 weapon  → no-op
2 weapons → A ↔ B
```

Normal switch yapılamaması exception değildir. Loadout/controller invariant ihlali programmer/configuration error'dır.

## 12. Input

Yeni Input System action:

```text
Gameplay/SwitchWeapon
```

Önerilen development bindings:

```text
Keyboard → Q
Gamepad  → Y / North button
```

Exact binding ve `PlayerInputReader` integration mevcut Input Actions asset/API incelendikten sonra yapılır; tahmin edilmez.

## 13. Events

Minimum event:

```text
ActiveWeaponChanged(
    previous WeaponDefinition,
    current WeaponDefinition)
```

Yalnızca active weapon gerçekten değiştiğinde publish edilir.

## 14. Player Death / Control Boundary

Switching mevcut gameplay control/death policy ile uyumlu olmalıdır. Dead/disabled player weapon değiştirmemelidir.

Exact integration mevcut player-control API incelendikten sonra yapılır.

## 15. Runtime State Preservation

Korunacak minimum state:

```text
Magazine ammo
Reserve ammo
Reload state
Reload remaining
Fire cooldown
```

Örnek:

```text
Weapon A
36 ammo → fire 6 → 30

switch B
switch A
→ ammo remains 30
```

## 16. Inactive Weapon Tick Policy

İlk sürüm:

```text
inactive weapon reload tick etmez
inactive weapon fire cooldown tick etmez
```

Yalnızca active runtime state `PlayerWeaponController.Update()` tarafından tick edilir.

## 17. Tests

### EditMode

```text
runtime entry creation/validation
entry order
active entry selection
round-robin next entry
single/empty loadout behavior
same-StableId different-instance protection
read-only exposure
runtime state identity preservation
ammo/reserve/reload/cooldown preservation
```

### PlayMode

```text
A → B switching
B → A switching
single weapon no-op
ActiveWeaponChanged once
second acquisition remains inactive
controller active definition matches loadout
fire after switch uses active entry state
player prefab contains switcher
```

## 18. Development Validation

`Test_Waves`:

```text
obtain two weapon definitions
fire A and reduce ammo
switch to B
use B
switch to A
A ammo remains reduced
reload state does not reset
keyboard switching works
Xbox controller switching works
wave/ability/reward systems unaffected
Console clean
```

## 19. Implementation Stages

### Stage 1 — Persistent Weapon Runtime Entry

```text
PlayerWeaponRuntimeEntry
PlayerWeaponLoadout migration
EditMode tests
```

Checkpoint:

```text
feat: add persistent weapon runtime entries
```

### Stage 2 — Controller Equip Boundary

```text
PlayerWeaponController Equip(...)
existing runtime state installation
state preservation tests
```

Checkpoint:

```text
feat: support equipping existing weapon runtime
```

### Stage 3 — Acquisition Migration

```text
PlayerWeaponAcquisitionController creates runtime entries
starting loadout remains valid
first auto-equip preserved
second weapon stays inactive
regression tests
```

Checkpoint:

```text
feat: integrate weapon acquisition with persistent runtime entries
```

### Stage 4 — Switching Runtime

```text
PlayerWeaponSwitcher
next-entry selection
ActiveWeaponChanged
tests
```

Checkpoint:

```text
feat: add player weapon switching runtime
```

### Stage 5 — Input + Prefab Integration

```text
SwitchWeapon Input Action
PlayerInputReader integration
Player prefab wiring
keyboard/controller validation
tests
```

Checkpoint:

```text
feat: integrate weapon switching input
```

### Stage 6 — Development Validation

Checkpoint yalnızca yeni development tooling/content eklenirse alınır.

### Stage 7 — Final Regression

```text
EditMode Run All
PlayMode Run All
manual Test_Waves
Console clean
weapon-switching → dev
```

## 20. Sonraki Sıra

Weapon Switching tamamlandığında:

```text
Reward Offer
→ Claim
→ second weapon acquired
→ switch between weapons
```

uçtan uca gerçek gameplay olur.

Sonraki branch:

```text
reward-selection-ui
```

Ardından:

```text
chest-foundation
```
