# Oyun başlangıç ayarları

## Durum ve amaç

Uygulama öncesi plan. Mevcut demo sahnesi zamanla gerçek oyuna dönüşecek ana geliştirme alanıdır; ayrı demo/test/normal oyun mimarileri kurulmaz. Teslim edilmiş sürüm Git etiketiyle korunur. Content arena hızlı içerik denemeleri için yardımcı olarak kalır.

Bu adım başlangıç silahı ve slot kapasitesini tek bir Inspector'dan düzenlenen veri kaynağına bağlar. Sahne yeniden oluşturulmaz; mevcut oynanış korunur.

## Mevcut durum

- `PlayerStartingLoadoutInitializer._startingWeapon` zaten serialized bir Inspector alanıdır; silah seçimi koda gömülü değildir.
- Aynı bileşenin otomatik başlangıcı `PlayerBuildCapacity.CreateDefault()` kullanır.
- `DemoOpeningController.Awake()` ise kapasiteyi doğrudan maksimum silah/yetenek/güçlendirme değerlerine zorlar.
- `WeaponSwitchingDevelopmentBootstrap` bazı test fixture'larında ayrı kapasiteyle açık kurulum yapar.
- Yeni yetenek kurucusu loadout tamamlanma bildirimine bağlıdır; bu sözleşme korunmalıdır.

## Karar

`Scripts/Builds/PlayerStartingLoadoutSettings.cs` altında önerilen bir ScriptableObject:

- Başlangıç silahı: `WeaponDefinition`.
- Silah slotu.
- Yetenek slotu.
- Güçlendirme slotu.

Asset yalnızca başlangıç yapılandırması taşır. Runtime build, mühimmat, eşya listeleri ve meta ilerleme asset'e yazılmaz. Başlangıçta doğrulanan değerlerden runtime kapasitesi oluşturulur; Play sırasında asset değişikliği mevcut run'ı yeniden kurmaz.

`PlayerStartingLoadoutInitializer` bu ayarı kullanarak tek seferlik kurulumun sahibi olur. Demo açılışı sayı belirlemek yerine yapılandırılmış başlangıç metodunu çağırabilir; otomatik Start yolu aynı metodu kullanır ve tamamlanmış kurulumu tekrarlamaz. Mevcut `Initialized` bildirimi silah edinimi başarıyla tamamlandıktan sonra bir kez yayınlanır.

Ayarlı sahnelerde silah ve kapasitenin tek kaynağı asset'tir. Eski serialized silah alanı ve açık kapasite alan `Initialize(capacity)` test/fixture uyumluluğu için dar bir geçiş yolu olarak korunabilir. Ayar atanmışken legacy override ile çelişen iki kaynak sessizce birleştirilmez; bu kullanım açıkça reddedilir ve test edilir. Eski fixtures bu adımda topluca taşınmaz.

Mevcut iki demo sahnesi aynı oyun başlangıç asset'ine bağlanır: mevcut başlangıç silahı ve 2/5/8 kapasitesi korunur. Asset için önerilen konum `Assets/_Project/Data/Builds/SO_GameStartingLoadout.asset`. Mevcut sahne/prefab override'larından silah referansı uygulamada doğrulanır; farklılık varsa sessizce tek değere indirgenmez.

## Doğrulama kuralları

- Eksik silah, geçersiz silah yapılandırması veya geçersiz kapasite, build/mühimmat/yetenek durumunu değiştirmeden reddedilir.
- `PlayerBuildCapacity` üst sınırları 2/5/8 olarak kalır. Bunlar seçili başlangıç değerleri değil, mevcut desteklenen model sınırlarıdır; kapsam dışı kapasite genişlemesi yapılmaz.
- Ana geliştirme sahnesinde eksik başlangıç asset'i varsayılan değerlere sessizce düşmez; açık yapılandırma hatası verir.
- Gelecekte meta ilerlemenin hesapladığı başlangıç değerleri aynı runtime kurulumuna aktarılabilir. Bu adımda meta sağlayıcısı, kayıt sistemi veya soyut servis çerçevesi eklenmez.

## Beklenen dosya kapsamı

- Yeni `Scripts/Builds/PlayerStartingLoadoutSettings.cs`, asset ve gerekli meta dosyaları.
- `Scripts/Builds/PlayerStartingLoadoutInitializer.cs`.
- `Scripts/Development/Arenas/DemoOpeningController.cs`.
- `Scripts/Editor/DeepJamOpeningSceneBuilder.cs`: gelecekte oluşturulan sahnelere aynı bağlantıyı yazar; mevcut sahneler için çalıştırılmaz.
- `Scenes/Demo/DeepJam_Opening.unity` ve `DeepJam_WideCombat.unity`: yalnızca başlangıç ayarı referansları.
- İlgili build/başlangıç/scene layout testleri ve yeni ayar testleri.

Kaynak yolları `Assets/_Project/` altındadır. Bağımlı test fixture'ları referans taramasıyla kontrol edilir; sadece gerekli uyumluluk değişiklikleri yapılır. Elle düzenlenen geometri, düşman yerleşimi ve prefabın ilgisiz alanları korunur.

## Kabul ölçütleri

1. Mevcut ana sahne aynı silah ve 2/5/8 kapasitesiyle açılır.
2. Inspector'da asset üzerinden seçilen geçerli başka silah/kapasite sonraki run başlangıcında uygulanır; deneme sonrası ana ayar geri alınır.
3. HUD slotları gerçek build kapasitesini gösterir; yeni ücretsiz yetenek/güçlendirme verilmez.
4. Yetenek kurulum testi, ilk sandıktan edinim ve restart sıfırlaması çalışmaya devam eder.
5. Bozuk ayar kısmi kurulum üretmez; çift kurulum ve çelişen legacy override reddedilir.
6. Content arena ve eski fixtures'ın mevcut başlangıç düzenleri korunur.
7. Unity testlerini kullanıcı çalıştırır. Yeni build/test başarısı çalıştırılmadan raporlanmaz.

## Kapsam dışı

Sandık oranları, XP, hasar, cooldown, yeni içerik, meta progression, save/load, menü akışı ve sahne geometrisi. Diğer çalışma ağacı değişiklikleri bu işe dahil edilmez.

Teslim sırası: TR/EN TDD check-in → uygulama ve ilgili testler → kullanıcı kabulü → kod ve kabul dokümanı commit'leri.
