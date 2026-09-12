# Player Build Foundation

## 1. Amaç

Player Build sistemi, oyuncunun tek bir run sırasında sahip olduğu build'i temsil eder.

Build üç ana item kategorisinden oluşur:

- Weapon
- Ability
- Upgrade

Bu sistemin görevi item davranışlarını çalıştırmak değildir. Player Build yalnızca oyuncunun hangi itemlara sahip olduğunu, hangi kategoride kaç slot kullanılabildiğini ve yeni bir item'ın build'e eklenip eklenemeyeceğini yönetir.

Bu foundation ileride aşağıdaki sistemlerin ortak runtime kaynağı olacaktır:

- Weapon sistemi
- Ability sistemi
- Upgrade sistemi
- Chest / Reward sistemi
- Evolution sistemi
- Run progression
- Meta progression

## 2. Tasarım Hedefleri

Player Build sistemi:

1. Run sırasında oyuncunun sahip olduğu build'i tek bir yerde temsil etmelidir.
2. Weapon, Ability ve Upgrade kategorilerini birbirinden ayırmalıdır.
3. Her kategori için bağımsız slot kapasitesi desteklemelidir.
4. Başlangıç ve meta progression ile artırılmış slot kapasitelerini desteklemelidir.
5. Aynı item'ın iki farklı slotu yanlışlıkla işgal etmesini önlemelidir.
6. Slot dolu olduğunda bunu açık bir sonuç olarak bildirmelidir.
7. Chest ve reward sistemlerinden bağımsız olmalıdır.
8. Item davranışlarını çalıştırmamalıdır.
9. Unity lifecycle'ına ihtiyaç duymayan küçük runtime/domain sınıflarından oluşmalıdır.
10. Gelecekte level-up ve evolution eklenmesine uygun olmalıdır.

## 3. Build Kategorileri

İlk sürümde üç build kategorisi vardır:

| Category | Starting Capacity | Maximum Capacity |
|---|---:|---:|
| Weapon | 1 | 2 |
| Ability | 3 | 5 |
| Upgrade | 5 | 8 |

Başlangıç kapasiteleri yeni bir oyuncunun run başındaki standart kapasitesidir.

Maximum Capacity ise meta progression tarafından ulaşılabilecek üst sınırı ifade eder.

Örneğin ileride bir oyuncunun kalıcı progression sonucu kapasitesi şu şekilde olabilir:

```text
Weapons   = 2
Abilities = 4
Upgrades  = 6
```

Player Build bu kapasitenin nereden geldiğini bilmez.

Meta progression yalnızca gerekli kapasite değerini sağlayacaktır.

## 4. Ownership

Player Build aşağıdaki verilerin sahibidir:

```text
PlayerBuild
├── Capacity
├── Weapon Entries
├── Ability Entries
└── Upgrade Entries
```

Player Build aşağıdaki sistemlerin sahibi değildir:

```text
Weapon ammo / reload / cooldown
Ability cooldown / targeting / casting
Stat modifiers
Chest generation
Reward selection
Evolution conditions
Save data
Meta currency
```

Bu davranışlar ilgili sistemlerde kalacaktır.

## 5. PlayerBuildCapacity

Slot kapasitesi Player Build'in içine hard-code edilmez.

Kapasite ayrı bir runtime value/model tarafından temsil edilir:

```text
PlayerBuildCapacity
├── WeaponSlots
├── AbilitySlots
└── UpgradeSlots
```

Default değerler:

```text
Weapon  = 1
Ability = 3
Upgrade = 5
```

Maximum değerler:

```text
Weapon  = 2
Ability = 5
Upgrade = 8
```

Capacity oluşturulduktan sonra immutable olmalıdır.

Bir run sırasında kapasitenin keyfi olarak değiştirilmesi foundation kapsamının dışındadır.

İleride meta progression sistemi yeni bir run başlatılırken uygun `PlayerBuildCapacity` değerini sağlayacaktır.

## 6. Item Identity

Bir build item'ının kimliği Unity instance referansına göre belirlenmemelidir.

Item identity, ilgili item definition'ın kalıcı kimliği üzerinden değerlendirilmelidir.

Örneğin:

```text
weapon.shotgun
ability.fireball
upgrade.glass_cannon
```

Aynı stable identity'ye sahip item aynı build item'ı kabul edilir.

Bu karar aşağıdaki sistemler için önemlidir:

- Reward generation
- Save system
- Evolution requirements
- Duplicate detection
- Content validation

Player Build item'ın gameplay davranışını bilmez.

## 7. Item Ekleme

Yeni bir item build'e eklenmek istendiğinde sistem aşağıdaki kontrolleri yapar:

```text
Item valid mi?
        ↓
Category destekleniyor mu?
        ↓
Item zaten build'de mi?
        ↓
Category'de boş slot var mı?
        ↓
Item eklenir
```

Başarısız ekleme exception ile normal gameplay akışını bozmamalıdır.

Beklenen runtime sonuçları açık şekilde temsil edilmelidir.

Örnek sonuçlar:

```text
Added
AlreadyOwned
CapacityReached
```

Geçersiz programlama kullanımları ise exception olabilir.

Örneğin null definition veya unsupported category bir programmer/configuration error olarak değerlendirilebilir.

## 8. Duplicate Item Davranışı

Foundation aşamasında aynı item tekrar build'e otomatik olarak eklenmez.

Örneğin oyuncunun build'inde zaten:

```text
Fireball
```

varsa ikinci Fireball:

```text
Ability Slot 2
```

işgal etmez.

Player Build bunun yerine:

```text
AlreadyOwned
```

sonucu üretir.

Foundation aşamasında Player Build otomatik level-up da yapmaz.

Gelecekte reward sistemi şu ayrımı yapabilecektir:

```text
Item build'de yok
→ Add new item

Item build'de var
→ Item progression / level-up candidate
```

Bu sayede slot ownership ile item progression birbirine karıştırılmaz.

## 9. Slot Doluluk Davranışı

Bir kategori kapasitesine ulaştığında yeni bir item eklenemez.

Örneğin başlangıç build'i:

```text
Weapon Capacity = 1

Weapon Slot
└── Shotgun
```

ise Minigun ekleme isteği:

```text
CapacityReached
```

sonucu üretmelidir.

Player Build otomatik olarak mevcut item'ı değiştirmez veya silmez.

Replacement sistemi gerekiyorsa ileride ayrı bir kullanıcı kararı / reward flow olarak ele alınacaktır.

## 10. Query API

Diğer gameplay sistemleri Player Build'e aşağıdaki türde sorular sorabilmelidir:

```text
Bu item mevcut mu?
Bu kategoride kaç item var?
Bu kategorinin kapasitesi kaç?
Boş slot var mı?
Kategori dolu mu?
Build'deki itemları getir
```

Query işlemleri Player Build state'ini değiştirmemelidir.

External sistemlere mutable collection verilmemelidir.

## 11. Runtime Architecture

İlk tasarım hedefi:

```text
PlayerBuild
│
├── PlayerBuildCapacity
│
├── Weapon collection
├── Ability collection
└── Upgrade collection
```

Player Build bir `MonoBehaviour` veya `ScriptableObject` olmayacaktır.

Run'a ait runtime state'i temsil eden plain C# object olacaktır.

ScriptableObject definition asset'leri data kaynağı olarak kullanılabilir, ancak build lifecycle Unity scene lifecycle'ına bağlı olmayacaktır.

## 12. Future Runtime Integration

İleride runtime akışı yaklaşık olarak şu şekilde olacaktır:

```text
Meta Progression
      ↓
PlayerBuildCapacity
      ↓
PlayerBuild created
      ↓
Starting Weapon added
      ↓
Arena starts
      ↓
Chest / Reward
      ↓
PlayerBuild query
      ↓
New Item / Level Up / Evolution
```

Player Build reward üretmez.

Reward sistemi Player Build state'ini sorgular ve uygun reward seçeneklerini oluşturur.

## 13. Weapon Integration

Player Build:

```text
Player owns Shotgun
```

bilgisini tutacaktır.

Weapon runtime sistemi ise:

```text
Ammo
Reload state
Fire cooldown
Trigger state
Active weapon
```

gibi operational state'i yönetmeye devam edecektir.

Bu iki state birbirine karıştırılmamalıdır.

## 14. Ability Integration

Ability foundation oluşturulduğunda Player Build:

```text
Player owns Fireball
Player owns Drone
```

bilgisini sağlayacaktır.

Ability runtime sistemi:

```text
Cooldown
Target selection
Auto cast
Projectile / behaviour execution
```

işlerini yönetecektir.

## 15. Upgrade Integration

Player Build oyuncunun hangi upgrade'lere sahip olduğunu belirleyecektir.

Upgrade sisteminin stat etkileri Player Build içinde uygulanmayacaktır.

Örneğin:

```text
PlayerBuild
└── Glass Cannon owned

Upgrade Runtime / Stat System
└── damage modifier uygulanır
```

şeklinde ayrım korunacaktır.

## 16. Evolution Integration

Evolution foundation bu branch'in kapsamında değildir.

Ancak Player Build daha sonra evolution sisteminin ihtiyaç duyacağı ownership sorgularını sağlayabilmelidir.

Örneğin:

```text
Fireball owned?
Required Upgrade owned?
Fireball required level'da mı?
```

Level bilgisinin tam ownership modeli item progression aşamasında netleştirilecektir.

Player Build Foundation bu aşamada evolution davranışı uygulamaz.

## 17. Save ve Meta Progression

Player Build run-scoped bir sistemdir.

Kalıcı save state'in sahibi değildir.

İleride akış:

```text
Save / Meta Progression
        ↓
Unlocked Capacity
        ↓
Run başlangıcı
        ↓
PlayerBuildCapacity
        ↓
PlayerBuild
```

şeklinde olacaktır.

Save sistemi runtime `PlayerBuild` instance'ını doğrudan serialize etmek zorunda değildir.

Kalıcı olarak saklanması gereken veriler ayrı save modelleri üzerinden ele alınacaktır.

## 18. Invariants

Player Build aşağıdaki kuralları her zaman korumalıdır:

```text
Weapon count  <= Weapon capacity
Ability count <= Ability capacity
Upgrade count <= Upgrade capacity

Capacity <= global maximum

Aynı stable item aynı kategoride birden fazla kez bulunamaz.

Player Build dışındaki sistemler internal collection'ları doğrudan değiştiremez.
```

## 19. Error Handling

Programmer/configuration hataları exception ile ifade edilebilir:

```text
Null item definition
Invalid capacity
Unsupported category
Invalid stable identity
```

Normal gameplay sonuçları exception olmamalıdır:

```text
Item already owned
Slot full
```

Bunlar explicit operation result ile döndürülmelidir.

## 20. Testing Strategy

Foundation'ın büyük kısmı EditMode testleri ile doğrulanacaktır.

Test kapsamı:

```text
Default capacity
Custom valid capacity
Maximum capacity
Invalid capacity

Empty PlayerBuild creation

Add Weapon
Add Ability
Add Upgrade

Duplicate rejection

Weapon capacity reached
Ability capacity reached
Upgrade capacity reached

Contains queries
Count queries
Capacity queries

Collections cannot be mutated externally
```

Runtime integration başladığında gerekli PlayMode testleri ayrıca eklenecektir.

## 21. Out of Scope

Bu foundation branch'inde aşağıdakiler yapılmayacaktır:

```text
Chest UI
Reward generation
Reward selection UI

Ability execution
Ability targeting
Ability cooldown

Upgrade stat modifiers

Item level-up implementation
Evolution implementation

Save system
Meta progression purchase system

Weapon switching UI
Inventory UI

Pooling
Addressables integration
```

## 22. Implementation Stages

### Stage 1 — Domain Foundation

```text
PlayerBuildSlotType
PlayerBuildCapacity
PlayerBuild operation result
PlayerBuild
EditMode tests
```

Bu aşama tamamlandığında ilk implementation checkpoint alınır.

Suggested changeset:

```text
feat: add player build domain foundation
```

### Stage 2 — Runtime Integration

Player build'in run sırasında oluşturulması ve başlangıç item'larının bağlanması ele alınacaktır.

İlgili integration testleri tamamlandıktan sonra ikinci checkpoint alınır.

Suggested changeset:

```text
feat: integrate player build runtime
```

### Stage 3 — Downstream Systems

Sonraki branch'lerde:

```text
Ability Foundation
Stat / Upgrade System
Chest / Reward System
Item Progression
Evolution System
```

Player Build API'si üzerine kurulacaktır.
