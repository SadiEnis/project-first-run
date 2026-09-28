# Koşullu ve bedelli güçlendirmeler

## Durum ve akış

upgrades-content üzerinde mekanikler onaylandı. Bu uygulama öncesi doküman checkpoint'idir; burada asset, oynanış değişikliği veya test çalıştırıldığı iddia edilmez.

### Son Direniş uygulama güncellemesi

Son Direniş uygulandı; Demir Yemin, Kan Ahdi ve Cam Kalp bekliyor. Son Direniş'in Unity derlemesi, otomatik test sonucu ve manuel kabulü henüz doğrulanmadı.

Üç seviyeli asset, yeni LowHealthDamageBonus statında (12) düz 0,20/0,30/0,40 değeri tutar. Kayıtlı oyuncu prefab'ındaki PlayerLastStandController can/stat değişikliklerini dinler ve runtime.last-stand kaynağıyla iki ayrı toplamsal hasar modifier'ına sahip olur. İkisini bildirimden önce değiştirir; devre dışı kalınca yalnızca kendi etkilerini kaldırır. Bonus değişmediyse işlem yapmaz; bildirim döngüsü ve tekrar birikme engellenir. Maksimum-can güncellemeleri mevcut survival controller'ı kullanır; mermi/zamanlı etki tüketicileri değişmedi.

Öğe kayıtlı arena, oluşturucu ve ödül havuzlarına eklendi (katalog/karışık havuz 23; upgrade havuzu 10). LastStandUpgradeTests eşit/üst/alt eşik, iyileşme, ölüm/reset, yaralı edinme, üç seviye, diğer kaynakları koruma, maksimum-can etkileşimi ve devre dışı/yeniden etkinleştirmeyi kapsar. Mevcut hasar anlık görüntüsü davranışı değişmedi; yeni testler tam hasar iletimi veya sahne yükleme kapsamı iddia etmez. Testler asistan tarafından çalıştırılmadı.

Sıra: Son Direniş, Demir Yemin, Kan Ahdi, Cam Kalp. Her öğeden sonra kullanıcı oynanış kabulü için durulur. Mevcut branch ve ortak güçlendirme slotları korunur; her özellik üç seviyelidir. Yeni seviye önceki kendi etkilerini değiştirir; eski seviyeler birikmez, diğer kaynaklar kaldırılmaz. Değerler başlangıç dengesidir.

| Öğe | İngilizce ad | L1 avantaj | L2 avantaj | L3 avantaj | Koşul veya sabit bedel |
| --- | --- | --- | --- | --- | --- |
| Son Direniş | Last Stand | +%20 hasar | +%30 | +%40 | Yaşayan oyuncunun canı güncel maksimumun %30'u veya altında |
| Demir Yemin | Iron Oath | +10 yüzde puanı hasar azaltma | +15 | +20 | Her seviyede -%10 hareket hızı |
| Kan Ahdi | Blood Pact | +%15 hasar | +%25 | +%35 | Her seviyede -%20 maksimum can |
| Cam Kalp | Glass Heart | +%20 hasar | +%35 | +%50 | Her seviyede +%25 alınan hasar |

Verilen hasar bonusları WeaponDamage ve AbilityDamage'a eşit uygulanır; Hiddet Mührü gibi normal bonuslarla toplanır. Bedeller seviyelerle büyümez. Ayrı rarity veya kapasite kuralı eklenmez.

## Son Direniş

- Yalnızca oyuncu hayattayken ve mevcut can <= 0,30 * güncel maksimum can olduğunda aktiftir; iyileşerek eşik aşılınca bonus kalkar.
- Edinme, seviye değişimi, hasar, iyileşme, maksimum-can değişimi ve reset sırasında yeniden değerlendirilir. Zaten eşik altındayken edinmek hemen çalışmalıdır.
- Koşullu modifier'ların sahipliği ayrı tutulur. Tekrarlanan bildirimler ve eşik geçişleri bonusu çoğaltmaz, başka öğenin modifier'ını kaldırmaz veya canı döngüsel biçimde değiştirmez.
- Mevcut hasar anlık görüntüsü zamanlaması korunur: fırlatılmış mermiler/başlamış etkiler yeniden yazılmaz. Sonraki hasar hesapları güncel bonusu kullanır.
- Ölüm, can sıfır diye bonusu etkinleştirmez; diriltme eklenmez.

## Demir Yemin

- Hasar azaltma yüzde puanları Demir Deri ve uygun diğer kaynaklarla toplanır; ortak %75 sınırı korunur.
- Sabit -%10 MoveSpeed, normal hareket bonuslarına eklenir. Rüzgâr Örgüsü +%25 ile net +%15 olur; ayrıca çarpılmaz.
- Yapılandırılmış yürüme/koşma hızını etkiler; yerçekimi veya zıplama davranışını değiştirmez. Yeni reload hareket cezası eklenmez.
- Mevcut hareket doğrulaması korunur; ileride yeni minimum-hız kuralı gerektiren birleşimler ayrıca kararlaştırılır.

## Kan Ahdi

- -%20 MaxHealth ve verilen hasar bonusu tek öğenin tam etki grubu olarak uygulanır.
- Maksimum can özgün temel değer ve tüm modifier'lardan hesaplanır. İkinci Kalp +%50 ile Kan Ahdi -%20 birlikte net +%30 maksimum can verir.
- Onaylanan maksimum-can azalma kuralı kullanılır: 100/100 → 80/80; 60/100 → 60/80. Sınıra çekme hasar değildir, vurulma tepkisi oluşturmaz.
- Son Direniş sonuçtaki mevcut/maksimum cana göre yeniden değerlendirilir. Yalnızca maksimum-can değişimi bile eşik uygunluğunu değiştirebilir.
- Seviyelendirme tekrar can cezası uygulamaz; bedel -%20 kalır. Öğeyi uygulamak için oyuncu resetlenmez/canı doldurulmaz.

## Cam Kalp

- Gelen hasar önce 1,25 ile çarpılır, sonra zırh azaltımı uygulanır; gerçek can kaybı kalan canla sınırlanır.
- Örnek: 20 hasar ve %20 azaltma için 20 * 1,25 * 0,80 = 20 can kaybı.
- Orijinal DamageInfo korunur; gerçek uygulanan hasar DamageResult/olaylarda ayırt edilir. Artırım bir kez ve yalnızca oyuncuya uygulanır.
- İyileştirme ve maksimum-can sınırlaması gelen hasar değildir. Yeni vurulma tepkileri veya düşman hasar değişiklikleri eklenmez.

## Entegrasyon ve doğrulama

Her uygulanan öğe için gerçek asset, kayıtlı içerik arenası/F1 ve sahne oluşturucu girdileri ile upgrade/karışık havuz bağlantıları eklenir; maksimum-seviye dışlaması korunur. Henüz uygulanmamış öğeler seçilebilir ödül olarak sunulmaz.

Üç seviye değerleri, sahiplik/değiştirme, normal yüzdelerin toplanması, sabit bedeller, sınırlar, iki hasar statı, Son Direniş eşit/üst/alt eşikleri/ölüm/iyileşme, maksimum-can etkileşimleri, çoğalmayan abonelikler ve Cam Kalp işlem sırası/orijinal-gerçek hasar için testler yazılır. Harita geçişinde run durumu korunur, yeni run izole olur. Mevcut hasar anlık görüntüsü testleri kullanılır; değişen tüketicilere hedefli regresyon eklenir.

Unity derlemesi, EditMode/PlayMode ve oynanış kabulü kullanıcıya aittir. Asistan testleri yazar; yalnızca açıkça istenen eksik/başarısız testleri çalıştırır. Manuel kabul otomatik test sonucu sayılmaz.

Akıcı Mekanizma kimliği değişmeden ertelenmiş kalır. İçe parlayan özel çerçeveler, altın/meta, evolution ve nihai denge bu grubun dışındadır.
