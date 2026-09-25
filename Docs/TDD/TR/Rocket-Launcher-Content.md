# Rocket Launcher içeriği

## Durum ve çalışma düzeni

Tasarım checkpoint'i. Git branch'i feature/content/rocket-launcher olarak doğrulandı; content entegrasyon akışı izlenir. Plastic branch'i /main/dev/content/rocket-launcher kullanıcı bildirimidir, ayrıca doğrulanmadı. GDD 11.3 takip edilir. Bu metin öneridir, uygulanmış oynanış kaydı değildir. Koddan önce iki dilli docs checkpoint'i alınır; silah tamamlanıp doğrulandıktan sonra kullanıcı testi için durulur. Evolution içeriği veya mermi kaynağı eklenmez.

## Önerilen mekanikler

- Kimlik weapon.rocket-launcher, Weapon kategorisi, sekiz seviye. Yarı otomatik: bir basış bir roket, bir şarjör mermisi ve bir cooldown tüketir. İlk aşamada hazırlık gecikmesi, kritik, güdüm, yerçekimi, sekme veya delme yoktur.
- Görünür fiziksel mermi muzzle'dan kameranın nişan noktasına gider. Kamera nişanı ve muzzle engeli kontrol edilir. Yakın duvarın öte tarafında roket oluşturarak engel atlanmaz.
- İlk öneri: 3 mermilik şarjör, 12 yedek, .8 atış/s, 3 sn reload, 25 m/sn roket, .15 m çarpışma yarıçapı, 60 m azami yol ve 4 sn ömür. Çarpmadan menzil/ömür dolarsa hasarsız silinir. Hareket adımı kalan yol/zamanla sınırlandırılır.
- Mermi hacmi hareket boyunca süpürülüp en yakın katı engel çözülür; yüksek hız ve başlangıçta iç içe collider durumu ele alınır. Ateşleyen hiyerarşi ve ilgisiz trigger'lar yok sayılır; duvar/kapalı bariyer yok sayılmaz. Yalnız OnTriggerEnter'a güvenilmez.
- İlk çarpma tam bir alan patlaması üretir; ayrıca doğrudan vuruş hasarı eklenmez. İlk öneri: 3 m yarıçap içindeki açık her hasar alıcısına 80 hasar, mesafe düşüşü yok. Mesafe collider'ın en yakın yüzeyinden ölçülür; çoklu collider aynı alıcıya hasarı çoğaltmaz.
- Dünya engelleri patlama etkisini keser. Hasar collider'ları kaldırmadan önce tüm adayların açıklığı belirlenir. Diğer hasar alabilen aktörler dünya engeli sayılmaz. Çarpma yüzeyi için küçük ve doğrulanmış boşluk noktası kullanılır; patlama/parçacıklar ince duvarın ötesine taşınmaz. Doğrudan vurulan hedef kendi patlama hasarını yanlışlıkla engellememelidir.
- Önerilen kendine hasar kuralı: roket, patlama ve parçacıklar ateşleyeni dışlar. Bu aşamada düşmana itiş, statü, stun veya rocket jump yoktur. Bu açıkça geçici oynanış tercihidir, GDD gereği değildir.
- Ateş anındaki stat uygulanmış hasar ve tüm seviye profili kopyalanır. Sonraki silah değişimi/seviye artışı havadaki roketi değiştirmez. Çarpma veya parçacık başına ek mühimmat/tepme yoktur.
- Boş şarjörde otomatik reload ve toparlanan tepme kullanılır. Tüm seviyeler için ilk tepme: 5° darbe, .10 sn bekleme, .35 sn dönüş, 10° sınır. Elle R ve mevcut pause/ölüm/kontrol kuralları korunur.
- Hareket/efektler oyun zamanını kullanır. Ateşleyen oyuncu ölürse veya sahibi olan harita boşaltılırsa roketler etkisizleşip temizlenir; ölüm sonrası ödül üretimi yoktur. Mermi sahipliği kalıcı oyuncu hiyerarşisinde değil ateşlenen haritada tutulur.

## Sekiz seviye önerisi

Sayılar oynanış testi başlangıç değerleridir; GDD yalnız gelişim sırasını belirler.

| Seviye | Patlama hasarı | Hız m/sn | Şarjör | Reload sn | Patlama yarıçapı m | Parçacık |
| --- | --- | --- | --- | --- | --- | --- |
| 1 | 80 | 25 | 3 | 3 | 3 | 0 |
| 2 | 100 | 25 | 3 | 3 | 3 | 0 |
| 3 | 100 | 35 | 3 | 3 | 3 | 0 |
| 4 | 100 | 35 | 4 | 3 | 3 | 0 |
| 5 | 100 | 35 | 4 | 2.4 | 3 | 0 |
| 6 | 120 | 35 | 4 | 2.4 | 3 | 0 |
| 7 | 120 | 35 | 4 | 2.4 | 4 | 0 |
| 8 | 120 | 35 | 4 | 2.4 | 4 | 8 |

## Sekizinci seviye parçalanması

- Bir çarpma patlaması sekiz görünür fiziksel parçacık çıkarır; ikinci patlama veya evolution değildir.
- İlk desen: doğrulanmış patlama noktasından yatay düzlemde eşit aralıklı radyal yönler. Hedef aramaz. Yakın geometriye yönelen parçacık engellenir; engelin ötesinde oluşturulmaz.
- İlk parçacık değerleri: 20 temel hasar, 20 m/sn hız, .05 m çarpışma yarıçapı, 8 m yol / .4 sn ömür. Ateş anındaki WeaponDamage çarpanı, patlama temel hasarından bağımsız olarak parçacığın temel hasarına bir kez uygulanır.
- Parçacıklar süpürmeli hareketle çarpışır; delmez, patlamaz, alt parçacık üretmez. Her parça bir kez çözülür. Tek roketin parçacık grubunda aynı alıcıya en fazla bir parçacık hasar verebilir; başlangıç patlaması da o alıcıya hasar verebilir. Ortak isabet kaydı bu gruba ve sınırlı ömrüne aittir.
- Parçacık çarpması mühimmat tüketmez veya yeni silah-atış/tepme bildirimi üretmez. Gecikmiş hasar sonuçları debug geri bildirimi için ayrıca görünür olur.

## Entegrasyon sınırları

- Mevcut silah controller'ı atışları hitscan volley çözümleyiciye gönderir. Fireball tek hedefli trigger mermisi, EnemyRangedProjectile süpürmeli tek hedef hareketi kullanır. Uygun çarpışma/yaşam döngüsü örüntüleri mevcut oynanış sözleşmeleri değiştirilmeden kullanılır.
- Silah verisine doğrulanan atış türü ve immutable roket/parçacık seviye verisi eklenir. Eski asset'lerin varsayılanı hitscan kalır. Gerekli mermi prefab/component'leri ve tüm seviyeler edinim/atış mühimmatı değiştirmeden önce doğrulanır.
- Fırlatma bildirimi ile gecikmiş çarpma/hasar ayrılır. Atış bildirimi tüketicileri/testleri birlikte güncellenir; hareketli roket için anlık hitscan izi taklit edilmez. Tek mühimmat, edinim, stat ve loadout hattı korunur.
- Kaydedilmiş roket/parçacık prefab'ları, sade geçici görseller ve sınırlı süreli patlama efekti eklenir. Ömür ve patlama başına sekiz parça sınırı açıktır; kalıcı iz/material/nesne birikmez. Havuzlama ön şart değildir; eklemeden önce ölçülür.
- Asset silah/karma ödül havuzlarına, kayıtlı ContentArena kataloğuna ve Editor üreticisine birer kez eklenir; artımlı bağlantı geometri ve ışığı korur. İki slot sınırı değişmez; temiz run'da PlasmaRifle ile Rocket Launcher test edilir.
- Diğer silahlar, Fireball, sahne geometrisi, altın/meta, yeni mermi drop'ları, nihai ses/model polish'i ve Windows build kapsam dışıdır. PlasmaRifle'ın planlanan projectile/delme/yanma genişletmesi daha sonraki içeriktir, bu branch'e dahil değildir.

## Doğrulama ve kabul

EditMode: sekiz seviye, sonlu/pozitif sınırlar, atış türü/prefab/profil tutarlılığı, immutable kopyalar, seviye artışında bedava mermi olmaması, eski varsayılanlar ve havuz/katalog tekilliği.

PlayMode: gerçek input, tetik başına tek fırlatma/mühimmat/tepme, duvar/muzzle/başlangıç örtüşmesi/yüksek hız çarpışmaları, tek patlama, yarıçap/engel/kendini dışlama, çoklu collider tekilleştirme, doğrudan vuruşun çifte hasar vermemesi, sınırlı parçalanma/grup isabet tekilleştirme, ömür, pause/ölüm/harita temizliği, havadaki mermide silah değişimi/seviye snapshot'ı, otomatik/elle reload, ödül edinimi/maksimum/dolu slot ve mevcut silah regresyonları.

Kullanıcı test listesi: yakın/orta/uzak atış, gruplanmış düşmanlar ve siper, üçüncü seviyede hız, dördüncüde kapasite, yedincide yarıçap, sekizincide parçacıklar, tepme ve otomatik reload. Kullanıcı oynanışı başarılı buldu ve bu aşamayı kabul etti; listedeki maddeler ayrı ayrı raporlanmadı. Sayısal denge geçicidir.

Ertelenen evolution fikri: parçacıklar çarptıkları yerde patlayıp süreli yakma alanları bırakabilir. Sonraki tasarım için fikir olarak kaydedildi; uygulanmış veya kesinleştirilmiş bir evolution mekaniği değildir.

## Uygulama checkpoint'i — 2026-09-25

- `WeaponDeliveryMode` varsayılanı Hitscan'dir; Rocket profilleri mevcut runtime entry içinde doğrulanır ve kopyalanır. Fireball ve diğer silah asset'leri değişmez.
- `RocketProjectile` küresel süpürme yapar ve adımı kalan menzil/ömürle sınırlar. Süpürülen merkez patlama noktasıdır; siper görünürlüğü hasardan önce hesaplanır. Parçalar patlama başına ortak isabet kaydı kullanır.
- `PlayerWeaponController.ProjectileLaunched` başarılı roket fırlatmasını bildirir. `ShotFired` hitscan volley sözleşmesini korur; `DamageApplied` gecikmiş patlama/parçacık hasarını bildirir. İz/debug tüketicileri ayrı fırlatma olayını dinler; sahte hitscan çizgisi gösterilmez.
- Kaydedilmiş asset'ler: `WD_RocketLauncher`, `RocketProjectile`, `RocketFragment`, `RocketBlast` ve üç paylaşılan material. Yalnızca Editor üreticisi eksik içeriği oluşturup havuz/kataloğu artımlı bağlar; Play sırasında sahne üretilmez.
- Kayıtlı arenada yedi katalog eşyası bulunur. Temiz run'da F1 ile Rocket Launcher edin, paneli kapatıp Q ile geç. F1 üzerinden seviye artır; L8 sekiz radyal parça ekler. Silah/karma sandıkları mevcut edinim/seviye artışı claim akışını kullanır.
- Havuzlama/performans iddiası veya Windows build doğrulaması kapsamda değildir. Görseller geçicidir; nihai ses/model ve mühimmat drop'ları sonraya bırakılmıştır.
- Otomatik doğrulama: izole Unity 6000.3.9f1 projesinde **843/843 EditMode, 500/500 PlayMode başarılı**. Raporlar: `.codex-temp/xp-attraction/rocket-final-EditMode.xml` ve `rocket-verified-PlayMode.xml`. Yeni 19 EditMode ve 11 PlayMode testi; seviye profilleri, eski atış türleri, snapshot, gerçek input/fırlatma/otomatik reload, süpürme/başlangıç/muzzle çarpışmaları, siper, tekilleştirme, parçalanma, ömür, pause/ölüm ve harita temizliğini kapsar. İlk tam çalıştırmada eski arena testinin kapanış yarışı görüldü; test temizliği artık asenkron unload öncesinde bekleyen drop üreticilerini durdurur. Değişen/yeni kaynak asset'leri test edilen kopyayla eşleşir.
