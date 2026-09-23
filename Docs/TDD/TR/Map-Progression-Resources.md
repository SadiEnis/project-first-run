# Harita içi ilerleme ve kaynaklar

## 6. aşama sözleşmesi

Bir harita tek `MapTraversalSession` kullanır. Inspector'daki `MapTraversalController` başlangıç bölgesini ve benzersiz bölge kimliklerini tanımlar; haritanın bütün yürüyüş geçitleri aynı controller'a bağlanır. Kimlikler büyük/küçük harfe duyarlıdır. Konum ancak oyuncunun tamamı hedef taraftan çıkınca değişir; giriş, geri çekilme, duraklatma, ölüm ve iptal ilerlemeyi tamamlamaz. Harita genelinde aynı anda yalnızca bir geçiş bekleyebilir. Tanımsız uçlar yapılandırma hatasıdır. Bağımsız iki bölgeli testler controller vermeyebilir; bu yedek davranış çok geçitli haritada kullanılmaz.

Dönüşlü geçitler isteğe bağlı yan yolları destekler. Tek yönlü geçit kendi ters yönünü kapatır; eski bölgeleri haritadan silmez. Serbest geçiş gerideki düşmanların öldürülmesini gerektirmez. Özel karşılaşma şartlı geçitler bu şartı korur. Geçiş; karşılaşma zaferi, run zaferi veya checkpoint/kayıt üretmez.

## Sahiplik ve yaşam süresi

- Aynı oyuncu nesnesi, build, item seviyeleri, XP durumu ve silah runtime kayıtları korunur. Yerel geçişte oyuncu kaydedilip yeniden üretilmez. Can ve mermi sıfırlanmaz; oynanış zamanlayıcıları normal şekilde ilerler.
- Bölgeden ayrılmak karşılaşmayı kapatmaz veya devre dışı bırakmaz. Yaşayan düşmanlar ve mevcut canları korunur; takip sınırları ve askıya alma sonraya bırakılır.
- Toplanmayan XP ve açılmayan sandıkların bölgeden çıkış süresi yoktur. Toplanana/tüketilene veya sahibi harita sahnesi kaldırılana kadar bağımsız dünya nesneleri olarak kalırlar. Tek yönlü geçiş ve karşılaşma temizliği bunları otomatik vermez veya silmez.
- Drop, o anda aktif olan rastgele sahneye değil kaynağının sahnesine aittir. Düşman/ön hazırlık kökünün dışında, sahne kökünde tutulur. Böylece sonraki additive sahne çalışması için sahiplik belirlenir; bu aşama sahne geçişi uygulamaz.
- Hazırlanıp ziyaret edilmeyen gruplar, sahipleri açıkça kapatılana/yok edilene veya sahne kaldırılana kadar tutulur. Mesafeye göre boşaltma yoktur.

## Doğrulama ve kapsam

EditMode: ortak rota uygunluğu, gidiş/dönüş/tek yön, harita genelinde eşzamanlı geçiş engeli, iptal, eski girişimler ve hatalı bölge yapılandırması. PlayMode: ortak konumu kullanan gerçek collider geçişleri, iptal/ölümün harita kilidini bırakması, mevcut oyuncu prefab'ı ile kaynak sürekliliği, drop'ların kaynak sahne sahipliği/yaşam süresi ve çalışan karşılaşmadan zafer/despawn olmadan serbest geçitle ayrılma. Mevcut ön hazırlık testleri yaşayan düşmanların ayrılma/geri dönüşte korunmasını kapsar.

Doğrulandı: **739/739 EditMode**, **427/427 PlayMode**, hata yok. Bu pakette 6 EditMode ve 7 PlayMode testi eklendi. Yerel raporlar: `.codex-temp/xp-attraction/map-resources-edit.xml` ve `map-resources-play.xml`. İzole Unity projesinde doğrulandı; elle sahne yerleşimi veya performans doğrulaması yapıldığı anlamına gelmez.

Oynanabilir sahne değişikliği, sahneler arası oyuncu aktarımı, teleport entegrasyonu, checkpoint, gerideki ödülleri otomatik verme, havuzlama ve sınır AI bu kapsamda değildir. Sahne geçişi 7., elle tasarlanmış oynanabilir harita 9. aşamadır. `Test_Waves` eski doğrulama örneği olarak kalır.
