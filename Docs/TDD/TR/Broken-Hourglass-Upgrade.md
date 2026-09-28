# Kırık Kum Saati

## Durum ve kapsam

upgrades-content üzerinde mekanikler onaylandı. Uygulama öncesi doküman checkpoint'idir; oynanış değişikliği veya test çalıştırıldığı iddia edilmez.

Kırık Kum Saati (Broken Hourglass), ortak güçlendirme slotlarını ve beş seviyeyi kullanır. Tam etkiler önceki seviyenin yerini alır; uygun kaynakların yüzdeleri toplanır. Denge değerleri başlangıç niteliğindedir.

| Seviye | Bekleme süresi azaltımı |
| --- | --- |
| 1 | %5 |
| 2 | %10 |
| 3 | %15 |
| 4 | %20 |
| 5 | %25 |

## Zamanlama sözleşmesi

- Etkin bekleme = güncel yetenek seviyesinin temel beklemesi * (1 - toplam azaltma).
- Toplam azaltma %0–75 aralığına sınırlanır; bu öğe tek başına %25'e ulaşır.
- Normal yeteneklerin yeniden kullanım beklemesini ve Shuriken'in yörüngesi bittikten sonraki beklemeyi etkiler.
- Drone atış sıklığı, silah atış/şarjör süreleri, mermi hızı, aktif etki/yörünge süresi, salvo içi aralıklar, zamanlı hasar tick'leri veya sersemletme/yavaşlatma sürelerini etkilemez.
- Güncel azaltma yeni bekleme başlarken alınır. Öğeyi edinmek veya seviyesini artırmak başlamış beklemeyi kısaltmaz, sıfırlamaz, yeniden ölçeklemez ve anında kullanım sağlamaz.
- Hedef gereksinimleri, başarısız kullanım davranışı, duraklama/ölüm/kontrol koşulları ve mevcut yetenek seviyelendirme kuralları korunur.
- Shuriken'de sıradaki bekleme mevcut yörünge-sonu bekleme başlangıcında, geçerli temel beklemeden hesaplanır. Bir kez uygulanır; sürekli çalışan sistemin tamamı hızlandırılmaz.
- Azaltmanın katlanmaması için değiştirilmemiş temel değerlerden hesaplanır. Sahne geçişi başlamış beklemeleri korur; yeni run önceki run modifier'larını taşımaz.

## Uygulama ve doğrulama planı

Tüketiciler değiştirilmeden AbilityRuntimeState, AbilityRuntimeEntry ve Shuriken'in bağımsız sayacı incelenir. Statın anlamı açık tutulur: mevcut AbilityCooldown enum üyesi tek başına runtime formülünü belirlemez.

Beş seviyeli gerçek asset; kayıtlı içerik arenasına, oluşturucusuna, F1 kataloğuna ve upgrade/karışık havuzlara eklenir. Slot sınırları ve maksimum-seviye dışlaması korunur.

Tüm seviyeler, yüzdelerin toplanması/sınır, doğru süre hesabı, başlamış beklemenin korunması, başarısız kullanımlar, Shuriken yörünge-sonu beklemesi ve aktif sürelerin/Drone sıklığının değişmemesi için testler yazılır. Katalog kontrolleri güncellenir ve temsilî normal yetenekler kapsanır. Unity derlemesi, EditMode/PlayMode ve manuel kabul kullanıcıya aittir; asistan yalnızca açıkça istenen eksik/başarısız testleri çalıştırır.

## Ertelenen iş

Akıcı Mekanizma şarjör değiştirirken hareket cezasını azaltma kimliğini korur. Şu anda azaltılacak böyle bir hareket cezası yoktur; yerine şarjör hızı bonusu konmaz ve sırf öğeyi kullanmak için ceza eklenmez.

Önce mevcut sistemlerle uygulanabilen güçlendirmeler tamamlanır. Ek sistem/tasarım kararı gerektiren öğeler sonrasında tek tek ele alınır. Akıcı Mekanizma asset'i ve ödül havuzu bağlantısı ertelenmiştir; tamamlanmış sayılmaz veya on altı öğelik kapsamdan çıkarılmaz.
