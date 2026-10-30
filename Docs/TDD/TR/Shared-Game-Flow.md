# Ortak oyun akışı ve restart

## Durum

A dilimi uygulandı; Unity test çalıştırması ve oynanış kabulü bekleniyor. Ana geliştirme alanı mevcut oyun sahnesidir. Eski demo sürümü etiketinde kalır.

### A dilimi uygulaması

Üç ekran artık iki kayıtlı sahnedeki Demo opening services üzerinde bulunan ortak `RunRestartController` bileşenine istek iletiyor. `RunRestartOperation` tek etkin yükleme isteğini korur ve başlangıç hatasından sonra yeniden denemeye izin verir. Editor/player yükleme API'sini yalnız `UnityRunSceneReloader` çağırır. Mevcut oyuncu kilit sahipleri korunur. Geometri ve başlangıç kapasitesi değiştirilmedi.

Eski sahne kaldırılana kadar zaman/ses durur ve restart'ın kendi oyuncu kilitleri tutulur. Başarılı restart sırasında pause/final temizliği global durumu geri yüklemez. Hedef sahneye özel varış işareti, yeni entry bileşeninden önce zaman/ses/imleci oyun durumuna alır; mevcut başlangıç akışı yeni oyuncuyu hazırlar. Entry kendi menü atlama işaretini tüketir; eşleşmeyen sahnelerde işaret temizlenir.

Hata telafisi, yükleme başlamadan oluşan senkron exception veya null yükleme işlemiyle sınırlıdır. Kaynak ekranın zaman/ses/imleç durumu korunur; iki işaret temizlenir ve yalnız restart'ın kilitleri kaldırılır. Unity asenkron yüklemeyi kabul ettikten sonra eski sahneye güvenli dönüş garanti edilmez; varsayımsal zaman aşımı/yeniden deneme eklenmedi.

Doğrulama: runtime ve EditMode/PlayMode test assembly'leri statik olarak derlendi. Saf işlem testleri, kayıtlı sahne bağlantı kontrolleri ve sahte yükleyiciyle pause/ölüm/final, tekrar deneme, çift istek, kilit sahipliği ve sahne kaldırma/varış PlayMode testleri eklendi. Asistan Unity testlerini/build'i çalıştırmadı. Manuel kabul: üç ekrandan restart, temiz başlangıç ve kontrol/ses dönüşü, hızlı yinelenen istekler. Tüm ekran/zaman sahipliğinin ortaklaşması B diliminde kalır.

## İlk inceleme ve sorumluluklar

- `DemoOpeningController.Restart`: ölüm koşulunu kontrol edip menüyü atlama işaretini koyuyor, zamanı 1 yapıp sahneyi yüklüyor.
- `DemoPauseController.Restart`: pause sahipliğini bırakıp sahneyi yüklüyor; hata olursa menüyü yeniden açmaya çalışıyor.
- `DemoFinalController.Replay`: final session'dan izin alıyor, yükleme başarısızsa replay isteğini geri alıyor.
- `DemoEntryController`: sonraki yüklemede giriş menüsünü atlama bilgisini taşıyor; ayrıca kendi kontrol/zaman yönetimi var.
- `RewardSelectionController` da zaman ve kontrol yönetiyor. Bu nedenle yalnızca pause'u ortaklaştırmak bütün global sahiplik sorununu çözmez.
- `RunSessionController` arena sıralaması ve run sonuçları için mevcut bir temeldir; demo ekran akışının merkezinde değildir. Bu görevde değiştirilmez.

Akış kuralları, Unity yükleme işlemi, ekran sunumu ve run ilerlemesi ayrı sorumluluklardır. Global singleton veya her şeyi bilen bir GameManager eklenmez.

## Parça A — İlk uygulanacak iş: tek restart işlemi

### Yeni kod için önerilen kapsam

`Assets/_Project/Scripts/Flow/` altında:

- `RunRestartOperation`: saf C# işlem sahibi. Idle/Loading/Failed geçişlerini, tek aktif isteği ve hata sonucunu yönetir; Canvas, Cinemachine veya Unity SceneManager bilmez.
- `IRunSceneReloader`: yalnızca aynı run sahnesini yeniden yüklemek için küçük bağımlılık sınırı. Genel servis deposu değildir.
- `UnityRunSceneReloader`: Editor ve player build yükleme yollarını uygular; null operation ve başarısız başlangıcı açık hata olarak döndürür.
- `RunRestartController`: sahnedeki tek bağlantı noktası; serialized referansları bağlar, ekranlardan gelen istekleri ortak işleme aktarır. İşlem durumunu UI'a bildirir. Bu, ilerideki genel GameFlowController'ın yerine her işi üstlenen bir sınıf olmaz.

Bunlar önerilen adlardır. Uygulama sırasında aynı sorumlulukları daha az sınıfla karşılamak mümkünse gereksiz katman eklenmez.

### Mevcut dosyalardaki değişiklikler

- `Scripts/Development/Arenas/DemoOpeningController.cs`: yerel sahne yükleme kodu kaldırılır, ölüm ekranı ortak restart talebi gönderir.
- `Scripts/UI/Hud/DemoPauseController.cs`: aynı talebi gönderir; görünüm, kart yerleşimi, input ve animasyon değişmez.
- `Scripts/Development/Arenas/DemoFinalController.cs`: aynı talebi gönderir; dev düşman sekansı ve final session kuralı korunur.
- `Scripts/Development/Arenas/DemoEntryController.cs`: menüyü atlama isteğinin tek sahibi ortak restart olur; mevcut sahne hedefleme ve hata temizliği korunur.
- İki kayıtlı demo/oyun sahnesi ve `DeepJamOpeningSceneBuilder`: ortak controller referansları eklenir. Sahne yeniden üretilmez.
- İlgili EditMode/PlayMode testleri: ortak işlem kuralları ve üç ekranın entegrasyonu.

### İşlem sözleşmesi

1. Yalnızca izinli kaynak kabul edilir: açık pause, görünür ölüm ekranı veya tamamlanmış final. Açılış/kamera geçişi, sandık seçimi ve devam eden final animasyonunda restart kabul edilmez.
2. İlk kabul edilen istek yüklemeyi başlatır; çift tıklama, eşzamanlı ekran isteği ve tekrar input ikinci yükleme oluşturmaz.
3. Ekranlar ve oynanış geçiş boyunca etkileşime kapalı kalır. Eski sahne yükleme sırasında kısa süreli oynanabilir hâle gelmemeli.
4. Giriş menüsünü atlama bilgisi yalnızca hedef sahne için kurulur. Başarısızlıkta temizlenir; ilgisiz sahneye sızmaz.
5. Başarılı yükleme yeni run verisini ve kontrolü mevcut başlangıç yolu üzerinden kurar. Eski OnDisable temizliği yeni sahnenin kilitlerini/zamanını ezmemeli.
6. Yükleme başlayamazsa istek geri alınır; kaynak ekran, önceki zaman/audio/cursor ve kendi kilitleri tutarlı kalır; tekrar deneme mümkündür. Başka sistemlerin kilitleri kaldırılmaz.
7. Kaynak sahne boşaltıldıktan sonra eski ekranın geri yüklenebildiği varsayılmaz. Unity yükleyicisinin güvenle toparlayabildiği hata sınırı test ve dokümanda açık belirtilir.
8. Sahne yeniden yükleme ile haritalar arası `SceneTravelController` transferi farklı işlemlerdir; transfer sistemi değiştirilmez.

## Parça B — Ortak ekran ve oyun akışı

A'nın kabulünden sonra ayrı uygulama/check-in:

- Saf geçiş kuralları ve küçük `GameFlowController` ile giriş, oynanış, ölüm, final ve yükleme uygunluğu koordine edilir.
- Pause ve ödül seçimi run sonucundan ayrı ele alınır; bütün olasılıklar tek dev enum'a sıkıştırılmaz.
- Zaman, audio ve cursor sahipliği birlikte incelenir; entry, pause, reward ve final aynı sözleşmeye taşınmadan merkezi yönetim tamamlandı denmez.
- Mevcut owner tabanlı oyuncu kilitleri korunur. Bir ekranın kapanması yalnızca kendi sahipliğini bırakır.
- Ekranlar animasyon/input sunumuna odaklanır; geçiş izni ve restart uygulaması taşımaz.
- Sağlık/ölüm olayının mevcut üreticisi tek kaynak kalır; ayrı bir ikinci sağlık veya run durumu kopyası üretilmez.
- RunSession ile serbest harita ilerlemesinin entegrasyonu ayrıca değerlendirilir; arena temizleme zorunluluğu eklenmez.

## Doğrulama ve kapsam dışı

A için saf testler: başarılı kabul, reddedilen kaynak, çift istek, yükleyici hatası/null operation, hata sonrası tekrar deneme. Entegrasyon: pause/ölüm/finalden restart, menü atlama, yeni build ve mühimmat sıfırlaması, yüklemede kontrol engeli, mevcut owner kilidini koruma.

Unity testlerini kullanıcı çalıştırır; asistan gerektiğinde yalnızca ilgili başarısız kapsamla ilgilenir. Derleme, test ve build sonuçları ayrı raporlanır. Hata simülasyonu test yükleyicisiyle yapılır; gerçek sahne asset'leri bozulmaz.

Görsel yenileme, IMGUI → Canvas taşıması, meta, kayıt, XP/ödül dengesi, düşman ve geometri bu parçanın dışında. Mevcut 1/3/5 başlangıç tercihi korunur. Önceden bulunan ilgisiz materyal değişikliği ve performans dosyası silinmeleri dahil edilmez.

Teslim: TR/EN TDD → Parça A uygulama/test/kullanıcı kabulü/commit → Parça B ayrıntılandırma. Bu doküman bütün akışın tek seferde değiştirileceği anlamına gelmez.
