# Yetenek çalışma zamanı kurulumu

## Durum ve amaç

2026-10-25: Uygulama öncesi teknik plan. Bu doküman uygulamanın veya testlerin tamamlandığını ifade etmez.

Sekiz mevcut yeteneğin kurulumunu demo/test kodundan ayırmak ve başlangıç sırasını açık hâle getirmek. Oynanış, denge, kapasite ve içerik seçimleri korunur.

## Mevcut durum

- `Scripts/Development/Abilities/FireballDevelopmentBootstrap.cs` sekiz fabrikanın kaydını ve isteğe bağlı başlangıç Fireball edinimini birlikte yapıyor.
- `Start()` bir kare bekleyip `Install()` çağırıyor. Bu bekleme, `PlayerStartingLoadoutInitializer.Start()` içindeki build kurulumuna zaman tanıyor; gerçek bir hazır olma sözleşmesi değil.
- `PlayerStatsController` kendi `Awake()` metodunda hazırlanıyor. `PlayerAbilityAcquisitionController.Initialize()` fabrika kaydını alıyor; yetenek edinimi ayrıca build/runtime hazırlığını gerektiriyor.
- Demo açılışı loadout kapasitesini kendisi belirleyebiliyor. Yeni bileşen varsayılan kapasiteyle erken kurulum yapmamalı veya mevcut build'i sıfırlamamalı.
- Mevcut bootstrap, başlangıç yeteneği verilmese bile Fireball tanımı ve projectile prefabı istiyor. Genel fabrika kurulumu bu geliştirme içeriğine bağlı olmamalı.

## Önerilen sınırlar ve sıra

1. `Scripts/Abilities` altında ortak fabrika kurulumunu sahiplenen, amacı açık bir bileşen oluşturulur; önerilen ad `PlayerAbilityRuntimeInstaller`.
2. Mevcut sekiz fabrika ve aynı registry/damage source/stats bağımlılıkları kullanılır. Fabrikalar veya yetenek algoritmaları yeniden yazılmaz.
3. Loadout'un başarıyla tamamlandığını bildiren açık bir hazır olma sözleşmesi kullanılır. Kurucu, loadout zaten hazırsa da henüz hazır değilse de doğru çalışmalıdır. Bir kare beklemek, polling veya yalnızca execution order sayıları çözüm olarak kullanılmaz.
4. Stats ve loadout hazır olduktan sonra acquisition controller bir kez kurulur. Eksik bağımlılık veya ikinci bir kurulum sahibi anlaşılır hata verir. Disable/re-enable yeni bir registry veya ikinci başlangıç ödülü üretmez; abonelikler temizlenir.
5. Başlangıç Fireball verme, eski geliştirme adaptörünün ayrı sorumluluğu olarak kalır. Genel kurulum ücretsiz yetenek vermez. Adaptör ortak kurulum yolunu kullanır; ikinci bir fabrika listesi taşımaz.
6. Eski serialized alanlar ve script GUID'i geçişte korunur. Adaptörün `Install()` / `IsInstalled` davranışları mevcut testler değerlendirilmeden kaldırılmaz. Normal oyun yolu geliştirme namespace'ine bağımlı olmaz.

Hazır olma bildiriminin tam API'si uygulama sırasında ilgili yaşam döngüsü testleriyle netleştirilecek. Bu iş genel bir servis altyapısı veya bütün oyuncu başlangıcının yeniden yazılması değildir.

## Etki alanı

Kesin mevcut kaynaklar:

- `Scripts/Development/Abilities/FireballDevelopmentBootstrap.cs`
- `Scripts/Builds/PlayerStartingLoadoutInitializer.cs`
- `Scripts/Editor/ContentArenaSceneBuilder.cs`
- `Scripts/Editor/PlayableMapFixtureSceneBuilder.cs`
- `Tests/PlayMode/Development/Chests/ChestDevelopmentBootstrapTests.cs`
- `Tests/EditMode/Chests/CommonChestContentTests.cs`

Serialized kullanım bulunan sahneler:

- `Scenes/Demo/DeepJam_Opening.unity`
- `Scenes/Demo/DeepJam_WideCombat.unity`
- `Scenes/Playable/PlayableMapFixture.unity`
- `Scenes/Tests/Test_ContentArena.unity`
- `Scenes/Tests/Test_Waves.unity`

Yollar `Assets/_Project/` altındadır. Bu liste inceleme/geçiş kapsamıdır; her dosyanın mutlaka değişeceği anlamına gelmez. Yeni bileşen ve testlerin `.meta` dosyaları birlikte takip edilir. Sahne araçları güncellenebilir ancak mevcut sahneler yeniden üretilmez; geometri, düşman yerleşimi ve diğer kullanıcı düzenlemeleri korunur.

## Kabul ve doğrulama

- Kurulum, loadout hazır olmadan ve hazır olduktan sonra bağlanma senaryolarında bir kez tamamlanır.
- Eksik bağımlılık ve çakışan kurulum sahipliği açık hata verir; eksik kurulum başarılı sayılmaz.
- Demo maksimum kapasitesi ve test sahnelerinin kendi başlangıç loadout'ları korunur.
- Sekiz yetenek mevcut edinme yollarıyla alınabilir ve çalışır; ücretsiz Fireball yalnızca açıkça isteyen geliştirme sahnelerinde verilir.
- Disable/re-enable ve sahne yeniden başlatma çift kurulum/ödül oluşturmaz. Mevcut sahne geçişinde registry yeniden bağlama davranışı korunur.
- İlgili EditMode/PlayMode testleri yazılır veya güncellenir. Unity testlerini kullanıcı çalıştırır; başarısızlık olursa yalnızca ilgili kapsam ele alınır. Çalıştırılmamış testler başarılı raporlanmaz.
- Kullanıcı iki demo sahnesinde başlangıç, ilk yetenek sandığı ve restart; content arena'da sekiz yeteneğin edinimi; eski fixture'larda başlangıç uyumluluğunu kontrol eder.

## Kapsam dışı ve teslim

Hasar, cooldown, item seviyeleri, slot sayıları, ödül havuzları, sandık olasılıkları, HUD/menüler, meta/kayıt sistemi ve harita tasarımı değişmez. Demo/test/normal oyun profil ayrımı sonraki iştir.

Önce bu TR/EN dokümanları check-in/commit edilir. Ardından uygulama, ilgili testler, kullanıcı kabulü ve ayrı kod commit'i gelir. Demo baseline etiketi değiştirilmez.
