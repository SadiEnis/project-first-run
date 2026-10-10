# Alternatif geniş savaş alanı

Bu çalışma, kullanıcının daha geniş savaş alanlarına sahip bir taslak istemesi üzerine yapılan ayrı bir greybox denemesidir. Onaylı `DeepJam_Opening` sahnesinin yerine geçmez. Hedef sahne: `DeepJam_WideCombat`; navigasyon verisi de ayrı tutulur.

## Geniş alan düşman yoğunluğu

### Ön zemin eklemesi

Merdivenler arasındaki boş zemine altı Chaser eklenir: X = ±7, Z = 110/120/130, Y = -19,9. Mevcut arka düşmanlar korunur; ana arena toplamı 24 (15 Chaser, 6 Ranger, 3 Charger) olur. Yeni noktalar ArenaLeft grubunun spawn listesinin sonuna eklenir; ayrı dalga veya yeni tetikleyici yoktur. Hazırlık ve algılama davranışı aynıdır.

Editörde ekleme: ilgili Encounter nesnesinin Prepared Region Encounter bileşenindeki Group asset'inde Entries/Count değerini değiştir. Encounter altında boş Transform oluşturup NavMesh üzerindeki konuma taşı; Spawn Points listesine ekle. Entries sırayla bu listeyi tüketir. Nokta sayısı yetmezse liste başa sarar, aynı yerde düşmanlar birikir; toplam sayı kadar ayrı nokta kullan. Sadece nokta eklemek sayıyı artırmaz. Play Mode dışında düzenle ve sahneyi kaydet.

- WideCombat'a özel karşılaşma asset'leri: ikinci oda 6 (4 Chaser, 2 Ranger); ana arena 18 (sol 9 Chaser, sağ 6 Ranger ve 3 Charger). Mevcut tür oranları korunur.
- Her düşmana ayrı, zemin üzerinde ve siper/kapı dışında bir doğma noktası ayrılır. Mevcut hazırlık ve algılama sistemi korunur; ek dalga veya zorunlu temizleme koşulu eklenmez.
- Koridor 4, opsiyonel oda 3 ve anahtar pususu 3+3 olarak kalır. Orijinal demo ve paylaşılan karşılaşma asset'leri değişmez.
- Hasar, can, XP ve düşme oranları değişmez; artan toplam XP/sandık fırsatı oynanışta değerlendirilir. Bu sayılar ilk yoğunluk denemesidir.
- Testler özel asset izolasyonunu, sayıları, türleri ve benzersiz spawn noktalarını kontrol eder. Unity testleri kullanıcı tarafından çalıştırılır.

Uygulama: üç ayrı EW_Wide asset'i ve 15 yeni spawn noktası eklendi. Geometri/NavMesh değiştirilmedi; önceki yerleşim bake'i güncelse yalnızca sayılar için yeniden bake gerekmez. Unity testleri çalıştırılmadı.

## İlk tasarım

- Ana arena 30×28 m yerine yaklaşık 48×36 m: dış çevrede dolaşım, iki farklı merdiven yaklaşımı ve siperler arasından çapraz geçiş.
- İlk koridor yaklaşık 13×30 m, ikinci savaş odası yaklaşık 23,5×24 m. Kapı açıklıkları okunabilir kalır; duvar ve trigger sınırları yeni geometriyle birlikte taşınır.
- Balkon yüksekliği değişmez. Yüksek, şaşırtmalı görüş kesiciler bazı düşmanları balkondan gizler; bütün alan körleştirilmez. Merdiven dipleri ve rota kapıları kapatılmaz.
- Düşman sayısı, tanımları, hasar/can, ödül tabloları ve XP değerleri değiştirilmez. Ana arena spawn noktaları yeni siper ceplerine dağıtılır.
- Parkur platformları, boşlukları, düşüş hasarı ve dönüş noktası birbirlerine göre aynı kalır. Anahtar odası ve pusu cepleri bütün olarak taşınır. Final gösterimi kendi iç ölçülerini korur.
- Opsiyonel oda, anahtar yolu, dönüş koridoru ve final kapısı bağlantıları korunur. Bu bir runtime harita üreticisi değildir; sonuç doğrudan düzenlenebilir kayıtlı sahnedir.

## Kabul

Sahne açılır, kendi NavMesh'i bake edilip kaydedilir. Başlangıçtan final ve yeniden oynamaya kadar rota denenir. Balkon ve arena zemini ayrı ayrı değerlendirilir: aşağı inmek avantajlı mı, siper çevresinde dolaşım var mı, düşmanlar yürüyebiliyor mu, kapılar ve merdivenler açık mı? Aynı düşman sayısıyla alanın fazla boş kalıp kalmadığı ayrıca değerlendirilir; bu deneme denge onayı değildir.

Mevcut demo sahnesinin ve nav asset'inin değişmemesi, parkur ölçülerinin korunması ve alternatif sahnedeki gameplay referanslarının çözülmesi statik/editör kontrollerinin kapsamıdır. Unity oynanış ve bake doğrulaması yapılmadan tamamlandı sayılmaz. Alternatif sahne otomatik olarak build başlangıcı yapılmaz.

## Kullanıcı yerleşimine uyarlama

- Daraltılan koridor, küçültülen ödül odası ve ikinci oda korunur. Ödül doğma noktası küçülen odanın içine alınır.
- Yeni ana zemin 48 × 71,3016 m; üst yüzeyi Y = -19,9. Balkon Y = -3 seviyesinde kalır. İki iniş, 0,3 m oyuncu basamak sınırını aşmayan 68 basamakla bu yüksekliğe bağlanır.
- Opsiyonel oda, parkur, anahtar odası, dönüş yolu ve final zemini ana zemin seviyesine hizalanır. Yan bağlantılar uzayan merdivenlerin sonrasına taşınır; parkur boşlukları ve düşüş/geri dönüş davranışı korunur. Eski kısa anahtar çıkış/iniş basamakları düz bağlantı olur.
- Arena sınırları, siperler, doğma noktaları ve tetikleyiciler uyarlanır. İleri taşınan final odası korunur; arena üstüne binen yaklaşım koridoru arena sonundan başlayacak şekilde kısaltılır. Final kapısı, giriş hacmi ve kamera/kol hedefleri taşınır.
- Düşman sayısı, ödül oranları ve hasar değerleri değişmez. Orijinal demo sahnesi değişmez. Geniş sahnenin NavMesh'i yeniden bake edilmelidir; eski bake yeni geometriyi doğrulamaz.

## Uyarlama teslimi

Kayıtlı sahne yukarıdaki yerleşime uyarlandı. Merdivenler her yanda 68 basamak (yaklaşık 0,249 m yükselti / 0,55 m derinlik); yan oda girişleri Z = 145,2, final kapısı arena bitişi Z = 170,5015 konumunda. Anahtar hattının önceki 2 m yükseltisi kaldırıldı; bağlı yürünebilir zeminler Y = -19,9 seviyesinde. Kullanıcının ana zemin ölçüleri ve ikinci oda geometrisi korundu.

EditMode dosyası yedi kontrol içerir: kat hizaları/referanslar, parkur oranları, basamak sürekliliği, ödül/final hedefleri, balkon görüş kesicileri, ayrı nav asset'i ve bake sonrası spawn/rota erişimi. Statik sahne kontrolünde eksik yerel referans veya yinelenen fileID yok. Unity testleri ve oynanış çalıştırılmadı; NavMesh bu değişiklikler sonrasında henüz bake edilmedi.

Unity'de sahnenin diskten güncel halini aç; WideCombat aktifken `Project First Run → Demo → Rebake Demo Navigation` çalıştır ve kaydet. Eski açık sahneyi kaydederek disk değişikliklerinin üstüne yazma. Ardından merdivenlerden iniş, iki yan rota, anahtar pususu ve finali dene. Build başlangıcı değiştirilmedi.

## İlk teslim durumu (kullanıcı düzenlemelerinden önce)

Alternatif kayıtlı sahne oluşturuldu. Ana arena 48×36 m, ikinci oda 23,5×24 m; yüksek siperler şaşırtmalı ve altı arena düşmanının doğma konumu yeniden yerleştirildi. Önceki odalar ve aralarındaki geçişler yatayda genişletildi; yükseklikler korunuyor. Parkur/anahtar alanı (+9; 0; +26,2) ötelenmiş, parkur boşlukları ve pusu odası ölçeği korunmuştur. Orijinal sahne ve OpeningNavigation.asset değiştirilmedi.

Kullanım: `Project First Run → Demo → Open Wide Combat Greybox`, ardından `Rebake Demo Navigation` ve kaydet. İlk bake `WideCombatNavigation.asset` oluşturur; bu sahne başlangıçta eski NavMesh'i paylaşmaz. Play'den önce bake gereklidir. Mevcut Build Windows Opening komutu hâlâ asıl sahneyi kullanır; alternatif otomatik olarak başvuru build'ine alınmaz.

DemoWideCombatLayoutTests dört EditMode kontrolü içerir: ölçüler/gameplay referansları, parkur ölçülerinin korunması, balkondan her iki hatta en az bir düşmanın siperle gizlenmesi ve bağımsız NavMesh asset'i. Son kontrol ilk bake'den sonra geçebilir. Testler ve Unity oynanış bu teslimde çalıştırılmadı. Statik kontrolde yeni sahnede tekrar eden fileID bulunmadı; kullanıcı ayar dosyalarına dokunulmadı. Düşman sayısı aynı kaldığından daha geniş alanın boş hissedip hissetmediği oynanışta değerlendirilecektir.
