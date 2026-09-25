# Toparlanan silah tepmesi

## Content üzerindeki sözleşme

Bu aşama kalıcı nişan kaymasını geçici yukarı tepme ofsetiyle değiştirir. İlk Shotgun/Minigun checkpoint'lerindeki geri dönmeyen davranışın yerine geçer. Yeni silah, merminin yüzeyden sekmesi, model animasyonu veya ses eklenmez.

- Temel nişan ve son kamera pitch'inin sahibi PlayerLook'tur. Fare/gamepad temel nişanı değiştirir; tepme bu temel açıya yazmaz. Son pitch, mevcut sınırlar içinde temel nişan eksi tepme ofsetidir.
- Önce mermi/tüm saçmalar çözümlenir, ardından başarılı tetik başına tek ani darbe uygulanır. Iska tepme üretir, engellenen atış üretmez.
- Yeni darbe kalan ofsetin üzerine eklenir, birikme sınırını uygular ve bekleme süresini yeniden başlatır. Ardından sonlu süreli smoothstep eğrisiyle sıfıra yumuşak dönüş olur. Dönüş sırasında ateş edilirse eski hedef açıdan değil mevcut ofsetten devam edilir.
- Toparlanma oyun zamanını kullanır. Pause ve devre dışı oyuncu kontrolü toparlanmayı dondurur; devamda birikmiş zaman sıçraması olmaz. Reload ve silah değişimi kamerayı aniden sıfırlamaz. Toparlanma zamanlamasını son gerçek atış belirler. Daha düşük sınırlı silaha geçmek mevcut tepmeyi aniden küçültmez; ofset o sınırın altına inene kadar artıramaz.
- Oyuncunun tepme karşılaması temel nişanı normal şekilde değiştirir. Dönüş yalnız tepme etkisini kaldırır; kamera atış öncesindeki dünya yönüne değil oyuncunun yeni nişanına oturur. Yukarı pitch sınırına bakmak görünmeyen tepmeyi kırpar; sınır ötesinde gizli darbe birikmez.
- Look component'ini devre dışı bırakmak/yok etmek geçici ofseti temizler. Yeni oyuncu tepmesiz başlar. Otomatik yatay darbe veya nişan yardımı yoktur.
- Inspector değerleri bütün seviyelerde edinim öncesi doğrulanıp kopyalanır: darbe ve gecikme sonlu/negatif olmayan; dönüş süresi ve birikme sınırı sonlu/pozitif; darbe sınırı aşamaz. Eski tepmesiz silahlar geçerli varsayılanlarla sıfır darbeyi korur.

## İlk deneme değerleri

| Silah | Darbe | Bekleme | Dönüş süresi | Birikme sınırı |
| --- | --- | --- | --- | --- |
| Shotgun, tüm seviyeler | 6° | 0.08 s | 0.30 s | 12° |
| Minigun, seviye 1–7 | 0.35° | 0.12 s | 0.25 s | 8° |
| Minigun, seviye 8 | 0.20° | 0.12 s | 0.25 s | 8° |

Sayılar geçicidir. Shotgun normalde sonraki izin verilen atışından önce toparlanır; Minigun atış aralığı toparlanma gecikmesinden kısa olduğundan sürekli ateşte tepme birikir. Diğer silahlar şimdilik sıfır darbede kalır; aynı veri tabanlı mekanizmayı kullanabilirler.

## Doğrulama planı

Saf model testleri gecikme sınırını geçmeyi, tam dönüşü, farklı kare adımlarını, bekleme/dönüş sırasında birikmeyi, sınırları, profil değişimini, pitch boşluğunu, sıfırlamayı, sıfır zamanı ve geçersiz değerleri kapsar. Asset testleri sekiz seviye ve eski varsayılan/snapshot davranışını kontrol eder. Sahne testleri gerçek Shotgun atışını, saçma başına çoğalmamayı, Minigun basılı ateş/bırakmayı, nişan karşılamayı, pause'u, silah değişimini ve mevcut engellenmiş atış davranışlarını kapsar.

EN/TR dokümanları güncellenir, uygulama ve otomatik doğrulama yapılır; kullanıcı his değerlendirmesi için durulur. Check-in/commit kullanıcıya aittir; doğrudan content üzerindeki bu aşamada PR veya merge gerekmez.

## Uygulama ve doğrulama

- Doğrulanan `WeaponRecoilConfig`, saf `WeaponRecoilState`, seviye başına kopyalanan veri ve tek kamera sahibi PlayerLook ile uygulandı. Toparlanma, gecikmeden kalan zamanı tüketip son atışın süresinde smoothstep eğrisini ilerletir. Darbe anlıktır; yalnız dönüş yumuşatılır.
- Shotgun tüm seviyelerde 6°/.08sn/.30sn/12° kullanır. Minigun .35° (sekizinci seviyede .20°) darbesini korur; .12sn/.25sn/8° kullanır. Sahne yeniden kurulmadı; yeni silah, hasar/mühimmat değişimi veya diğer silahların darbesine müdahale yoktur.
- İzole Unity 6000.3.9f1 doğrulaması: **824/824 EditMode, 485/485 PlayMode geçti**. Raporlar: `.codex-temp/xp-attraction/recoil-recovery-edit.xml` ve `recoil-recovery-play.xml`. Toparlanma aşamasında 19 EditMode senaryosu ve bir PlayMode senaryosu eklendi; mevcut asset/sahne testleri genişletildi. Önceki henüz commit edilmemiş Shotgun darbe düzeltmesinde de bir PlayMode senaryosu eklenmişti.
- Dönüş zamanlaması, dönüş sırasında yeniden ateş, sınırlar, geçersiz profiller, snapshot'lar, eski varsayılanlar, gerçek atış, fare/gamepad nişanıyla karşılama, pause/değişim/devre dışı bırakma ve engellenmiş atış/çoklu saçmada ek darbe olmaması doğrulandı. Mevcut test paketlerinin tamamı da geçti.
- Değişen/yeni asset ve kod dosyaları hash ile test kopyasıyla eşleşti; `git diff --check` temiz. Windows build veya sürüm kontrol yazma işlemi yapılmadı. Kullanıcı his kabulü bekleniyor.

## Oynanış testi

Temiz Test_ContentArena oturumunda F1 ile Shotgun edinip Q ile kuşan. Tek atışta daha güçlü yukarı darbe, .08 + .30 saniye içinde temel nişanı değiştirmeden toparlanmalı. Dönüş sırasında fareyi hareket ettirip yeni nişana oturduğunu kontrol et. Saçma sayısı farklı olsa da birinci ve sekizinci seviyede aynı darbe olmalı.

İkinci silah slotunu boşaltmak için Play'i yeniden başlatıp Minigun edin ve basılı ateş et: küçük darbeler 8° sınırına kadar birikip bıraktığında sönmeli. Kısa süre bırakıp toparlanma sürerken yeniden ateş ederek üst üste eklenmeyi dene. Nihai ayar/commit öncesinde darbe gücünü, gecikmeyi ve dönüş hızını değerlendir.
