# Çalışma Zamanı Mimarisi

# Çalışma Zamanı Mimarisi ve Sahne Akışı

## 1. Amaç

Bu bölüm; çalışma zamanı yaşam sürelerini, uygulamanın başlangıç akışını, sahne geçişlerini, tur durumunun sahipliğini ve arena başlatma sürecini tanımlar.

Temel hedefler şunlardır:

- Oyuncunun tur durumunu arena sahneleri arasında korumak.
- Sahne nesnelerinin global bağımlılıklara dönüşmesini engellemek.
- Her şeyi yöneten tek parça bir `GameManager` oluşturmamak.
- Kalıcı uygulama servislerini tur ve arena sistemlerinden ayırmak.
- Arena sahnelerinin bağımsız olarak test edilebilmesini sağlamak.
- Oyuncunun build’ini ve ilerlemesini kaybetmeden sahne nesnelerini güvenli biçimde yeniden oluşturmak.
- Gereksiz soyutlamalardan ve gizli global referanslardan kaçınmak.

---

## 2. Yaşam Süresi Modeli

Projede üç temel yaşam süresi bulunur.

### 2.1 Uygulama Yaşam Süresi

Uygulama yaşam süresine sahip sistemler, oyun açıldığında oluşturulur ve uygulama kapatılana kadar yaşamaya devam eder.

Örnekler:

- Sahne akışı
- Oyuncu profil verileri
- Kayıt sistemi
- Ayarlar
- Eşya kataloğu
- Tur koordinasyonu
- Yükleme ekranı sunumu

Bu sistemler Bootstrap sahnesi tarafından bir kez oluşturulur.

### 2.2 Tur Yaşam Süresi

Tur yaşam süresine sahip veriler, bir turun başlangıcından oyuncu ölene, turu terk edene veya son boss’u yenene kadar yaşar.

Örnekler:

- Mevcut can
- Mevcut tur seviyesi
- Mevcut deneyim
- Toplanan tur altını
- Kuşanılmış eşyalar
- Eşya seviyeleri
- Aktif tur güçlendirmeleri
- Mevcut arena
- Tur rastgelelik tohumu
- Tur istatistikleri

Tur durumu arena sahneleri arasındaki geçişlerde korunmalıdır. İlk sürümde oyunun kapatılması durumunda korunması zorunlu değildir.

### 2.3 Sahne Yaşam Süresi

Sahne yaşam süresine sahip nesneler yalnızca o anda yüklenmiş olan sahneye aittir.

Örnekler:

- Oyuncu GameObject’i
- Kamera
- Silah ve yetenek çalışma zamanı bileşenleri
- Düşmanlar
- Mermiler
- Toplanabilir nesneler
- Arena yerleşimi
- Dalga yöneticisi
- Düşman kayıt sistemi
- Sahne arayüzü
- Yerel nesne havuzları
- Işıklandırma ve çevre nesneleri

Bu nesneler mevcut sahne kaldırıldığında yok edilir.

---

## 3. Kalıcı Uygulama Kökü

Oyun küçük bir `Bootstrap` sahnesinden başlar.

Bootstrap sahnesi tek bir kalıcı kök nesne oluşturur:

```
ApplicationRoot
├── SceneFlowController
├── RunCoordinator
├── ProfileService
├── SaveService
├── ItemCatalog
└── GlobalLoadingOverlay
```

`DontDestroyOnLoad` kullanması beklenen tek kök nesne `ApplicationRoot` olacaktır.

Aşağıdaki nesneler `DontDestroyOnLoad` kullanmamalıdır:

- Oyuncu
- Kamera
- Silahlar
- Yetenekler
- Düşmanlar
- Arena kontrolcüleri
- Sahne arayüzü
- Mermiler
- Toplanabilir nesneler

Bu kural; geçerliliğini kaybetmiş sahne referanslarını, yinelenen oyuncu nesnelerini, sahneler arasında kalan kameraları ve hata ayıklaması zor arena geçişlerini engeller.

`ApplicationRoot`, global bir `GameManager` değildir. Oynanış kuralları içermez. Açıkça tanımlanmış sorumluluklara sahip ayrı sistemleri barındırır.

---

## 4. Bağımlılıklara Erişim

Projenin ilk sürümünde aşağıdaki yapılar kullanılmayacaktır:

- Global service locator
- Statik bağımlılık container’ı
- Global event bus
- Dependency Injection framework’ü
- `GameManager.Instance` gibi statik erişimler

Bootstrap sahnesi composition root görevi görür. Uygulama servislerini oluşturur ve yeni yüklenen sahnelere yalnızca ihtiyaç duydukları bağımlılıkları verir.

Bir sahne yalnızca birkaç bağımlılığa ihtiyaç duyuyorsa tüm uygulama bağlamını almamalıdır.

Örnek:

```
ArenaLoadContext
├── RunSession
├── ItemCatalog
├── RandomSource
└── Arena tamamlanma callback’i
```

Bir arenanın aşağıdaki yapılara doğrudan erişmesi gerekmez:

- SaveService
- SettingsService
- Ana menü arayüzü
- Profil dosyası depolama sistemi
- Sahne geçişinin teknik uygulanışı

Bu yaklaşım bağımlılıkları açık ve sınırlı tutar.

---

## 5. Sahne Yapısı

Şu anda planlanan sahne yapısı:

```
Scenes
├── Bootstrap
├── MainMenu
├── Base
├── Arena_01
├── Arena_02
├── Arena_Boss_01
├── Arena_FinalBoss
└── Test
    ├── Test_Player
    ├── Test_Weapons
    └── Test_Combat
```

Her arena ayrı bir Unity sahnesinde tutulur.

Arena geçişlerinde asenkron `LoadSceneMode.Single` yükleme kullanılır.

Additive sahne yükleme şu anda gerekli değildir.

Ayrı bir Loading sahnesi yerine kalıcı bir yükleme ekranı katmanı kullanılacaktır. Bu yöntem, arenalar arasında tam ekran yükleme sunarken sahne yapısını daha sade tutar.

---

## 6. Uygulama Akışı

İlk uygulama akışı:

```
Uygulama Başlangıcı
    ↓
Bootstrap Sahnesi
    ↓
ApplicationRoot Oluştur
    ↓
Profil ve Ayarları Yükle
    ↓
Ana Menü veya Üs Sahnesini Yükle
    ↓
Oyuncu Tura Başlar
    ↓
RunSession Oluştur
    ↓
İlk Arenayı Yükle
    ↓
Arenayı Başlat
    ↓
Arenayı Oyna
    ↓
Arenayı Tamamla
    ↓
Tur İlerlemesini Güncelle
    ↓
Sonraki Arenayı Yükle
    ↓
Son Boss Zaferi veya Oyuncu Ölümü
    ↓
RunResult Oluştur
    ↓
Kalıcı Ödülleri Uygula
    ↓
Profili Kaydet
    ↓
Üsse Dön
```

Geçersiz uygulama geçişlerini engellemek için basit bir akış enum’u kullanılabilir:

```
Booting
MainMenu
Base
LoadingArena
PlayingArena
ResolvingRun
```

Uygulama akışı gerçekten karmaşıklaşmadıkça sınıf tabanlı bir state machine eklenmeyecektir.

---

## 7. Sahne Geçiş Süreci

Normal sahne geçişlerini yalnızca `SceneFlowController` gerçekleştirebilir.

Bir arena geçişi şu sırayı takip eder:

```
Arena geçişi talep edilir
    ↓
Yinelenen geçiş talepleri reddedilir
    ↓
Oynanış girdileri devre dışı bırakılır
    ↓
Yükleme ekranı gösterilir
    ↓
Hedef sahne asenkron yüklenir
    ↓
Hedef sahne etkinleştirilir
    ↓
Sahne giriş noktası bulunur
    ↓
Sahne bağımlılıkları hazırlanır
    ↓
Oyuncunun çalışma zamanı nesneleri oluşturulur
    ↓
Tur durumu geri yüklenir
    ↓
Arena başlatılır
    ↓
Yükleme ekranı gizlenir
    ↓
Oynanış girdileri yeniden etkinleştirilir
```

Sahne sınırında bir defaya mahsus nesne araması kabul edilebilir.

Örneğin `SceneFlowController`, arena yüklendikten sonra sahnedeki `ArenaSceneBootstrap` bileşenini bir kez bulabilir. Oynanış güncellemeleri içerisinde sürekli `FindObjectOfType` gibi aramalar yapılmasına izin verilmez.

Yükleme ekranı `ApplicationRoot` altında bulunduğu için eski sahne yenisiyle değiştirilirken görünür kalabilir.

---

## 8. Run Coordinator

`RunCoordinator`, aktif turun yaşam döngüsünden sorumludur.

Sorumlulukları:

- Yeni bir tur başlatmak.
- İlk `RunSession` nesnesini oluşturmak.
- Aktif `RunSession` nesnesini saklamak.
- Arena geçişlerini talep etmek.
- Arena tamamlanma sonuçlarını işlemek.
- Başka bir arenanın yüklenip yüklenmeyeceğine karar vermek.
- Ölüm, turu terk etme veya son zafer durumlarında turu sonlandırmak.
- Son `RunResult` nesnesini oluşturmak.
- Korunan altını ve kalıcı ödülleri oyuncu profiline uygulamak.
- Profil kaydını talep etmek.
- Oyuncuyu üs sahnesine geri göndermek.

`RunCoordinator` aşağıdaki işlemleri yapmamalıdır:

- Düşman oluşturmak.
- Dalgaları kontrol etmek.
- Silah hasarını hesaplamak.
- Sandık seçeneklerini oluşturmak.
- Oyuncuyu kontrol etmek.
- Oynanış arayüzünü güncellemek.
- Arenaya özel kurallar içermek.

---

## 9. Run Session

`RunSession`, saf bir C# çalışma zamanı modelidir.

Bir `MonoBehaviour` değildir ve doğrudan herhangi bir sahnede saklanmaz.

Önerilen yapısı:

```
RunSession
├── RunIdentity
│   ├── RunId
│   └── RandomSeed
│
├── RouteState
│   ├── CurrentArenaId
│   ├── CurrentArenaIndex
│   └── DefeatedBosses
│
├── VitalsState
│   └── CurrentHealth
│
├── ProgressionState
│   ├── RunLevel
│   ├── CurrentExperience
│   └── RequiredExperience
│
├── WalletState
│   └── RunGold
│
├── LoadoutState
│   ├── Weapons
│   ├── Abilities
│   └── Upgrades
│
├── StatState
│   ├── BaseStatSnapshot
│   └── ActiveRunModifiers
│
└── Statistics
    ├── EnemiesDefeated
    ├── ElitesDefeated
    ├── DamageDealt
    └── RunDuration
```

Kesin istatistik listesi değişebilir.

Ödül üretimlerini ve çözülmesi zor hataları geliştirme sırasında yeniden oluşturmayı kolaylaştırmak için rastgelelik tohumu saklanır.

---

## 10. Tur Durumunun Sahipliği

Arena geçişleri arasında korunması gereken durumun tek doğru kaynağı `RunSession` olacaktır.

Sahne nesneleri kalıcı tur verilerinin ayrı ve yetkili kopyalarını tutmaz.

Örnekler:

```
PlayerHealth
    ↓
RunSession.VitalsState değerini okur ve günceller
```

```
ExperienceSystem
    ↓
RunSession.ProgressionState değerini okur ve günceller
```

```
RunWallet
    ↓
RunSession.WalletState değerini okur ve günceller
```

```
LoadoutSystem
    ↓
RunSession.LoadoutState değerini okur ve günceller
```

Sistemler yalnızca ihtiyaç duydukları tur modeli bölümünü almalıdır. Bir silahın tüm `RunSession` nesnesine erişmesi gerekmez.

---

## 11. Arenalar Arasında Korunan Veriler

Aşağıdaki durumlar varsayılan olarak korunur:

- Mevcut can
- Tur seviyesi
- Mevcut deneyim
- Tur altını
- Kuşanılmış silahlar
- Kuşanılmış yetenekler
- Kuşanılmış güçlendirmeler
- Ekipman seviyeleri
- Evrimleşmiş ekipmanlar
- Tur genelindeki stat modifier’ları
- Mevcut rota ilerlemesi
- Tur istatistikleri

Aşağıdaki durumlar varsayılan olarak sahneye özeldir:

- Aktif düşmanlar
- Mermiler
- Geçici görsel efektler
- Yetenek bekleme süresi ilerlemesi
- Düşmanlardaki aktif durum etkileri
- Arena dalga ilerlemesi
- Mevcut silah nesnesi referansları
- Yerel nesne havuzları
- Geçici sahne etkileşimleri

Mühimmatın, bekleme sürelerinin ve oyuncudaki geçici etkilerin arenalar arasında korunup korunmayacağı bir oyun tasarımı kararıdır ve henüz kesinleşmemiştir.

Yeni bir karar alınana kadar:

- Silahlar yeni arenaya girildiğinde yeniden oluşturulur.
- Şarjörler varsayılan hazır durumunda başlar.
- Yetenek bekleme süreleri varsayılan durumunda başlar.
- Düşmanlara ait durum etkileri arena ile birlikte yok edilir.

---

## 12. Profil Verileri ve Tur Verileri

Kalıcı profil verileriyle geçici tur verileri birbirinden ayrı kavramlardır.

### 12.1 Profil Verileri

Diske kaydedilir:

```
PlayerProfile
├── PermanentGold
├── UnlockedSlots
├── PermanentUpgrades
├── UnlockedContent
├── ProgressionFlags
└── SaveVersion
```

### 12.2 Tur Verileri

Tur sırasında bellekte tutulur:

```
RunSession
├── CurrentHealth
├── RunGold
├── RunLevel
├── Equipment
├── ItemLevels
└── ArenaProgress
```

Oyunun ilk sürümünde uygulama kapatıldıktan sonra yarım kalan turun kaydedilmesi ve devam ettirilmesi gerekli değildir.

Gelecekte bir duraklatıp devam ettirme sistemi `RunSessionSnapshot` nesnesini serialize edebilir. Ancak ilk mimari, henüz kesinleşmemiş bu özellik etrafında karmaşıklaştırılmayacaktır.

---

## 13. Temel Stat Anlık Görüntüsü

Bir tur başladığında kalıcı profil geliştirmeleri başlangıç stat anlık görüntüsüne dönüştürülür.

```
Varsayılan Karakter Statları
    +
Kalıcı Profil Geliştirmeleri
    ↓
Tur Temel Stat Anlık Görüntüsü
```

Tur sırasında:

```
Tur Temel Stat Anlık Görüntüsü
    +
Ekipman Modifier’ları
    +
Trait Modifier’ları
    +
Geçici Tur Modifier’ları
    ↓
Son Çalışma Zamanı Statları
```

Savaş sistemleri oynanış sırasında sürekli olarak kalıcı profil verilerini sorgulamaz.

Bu yaklaşım, turun sabit bir temelle başlamasını ve tura ait tüm değişikliklerin `RunSession` içinde kalmasını sağlar.

---

## 14. Eşya Kimliği ve Katalog

Kayıt dosyaları ve tur durumu, oyuncuya gösterilen isimlere veya doğrudan sahne nesnesi referanslarına bağlı olmamalıdır.

Her eşya tanımı değişmeyecek bir kimlik kullanır.

Örnekler:

```
weapon.shotgun
weapon.minigun
ability.force_wave
ability.fireball
upgrade.damage
```

Merkezî `ItemCatalog`, sabit kimlikleri ScriptableObject tanımlarına dönüştürür.

```
Sabit Eşya Kimliği
    ↓
ItemCatalog
    ↓
ItemDefinition
```

Kataloğun sorumlulukları:

- Eşya kimliklerini çözümlemek.
- Tekrarlanan kimlikleri doğrulamak.
- Loadout ve ödül sistemlerine tanımları sağlamak.
- Gelecekteki Addressables entegrasyonunu desteklemek.

`RunSession` aşağıdaki verileri saklar:

```
Item ID
Item Level
```

Doğrudan bir ScriptableObject referansı saklaması gerekmez.

---

## 15. Eşyaların Çalışma Zamanında Yeniden Oluşturulması

Bir arena yüklendiğinde oyuncunun ekipmanları `RunSession.LoadoutState` verisinden yeniden oluşturulur.

Örnek:

```
Tur Loadout Kaydı
├── Item ID: weapon.shotgun
└── Level: 5
        ↓
ItemCatalog, ShotgunDefinition değerini çözümler
        ↓
Silah çalışma zamanı nesnesi oluşturulur
        ↓
Seviye verileri uygulanır
        ↓
Silah oyuncunun loadout’una eklenir
```

Evrimleşmiş ekipmanlar da aynı şekilde işlenir.

Evrim sonucu yalnızca başka bir eşya kimliğidir:

```
ability.fireball
    ↓ evrim
ability.meteor
```

Yeni arena Fireball’u yeniden oluşturmak yerine Meteor’un çalışma zamanı nesnesini oluşturur.

---

## 16. Arena Scene Bootstrap

Her oynanış sahnesi bir adet `ArenaSceneBootstrap` içerir.

Sorumlulukları:

- Arenaya özgü sahne nesnelerinin referanslarını tutmak.
- `ArenaLoadContext` almak.
- Arena içi yerel sistemleri hazırlamak.
- Oyuncuyu oluşturmak veya hazırlamak.
- Oyuncuyu mevcut tur durumuna bağlamak.
- Düşman kayıt ve dalga sistemlerini hazırlamak.
- Tüm bağımlılıklar hazırlandıktan sonra arenayı başlatmak.
- Arena tamamlanma sonucunu `RunCoordinator` sistemine bildirmek.

Sahne referansları şunları içerebilir:

```
ArenaSceneBootstrap
├── PlayerSpawnPoint
├── EnemySpawnAreas
├── WaveController
├── ArenaController
├── ArenaCameraReferences
└── SceneUIRoot
```

`ArenaSceneBootstrap` aşağıdaki işlemleri yapmamalıdır:

- Oyuncu profilini kaydetmek.
- Korunacak kalıcı altın miktarına karar vermek.
- Bir sonraki sahneyi doğrudan yüklemek.
- Tüm uygulama bağlamına sahip olmak.
- Silah veya yetenek davranışı uygulamak.

---

## 17. Arena Tamamlanması

Bir arena tamamlandığında:

```
Dalga veya boss hedefi tamamlanır
    ↓
ArenaController arenayı tamamlanmış olarak işaretler
    ↓
Oyuncunun etkileşimi sınırlandırılır
    ↓
ArenaResult oluşturulur
    ↓
RunCoordinator, ArenaResult değerini alır
    ↓
Tur rotası güncellenir
    ↓
Sonraki arena seçilir
    ↓
SceneFlowController sonraki arenayı yükler
```

`ArenaResult` şunları içerebilir:

```
ArenaResult
├── ArenaId
├── CompletionType
├── BossDefeated
├── CompletionTime
└── Arenaya özel ödüller
```

Kalıcı oyuncu verilerinin büyük bölümü zaten `RunSession` içinde tutulduğu için `ArenaResult` tüm tur durumunu tekrar etmemelidir.

---

## 18. Turun Tamamlanması

Bir tur aşağıdaki sonuçlardan biriyle biter:

```
PlayerDeath
FinalBossVictory
PlayerAbandon
```

Tur sonu akışı:

```
Tur sona erer
    ↓
RunResult oluşturulur
    ↓
Korunan altın hesaplanır
    ↓
Kalıcı ödüller uygulanır
    ↓
PlayerProfile güncellenir
    ↓
Profil kaydedilir
    ↓
Aktif RunSession temizlenir
    ↓
Üs sahnesine dönülür
```

`RunResult` şunları içerebilir:

```
RunResult
├── Outcome
├── TotalGoldCollected
├── GoldRetained
├── ArenasCompleted
├── BossesDefeated
├── EnemiesDefeated
├── RunDuration
└── FinalBuild
```

---

## 19. Sahne Sahipliği Kuralları

Aşağıdaki kurallar uygulanacaktır:

1. Sahne nesneleri kalıcı ScriptableObject’lerin içinde saklanmamalıdır.
2. Uygulama servisleri sahne geçişinden sonra yok edilmiş nesnelerin referanslarını tutmamalıdır.
3. Arena sahneleri kayıt dosyası depolamasına doğrudan erişmemelidir.
4. Arena sistemleri sahneleri doğrudan yüklememelidir.
5. Oyuncu GameObject’i her arena için yeniden oluşturulmalıdır.
6. Tur verileri korunur; sahne sunumu korunmaz.
7. Aynı anda yalnızca bir normal sahne geçişi yapılabilir.
8. Arena başlatma süreci tamamlanana kadar input devre dışı kalmalıdır.
9. Oynanış sahneleri yalnızca bir geçerli arena giriş noktasına sahip olmalıdır.
10. Sık çalışan oynanış kodlarında çalışma zamanı nesne aramalarına izin verilmez.

---

## 20. İlk Vertical Slice Akışı

İlk vertical slice bu mimariyi aşağıdaki akışla doğrular:

```
Bootstrap
    ↓
Üs Sahnesi
    ↓
Tura Başla
    ↓
RunSession Oluştur
    ↓
Test Arenasını Yükle
    ↓
Oyuncuyu Oluştur
    ↓
Shotgun Kuşan
    ↓
Düşmanları Oluştur
    ↓
Deneyim Kazan
    ↓
Sandık Aç
    ↓
Güç Dalgası veya Güçlendirme Al
    ↓
Arenayı Tamamla veya Öl
    ↓
RunResult Oluştur
    ↓
Üsse Dön
```

Mimari yalnızca aşağıdaki koşullar sağlandığında doğrulanmış kabul edilir:

- Oyuncu tur durumunu kaybetmeden sahneler arasında hareket edebilir.
- Oyuncu GameObject’i güvenli biçimde yok edilip yeniden oluşturulabilir.
- Mevcut build, eşya kimliklerinden ve seviyelerinden yeniden oluşturulabilir.
- Arena sistemleri kalıcı sahne nesnelerine bağımlı değildir.
- Mevcut arena kodları değiştirilmeden yeni bir arena eklenebilir.
- Kayıt sistemi savaş sistemlerinden ayrı kalır.
- Global `GameManager` veya service locator gerekmez.

---

## 21. Kesinleşen Teknik Kararlar

| Konu | Karar |
| --- | --- |
| Kalıcı kök | Bir adet `ApplicationRoot` |
| Kalıcı oynanış nesneleri | Kullanılmayacak |
| Tur durumu | Saf C# `RunSession` |
| Arenalar arasındaki oyuncu | Yok edilip yeniden oluşturulacak |
| Arena yükleme | Asenkron Single sahne yükleme |
| Yükleme sunumu | Kalıcı yükleme ekranı katmanı |
| Additive yükleme | Şu anda kullanılmayacak |
| Sahne başlatma | Açık arena bağlamı |
| Sahne girişini bulma | Yükleme sonrasında bir defalık arama |
| Eşya kalıcılığı | Sabit eşya kimlikleri ve seviyeleri |
| Eşya çözümleme | Merkezî `ItemCatalog` |
| Kayıt ve tur durumu | Ayrı modeller |
| Tur ortasında kayıt | İlk sürümde gerekli değil |
| Uygulama akışı | Geçişleri korunan basit coordinator |
| Global service locator | Kullanılmayacak |
| Global event bus | Kullanılmayacak |
| DI framework | Başlangıçta kullanılmayacak |
| `DontDestroyOnLoad` | Yalnızca `ApplicationRoot` ile sınırlı |