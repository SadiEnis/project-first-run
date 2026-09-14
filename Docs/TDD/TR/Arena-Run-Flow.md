# Arena ve run akışı temeli

## Durum ve kapsam

Bu increment mevcut tek arena oturumunu güvenli bir run yaşam döngüsüne genişletir. Sahne geçişleri, arena modifier'ları, boss'lar, gold, meta progression ve evolution oynanışı bu adıma dahil değildir.

## Kabul edilen davranış

- Hazır arena yapılandırılmış wave dizisini tam olarak bir kez başlatır.
- Son wave tamamlandığında tek bir terminal Victory durumu oluşur.
- Oturum çalışırken oyuncu ölürse aktif wave durur, aktif düşmanlar kaldırılır ve tek bir terminal Defeat durumu oluşur.
- Geç gelen wave tamamlanması veya ikinci ölüm bildirimi terminal sonucu değiştiremez.
- Bitmiş oturum yalnızca oyuncu hayattaysa yeniden başlatılabilir. Restart yeni bir wave dizisi oluşturur ve yeni bir oturum başlangıç olayı yayınlar.
- Çalışan oturumda, initialize edilmeden veya oyuncu ölüyken restart reddedilir.

## Teknik sınırlar

ArenaSessionController oturum durumunu ve terminal sonuç olaylarını yönetir. WaveController wave sırasını yönetir ve durdurma/yeniden başlatma yaşam döngüsünü açar. Çalışan bir diziyi durdurmak, takip edilen düşmanların aboneliklerini kaldırır, aktif wave örneklerini devre dışı bırakıp yok eder ve diziyi Failed olarak işaretler. Runtime sistemleri açık dependency injection ile kalır; global singleton veya sahne araması eklenmez.

Mevcut development bootstrap test sahnesinin composition root'u olarak kalır. Birden fazla arena orkestrasyonu ve production sahne yükleme sonraki increment'lardır.

## Doğrulama

EditMode state geçiş değişmezlerini kapsar. PlayMode başlatma, son wave Victory'si, defeat temizliği, terminal sonuç koruması ve restart tazeliğini; mevcut wave, sandık, XP ve düşman regresyonlarıyla birlikte doğrular.

14.09.2026 doğrulamasında izole Unity 6000.3.9f1 projesinde 700/700 EditMode ve 381/381 PlayMode testi başarılıdır.
