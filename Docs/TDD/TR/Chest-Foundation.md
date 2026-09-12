# Sandık Temeli

## Amaç

Mevcut reward offer, claim ve selection sistemlerini dünyaya ait bir sandık
akışına dönüştürmek.

```text
Sandık spawn kaynağı
    ↓
ChestSpawner
    ↓
ChestController (Available)
    ← PlayerChestInteractor
    ↓
RewardOfferGenerator
    ↓
RewardClaimSession
    ↓
RewardSelectionController
    ↓ başarılı claim
ChestController (Opened)
```

Chest Foundation, offer'ın ne zaman üretileceğine ve reward UI'ın ne zaman
açılacağına karar verir. Offer, eligibility, claim veya acquisition kurallarını
kopyalamaz.

## Mevcut Temeller

```text
RewardItemPool              data-driven aday kaynağı
RewardOfferGenerator        eligibility ve benzersiz offer üretimi
RewardClaimSession          authoritative claim state
RewardSelectionController   tek seçimli modal ve claim akışı
PlayerBuildController       mevcut ownership ve capacity kaynağı
PlayerInputReader           gameplay input sınırı
```

## Temel Kararlar

### Sandığı Claim Tamamlar

Selection UI'ın açılması sandığı tüketmez.

```text
Available
    ↓ başarılı etkileşim ve boş olmayan offer
Selecting
    ↓ RewardSelectionController.SelectionClosed
Opened
```

Mevcut tek seçimli session'ı yalnızca `Claimed` veya `AlreadyClaimed`
tamamlayabilir. Recoverable claim başarısızlıklarında selection UI ve sandık
session'ı aktif kalır.

### Tek Runtime State Sahibi

`ChestState`, tek bir `ChestController` tarafından sahiplenilen saf C# nesnesidir.

View sunumu gizleyebilir ve interactor availability durumunu gözlemleyebilir;
ancak ikisi de lifecycle state'in rakip bir kopyasını tutmaz.

### Definition Tabanlı Sandık İçeriği

`ChestDefinition` immutable tasarım verisidir. Foundation için yalnızca şunlar
gereklidir:

```text
StableId
RewardItemPool
RequestedChoiceCount
WorldPrefab
```

Definition mutable offer veya claim state içermez.

Rarity, renkler, evolution ihtimali, multi-selection sayısı ve gold değeri
foundation içinde tahmin edilmez. Domain kuralları oluştuğunda definition'ı
genişleteceklerdir.

### Açık FPS Etkileşimi

Oyuncu configured mesafe içindeki available sandığa bakar ve Interact aksiyonuna
basar.

```text
Klavye / Mouse  E
Gamepad          Button South
```

`PlayerChestInteractor` raycast'in sahibidir ve odaktaki gerçek
`ChestController`'ı çağırır. Sandıklar sahneyi taramaz veya player input'u ayrı
ayrı poll etmez.

### Boş Offer Modal Açmaz

Mevcut reward katmanı, oyuncu için eligible item kalmadığında meşru biçimde boş
offer döndürebilir.

Gold fallback oluşana kadar:

```text
boş offer
    → NoEligibleRewards döndür
    → sandığı Available tut
    → RewardSelectionController'ı açma
```

Bu yaklaşım hem modal soft-lock'u hem de reward'ın sessizce kaybolmasını engeller.
Sonraki fallback katmanı selection UI'ı değiştirmeden bu sonucu gold ile
değiştirebilir.

## Kapsam

```text
ChestDefinition
ChestStatus ve ChestState
ChestOpenResult
ChestController
ChestSpawnRequest ve ChestSpawnResult
ChestSpawner
PlayerChestInteractor
Interact input aksiyonu
foundation sandık sunumu/prefab'ı
Test_Waves development entegrasyonu
EditMode ve PlayMode testleri
```

## Kapsam Dışı

```text
weighted chest-type tabloları
normal veya elite enemy drop ihtimalleri
experience ve level-up spawning
boss ve final-boss spawning kuralları
rarity ve renk politikası
item level ve evolution
Legendary/Boss multi-selection
gold fallback ve Golden Chest davranışı
inventory replacement
opening animasyonu, final art, ses, VFX ve localization
save/meta progression
interaction prompt UI
```

Spawn kaynakları daha sonra `ChestSpawner`'ı çağırabilir. Bu kaynaklar sandık
lifecycle controller'ın içine ait değildir.

## ChestDefinition

`ChestDefinition` bir `ScriptableObject` content asset'idir.

Validation şunları reddeder:

```text
eksik veya trim edilmemiş StableId
eksik RewardItemPool
RequestedChoiceCount <= 0
eksik WorldPrefab
```

İstenen sayı eligible aday sayısını aşabilir; mevcut offer generator zaten tüm
available eligible seçenekleri döndürür.

## ChestState

```text
ChestStatus.Available
ChestStatus.Selecting
ChestStatus.Opened
```

Geçerli geçişler:

```text
Available → Selecting
Selecting → Available   yalnızca UI ownership öncesi opening orchestration bozulursa
Selecting → Opened      claim session tüketildikten sonra
```

Geçersiz geçişler reddedilir. Tekrarlanan completion, duplicate open event
yayınlayamaz.

## ChestController

Sorumlulukları:

```text
ChestState'e sahip olmak
kabul edilen open attempt için tek offer üretmek
tek RewardClaimSession oluşturmak
RewardSelectionController'ı açmak
selection completion'ı gözlemlemek
presentation'ı tam bir kez tamamlamak
ChestOpened yayınlamak
```

Yapmadıkları:

```text
input okumak
raycast yapmak
reward adaylarını kendi seçmek
item claim etmek
drop ihtimallerine karar vermek
claim tamamlanmadan kendini yok etmek
```

Open sonuçları:

```text
SelectionOpened
AlreadySelecting
AlreadyOpened
SelectionBusy
NoEligibleRewards
```

Configuration hataları exception olarak kalır.

## Spawn

`ChestSpawner`, validated definition'daki prefab'ı instantiate eder ve sahneye ait
dependency'leri inject eder.

```text
ChestSpawnRequest
├── ChestDefinition
├── Position
└── Rotation

ChestSpawnResult
├── ChestController
└── SpawnedObject
```

Spawner hangi chest definition'ın düşeceğine karar vermez. Bu kararı gelecekteki
level-up, enemy-drop veya boss-reward kaynağından alır.

## Etkileşim

`PlayerChestInteractor`, açık bir origin'den tek forward raycast kullanır.

```text
Interact basıldı
    ↓
InteractionDistance içinde raycast
    ↓
hit nesnesinden/parent'tan ChestController çöz
    ↓
TryOpen
```

Raycast serialized layer mask kullanır ve dünya occlusion'ına uyar. Başarılı
etkileşimin hemen ardından reward modal gameplay input'u kapattığı için aynı input
sandığı art arda açamaz.

## Sunum

Foundation prefab'ı collider ve odaklı bir `ChestView` ile tanınabilir placeholder
dünya nesnesi sağlar.

Tamamlandığında view interaction collision'ı kapatır ve visual root'u gizler.
Controller nesnesi event'i yayınlamak ve authoritative Opened state'i korumak için
yaşamaya devam edebilir. Pooling veya destruction politikası gerçek chest spawn
ekonomisi oluşana kadar ertelenir.

## Dependency Yönü

```text
Player Input
    ↓
PlayerChestInteractor
    ↓
ChestController
    ↓
Reward Foundation / Claim / Selection UI
```

Reward ve UI katmanları Chests'e bağımlı olmaz. Wave, enemy, level-up ve boss
sistemleri daha sonra spawning sınırına bağımlı olabilir; Chests bu kaynaklara
bağımlı olmaz.

## Testler

### EditMode

```text
ChestDefinition validation
ChestState geçerli geçişler
ChestState geçersiz/tekrarlanan geçişler
ChestSpawnRequest validation
chest prefab composition
player prefab interaction composition
Test_Waves scene wiring
```

### PlayMode

```text
geçerli open gerçek offer/session oluşturur ve UI'ı açar
başarılı seçim sandığı tam bir kez Opened yapar
boş offer sandığı Available ve UI'ı kapalı bırakır
busy selection başka sandığı Available bırakır
tekrarlanan etkileşim ikinci aktif session üretmez
spawner dependency'leri inject eder ve transform'u korur
interactor bakılan gerçek sandığı açar
menzil dışındaki veya occluded sandık açılmaz
disabled/dead gameplay input etkileşemez
```

## Development Doğrulaması

`Test_Waves`, otomatik reward popup yerine tek development chest kullanır.

```text
gameplay başlat
    ↓
sandığa bak ve E / Gamepad South'a bas
    ↓
reward selection açılır
    ↓
Development Secondary veya Fireball claim et
    ↓
sandık kaybolur ve gameplay devam eder
    ↓
kazanılan davranış çalışır; Console temiz kalır
```

Eski reward debug presenter diagnostic tooling olarak kalabilir; ancak ikinci bir
selection session'ı otomatik açmamalıdır.

## Uygulama Aşamaları

### Aşama 1 — Mimari

Checkpoint:

```text
docs: define chest foundation architecture
```

### Aşama 2 — Definition ve Lifecycle Domain

```text
ChestDefinition
ChestStatus
ChestState
ChestOpenResult
EditMode testleri
```

Checkpoint:

```text
feat: add chest lifecycle domain
```

### Aşama 3 — Reward Opening Orchestration

```text
ChestController
claim-completion lifecycle
empty-offer ve busy-selection guard'ları
PlayMode testleri
```

Checkpoint:

```text
feat: add chest reward orchestration
```

### Aşama 4 — Spawn ve Player Etkileşimi

```text
ChestSpawnRequest
ChestSpawnResult
ChestSpawner
Interact input aksiyonu
PlayerChestInteractor
testler
```

Checkpoint:

```text
feat: add chest spawning and interaction
```

### Aşama 5 — Prefab ve Development Entegrasyonu

```text
foundation chest prefab
Player prefab composition
Test_Waves chest spawn/entegrasyonu
```

Checkpoint:

```text
test: integrate chest foundation with Test_Waves
```

### Aşama 6 — Final Regression

```text
EditMode Run All
PlayMode Run All
manual Test_Waves
Console temiz
chest-foundation → dev
```

## Sonraki Katmanlar

```text
chest-drop-sources
chest-type-and-rarity-rules
item-level-and-evolution rewards
multi-selection reward sessions
gold fallback
```

Bu katmanlar `ChestController`'a kaynağa özel mantık eklemek yerine spawn,
lifecycle, offer, claim ve UI sınırlarını yeniden kullanır.
