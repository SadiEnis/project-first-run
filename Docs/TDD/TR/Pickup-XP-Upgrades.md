# Toplama ve XP güçlendirmeleri

## Durum

upgrades-content üzerinde Çekim Çekirdeği ve Hafıza Kristali mekanikleri onaylandı. Uygulama öncesi doküman checkpoint'idir; uygulama veya test çalıştırıldığı iddia edilmez.

İkisi de mevcut ortak güçlendirme slotlarını ve beş seviyeyi kullanır. Her seviye önceki tam etki grubunun yerini alır; farklı kaynakların normal yüzdeleri toplanır. Sayılar başlangıç dengesidir.

| Öğe | İngilizce ad | L1 | L2 | L3 | L4 | L5 |
| --- | --- | --- | --- | --- | --- | --- |
| Çekim Çekirdeği | Attraction Core | +%20 yarıçap | +%40 | +%60 | +%80 | +%100 |
| Hafıza Kristali | Memory Crystal | +%10 XP | +%20 | +%30 | +%40 | +%50 |

## Çekim Çekirdeği

- Yalnızca yerdeki XP parçalarının çekim yarıçapını etkiler.
- Sandık çekme, açma mesafesi veya otomatik açma yoktur. Diğer pickup türleri bu aşamanın dışındadır.
- Yapılandırılmış temel yarıçaptan hesaplanır; değiştirilmiş yarıçap tekrar çarpılmaz. 3 metre temel menzil son seviyede 6 metre olur.
- Çekilme hızı, fiziksel toplama davranışı, XP parçasının değeri ve mevcut ölüm/duraklama kısıtları korunur.
- Fizik taraması, çekim uygunluğu ve oyun sırasındaki menzil görselleştirmesi aynı etkin yarıçapı kullanmalıdır.
- Stat bağlantısında mevcut yarıçap setter'ı temel yarıçapı değiştirir olarak belgelenir; etkilenen testler bilinçli güncellenir.

## Hafıza Kristali

- Güncel XP çarpanı XP kazanıldığı anda uygulanır; parçanın oluştuğu anda değil. Önceden kazanılmış XP ve seviye eşikleri değişmez.
- Normal run XP kazancı tek uygulama noktasından geçer; çift bonus uygulanmaz. Test panelinin doğrudan seviye kontrolleri belgelenmiş davranışını korur.
- Kesirli XP run içinde saklanır: +%10 ile on adet 1 XP toplam 11 XP vermelidir. Son puanı kaybettiren kayan nokta sapması önlenir.
- Kesirli kalan seviye ve harita geçişinde korunur; çarpan değişince önceden kazanılmış kalan yeniden çarpılmaz. Yeni run'da sıfırlanır.
- Mevcut geçersiz girdi, taşma ve bildirim sırasında tekrar giriş korumaları korunur. Reddedilen kazanç kalanı veya pickup'ı tüketmez; bildirim hatasından önce işlenmiş kazanç tekrarlanmaz.
- Daha hızlı seviye atlama mevcut level-up sandık akışını doğal olarak çalıştırır. Sandık üretim kuralları değiştirilmez.

## Entegrasyon ve doğrulama

İki gerçek asset kayıtlı içerik arenasına, sahne oluşturucuya, F1 kataloğuna ve upgrade/karışık havuzlara eklenir. Slot kapasitesi ve maksimum-seviye dışlaması korunur.

Beş seviye, yüzdelerin toplanması, tam etki değişimi, yarıçap sınırları/temel değer değişimi, çekim hızının korunması, kesir birikimi, çarpan değişimi, çoklu seviye atlama, reddedilen kazançlar, kalıcılık ve yeni-run durumu için testler yazılır. Katalog sayıları güncellenir. Sandık etkileşim davranışı eklenmez.

Unity derlemesi, EditMode/PlayMode çalıştırılması ve manuel kabul kullanıcıya aittir. Asistan testleri yazar; yalnızca açıkça istenen eksik/başarısız testleri çalıştırır. Uygulama sonrasında kabul için durulur.
