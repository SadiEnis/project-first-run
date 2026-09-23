# Oynanabilir harita fixture'ı — 9–10. aşamalar

## Yerleşim sözleşmesi

Prototip haritası placeholder geometri kullanan tek bir elle hazırlanmış sahne ve küçük bir hedef sahneden oluşur. Ana alanda oyuncu başlar ve iki isteğe bağlı geri dönüşlü yan yol bulunur. Ön hazırlık koridorları grupları etkinleştirme alanlarından önce hazırlar. İkinci ana alana yeniden kullanılabilir tek yönlü geçitle gidilir; ters bağlantı sözleşme gereği yoktur. Son harita çıkışı 7. aşamadaki sahne yükleme adaptörünü kullanarak PlayableMapDestination sahnesine gider.

Harita wave'leri temizleme koridoru değildir: oyuncu karşılaşmadan ayrılıp düşmanlar yaşarken serbest çıkışı kullanabilir. Gelecekte karşılaşma şartlı çıkışlar eklenebilir; sıradan yan yollarda kullanılmaz. Yan yola dönüş aynı karşılaşma nesnelerini ve drop'ları korur. Tek yönlü bağlantı checkpoint değildir.

## Fixture bileşimi

- Deterministik placeholder zemin/duvar/etiket ve trigger'lar tek içerik kökü altında sahnede bulunur. `PlayableMapFixtureBootstrap` yalnızca debug sunucusudur; runtime'da gameplay nesnesi üretmez.
- Tüm component referansları sahnede serialize edilmiştir: tek oyuncu, tek map session/registry, iki dönüşlü yan geçit, üç hazırlama/etkinleştirme çifti, tek yönlü geçit ve bir sahne çıkışı.
- Mevcut düşman, XP ve level-up sandık servisleri bağlanmıştır. Fixture wave'inde dört Basic Chaser vardır; XP düşürürler, düşman kaynaklı sandık profilleri yoktur. Level-up sandıkları mevcut havuzları kullanır.
- Hedef sahne metadata, kapalı içerik kökü, bilinen giriş, zemin/duvar/NavMesh, registry ve ödül servisleri taşır. İkinci bir çatışma alanı değildir.
- Çıkış/trigger collider'ları oyuncu gövdesine uygun boyuttadır ve oyuncunun üzerinde spawn olmaz. Etiketler kaynak/hedef kimliğini ve dönüş/tek yön bilgisini gösterir.

Geçici görünümde gri/mavi ana zemini, camgöbeği kuzey dönüşlü yan alanı, yeşil güney dönüşlü yan alanı, sarı ise ikinci ana alanı gösterir. Gri bağlantı zeminleri görünen boşlukları yürünebilir hale getirir. Renkli alanlar kilitli duvar değil zemindir; her bağlantının ince geçit kısmı geçiş trigger'ı/bariyeridir.

## Kabul testleri

Fixture'ı Unity Editor'den açıp çalıştır: oyuncu ana alanda başlar, iki yan yolu ziyaret edip dönebilir, hazırlama/etkinleştirme rotasını görür ve tek yönlü rotayı ters geçit olmadan kullanır. Son çıkış hedef sahneyi yükler ve aynı oyuncuyu girişe yerleştirir. Kısa ve keşif rotalarını dene. İkinci oyuncu, varıştan önce görünen hedef spawn'ı veya geçişte otomatik XP/sandık ödülü olmadığını doğrula.

Fixture küçük ve deterministiktir. Nihai sanat, procedural üretim, düşman çeşitliliği, performans bütçesi veya restart UI'sını temsil etmez; bunlar sonraki polish/doğrulama işleridir.

Bu prototipte hedef sahne asset path üzerinden Editor'de yüklenebilir. Doğrulama build aracı iki sahneyi açık sahne listesiyle paketler; varsayılan Build Profile'ı değiştirmez. Elle başka bir build alınacaksa bu sahneler onun listesine eklenmelidir.

## 10. aşama uygulama ve kabul sözleşmesi — 20 Eylül 2026

- Örnek sahne Editor'de hazırlanır; zemin, duvar, trigger, NavMesh ve servis referansları kaydedilir. Play sırasında harita yeniden kurulmaz. Yeniden üretme aracı yalnızca bu fixture sahnelerini yönetir.
- İki yan alan ve ikinci ana alan mevcut Basic Chaser içeriğinden dörder düşman kullanır. Her grup 100 XP sağlar; mevcut 100/150/... eğrisi ve level-up sandık kaynağıyla keşif rotası gerçek ilerleme sağlar. Bu sayılar test verisidir, oyun dengesi değildir.
- Görüş kesen duvarlar hazırlama noktası ile spawn noktalarını ayırır. Hazırlama geçitten önce; aktivasyon oda içindedir. Geçitler hazırlığın bitmesini bekler, düşman ölümünü beklemez. Bariyer görünümü ve NavMesh engeli fiziksel collider durumunu izler. Dönüşte aynı düşmanlar ve drop'lar kalır.
- XP başlangıcı, mevcut ödül seçimi prefabı, sandık spawner'ı ve Fireball edinim altyapısı bağlanır. Başlangıçta otomatik ödül ekranı veya ücretsiz yetenek verilmez. Gerçek oyuncu, build ve XP sonraki haritaya taşınır.
- Ayrı oynanabilir varış sahnesi zemin, yerel registry ve ödül servisleri içerir; eski teknik Test_SceneTravelTarget değiştirilmez. Varış zafer ya da checkpoint değildir.
- Gerçek kaydedilmiş sahneyi açan PlayMode testleri hazırlama, aktif navigasyon, XP → level → sandık → seçim, tekrar giriş ve gerçek sahne aktarımını kapsar. Ayrı build doğrulaması açık sahne listesiyle yapılır; kullanıcının varsayılan Build Profile listesi değiştirilmez.
- Hazırlık/aktivasyon maliyetleri ve kare süresi geliştirme panelinde gösterilir. Batch ölçümleri görsel FPS kabulü sayılmaz. Tek yönlü geçişten sonra fiziksel olarak geri kaçış, görünür spawn ve kısa/keşif rotası elle de kontrol edilir.
- Üs/meta/altın bu pakete eklenmez. Harita tabanlı nihai sonuç ve gerçek yeni-run kurulumu ayrıca kapatılacak entegrasyon maddesidir; mevcut arena restart callback'i bunun yerine geçtiği kabul edilmez.

## Otomatik doğrulama kanıtları — 20 Eylül 2026

- İzole proje kopyasında tam test paketleri: **752/752 EditMode, 449/449 PlayMode**; başarısız veya atlanan test yok. Bunların dördü yalnızca yapay karşılaşma nesnelerini değil, kaydedilmiş fixture'ı ve gerçek prefab/servis bağlantılarını kullanır.
- Sahne testlerinin kapsamı: görüş dışında hazırlık ve baked navigasyon; fiziksel tek yönlü kapanma; yan alana dönüşte düşman kimliği/canının korunması; dört ölümden 100 XP ve seçilebilir level-up sandığı; yaşayan düşmanları geride bırakma; gerçek sahne yüklemesinde oyuncu/build/silah/XP sürekliliği ve hedef ödül servislerinin bağlanması.
- Ayrı 32 düşmanlık test, kare başına en çok iki nesne üretildiğini ve hazırlanan aynı nesnelerin etkinleştirildiğini doğrular. En yüksek hazırlık adımı: **2,4892 ms**; aktivasyon: **0,9747 ms**. Testin tamamındaki managed heap farkı **466.944 bayt**; allocation sayacı veya kare başına GC ölçümü değildir.
- Önceki soğuk başlangıçlı sahne testinde bir hazırlık adımı **25,9395 ms** sürdü; sonraki fixture ölçümleri en fazla **1,1808 ms** oldu. 2 ms sınırı yumuşak bütçedir: tek bir Instantiate yarıda kesilemez. Bu değerler takılmasız oynanışı kanıtlamaz veya havuzlamayı gereksiz ilan etmez. Tekrarlanan maliyet ve hedef bütçe normal görüntülü build'de profillendikten sonra havuzlama kararı verilir.
- Batch çalışma, her oynanabilir kamera açısından spawn görünmezliğini, GPU kare süresini, hissedilen takılmayı veya iki rotanın eğlence/dengesini doğrulamaz. Bunlar manuel kabul maddeleridir.

### Manuel rota kontrolü

1. `Assets/_Project/Scenes/Playable/PlayableMapFixture.unity` sahnesini aç; runtime'da yeniden üretme. Başlangıç alanı gridir.
2. Keşif rotası: camgöbeği kuzey veya yeşil güney koridoruna gir. Hazırlık geçitten önce olur. Odada görüş kesen duvarın **sağ ucundan** dolaşarak dört Chaser'a ulaş. XP'lerini topla, level-up sandığını açıp ödül seç. Çıkıp dön; yaşayan düşmanların canı korunmalı ve düşmanlar çoğalmamalı.
3. Kısa rota: iki yan odayı atlayıp doğu geçidinden sarı alana gir. Tamamen geçince kapı arkanda kapanır. Görüş kesen duvarın **kuzey ucundan** dolaş; düşmanlar etkinleşir ama çıkış için onları öldürmek gerekmez.
4. Sarı alanın kuzeydoğu köşesine yakın çıkışı kullan. Oynanabilir hedefte zemin vardır ve oyuncu durumu korunur; ücretsiz XP/sandık veya zafer verilmez. Burası tamamlanmış ikinci bölüm değil, geçiş doğrulama son noktasıdır.
5. Normal görüntülü oynanışta hazırlığın görünmezliğini, rotaların anlaşılabilirliğini ve takılmaları kontrol et. Harita tabanlı ölüm/nihai sonuç/yeni-run bağlantısı ayrıca kalır; bu liste üs döngüsünü onaylamaz.

## Windows paket sonucu — dağıtım kabulü engelli, çalışma ertelendi

- Unity **6000.3.9f1**, URP **17.3.0**; izole test projesinde açık iki sahne listesiyle Windows x64 Development build. Kaynak projenin varsayılan Build Profile'ı, Unity sürümü ve paket bağımlılıkları değiştirilmedi.
- İlk build, Input System linker callback'inde assembly CodeBase metin dönüştürme hatası verdi. Unity mevcut ASCII Windows kısa yoluyla açılınca bu adım geçti. Ardından temiz build tamamlandı (`STAGE10_BUILD_PASS`, 170.735.586 bayt). Üretilen build dosyaları sürüm kontrolü dışındadır.
- **Standalone geçiş testi GEÇMEDİ.** Smoke kontrolü haritaya ulaşmadan player, `UniversalRenderPipelineAsset` (okunan 448 / beklenen 596) ve `UniversalRendererData` (260 / 292) için serileştirme düzeni uyuşmazlığı bildiriyor; yaklaşık 63,8 GB'lık geçersiz bellek ayırma isteğinin ardından çöküyor. Bu, harita hazırlığının o kadar bellek kullandığına dair kanıt değildir.
- `BuildOptions.CleanBuildCache` sonrasında hem `-batchmode -nographics` hem `-batchmode -force-d3d11` ile tekrarlandı. Çalışma dizinini paket klasörü yapmak da çözmedi. Bu denemeler bütün import önbelleği veya asset sorunlarını elemez; kök neden henüz kesinleşmedi.
- Unity'nin [UUM-146262](https://issuetracker.unity.com/issues/23231/urp-asset-serialization-error-and-crash-on-android-when-moving-asemdef-and-scripts-as-part-of-pre-build-process) kaydı, farklı bir Android/build-hook senaryosunda benzer renderer düzeni hatasını rapor ediyor. Bu bir inceleme ipucudur; **bu projede aynı hata bulunduğunun kanıtı** veya Unity yükseltme onayı değildir.
- Devam edildiğinde: geçici kopyada import/tip verisi önbelleklerini ve URP asset'lerini ayırarak incelemek; gerekirse bağımsız import edilmiş kopya veya ayrıca kararlaştırılmış Unity sürümüyle karşılaştırmak. Kaynak projeyi kendiliğinden yükseltme. Ardından `--validate-map` ve dağıtım kabulünü tamamla. 24 Eylül kapsam kararı bu araştırmayı mevcut branch'in merge önkoşulu olmaktan çıkardı; hata hâlâ açıktır.
- Yerel kanıtlar: `.codex-temp/xp-attraction/stage10-all-edit.xml`, `stage10-all-play.xml`, `stage10-build-clean.log`, `stage10-player-clean.log` ve `stage10-player-d3d11.log`. Log ve build'ler geçicidir; kalıcı sonuç bu belgede tutulur.

## Kullanıcı kabulü ve kapsam kapanışı — 24 Eylül 2026

Kullanıcı bölgeler/yan alanlar, kapanan geçit, düşman oluşumu ve ölümü, sandık/XP/seviye akışı ve sahne geçişinin çalıştığını bildirdi. Bu işlevsel kabul, her kamera açısından görünmez spawn veya takılmasız performans garantisi değildir. Kullanıcı Windows build incelemesini erteledi; gerçek haritalarda ölüm/nihai sonuç/temiz yeni-run bağlantısını demo sırasında veya daha erken ihtiyaç doğarsa yapmayı seçti. Branch bu açık işler kayıtlı kalmak koşuluyla harita temeli olarak kapatılabilir; sıradaki iş içerik test sahnesi ve tek tek silah/yetenek genişletmesidir. Bu doküman güncellemesi runtime değişikliği veya yeni otomatik test çalıştırması içermez.
