# Geçit bariyeri yaşam döngüsü

## Düzeltme sözleşmesi — 15 Eylül 2026

TransitionBarrier collider kimliklerini bir kümede tutar. Aynı kimlik tekrarlanmaz; farklı collider'lar ayrı sayılır. Kapalıyken de bulunma bilgisi kaydedilir. Kapanma son kimlik temizlenene kadar ertelenir. Open bekleyen kapanmayı iptal eder; bildirimler durum değişiminde bir kez yayınlanır.

RegionPassageController atanmış oyuncunun etkin, trigger olmayan collider'larını tarar. Güvenlik hacmiyle AABB kesişimi korumacı sınırdır: düzensiz hacimlerde kapanma geç olabilir. Hacim fiziksel engelin tamamını çevrelemeli; hacim ve engel ayrı collider'lar olmalıdır. Bu kapsama koşulu bozulursa engel devre dışı kalır ve geçiş kabul edilmez.

Yeni temaslar önce eklenir, eski temaslar sonra çıkarılır; aynı karede collider değişimi geçici boşluk üretmez. Silinmiş/devre dışı collider'lar sonraki taramada temizlenir. Engel yalnızca bariyer kapalı ve güvenlik hacmi boşken etkinleştirilir. Bileşen devre dışı kalınca pending işlem iptal edilir ve engel açık tutulur; yeniden etkinleşince hacim taranır.

Trigger transformunun yerel Z ekseni yönü belirler. Oyuncu geldiği tarafa çıkarsa işlem iptal edilir; hedef tarafa tamamen çıkarsa tamamlanır. OneWay sonrasında oyuncunun hedef bölge kimliği ters yönü reddettiği için arkasındaki bariyer kapanır. Opened/Closed bildirimleri sunuma bağlanabilir; hareketli animasyon geometrisi bu adımın kapsamında değildir.

Güvence sabit fiziksel engel ve doğru boyutlandırılmış güvenlik hacmi içindir. Çoklu collider, geri çekilme, ölüm/pause, devre dışı bırakma ve gerçek fizik adımlarıyla yürüyüş PlayMode testleriyle doğrulanır.

## Kurulum ve sınırlar

- RegionPassageController'ı güvenlik trigger'ının GameObject'ine ekle. Trigger için kinematic Rigidbody kullan; fiziksel engeli ayrı bir GameObject/collider olarak ata.
- Player ve Health referanslarını, kaynak/hedef kimliklerini, Direction ve Requirement alanlarını ata. Başlangıç bölgesi SourceId kabul edilir; +Z yönü hedefe bakmalıdır.
- Şartlı geçişte sahne kurucusu ilgili RegionEncounterSession'ı Start'tan önce BindEncounter ile bağlar veya Configure'a verir.
- Opened/Closed UnityEvent'leri görsele bağlanabilir. Fiziksel engeli bileşen yönetir; başka script aynı collider'ı açıp kapatmamalıdır.
- Tarama sabit fizik adımları arasındadır; tek adımda hacmi tamamen atlayan hareket için süpürme algısı eklenmedi. Örnek sahnede hacim oyuncu hızına uygun boyutlandırılmalıdır.
- Tek kullanımlılık ve yön bileşene özgü bağlantı durumudur; çoklu geçitlerin ortak harita konumu/sahne yüklemesi sonraki akış entegrasyonunda yönetilecektir.

## Doğrulama — 15 Eylül 2026

EditMode: 733/733 geçti. PlayMode: 408/408 geçti; bunun 10'u RegionPassageTests, 3'ü önceki RegionEncounterIntegrationTests kapsamıdır. Yan taraftan çıkışın başarı sayılmaması da doğrulandı. Test sonuçları yerel .codex-temp/xp-attraction/passage-review-edit.xml ve passage-review-play.xml dosyalarındadır. Test_Waves sahnesinin oynanışı bu paketle değiştirilmedi.
