# Run sonucu ve yeniden başlatma — 8. aşama

## Sözleşme

- Run `Ready`, `Running`, `Transition`, `Victory` veya `Defeat` durumundadır. Savaş, keşif ya da sahne geçişi denemesi sırasında ölüm bir kez `Defeat` yayınlar ve run'ı terminal yapar. Sonraki arena zaferi, geçit tamamlanması veya gecikmiş ölüm olayı sonucu değiştiremez.
- Yapılandırılmış son arena oturumu tamamlanınca nihai run zaferi bir kez yayınlanır. Sıradan bölge çıkışı veya sahne varışı run zaferi üretmez; ilerideki final hedef adaptörü sonucu açıkça tamamlayabilir.
- Yeniden başlatma yalnızca `Victory`/`Defeat` sonrasında açık bir işlemdir: çağıran taraf tek atomik dünya/oyuncu sıfırlama callback'i verir. Callback karşılaşmaları ve geçici dünya nesnelerini temizlemeli/sıfırlamalı, oyuncu canını/kontrolünü düzeltmeli ve bütün arena oturumlarını `Ready` hazırlamalıdır. Hata verir veya oturum/oyuncu geçersiz kalırsa eski terminal durum korunur ve yeni arena başlamaz.
- Sıfırlama başarılı olunca controller yeni run durumu oluşturur, yalnızca ilk oturumu başlatır ve `SessionRestarted`/`SessionStarted` yayınlar. Checkpoint, save, meta para veya eski haritayı geri yükleme varsayılmaz. Sahnenin yeniden yüklenmesi, mevcut haritanın yeniden kurulması veya elle reset çağırılması seçimini çağıran taraf yapar.
- Reset callback'i aynı oyuncu nesnesini kullanabilir; build/XP/item durumunu temizlemek callback'in sorumluluğudur. Kalıcı meta ilerleme bu aşamada kararlaştırılmaz.

## Doğrulama

Testler arena ve transition sırasında ölümü, tekil defeat olayını, terminal olay bağışıklığını, callback ile başarılı restart'ı, callback hatası geri dönüşünü, geçersiz oyuncu/oturum durumunu ve ilk arena'nın temiz başlangıcını kapsar. Sahne geçişi iptali Scene-Travel altında kalır; restart coordinator terminal sonucu tüketir ancak sahne yüklemez.

Doğrulandı: **747/747 EditMode**, **445/445 PlayMode**, hata yok. Yerel raporlar: `.codex-temp/xp-attraction/run-outcome-final-EditMode.xml` ve `run-outcome-final-PlayMode.xml`.
