# Demo HUD — Yerleşim ve Davranış Sözleşmesi

## Durum ve kapsam

Tasarım onayına dayalı uygulama planı; HUD henüz uygulanmadı. Kullanıcının çizimi esas alınır: üstte tam genişlikte XP, sol üst yetenekler, sağ üst güçlendirmeler ve altında anahtar; sol altta silah, mermi ve can. İlk entegrasyon WideCombat üzerinde yapılır. Harita geometrisi, düşman yoğunluğu ve oyun dengesi değişmez.

Bu aşama yalnızca dokümantasyondur. Meta progression, HUD ayarlar menüsü, final ikon üretimi ve diğer sahnelere otomatik uygulama kapsam dışıdır. DOTween ücretsiz sürümü animasyon aşamasında kullanılabilir; Pro zorunlu değildir. Bu docs adımında paket kurulmaz.

## Yerleşim

| Bölge | İçerik |
| --- | --- |
| Üst kenar | Güvenli kenar boşluğuyla tam genişlik XP çubuğu; altında ortada seviye |
| Sol üst | Aktif kapasite kadar yetenek yuvası |
| Sağ üst | Aktif kapasite kadar güçlendirme yuvası; altında sahip olunduğunda anahtar |
| Sol alt | Aktif silah kartı; yanında şarjör/yedek mermi ve can çubuğu |
| Merkez | Sade nişangâh; gerektiğinde kısa etkileşim bildirimi |

Kayıtlı, Inspector'dan düzenlenebilir Canvas prefab'ı kullanılır. Yerleşim runtime'da sıfırdan sahne kurmaz; yalnızca kapasiteye bağlı slot görünümleri çoğaltılabilir. Anchor, Canvas ölçekleme ve kenar boşlukları farklı çözünürlüklerde korunur; XP çizgisi ile slot/seviye alanı çakışmaz. Can barında mevcut/maksimum can okunur. İkon eksikse ayırt edilebilir kısa ad/placeholder gösterilir; boş kutu ile sahip olunan ikon eksiği birbirinden ayrılır.

## Kapasite ve item'lar

Kaynak PlayerBuildCapacity ve oyuncunun mevcut run envanteridir; HUD kapasite açmaz veya item kazandırmaz. Mevcut demo başlangıcı 2 silah, 3 yetenek, 5 güçlendirme kullanır; modelin üst sınırları 2/5/8'dir. Bunlar UI'da sabit slot sayısına çevrilmez. Gelecekte meta tarafından belirlenen run kapasitesi aynı sözleşmeden okunur.

Aktif ama boş slot görünür; henüz açılmamış kapasite için slot gösterilmez. Item alındığında boş slot dolar; level-up aynı slotun seviye göstergesini değiştirir, yeni slot oluşturmaz. Item sırası kararlı kalır. Yeni run'da önceki item, anahtar ve animasyon durumu temizlenir.

## Silah durumları ve reload karartısı

- Ateşe hazır kart normal görünür. Yalnızca yedek merminin sıfır olması, dolu şarjörü karartmaz.
- Şarjör boş ve yedek de yoksa ikon yarı saydam siyah katmanla karartılır; 0/0 bilgisi okunur.
- Reload başlayınca (otomatik veya elle, şarjör kısmen doluyken de) karartı tam görünür. Üst noktadan başlayarak saat yönünde silinir; gerçek reload tamamlandığında sıfırlanır.
- Kısmen açılmış ikon reload sırasında kullanılabilir anlamına gelmez. İptal, silah değişimi veya ölümde gerçek runtime durumu esas alınır.
- Karartı kalan süre / ilgili işlemin gerçek toplam süresidir; 0..1'e sınırlandırılır. Statlarla değişen sürelerde asset'in temel süresine körlemesine bölünmez. Sıfır süre güvenli işlenir.
- Normal atış aralığı ve Minigun hazırlığı, reload gibi gösterilmez; ek davranış ayrıca kararlaştırılır.

WeaponRuntimeState.ReloadTimeRemaining ve IsReloading mevcuttur. Uygulamada geçerli işlem süresini salt okunur olarak UI'a sunma gereksinimi kontrol edilir. HUD kendi sayacını çalıştırarak mermi doldurmaz veya silahı hazır ilan etmez.

## Yetenek karartısı

Gerçek kullanımdan sonra ikon kararır; kalan süre azaldıkça üstten saat yönünde açılır. Hedef bulunamadığı veya kullanım başarısız olduğu için cooldown başlamamışsa karartı başlamaz. Normal ikon yalnızca cooldown açısından hazır olmayı ifade eder; otomatik yetenek hedef bekleyebilir.

AbilityRuntimeState.CooldownRemaining mevcuttur; kullanım süresi cooldown çarpanıyla değişir. Normalleştirme gerçek aktif cooldown süresini kullanmalıdır. AbilityRuntimeEntry.IsContinuous tek başına görsel süre seçmeye yetmez: sürekli çalışan executor içinde dönemsel saldırı olabilir. Örneğin DroneRuntime her drone için ayrı süre tutar. İlgili aşamada tüm yeteneklerin zamanlayıcıları incelenip salt okunur sunum durumu sağlanır. Gerçek tek bir cooldown olmayan pasif/sürekli etkide sahte radial sayaç gösterilmez; çoklu drone zamanlaması gibi durumlar ayrıca değerlendirilir.

## Saat yönünde silah kartı geçişi

İki kart ortak yay üzerinde saat yönünde hareket eder. Bekleyen kart sol dışarıdan üst yayla aktif konuma gelir; önceki aktif kart alt yayla sol dışarıya ayrılır. Görünmeyen bölüm maskelenir. Yazılar okunur kalır; hafif eğim mümkün, ters dönmüş kart hedef değildir.

Başlangıç deneme süresi 0,25–0,35 saniye, Inspector'dan ayarlanabilir. Can sabittir. Mermi bilgisi gerçek aktif silaha aittir. Silah değişimi runtime'da gerçekleşir; animasyon ek kullanım gecikmesi yaratmaz. Hızlı Q girişlerinde animasyonlar kuyrukta birikmez, son gerçek seçime doğru yeniden hedeflenir. Tek silah varsa hayali ikinci kart yoktur. Sahip olunan ama mermisiz silahın kartı da durumunu korur.

## Veri ve yaşam döngüsü

Sunum, oyun durumunu yalnızca okur. Sağlık, XP, envanter ve seçim değişiklikleri mümkünse olaylarla; aktif süreler gerektiğinde hafif güncelleme ile okunur. Henüz initialize olmamış kaynaklar güvenle beklenir. Sahne referansları açıkça atanır/doğrulanır; her kare tüm sahneyi tarama yapılmaz.

XP çubuğu CurrentExperience / RequiredExperience kullanır; TotalExperience kullanılmaz. Birden fazla level atlama son gerçek level ve kalan XP ile sonuçlanır. Sağlık maksimumu değişince bar da güncellenir.

Sandık/reward ekranında cooldown ve reload oyunun duraklatılmış durumunu yansıtır; görsel sayaç bağımsız ilerlemez. Dekoratif hareket oyun sürelerinden ayrıdır, gizlenen HUD animasyonları durdurulur. Ölümde kontroller kullanılabilir görünmez; mevcut restart/quit ekranı korunur. Açılış kontrol devri tamamlanınca HUD görünür; final sunumu başlayınca gizlenir. Restart abonelikleri/animasyonları temizler ve yeni oyuncuya bağlanır.

Eski OnGUI debug kutuları yeni HUD ile çift çizilmez. Devre dışı bırakma yalnızca yerine geçirilen sağlık/mermi/XP/nişangâh parçalarını hedefler; henüz taşınmamış hedef metni, hata bildirimi ve ölüm/restart/quit davranışı korunur.

## Uygulama dilimleri ve kabul

1. Docs check-in: bu sözleşme ve indeksler.
2. Temel prefab: statik aktif silah, can, mermi, XP/seviye, kapasiteye bağlı boş kutular. Animasyon yok. Kullanıcı yerleşimi değerlendirir.
3. Item bağlantıları: ikon/placeholder, seviye, kararlı slot sırası ve anahtar.
4. Radial göstergeler: reload/mermisizlik, ardından yetenek türlerine uygun cooldown sunumu.
5. Silah kartı hareketi: iki yönlü tekrar değişim, hızlı Q, tek silah, ölüm/finalde temizlik.
6. Entegrasyon: farklı ekran oranları, reward pause, restart, final ve gerektiğinde Windows build kontrolü.

Her anlamlı dilim ayrı değerlendirilir; her küçük işlem ayrı commit gerektirmez. EditMode/PlayMode testleri yazılır; kullanıcının tercihiyle testleri kullanıcı çalıştırır, bildirilen hatalar hedefli ele alınır.

Planlanan testler: kapasite 0/varsayılan/maksimum; acquire/level-up slotları; XP çoklu level; değişen max can; dolu şarjör + sıfır yedek; manuel/otomatik reload; reload kesintisi; cooldown indirimi; başarısız kullanım; hedef bekleme; pause; hızlı silah değişimi; ikon eksikliği; ölüm/final/restart; çift HUD ve abonelik sızıntısı olmaması. Görsel kabul: karartının gerçekten saat yönünde silinmesi ve yay yönünün çizimle eşleşmesi oynanışta kontrol edilir.

## Teslim

Yalnızca dokümantasyon. Sahne, oyun kodu ve paketler değiştirilmedi; Unity testleri çalıştırılmadı.
