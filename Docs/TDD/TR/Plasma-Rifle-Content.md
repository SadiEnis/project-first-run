# Plasma Rifle içeriği

## Aşama ve kapsam

Content entegrasyon branch'inden açılan `feature/content/plasma-rifle` üzerinde tasarım checkpoint'i. Uygulama, mekanik kabulü ve docs kaydından sonra yapılır. Bu belge Weapon-Level-Effects içindeki geçici Plasma Rifle tablosunun yerini alır; yükseltme sırası GDD 11.4'e dayanır. Aşağıdaki sayılar önerilen test başlangıç değerleridir, nihai denge değildir.

`weapon.plasma-rifle`, mevcut asset, başlangıç silahı referansları, ödül havuzu üyeliği ve sekiz seviyeli sahiplik korunur. İkinci Plasma Rifle eklenmez, arena yerleşimi değiştirilmez. Evolution oynanışı, yakma alanı, roket değişikliği, mühimmat drop'u, altın/meta, nihai ses/model ve Windows build kapsam dışıdır.

## Önerilen mekanik

- Otomatik fiziksel enerji mermisi; kabul edilen atış başına bir mermi ve bir mühimmat. Hazırlık süresi, saçılma, kritik, patlama, güdüm, yerçekimi veya itme yok. Mevcut reload, kontrol ve toparlanan tepme hattı kullanılır.
- Mermi: 60 m/s hız, .08 m çarpışma yarıçapı, 100 m menzil, 2 s ömür. Adım kalan menzil/ömürle sınırlanır. Süre/menzil bitişinde hasarsız kaybolur.
- Namludan kamera hedef noktasına gider; kamera-namlu engeli duvarın ötesinde doğmayı önler. Süpürme başlangıç örtüşmesini ve aynı karede birden fazla düşmanı mesafe sırasıyla işler. Atan aktör ve trigger'lar dışlanır; duvar ve kapalı bariyerler dışlanmaz.
- Delme sayısı ek farklı hasar alıcılarını belirtir: sıfır toplam bir, bir toplam iki, iki toplam üç hedef. Çoklu collider olsa da her hedef aynı mermiden en fazla bir kez hasar alır. Her hedefte tam doğrudan hasar, azalma yok. Hasarı reddeden hedef de temas bütçesini tüketir; yanma yalnızca kabul edilen, öldürücü olmayan isabette uygulanır. Dünya geometrisi her zaman durdurur. Önceden vurulan hedef sonraki karede mermiyi içine hapsetmez.
- Seviye profili ve stat uygulanmış doğrudan/yanma hasarı fırlatma anında kopyalanır. Silah değişimi veya level-up havadaki mermiyi değiştirmez. Zaman ölçeklidir; kaynak ölümü/yok edilmesi ve harita boşaltılması bekleyen mermi/yanma hasarını iptal eder. Yalnızca silah değiştirmek iptal etmez.
- Tüm seviyelerde önerilen tepme: .3 derece, .10 s toparlanma gecikmesi, .22 s dönüş, 6 derece tavan. Atış başına bir tepme; delinen hedef veya yanma tick'i başına ek tepme yok.

## Önerilen seviyeler

Tüm seviyelerde 48 yedek mermi ve 1.5 s reload. Başlangıç şarjörü 12; kapasite artışı bedava mühimmat vermez.

| Seviye | Doğrudan hasar | Atış/s | Şarjör | Ek delinen hedef | Yanma |
| --- | --- | --- | --- | --- | --- |
| 1 | 25 | 5 | 12 | 0 | Yok |
| 2 | 30 | 5 | 12 | 0 | Yok |
| 3 | 30 | 6 | 12 | 0 | Yok |
| 4 | 30 | 6 | 12 | 1 | Yok |
| 5 | 35 | 6 | 12 | 1 | Yok |
| 6 | 35 | 6 | 20 | 1 | Yok |
| 7 | 35 | 6 | 20 | 2 | Yok |
| 8 | 35 | 6 | 20 | 2 | Var |

## Önerilen yanma sözleşmesi

- L8 hedef üzerinde yanma uygular, zeminde alan bırakmaz: 3 s boyunca .5 s arayla 5 temel hasar (yenilenmezse altı tick / 30 temel hasar). İlk tick anında değil .5 s sonra. Fırlatma anındaki WeaponDamage değiştiricisi doğrudan hasardan bağımsız, bir kez uygulanır.
- Kaynak/hedef başına tek Plasma yanması, üst üste birikme yok. Kabul edilen yeni isabet kalan süreyi 3 s yapar ve tick hasarını yeni atışın kopyasıyla değiştirir; bekleyen tick sayacı korunur. Sürekli ateş tick'leri engellemez veya anlık ekstra tick üretmez. Bitiş anındaki son planlı tick dahildir.
- Mevcut can/ölüm/ödül hattı kullanılır. Öldürücü vuruş, ölü/yok olmuş hedef, kaynak ölümü ve harita temizliği kalıcı hasar veya çift ödül bırakmaz. Hedef devre dışı bırakıldığında/yeniden kullanıldığında yanma temizlenir; havuzlanan hedef yanma devralmaz. Pause süre ve tick sayaçlarını durdurur. Yayılma ve genel statü sinerji altyapısı bu aşamada yoktur.
- Delta-time işleme normal kare hızlarında tick toplamını korur; kalan ömürle sınırlı telafi yapar, sınırsız birikmiş iş oluşturmaz. Hedefte küçük geçici görsel geri bildirim eklenir; her tick'te kalıcı yeni material üretilmez.

## Entegrasyon ve doğrulama planı

- Eski Hitscan/Rocket enum değerleri korunarak doğrulanan Plasma atış profili eklenir. Tüm seviyeler ve kayıtlı prefab edinim/atış mühimmatı değiştirmeden önce doğrulanır.
- Mevcut fırlatma olayı Rocket tipine bağlıdır. Olay uygun biçimde genişletilir/genellenir; tüm tüketiciler ve Rocket regresyonları birlikte güncellenir. İki fiziksel silah da sahte hitscan izi göstermez. Fırlatma, doğrudan isabet ve periyodik hasar ayrı anlam taşır; ek ateş/mühimmat/tepme bildirimi üretilmez.
- Kayıtlı mermi/geri bildirim asset'leriyle mevcut silah artımlı güncellenir. Arena geometrisi, başlangıç silahı seçimi ve ödül sırası korunur. Katalog sayısı artmaz. F1 delme/yanma bilgisini gösterir.
- Üretim Plasma asset'ini hitscan fixture'ı olarak kullanan testler incelenir. Üretim beklentileri GDD sırasına taşınır; hitscan davranış testleri doğrulamaları gevşetilmeden açık sentetik hitscan fixture'ları kullanır.
- EditMode: seviyeler, geçersiz profil/prefab, snapshot, eski atış türleri, kimlik/katalog tekilliği, yanma süre/yenileme sınırları ve mühimmat korunması.
- PlayMode: gerçek otomatik input/tepme/reload, kamera/namlu engeli, ince duvar, başlangıç örtüşmesi, aynı karede çoklu delme, collider tekilleştirme, reddedilen/öldürücü vuruş, yanma yenileme/pause/öldürme/yeniden kullanım, stat snapshot'ı, silah değişimi, oyuncu ölümü, harita boşaltma ve ödül entegrasyonu. Tam paketler çalıştırılır, sonra kullanıcı testi için durulur.

## Uygulama checkpoint'i

- Kabul edilen başlangıç değerleri mevcut `WD_PlasmaRifle` asset'ine işlendi; GUID ve sabit kimlik değişmedi. Hitscan/Rocket enum değerleri 0/1 kaldı; Plasma 2 oldu. Arena kataloğu, ödül havuzları, sahne yerleşimi ve kullanıcının başlangıç silahı seçimi korundu.
- `PlasmaProjectile` sıralı süpürme temaslarını mermiye ait hedef kaydıyla işler. `PlasmaBurnState` tick/yenileme zamanlamasını, `PlasmaBurn` hedef üzerinde kaynağa özgü hasarı yönetir. Hedef devre dışı/ölü olduğunda, kaynak öldüğünde/yok olduğunda temizlenir. Yanma görseli hedef üstünde geçici turuncu işarettir; nihai alev efekti veya zeminde alan değildir.
- Rocket tipli mevcut `ProjectileLaunched` yanına `PlasmaLaunched` eklendi. `ShotFired` yalnızca hitscan olarak kaldı; iki fiziksel mermi de gecikmiş kabul edilen hasarı `DamageApplied` ile bildirir. İz/debug tüketicileri iki fırlatma türünü de işler. Rocket olay uyumluluğu korunur.
- Kayıtlı Plasma mermisi/yanma görseli prefab'ları iki material paylaşır. Editor menüsündeki **Apply Plasma Rifle Content Defaults**, geçici denge tablosunu mevcut silaha yeniden uygular; özel ayarları korumak için tekrar çalıştırılmamalıdır. Çalışma anında sahne üretimi eklenmedi.
- Mevcut içerik testleri GDD seviye değerlerini bekler. Minigun edinim/geçiş testleri, sahne Minigun ile başlasa da normal edinim üzerinden iki silahlı düzeni kurar; kayıtlı sahneyi değiştirmez.
- Oynanış kabulü bekleniyor. Temiz ContentArena run'ında sahip değilsen F1 ile Plasma Rifle edin, Q ile geç; sıralı düşmanlarda L1/L4/L7 farkını, hayatta kalan hedeflerde L8 yanmasını dene. Değerler ve geçici görseller oynanış değerlendirmesine açıktır.
- 2026-09-25 otomatik doğrulama: izole Unity 6000.3.9f1 üzerinde **864/864 EditMode ve 516/516 PlayMode başarılı**. Raporlar: `.codex-temp/xp-attraction/plasma-final-EditMode.xml` ve `plasma-complete-PlayMode.xml`. Yeni 21 EditMode ve 16 PlayMode testi eklendi. Tam paket Rocket, Shotgun, Minigun ve mevcut ilerlemeyi de kapsar; eski Minigun testindeki bırakma input'u süreli yield sonrasında yeni kareye hizalanarak test zamanlama hatası giderildi. Değişen/yeni 36 Assets dosyası doğrulanan kopyayla eşleşir. Windows build veya manuel görsel kabul yapılmadı.
