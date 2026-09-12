# Reward Selection UI

## Amaç

Mevcut bir `RewardOffer`’ı oyuncuya sunmak, oyuncunun teklif edilen gerçek item referanslarından birini seçmesini sağlamak, seçimi `PlayerRewardClaimController` üzerinden claim etmek ve modalı yalnızca claim tüketildiğinde kapatmak.

```text
RewardClaimSession
    ↓
RewardSelectionController
    ↔ RewardSelectionView
        └── RewardChoiceView[]
    ↓
PlayerRewardClaimController
```

UI reward state’i gösterir; offer generation, eligibility, acquisition veya claim kurallarını sahiplenmez.

## Mevcut Temeller

```text
RewardOffer                  immutable sunulan seçenekler
RewardClaimSession           authoritative claim state
PlayerRewardClaimController  category-independent claim orchestration
ItemDefinition               stable identity ve gösterim metadata’sı
```

Selection katmanı bu nesneleri doğrudan kullanmalı ve state’lerini tekrar oluşturmamalıdır.

## Temel Kararlar

### Tek Authoritative Claim Session

`RewardSelectionController` aktif `RewardClaimSession` referansını tutar. View yalnızca render için gereken offer verisini alır.

Controller ikinci bir selectable domain listesi oluşturmaz ve seçimleri StableId ile karşılaştırmaz.

### Tek Seçimli Modal

Bu branch offer başına tek başarılı seçimi destekler.

```text
kalan seçim       = 1
başarılı claim   = modalı kapat
başarısız claim = modalı açık tut
```

Legendary/Boss çoklu seçim semantiği daha sonraki chest/reward-session katmanına aittir. Kullanılmayan generic abstraction’larla önceden tahmin edilmeyecektir.

### Sabit ve Yeniden Kullanılabilir Choice View’lar

Prefab, planlanan en büyük normal reward ekranıyla uyumlu altı adet yeniden kullanılabilir `RewardChoiceView` slotu içerir.

Kullanılmayan slotlar gizlenir. Runtime her reward açıldığında choice nesneleri instantiate/destroy etmez.

## Kapsam

```text
ItemDefinition gameplay-effect gösterim metni
RewardChoiceView
RewardSelectionView
RewardSelectionController
tek seçimli claim akışı
modal zaman/kontrol yönetimi
Player/UI prefab composition
Input System UI event handling
EditMode ve PlayMode testleri
Test_Waves development entegrasyonu
```

## Kapsam Dışı

```text
chest spawn ve interaction
chest rarity ve renkleri
weighted chest tabloları
çoklu reward seçimi
item level-up ve evolution
reroll ve skip
gold fallback
final art, icon, animation, audio ve localization
capacity doluyken inventory replacement
```

## Gösterim Sözleşmesi

Her seçenek şunları gösterir:

```text
DisplayName
ItemCategory
GameplayEffect
NEW
```

Ekran ayrıca şunları gösterir:

```text
Bir Ödül Seç
Kalan seçim: 1
recoverable claim başarısızlığında feedback
```

Current/new level ve evolution state, ilgili domain sistemleri oluşana kadar doğru biçimde gösterilemez. View var olmayan runtime state uydurmamalıdır.

`ItemDefinition.GameplayEffect` statik presentation metadata’sıdır. Validation sırasında trim edilir ve gameplay hesabında kullanılmaz.

## RewardChoiceView

Sorumluluklar:

```text
tek bir exact ItemDefinition referansı bind etmek
display name/category/effect/state render etmek
Button tıklandığında seçimi yayınlamak
kullanılmayan slotu temizlemek ve gizlemek
```

Claim veya acquisition API’lerini çağırmaz.

Button listener bir kez kaydedilir. Rebind işlemi callback biriktirmemelidir.

## RewardSelectionView

Sorumluluklar:

```text
serialized referansları doğrulamak
RewardOffer choice’larını sabit slotlara render etmek
modal root’u göstermek/gizlemek
exact seçilen ItemDefinition’ı yayınlamak
recoverable feedback göstermek
klavye/gamepad navigation için ilk Button’ı seçmek
```

Boş offer açmak reddedilir; geçerli aksiyonu olmayan bir modal oyuncuyu soft-lock eder. Chest fallback davranışı chest katmanında eklenecektir.

## RewardSelectionController

Public akış:

```text
Open(RewardClaimSession)
    null, claimed ve empty session kontrolü
    önceki time scale/control state’i yakala
    gameplay’i duraklat
    view’ı render et ve göster

Select(ItemDefinition)
    PlayerRewardClaimController.TryClaim çağır
    Claimed         → kapat ve eski state’i geri yükle
    AlreadyClaimed  → kapat ve eski state’i geri yükle
    AlreadyOwned    → açık tut ve feedback göster
    CapacityReached → açık tut ve feedback göster
```

Configuration hataları exception olarak kalır.

## Modal Gameplay Politikası

Reward ekranı açıkken:

```text
Time.timeScale = 0
PlayerController gameplay input kapalı
PlayerController politikasıyla cursor açık ve görünür
UI input açık
```

Kapanışta veya component disable olduğunda, önceki state’in normal gameplay olduğu varsayılmadan yakalanan değerler geri yüklenir.

Zaten disabled/dead olan oyuncu reward ekranı kapandığında yeniden etkinleştirilmemelidir.

## Event’ler

Minimum controller event’leri:

```text
SelectionOpened(RewardClaimSession)
SelectionClosed(RewardClaimResult)
```

Event’ler gerçek transition başına bir kez yayınlanır.

## Prefab Composition

```text
RewardSelectionUI
├── Canvas
├── CanvasScaler
├── GraphicRaycaster
├── RewardSelectionView
├── RewardSelectionController
├── ModalRoot
│   ├── Header
│   ├── RemainingSelections
│   ├── Feedback
│   └── ChoiceContainer
│       └── RewardChoiceView x6
└── EventSystem
    └── InputSystemUIInputModule
```

UI prefab scene-owned olmalıdır. Player prefabına ait değildir.

## Development Entegrasyonu

`Test_Waves` development-only bir bridge kullanır:

```text
RewardDevelopmentController offer üretir
    ↓
RewardSelectionDevelopmentBootstrap session’ı açar
    ↓
oyuncu Development Secondary’yi claim eder
    ↓
modal kapanır ve gameplay devam eder
    ↓
Q / Gamepad North ile silah değiştirilir
```

Mevcut debug presenter tanılama aracı olarak kalabilir; normal seçim yeni UI üzerinden yapılır.

## Testler

### EditMode

```text
ItemDefinition effect metnini trim eder
choice binding exact definition’ı render eder
choice rebind callback biriktirmez
kullanılmayan choice temizlenir ve gizlenir
view yetersiz slot configuration’ı reddeder
view empty offer’ı reddeder
player/UI prefab composition
```

### PlayMode

```text
open tüm choice’ları gösterir
open zamanı durdurur ve player control’ü kapatır
başarılı button seçimi exact definition’ı claim eder
başarılı claim kapanır ve önceki state’i geri yükler
AlreadyOwned modalı açık tutar
CapacityReached modalı açık tutar
claimed session açılamaz
açıkken disable olmak önceki state’i geri yükler
keyboard/gamepad UI navigation choice seçer
development bridge Test_Waves offer’ını açar
```

## Uygulama Aşamaları

### Aşama 1 — Mimari ve Gösterim Metadata’sı

Checkpoint:

```text
docs: define reward selection UI architecture
```

### Aşama 2 — Yeniden Kullanılabilir Presentation View’ları

Checkpoint:

```text
feat: add reward selection presentation
```

### Aşama 3 — Claim ve Modal Orchestration

Checkpoint:

```text
feat: add reward selection orchestration
```

### Aşama 4 — UI Prefab Composition

Checkpoint:

```text
feat: add reward selection UI prefab
```

### Aşama 5 — Development Doğrulaması

Yalnızca development tooling/content değişirse checkpoint:

```text
test: integrate reward selection UI with Test_Waves
```

### Aşama 6 — Final Regresyon

```text
EditMode Run All
PlayMode Run All
manual Test_Waves
Console clean
reward-selection-ui → dev
```

## Sonraki Katman

```text
chest-foundation
```

Chest Foundation offer’ın ne zaman üretileceğine ve bu UI’ın ne zaman açılacağına karar verir. Chest lifecycle kurallarını view’a taşımak yerine mevcut selection/claim sınırını yeniden kullanmalıdır.
