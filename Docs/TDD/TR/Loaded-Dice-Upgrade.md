# Hileli Zar

## Durum ve kapsam

Davranış dokümantasyon için kararlaştırıldı; oyun kodu uygulaması ve Unity doğrulaması bekliyor. Hileli Zar (Loaded Dice) ortak güçlendirme slotlarını kullanır ve üç seviyelidir. Her seviye, önceki seviyelerle birikmek yerine önceki tam etkinin yerini alır.

| Seviye | Yeşil/Mor/Efsanevi ağırlık çarpanı |
| --- | --- |
| Sahip değil | 1,00 |
| 1 | 1,20 |
| 2 | 1,40 |
| 3 | 1,60 |

Bunlar başlangıç denge değerleridir; nihai denge iddiası değildir. Açıklama seçim ağırlığı artışını belirtmeli; yüzde puan artışı veya nihai ihtimalde aynı oranda garantili artış vaat etmemelidir.

## Olasılık sözleşmesi

- GDD 15.9 kaynak bazlı temel dağılımı kullanılır. Yalnızca Yeşil, Mor ve Efsanevi ağırlıkları mevcut çarpanla çarpılır; ardından tüm geçerli girişler üzerinden normalize edilir. Basit Silah/Yetenek/Güçlendirme ağırlıkları değişmez ve temel tablolarda birbirine eşit kalır.
- Normal düşman, elit ve seviye atlama sandık türü seçimine uygulanır. Normal/elit sandık oluşturma ihtimali %25/%50; seviye atlamada bir sandık garantisi korunur.
- Sandık tanımı seçilirken oyuncunun güncel şansı okunur. Seçilmiş, oluşturulmuş veya yerleştirme bekleyen sandık; güçlendirme seviyesi değişince ya da konum bulma tekrar denenince yeniden seçilmez.
- Paylaşılan ScriptableObject ağırlıkları değiştirilmez. Mevcut etkisiz çağrılar ve rastgelelik kaynağı enjeksiyonu korunur. Değiştirilmiş girişleri tek tek tam sayıya yuvarlayarak dağılım bozulmamalıdır. Seçilen sayısal seçim yöntemi uygulamada belgelenip test edilir.
- Mevcut Luck statı, tüketicileri incelendikten sonra kullanılır; oyuncu statları yoksa temel dağılım geçerlidir. Sonlu olmayan/geçersiz değerler açıkça doğrulanır.
- Boss/Altın ödülleri, evrim ihtimalleri, sandık içi ödül seçenekleri, ek sandık adedi ve şanssızlık koruması kapsam dışıdır. Hariç tutulan girişleri içeren özel tablolarda bu girişlerin ağırlıkları yanlışlıkla artırılmaz.
- Harita geçişinde sahiplik korunur; yeni run sahipliği sıfırlar. Asset içerik arenasına, sahne üreticisine, F1 kataloğuna ve upgrade/karma havuzlara eklenir; kapasite ve maksimum seviyede havuzdan çıkarma korunur.

Temel gelişmiş sandık ihtimali A, çarpan m ise yeni toplam gelişmiş sandık ihtimali A*m / (1-A+A*m) olur. Gelişmiş türlerin kendi aralarındaki oranları korunur.

| Kaynak | Sahip değil | L1 | L2 | L3 |
| --- | --- | --- | --- | --- |
| Normal: sandık düştüğünde yeşil veya üstü | %35 | %39,25 | %42,98 | %46,28 |
| Elit: sandık düştüğünde yeşil veya üstü | %60 | %64,29 | %67,74 | %70,59 |
| Seviye atlama: yeşil veya üstü | %70 | %73,68 | %76,56 | %78,87 |

## Denge değerlendirmesi: görsel polish değil

Sandık kalitesi temel ödül ekonomisinin parçasıdır. Temel ödül temposu, bu isteğe bağlı güçlendirmenin değeri ve uzun şanssızlık serilerinin korunması ayrı değerlendirilir. Tatmin edici ödüller için Hileli Zar'a sahip olmak zorunlu hale getirilmez.

Normal düşmanda L1/L3, sandık düşmesi koşuluyla kalite ihtimalini yaklaşık 4,25/11,28 yüzde puan artırır; 4/15 değil. Değişmeyen %25 düşürme ihtimali dahil edildiğinde öldürme başına yeşil veya üstü ihtimali %8,75'ten yaklaşık %9,81/%11,57'ye çıkar. Bu, 100 normal öldürmede beklenen ilave 1,06/2,82 gelişmiş sandıktır; garantili düşüş değildir. Ayrıca sandık içeriklerinin faydasını tek başına ölçmez.

Gerçek run süresi, sandık kaynakları ve adetleri, eşyayı edinme zamanı, kalan ödül fırsatları, değerli ödüller arasında geçen süre, ödüllerin faydası, build gücü ve güçlendirme slotu/üç seçim fırsat maliyeti değerlendirilir. Erken edinmenin karşılığını verme süresi uzundur; geç edinmek zayıf kalabilir. Yalnızca F1 ile seviye atlamak yerine eşyalı/eşyasız run'lar karşılaştırılır. Üç küçük örneklem nihai dengeyi belirlemez.

Başlangıçta %20/40/60 ağırlık bonusları korunur. Ölçümlerden sonra aynı anda tek denge değişkeni düzenlenir. Seviye/bölge bazlı tablolar ve şanssızlık koruması sonraki ayrı tasarım kararlarıdır; bu işe sessizce dahil edilmez. İleride eklenirlerse şans ile uygulama sıraları tanımlanıp birleşik dağılım test edilir. Bu dokümantasyon adımı bir telemetri sistemi gerektirmez.

## Doğrulama planı

Üç kaynak için eşyasız/üç seviyeli olasılıklar, seçim sınırları, basit türlerin eşit payları, gelişmiş türlerin göreli oranları, Boss/Altın istisnaları, geçersiz girdiler, değişmeyen ortak asset'ler, korunan düşürme sıklıkları, seviye değiştirme, maksimum seviye, eksik statlar, bekleyen yerleştirmede seçimin korunması ve run/harita sahipliği için deterministik testler yazılır. Küçük rastgele örneklemler olasılık testlerinin geçti/kaldı ölçütü yapılmaz.

Derleme, EditMode/PlayMode ve elle kabulü kullanıcı yapar. Yalnızca dokümantasyon içeren bu adımda test çalıştırılmadı. Uygulamadan önce docs checkpoint'inde durulur.
