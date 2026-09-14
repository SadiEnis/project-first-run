# Common kategori sandıkları

## Kapsam ve branch kararı

Nadirlik/tablo uygulaması **cs:156 / 01b1d29** sonrasında `/main/dev/chest-tier-foundation` / `codex/chest-tier-foundation` üzerinde devam edilir. Kullanıcı temel ve ilk türlerin aynı branch'te tamamlanıp testten sonra birlikte merge edilmesini açıkça seçmiştir. Yeni branch veya ara merge yapılmaz.

Bu adım GDD 15.1–15.3 için Weapon, Ability ve Upgrade sandıklarının yeni eşya edinme yolunu ekler. Hepsi Common'dır; en fazla üç farklı uygun seçenek sunar ve bir seçimle tamamlanır. İçerik şimdilik azdır: silah havuzunda Plasma Rifle ve Development Secondary (başlangıç silahı zaten sahip olunan eşyadır), yetenekte Fireball, geliştirmede Development Damage Boost vardır. Üçten az uygun seçenek normaldir. Eşya seviyesi artırma henüz yoktur; bunlar kategoriye özel ilk edinme sandıklarıdır, GDD'deki ödül ilerlemesinin tamamı değildir.

## Kategori sözleşmesi

`ChestRewardCategory` açık değerlerle Mixed=0, Weapon=1, Ability=2, Upgrade=3 kullanır. Mixed eski geliştirme tanımını korur. `ChestDefinition`, kategori ve isteğe bağlı görünen adı; nadirlik, havuz ve seçenek sayısından ayrı tutar. Görünen ad yoksa StableId kullanılır.

Türlü tanımlar havuzda farklı kategoriye ait eşya varsa reddedilir. Bu bir içerik hatasıdır; yanlış adayı sessizce filtreleyerek gizlenmez. Doğrulama spawn/başlatma sırasında ve teklif üretiminden önce yeniden yapılır; sonradan değiştirilen havuz kategori sınırını aşamaz. Geçersiz veri sandığı Available bırakır, modal açmaz. Mevcut uygunluk filtresi sahip olunan eşyaları ve dolu kategorileri eler; edinme/tamamlama yetkisi mevcut claim/oturum/UI katmanlarında kalır.

## İçerik ve test sahnesi

- Mevcut Chests/Dev ve Reward/Dev klasörlerine üç sandık tanımı ve kategoriye özel üç havuz eklenir. Aynı sandık prefabını paylaşırlar.
- Bağımsız kaynak tablolarının her biri Weapon/Ability/Upgrade tanımlarını geçici 1:1:1 ağırlıklarla içerir. Level-up yine bir sandığı garantiler. Düşman ihtimali %25, henüz kullanılmayan elit örneği %50 kalır. Ağırlıklar nihai denge değil test ayarıdır.
- `ChestView`, Common kategori sandıklarına örnek başına gri renk ve kameraya dönük tek ad etiketi uygular; ortak materyaller değişmez. Etiket mevcut visual root'a bağlıdır ve seçim tamamlanınca onunla kaybolur. LateUpdate'te ana kamerayı takip eder; ana kamera yoksa son yönünü korur. Render kontrolünde ön/arka etiketlerin çift taraflı yazı materyali nedeniyle ters ve çift göründüğü saptandığı için taslaktaki sunum tek etiketle değiştirildi. Mixed geliştirme sandığının görünümü korunur. Bu geçici sunumdur; nihai sanat veya yerelleştirme değildir.
- Editor'a özel başlangıç düzeneği öne Weapon, soluna Ability, sağına Upgrade sandığı üretir. Ek sandıklar mevcut bootstrap'ta yapılandırılır; boş ek liste önceki tek sandık davranışını korur. Tanımlar üretimden önce doğrulanır, tekrar üretim reddedilir ve kısmi başarısızlıkta üretilen grup temizlenir.
- Otomatik `UpgradeDevelopmentBootstrap` yalnızca Test_Waves'te kapatılır; aksi halde tek geliştirmeye sandık açılmadan sahip olunurdu. Sandıktan edinildiğinde aynı %20 silah/yetenek hasar bonusu uygulanır. Diğer saldırı/test sahneleri değiştirilmez.

## Sınırlar ve devamı

Test_Waves'te `FireballDevelopmentBootstrap.Grant Starting Ability` false yapılır: yetenek fabrikası kaydı kurulur fakat Fireball sandıktan edinilir. Yeni seçenek varsayılan olarak true kaldığından diğer sahneler korunur. Başlangıç düzeneğinin mevcut iki silah slotu korunur; varsayılan tek slotlu oyuncu kapasitesi değişmez.

Sandık hâlâ E/gamepad South ile açılır. Boş teklif sandığı kullanılabilir bırakır ve modal açmaz; altın telafisi veya başka kategoriye yeniden çekiliş yoktur. Üç örnek edinildikten sonra sonraki birçok sandıkta uygun ödül kalmaması normaldir; drop kaynağı hatası değildir.

Eşya seviyesi ödülleri, yeni eşya içerikleri, evolution, Golden Chest ve altın telafisi ayrı adımlardır. Karma Green/Purple içerikleri ile Legendary/Boss çoklu seçimi [Gelişmiş Sandık Türleri](Advanced-Chest-Types.md) içinde tanımlanır. Enum veya gri renk bu davranışları tek başına eklemiş sayılmaz.

## Doğrulama

2026-09-14 kullanıcı onayı: üç başlangıç sandığı da oynanışta beklendiği gibi çalıştı. Sandık temeli/Common edinme adımının birleştirilmesi onaylandı. Eşya seviyesi ödülleri sonraki aşamadır.

2026-09-14 tarihinde izole Unity render önizlemesi gözle incelendi: üç gri örneğin her birinde tek okunabilir ad etiketi var; önceki ters/çift yazı giderildi. Bu bir sunum kontrolüdür, Test_Waves oynanış denemesi değildir.

Otomatik testler kategori doğrulaması/yanlış kategorili havuz, eski Mixed uyumluluğu, gerçek içerik/tablo/sahne bağlantıları, doğru kategorili teklif ve tek seçim, spawn sonrası havuz bozulması, geçici etiket/renk ve başlangıç grubunun davranışını kapsar. Check-in öncesi iki test paketi tam çalıştırılır.

2026-09-14 doğrulaması: izole Unity 6000.3.9f1 test projesinde tam paketler **626 EditMode / 332 PlayMode** ile geçti. Bu adım 22 EditMode ve 6 PlayMode testi ekler. Yeni/değişen 29 Unity dosyası SHA-256 ile test kopyasıyla eşleştirildi. Başlangıç testi sahnenin iki silah slotlu kapasitesini kullanır ve Fireball fabrikasını sahiplik vermeden kurar; üç kategori de gerçek seçim düğmeleri üzerinden edinilir. Diğer sahneler için varsayılan Fireball verme davranışı da testlidir. Bu sonuçlar kullanıcının oynanış onayının yerine geçmez.

Elle Test_Waves: yeni run aç. Etiketli üç başlangıç sandığından sırasıyla Development Secondary, Fireball ve Development Damage Boost edin. Q ile yeni silaha geç; geliştirme seçim öncesinde otomatik verilmemiş olmalı. Düşman öldürüp XP toplayarak üretilen sandıkların üç kategoriyi kullandığını, düşman ihtimali ve level başına bir sandık kuralının korunduğunu kontrol et. Havuzu tükenmiş kategoriyi tekrar açmaya çalışmak oyunu duraklatmamalı veya sandığı tüketmemeli. Elle onay otomatik test sonuçlarından ayrı kaydedilir.
