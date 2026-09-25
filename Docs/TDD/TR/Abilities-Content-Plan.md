# Yetenek içerik planı

## Çalışma düzeni

Git: `content → feature/content/abilities-content`; Plastic: `/main/dev/content → abilities-content`. Sekiz yetenek aynı branch içinde yapılır. Branch, commit/check-in ve merge kullanıcıya aittir. Yetenek başına yeni branch açılmaz; doğrudan content üzerinde geliştirme yapılmaz.

Her yetenek: mekanik konuşması → EN/TR TDD ve docs checkpoint'i → anlamlı uygulama parçaları → otomatik EditMode/PlayMode doğrulaması → kayıtlı arena/ödül/F1 entegrasyonu → kullanıcı oynanış testi için durma. Tek branch, yetenek başına tek commit demek değildir: bağımsız doğrulanabilir anlamlı sınırlar seçilir; her dosya değişikliğine kayıt alınmaz. Otomatik commit veya merge yoktur.

## Sıra

| Sıra | Yetenek | Ana kapsam | Durum |
| --- | --- | --- | --- |
| 1 | Fireball | Mevcut içeriği tamamlama: menzilde rastgele hedef, 2/3/4 mermi, yanma, GDD seviyeleri | Hedefleme/salvo uygulandı; yanma bekliyor |
| 2 | Force Wave | Öne doğru kısa menzilli alan hasarı ve itme | Planlandı |
| 3 | Drone | Oyuncuyu takip, hedefleme/atış ritmi, ikinci drone, boss hız bonusu | Planlandı |
| 4 | Acid Bottle | Rastgele hedefe şişe, süreli hasar alanı, L8 yavaşlatma | Planlandı |
| 5 | Lightning Staff | Rastgele yıldırım, artan saldırı sayısı, sersemletme, L8 zincir | Planlandı |
| 6 | Shuriken | Oyuncu etrafında iki tur, kontrollü tekrar isabeti, ikinci shuriken, kanama | Planlandı |
| 7 | Enchanted Staff | Süreli, delici ve rastgele yön değiştiren ışınlar | Planlandı |
| 8 | Sniper Bomb | Elite/boss önceliği, güdümlü patlama, ikinci bomba, hedef ölünce yeniden seçim | Planlandı |

Hepsi sekiz temel seviyelidir. GDD yükseltme sırası esas alınır; sayısal denge ve belirsiz hedefleme/statü kuralları her yetenekten önce konuşulur. Uygun doğrulanmış davranışlar tekrar kullanılır; ortak kod somut ikinci tüketici gerektirdiğinde çıkarılır. Yetenek hasarı silah hasarına bağlanmaz. Haritalar arası registry yeniden bağlama ve mevcut run yaşam döngüsü korunur.

Evolution oynanışı, altın/meta, mühimmat kaynakları, nihai sunum ve Windows build kapsam dışıdır. Okunabilir geçici geri bildirim dahildir. Oynanabilir boss içeriği yoksa boss kuralları otomatik rank fixture'larıyla doğrulanabilir.

İlk checkpoint: Plasma oynanış kabulü kaydedildi; [Fireball tasarımı](Fireball-Content.md) hazırlandı. Bu checkpoint'te yeni yetenek uygulaması veya test çalıştırma yoktur.
