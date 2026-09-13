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
- `ChestView`, Common kategori sandıklarına örnek başına gri renk ve ön/arka ad etiketi uygular; ortak materyaller değişmez. Etiketler mevcut visual root'a bağlıdır ve seçim tamamlanınca onunla kaybolur. Mixed geliştirme sandığının görünümü korunur. Bu geçici sunumdur; nihai sanat veya yerelleştirme değildir.
- Editor'a özel başlangıç düzeneği öne Weapon, soluna Ability, sağına Upgrade sandığı üretir. Ek sandıklar mevcut bootstrap'ta yapılandırılır; boş ek liste önceki tek sandık davranışını korur. Tanımlar üretimden önce doğrulanır, tekrar üretim reddedilir ve kısmi başarısızlıkta üretilen grup temizlenir.
- Otomatik `UpgradeDevelopmentBootstrap` yalnızca Test_Waves'te kapatılır; aksi halde tek geliştirmeye sandık açılmadan sahip olunurdu. Sandıktan edinildiğinde aynı %20 silah/yetenek hasar bonusu uygulanır. Diğer saldırı/test sahneleri değiştirilmez.

## Sınırlar ve devamı

Sandık hâlâ E/gamepad South ile açılır. Boş teklif sandığı kullanılabilir bırakır ve modal açmaz; altın telafisi veya başka kategoriye yeniden çekiliş yoktur. Üç örnek edinildikten sonra sonraki birçok sandıkta uygun ödül kalmaması normaldir; drop kaynağı hatası değildir.

Eşya seviyesi ödülleri, yeni eşya içerikleri, karma Green/Purple içerikleri, evrim, Legendary/Boss çoklu seçimi, Golden Chest ve altın telafisi ayrı adımlardır. Enum veya gri renk bunları eklemiş sayılmaz.

## Doğrulama

Otomatik testler kategori doğrulaması/yanlış kategorili havuz, eski Mixed uyumluluğu, gerçek içerik/tablo/sahne bağlantıları, doğru kategorili teklif ve tek seçim, spawn sonrası havuz bozulması, geçici etiket/renk ve başlangıç grubunun davranışını kapsar. Check-in öncesi iki test paketi tam çalıştırılır.

Elle Test_Waves: yeni run aç. Etiketli üç başlangıç sandığından sırasıyla Development Secondary, Fireball ve Development Damage Boost edin. Q ile yeni silaha geç; geliştirme seçim öncesinde otomatik verilmemiş olmalı. Düşman öldürüp XP toplayarak üretilen sandıkların üç kategoriyi kullandığını, düşman ihtimali ve level başına bir sandık kuralının korunduğunu kontrol et. Havuzu tükenmiş kategoriyi tekrar açmaya çalışmak oyunu duraklatmamalı veya sandığı tüketmemeli. Elle onay otomatik test sonuçlarından ayrı kaydedilir.
