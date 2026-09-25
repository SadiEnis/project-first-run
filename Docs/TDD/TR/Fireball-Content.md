# Fireball içeriği — tasarım taslağı

[Yetenek İçerik Planı](Abilities-Content-Plan.md) içindeki ilk yetenek. Mevcut `ability.fireball` asset/kimliği ve ödül üyeliği korunur. Mevcut kod en yakın kayıtlı düşmana tek düz mermi atar, trigger çarpışması kullanır ve geçici hasar/cooldown/hız seviyelerine sahiptir. Yanma veya çoklu mermi yoktur. Bu aşama geçici içeriği GDD 12.2 davranışına taşır; yeni eşya veya evolution değildir.

## Önerilen mekanik (uygulamadan önce netleştirilecek)

- Atış merkezinin 20 m çevresinde uygun hedef varsa otomatik çalışır. Yalnızca mevcut harita registry'sindeki canlı, aktif, hazırlanmış düşmanlar uygundur. Açık bir dünya maskesiyle siper arkasındaki hedefler dışlanır; aktörler siper sayılmaz. Hedef yoksa mermi çıkmaz ve cooldown tüketilmez.
- Bütün seri aynı anda çıkar. Uygun hedefler tükenene kadar tekrar etmeden rastgele seçilir; mermiden az düşman varsa yeniden karıştırılmış turlarla seçim tekrarlanır. Tek düşman tüm seriyi alabilir. Rastgelelik testte enjekte edilebilir olmalı, Unity global random durumunu tüketmemelidir. Hedefler üretim öncesinde yeniden doğrulanır; haritalar arası registry bağlantısı korunur.
- Mermiler hedefin fırlatma anındaki collider merkezine yönelip düz gider. Güdüm, yeniden hedefleme, patlama, delme veya itme yoktur. Her mermi tek hedefe bir kez hasar verir; yoldaki başka düşman veya duvar mermiyi karşılayabilir. Aynı serinin farklı mermileri aynı düşmana ayrı ayrı vurabilir.
- Başlangıç örtüşmesi, kaynağı dışlama, en yakın engel ve kaynak-doğma noktası engel kontrolü dahil süpürmeli çarpışma kullanılır. Yalnızca trigger'a güvenilmez. Öneri: .12 m yarıçap, 60 m azami yol, 5 s ömür; hareket kalan yol/zamana sınırlanır, bitişte hasarsız kaybolur. İlgisiz trigger'lar dışlanır. Kayıtlı görseller kardeş mermileri engellemez.
- Seviye ve stat uygulanmış doğrudan/yanma hasarı fırlatma anında kopyalanır; her temel değere AbilityDamage bir kez uygulanır. Silah değişimi veya level-up havadaki atışı değiştirmez. Pause hareketi/yanmayı durdurur. Oyuncu ölümü/yok edilmesi ve harita boşaltılması etkileri iptal eder; ölüm sonrası ödül oluşmaz. Etkiler kalıcı oyuncu hiyerarşisine değil haritaya/hedefe aittir.
- Başarılı seri başına tek cooldown, mermi başına değil. Üretimden önce bütün atış doğrulanır; oluşturma hatasında kısmen üretilen mermiler geri temizlenir. Mevcut edinim/seviye runtime'ı ve level-up sırasında kalan cooldown korunur.

## Önerilen seviyeler

Sayılar başlangıç denge önerisidir. Mermi sayısı ve yükseltme sırası GDD'yi izler.

| Seviye | Mermi başına doğrudan hasar | Cooldown s | Mermi | Hız m/s | Yanma süresi s |
| --- | --- | --- | --- | --- | --- |
| 1 | 25 | 2 | 2 | 12 | 1.5 |
| 2 | 30 | 2 | 2 | 12 | 1.5 |
| 3 | 30 | 1.5 | 2 | 12 | 1.5 |
| 4 | 30 | 1.5 | 3 | 12 | 1.5 |
| 5 | 35 | 1.5 | 3 | 12 | 1.5 |
| 6 | 35 | 1.5 | 3 | 16 | 1.5 |
| 7 | 35 | 1.5 | 4 | 16 | 1.5 |
| 8 | 35 | 1.5 | 4 | 16 | 3 |

## Yanma önerisi ve açık varsayımlar

GDD L8'de yanma süresini artırır fakat yanmanın hangi seviyede başladığını belirtmez. Öneri: L1'de başlar, L8'de süresi iki katına çıkar. Kabul edilen öldürücü olmayan doğrudan isabet .5 s arayla 5 temel yanma hasarı uygular; ilk tick .5 s sonra: yenilenmezse L1–7'de üç, L8'de altı tick.

Aynı kaynak/hedef/Fireball etkisi birikmez; tekrar uygulama süreyi yeniler, hasarı yeni fırlatmanın kopyasıyla değiştirir ve tick ritmini korur. Çoklu toplar yanma katmanı eklemez veya bekleyen tick'i ertelemez. Öneri: Plasma ve Fireball yanmaları ayrı etki kimlikleriyle birlikte çalışır; her biri kendi silah/yetenek stat kopyasını kullanır. Hedef ölümü, devre dışı/yeniden kullanım ve kaynak/harita temizliği ilgili etkileri kaldırır.

Yalnızca bu iki tüketicinin gerektirdiği ortak süreli hasar yaşam döngüsü ayrıştırılır; Plasma davranışı korunup regresyonları çalıştırılır. Yeteneğe Plasma'ya özgü WeaponDamage hattı bağlanmaz. Zeminde ateş veya yanma yayılması yoktur. Okunabilir geçici geri bildirim yeterlidir; nihai VFX/ses sonraya bırakılır.

## Uygulama checkpoint'leri ve doğrulama

1. Mekanik uzlaşması ve EN/TR docs kaydı; yetenek sırası ve düzeltilmiş Plasma kabulü dahil.
2. Rastgele menzilli hedefleme, bütünlüklü çoklu mermi atışı ve süpürmeli mermi yaşam döngüsü; testlerle bağımsız doğrulanabilirse anlamlı ara kayıt.
3. Ortak süreli hasar, Fireball yanması, sekiz seviye asset'i ve kayıtlı arena/F1/ödül entegrasyonu. Sahne yeniden üretimi, katalog kopyası veya ilgisiz denge değişikliği yok.
4. Tam EditMode/PlayMode doğrulaması, sonuçların dokümana işlenmesi, ardından Force Wave öncesinde kullanıcı testi.

Sıfır/tek/çok hedef, rastgele seçim turları ve menzil/siper sınırları, registry yeniden bağlama, tek cooldown ile 2/3/4 mermi, stat snapshot'ı, duvar/başlangıç/yüksek hız çarpışması, çift çözümleme olmaması, yarım atış temizliği, yanma yenileme/birlikte çalışma/bitiş/öldürme ödülleri, pause/ölüm/yeniden kullanım/harita temizliği, maksimum/dolu slot ödül uygunluğu ve mevcut silah/yetenek regresyonları kapsanır. Değerler ve hedef davranışı konuşulup netleşene kadar öneridir. Bu tasarım checkpoint'inde kod veya test çalıştırma yoktur.
