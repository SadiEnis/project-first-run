# DeepJam demo — kapsam ve sahne tasarımı

## Durum ve çalışma sırası — 04.10.2026

Git: `feature/deepjam-demo`. İlk açılış bölümü `12a2dd0` ile kaydedildi; kullanıcı sahnedeki çatışma/ödül akışını ve Windows build açılışını doğruladı. Tam standalone oynanış kabulü ve otomatik test sonuçları ayrıca bekleniyor. Başvuru hedefi 11 Ekim; 10 Ekim teslimi önerilen tampon hedeftir. Süre hedefi yaklaşık 8–12 dakikadır, kesin kabul şartı değildir.

Sıra: TDD ve docs checkpoint → erken oynanabilir greybox ve Windows build kontrolü → tüm rota/oyun döngüsü → sunum ve son dokunuşlar. İlk build için bitmiş harita, animasyon veya final beklenmez. Build ve Unity test çalıştırmaları kullanıcıya aittir; asistan uygulama/test kodunu ve talimatları hazırlar. Merge/check-in kullanıcıya aittir.

## İlk uygulama dilimi — başlangıç / koridor / ödül

- Unity menüsü: `Project First Run > Demo > Create Opening Greybox`. Mevcut `Test_ContentArena` sahnesinin bağlantıları ayrı bir dosyaya kopyalanır; kaynak sahne, prefablar ve test NavMesh'i değiştirilmez. Demo zaten varsa yalnızca açılır; kullanıcı düzenlemeleri yeniden üretimle ezilmez.
- Hedef: `Assets/_Project/Scenes/Demo/DeepJam_Opening.unity`. Geometri, dört spawn noktası, aktivasyon hacmi ve ödül noktası Edit Mode'da görünür/kayıtlıdır. NavMesh oluşturma sırasında bake edilir. Play sırasında harita üretilmez; yalnızca normal düşman/ödül örnekleri oluşturulur.
- Bu ilk ölçü/prova dilimi düz zemindedir: güvenli başlangıç → görüşü kesen giriş → siperli geniş koridor → ilk ödül odası. Aşağı iniş, bütün dungeon ve sinematik henüz yoktur.
- Dört Chaser önceden hazırlanır, koridora girilince mevcut encounter sistemiyle etkinleşir. Bu dilimde yeni görüş konisi/LOS takibi henüz uygulanmadı; mevcut düşman davranışı kullanılır.
- Grubun tamamı ölünce ödül odasının belirlenmiş noktasında bir Ability Chest kesin olarak oluşur. Normal düşman ve level-up sandıkları ayrıca mevcut olasılıklarla çalışır. Bu, ilk ödülü göstermek içindir; tüm ana rotaya kill-all kapısı eklenmez.
- F1 paneli yoktur. Geçici can/mermi/XP/hedef göstergesi ve nişangâh bulunur. Sandık E ile açılır. Ödül seçimi tamamlanınca yalnızca bu dilimin tamamlandığı gösterilir; demo finali/Victory değildir.
- Ölümde Enter veya Retry ile aynı sahne temiz yüklenir; geçici run kazanımları sıfırlanır. Henüz topraktan çıkış animasyonu yoktur. Yaşarken retry çağrısı yok sayılır.
- İki silah yuvalı başlangıç build'de de çalışır; Editor-only silah değiştirme bootstrap'ı demo kopyasından kaldırılır. Mevcut ability kurulum servisi bu dilimde korunur.
- Windows: `Project First Run > Demo > Build Windows Opening`. Yalnızca bu sahneyle `Builds/DeepJamOpening/ProjectFirstRun.exe` üretilir; mevcut Build Settings sahne listesi değişmez. Build alındıktan sonra exe elle açılarak kontrol edilir; çalışan oyundan Alt+F4 ile çıkılabilir, ölüm ekranında Quit da vardır.
- Testler: `DemoOpeningLayoutTests`, `DemoOpeningTests`. Sahne henüz üretilmemişse açık gerekçeyle atlanır; bu başarı sayılmaz. Başlangıç güvenliği, gerçek aktivasyon hacmi, tek garanti ödül ve ölüm iptali kapsanır. Otomatik test sonucu henüz bildirilmedi. Kullanıcı Windows build almayı ve açılışını doğruladı.
- Kullanıcı ilk kısa bölümde garanti ödül, level-up ve rastgele düşman kaynağından toplam üç sandık gözlemledi. Kaynaklar bağımsızdır; bu gözlem üzerine denge değiştirilmedi.
- İlk kabul: koridora kadar düşman takibi başlamaz; içeri girince dört düşman etkinleşir; öldürünce XP ve garanti sandık çalışır; E/seçim sonrası oynanış sürer; ölüm/yeniden deneme temiz başlar. Mermi kaynağı ve uzun run dengesi sonraki demo adımlarında ayrıca çözülmelidir.

## İkinci uygulama dilimi — yürünebilir harita greybox'ı

Durum: geometri mevcut `DeepJam_Opening.unity` dosyasına işlendi. Kullanıcı yeniden bake sonrası NavMesh testindeki sorunun çözüldüğünü ve oda yerleşimini şimdilik kabul ettiğini bildirdi. Odaların küçüklüğü ileride karşılaşmalarla değerlendirilecek; bu checkpoint'te ölçüler değiştirilmedi. Tüm otomatik testlerin geçtiği veya tam standalone oynanış kabulü yapıldığı iddia edilmez. Yeni harita veya test sahnesi oluşturulmadı. Başlangıç sahnesi üretme menüsü yeniden kullanılmaz. Mevcut başlangıç, koridor çatışması, oyuncu/servis bağlantıları ve sandık kuralları korunur.

### Yerleşim ve kot planı

1. **Koridor sonu → aşağı iniş:** koridorun ardından geniş, kenarları korunan bir merdiven gelir. Dar spiral yerine okunabilir düz veya sahanlıklı iniş kullanılır. İlk ölçü hedefi yaklaşık 3 m kot farkı ve en az 3 m net geçiş genişliğidir; oyuncu collider'ı ve gerçek yürüyüşle doğrulanır.
2. **İlk ödül odası:** mevcut odanın rolü ve garanti sandık korunur, oda merdiven sonundaki alt kota uyarlanır. Ödül noktası zeminle birlikte taşınır; sandık havada veya zeminin içinde kalmaz. Sırf yerleşim değişti diye ikinci garanti ödül eklenmez.
3. **İkinci oda:** kısa bağlantının ardından ileride Ranger/siper ilişkisini gösterecek alan gelir. En az iki yürünebilir yaklaşma hattı ve aralarında görüşü bölen siperler hazırlanır. Bu aşamada Ranger veya başka yeni karşılaşma eklenmez.
4. **Büyük arena üst girişi:** ikinci oda, arenanın yaklaşık 4 m üzerindeki sahanlığa açılır. Kamera oyuncuda kalır. Oyuncu buradan arena zeminini ve üç rota girişini okuyabilir. İlk ölçü hedefi yaklaşık 30 × 28 m arena tabanıdır; bu nihai denge ölçüsü değildir.
5. **İki yandan iniş:** sahanlığın solunda ve sağında arena tabanına kesintisiz bağlanan merdivenler bulunur. Bir yaklaşım daha korunaklı, diğeri daha açık olacak şekilde siper konur. Merdivenler çıkış yönünde de yürünebilir; bu aşamada tek yönlü kilit eklenmez.
6. **İleride bağlanacak rotalar:** opsiyonel oda için ayrı giriş/oda hacmi; zorunlu anahtar rotası için yukarı giden giriş; final odası için uzaktan ayırt edilen kapı konumu belirlenir. Anahtar ve final tarafındaki henüz yapılmamış yollar, açıkça geçici fiziksel engellerle sonlandırılır. Bunlar çalışan anahtar/kilit mekanizması gibi sunulmaz; ana rotanın güvenli dolaşımını kapatmaz.

### Uygulama sınırları

- Geometri Hierarchy'de bölüm adlarıyla gruplanır ve sahnede kayıtlı kalır; Play sırasında üretilmez. Mevcut nesneler topluca silinip yeniden kurulmaz; yalnız gerekli zemin, sınır duvarı, bağlantı ve ödül konumu düzenlenir.
- Merdiven ve kapı ölçüleri mevcut CharacterController ile yürünerek geçilmeli; ilerlemek için zıplama, sprint veya teleport gerekmemeli. Basamak yüksekliği mevcut step offset sınırına göre belirlenir. Duvar kenarından harita dışına düşülebilen açıklık bırakılmaz.
- Zeminler mevcut ground layer kuralına uyar; yeni kotlarda XP/sandık yerleşimi engellenmez. Ödül odası taşınırken mevcut koridor aktivasyon hacmi ve referansları kontrol edilir; yeni alanlara geniş bir tetikleyici kopyalanmaz.
- NavMesh değişen geometriye göre yeniden bake edilir. Eski koridor düşmanlarının navigasyonu korunur; yeni alanlara henüz yeni düşman spawn noktaları/karşılaşmalar bağlanmaz.
- Basit renk, kapı çerçevesi ve geçici etiketlerle anahtar rotası, opsiyonel oda ve final birbirinden ayrılır. Nihai materyal, model, ışık, HUD veya sinematik çalışması yapılmaz.
- Mevcut HUD'daki yalnız açılış bölümünün bittiğini belirten metin, yeni yürünebilir alan varken oyunun tamamlandığı izlenimi vermeyecek şekilde güncellenir.
- Algılama mesafesi/açısı/LOS, anahtar pususu, parkur, ayrı dönüş çıkışı ve final bu checkpoint'in uygulama kapsamı dışındadır. Önce yerleşim oynanarak kabul edilir; sonra mekanikler ayrı adımlarla bağlanır.

### Kabul ve sonraki adım

Uygulanan ilk ölçüler: koridor sonundan 15 adet 0,2 m basamakla 3 m aşağı iniş; 16 × 14 m ödül odası; 16 × 16 m ikinci oda; 30 × 4 m sahanlık; 30 × 28 m arena tabanı. Arenanın iki merdiveninde 20'şer adet 0,2 m basamak, 4 m net basamak genişliği ve 0,6 m derinlik kullanıldı. Mevcut oyuncu step offset değeri 0,3 m'dir. Solda 12 × 12 m opsiyonel oda, sağda kısa çıkış merdiveniyle geçici sonlandırılan anahtar rotası, karşıda geçici final engeli bulunur. Rota renkleri: yeşil opsiyonel, sarı anahtar, camgöbeği final. Bunlar henüz ödül/pusu/anahtar mekaniği içermez.

Sahneyi Unity'de yeniden açtıktan sonra `Project First Run > Demo > Rebake Demo Navigation` çalıştırılır. Bu menü yalnız sahnenin mevcut NavMesh asset'ini günceller ve sahneyi kaydeder; geometri oluşturmaz veya kullanıcı yerleşimini yeniden üretmez. Bake çıktısı olan `OpeningNavigation.asset` sonraki check-in'e dahil edilir. Bake yapılmadan eski NavMesh yeni alt katları temsil etmez; `DemoMapLayoutTests.BakedNavigationReachesNewElevationsAndOptionalRoom` bu eksikliği hata olarak bildirir.

Otomatik kabul kodu: `DemoMapLayoutTests` hiyerarşiyi, ödül yüksekliğini, basamakları ve bake sonrası rota erişimini denetler. `DemoOpeningTests.ExpandedMapCanBeWalkedDownAndBackUpWithoutJumpOrTeleport` yalnız ilk konumlandırmada teleport yapıp kalan ana rotayı ve iki arena merdivenini mevcut PlayerMotor ile yürür. Testler çalıştırılmadı; bu kayıt başarı raporu değildir. Kaydedilen sahnenin yerel fileID referansları ve benzersizliği metin düzeyinde kontrol edildi; bu Unity görsel/fizik doğrulamasının yerine geçmez.

- Başlangıçtan ilk ödüle mevcut akış bozulmaz; sandık sayısı/olasılıkları değişmez.
- Oyuncu aşağı inişi, iki ara odayı ve üst sahanlığı yürüyerek geçebilir; her iki merdivenden arena tabanına inip geri çıkabilir.
- Sahanlıktan üç rota konumu ayırt edilir; opsiyonel alan anahtar yolu sanılmaz. Siperler bütün arena tabanını tek risksiz atış hattına dönüştürmez; bu kontrol düşman algılama kabulü yerine geçmez.
- Her geçişte zemin sürekliliği, oyuncu baş mesafesi, sıkışma ve harita dışına kaçış kontrol edilir. Yeni bölüm sınırları görünürdür; boşluğa açılan tamamlanmamış çıkış yoktur.
- EditMode sahne testleri kayıtlı hiyerarşi/referanslar ve NavMesh bağlantılarını; PlayMode testleri ilk bölümün korunmasını ve önemli yürüyüş bağlantılarını kapsayacak şekilde genişletilir. Testleri kullanıcı çalıştırır; yalnız yazılmış testler geçmiş kabul edilmez.
- Bu kabulden sonra sıradaki mekanik adım düşman algılama ve yeni karşılaşmaların yerleşime bağlanmasıdır. Yerleşim kabul edilmeden anahtar/parkur/sinematik kapsamı büyütülmez.

### Çalışma kopyaları ve build çıktısı

Ana geliştirme masaüstündeki projede sürer; `C:\Project\ProjectFirstRun` yalnız build kopyasıdır. Build öncesinde aynı commit ve gerekli proje ayarları kullanıldığı kontrol edilir; kopyalar arasında bağımsız geliştirme yapılmaz. Masaüstü eşitlemesi öncesindeki yerel ayarlar stash yedeğinde korunur; bu harita aşamasında topluca geri uygulanmaz. `Builds` Git ve Plastic check-in dışında kalır.

## Üçüncü uygulama dilimi — oda karşılaşmaları (05.10.2026)

Algılama kabul edildi ve kullanıcı check-in/commit aldığını bildirdi. Kullanıcı aşağıdaki dağılımla uygulamaya geçilmesini onayladı. Dağılım mevcut kayıtlı sahneye işlendi; nihai denge değildir. Unity derlemesi, testler ve oynanış kabulü bekleniyor; asistan Unity testlerini çalıştırmadı.

Sahnedeki `Demo room encounters` kökü dört ayrı karşılaşma içerir; açılışla birlikte toplam beş karşılaşma ve 16 düşman vardır. `EW_DemoSecondRoom`, `EW_DemoArenaLeft`, `EW_DemoArenaRight`, `EW_DemoOptionalRoom` asset'leri yalnız demoya aittir. Hazırlık hacimleri aşağı inişin başında z=30'dan itibaren, etkinleştirme ikinci oda için z=39, arena ve opsiyonel oda için z=49'dan itibaren başlar; hacimler sonraki rotayı da kapsar. Bunlar kapı veya bölge sınırı değil, erkenden hazırlama/etkinleştirme hacimleridir. Görüş ve hasar kuralları algılama profilinde kalır. Başarısız hazırlık/ödül işlemi demo HUD'ında gösterilir.

Geometri ve NavMesh değiştirilmedi; sahneyi yeniden açmak yeterlidir, oluşturma menüsü tekrar çalıştırılmaz. Spawn erişimi `DemoMapLayoutTests` ile mevcut bake üzerinde kontrol edilir. `DemoOpeningTests` hazırlık/etkinleşme, yeniden giriş, bağımsız ve tek ödül, iptal ve ölüm akışlarıyla genişletildi; geometri yürüyüş testi yeni karşılaşmaları da iptal ederek çatışmadan ayrıştırıldı. Restart sonrası temiz başlangıç ve iki farklı yaklaşım rotası ayrıca elle doğrulanmalıdır.

| Alan | İlk deneme grubu | Oynanış amacı / geçiş |
| --- | --- | --- |
| Açılış koridoru | Mevcut 4 Chaser | Değişmez; mevcut garanti Ability Chest korunur. |
| İlk ödül odası | Yeni düşman yok | Ödülü seçme ve kısa nefes; geriden takip eden düşmanlara görünmez engel yok. |
| İkinci oda | 2 Chaser + 1 Ranger | Chaser baskısı altında siper ve Ranger'a yaklaşmayı öğretir; çıkış serbest. |
| Büyük arena | Toplam 3 Chaser + 2 Ranger + 1 Charger | İki küçük yerleşim grubu; farklı merdiven/yaklaşım açıları. Oyuncuyu aynı anda altı düşmanla karşılamaz, farkındalık bireyseldir. Çıkışlar kill-all koşuluna bağlanmaz. |
| Opsiyonel oda | 2 Chaser + 1 Charger | Yakın mesafe risk/ödül; giriş ve geri dönüş serbest. Yalnız bu grubu temizleyince bir garanti Green Chest. |

### Entegrasyon ve sınırlar

Oynanış geri bildirimi (05.10.2026): kullanıcı düşmanların uygun zamanda etkinleşmesini ve algılama mesafesi dışından oyuncu hasarıyla alarma geçip yaklaşmasını doğruladı. Balkondan tüm arena düşmanları öldürülebildi; bu, nihai yerleşim kabulü değildir. Boyutlandırma geçişinde arena büyütülecek; daha yüksek/hâkim sahanlık bir seçenek olarak değerlendirilecek. Yükseklik tek başına güvenli taramayı engellemez: sahanlık altındaki kör alanlar, tam boy siperler, düşman yerleşimi ve iki merdivenin yaklaşma hatları birlikte yeniden test edilmeli. Yapay görünmez ateş engeli veya hasar bağışıklığı eklenmeyecek. Kullanıcı sahne ölçülerini elle düzenlemeyi planlıyor; bu düzenlemeler yeniden üretimle ezilmeyecek. Geometri değişince NavMesh, spawn/ödül noktaları ve hazırlık/etkinleşme hacimleri yeniden kontrol edilecek. Bu geri bildirim yeni otomatik testlerin geçtiği anlamına gelmez; ölçüler henüz değiştirilmedi.

- Aynı kayıtlı `DeepJam_Opening` sahnesi; oda başına açık spawn noktaları, ayrı grup tanımları ve mevcut `PreparedRegionEncounter` kullanılır. Büyük arenadaki iki grubun toplamı tablodaki altı düşmandır; yeniden girişte tekrar üretilmez.
- Hazırlık, görüşe çıkıştan önceki bağlantıda başlatılır. Büyük arena grupları sahanlıktan atış yapılabilir hale gelmeden etkinleşmiş olmalıdır; görünür fakat vurulamayan hazırlık düşmanları bırakılmaz. Etkinleşmek alarm değildir: mevcut algılama profili takip ve saldırıyı yönetir.
- Hazırlık gecikirse görünür noktada aniden oluşturma veya ana rotaya yeni kill-all kilidi eklemek yerine hazırlık mesafesi/zamanlaması düzeltilir. İlk prova boyunca hazırlık hataları görünür biçimde raporlanır; başarısız grup temizlenmiş sayılıp ödül verilmez.
- Düşmanların bakış yönleri, Ranger atış hattı ve Charger atılış alanı mevcut geometriye göre yerleştirilir. Üst sahanlığa giriş toplu alarm oluşturmaz; açıkta duran oyuncu yine görülebilir. Oda dışına çıkan düşmanlar silinmez, canları yenilenmez ve kendi karşılaşmalarında sayılmaya devam eder.
- Opsiyonel ödül yalnız kendi karşılaşmasının tamamlanmasıyla, bir kez ve oda içindeki kayıtlı noktada oluşur. Başka odadaki canlı düşmanlar ödülü engellemez. `EncounterChestReward` kaldırılmış/açılmış sandığı yeniden üretmez; ölüm veya iptal ödül vermez. Mevcut düşman/level-up drop tabloları değişmez, diğer yeni odalara garanti sandık eklenmez.
- Anahtar rotası, parkur, anahtar pususu, kapı kilitleri ve final bu dilimin dışındadır. Yeni düşman türü, elite, mühimmat kaynağı veya genel denge değişikliği eklenmez. Mühimmat yetmezliği oynanışta kaydedilir ve sonraki ikmal kararında ele alınır.

### Kabul

EditMode: kayıtlı grup sayıları, prefab/tanım/profil/oyuncu/registry bağlantıları, spawn noktalarının NavMesh erişimi ve ödül referansları. PlayMode: hazırlık/etkinleşme ayrımı, yeniden girişte çoğalmama, bağımsız karşılaşma tamamlama, tek opsiyonel ödül, ölüm/restart temizliği ve açılış regresyonu. Testleri kullanıcı çalıştırır.

Elle prova: iki arena merdivenini ayrı dene; düşmanları kesmeden anahtar yolu girişine ulaş; opsiyonel odaya girmeden ilerle ve girip geri çık; Ranger siperini ve Charger kaçınma alanını değerlendir. Tüm harita veya anahtar/final kabulü bu checkpoint'e dahil değildir.

## Deneyim ve kapsam

Manuel FPS çatışması, otomatik yetenekler, sandıktan build oluşturma ve risk/ödül rotaları gösterilir. İlk denemede bitirmek mümkündür; zorunlu ölüm, kazanılamayan savaş veya gizli ilk-run engeli yoktur. Demo bitişi hikâyenin zaferle tamamlanması olmak zorunda değildir.

Kullanıcının dungeon çizimi harita temelidir. Önceki enerji tesisi önerisi kesin tema değildir. Cehennem/yüzey yolculuğunun genel yönü ve nihai sanat dili ayrıca kararlaştırılır; karakterin topraktan çıkışı bunları tek başına belirlemez.

Tek kayıtlı sahne önerilir; geometri ve bağlantılar Edit Mode'da hazırlanır, Inspector referansları açıkça bağlanır. Play sırasında tüm haritayı oluşturan test bootstrap'ı kullanılmaz. Mevcut test sahneleri korunur. Yeni silah/yetenek, Akıcı Mekanizma, altın/meta, gerçek evolution, prosedürel harita ve tam boss sistemi kapsam dışıdır.

## Rota ve odaların görevleri

| Alan | Amaç |
| --- | --- |
| Başlangıç | Topraktan çıkış, yön bulma ve kontrol devri |
| Koridor savaşı | İlk silah/Chaser tanıtımı; yana hareket alanı olan koridor |
| Aşağı iniş ve ilk oda | İlk ödül ve yeni gücü deneme |
| İkinci oda | Ranger/siper ilişkisi ve büyük arenaya hazırlık |
| Büyük arena | Mevcut düşman davranışlarını birleştirme, yan yol ve anahtar rotasını gösterme |
| Ayrı opsiyonel oda | İsteğe bağlı risk karşılığında ek ödül; anahtar burada değildir |
| Yukarı merdiven, giriş odası | Anahtar rotasının başlangıcı ve kısa çatışma |
| Düşmansız zıplama parkuru | Üç sabit platform ve güvenli varış; düşüşte hasarlı yerel geri dönüş |
| Küçük anahtar arenası | Başta boşluk/sessizlik ve yalnız parlayan anahtar; alımdan sonra pusu |
| Final odası | Anahtarla erişilen dev düşman gösterisi ve demo sonu; boss savaşı yok |

Anahtar rotası zorunludur; opsiyonel oda ile karıştırılmaz. Büyük arena ve ana rota için evrensel tüm düşmanları öldürme koşulu eklenmez. Anahtar pususu özel bir kilitli karşılaşmadır; mevcut serbest keşif yönü bu istisnayla korunur.

## Büyük arenaya hâkim noktadan giriş

Önceki oda oyuncuyu arenaya bakan üst sahanlığa çıkarır. Kamera kontrolü oyuncuda kalır; zorunlu panorama sinematiği yoktur. Final kapısı, anahtar rotasının merdiveni ve ayrı opsiyonel oda girişi ayırt edilebilir olmalıdır. Oyuncuya ortamı okumak için kısa bir fırsat verilir; giriş bütün düşmanları aynı anda alarma geçirmez.

Sahanlığın iki yanında aşağı inen merdivenler bulunur. Yalnızca dekoratif simetri yerine farklı yaklaşma açıları hedeflenir; bir iniş daha korunaklı/yakın mesafeli, diğeri daha açık bir alternatif olabilir. Kesin siper yerleşimi greybox'ta seçilir. Üst nokta tüm arenanın risksiz temizlendiği bir mevzi olmamalıdır: siper ve görüş hatlarıyla bazı düşmanlar gizlenir; görünmez ateş engeli eklenmez.

## Düşman algılama ve çatışmaya girme

Uygulama sözleşmesi, ilk deneme değerleri ve görüş kaybında hareket/saldırı sahipliği [Düşman Algılama](Enemy-Perception.md) dokümanında netleştirildi. Aşağıdaki genel ilkeler korunur; başlangıç unutma davranışı, son bilgi noktasına yönelip 3 saniye sonunda bulunduğu yerde beklemektir. Açılış grubunda kullanıcı 05.10.2026 tarihinde algılama oynanışını ve düzeltilen testlerin yeşil olduğunu doğruladı. Aynı profil yeni oda gruplarına bağlandı; yeni karşılaşmaların kabulü bekleniyor.

- Hazırlanma, oyuncuyu fark etme ve saldırı ayrı adımlardır. Düşmanlar önceden hazır olabilir; henüz fark etmedikleri oyuncuyu otomatik takip edip vurmazlar.
- İlk görsel algılama için üç koşul birlikte sağlanır: algılama mesafesi, düşmanın bakışına göre görüş açısı ve arada engel olmayan görüş hattı. Duvar/kat arkasındaki yakınlık tek başına yeterli değildir. Mesafe ve açı değerleri yapılandırılabilir; sayılar oynanış testinde seçilir.
- Fark etmek hemen hasar vermek değildir. Mevcut saldırı menzili, görüş, saldırı hazırlığı ve cooldown kuralları korunur.
- Oyuncunun doğrudan veya ona ait yetenek/zamanlı hasarıyla vurulan, hayatta kalan düşman; başlangıç algılama konisi/mesafesi dışında da tepki verir. Kaynağı oyuncuya ait olmayan hasar otomatik oyuncu tespiti sayılmaz. Bu kural saldırının görüş/menzil kurallarını kaldırmaz.
- Algılama sınırından çıkmak anında pasifleşme yaratmaz. Kısa takip hafızası ve son görülen konum kullanımı uygulama öncesinde mevcut takiple netleştirilir; duvar arkasında sonsuz kesin konum bilgisi verilmez. Süre, takip sınırı ve unutunca bekleme konumu açık ayar kararlarıdır.
- Anahtar pususu karşılaşma tarafından doğrudan alarma geçirilebilir; bu yine duvar içinden saldırma izni vermez. Alarm tüm haritaya yayılmaz.
- Ses algısı, devriye, gelişmiş arama ve grup alarm sistemi demo kapsamına eklenmez. Mevcut test sahnelerinin başlangıç davranışı sessizce değişmemeli; yeni algılama politikasının kapsamı entegrasyonda açıkça seçilir.

Doğrulama: mesafe/açı sınırları, arkada/duvar arkasında oyuncu, doğrudan ve sahipli zamanlı hasarla alarm, kısa görüş kaybı, pusu alarmı, ölüm/disable temizliği ve saldırı koşullarının korunması. Sahanlık güvenliği ayrıca gerçek geometriyle elle değerlendirilir; yalnız algılama testleri level design kabulü değildir.

## Parkur ve yerel geri dönüş

İlk sürüm düşmansız üç sabit platform ve güvenli varıştan oluşur. İlk atlayış öğretici/geniş, diğerleri biraz daha dikkat isteyen fakat sprint gerektirmeyen mesafelerde olur. Mevcut PlayerMotor yalnız hareket ve yerçekimi içerir; önce zıplama eklenir, sonra mesafeler gerçek oyuncu kontrolüyle greybox'ta ölçülür. Yeni bulmaca sistemi veya dar spiral merdiven şartı yoktur.

Düşüş algılandığında bir kez hasar uygulanır; başlangıç denge değeri o andaki maksimum canın %10'u kadar temel hasardır, oynanışta ayarlanabilir. Hasar mevcut HealthComponent akışından geçer; gelen hasar artışı/azaltması ve mevcut hayatta kalma kuralları korunur. Doğrudan can çıkarılmaz, oyuncuya ait saldırı veya öldürme sayılmaz. Hayatta kalan oyuncu kısa kararmayla parkurun güvenli başlangıcına taşınır; düşüş hızı temizlenir. Aynı düşüşün birden fazla collider/olayla tekrar hasar vermesi önlenir. Ölümcül hasarda normal run ölümü işler; yerel geri dönüş oyuncuyu diriltmez.

Bu bir run checkpoint'i değildir: hayatta kalınan parkur hatası XP, ekipman, anahtar veya karşılaşmaları sıfırlamaz; başka yerde ölmek parkurdan başlatmaz. Alt zemin ateş/lav olarak sunulabilir ancak ilk sürümde orada yürüyüş, süreli yanma hasarı veya geri tırmanma döngüsü yoktur. Yıkılan üçüncü platform ancak sabit parkur kabulünden sonra değerlendirilecek isteğe bağlı iştir; ilk sürüme dahil değildir.

Anahtar odasından parkuru ters yönde dönmek gerekmeyecek. Pusu tamamlanınca ayrı bir çıkış kapısı, merdiven/rampa ile aşağıdaki büyük arenaya ve final kapısına giden kısa yolu açar. Bu yol baştan yukarı çıkılarak parkurun/anahtarın atlanmasına izin vermemeli; başlangıçta kapalı olması ve final uygunluk kontrolü korunmalıdır.

### Dördüncü uygulama dilimi: zıplama ve sabit parkur

Uygulama durumu: zıplama kullanıcı tarafından oynanışta kabul edildi ve kaydedildi. Üç sabit platform, varış alanı ve ParkourRecovery kayıtlı demo sahnesine eklendi; bu yeni bölümün Unity testleri ve oynanış kabulü bekleniyor. Aşağıdaki maddeler bütün dilimin kabul sözleşmesidir.

Kontrol çakışması giderildi: gamepad güney tuşu zıplamaya ayrıldı; mevcut sandık etkileşimi boş olan sağ omuz tuşuna taşındı. Klavyede etkileşim E olarak kalır. Sandık etkileşim testi yeni eşlemeyi kullanır.

- **Giriş ve motor:** Input Actions üzerinden Space / gamepad güney tuşu kullanılır; üretilen C# sarmalayıcı asset ile birlikte güncellenir. Yerdeyken yeni basış tek zıplama başlatır; basılı tutmak otomatik tekrar üretmez. Çift zıplama, dash ve yeni hareket sistemi yoktur. Zıplama yüksekliği Inspector ayarıdır; ilk deneme 1,2 m, hız mevcut yerçekiminden hesaplanır. Havada mevcut yatay hareket korunur; tavana çarpınca yukarı hız kesilir.
- **Kontrol sahipliği:** Ödül ekranı, pause, ölüm veya kontrol kilidi sırasında zıplama alınmaz; bekleyen basış kilit açılınca zıplatmaz. Yerel geri dönüş kendi hareket/ateş kilidini kaldırırken başka sistemin kilidini açamaz. Otomatik yetenekler ve diğer karşılaşmalar sıfırlanmaz; yeni dokunulmazlık kuralı eklenmez.
- **Düşüş işlemi:** Yalnız parkurun altındaki açıkça atanmış hacim yerel geri dönüş başlatır; tüm haritanın düşüş kuralını değiştirmez. Bir işlem sürerken ek collider girişleri yeni hasar/taşıma başlatmaz. Güvenli dönüş tamamlanıp oyuncu tehlike hacminden çıktıktan sonra sonraki düşüş yeniden işlenebilir. Kararma sırasında ölüm gerçekleşirse ölüm önceliklidir; geri dönüş kontrolü açmaz veya can vermez.
- **Güvenli konum:** Sahneye kaydedilmiş dönüş noktası zemin ve baş boşluğu olan, tehlike hacmi dışındaki parkur başlangıcıdır. CharacterController güvenli taşınır, düşey hız temizlenir; kök yönü ve PlayerLook bakışı başlangıç yönüne eşitlenir, kalan kamera sekmesi temizlenir. Geçersiz referans/yerleşim sessizce dünya merkezine taşımaz; doğrulama hatası olarak görünür.
- **Sahne kapsamı:** Mevcut kayıtlı demo sahnesinde anahtar rotasına üç sabit platform, varış, düşüş hacmi ve dönüş noktası eklenir. Sahne yeniden üretilmez; kullanıcının geometri düzenlemeleri korunur. Atlayışlar yükseltmesiz yürüyüş hızıyla geçilebilir olmalıdır. Düşman navigasyonu boşluklar üzerinden otomatik yürüme yolu oluşturmamalıdır. Geometri sonrası NavMesh yeniden bake edilir ve mevcut oda yolları kontrol edilir.
- **Sonraki dilim:** Anahtar alımı, pusu, kapılar ve aşağı kısa yol ayrı uygulamadır. Bu dilimin varışı tamamlanmış demo gibi sunulmaz; geçici test sonu açıkça belirtilir ve oyuncu geri yürüyerek/zıplayarak ayrılabilir. Pusu sonrası ters parkur gerektirmeyen nihai çıkış sözleşmesi değişmez.

Doğrulama için testler yazılır, kullanıcı çalıştırır: yerde tek basış, havada/tuş basılıyken tekrar etmeme, kontrol kilidinde basış temizliği, tavana çarpma; aynı düşüşte tek hasar, maksimum can ve gelen-hasar değiştiricileri, güvenli poz/hız/bakış, ölümcül düşüş ve kararma sırasında ölüm, tekrar düşebilme, run ilerlemesinin korunması. Elle üç atlayışın yürüyüşle geçilmesi, her boşluktan güvenli dönüş, ödül/pause çakışması ve mevcut odalara regresyon kontrol edilir. Bu checkpoint Unity testleri veya Windows build çalıştırmaz.

### Parkur entegrasyonu ve deneme rotası

- Büyük arenanın sağındaki anahtar merdivenini çık. Başlangıç sahanlığı (22,5; -5; 97), platform merkezleri X=26,5 / 30,5 / 34,5; güvenli varış X=39,25'tir. Üç platforma çıkış ve son varış dahil dört boşluk atlanır. İlk boşluk 1,25 m, diğerleri 1,5 m; tüm üst yüzeyler Y=-5'tedir. Sprint gerekmez; platform kenarına yaklaşarak zıpla.
- Sahne kökü `Demo parkour` altında geometri, düşüş hacmi ve dönüş noktası Inspector'dan düzenlenebilir. Geçici son bariyer X=24'ten X=41,5'e taşındı. Varışta anahtar odasının henüz hazır olmadığı belirtilir; şimdilik geri atlayarak çıkılır.
- Parkur geometrisi düşman NavMeshSurface'inin çocukları dışında tutulur; zıplama boşlukları düşman yürüyüş rotası değildir. Mevcut anahtar girişindeki bariyer taşındığı için `Project First Run → Demo → Rebake Demo Navigation` çalıştırılıp sahne kaydedilir. Bu işlem asistan tarafından çalıştırılmadı; eski oda rotalarının bake testi korunur.
- Düşüşte HealthComponent üzerinden tek hasar, 0,15 saniye kararma, güvenli konuma taşıma ve 0,15 saniye açılma uygulanır. Motorun düşey hızı, bakış eğimi ve kamera sekmesi temizlenir; oyuncu platformlara döndürülür. Pause süreyi durdurur; ölüm işlemi iptal eder. XP, şarjör, yetenekler ve karşılaşmalar sıfırlanmaz.
- Geçici giriş engelleri sahip bazlıdır; PlayerInputReader istenen kontrol durumu ile geçici engelleri ayrı tutar. Motor da yalnız geri dönüş sahibinin askıya alma kaydını kaldırır. Ödül/ölüm kilitleri üzerine yazılmaz; geliştirme paneli ve sahne geçişi, girişin geçici engellenmiş halini kalıcı tercih gibi kaydetmez.
- Dönüş noktası tehlike dışında, oyuncu kapsülü için boş ve zemine en fazla 0,35 m uzak olmalıdır. Başlangıçta ve taşıma öncesinde doğrulanır. Hatalı referans/yerleşim ekranda yapılandırma hatası gösterir; dünya merkezine taşıma veya yeniden hasar döngüsü yaratmaz. Oyuncu ölçeği bu ilk sürümde (1,1,1) olmalıdır.
- Yeni doğrulama: `DemoMapLayoutTests.ParkourHasThreeFixedPlatformsAndExplicitRecoveryReferences` ve `DemoOpeningTests` içindeki yedi `Parkour...` testi. Bunlar dört atlayışın yürüyüşle geçilebilirliğini, hasarı, yeniden düşüşü, durum korumasını, ölüm/pause/kontrol kilitlerini ve hatalı dönüş noktasını kapsar. Testler yazıldı; çalıştırılmış/geçmiş sayılmaz.

## Anahtar ve pusu sözleşmesi

1. Final kapısı baştan kilitlidir; anahtar gerektiğini ve üst rotayı oyuncuya anlaşılır biçimde gösterir.
2. Küçük arena ilk gelişte görünür düşman içermez. Anahtar kapı eşiğinden uzakta, oyuncu odaya tamamen girmişken alınabilecek konumdadır.
3. Anahtarı alma tek seferliktir. Anahtar sahipliği run'a kaydedilir, oyuncunun geldiği giriş kapısı güvenli biçimde kapanır ve pusu bir kez başlar. Ayrı aşağı iniş kapısı karşılaşma tamamlanana kadar kapalıdır.
4. Düşmanlar görüş dışında girişlerden veya okunaklı belirme sunumuyla gelir; oyuncunun üstünde sessizce oluşmaz. Geometrik yerleşim sonradan yapılır.
5. İlk kurulum iki gruptur: önce 3 Chaser, ardından 2 Chaser + 1 Ranger. İkinci grup birinci tamamen temizlendikten sonra başlar. Bu sayılar başlangıç dengesi olup oynanış kabulünde değişebilir; yeni düşman tipi eklenmez.
6. Karşılaşmanın tamamlanması: planlı bütün gruplar oluşturulmuş ve o karşılaşmaya ait yaşayan düşman kalmamış olmalıdır. Gruplar arasındaki anlık sıfır düşman sayısı kapıyı açmaz. Başka odadaki düşmanlar bu kapıyı tutmaz.
7. Tamamlanınca geldiğimiz giriş kapısı yerine ayrı aşağı iniş kapısı açılır; giriş kapısı kapalı kalır. Oyuncu parkuru geri oynamadan büyük arenaya iner ve anahtarla final kapısına ilerler. Yeniden giriş anahtarı veya pusuyu yeniden üretmez; anahtar için genel envanter sistemi gerekmez.

Parkur/kapı testleri: düşüşte tek hasar, güvenli konum/hız sıfırlama, ölümcül düşüşte diriltmeme, run ilerlemesini koruma, anahtar alımında girişin kapanması, pusu sırasında iki kapının kilitli olması, son grup tamamlanınca yalnız ayrı çıkışın açılması ve restart'ta başlangıç kapı düzeninin geri kurulması.
8. Final sekansına yalnızca anahtar sahipliği ve pusu tamamlanması ile girilir; böylece kapı/trigger atlatmak karşılaşmayı atlamaz. Bariyer oyuncuyu sıkıştırmaz. Kaçan/erişilemeyen düşman veya spawn hatası karşılaşmayı sonsuza kadar kilitlememeli; uygulama aşamasında mevcut hata/cleanup sözleşmesine uygun kurtarma yolu tasarlanır.
9. Normal ölüm/restart anahtar, pusu, kapılar, düşmanlar, drop'lar, XP ve run build'ini sıfırlar. Kalıcı meta kazanımı yoktur.

### Beşinci uygulama dilimi: anahtar odası ve iki gruplu pusu

Uygulama durumu: KeyAmbushSession ve DemoKeyAmbushController, kayıtlı DeepJam_Opening sahnesindeki anahtar odasına bağlandı. Oda, görüş kesen doğma bölmesi, iki bağımsız grup asset'i, giriş/çıkış kapıları ve ayrı aşağı dönüş yolu eklendi. NavMesh yeniden bake edildikten sonra kullanıcı Unity testlerinin başarılı olduğunu ve iki grup ile dönüş çıkışının oynanışta çalıştığını bildirdi. Önceki parkur diliminde kullanıcı zıplamayı, %10 temel düşüş hasarını ve güvenli geri dönüşü de doğruladı.

Navigasyon koruması eklemesi: iki grubun her biri başlamadan, oyuncuya uzaklıktan bağımsız olarak bütün hazırlanmış düşmanların etkin ve NavMesh'e bağlı NavMeshAgent sahibi olması gerekir. Eksik navigasyon, yeniden bake/kayıt yönlendirmesiyle karşılaşmayı hataya geçirir; iki grup temizlenir, çıkış kilitli kalır ve tamamlanma verilmez. Böylece oyuncu konumuna bağlı olarak hareketsiz düşmanlarla ilerleme engellenir. İki yeni PlayMode regresyonu, oyuncu doğma noktalarından uzaktayken sahne navigasyonunun kaldırılmasını ve ikinci gruptaki kapalı agent'ı kapsar. Bu yeni testlerin kullanıcı tarafından çalıştırılması bekleniyor; önceki kabul bu eklemeyi kapsamaz. Kontrol agent bağlantısını doğrular, bütün yolların erişilebilirliğini değil; oda içi bake edilmiş yollar sahne yerleşim testleriyle ayrıca kapsanır.

Altyapı testleri: KeyAmbushSessionTests güvenli/tekrarlanan alımı, sıralamayı, ara süresini, hatayı, ölümü ve yeni run durumunu kapsar. PlayerJumpTests'e iki ortak dünya etkileşimi tüketim testi eklendi; anahtar ve sandık aynı basışı iki kez kullanamaz. DemoMapLayoutTests'e anahtar bağlantıları/grup bileşimleri ve bake edilmiş oda yolları testi; DemoOpeningTests'e sıralı iki grup/kapılar, alım engelleri, ölüm, karşılaşma hatası ve yürünebilir dönüş yolu için beş test eklendi. Bunlar yazıldı, Unity'de çalıştırılmadı.

Sahne denemesi: güncel sahneyi yeniden aç, `Project First Run → Demo → Rebake Demo Navigation` çalıştır ve kaydet; sahneyi yeniden üretme. Parkurun sonundan anahtar odasına gir, ortadaki sarı/ışıklı nesneye bakıp E / RB-R1 ile al. İlk 3 Chaser ve ardından 2 Chaser + 1 Ranger temizlenince odanın kuzeyindeki sarı çıkış açılır. Merdivenden aşağı in, dış koridordan büyük arenanın sağ arka bölümüne dön. Eski parkur giriş kapısı kapalı kalır. Final uygunluğu hazır bir koşul olarak sunulur; dev/final sekansı ve son kapının açılması henüz bu sahnede uygulanmaz.

Yerleşim: oda X=41,5–61,5 / Z=87–107 ve Y=-5; anahtar (49; -3,6; 97). Dönüş merdiveni (49; 107–113) boyunca Y=-7'ye iner; koridor (49;115) → (19;115) → (19;105) → büyük arena rotasını izler. Yalnız bu birleşim için arenanın sağ arka duvarında açıklık oluşturuldu; arena boyutları değişmedi. Kapılar bake sırasında kapalıdır: düşmanların parkura veya kilitli çıkışa yürüyen rotası yoktur; oyuncu çıkış açıldıktan sonra fiziksel merdiveni kullanır.

- **Sahne:** Parkurun varışındaki geçici bariyerin devamına anahtar odası eklenir. Mevcut sahne düzeni korunur; yeniden sahne üretimi yapılmaz. Oda girişi, anahtar, iki grubun doğma noktaları ve ayrı aşağı çıkış Inspector referanslarıyla kurulur. Bu dilim yeni bir parkur öncesi savaş veya bulmaca eklemez.
- **Alım:** Parlayan geçici anahtar modeli ve kısa etkileşim yazısı kullanılır. Oyuncu anahtara bakıp E / gamepad RB-R1 ile, başlangıçta 2 m menzil içinde alır. Duvar arkasından veya kapı eşiğinden alınamaz; oyuncu tamamen odada ve giriş kapanma hacmi boş olmalıdır. Pause, ödül ekranı, ölüm ve geçici giriş kilitleri alımı engeller. Anahtar bir kez alınır; alım girdisi aynı karede başka bir dünya etkileşimini de gerçekleştirmez.
- **Sahiplik ve kapılar:** Demo akış denetleyicisi anahtar sahipliğini ve pusu aşamasını tutar; genel envanter veya kayıt sistemi kurulmaz. Giriş başlangıçta açık, aşağı çıkış kapalıdır. Güvenli alımda anahtar görseli kapanır, giriş kapanır ve pusu başlar. Kapı collider'ı oyuncunun içinde etkinleştirilmez; giriş kapanamıyorsa anahtar tüketilmez/pusu başlamaz.
- **Hazırlık ve görünürlük:** Mevcut PreparedRegionEncounter ve düşman takip altyapısı kullanılır; iki ayrı grup bir demo pusu denetleyicisiyle sırayla yönetilir. Hazırlanan düşmanların render edilebildiği dikkate alınarak doğma cepleri görüş dışında tasarlanır. Oyuncunun yanında sessizce belirme yoktur. Hazırlık, grup aktivasyonu ve oyuncuyu fark etme ayrıdır; pusu düşmanlarına bir kez oyuncunun o anki konumuyla alarm verilir, sonrasında mevcut görüş/hafıza/saldırı kuralları işler.
- **Sıralama:** Birinci grubun Victory'sinden sonra kısa 1 saniyelik, oyun zamanı kullanan ara verilir. İkinci grup hazır olmadan etkinleştirilmez; hazırlık beklenirken iki kapı da kapalıdır. Birinci grubun bitmesi, gruplar arasındaki sıfır düşman sayısı veya başka odaların temizlenmesi pusu tamamlandı sayılmaz. Ölüm/iptal/hata Victory değildir. Tekrarlanan olaylar ikinci grubu tekrar başlatamaz.
- **Çıkış ve final:** İkinci grup da temizlenince yalnız aşağı çıkış açılır; giriş kapalı kalır. Merdiven/rampa büyük arenaya döner, oyuncuyu ışınlamaz ve baştan aşağıdan yukarı çıkılarak anahtar rotasının atlanmasına izin vermez. Final uygunluğu `anahtar alındı VE pusu tamamlandı` koşuludur. Dev/final sinematiği sonraki dilimdedir; bu adımda demo bitmiş gibi gösterilmez.
- **Kaynaklar:** Mevcut düşman XP ve sandık düşürme kuralları korunur. Anahtar için yeni rastgele ödül, ilave garanti sandık veya altın eklenmez. Ödül modalı aradaki süreyi de durdurur. Anahtar/pusu ve kapılar run'a aittir; normal ölüm ve yeniden başlatmada başa döner.
- **Hatalar:** Spawn/aktivasyon hatası veya kaybolan yaşayan düşman sessiz başarıya çevrilmez. Oda dışına kaçmış düşman için güvenli, bake edilmiş oda içi noktaya geri yerleştirme denenir; bu mümkün değilse karşılaşma hatası gösterilir. Hata durumunda final uygunluğu verilmez, karşılaşma temizlenir ve kullanıcıya açık bir run'ı yeniden başlatma seçeneği sunulur. Sırf uzun sürdü diye düşman öldüren veya kapıyı açan zaman aşımı yoktur.

Test kapsamı: alım menzili/görüşü ve kapı boşluğu; tekrar alım/olay; önce ilk grup sonra ikinci grup; ara sırasında kapalı çıkış; başka odadaki düşmanların bağımsızlığı; pusu alarmının duvar arkasından saldırıya dönüşmemesi; anahtar ve pusu birlikte olmadan final uygunluğu verilmemesi; pause, ölüm, hata ve yeniden başlatmada temizlik. Sahne testleri kapı, grup, anahtar ve yol referanslarını; oynanış denemesi doğma sunumunu ve aşağı çıkışın atlanamamasını kontrol eder. Test kodları uygulamada yazılır, Unity çalıştırmasını kullanıcı yapar. Geometri sonrası NavMesh yeniden bake edilir.

Navigasyon regresyonu takibi (08.10.2026): eksik navigasyon testinin hatası ayrı proje kopyasında yeniden üretildi. Hazırlanmış pasif düşmanlarda surface verisi kaldırıldıktan sonra `isOnNavMesh` bilgisi korunabildiği için bu bayrak tek başına eksik navigasyonu doğrulamıyordu. Runtime kontrolü artık düşmanın agent türü ve alan maskesiyle doğma konumundaki güncel NavMesh verisini de sorguluyor. Additive sahne testi kayıtlı surface verilerini geçici kaldırıyor, veri yokluğunu doğrudan doğruluyor ve `finally` içinde geri ekliyor. Yalnızca `KeyAmbushMissingNavigationFailsEvenWhenPlayerIsFarFromSpawns` PlayMode testi yeniden çalıştırıldı: **1/1 başarılı**. Bu takipte bütün paket veya ikinci regresyon çalıştırılmadı. Kayıtlı sahne geometrisi ve bake verisi değiştirilmedi.

## Kuşaksız açılış ve FPS kontrol devri

İstenen sunum: kamera karakterin topraktan çıkışını dışarıdan gösterir, sonra gerçek FPS göz konumuna yumuşak geçer. Alt/üst siyah kuşak hiçbir açılışta kullanılmaz. HUD kamera hazır olunca görünür, ardından kontrol devredilir. Açılışta hareket, ateş, otomatik yetenekler ve oyuncuya hasar kapalıdır.

Basit model yeterlidir. Mevcut FPS hareket sistemi değiştirilmez; dışarıdan görünen gövde yalnızca sunum olabilir. Modelin varlığı yerden çıkış animasyonunun hazır olduğu anlamına gelmez. Önerilen kısa deneme: bir görsel gövde, basit yükselme/doğrulma hareketi, toprak/toz ve ses. Gerçek terrain deformasyonu gerekmez.

04.10 incelemesi: manifest'te Timeline mevcut, Cinemachine bağımlılığı yok; Assets dosya taramasında Robot/StarterAssets veya FBX bulunmadı. Model/paket seçimi ve lisans-kaynak kaydı uygulamadan önce yapılır; bu checkpoint bunları indirmez. Cinemachine sürüm uyumu kontrol edilmeden entegrasyon sözü verilmez.

Kamera devrinde konum, yön ve FOV eşleşir; motorun yaw/pitch durumu da eşitlenir. İki kontrol sahibi veya iki AudioListener aynı anda çalışmaz. FPS görüşüne giren kafa/gövde mesh'i kırpılma yapmamalı; görsel gizleme ayrı tutulmalı, oyuncu kökü kapatılmamalıdır. Atlanan sinematik de aynı son kontrol/HUD durumuna ulaşmalıdır.

Dış kamera/model denemesi takvimi zorlarsa kullanıcı tarafından kabul edilen alternatif FPS açılış ve FPS finaldir. Önerilen deneme bütçesi yarım gün; kesin animasyon/model seçimi açık. İlk açılış yaklaşık 5–7 saniye hedeflenir ve atlanabilir olmalıdır. Ölümden sonraki dönüş 1–2 saniyelik kısa varyanttır; her seferinde uzun sinematik yoktur. Açılış-görüldü bilgisi sahne yeniden yüklenirken oturum boyunca korunur; bu bilgi run ilerlemesi veya kalıcı kayıt değildir.

## Final sekansı

Oyuncu odaya girince devin silueti/sesi/hareketi görülür. Finale geçiş tek seferliktir; oyuncu kontrolü, otomatik saldırılar ve hasar işleme durur. Dev yere vurur; ses, kısa kamera tepkisi, toz ve kontrollü yıkıntı sunumu gelir. Görüş kapanır veya kısa düşüş hissiyle kararır; demo tamamlandı ekranı, tekrar oyna ve çıkış sunulur.

Gerçek fizik tabanlı oda yıkımı veya dev için savaş AI'ı gerekmez. FPS sunum güvenli temel seçenektir; dışarı çekilen kamera ancak basit model ve sekansla kolayca yapılabiliyorsa eklenir. Bu son, normal Defeat/restart tetiklemez. Aynı karede ölüm/final tetiklenmesi ve tekrar trigger durumları için tek otorite gerekir; başlamış terminal sonuç diğer olayla değiştirilmez.

### Altıncı uygulama dilimi: final kapısı ve demo sonu

Durum: ilk alt dilim uygulandı; Unity testleri ve kullanıcı oynanış kabulü bekleniyor. Anahtar pususu ve ayrı dönüş yolu önceden kabul edildi. DemoFinalSession ve DemoFinalController eklendi; mevcut final bariyeri koşullu kapıya dönüştürüldü. Kapının arkasında küçük, kayıtlı bir giriş alanı ve yalnız oyuncu kapsülünü kontrol eden hacim var. Aynı demo branch'inde mevcut yerleşim korunuyor.

Ara sürüm sınırı: henüz dev, yıkıntı/kararma veya tamamlandı ekranı yoktur. Final başlangıcında oyun zamanı dondurulur; sonraki sunum ölçeklenmemiş zamanla işleyecektir. Sahip bazlı input/motor/silah/yetenek/hasar kilitleri alınır ve normal HUD gizlenir. Geçici “Final entry reached” ekranı Restart preview / Quit sunar; bu durum Completed değil Ending'dir. Restart preview yeni sahne yükler; kalıcı kayıt/checkpoint eklenmez. Final kontrol bileşeni kapanırken yalnız kendi kilitlerini bırakır. Sonraki dilim bu geçici ekranı gerçek bitiş sekansıyla değiştirir.

Yeni kapsam: DemoFinalSessionTests içinde 5 EditMode vaka; DemoMapLayoutTests içinde kapı, hacim, zemin ve referans kontrolü; DemoOpeningTests içinde 3 PlayMode test (kapı koşulu ve mücadele kilidi, pause/input/ölüm engelleri, diğer sahiplerin kilitlerinin korunması). Otomatik testler bu dilimde çalıştırılmadı; yalnız statik diff ve yeni sahne fileID referansları kontrol edildi. Sonraki görsel dilimde tamamlanma/restart entegrasyon kapsamı genişletilecek.

Oynanış denemesi: güncel sahneyi yeniden aç; geometri eklendiği için Demo → Rebake Demo Navigation çalıştır ve kaydet, sahneyi yeniden üretme. Anahtarsız final kapısı kapalı kalmalı. Anahtarı alıp iki grubu temizle, ayrı yoldan arenaya dön: final kapısı açık olmalı. Kapının ardına tamamen girince hareket/ateş/otomatik yetenekler durmalı, HUD yerine geçici final başlangıç ekranı görünmeli. Restart preview temiz bir run açmalı. Büyük arenada kalan düşmanlar kapıyı engellememeli.

- **Kapı koşulu:** Mevcut geçici final bariyeri, yaşayan oyuncu için `anahtar alındı VE iki pusu grubu tamamlandı` olduğunda açılır. Büyük arenadaki veya opsiyonel odadaki bütün düşmanların ölmesi gerekmez. Anahtar harcanmaz. Kapının açılması kendi başına finali başlatmaz.
- **Başlangıç:** Kapının ardındaki oda içi hacme oyuncu girince koşullar yeniden kontrol edilir ve final yalnız bir kez başlar. Düşman/mermi collider'ları, tekrar giriş, ölü oyuncu, açık ödül ekranı veya pause finali başlatamaz. Oyuncu kapı eşiğinde sıkıştırılmaz; ilk sürümde arkasındaki kapıyı tekrar kapatmak gerekmez.
- **Sonuç sahipliği:** Akış `Playing → Ending → Completed` veya `Playing → Dead` olur. Başlamış terminal akış diğerine çevrilmez. Final kabulü anında oyuncu ölmüşse ölüm önceliklidir; Ending başladıktan sonra yeni hasar oyuncuyu öldüremez. DemoOpeningController'ın ölüm/retry sunumu final ekranıyla yarışmamalıdır.
- **Kontrol ve mücadele:** Ending başladığında hareket, bakış, ateş, yeniden doldurma, etkileşim ve otomatik yetenekler durur; mevcut mermi ve süreli etkiler oyuncuya hasar veremez. Yalnız input kapatılması yeterli kabul edilmez. Oyun HUD'ı gizlenir. Kontrol kilitleri mevcut sahiplik modeline uyar; başka bir sistemin kilidini yanlışlıkla kaldırmaz. Eksik zorunlu referans başlangıçta anlaşılır hata verir, oyuncu yarım sekans içinde kilitlenmez.
- **İlk sunum:** FPS kamerası temel alınır; yeni model/paket indirilmez ve Cinemachine zorunlu tutulmaz. Sahneye düzenlenebilir basit dev silueti ve sunum noktaları yerleştirilir. Kısa görünüş/hareket, yere vuruş, sınırlı kamera tepkisi ve toz/yıkıntı hissinden sonra ekran kararır. Gerçek yıkım fiziği, boss AI'ı, boss can barı veya boss ödülü yoktur. Sunum süreleri Inspector'dan ayarlanır; alt/üst sinema kuşağı eklenmez.
- **Bitiş ve tekrar:** Kararma sonunda demo tamamlandı yazısı, Tekrar Oyna ve Çıkış sunulur; imleç serbesttir. Tekrar Oyna aynı sahneyi yeni run olarak bir kez yükler; can, XP, envanter, anahtar, gruplar ve kapılar başlangıç durumuna döner. Çıkış player build'de uygulamayı kapatır. Yeniden yüklemede zaman ölçeği ve geçici kilitler temiz başlangıcı engellemez.
- **Kapsam sınırı:** Açılış sinematiği, arena boyutları, balkon ateş avantajı, mühimmat ikmali ve ödül dengesi bu dilimde değiştirilmez. Kullanıcının elle yaptığı yerleşim korunur; bütün sahne yeniden üretilmez. Geometri değişirse navigasyon yeniden bake edilir.

Uygulama sırası: (1) kapı koşulu, tek seferlik final başlangıcı ve ölüm/final ayrımı; (2) basit FPS bitiş sunumu, tamamlanma ekranı ve yeniden oynama. Bunlar anlamlı ara kayıt noktalarıdır; her küçük değişiklik ayrı commit değildir.

Test sözleşmesi: anahtarsız ve yalnız anahtarla kapının kapalı kalması; iki grup sonrası açılması; başka odalardaki yaşayan düşmanların engel olmaması; yalnız oyuncuyla ve bir kez tetikleme; pause/ödül/ölüm engelleri; başlangıçtan sonraki hasar ve otomatik saldırıların bastırılması; terminal durum önceliği; tamamlanma ekranı ve tekrarlı restart isteğinin tek yükleme yapması. Testler yazılır, olağan Unity çalıştırmasını kullanıcı yapar. İki uygulama dilimi bağlandıktan sonra Windows build'de baştan sona rota ve yeniden oynama ayrıca kontrol edilir; bu belge test veya build başarısı iddia etmez.

## Erken build ve kabul

İlk küçük greybox'ta Windows build al: doğru sahnede açılış, giriş/ateş, HUD, ödül UI, NavMesh/düşman, ölüm/restart, çıkış ve Editor bağımlılıklarının dışarı sızmaması kontrol edilir. O aşamada henüz olmayan final/anahtar adımları sonraki build kontrolüne eklenir; yapılmayan kontroller geçti sayılmaz. Mümkünse temiz klasörden ve başka bilgisayardan çalıştırılır.

Son kabul: F1 gerektirmeyen baştan sona rota; ilk denemede bitirebilme; ayrı opsiyonel yol; anahtar pusu kilidinin doğru açılması; gruplar arası erken açılmama; tekrar alım/girişte çoğalmama; ölümde temiz reset; kamera kontrol/HUD devri ve skip; finalde boss hasarı/yanlış restart olmaması; parkurda takılmama; demo sonu ve yeniden deneme. Otomatik testler bu sözleşmelere göre yazılır, kullanıcı çalıştırır. Mühimmat ikmali ve ödül temposu demo kurulurken ayrıca netleştirilecek; bitişi engelleyen rezerv tükenmesi bırakılmaz.
