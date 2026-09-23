# Harita, bölge ve karşılaşma ilerleme planı

## Durum ve kapsam

24 Eylül 2026 kapanış kararı: Kullanıcı bölge girişlerini, düşman oluşumu/ölümünü, XP/seviye/sandık zincirini, yan alanları, kapanan geçidi ve hedef sahneye geçişi oynayarak doğruladı. `multi-arena-run`, harita/bölge/geçiş temeli kapsamında merge edilebilir. 20 Eylül otomatik kanıtı 752/752 EditMode ve 449/449 PlayMode sonucudur; bu kapanış güncellemesinde testler yeniden çalıştırılmadı. Eski wave örneği ayrı test akışı olarak korunur.

10. aşamanın bütün ilk kabul hedefleri tamamlanmış değildir. Kullanıcı kararıyla Windows build açılış hatası ve standalone kabulü şimdilik ertelendi; dağıtılabilir demo öncesinde çözülmelidir. Gerçek haritalarda ölüm/nihai sonuç/temiz yeni-run bağlantısı demo entegrasyonuna bırakıldı; daha erken ihtiyaç doğarsa ele alınabilir. Bunlar içerik genişletmenin veya bu daraltılmış branch kapsamının merge önkoşulu değildir. Görüntülü performans/soğuk başlangıç ve rota dengesi değerlendirmeleri de açık kalır; hata çözülmüş ya da tam üs döngüsü uygulanmış sayılmaz.

Bu plan, eski belgelerdeki her arenanın ayrı sahne olması ve her ilerlemenin tüm wave'leri temizlemeyi gerektirmesi varsayımlarının yerini alır. Altın ve meta gelişim hedef oyun döngüsünün parçasıdır; bu harita aşamasında uygulamaları ertelenmiştir. Yeni evolution içeriği ve prosedürel üretim de kapsam dışıdır.

## Kavramlar ve kabul edilen mekanikler

| Kavram | Sorumluluk |
| --- | --- |
| Harita | İlk prototipte bir Unity sahnesindeki gezilebilir alan; birden fazla bölge ve çıkış içerebilir. |
| Bölge | Haritanın ana alanı, yan odası veya mağarası; bağımsız sahne olmak zorunda değildir. |
| Karşılaşma | Bölgedeki düşman grubu veya wave dizisi; tamamlanması haritayı otomatik bitirmez. |
| Geçiş | Kaynak ve hedef arasında ilerleme isteği; uygunluk, hareket yöntemi ve görsel kapı birbirinden ayrılır. |
| Run | Haritalar boyunca oyuncunun ilerlemesini ve nihai sonucunu kapsar. |

- Oyuncu isteğe bağlı savaşları atlayıp çıkışa ilerleyebilir. Bunun bedeli kaçırılan XP ve gelişim fırsatlarıdır; denge daha sonra ölçülecek.
- Bu serbestlik yalnızca yan dungeon'larla sınırlı değildir: serbest çıkışlı ana alanda da düşmanları geride bırakıp kapanan geçidin ardına geçilebilir. Tek yönlülük, karşılaşma tamamlama şartından bağımsızdır.
- Hedef dış döngü: üs/meta alanında uyanma → run'a giriş → ölümde aynı üs/geliştirme alanına dönüş → korunan tur altınıyla kalıcı gelişim → yeni run. Altın koruma oranı ve üs sunumu bu kararla yeniden belirlenmez; mevcut ekonomi önerileri ayrı tasarlanacaktır.
- Karşılaşma temizleme şartı yalnızca açıkça tanımlanmış özel geçişlerde kullanılır. Haritadaki tüm düşmanlar genel bir çıkış kilidi değildir.
- Yan alanlara dönüş ve tek yönlü bağlantılar desteklenecek. Tek yönlü geçiş otomatik checkpoint veya kayıt anlamına gelmez.
- Aynı sahnede merdiven/koridor boyunca yürümek de geçiştir; teleport zorunlu değildir. Sahne yüklemesi ayrı bir geçiş yöntemidir.
- `GateTrigger` / `PitTrigger` gibi dekor türüne bağlı sınıflar yerine genel geçiş davranışı kullanılır.
- Hazırlama ve savaşı etkinleştirme ayrılacak. Oyuncu yeni bölgeyi görmeden düşmanlar hazırlanabilecek; takılmama hedefi ölçümle doğrulanacak, yalnızca async çağrı kullanmak yeterli sayılmayacak.
- Bölge değişimi oyuncu sağlığını, ekipmanını, mermisini, XP'sini veya cooldown'larını sıfırlamaz. Sahne sınırında bu verileri taşıma mekanizması ayrıca tasarlanacak.
- Sıradan karşılaşma veya harita çıkışı run zaferi değildir. Nihai hedef ayrı bir koşuldur.
- İlk oynanabilir örnek el yapımı tek bir harita düzeni kullanacak; varyantlar zorunlu değil.

## Mevcut sistemden geçiş envanteri

| Parça | Korunacak / değiştirilecek yön |
| --- | --- |
| `WaveController`, `WaveEnemyTracker` | Yerel karşılaşmaların spawn ve bitiş takibi korunur; harita ilerlemesinin tek otoritesi olmaz. |
| `ArenaSessionController` | Wave tabanlı yaşam döngüsü karşılaşma kapsamında değerlendirilecek; otomatik harita tamamlama bağlantısı ayrılacak. İsim değişikliği henüz kararlaştırılmadı. |
| `RunSessionController` | Tüm arena oturumlarını baştan hazırlayan sıralı liste ve victory üzerinden ilerleme varsayımı gözden geçirilecek; harita çıkışı ve karşılaşma bitişi ayrılacak. |
| `ArenaTransitionTrigger` | Genel collider ve açık oyuncu referansı korunur; kaynak arena indeksi bağımlılığı yeni hedef modeline uyarlanır. |
| `ArenaTransitionController` | Uygunluk kontrolü, geçiş yöntemi ve hedef çözümleme ayrılacak; serbest çıkış için zorunlu arena victory şartı kaldırılacak. |
| `PlayerMotor.Teleport` | Gereken yerel taşımalarda kalır; yürüyüş bağlantıları bunu kullanmak zorunda değildir. |
| Development bootstrap / `Test_Waves` | Mevcut doğrulama örneği korunur; dolaşılabilir örnek sahnenin yerine geçtiği varsayılmaz. |

## Aşamalı geliştirme sırası

Her adımın mekanikleri uygulamadan önce konuşulup TDD'ye işlenir. 1–8'in kapsamı aşağıdadır; 8'in sıralı arena controller'ındaki terminal/restart temeli, yeni harita örneğinde üs dönüşü olarak bağlanmış değildir. 9'un teknik geçiş örneği mevcut; 10 entegrasyon eksiklerini ve doğrulamayı kapsar. Tekrar giriş ve sürekli aktif karşılaşma kararları aşağıda güncellenmiştir.

1. **Tasarım ayrımı — bu belge:** GDD yönünü, terimleri, korunacak parçaları ve sırayı kaydet. Runtime değişikliği yok.
2. **Bölge ve karşılaşma yaşam döngüsü:** Hazırlama, etkinleştirme, tamamlama ve tekrar giriş kurallarını tasarla. Karşılaşma bitişinin harita bitişi olmadığını test et.
3. **Genel geçiş sözleşmesi:** Serbest / karşılaşma şartlı uygunluk, hedef, tek yön / dönüş ve yürüyüş / taşıma / sahne yükleme ayrımını kur. Yanlış oyuncu ve yinelenen istekleri test et.
4. **Geçiş bariyeri:** Görselden bağımsız açılma/kapanma sinyalleri; oyuncu tamamen geçmeden kapatmama, arada sıkışmama ve tekrar tetiklemeyi test et.
5. **Ön hazırlık — tek grup entegrasyonu uygulandı:** Ayrı hazırlama/aktivasyon hacimleri, adet ve süre bütçesi, hazır olmayan hedef için bekleyen giriş ve geçit kilidi eklendi. Ölçümler ve doğrulama Region-Preparation belgesindedir. Wave dizilerini ön hazırlama ve havuzlama henüz eklenmedi.
6. **Kaynak sürekliliği — uygulandı:** Ortak bölge konumu ve harita genelinde geçiş kilidi; oyuncu durumu yerinde korunur. Drop kaynak haritanın sahnesine aittir ve bölge/düşman temizliğinde kalır. Geri dönüş, tek yön ve savaş atlama kapsamı Map-Progression-Resources belgesindedir.
7. **Sahneler arası geçiş — temel uygulandı:** Inspector üzerinden sahne yolu/giriş kimliği, additive yükleme, mevcut oyuncunun aktarımı, hedef bağımlılıklarını bağlama, kaynak temizliği ve hata/tekrar davranışı. Sözleşme ve test kapsamı Scene-Travel belgesindedir; paketlenmiş build doğrulaması oynanabilir harita entegrasyonunda yapılacaktır.
8. **Run sonucu ve yeniden başlatma — temel uygulandı:** Gezinme/geçiş sırasında ölüm, nihai zafer ve atomik reset callback'i ile temiz restart. Checkpoint ve kayıt kararı bu adıma otomatik dahil değildir; sözleşme Run-Outcome-Restart belgesindedir.
9. **Oynanabilir örnek sahne — sahnede kayıtlı entegrasyon mevcut:** Ana alan, iki geri dönülebilen yan alan, hazırlama/aktivasyon hacimleri, tek yönlü ikinci ana alan ve küçük ikinci sahneye çıkış. Mevcut düşman/XP/level-up sandık servisleri, baked navigasyon, çevre sınırları ve görüş kesen duvarlar bağlandı. Bu hâlâ doğrulama örneğidir; nihai demo veya tamamlanmış run döngüsü değildir.
10. **Oynanış ve performans doğrulaması — devam ediyor:** Tam paketler geçti (752 EditMode / 449 PlayMode); kayıtlı sahnede geçiş/tekrar giriş/ödül entegrasyonu ve 32 düşmanlık hazırlık testi dahil. Ölçümler ve sınırları Playable-Map-Fixture belgesindedir. Görüntülü kısa/keşif rotası değerlendirmesi ve soğuk başlangıç profili kalır; havuzlama kararı yalnızca sıcak batch ölçümlerine dayanmaz. Paketlenmiş build kanıtı aynı belgede ayrıca kaydedilir. Harita temelli ölüm/nihai sonuç bağlantısı, eski arena listesinden bağımsız ele alınmalı; callback restart temeli tamamlanmış üs döngüsü sayılmamalı.

İlk tasarım ve uyarlamalar mevcut `feature/multi-arena-run` üzerinde ilerler. Her madde ayrı branch veya commit gerektirmez. Yeni bağımsız aşama gerektiğinde Plastic dev / Git main tabanından açılır; merge'i kullanıcı yapar.

## Kararlar ve kalan tasarım konuları

### Harita çalışması sonrası sıra — 20 Eylül 2026

24 Eylül kullanıcı kararı bu sırayı günceller:

1. Mevcut branch'i harita/geçiş temeli kapsamında kapat; Plastic dev / Git main'e merge'i kullanıcı yapar.
2. İçerik test sahnesi: önerilen ayrı `Test_ContentArena`, oynanabilir fixture'ın mevcut prefab/servis bağlantılarını kullanır. Editor'de kayıtlı geometri ve referanslar; düşmanlar, XP, item seviyeleri ve gerçek sandık/ödül akışı bulunur. Mevcut harita fixture'ı ve Test_Waves kendi test kapsamları için korunur. İstenen eşyayı edinme/seviyesini deneme gibi geliştirme kontrolleri, gerçek edinim/seviye API'lerini kullanmalı; sandık akışının yerine geçmemelidir. Sahne henüz oluşturulmadı.
3. Silahlar ve yetenekler tek tek tamamlanır; her biri için mekanik/TDD → uygulama ve otomatik test → kullanıcı oynanış testi → sonraki içerik sırası izlenir. Kullanıcı denemesi gelmeden sıradaki silah/yeteneğe geçilmez. GDD'deki dört silah ve sekiz yetenek hedeflenir; mevcut Plasma Rifle ve Fireball'un geçici profilleri tam GDD davranışı kabul edilmez. Yeni evolution içeriği eklenmez.
4. Güçlendirmeler ortak bir içerik paketi halinde ele alınabilir. GDD'de hedef on olmasına rağmen dokuz stat ve yedi özellik adayı vardır; kesin liste, sayılar ve etkileşimler uygulamadan önce netleştirilir. Kullanıcının kapsamı eklenebilir adayları değerlendirmektir; desteklenmeyen etki yalnızca asset eklenerek tamamlanmış sayılmaz. Lucky'nin evolution/ekonomi kısmı ertelenmiş kapsamı kendiliğinden açmaz.
5. Oynanabilir demo haritasında içerikleri birleştir; süre–risk–ödül dengesini, gerçek run yeniden başlatma bağlantısını ve dağıtım öncesi build/performance kabulünü tamamla. Mevcut teknik örnek final demo haritası değildir.

İlk yeni iş için önerilen branch: Plastic `/main/dev/content-test-arena`, Git `feature/content-test-arena`; yalnızca mevcut merge'den sonra güncel tabandan açılır. Sonraki silah/yetenek işleri de kendi kararlaştırılmış kapsamlarıyla güncel dev/main tabanından açılır. Bu belge branch oluşturmaz ve tamamlanmamış mekaniklere onay yerine geçmez.

Boss, içerik genişletmenin önüne zorunlu bir aşama olarak konulmaz; kapsamı demo ihtiyaçlarıyla ayrıca kararlaştırılır. Altın/meta uygulaması ve evolution oynanışı ertelenmiş kalır. Demo kapsamının üs/ekonomi içermesi ayrıca belirlenmelidir; teknik doğrulama bu sistemlerin hazır olduğunu iddia etmez.

### Yaşam döngüsü kararları

- Karar: tekrar giriş düşmanları yeniden üretmez; yaşayanların canı ve mevcut karşılaşma durumu korunur.
- Düşmanlar bölge dışına oyuncuyu takip edebilecek mi; sahiplik ve takip sınırı nasıl ayrılacak?
- Karar: oyuncu ayrılınca aktif karşılaşma çalışmaya devam eder; askıya alma optimizasyonu ertelendi.
- Prototip kararı: hazırlanıp ziyaret edilmeyen gruplar, sahipleri açıkça kapatılana/yok edilene veya harita sahnesi kaldırılana kadar tutulur; mesafeye göre boşaltma yoktur.

Takip sınırları açık konudur. Kararlaştırılmış yaşam döngüsü ve kaynak kuralları Region-Encounter-Lifecycle ve Map-Progression-Resources belgelerinde yer alır.

## Doğrulama ve kayıt düzeni

Önce anlamlı tasarım paketi için docs check-in/commit; sonra ilgili runtime ve testler için anlamlı uygulama paketleri. Her küçük dosya değişimi ayrı kayıt gerektirmez. İlk kayıt yalnızca tasarımdı; sonraki uygulamalar ve tarihli doğrulama kanıtları ilgili aşama belgelerinde tutulur. Eski test sonucu daha sonraki değişikliğin doğrulandığı anlamına gelmez.
