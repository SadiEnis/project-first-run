# Bölge ve karşılaşma yaşam döngüsü

## Uygulama sözleşmesi — 15 Eylül 2026

Bu adım [ilerleme planının](Map-Region-Encounter-Plan.md) ikinci parçasıdır. Önce test edilebilir yaşam döngüsü eklenir; fiziksel hazırlama/giriş trigger'ları, takip sınırları, yeni geçiş uygunluğu ve örnek sahne sonraki adımlardır. Mevcut `Test_Waves` akışı değiştirilmez.

- `RegionEncounterSession`, sabit bölge kimliği ve bir yerel karşılaşmayı yönetir. İlk artım bölge başına tek karşılaşma oturumu kullanır.
- `Prepare`, önceden initialize edilmiş ve Ready durumundaki `IArenaSession` bağımlılığını bağlar. Bu aşamada hazırlık bağımlılık hazırlığıdır; düşmanları önceden instantiate etme/havuzlama değildir.
- Durumlar: Unprepared → Prepared → Active → Completed veya Failed. Oyuncunun içeride bulunması ayrı `IsPlayerInside` bilgisidir.
- İlk `Enter` karşılaşmayı bir kez başlatır. Yinelenen giriş yeni spawn üretmez. `Leave` yalnızca bulunma bilgisini değiştirir; düşmanları durdurmaz, yok etmez veya iyileştirmez.
- Aktif bölgeye yeniden giriş aynı karşılaşmayı sürdürür. Tamamlanmış bölgeye giriş karşılaşmayı başlatmaz. Failed oturum yeniden başlamaz; yeni run yeni oturumlar kurmalıdır.
- Bölge dışında kalan aktif karşılaşma tamamlanabilir veya oyuncu ölümüyle başarısız olabilir. Terminal bildirim bir kez yayınlanır.
- Completion yerel bir olaydır. Bu sınıf `RunSessionController` çağırmaz, harita ilerletmez veya kapı açmaz. Sonraki geçiş katmanı ilgili olaya açıkça bağlanacaktır.
- Yaşam döngüsünün tek sahibi bölge oturumudur; aynı karşılaşma ayrıca eski run listesine bağlanmamalı veya dışarıdan Begin/Restart edilmemelidir.
- `Dispose` abonelikleri kaldırır, dünya nesnelerini silmez. Sahne sahibi oturumu dispose eder; düşman/sahne temizliği ayrı sorumluluktur.

## Kabul testleri

EditMode: geçersiz hazırlık, hazırlıkta başlamama, giriş öncesi hazırlık şartı, yinelenen giriş, çıkış/yeniden giriş, terminal bildirim tekrarı, başlatma hatası, senkron bitiş ve abonelik temizliği.

PlayMode: gerçek `ArenaSessionController` / `WaveController` ile spawn sayısının ve yaralı düşman kimliği/canının yeniden girişte korunması; dışarıdayken tamamlanma; tamamlanan bölgeye dönüşte respawn olmaması; dışarıdayken oyuncu ölümü.

Takip alanı oyuncunun bölge bilgisine bağlanmaz; mevcut AI değişmeden devam eder. Uzaktaki bölgeleri askıya alma, spawn ön hazırlığı ve kaynak boşaltma bu artımın performans iddiaları değildir.
