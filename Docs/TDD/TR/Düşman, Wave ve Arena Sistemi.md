# Düşman, Wave ve Arena Sistemi

## 1. Amaç

Bu modül, Project First Run içerisindeki düşmanların çalışma zamanı yapısını,
hedef takibini, sahneye ait düşman kaydını ve ileride eklenecek wave sisteminin
sınırlarını tanımlar.

İlk sürüm yalnızca Enemy Foundation kapsamını uygular:

- ScriptableObject tabanlı düşman tanımı
- Açık hedef ataması
- NavMeshAgent tabanlı hareket
- Oyuncuya yaklaşma
- Belirlenen mesafede durma
- Combat Foundation ile can ve ölüm entegrasyonu
- Sahneye ait EnemyRegistry
- Ölüm sonrasında registry kaydının kaldırılması
- Basit takipçi düşman prototipi

Düşman saldırıları, wave üretimi, spawn bütçeleri ve arena tamamlanma kuralları
ilk sürümün dışında tutulur.

---

## 2. Tasarım Hedefleri

- Düşman yapılandırması çalışma zamanı durumundan ayrılmalıdır.
- ScriptableObject asset'leri çalışma sırasında değiştirilmemelidir.
- Düşmanlar oyuncuyu global arama yöntemleriyle bulmamalıdır.
- Hedef Transform'u initialization sırasında açıkça verilmelidir.
- Hareket kodu NavMeshAgent kullanımını diğer sistemlerden izole etmelidir.
- Can ve ölüm kuralları mevcut HealthComponent üzerinden yürütülmelidir.
- Aktif düşman listesi sahneye ait olmalıdır.
- Global EnemyManager veya servis locator kullanılmamalıdır.
- Ölüm kararı ile ölüm sunumu birbirinden ayrılmalıdır.
- Yapı ileride object pooling ile kullanılabilmelidir.

---

## 3. EnemyDefinition

EnemyDefinition, değişmeyen düşman yapılandırmasını taşıyan ScriptableObject'tir.

İlk sürüm alanları:

- Stable ID
- Display Name
- Maximum Health
- Movement Speed
- Acceleration
- Angular Speed
- Stopping Distance
- Destination Update Interval

EnemyDefinition aşağıdaki çalışma zamanı durumlarını saklamaz:

- Mevcut can
- Mevcut hedef
- NavMesh yolu
- Ölüm durumu
- Registry üyeliği
- Aktif saldırı cooldown'u

İlk prototip stable ID değeri:

`enemy.chaser-basic`

---

## 4. EnemyController

EnemyController, düşman prefab'ının çalışma zamanı koordinatörüdür.

Sorumlulukları:

- EnemyDefinition ile başlatılmak
- Hedef Transform'u almak
- EnemyRegistry referansını almak
- HealthComponent'i yapılandırmak
- EnemyMotor'u yapılandırmak
- Düşmanı registry içine kaydetmek
- Ölüm event'ini dinlemek
- Ölüm sonrasında hareketi durdurmak
- Düşmanı registry'den çıkarmak

Sorumlu olmadığı alanlar:

- NavMeshAgent hareket ayrıntıları
- Hasar hesaplama
- Oyuncuyu sahnede arama
- Loot üretme
- Deneyim verme
- Wave ilerletme
- Ölüm animasyonu
- Pool'a geri gönderme

Initialization açıkça şu bağımlılıkları alır:

- EnemyDefinition
- Target Transform
- EnemyRegistry

Düşman initialization tamamlanmadan gameplay davranışına başlamamalıdır.

---

## 5. EnemyMotor

EnemyMotor, NavMeshAgent ile hedef takibi arasındaki sınırdır.

Sorumlulukları:

- NavMeshAgent ayarlarını EnemyDefinition üzerinden uygulamak
- Hedef pozisyonunu belirli aralıklarla agent'a aktarmak
- Stopping Distance değerini uygulamak
- Hareketi etkinleştirmek ve durdurmak
- Geçersiz veya ulaşılamayan hedef durumunu güvenli yönetmek

Sorumlu olmadığı alanlar:

- Düşman canı
- Düşman ölümü
- Oyuncu saldırısı
- Registry kaydı
- Düşman animasyonları

İlk sürümde düşman hedefe düz fizik hareketiyle değil, NavMeshAgent üzerinden
yaklaşır.

---

## 6. Hedef Ataması

Düşman hedefini kendi başına aramaz.

Hedef aşağıdaki sistemlerden biri tarafından initialization sırasında sağlanır:

- ArenaSceneBootstrap
- EnemySpawner
- TestEnemyBootstrap

İlk test sahnesinde oyuncu Transform'u TestEnemyBootstrap tarafından düşmana
aktarılır.

Aşağıdaki yöntemler kullanılmaz:

- FindObjectOfType
- FindFirstObjectByType
- GameObject.Find
- Player singleton
- Global service locator

Bu yaklaşım bağımlılığı görünür hâle getirir ve test sahnelerinde farklı hedeflerin
kullanılmasını kolaylaştırır.

---

## 7. EnemyRegistry

EnemyRegistry sahneye ait aktif düşman koleksiyonudur.

Sorumlulukları:

- Başlatılmış aktif düşmanları kaydetmek
- Ölen veya devre dışı bırakılan düşmanları kaldırmak
- Aktif düşman sayısını sunmak
- İleride yetenek ve arena sistemlerine salt okunur düşman erişimi sağlamak

EnemyRegistry:

- DontDestroyOnLoad kullanmaz.
- Static instance değildir.
- Düşman spawn etmez.
- Wave durumunu yönetmez.
- Hasar uygulamaz.
- Düşman seçme stratejisi uygulamaz.

Registry içerisindeki düşmanlar sahne değiştiğinde sahneyle birlikte yok edilir.

---

## 8. Can Yapılandırması

EnemyDefinition içindeki Maximum Health, düşman initialization sırasında
HealthComponent'e uygulanır.

HealthComponent, çalışma zamanı başlamadan önce açıkça yapılandırılabilmelidir.

Kurallar:

- Maximum Health sonlu ve sıfırdan büyük olmalıdır.
- Yapılandırma düşman registry'ye eklenmeden önce yapılmalıdır.
- Yapılandırma mevcut HealthState'i yeni maksimum canla yeniden oluşturur.
- Düşman tam canla başlar.
- Hasar aldıktan sonra rastgele yeniden yapılandırma yapılmaz.
- Pooling sırasında yeniden initialization açıkça gerçekleştirilir.

---

## 9. Hareket Modeli

EnemyMotor hedef pozisyonunu her render frame'inde zorunlu olarak yeniden yazmaz.

İlk değer:

`Destination Update Interval: 0.1 seconds`

Her güncellemede:

1. Hedefin geçerli olduğu kontrol edilir.
2. NavMeshAgent aktif ve NavMesh üzerinde mi kontrol edilir.
3. Hedef pozisyonu yeni destination olarak atanır.
4. Agent, Stopping Distance sınırına kadar hareket eder.

İlk sürümde özel avoidance, sürü davranışı veya düşman-düşman itme sistemi
eklenmez.

---

## 10. Ölüm Akışı

Öldürücü hasar sonrasında:

1. HealthComponent `Died` event'ini yayınlar.
2. EnemyController ölüm durumuna geçer.
3. EnemyMotor durdurulur.
4. NavMeshAgent yeni hedef almaz.
5. Düşman EnemyRegistry'den çıkarılır.
6. Yerel ölüm sunumu bilgilendirilir.

EnemyController aynı ölüm için işlemleri yalnızca bir kez yapmalıdır.

İlk prototipte düşman nesnesi hemen yok edilmez. Basit bir görsel ölüm durumu
uygulanabilir.

Object pool'a dönüş daha sonraki modülde ele alınacaktır.

---

## 11. İlk Chaser Değerleri

| Ayar | Başlangıç Değeri |
|---|---:|
| Maximum Health | 100 |
| Movement Speed | 3.5 |
| Acceleration | 12 |
| Angular Speed | 720 |
| Stopping Distance | 1.5 |
| Destination Update Interval | 0.1 |

Bu değerler yalnızca teknik doğrulama içindir.

---

## 12. İlk Sürüm Kabul Kriterleri

- Düşman EnemyDefinition ile initialize edilmelidir.
- Oyuncu hedefi dışarıdan verilmelidir.
- Düşman geçerli NavMesh üzerinde oyuncuya yaklaşmalıdır.
- Düşman Stopping Distance içinde durmalıdır.
- Oyuncu hareket ettiğinde düşman yeni konumu takip etmelidir.
- Düşman spawn sonrasında EnemyRegistry içine eklenmelidir.
- Plasma Rifle ile dört adet 25 hasarlı atışta ölmelidir.
- Ölüm yalnızca bir kez işlenmelidir.
- Ölüm sonrasında hareket durmalıdır.
- Ölüm sonrasında registry kaydı kaldırılmalıdır.
- Ölü düşman yeni hasarı reddetmelidir.
- Console'da error veya sürekli warning oluşmamalıdır.

---

## 13. Ertelenen Konular

- Düşman saldırıları
- Oyuncuya hasar verme
- Attack cooldown
- Attack animation
- Hit reaction
- Knockback
- Enemy pooling
- WaveDefinition
- WaveSpawner
- Spawn point seçimi
- Spawn bütçesi
- Elite düşmanlar
- Boss düşmanlar
- Arena tamamlanma koşulları
- XP ve loot üretimi
- Uzak düşman culling
- Crowd avoidance optimizasyonu
- Farklı navigasyon modelleri