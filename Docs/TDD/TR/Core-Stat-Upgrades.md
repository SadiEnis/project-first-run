# Temel stat güçlendirmeleri — ilk içerik grubu

## Durum ve kapsam

upgrades-content üzerinde Hiddet Mührü, Rüzgâr Örgüsü ve Ateş Ritmi mekanikleri onaylandı. Bu uygulama öncesi doküman checkpoint'idir; yeni oynanış uygulaması veya test çalıştırma tamamlanmış değildir.

Ortak ilerleme: dokuz stat güçlendirmesi beş, yedi özellik güçlendirmesi üç seviyelidir. Aynı stata gelen normal yüzdeler toplanır. Her seviye kendi güçlendirmesinin önceki tam etki grubunun yerini alır; önceki seviyeler tekrar eklenmez. Diğer kaynaklar korunur. Özellik bedelleri/sınırları ve kalan eşyaların kuralları ayrıca konuşulacak.

## Onaylanan başlangıç ilerlemesi

| Eşya | İngilizce ad | L1 | L2 | L3 | L4 | L5 |
| --- | --- | --- | --- | --- | --- | --- |
| Hiddet Mührü | Wrath Seal | +%10 | +%20 | +%30 | +%40 | +%50 |
| Rüzgâr Örgüsü | Windweave | +%5 | +%10 | +%15 | +%20 | +%25 |
| Ateş Ritmi | Fire Rhythm | +%10 | +%20 | +%30 | +%40 | +%50 |

Bunlar geçici denge değerleridir; nihai ayarlar değildir.

### Hiddet Mührü

- WeaponDamage ve AbilityDamage statlarına aynı toplanabilir yüzde uygulanır.
- Parçacıklar, ikincil patlamalar ve zamanlı etkiler dahil gerçek hasar bağlantıları incelenir. Bonus bir kez uygulanır; hem atışta hem isabette tekrar çarpılmaz.
- Uçan mermi/etkin etkilerin mevcut atış/aktivasyon anlık görüntüsü korunur; sonraki stat değişiklikleri yeni hasar hesaplarını etkiler. Bu eşya için mevcut yetenek yaşam döngüleri gereksiz yere canlı güncellemeye dönüştürülmez.
- Oynanabilir katalog/ödül havuzlarında geliştirme amaçlı hasar öğesinin yerini alır; iki eşdeğer hasar öğesi sunulmaz. Uygulamadan önce referanslar incelenip asset/kimlik geçişi açıkça seçilir; mevcut test fixture'ları bilinçli korunur.

### Rüzgâr Örgüsü

- MoveSpeed yerdeki harekete bağlanır. Zıplama yüksekliği ve yerçekimi değişmez.
- Önceden değiştirilmiş sonuçtan değil, ayarlanan asıl temel hızdan hesaplanır. Girdi normalleştirmesi ve mevcut hareket kısıtları/cezaları korunur.
- Edinme, seviye değişimi, sıfırlama ve harita geçişleri çarpanları biriktirmemeli veya eski hızı bırakmamalıdır.

### Ateş Ritmi

- WeaponFireRate uygun oyuncu silahlarında saniyedeki atış sayısına bağlanır. Aralık = 1 / hesaplanan atış hızı: +%50 hız aralığı 2'ye değil 1,5'e böler.
- Otomatik/yarı otomatik girdi davranışı, şarjör tüketimi ve reload kuralları korunur. Yarı otomatik silah her atış için yeni basış gerektirir.
- Reload, yetenek cooldown'u veya Drone atışı hızlanmaz; mermi hızı, saçma/parçacık sayısı ve atış türü değişmez.
- Edinme/seviye/silah değişiminde mevcut atış beklemesi sıfırlanmaz, bedava atış verilmez. Yeni atış aralıkları güncel hızdan hesaplanır. Düzenlemeden önce mevcut silah durum sahipliğiyle uyumu doğrulanır.
- Silah değişiminde silaha ait mermi veya zamanlama durumu kaybolmaz.

## Entegrasyon ve doğrulama planı

1. Asset geçişinden önce stat bağlantılarını ve geliştirme hasar öğesinin referanslarını incele.
2. Eksik MoveSpeed ve WeaponFireRate bağlantılarını, temel hareket/silah davranışını değiştirmeden belgele ve tamamla.
3. Beş seviyeli gerçek asset'ler, kayıtlı içerik arenası/F1 ve upgrade/karma ödül havuzları.
4. Tam etki değiştirme, yüzdelerin toplanması, maksimum seviyede havuzdan çıkma, üç seviye tablosu, zıplama/yerçekimini değiştirmeyen hareket, atış hızı hesabı/girdi, reload ayrımı, hasar anlık görüntüsü kapsamı, sıfırlama ve haritalar arası koruma testlerini yaz.
5. EditMode/PlayMode ve elle kabulü kullanıcı yapar. Asistan yalnızca istenen eksik/başarısız testleri çalıştırır. Otomatik VCS işlemi yoktur.

Uygulama sonunda bu üç eşyanın oynanış değerlendirmesi için durulur; sonraki grup veya özel kart ışıltısı henüz uygulanmaz.
