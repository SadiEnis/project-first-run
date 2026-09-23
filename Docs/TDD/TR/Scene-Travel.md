# Sahneler arası harita geçişi — 7. aşama

## Mekanik

- Genel harita çıkışında Inspector'dan tam sahne asset yolu ve giriş kimliği seçilir. Harita sırasını çıkışların hedefleri belirler; yollar dallanabilir, zorunlu doğrusal liste yoktur. Sahne build'e dahil olmalıdır. Çıkış, kapı/çukur türü değil sahne yükleme adaptörüdür.
- `MapSceneRoot`, hedef haritayı, kapalı içerik kökünü ve bölge kimliği taşıyan adlandırılmış girişleri tanımlar. Yalnızca yüklenen sahnede giriş aranır. Eksik/tekrarlanan girişler ve tanımsız bölgeler aktarımı engeller. Seçilen bölge haritanın ilk konumu olur.
- Oyuncunun aynı kök GameObject'i, çocukları ve runtime bileşenleri korunur. Yeni oyuncu üretme, snapshot'tan yeniden kurma, XP/build sıfırlama veya otomatik ödül yoktur. Oyuncunun kamerası, silah/yetenek çıkışları ve kalıcı runtime bağımlılıkları bu hiyerarşiye ait olmalıdır. Sahne UI/spawner'ları haritada kalır ve gelen oyuncuya bağlanır.
- Yetenek hedef seçicileri ve factory'leri için harita registry bağlama sözleşmesi vardır: mevcut runtime kayıtları ve gelecekteki edinimler, yalnızca aktarım kesinleşince hedef düşman registry'sine bağlanır. Yetenek seviyesi/cooldown ve upgrade modifier'ları korunur. Başlatılmış yetenek edinimi veya edinilmiş yeteneği olan oyuncu için haritada tam bir yerel düşman registry'si gerekir. İleride ek harita bağımlılığı olan yetenekler bu sözleşmeyi açıkça genişletmelidir.
- Oyuncu kökünde tek `SceneTravelController` bulunur. Kaynak haritada yerel/diğer geçişleri kilitler; scaled oynanışı ve oyuncunun input/aksiyon güncellemelerini durdurur; hedefi additive yükler. Hedef doğrulama/bağlama tamamlanana kadar eski harita tutulur. Time scale ve önceden kapalı bileşenler dahil enabled durumları geri yüklenir.
- Hedef içerik sahnede kapalı kaydedilir: bağlamadan önce düşman, UI veya bootstrap çalışmaz. Kök yalnızca metadata taşır; oynanış içerik altında bulunur. Bölge, hazırlık ve sandık bootstrap bileşenlerine oyuncu aktivasyondan önce verilir. Hedefte eski development bootstrap'ları veya ikinci oyuncu bulunmamalıdır.
- Doğrulamadan sonra oyuncu girişe taşınır, hedef aktif sahne yapılır, içerik açılır ve eski sahne kapatılıp kaldırılır. Eski düşman, projectile ve toplanmamış drop'lar taşınmaz. Harita kaldırılınca gerideki ödüller telafi edilmeden kaybolur. Varış zafer veya checkpoint değildir.

## Hata ve yaşam döngüsü

Ölü/duraklatılmış oyuncu, bekleyen yerel geçiş, yinelenen istek, yanlış yol, zaten yüklü hedef ve aynı sahne isteği yüklemeden reddedilir. Aktarım kesinleşmeden iptal/ölüm/controller kapanması veya hatalı hedef kaynak haritada kalmayı ve yalnızca yeni hedefin kaldırılmasını sağlar. Unity yüklemesi zorla kesilmez: işlem tamamlanır, gereksiz sahne temizlenir. Devam eden yüklemeyi sahipsiz bırakan timeout yoktur.

Oyuncu taşındıktan sonraki eski sahne temizleme hatasında yok edilen içerik geri alınamaz: oyuncu hedefte tutulur, eski kökler kapatılır, hata bildirilir ve açık temizleme tekrarı başarılı olana kadar yeni geçiş engellenir. Aktarım öncesindeki temizlik hatası da tekrar gerektirir. İptal yalnızca aktarım kesinleşmeden uygulanır. Uygulamanın kapanması ve sahip olunan sahnelerin dışarıdan kaldırılması normal işlem sözleşmesinin dışındadır.

## Sahne kurulumu

Hedefte iki kök oluşturulur: `MapSceneRoot` taşıyan açık metadata nesnesi ve `MapTraversalController`, geometri, yerel `EnemyRegistry`, girişler, bölge bileşenleri, çıkışlar ve UI taşıyan **kapalı** içerik nesnesi. Metadata üzerinde içerik/harita/giriş dizisi atanır. Giriş noktaları içerik altında bulunur. Yerel geometri/grup/spawner/view referansları normal atanır; gelen oyuncu runtime'da bağlanır. Çıkışın kaynak bölgesi yerel haritada bulunmalıdır; hedef yolu ve giriş kimliği sonraki haritayı seçer. Runtime yükleyici için hedef sahneler build'e eklenir.

İlk sahnede kök oyuncu ve başlatılmış build/edinim sistemleri korunur; içerik açıktır, çıkışlar oyuncudaki `SceneTravelController` ve yerel `MapSceneRoot` referanslarını kullanır. Hedef haritalar başlangıç loadout/XP/development bootstrap'larını yeniden çalıştırmaz. Gerekli bir hedef bileşeni bağlanamıyorsa yarım yapılandırılmış oynanış başlatmak yerine hedef reddedilir.

Hedefteki `LevelUpChestSource`, mevcut oyuncu XP'sine bağlanır ve güncel seviyeyi başlangıç kabul eder: eski seviyeler tekrar ödüllendirilmez, sonraki seviyeler normal şekilde ödül kuyruğuna girer. Sahneye ait bekleyen drop kuyrukları oyuncuyla taşınmaz; bekleyen ödüllerin kalıcı aktarımı uygulanmadı.

`Test_SceneTravelTarget`, kapalı içerik ve bir giriş taşıyan küçük teknik test sahnesidir; arena tasarımı değildir. Gerçek yükleme testi Unity'nin Editor additive yükleme yolunu kullanır; kullanıcının build sahne listesi değişmez. Paketlenmiş build yüklemesi ve platform performansı, oynanabilir harita entegrasyonunda ayrıca doğrulanmalıdır.

## Doğrulama kapsamı

Bu tek oyunculu temeldir; koridorda arka plan streaming'i veya sıfır takılma garantisi değildir. Yükleme UI/fade, save/restart, nihai zafer, NavMesh/lightmap tasarımı ve oynanabilir harita sonraki adımlardır. Yeni hedefler bu sözleşmeyi kullanır; `Test_Waves` örtük biçimde dönüştürülmez. Testler gerçek Unity sahnelerinde oyuncu kimliği/durumu, giriş seçimi, yinelenen istek, hatalı hedef, iptal/ölüm, temizleme/tekrar ve harita kilidini kapsar. Kontrollü işlem testlerine gerçek additive yükleme adaptörü testi eklenir. Sonuçlar çalıştırıldıktan sonra yazılır.

Unity kaynakları: [asenkron sahne yükleme](https://docs.unity3d.com/6000.3/Documentation/ScriptReference/SceneManagement.SceneManager.LoadSceneAsync.html), [kök nesneyi sahneye taşıma](https://docs.unity3d.com/6000.3/Documentation/ScriptReference/SceneManagement.SceneManager.MoveGameObjectToScene.html).
