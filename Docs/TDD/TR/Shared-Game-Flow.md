# Ortak oyun akışı ve restart

## Durum

A dilimi uygulandı; kullanıcı pause/ölüm/finalden restart oynanışını onayladı ve check-in aldığını bildirdi. 30 Ekim 2026: B1 ve B2 kodu mevcut; B2 sahne bağlantıları tamamlandı. Kullanıcı B2 oynanışında sorun görmediğini ve testlerin yeşil olduğunu bildirdi; B2 kabul edildi. Ana geliştirme alanı mevcut oyun sahnesidir. Eski demo sürümü etiketinde kalır.

### B2 uygulaması ve doğrulama sınırı

- İki oyun sahnesindeki `Demo opening services` nesnesine `GameFlowController` ve `GameplayAccessController` eklendi. Giriş, pause, ödül seçimi ve restart serialized referanslarla bağlandı. Final/ölüm/pusu hatası mevcut sağlık/session durumlarından okunur; ikinci bir mutable run modeli yoktur.
- Giriş ve pause kendi token'larını alıp bırakır. Giriş yaklaşması aynı token ile değişir; pause kapanış animasyonu ve basılı tuş beklemesi bitmeden bırakmaz. Finalin Ending/Completed ve ölüm token'ını koordinatör mevcut kaynaklardan türetir. Ödül ekranı claim/multi-claim sunumunu korur; ortak yolda temel kontrol boolean'ını değiştirmez. Ölüm olursa açık ödül kapatılır; ölümün temel kontrol yasağı korunur.
- `GameplayAccessController` hedef sahnelerde zaman/ses/cursor'un tek yazarıdır. Motor/hasar/input/silah/yetenek kilitleri kendi sahibiyle uygulanır. Hasar koruması giriş/pause/final/yükleme/hata ekranındadır; ödül veya ölüm oyuncuyu ayrıca ölümsüz yapmaz. PlayerController ortak cursor sahibi bağlıyken cursor yazmaz. Input reader, kendi Awake'inden önce gelen kilit kaydını güvenle saklar.
- Temel zaman ölçeği Inspector'da 1'dir; normal ses politikası susturulmamıştır. Kamera/UI animasyonları unscaled kalır. Yeni yükleme önce Loading token'ı alır; başlatma hatasında yalnız bunu bırakır. Başarılı yükleme eski uygulayıcıyı emekliye ayırır. Uygulayıcının özel statik sahiplik işareti yalnız Unity'nin global alanlarına yazma hakkını sınırlar; servis bulma veya global oyun yöneticisi API'si değildir. Yeni sahne sahipliği aldıktan sonra eski sahnenin geç kapanması global değerleri ezemez.
- Taramada bulunan `DemoKeyAmbushController.RestartFailedRun` da ortak yükleyiciye taşındı. Yalnız `KeyAmbushPhase.Failed` durumunda `FailedEncounter` isteği kabul edilir. Hata ekranı dünyayı durdurup oyuncuyu korur; yükleme hatasında ekranda kalıp tekrar denenebilir. Normal pusu davranışı değişmez.
- Geometri, prefab görselleri, başlangıç loadout'u ve slot sayıları değiştirilmedi. Yeni sahne üretilmedi. Builder yeni opening kurulumuna aynı bağımlılıkları bağlar.

**Açık uyumluluk kapsamı:** `Test_ContentArena`, `Test_Waves`, `PlayableMapFixture`, `PlayableMapDestination` ve bağımsız reward test fixture'ları bu dilimde ortak yönetime taşınmadı. RewardSelectionUI prefab'ında `_flow` boş kalır; iki oyun sahnesi prefab override ile bağlar. Ortak yönetim yoksa reward'ın eski zaman/kontrol yolu ve PlayerController'ın eski cursor yolu korunur. Bunlar hedef iki sahnede çalışmaz. Haritalar arası SceneTravel ayrıca ele alınacaktır.

**Doğrulama:** Asistan runtime, EditMode ve PlayMode assembly'lerini statik derledi; Unity testlerini veya build çalıştırmadı. Kullanıcı testleri çalıştırıp yeşil olduklarını ve oynanışta sorun görmediğini bildirdi. Yeni build doğrulaması bildirilmedi. PlayMode derlemesindeki mevcut `PlayerChestInteractorTests.TearDown` CS0114 uyarısı bu işten bağımsızdır. Policy testleri artık 25 vaka; sahne bağlantı kontrolleri genişletildi. `RunRestartTests` giriş/pause sırasında ödül reddi, reward-ölüm çakışması, diğer owner'ların korunması, eski sahne temizliğinin yeni giriş ekranını ezmemesi ve pusu hata restart'ını kapsar. Pusu intermission testi doğrudan global zaman yazmak yerine flow token'ıyla durdurur.

Kullanıcıda hedef testler: EditMode `GameFlowPolicyTests`, `RunRestartOperationTests`, `DemoOpeningLayoutTests`; PlayMode `RunRestartTests`, `DemoEntryTests`, `DemoOpeningTests` ve mevcut `RewardSelectionControllerTests`. Manuel: girişten oynanışa geçiş, pause/resume, çok seçimli sandık, ölüm, final sesi/animasyonu ve üç ana ekrandan restart; klavye/mouse ve gamepad. Kullanıcı genel test/oynanış kabulünü verdi; cihaz bazında ayrı sonuç veya test sayısı bildirmedi.

### A dilimi kabul kaydı (B2 öncesi)

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

### B dilimi — somut sorumluluklar ve uygulama sırası

#### B1 uygulama durumu

- `Scripts/Flow/GameFlowContext.cs`: mevcut giriş/sağlık/final sistemlerinden gelecek salt okunur phase/readiness/temel kontrol snapshot'ı ve hesaplanan erişim sonucu. Yeni bir mutable run state machine değildir.
- `Scripts/Flow/GameFlowPolicy.cs`: geçiş uygunluğu, owner referans kimliği, `IDisposable` token, tekrar bırakma güvenliği ve saf erişim hesabı. Aynı owner'ın ikinci acquire isteği reddedilir; başka owner'ların aynı pause talebini tutmasına izin verilir. Pause/ödül çakışması reddedilir.
- `TryChange` yalnız giriş menüsü → kamera yaklaşması ve final animasyonu → sonuç için aynı token'ı günceller. Loading ayrı bir owner/token alır; yükleme hatasında bunun bırakılması kaynak ekranı korur. Ölüm/final restart'ı ilgili görünür ekranın token'ını gerektirir.
- `Reset` tüm eski token'ları geçersiz kılar. Eski token'ın gecikmiş Dispose çağrısı yeni run sahibini etkileyemez; başka policy'nin token'ı kaldırılamaz. Unity OnDisable/scene unload bağlantısı B2'nin sorumluluğudur.
- `FreezeWorld`, `MuteAudio` ve `BlockGameplay` uygulayıcıya taleptir; temel izinleri açma komutu değildir. `AllowsGameplay` dışarıdan gelen temel izni de dikkate alır. Normal zaman ölçeği ve ses tabanı bu sınıfta saklanmaz. Loading yalnız cursor sunumunu yakalar; eski zaman/kontrol snapshot'ını geri yüklemez.
- `Tests/EditMode/Arenas/GameFlowPolicyTests.cs`: 23 test vakası; retlerin yan etkisizliği, iki sahibin farklı sırada kapanması, loading hatası/tekrar deneme, ölüm-ödül çakışması, temel izin, atomik kamera geçişi, reset ve referans kimliği.

B1 tesliminde runtime ve EditMode test assembly'leri statik olarak derlendi; Unity testleri çalıştırılmadı. B1 tek başına sahne/controller davranışı değiştirmiyordu. Sonraki B2 bağlantıları ve doğrulama durumu yukarıda kayıtlıdır.

İncelemede `PlayerController.ApplyControlState/OnDisable` içinde de cursor yazımı bulundu. `RewardSelectionController` önceki zamanı ve tek boolean kontrol değerini saklayıp geri yüklüyor. `PlayerDeathController` kontrol/silah/yetenek temel izinlerini sağlık olaylarıyla değiştiriyor. Yalnız ekran dosyalarını değiştirmek sahipliği tamamlamaz.

Önerilen sınıflar `Scripts/Flow/` altında olacak; isimler uygulamada sadeleştirilebilir:

| Parça | Sorumluluk | Yönetmeyeceği işler |
| --- | --- | --- |
| `GameFlowPolicy` | Saf C#: kaynak durumuna göre geçiş izni; etkin sahiplerden zaman/ses/imleç ve kontrol gereksinimlerini hesaplama | Unity, sahne yükleme, sağlık azaltma, UI animasyonu |
| `GameFlowController` | Sahne bağlantıları, ekranların sahiplik talepleri, mevcut ölüm/final durumlarını okuma; policy sonucunu uygulayıcıya iletme | İkinci sağlık/run modeli, eşya dağıtma, düşman yönetme |
| `GameplayAccessController` | Hesaplanan sonucu Unity zaman/ses/imleç API'lerine ve mevcut owner kilitlerine uygulama | Geçiş kararları, buton/kamera sunumu |
| Mevcut `RunRestartController` | Kabul edilmiş yeniden yükleme işlemi ve hata sınırı | Pause/ödül/final sunumu |

Akış: ekran isteği → policy uygunluk kontrolü → sahneye ait sahiplik kaydı → hesaplanan erişim durumu → Unity uygulayıcısı. Her sistemin üstünde başka bir yönetici gerekmez; Inspector bağlantıları bunları birleştirir. Global singleton veya her bileşenin eriştiği servis deposu kullanılmaz.

**Sahiplik sözleşmesi:** Her ekran kendine ait bir token alır; tekrar kapatma güvenlidir. Token bırakılması yalnız o talebi kaldırır, eski global değerlerin kopyasını geri yazmaz. Zamanı durdurma ve sesi susturma talepleri birleşir: en az bir etkin talep varsa ilgili engel sürer. Cursor için açık öncelik kullanılır: Loading > terminal ekran/final > giriş > ödül > pause > oynanış. Öncelik izinsiz ekranları açma hakkı vermez. Pause ile ödül aynı anda kabul edilmez. Başka bir sistemin oyuncu kilidi kaldırılmaz ve ölümün kapattığı temel izinler açılmaz.

**İlk davranış hedefi:** Görünüm ve mevcut oynanış korunur.

| Durum | Dünya zamanı | Ses | Cursor / oyuncu |
| --- | --- | --- | --- |
| Oynanış | Normal oyun ölçeği | Normal ses politikası | Kilitli/gizli; temel izinler ve diğer sahipler uygunsa kontrol açık |
| Giriş menüsü / kamera yaklaşması | Durur | Yeni bir global susturma eklenmez | Menüde görünür; yaklaşmada gizli; kontrol kilitli |
| Pause | Durur | Pause susturması | Görünür/serbest; kontrol kilitli |
| Sandık seçimi | Durur | Yeni bir global susturma eklenmez | Görünür/serbest; kontrol kilitli |
| Ölüm | Mevcut gibi dünya devam eder | Mevcut gibi | Görünür/serbest; ölüm sistemi temel kontrolleri kapatır |
| Final animasyonu / sonuç | Durur | Final sesleri korunur | Animasyonda gizli/kilitli, sonuçta görünür/serbest; kontrol kilitli |
| Yeniden yükleme | Durur | Susturulur | Kaynak ekranın cursor görünümü korunur; ekran ve oyuncu etkileşimi kapalı |

Normal zaman ölçeği eski bir ekranın snapshot'ından alınmaz; sahne oturumunun temel değeridir. Bu dilim yavaş çekim sistemi eklemez. UI ve kamera geçişleri unscaled time kullanmayı sürdürür. Menü seçme/onaylama input'u oyuncu kilidinden etkilenmez; basılı onay/geri tuşunun oyuna sızmasını önleyen mevcut bekleme korunur.

**Dosyalarda yapılacaklar:**

- `DemoEntryController`: kamera ve giriş görünümü kalır; menü/yaklaşma/oynanış geçişlerini bildirir, yerel zaman/cursor snapshot'ları kaldırılır.
- `DemoPauseController`: kartlar, animasyon ve input kalır; açık/kapalı talepleri ortak katmana taşınır. Resume kapanış animasyonu ve input bırakılması tamamlanmadan token bırakmaz.
- `RewardSelectionController`: teklif/claim/multi-claim sorumluluğu kalır; doğrudan zaman ve `SetControlEnabled` geri yüklemesi kaldırılır. Son seçim veya iptal tek kez sahiplik bırakır; ölümle çakışmada oyuncu yeniden etkinleşmez.
- `DemoFinalController`: `DemoFinalSession` ve Cinemachine/sunum kararı korunur; global yan etkiler ortak katmana taşınır. Finalin tamamlanması mevcut session'dan okunur.
- `DemoOpeningController`: ölüm menüsü sunumu kalır; görünürlük/restart uygunluğu ortak kurallarla uyumlu olur.
- `PlayerController`: ortak yönetim bağlıyken bağımsız cursor yazmaz. `PlayerDeathController` sağlık olaylarının üreticisi/temel ölüm kontrolü olarak kalır; UI kapatılması bu durumu geri alamaz.
- `RunRestartController`: mevcut işlem sınıfı ve yükleyici korunur. Loading talebi önce alınır; başlangıç hatasında yalnız bu talep bırakılır. Önceki pause/ölüm/final durumu yeniden hesaplanır.
- İki kayıtlı sahne ve ilgili builder/test fixture bağlantıları güncellenir. `RewardSelectionController` ve `PlayerController` kullanan diğer sahne/prefab/test kurulumları taranır; ortak katman zorunlu yapılmadan önce açıkça bağlanır. Sessiz otomatik manager oluşturma veya yalnız demo sahnesinde çalışan gizli bağımlılık eklenmez.

**Küçük teslimler:**

1. B1: saf policy, sahiplik/token sözleşmesi ve çakışma testleri. Henüz mevcut ekranların davranışı değişmez.
2. B2: Unity uygulayıcısı, sahne koordinasyonu ve yukarıdaki tüm global yazarların birlikte taşınması. Yalnız pause'u taşıyıp kalanlarla aynı global alanları kontrolsüz paylaşan bir ara sürüm kabul edilmez.
3. Kullanıcı testleri ve oynanış kabulü; ardından ayrı check-in. Görsel yenileme veya yeni oyun mekaniği eklenmez.

**Kabul kontrolleri:** İki sahibin farklı sırada kapanması; aynı token'ın iki kez bırakılması; temel izin kapalıyken UI kapanması; giriş/pause/ödül/final sırasında reddedilen isteklerin yan etkisiz olması; multi-claim odağının korunması; ödül açıkken ölüm; pause kapanırken restart; yükleme hatası/yeniden deneme; eski sahnenin temizliğinin yeni sahnenin zaman/cursor'unu değiştirmemesi; sahne devre dışı kalınca sarkan token kalmaması. Mouse/klavye ve gamepad, tüm ekran animasyonları ve final sesi manuel doğrulanır. Yeni run nesli eski token'ları geçersiz kılar; global uygulayıcının sahne devri açıkça test edilir.

Önceki test sahnelerinde ortak yönetim kullanılmayan kurulumlar geçiş boyunca açıkça desteklenir veya aynı check-in'de taşınır; geride kalan fallback varsa belgeye yazılır. B2 sonunda hedef iki sahnede zaman/ses/cursor için tek uygulama sahibi bulunduğu kod taramasıyla doğrulanır.

A için saf testler: başarılı kabul, reddedilen kaynak, çift istek, yükleyici hatası/null operation, hata sonrası tekrar deneme. Entegrasyon: pause/ölüm/finalden restart, menü atlama, yeni build ve mühimmat sıfırlaması, yüklemede kontrol engeli, mevcut owner kilidini koruma.

Unity testlerini kullanıcı çalıştırır; asistan gerektiğinde yalnızca ilgili başarısız kapsamla ilgilenir. Derleme, test ve build sonuçları ayrı raporlanır. Hata simülasyonu test yükleyicisiyle yapılır; gerçek sahne asset'leri bozulmaz.

Görsel yenileme, IMGUI → Canvas taşıması, meta, kayıt, XP/ödül dengesi, düşman ve geometri bu parçanın dışında. Mevcut 1/3/5 başlangıç tercihi korunur. Önceden bulunan ilgisiz materyal değişikliği ve performans dosyası silinmeleri dahil edilmez.

Teslim: TR/EN TDD → Parça A uygulama/test/kullanıcı kabulü/commit → Parça B ayrıntılandırma. Bu doküman bütün akışın tek seferde değiştirileceği anlamına gelmez.
