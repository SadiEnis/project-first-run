# Lightning Staff — içerik tasarımı

## Durum

abilities-content üzerindeki beşinci yetenek; GDD 12.4 temel alınır. Rastgele hedefli alan yıldırımları, L6 sersemletme ve L8 tek sıçramalı zincir kullanıcı tarafından onaylandı. Bu checkpoint yalnızca dokümantasyondur; uygulama ve doğrulama henüz yapılmadı.

## Mekanik

- 15 m içindeki görülebilen canlı mevcut-harita düşmanları rastgele ana hedef seçilir. Hedef yoksa cooldown tüketilmez. Farklı hedefler tercih edilir; düşman sayısı yıldırım sayısından azsa kalan yıldırımlar hedefleri tekrar seçebilir.
- Hedef konumuna anlık isabet, yukarıdan gelen geçici yıldırım ve isabet görseli. Yol alan mermi veya kalıcı hasar alanı yoktur.
- Her yıldırım 2 m içindeki uygun düşmana collider sayısından bağımsız bir kez hasar verir. Aynı kat ve isabet noktasından engelsiz görüş gerekir. Bağımsız örtüşen yıldırımlar aynı düşmana hasar verebilir.
- AbilityDamage ölçeklemesi uygulanır. L6, 0,5 s sersemletme ekler: isteğe bağlı hareket ve saldırı ilerlemesi, Charger atılması ve Ranger ateşi dahil, durur. Saldırı durumu/sayaçları korunur; bitince ek saldırı üretmeden veya cooldown sıfırlamadan devam edilir. Gelen hasar ve diğer durum etkilerinin süreleri devam eder.
- Yeniden uygulama kalan sersemletme süresini en az 0,5 s yapar; süreler toplanmaz. Kontrol geri verilirken asit yavaşlatması veya diğer sahiplerin katkıları ezilmez. Ölüm/yeniden kullanım sersemletmeyi temizler.
- L8: Her yıldırım isabet noktasının 4 m yakınındaki en yakın canlı düşmana bir kez zincirlenir; o yıldırımın zaten hasar verdiği düşmanlar dışlanır. Aynı kat ve engelsiz görüş gerekir. Aynı hasar/sersemletme yalnızca tek hedefe uygulanır; yeni zincir veya ikinci alan patlaması oluşmaz. Aday yoksa zincir yoktur. Bağımsız yıldırımlar aynı zincir hedefini kullanabilir.
- Prototipte sersemletme tüm rank'lara uygulanır. Boss direnci/bağışıklığı sonraki açık denge kararına bırakılır.
- Pause cooldown, sersemletme ve görsel süresini dondurur. Oyuncu ölümü yeni atışları durdurur; mevcut sersemletme normal süresinde biter. Harita değişiminde registry yeniden bağlanır, haritaya ait görsel/durum nesneleri temizlenir.

## Başlangıç dengesi

Hedef menzili her seviyede 15 m, isabet yarıçapı 2 m. Hasarlar ve 4 m zincir menzili geçici uygulama varsayımlarıdır; denge yeniden değerlendirilebilir.

| Seviye | Yıldırım/zincir hasarı | Cooldown s | Yıldırım | Sersemletme s | Yıldırım başına ek zincir hedefi |
| --- | --- | --- | --- | --- | --- |
| 1 | 20 | 4 | 1 | 0 | 0 |
| 2 | 30 | 4 | 1 | 0 | 0 |
| 3 | 30 | 3 | 1 | 0 | 0 |
| 4 | 30 | 3 | 2 | 0 | 0 |
| 5 | 40 | 3 | 2 | 0 | 0 |
| 6 | 40 | 3 | 2 | 0.5 | 0 |
| 7 | 40 | 3 | 3 | 0.5 | 0 |
| 8 | 40 | 3 | 3 | 0.5 | 1 |

## Uygulama ve doğrulama

1. Doğrulanan seviye ayarları, runtime factory, mevcut-harita hedef seçimi ve mevcut hasar ölçeklemesi.
2. Hareket ve saldırı sahipliğiyle entegre süreli sersemletme. Yalnızca navigasyon hızını sıfırlamak yeterli değildir; doğrudan Charger hareketi ve saldırılar da durmalıdır.
3. Yıldırım/zincir görselleri, asset, kayıtlı arena/F1 ve ability/karma ödül havuzları. Başlangıç ekipmanı ve slot sınırları korunur.
4. Seviye, hedef/görüş uygunluğu, collider tekilleştirme, bağımsız örtüşmeler, zincir dışlamaları, sersemletme yenileme/bitişi, asitle birlikte çalışma, Charger/Ranger durdurma, pause/ölüm/yeniden kullanım ve harita bağlama testlerini yazma. Katalog/havuz beklentilerini güncelleme.
5. EditMode/PlayMode ve oynanış kabulünü kullanıcı yapar; asistan yalnızca özellikle istenen eksik/başarısız testleri çalıştırır. Evolution, nihai görseller, build ve VCS işlemleri kapsam dışıdır.

Docs ve uygulama ayrı check-in noktalarıdır. Shuriken öncesinde oynanış kabulü için durulur.
