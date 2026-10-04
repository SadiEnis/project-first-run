# Acid Bottle — içerik uygulaması

Onaylanan içerik sırasının dördüncü yeteneği; abilities-content üzerinde ilerlenir. GDD 12.6 rastgele hedefe şişe, süreli asit alanı ve L8 yavaşlatmasını tanımlar. L8 yavaşlatması %50 olarak ayarlandı ve kullanıcı oynanış kabulü tamamlandı. Sayılar dengeleme için geçicidir.

## Kararlaştırılan davranış

- 4 saniyede bir, 15 m içinde görünen canlı mevcut-harita düşmanı varsa rastgele seçilen düşmanın atış anındaki zemin konumuna şişe fırlatılır. Güdüm yoktur; hareket eden düşman iniş noktasından uzaklaşabilir. Hedef yoksa cooldown tüketilmez.
- Kısa, görünür yay hareketi dünya geometrisiyle çarpışmada sonlanır; uçuş sırasında karakterler dikkate alınmaz. Asit yalnızca NavMesh ile doğrulanan uygun yürünebilir zeminde oluşur; duvar/tavanda havada duran gölcük olmaz. Sekme veya ayrı doğrudan isabet/patlama hasarı yoktur.
- Gölcük indiği yerde kalır. Yarıçap içinde, aynı katta ve gölcükten kendisine engelsiz yol bulunan düşmanlar hasar alır. İlk tick .5 s sonra, devamı .5 s arayladır. Dışarı çıkınca sonraki tick'ler durur; üzerinde taşınan yanma etkisi değildir.
- L5 tek aktivasyon/cooldown içinde iki şişe fırlatır; mümkünse farklı hedefler seçilir. Aynı oyuncunun Acid Bottle alanları üst üste geldiğinde tick hasarı veya yavaşlatma katlanmaz. Yenileme/örtüşme hasarı sürekli ertelememelidir.
- L8 yalnızca asit içindeyken yerdeki hareket hızını %50 azaltır. Alanlar üst üste gelince yavaşlatma birikmez. Saldırı cooldown'ları yavaşlamaz; çıkış, süre bitişi, ölüm ve yeniden kullanımda hareket geri yüklenir. Charger hareketi aynı katkıyı kullanır.
- Tick hasarına AbilityDamage atış anında uygulanır. Pause uçuşu, alan süresini ve tick'leri dondurur. Oyuncu ölümü/harita boşaltma şişeleri, alanları ve yavaşlatma katkılarını temizler. Yeni haritada registry yeniden bağlanır. Evolution oynanışı ve nihai görseller yoktur.

## Başlangıç değerleri

| Seviye | .5 s başına hasar | Süre s | Yarıçap m | Şişe | Yavaşlatma |
| --- | --- | --- | --- | --- | --- |
| 1 | 5 | 3 | 2 | 1 | Yok |
| 2 | 7 | 3 | 2 | 1 | Yok |
| 3 | 7 | 4 | 2 | 1 | Yok |
| 4 | 7 | 4 | 3 | 1 | Yok |
| 5 | 7 | 4 | 3 | 2 | Yok |
| 6 | 10 | 4 | 3 | 2 | Yok |
| 7 | 10 | 5 | 3 | 2 | Yok |
| 8 | 10 | 5 | 3 | 2 | %50 |

## İş akışı

Runtime, sekiz seviyeli asset, kayıtlı arena/F1 kataloğu ve ability/karma ödül havuzları bağlandı. EnemyMotor kaldırılabilir hareket katkılarını yönetir; en güçlü yavaşlatma çarpılarak birikmeden uygulanır. Charger ilerlemesi de etkilenir; saldırı cooldown'ları değişmez. Yeşil ilkel şişe/alan görselleri geçicidir.

Üç EditMode kontrolü seviye verilerini, hareket katkılarının sahipliğini ve geçersiz ayarları kapsar. Mevcut katalog/havuz beklentileri güncellendi; ability sandığı testi büyüyen rastgele havuzda belirli bir yeteneğin daima sunulacağını varsaymaz. Bu değişiklikler asistan tarafından Unity'de derlenmedi veya çalıştırılmadı. EditMode/PlayMode çalıştırma ve commit/check-in kullanıcıya aittir; asistan yalnızca bildirilen eksik/başarısız testlerle ilgilenir.
