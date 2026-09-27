# Hayatta kalma stat güçlendirmeleri

## Durum ve kapsam

upgrades-content üzerinde İkinci Kalp, Demir Deri ve Yaşam Filizi mekanikleri onaylandı. Bu uygulama öncesi doküman checkpoint'idir. Oynanış uygulaması veya test çalıştırılması tamamlanmış değildir.

Mevcut ortak güçlendirme slotları ve beş seviyeli ilerleme kullanılır. Her seviye öğenin önceki tam etki grubunu değiştirir; diğer kaynaklar korunur. Değerler başlangıç dengesidir.

| Öğe | İngilizce ad | L1 | L2 | L3 | L4 | L5 |
| --- | --- | --- | --- | --- | --- | --- |
| İkinci Kalp | Second Heart | +%10 maksimum can | +%20 | +%30 | +%40 | +%50 |
| Demir Deri | Ironhide | %5 hasar azaltma | %10 | %15 | %20 | %25 |
| Yaşam Filizi | Lifesprout | 0,5 can/sn | 1 | 1,5 | 2 | 2,5 |

## İkinci Kalp

- Maksimum can yapılandırılmış temel değerden ve güncel modifier'lardan hesaplanır; önceki değiştirilmiş maksimumdan hesaplanmaz.
- Yaşayan oyuncuda maksimum can artışının pozitif farkı mevcut cana da eklenir: 60/100 → 70/110. Tam iyileştirme sağlamaz.
- Maksimum can azalınca mevcut can korunur; yalnızca yeni maksimumu aşan kısım sınıra çekilir. Bu hasar değildir ve hasar tepkisi tetiklemez.
- Değişmeyen modifier'ların yeniden hesaplanması veya harita geçişi tekrar can kazandırmaz.
- Maksimum can değişimi ölü oyuncuyu diriltmez. Açık yeni-run/reset davranışı ayrıdır.

## Demir Deri

- Uygun kaynakların normal hasar azaltma yüzdeleri toplanır; toplam %0–75 aralığına sınırlanır.
- Gelen hasar * (1 - etkin azaltma) hesaplanır; ardından gerçek can kaybı kalan canla sınırlanır.
- Kesirli hasar desteklenir; yuvarlama veya keyfi bir minimum 1 hasar kuralı eklenmez.
- Oyuncunun ortak hasar alma sınırında bir kez uygulanır; mevcut düşman saldırılarını ve bu sınırdan geçen diğer hasarları kapsar. Oyuncunun zırhı düşmanlara uygulanmaz.
- Olaylarda ve ilerideki tepkilerde orijinal gelen hasar ile gerçek can kaybı ayırt edilebilir kalır. İyileştirme ve maksimum-can sınırlaması hasar değildir.
- Sonraki özellik güçlendirmelerinin etkileşimleri ayrıca kararlaştırılır; bu aşama Demir Yemin veya Cam Kalp'i uygulamaz.

## Yaşam Filizi

- Aktif, ölçeklenmiş oyun zamanında saniyede bir iyileştirir; her tick güncel seviyenin can/sn değerini verir.
- Çatışmada ve keşifte çalışır. Duraklamada/ödül seçiminde ve ölüm/run bitişinden sonra durur.
- Maksimum canı aşmaz, diriltmez ve tam canda kullanılmayan iyileştirmeyi biriktirmez.
- Seviye değişimi anında ek tick veya ikinci bir yenilenme döngüsü oluşturmaz.
- Sahne geçişi/yükleme süresi toplu iyileştirmeye dönüşmez. Run statları korunurken zamanlayıcılar veya stat abonelikleri çoğalmaz.

## Uygulama sınırları

Mevcut HealthState maksimumu değişmezdir ve iyileştirme işlemi yoktur. Ortak can modeline doğrulamalı iyileştirme ve canlı maksimum-can değişimi eklenir. Spawn başlangıcı/reset, güçlendirme etkisi için kullanılmaz; bunlar mevcut can durumunu değiştirecektir.

Oyuncu stat bağlantısı düşman can yapılandırmasından ve havuzdan yeniden doğan düşman başlangıcından ayrılır. Mevcut hasar/ölüm olayları ve ölümün bir kez bildirilmesi korunur. Mevcut veya maksimum can değiştiğinde debug/UI tüketicileri için can değişikliği bildirilir.

Üç gerçek asset; kayıtlı içerik arenası/F1, sahne oluşturucu ve upgrade/karışık ödül havuzlarına bağlanır. Maksimum-seviye dışlaması ve mevcut slot sınırları korunur. Özel çerçeve ışıltısı, altın, meta ilerleme, evolution ve build bu grubun dışındadır.

## Doğrulama planı

- Beş seviyeli tablolar, tam etki değişimi, yüzdelerin toplanması ve maksimum seviyede reddetme için testler yazılır.
- Maksimum can: yaralı/tam/ölü durumlar, artış/azalış, değişmeyen yeniden hesaplama, geçersiz değerler ve olay davranışı.
- Hasar azaltma: sıfır zırh, her seviye, birden fazla kaynak, %75 sınırı, kesirli/ölümcül hasar ve çift uygulamama.
- Yenilenme: tick sıklığı, seviye değişimi, aşırı iyileştirme, biriktirmeme, duraklama/ödül ekranı, ölüm ve çift döngüyü engelleme.
- Harita geçişinde koruma ve yeni run'da önceki run modifier'larını taşımadan sıfırlama.
- Katalog/havuz beklentilerini güncelleme ve üç güçlendirmeyi birlikte sınama.

Unity derlemesi, EditMode/PlayMode ve oynanış kabulünü kullanıcı yapar. Asistan testleri yazar; yalnızca açıkça istenen eksik/başarısız testleri çalıştırır. Sonraki gruba geçmeden manuel kabul için durulur.
