# Düşman algılama — demo temeli

## Durum ve kapsam

Tasarım checkpoint'i; henüz oyun kodu veya sahne bağlantısı değişmedi. `feature/deepjam-demo` içinde uygulanacak. Amaç, hazır düşmanların oyuncuyu görmeden takibe başlamamasıdır. Greybox oda ölçüleri bu aşamada değiştirilmez. Önce algılama ve mevcut açılış grubuyla entegrasyon; yeni odalara karşılaşma ekleme ayrı sonraki adımdır.

## Mevcut koddaki entegrasyon noktaları

- `EnemyController.Initialize` motoru başlatır; `OnEnable` tekrar `Resume` çağırabilir. `EnemyMotor` hedef Transform'un güncel konumunu periyodik olarak takip eder. Görüş kaybında bu yolun son görülen konumu aşarak güncel oyuncu konumunu okuması engellenmelidir.
- `EnemyAttackController` Chaser, Charger ve Ranger davranışlarını yürütür. Ranger geri çekilirken destination override kullanır; Charger atılış yönünü sabitler. Algılama bunlarla yarışan ikinci bir hareket sahibi olmamalıdır.
- Ranger'ın görüş hattı kontrolü var; temel Chaser saldırısı düzlemsel mesafeye bakıyor. Demo algılama politikası altında yeni yakın saldırı hem engelsiz görüş hem gerçek 3B saldırı mesafesi gerektirecek; duvar veya farklı kat üzerinden hasar verilmeyecek. Bu değişiklik eski politikasız düşmanlara sessizce uygulanmaz.
- `HealthComponent.Damaged`, uygulanmış hasarın `DamageInfo.Source` bilgisini taşır; ölümcül hasarda `Died` olayından önce çağrılır. Alarm için ayrıca canlılık/ölümcüllük kontrolü gerekir. Drone, yanma ve kanama akışlarında sahip oyuncu Source olarak taşınabiliyor; alarm atış objesinin adına/tag'ine göre tahmin edilmez.

## Yapılandırma ve uyumluluk

`EnemySpawner` üzerinde isteğe bağlı bir algılama profili referansı önerilir. Profil atanmadığında bugünkü otomatik takip/saldırı davranışı korunur. Demo için ayrı profil kullanılır; ortak düşman prefab'ı ve tanımları küresel olarak algılamaya zorlanmaz. Profil değişmez tasarım verisi, farkındalık/hafıza ise her düşmanın kendi runtime durumudur.

İlk deneme değerleri, nihai denge değil:

| Ayar | Başlangıç değeri | Anlam |
| --- | --- | --- |
| İlk algılama mesafesi | 18 m | Gerçek 3B uzaklık |
| İlk yatay görüş açısı | 120° toplam | İleri yönün iki yanında 60° |
| Alarm sonrası görsel takip mesafesi | 24 m | Alarmda koni aranmaz; mesafe ve LOS hâlâ zorunlu |
| Görüş sorgusu aralığı | 0,1 s | Oyun zamanı; pause sırasında ilerlemez |
| Son bilgi hafızası | 3 s | Son görsel teyit veya geçerli hasardan itibaren |
| Son noktaya varış toleransı | max(0,75 m, stoppingDistance + 0,1 m) | Motor durma mesafesiyle çelişmez |

Göz ve hedef gövde yükseklikleri, engel layer maskesi ve bu değerler Inspector'dan düzenlenebilir. Başlangıç göz/gövde noktası kök konumundan yaklaşık 1 m yukarıdır. Sorguda kendi collider'ları, hedef oyuncunun collider'ları ve trigger'lar engel sayılmaz; diğer katı collider'lar varsayılan olarak görüşü keser. Aynı yatay konumdaki hedef için yatay açı geçerli kabul edilir, 3B mesafe ve LOS yine değerlendirilir. Kısa siper üzerinden görülebilme bu tek gövde noktası modeline bağlıdır; çok noktalı görünürlük ilk kapsama dahil değildir.

## Durum ve davranış sözleşmesi

1. **Bekleme:** hazırlığı tamamlanıp karşılaşması etkinleşmiş düşman yerinde durur; devriye veya otomatik tarama dönüşü yapmaz. İlk tespit için mesafe + yatay koni + engelsiz görüş birlikte gerekir. Başlangıç spawn yönleri girişe/istenen gözetleme yönüne göre sahnede ayarlanır.
2. **Görerek takip:** hedefin güncel konumu sadece görsel teyit varken hareket kararı için kullanılabilir. Alarmda 24 m ve LOS şartıyla koni dışında da takip sürer; arkaya geçmek anında unutmaya yol açmaz. Chaser takip, Ranger mesafe koruma ve Charger hazırlık davranışları mevcut tür kurallarına tabidir.
3. **Son bilinen noktaya yönelme:** görüş kaybında bir konum anlık görüntüsü tutulur. Motor bunu izler; duvar arkasındaki hedef Transform'undan yeni rota/geri çekilme kararı üretilmez. Noktaya erken ulaşırsa kalan hafıza süresince bekler. Yol kısmi/geçersizse ulaşabildiği yerde durur; teleport veya sonsuz yeniden deneme yoktur.
4. **Unutma:** 3 saniye yeni bilgi yoksa hareket durur, bulunduğu yerde bekleme/koni politikasına döner. Spawn'a dönme, bölgesel leash, can yenileme veya düşman respawn'ı bu sürümde yoktur. Sağlık ve cooldown sıfırlanmaz. Bölge geçişlerinin fiziksel sınırları ayrıca korunur.

## Hasarla alarm

Yalnız uygulanmış, ölümcül olmayan, atanmış hedef oyuncudan veya onun hiyerarşisinden kaynaklanan hasar alarm oluşturur. Null, çevre veya başka düşman kaynakları oyuncuyu otomatik hedef göstermez. Sahipli mermi/yetenekler oyuncu Source bilgisini taşımaya devam etmelidir.

Görülmeyen oyuncudan ilk kez hasar alan bekleyen düşman, hasar anındaki oyuncu konumunu bir defalık araştırma noktası olarak kaydeder; başlangıç konisi/mesafesi alarmı engellemez. Görsel teyit olmadan yeni saldırı izni verilmez. Alarm zaten sürerken gelen hasar hafızayı yeniler ancak gizli oyuncunun konumunu sürekli güncellemez. Böylece yanma/kanama tick'leri alarmı uzatabilir fakat duvar arkasında kesintisiz konum takibine dönüşmez. Görüş yeniden kurulduğunda son bilgi normal güncellenir.

## Saldırı, motor ve yaşam döngüsü

- Algılama hareket/saldırı izni ve son bilinen hedefi sunar; aynı karede birden fazla sistemin `Stop/Resume/SetDestination` ile birbirini ezmesine izin verilmez. Gizli hedefte Ranger geri çekilme kararı veremez, Charger yeni atılış hazırlığına giremez.
- Görüş kaybında Ranger'ın henüz bırakılmamış atışı ve Charger'ın henüz başlamamış atılış hazırlığı iptal edilir. Ateşlenmiş fiziksel mermi normal ömrünü sürdürür. Başlamış Charger atılışı sabit yönde mevcut engel/hit/recovery kurallarıyla tamamlanır; hedefi duvar arkasında yeniden nişanlamaz. Recovery sonunda algılama politikası yeniden belirleyicidir.
- Cooldown ve stun kuralları korunur. Algılama değiştikçe bileşenleri açıp kapatmak veya `Stop` ile saldırı durumunu her kare sıfırlamak kabul edilmez; yeniden görme ücretsiz saldırı/cooldown reset'i vermez.
- `PreparedRegionEncounter` hazırlığı sırasında algılama veya hasar alarmı mücadeleyi erkenden başlatmaz. Entegrasyon ilk aktif hareket/saldırı tick'inden önce kurulur; bir karelik otomatik takip sızıntısı olmamalıdır.
- Pasif fakat etkin düşmanlar registry'de kalır; otomatik yeteneklerin hedef havuzundan sırf oyuncuyu fark etmedikleri için çıkarılmazlar. Hazırlık aşamasındaki dokunulmazlık mevcut yaşam döngüsünün sorumluluğudur.
- Oyuncu ölünce/hedef kaybolunca hareket ve yeni saldırı durur. Düşman ölümünde, disable ve yeniden initialize durumlarında hafıza, olay abonelikleri ve bekleyen izinler temizlenir. Pool/re-enable ile eski oyuncu bilgisi taşınmaz; yalnız algılama bileşeninin kapatılması aynı instance'ı otomatik agresif yapmaz.
- İleride anahtar pususu için açık alarm girişi sağlanabilir; karşılaşma etkin değilken kullanılamaz ve LOS/hasar şartlarını kaldırmaz. Bu aşama anahtar/pusu mekaniğini oluşturmaz.

## Kabul ve çalışma sırası

1. Saf durum/matematik testleri: menzil/koni sınırları, alarm sonrası geniş menzil, hafıza yenileme/sona erme, görünmeden gelen hasar, geçersiz ayarlar.
2. Physics/PlayMode: duvar ve kat engeli, trigger/self/target filtreleri, koni dışı sahipli hasar, yanma/kanama sırasında gizli konumun sabitliği, pause, ölüm ve tekrar kullanım.
3. Chaser/Ranger/Charger entegrasyonu: ilk-frame sızıntısı yok, son bilinen nokta gerçekten sabit, görünmeyen hedefe yeni saldırı yok, başlamış atılış davranışı ve cooldown korunuyor.
4. Profil atanmamış eski sahneler ve prepared encounter regresyonları. Açılış sahne testi artık tetikleyiciye girmeyi doğrudan fark edilme ile eşitlemez; encounter etkinleşmesi ile düşmanın oyuncuyu fark etmesi ayrı doğrulanır.
5. Demo açılış grubuna profil ve uygun bakış yönleri bağlanır; Inspector/gizmo ile menzil, koni, durum ve son bilinen nokta gözlemlenebilir olur. Yeni oda düşmanları ayrı checkpoint'te bağlanır.

Test kodunu asistan yazar; kullanıcı çalıştırır. Test geçmeden başarı yazılmaz. Ses, devriye, grup alarmı, gelişmiş arama ve boss AI kapsam dışıdır.
