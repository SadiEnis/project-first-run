# Minigun içeriği

## Durum ve çalışma düzeni

Tasarım checkpoint'i — 25 Eylül 2026. Git branch'i doğrulandı: `feature/content/minigun`; kullanıcı iki platformda branch açıp geçtiğini bildirdi. Kararlaştırılan Plastic branch'i `/main/dev/content/minigun`; sunucu durumu ayrıca kontrol edilmedi. Bu doküman uygulama önerisidir; yapılmış veya test edilmiş mekanik kaydı değildir.

GDD 11.2 takip edilir. Uygulamadan önce bu iki dilli tasarım checkpoint'i alınır. Kabul edilen silah content'e merge edilir; content → main PR'ı içerik paketinin kapanışına bırakılır. Bu silah tamamlanınca başka silah/yeteneğe geçmeden kullanıcı oynanış testi için durulur.

## Önerilen atış sözleşmesi

- Kimlik `weapon.minigun`, otomatik hitscan, sekiz seviye; başlangıçta ücretsiz verilmez. Başarılı atış başına tek mermi, tek mühimmat tüketimi ve tek atış bildirimi. Shotgun itişi, delme, patlama veya ısınma sistemi yoktur.
- Başlangıç değerleri: 40 m menzil, sabit 1 derece koni yarı açısı, 240 yedek mermi. Volley çözümleyici tek saçmayla kullanılır; kendi collider'larını/trigger'ları dışlama, muzzle engeli ve menzil kontrolleri korunur.
- Ateşe basılı tutmak namluyu 0.35 oyun saniyesinde hazırlar, ardından tanımlı hızda ateş başlar. İlk prototip sürekli hızlanma eğrisi değil hazırlık gecikmesi kullanır. Hazırlık mühimmat tüketmez, tepme üretmez.
- Ateşi bırakmak, reload başlatmak, silah değiştirmek, silah kontrolünü kaybetmek veya ölmek hazırlığı sıfırlar. Boş silah hazırlanmaz; reload sonrasında tekrar hazırlık gerekir. Pause sırasında hazırlık ilerlemez ve atış olmaz; mevcut panel/reward kontrol engelleri geçerlidir. Kontrol geri geldiğinde birikmiş atış patlaması yapılmaz.
- Silah değiştirme ve seviye artışında mühimmat ile kalan reload/cooldown korunur. Kapasite artışı mühimmat vermez. Hazırlık geçicidir ve mühimmatın aksine silah değişiminde sıfırlanır. Seviyeler boyunca hazırlık süresi aynı olduğundan seviye artışı mevcut hazırlığı yeniden başlatmaz.
- Tüm silahların atış zamanlaması sessizce değiştirilmez. 30/60/120 simüle FPS'te sürekli atış ölçülür; Minigun zamanlayıcısı geçen sürenin kalanını korumalı, takılma sonrası sınırsız telafi atışı üretmemelidir. Uygulamada sınırlı telafi politikası seçilip belgelenir ve azami atış sayısı test edilir. Hazırlık sonrası sabit süre boyunca normal karelerde atış sayısı sapması en fazla bir olmalıdır.

## Kritik vuruş ve tepme

- Başarılı atış başına bir kritik ihtimali değerlendirmesi; testte belirlenebilir, Unity global rastgeleliğinden bağımsız kaynak kullanılır. Sıfır ihtimal asla, tam ihtimal daima kritiktir. Kritik, stat uygulanmış mermi hasarını 2 ile çarpar; ikinci hasar uygulaması veya mühimmat tüketimi değildir. Iska da atışı tüketir. Kritik sonucu test/debug geri bildiriminde görünür olmalıdır.
- Kritik profil silaha aittir; bu aşamada yetenek kritikleri, global kritik geliştirmeleri veya yeni stat kategorileri eklenmez. Mevcut silahların varsayılan kritik ihtimali sıfırdır; davranışları korunur.
- Mekanik tepme: başarılı mermi çözümlendikten sonra mevcut PlayerLook pitch sahibi üzerinden yukarı nişan tepmesi uygulanır. Kamera transform'unu ayrıca yazan ikinci script eklenmez. Oyuncu fare/gamepad ile karşılayabilir; mevcut pitch sınırları korunur. İlk prototipte otomatik nişan geri dönüşü, rastgele yatay tepme, kamera sallama veya silah animasyonu yoktur.
- Sabit saçılım tepme değildir, sürekli atışla büyümez. Sekizinci seviye gerçek pitch tepmesini azaltır. Tepme çözümlenen mermiyi değil sonraki atışı etkiler; engellenen atış ve hazırlık tepme üretmez. Mevcut silahların varsayılan tepmesi sıfırdır. Yeni run'da kamera sıfırlanması mevcut oyuncu yaşam döngüsünü izler.

## Önerilen sekiz seviye değerleri

Sayılar ilk oynanış testi önerisidir; GDD'nin belirlediği sayılar veya nihai denge değildir. GDD'deki gelişim sırası korunur.

| Seviye | Hasar | Kritik ihtimali | Şarjör | Atış/s | Reload s | Atış başına pitch tepmesi |
| --- | --- | --- | --- | --- | --- | --- |
| 1 | 8 | %5 | 80 | 12 | 3.0 | 0.35° |
| 2 | 10 | %5 | 80 | 12 | 3.0 | 0.35° |
| 3 | 10 | %10 | 80 | 12 | 3.0 | 0.35° |
| 4 | 10 | %10 | 100 | 12 | 3.0 | 0.35° |
| 5 | 10 | %10 | 100 | 15 | 3.0 | 0.35° |
| 6 | 12 | %10 | 100 | 15 | 3.0 | 0.35° |
| 7 | 12 | %10 | 100 | 15 | 2.4 | 0.35° |
| 8 | 12 | %10 | 100 | 15 | 2.4 | 0.20° |

Sekizinci seviyede evolution asset'i, tarifi, teklifi veya dönüşüm eklenmez. GDD'deki evrime uygunluk gelecekteki entegrasyondur; bu aşamada vaat edilen oynanış etkisi değildir.

## Entegrasyon ve kabul

- Yeni seviye/profil verilerinin tamamı edinimden önce doğrulanıp kopyalanır: sonlu sayılar, [0,1] kritik ihtimali, en az 1 kritik çarpanı, negatif olmayan hazırlık/tepme. Eski serialize asset'lerde geçerli varsayılanlar korunur. Geçersiz profil mühimmat tüketmeden veya mevcut loadout kaydını değiştirmeden reddedilir.
- WeaponRuntimeState, PlayerWeaponRuntimeEntry, stat hesabı ve mevcut edinim/seviye/ödül kuralları kullanılır; alternatif loadout kurulmaz. Hazırlık, kritik ve atış zamanlaması için yalnız gerekli küçük, test edilebilir durum/profil eklenir.
- Asset silah/karma havuzlarına ve kayıtlı ContentArena kataloğuna birer kez eklenir. Editor üreticisi güncellenir; elle düzenlenen geometri/ışığı koruyan artımlı sahne bağlantısı sağlanır. İki silah slotu korunur; temiz run'da PlasmaRifle varken başka silah almadan Minigun edinilir.
- Sınırlı izler yeniden kullanılır; test paneli/HUD hazırlık durumu, hasar, kritik ihtimali ve atış hızını gösterir. Play sırasında arena kurulmaz. Özel model/ses/animasyon, altın/meta, Windows build ve yeni evolution çalışması yoktur.
- EditMode: sekiz seviye, doğrulama/eski varsayılanlar, deterministik kritik sınırları, hazırlık geçişleri, farklı kare süreleri/takılmalarda atış sayısı, snapshot izolasyonu ve seviye artışında durum korunması.
- PlayMode: kayıtlı sahne ve gerçek basılı input; hazırlık/bırakma/reload/değişim/pause/ölüm engelleri; hasar/kritik/tepme yönü ve sınırları; fare/gamepad ile karşılama; mühimmat/bildirimler, gerçek sandık edinimi/dolu slot/maksimum seviye; Shotgun ve PlasmaRifle regresyonları.
- Kullanıcı kabulü: sürekli ve kısa basışlar, nişanla tepme karşılama, reload/mühimmat baskısı, sekiz seviyenin farkları ve sandık/panel edinimi. Otomatik testlerden sonra bu değerlendirme için durulur.

Bu tasarım checkpoint'inde uygulama veya test çalıştırma yapılmadı. Önceki Shotgun sonuçları (774 EditMode / 475 PlayMode) Minigun'u doğrulamaz.
