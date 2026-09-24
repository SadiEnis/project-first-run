# Shotgun içeriği

## Durum ve çalışma düzeni

Tasarım checkpoint'i — 24 Eylül 2026. Git branch'i `feature/content/shotgun` olarak doğrulandı. Kararlaştırılan Plastic branch'i `/main/dev/content/shotgun`; bu adımda Plastic sunucu durumu yeniden kontrol edilmedi. Runtime uygulaması ve Shotgun asset'i henüz eklenmedi.

İçerik dönemi için branch akışı: Git `main → content → feature/content/shotgun`, Plastic `dev → content → shotgun`. Docs checkpoint'i ve ardından anlamlı uygulama checkpoint'i alınır. Otomatik doğrulama ve kullanıcı oynanış kabulünden sonra bu çalışma **content** branch'ine merge edilir; sonraki içerik güncel content'ten açılır. Oynanabilir içerik paketleri content'ten dev/main'e aktarılır. Bu, önceki her aşamayı dev/main'den açma kuralının içerik dönemi için kararlaştırılmış istisnasıdır.

Amaç, GDD 11.1'deki yakın mesafe ani hasar/kalabalık kontrolü silahını mevcut edinim, seviye, mühimmat ve ödül sistemleriyle uygulamaktır. Aşağıdaki detaylar ilk uygulama önerisidir; sayılar denge kararı değil, kullanıcı testi için başlangıç değerleridir.

## Atış sözleşmesi

- Kimlik `weapon.shotgun`, görünen ad `Shotgun`; silah kategorisi, maksimum sekiz seviye. Ücretsiz başlangıç silahı yapılmaz.
- Yarı otomatik: her fiziksel basış en fazla bir atış üretir. Basılı tutmak tekrarlamaz. Mevcut cooldown, reload, ölüm, panel ve reward UI engelleri geçerlidir.
- Bir başarılı tetik, hedefe değmese de **bir** fişek ve **bir** cooldown tüketir. Saçmalar ayrı atış sayılmaz. Ses/atış bildirimi de saçma sayısı kadar tekrarlanmaz; sonuç modeli tüm saçma izlerini ve hedef sonuçlarını taşıyabilmelidir.
- Saçmalar fiziksel projectile değil hitscan'dir. Kamera nişanı ve muzzle-engel kontrolü her saçma için korunur. Duvarlar hasar almasa bile atışı durdurur; trigger ve oyuncunun kendi collider'ları hedef değildir. Delme/sekme yoktur.
- Koninin yarı açısı 6 derece (tam açı 12 derece), azami menzil 30 m. İlk sürümde ayrıca mesafe hasar çarpanı yoktur; uzakta aynı hedefe daha az saçma isabet etmesi etkinliği düşürür.
- Saçılım koni içinde dairesel dağılım kullanır; kare dağılım veya bütün saçmaların tek ışına yığılması olmaz. Rastgelelik kaynağı testte belirlenebilir olmalı; Unity'nin global rastgele durumunu değiştirerek başka sistemlerin sonuçları etkilenmemelidir.
- Tüm saçmaların fizik sorgusu hasar/itiş uygulanmadan önce tamamlanır. Böylece ilk saçmanın öldürdüğü veya ittiği hedef diğer saçmaların geometrisini değiştirmez.
- Her saçma ilk engelde durur. Aynı hasar alıcısına ulaşan saçmalar hedef başına toplanır; birden fazla collider aynı saçmanın hasarını çoğaltmaz. Bir atışta aynı hedefe tek toplam `DamageInfo` uygulanır; farklı hedefler ayrı sonuç alır.
- WeaponDamage statı saçma başına temel hasara bir kez uygulanır; toplam, isabet eden saçmaların toplamıdır. Örneğin seviye 1'de statsız sekiz isabet 64 hasardır. Silah hasarı gösteriminde saçma başına değer ile teorik toplam birbirine karıştırılmaz.

## Mühimmat ve sekiz seviye

Başlangıç yedeği 48 fişek; atış hızı her seviyede 1.2 atış/s. Reload mevcut şarjör modelini kullanır: tek işlemde gereken fişekler yedekten aktarılır. Tek tek fişek doldurma yoktur.

| Seviye | Saçma hasarı | Saçma sayısı | Şarjör | Reload (s) | İtiş mesafesi (m) | Değişiklik |
| --- | --- | --- | --- | --- | --- | --- |
| 1 | 8 | 8 | 6 | 2.0 | 0.6 | Temel silah |
| 2 | 10 | 8 | 6 | 2.0 | 0.6 | Hasar |
| 3 | 10 | 8 | 8 | 2.0 | 0.6 | Kapasite |
| 4 | 10 | 8 | 8 | 2.0 | 1.0 | Knockback |
| 5 | 12 | 8 | 8 | 2.0 | 1.0 | Hasar |
| 6 | 12 | 8 | 8 | 1.6 | 1.0 | Reload |
| 7 | 12 | 8 | 10 | 1.6 | 1.0 | Kapasite |
| 8 | 12 | 10 | 10 | 1.6 | 1.0 | Saçma sayısı |

GDD'deki seviye sırası korunur. Sekizinci seviye bu aşamada yalnızca saçma sayısını artırır; evolution tarifi, asset'i, ödülü veya otomatik dönüşüm eklenmez.

Mevcut seviye sözleşmesi aynen geçerlidir: bütün seviyeler edinim öncesi doğrulanır ve runtime'a kopyalanır; asset değişikliği edinilmiş silahı değiştirmez. Seviye artırma aktif/pasif silahın aynı runtime kaydını, mevcut mermisini ve kalan cooldown/reload saniyelerini korur. Kapasite artışı bedava mermi vermez. Yeni saçma/itiş değerleri sonraki atışta kullanılır.

## Knockback sınırı

- İtiş opsiyonel bir combat alıcısı üzerinden uygulanır; silah çözümleyicisi doğrudan EnemyController'a bağlanmaz. İtiş desteklemeyen hedef yine normal hasar alır.
- Hasar kabul edilmiş ve hedef hayattaysa atış başına hedefe en fazla bir yatay itiş uygulanır. Yön muzzle'dan hedefe doğrudur. Saçma sayısı mesafeyi çarpmaz; ölümden sonra hareket başlatılmaz.
- İlk sürüm engelle sınırlanan tek yer değiştirmedir; ragdoll, havaya fırlatma, süreli stun veya ayrı bir statü sistemi değildir. NavMesh dışına, duvarın veya kapalı bariyerin içinden geçilmez. Fizik süpürmesi oyuncuyu da engel sayar; hedefin kendi collider'ları yok sayılır.
- EnemyMotor'un navigasyon ve saldırı hareketiyle sahiplik çatışması oluşturulmaz. İlk sürüm itişi saldırı cooldown'unu sıfırlamaz, Ranger'ın çıkmış mermisini silmez ve Charger'ın mevcut saldırısını stun gibi iptal etmez. Sonraki normal davranış güncellemesi yeni konumdan devam eder. Charger'ın mevcut çarpışmalı charge hareketi bozulmamalıdır.
- Ölü/devre dışı/henüz hazırlıkta olan veya geçerli NavMesh üzerinde olmayan düşmanda itiş reddedilir. Yeniden etkinleştirmede bekleyen itiş uygulanmaz.

## Mevcut kodla entegrasyon

- İncelenen WeaponDefinition şu anda tek hitscan, WeaponLevelData ise hasar/kapasite/hız/reload taşır. Saçma sayısı, koni ve itiş için immutable runtime atış verisi eklenmesi gerekir. Tek saçma, sıfır saçılım ve sıfır itiş varsayılanları mevcut PlasmaRifle/Development Secondary asset'lerini ve davranışını korumalıdır.
- Mühimmat/cooldown/reload kuralları WeaponRuntimeState'te kalır; alternatif loadout veya Shotgun'a özel edinim hattı kurulmaz. Küçük saçılım hesaplayıcısı, çoklu isabet çözümleme ve opsiyonel itiş alıcısı yeterlidir; genel bir silah framework'ü yeniden yazılmaz.
- Mevcut tek isabet taşıyan bildirimler tüm atış sonucunu taşıyacak biçimde genişletilirken debug/presentation tüketicileri ve testleri birlikte güncellenir. Bir tetik = bir atış bildirimi; hedef başına gerçek hasar bildirimi. Tek ışınlı silahlarda mevcut davranış korunur.
- Yeni Shotgun asset'i ve sekiz seviye verisi, ContentArena kataloğu ve Editor sahne üreticisi birlikte güncellenir. Kaydedilmiş sahne kullanılır; Play sırasında sahne kurma eklenmez.
- Shotgun, mevcut silah ödül havuzuna ve uygun karma havuza bir kez eklenir; sandık türleri veya ağırlık algoritması değiştirilmez. Yeni silah/seviye seçenekleri, iki slot sınırı ve maksimum seviye mevcut uygunluk kurallarına tabidir. Development Secondary bu adımda kaldırılmaz.
- Test için hafif, süreli saçma izi/isabet geri bildirimi sağlanır; her atıştan kalan sınırsız GameObject birikimi olmaz. Özgün silah modeli, animasyon, ses ve kamera recoil polish'i bu mekanik teslimin kapsamı değildir.

## Testler ve kabul

1. EditMode: sekiz seviye verisi; geçersiz/NaN/sonsuz değerler; saçma sayısı/açı sınırları; aynı kontrollü rastgele girdiden aynı dağılım; koni içindeki normalize yönler; tek-saçma geriye uyumluluğu; runtime kopyalama ve maksimum seviye.
2. PlayMode: bir basışta bir fişek/cooldown/bildirim; basılı tutma, boş şarjör, reload ve ölüm; hedef başına saçma toplamı, farklı hedefler, çoklu collider, duvar/muzzle engeli, menzil ve trigger filtreleme.
3. Knockback: hedef başına bir itiş; mesafe artışı; ölüm/hasar reddi/alıcısız hedef; duvar, oyuncu, kapalı bariyer ve NavMesh kenarı; Chaser/Charger/Ranger davranışlarının devam etmesi.
4. Aktif/pasif Shotgun seviye artırma, Q ile geçiş, ammo/cooldown/reload korunması, stat etkileri, bir oyuncunun seviyesinin başka oyuncuyu/asset'i etkilememesi.
5. Kayıtlı ContentArena'da panelden edinim ve tüm seviyeler; gerçek sandıktan yeni Shotgun/seviye seçimi, dolu slot/maksimum sınırı, mevcut silahlar için regresyon.
6. Kullanıcı yakın/orta/uzak hedefleri, kalabalığı, itişi, şarjör/reload hissini ve sekiz seviyeyi dener. Bu kabulden önce Minigun veya başka içerik uygulanmaz. Windows build çalışması açılmaz.

Bu docs checkpoint'inde runtime değişmediği için yeni otomatik test çalıştırılmadı. İçerik arenasının önceki doğrulaması 753 EditMode / 457 PlayMode'dur; bunlar Shotgun doğrulaması değildir.
