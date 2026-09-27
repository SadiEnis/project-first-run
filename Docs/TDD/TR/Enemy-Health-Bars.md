# Düşman can çubukları

Kapsam: içerik testinde kanama/yanma gibi gecikmeli hasarları görünür kılan geçici can geri bildirimi. Durum ikonları, hasar sayıları, savaş veya denge değişikliği yoktur.

- Chaser, Charger ve Ranger'ın kullandığı ortak düşman prefabına EnemyHealthBar eklenir.
- Düşman üstündeki küçük world-space canvas Camera.main'e döner; isteğe bağlı kamera atanabilir. Kamera yoksa, düşman başlatılmamış/ölmüşse veya component kapalıysa görünmez.
- Tam can dahil mevcut/maksimum can sola sabit dolulukla gösterilir. HealthChanged anında günceller; Initialize olay göndermediği için LateUpdate da değeri eşitler.
- Küçük UI düşman örneği başına bir kez oluşturulur; yeniden etkinleşme/reset sırasında kullanılır, düşmanla birlikte silinir. Raycast hedefleri ve GraphicRaycaster yoktur.
- Yükseklik, boyut ve renkler Inspector'dan ayarlanır. Aynı nesnedeki HealthComponent kaynak alınır.
- Unity testleri ve oynanış kabulünü kullanıcı yapar. Dönüş alanından çıktıktan sonra can azalması gözlenene kadar kanamanın oynanış doğrulaması bekler; kod yazılmış olması kabul sayılmaz.
