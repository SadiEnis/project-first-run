# Drone içeriği
Fiziksel mermi yerine tek tek raycast atışı ve kısa iz kullanılır. Her drone mevcut haritadaki canlı ve görünür en yakın düşmanı bağımsız seçer. Duvarlar atışı keser; ışının ilk vurduğu düşmana AbilityDamage bir kez uygulanır. Mühimmat, reload, hazırlanma, delme, yanma ve evolution oynanışı yoktur.

| Seviye | Hasar | Drone başına atış/s | Menzil m | Drone | Boss hız çarpanı |
| --- | --- | --- | --- | --- | --- |
| 1 | 8 | 2 | 12 | 1 | 1 |
| 2 | 12 | 2 | 12 | 1 | 1 |
| 3 | 12 | 3 | 12 | 1 | 1 |
| 4 | 12 | 3 | 18 | 1 | 1 |
| 5 | 16 | 3 | 18 | 1 | 1 |
| 6 | 16 | 3 | 18 | 2 | 1 |
| 7 | 16 | 4 | 18 | 2 | 1 |
| 8 | 16 | 4 | 18 | 2 | 1.5 |

Hasar/menzil geçici başlangıç değerleridir. L8 yalnız Boss rank'ına uygulanır. İkinci drone yarım atış aralığı gecikmeyle başlar; bağımsız hedefleme kalıcı dönüşümlü atış garantisi vermez. Yükseltmeler kalan beklemeyi korur; hedefsiz karelerde yeni cooldown tüketilmez, takılma sonrası toplu telafi atışı olmaz.

Dar kapsamlı sürekli-yetenek tick kancası normal atış yeteneklerinin yolunu değiştirmez. Drone, entry'nin tek atış cooldown'ı yerine drone başına sayaç kullanır. Haritaya ait omuz görsellerinde collider yoktur, izler tekrar kullanılır ve registry yeniden bağlanınca görseller yeniden kurulur. Dünya engelleri konumu sınırlar; pause sayaçları durdurur, kontrol kapalıyken ateş edilmez, ölüm/kaynak yok olması görselleri temizler. İki drone tek ability slotundadır. Factory başarılı edinme öncesinde sahne nesnesi oluşturmaz.

AD_Drone sahne yeniden üretilmeden F1, ability/karma havuzları ve editor builder'a eklenir. Geçici geometri/materyal geri bildirim içindir; nihai görsel değildir.

## Doğrulama
Kabul güncellemesi: kullanıcı Drone oynanışını, iki drone'u ve hedefe yönelmelerini kabul etti. EditMode 889/889 ve dört silah-arena sınıfı dışındaki PlayMode testlerinin yeşil olduğunu bildirdi. Sonrasında yalnızca bu dört sınıf çalıştırıldı: Minigun 13/13, Plasma 2/2, Rocket 2/2, Shotgun 9/9 (26/26, weapon-arenas-only.xml). Asistanın tam paket çalıştırdığı veya L8 boss davranışının ayrıca kabul edildiği iddia edilmez. Aşağıdaki paragraf ilk teslimin kaydıdır.

Kullanıcı isteğiyle bu aşamada EditMode/PlayMode ve build atlanır; derleme/çalışma başarısı iddia edilmez. Katalog sayısı beklentileri mekanik olarak güncellenir. Elle kabul: F1 ile edin; takip/hedefleme/duvar engelini, L3/L7 hızı, L4 menzili, L6 ikinci drone'u dene; pause, ölüm ve harita geçişini kontrol et. L8 için Boss rank'lı hedef gerekir. Önceki tam paket takibi ertelenmiş olarak kalır.
