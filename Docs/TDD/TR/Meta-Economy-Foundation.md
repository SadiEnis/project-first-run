# Meta ekonomi temeli

Durum: 9 Kasım 2026. Kalıcı cüzdanın ilk kod adımı hazır; Unity test kabulü ve oyun akışı bağlantısı bekleniyor.

## Amaç ve önkoşul

Run kazancını kalıcı cüzdandan ayırmak; ölüm/zafer sonucunu hesaplamak ve aynı run için yalnızca bir kez güvenli aktarım yapmak. Tasarım kuralları GDD 18.1'de bulunur.

Branch incelemesinde `feature/meta-economy`, `02a1383` içerik commit'inden başlamaktadır. Son başlangıç ve ortak akış çalışmaları `feature/runtime-foundation` üzerindedir (`979a8b9`). Kodlamadan önce bu temel entegre edilmelidir. Mevcut doküman değişiklikleri bu entegrasyonu gerçekleştirmez.

## Sorumluluklar

- Run kazancı: yalnızca mevcut run'a ait kazanımları tutar; yeni run'da sıfırlanır.
- Sonuç hesabı: kaynak, sonuç, koruma oranı ve boss ödülünden aktarılacak miktarı üretir. UI veya dosya erişimi yapmaz.
- Kalıcı kayıt: cüzdan bakiyesi ve ödeme tekrarını önleyecek run kimliğini tutar. Bakiye ve sonuç kimliği birlikte kaydedilir.
- Akış bağlantısı: mevcut ölüm/final akışından sonucu alır; başarılı kayıt öncesinde run'ı temizleyerek sonucu kaybetmez.
- Sunum: hesaplanan sonuç verisini gösterir; UI'yı açmak veya yeniden çizmek ödeme yapmaz.

Bunlar sorumluluk sınırlarıdır, her satır ayrı sınıf veya interface zorunluluğu değildir. Gerekli ayrımlar mevcut kod incelenerek yapılacaktır. Genel servis bulucu veya geniş bir ekonomi framework'ü kurulmayacaktır.

## Kayıt ve aktarım sözleşmesi

- Kayıt kullanıcı verisi konumunda tutulur; SO asset'i oyuncu kaydı olarak değiştirilmez.
- İlk açılış sıfır bakiyeyle başlar. Kayıt sürümü açıkça tutulur.
- Bozuk/okunamayan veya desteklenmeyen sürümlü kayıt sessizce sıfırlanıp üzerine yazılmaz; hata ve kurtarma yolu sunulur.
- Yazma yarıda kesildiğinde son geçerli kaydı koruyan geçici dosya/değiştirme ve yedek yaklaşımı uygulanıp doğrulanır.
- Kaynak tam sayı olarak tutulur; negatif miktar, geçersiz oran ve sayı taşması reddedilir. Yuvarlama davranışı onaylandıktan sonra sabitlenir.
- Aynı run kimliğiyle yinelenen sonuç çağrısı ikinci ödeme üretmez. Kimlik ve bakiye aynı kayıt işleminde korunur.
- Kayıt hatası başarı olarak gösterilmez. Yeniden deneme aynı sonuç kimliğiyle yapılır; yeniden hesaplayıp ikinci ödül yaratmaz.
- Sonuç kaydı sırasında kapanma ile henüz tamamlanmamış run'ın kapanması ayrı durumlardır. Tamamlanmamış run kazancı için GDD'deki açık karar çözülmelidir.

## Uygulama sırası ve kabul

| İş | Kabul ölçütü |
| --- | --- |
| Kurallar | Ölüm/zafer kabulü ve bırakma/kapanma/yuvarlama kararları yazılı |
| Kalıcı cüzdan | İlk açılış, tekrar açılış, bozuk kayıt ve yazma hatası ele alınmış |
| Run kazancı | Kazanç birikir; yeni run'da sıfırlanır; eski cüzdan etkilenmez |
| Sonuç aktarımı | Ölümde kesinti, zaferde tam aktarım + boss ödülü; yinelenen çağrı ödeme yapmaz |
| Sonuç ekranı | Toplanan, kayıp, boss ödülü, aktarım ve toplam; klavye/gamepad ile erişim |
| Oynanış kaynağı | Kazanım miktarları veri üzerinden ayarlanır; aynı düşmanın ölümü iki kez ödül vermez |

Her anlamlı adımda kullanıcı oynanış kabulü ve check-in yapılır. Unity EditMode/PlayMode çalıştırmalarını kullanıcı yürütür; gerektiğinde yalnızca bildirilen başarısız testler ele alınır. Kritik hesap/kayıt davranışları için odaklı testler yazılır. Windows build kontrolü kayıt sistemi bağlandıktan sonra ayrıca planlanır.

## Araştırma ve sonraki kapsam

Meta öğeleri henüz seçilmedi. Oyun araştırması kaynak kazanımı, ölüm/zafer farkı, satın alma temposu, seviye sınırları ve run içi güçlendirmelerle ilişkiyi karşılaştıracaktır. Ardından tek bir gelişim için satın alma → kayıt → yeni run'da uygulama tamamlanacaktır.

İlk kapsam dışında: gelişim ağacı, ayrıntılı üs, zorluk/hikâye ilerlemesi, anı kayıtları, run ortasında devam kaydı ve bulut kayıt.

## İlk uygulama adımı — kalıcı cüzdan

Runtime temeli feature/meta-economy branch'ine birleştirildi. PersistentMetaWallet, Progression altında tek sınıf olarak eklendi. OpenDefault kullanıcı veri dizinini; Open(directory) ise izole test dizinini kullanır. Sürüm 1 kaydı bakiye tutar. Run kimliği ve tek seferlik sonuç aktarımı sonraki adımda aynı kayda eklenecektir; Credit doğrudan bir run ödülü API'si değildir.

İlk açılış sıfır bakiye dosyasını oluşturur. Artış önce geçici dosyaya yazılıp diske aktarılır; mevcut dosya yedeklenerek değiştirilir. Bellekteki bakiye yalnızca başarılı yazmadan sonra değişir. Ayrı örnekler eski dosya içeriğini ezemez; dosya kilidi eşzamanlı yazmayı engeller.

Bozuk veya bilinmeyen sürümlü kayıt korunarak hata bildirilir. Ana dosya yokken kurtarma dosyası varsa sıfırdan kayıt açılmaz. Otomatik yedeğe dönüş yapılmaz; kurtarma UI'sı sonraki sahne bağlantısı işidir. Sahnelere henüz bağlanmadığı için bu adım oynanışta görünür değişiklik üretmez.

Odaklı EditMode testleri: ilk açılış/yeniden açılış, önceki kayıt yedeği, bozuk/eksik/yeni sürümlü kayıt, kayıp ana dosya, başarısız yazma ve yeniden deneme, eski örnekten yazma, negatif tutar ve taşma. Test yürütme kullanıcıdadır; derleme kontrolü testlerin geçtiği anlamına gelmez.

## İkinci adım — run kazancı ve sonuç aktarımı

RunEarnings yalnızca bir run'ın kimliğini, kazancını ve sabitlenmiş sonucunu tutar. Add pozitif miktar kabul eder; Finish sonrası kazanç eklenemez veya sonuç değiştirilemez. Yeni run için yeni nesne oluşturulur.

RunEconomyResult hesaplaması dosya/UI bağımlılığı taşımaz. Ölümde varsayılan %40 korunur ve kesir aşağı yuvarlanır; boss ödülü verilmez. Zaferde tüm kazanç ve boss ödülü aktarılır. Oran parametreyle değiştirilebilir (0–100); Inspector ayarı sahne bağlantısında eklenecektir. Geçersiz tutar ve taşmalar reddedilir.

PersistentMetaWallet.Settle sonucu bakiye ve run kimliğiyle birlikte kaydeder. Aynı kimlik için sonraki çağrılar false döner ve ödeme yapılmaz; sıfır kazançlı sonuç da kaydedilir. Kimlik geçmişi şimdilik tüm ödenmiş run'ları tutar; yalnızca son kimliği tutmak eski bir sonucun yeniden ödenmesine izin vereceğinden kullanılmamıştır.

Kayıt sürümü 2'dir. Sürüm 1 bakiyesi okunur; ilk başarılı değişiklikte kimlik geçmişiyle sürüm 2 olarak yazılır, eski kayıt yedeklenir. Başarısız yazma bakiyeyi veya bellekteki kimlik geçmişini değiştirmez; aynı immutable sonuçla yeniden denenir. Doğrudan Credit yalnızca düşük seviyeli cüzdan işlemidir, oyun sonuçları Settle kullanmalıdır.

RunEconomyTests ve PersistentMetaWalletTests kullanıcı tarafından çalıştırılmalıdır. Runtime/test derleme kontrolü yapıldı; Unity testleri bu adımda ajan tarafından yürütülmedi. Sahne bağlantısı, sonuç ekranı, düşman kazanımı ve bırakma/kapanma politikası henüz eklenmedi.

## Üçüncü adım — sahne sonuç/çıkış bağlantısı

Kabul edilen geçici politika: aktif run'dan Restart ve menü Quit ölümle aynı %40 aktarımı yapar. Quit kayıt başarılıysa uygulamayı kapatır; Restart yeni run açar. Demo finali zaferdir. Önceden ölüm/zaferle sabitlenmiş sonuç çıkış isteğiyle değiştirilmez. Run kaydet/devam et eklendiğinde Quit aktif run'ı sonlandırmadan kapanacak şekilde yeniden ele alınacaktır. Zorla kapatma/çökmede tamamlanmamış kazanç için henüz devam kaydı yoktur.

RunEconomyController iki demo sahnesinde Demo opening services üzerinde bulunur. Koruma oranı ve boss ödülü Inspector alanlarıdır. Boss ödülü henüz denge kararı verilmediğinden 0'dır; test için değiştirilebilir. Kaynak kazanımı ve sonuç özeti UI'sı sonraki küçük adımdır; bu adım onları eklemez.

RunRestartController, Restart/Quit öncesinde kaydı ister; başarısızsa sahneyi değiştirmez/kapatmaz. RunEndController ölümü HealthComponent.Died olayından, zaferi final tamamlanma olayından alır; ekranın görünmesine bağlı değildir ve otomatik kayıt tekrarına girmez. Kullanıcının çıkış isteği başarısız kaydı yeniden dener. Sonuç sabitlendiğinde pause Resume kapatılır; yükleme hatasından sonra aynı run'da kazanç toplanamaz.

RunEconomyControllerTests ve RunRestartTests kayıt hatası, yeniden deneme ve tekrar ödeme engelini kapsar. Restart testleri geçici dizin kullanır. Runtime/EditMode/PlayMode kodları derlendi; Unity test yürütme ve oyun içi kabul kullanıcıdadır.

## Dördüncü adım — yerden meta kaynak toplama

İki demo sahnesindeki EnemyMetaResourceDrops, kayıtlı düşmanın Died olayını dinler. Registry olayları yalnızca abonelikleri yönetir; registry'den ayrılma tek başına ödül üretmez. Normal despawn ödül oluşturmaz. Ödül miktarı her düşmanın EnemyDefinition asset'indeki Meta Resource Reward alanındadır. Düşürme bileşeni düşman türü/rank ayrımı yapmaz. 0 değerinde collectible oluşmaz; pozitif değerde o miktarı taşıyan bir collectible oluşur. Mevcut normal düşman asset'lerine 1, elit asset'ine 5 deneme değeri yazılmıştır; yeni asset'in varsayılanı 0'dır. Boss asset'ine de istenirse miktar verilebilir; zafer ödülü ayrı kalır.

MetaResourcePickup ayrı altın renkli prefab/materyaldir. Spawn anında bakiye artmaz; atanmış oyuncu topladığında run kazancı artar. Pause, ölüm ve kesinleşmiş run sonucunda toplama engellenir. 2 metre sabit çekim yarıçapı PlayerMetaResourceCollector üzerinden, 8 hız pickup prefab'ından ayarlanır; bu sürüm XP çekim güçlendirmelerini meta kaynağına uygulamaz. Haritayla birlikte temizlenir.

HUD'da XP yazısının altında GOLD run sayacı bulunur. Kalıcı bakiye ve sonuç dökümü sonraki sunum adımıdır. MetaResourcePickupTests, bir kez toplama, spawn'da kazanç olmaması, pause ve ölüm korumasını kapsar. Unity test yürütme ve görsel kabul kullanıcıdadır.

## Refactor sonrası sorumluluk sınırları

Yukarıdaki adımlar geliştirme geçmişini anlatır. Güncel sorumluluk dağılımı:

- PersistentMetaWallet bakiye, taşma kontrolü ve aynı run için tek ödeme kuralını yönetir. Tek kayıt sınırı IMetaWalletStore sözleşmesidir. Immutable MetaWalletSnapshot bakiye ve ödenmiş kimlikleri birlikte taşır.
- JsonMetaWalletStore JSON sürümü, dosya yolu, kilit, eski kaydı ezmeme, atomik yazma ve yedek korumayı yönetir. Sürüm 1/2 kayıtları uyumludur; kayıt yolu ve şema değişmedi.
- RunEconomyController sonucu sabitler, enjekte edilen kayıt deposuyla aktarım ve yeniden denemeyi koordine eder. Yerel JSON deposunu sahne bağlantı noktası RunSceneStartup seçer.
- RunEndController terminal sonuçları alır. Demo final oturumu tamamlanmayı bir kez yayımlar; final controller bu sinyali iletir. Restart/Quit çıkmadan önce kaydı ister, menü kaynağından ölüm/zafer seçmez.
- EnemyMetaResourceDrops ölen düşmanın asset ödülünü okur ve spawn ister. MetaResourceSpawner prefab oluşturma ve sahneye yerleştirmeyi üstlenir. PlayerMetaResourceCollector kabul koşullarını kontrol edip run kazancına ekler. MetaResourcePickup hareket/temas ve tüketilmeyi yönetir; can/ekonomi bağımlılığı taşımaz.

İki kayıtlı demo sahnesinde Demo opening services üzerinde run-end controller ve resource spawner, Player (demo) üzerinde collector bulunur. Sahne geometrisi yeniden üretilmeden bağlantılar güncellendi. Genel event bus, service locator veya her somut sınıf için interface eklenmedi.

Doğrulama: Runtime, Editor, EditMode ve PlayMode derlemeleri başarılı. Unity test yürütmesi kullanıcıdadır. Odaklı kontroller: PersistentMetaWalletTests, MetaWalletStoreContractTests, RunEconomyTests, RunEconomyControllerTests, DemoFinalSessionTests; RunEndControllerTests, RunRestartTests, EnemyMetaResourceDropsTests ve MetaResourcePickupTests. Oynanışta yerden toplama, ölümde %40 aktarım, zaferde tam aktarım ve tekrar çıkış isteğinde çift ödeme olmaması kontrol edilmelidir.
