# Güçlendirme içerik planı

## Kapsam ve iş akışı

Git branch'i: `feature/content/upgrades-content`; tabanı `content`. Hedef Plastic branch'i: `/main/dev/content/upgrades-content`. 16 güçlendirme bu tek dalda yapılır. Branch, check-in, commit ve merge kullanıcıya aittir; eşya başına branch açılmaz.

Kabul edilen kapsam GDD 13.1'deki dokuz stat ve 13.2'deki yedi özellik güçlendirmesinin tamamıdır; önceki on güçlendirme hedefinin yerini alır. Aşağıdaki isimler onaylanan çalışma adlarıdır; oyun temasıyla değişebilir. Sabit teknik kimlikler çevrilen görünen adlara bağlı olmamalıdır; somut kimlikler uygulamada atanır.

Bu checkpoint yalnızca dokümantasyondur. Yeni asset, runtime entegrasyonu veya doğrulama tamamlanmış sayılmaz.

## Onaylanan liste

| Grup | Türkçe ad | İngilizce ad | Mekanik kimlik |
| --- | --- | --- | --- |
| Stat | Hiddet Mührü | Wrath Seal | Genel silah/yetenek hasarı |
| Stat | Demir Deri | Ironhide | Hasar azaltma |
| Stat | İkinci Kalp | Second Heart | Maksimum can |
| Stat | Yaşam Filizi | Lifesprout | Zamanla can yenileme |
| Stat | Çekim Çekirdeği | Attraction Core | Pickup çekim menzili |
| Stat | Hafıza Kristali | Memory Crystal | XP kazancı |
| Stat | Kırık Kum Saati | Broken Hourglass | Yetenek bekleme süresi |
| Stat | Rüzgâr Örgüsü | Windweave | Hareket hızı |
| Stat | Ateş Ritmi | Fire Rhythm | Uygun silahların atış hızı |
| Özellik | Son Direniş | Last Stand | Düşük canda hasar (Berserker) |
| Özellik | Demir Yemin | Iron Oath | Zırh karşılığında hareket kaybı (Heavy Armor) |
| Özellik | Kan Ahdi | Blood Pact | Maksimum can karşılığında hasar |
| Özellik | Cam Kalp | Glass Heart | Dayanıklılık karşılığında büyük hasar (Glass Cannon) |
| Özellik | Hileli Zar | Loaded Dice | Şans/ödül olasılıkları (Lucky) |
| Özellik | Akıcı Mekanizma | Fluid Mechanism | Reload hareket cezasını azaltma (Ammo Expert) |
| Özellik | Kızıl Diş | Crimson Fang | Öldürmelerle can kazanma (Vampire) |

## Önerilen uygulama sırası

1. Ortak kurallar ve etki bağlantıları: seviye sayısı, birleşme formülü, sınırlar, stat değişim zamanı ve can uyarlama politikaları. Stat enum'unda bulunmak oyuna bağlı olduğu anlamına gelmez.
2. Hiddet Mührü, Rüzgâr Örgüsü, Ateş Ritmi: temel hasar/hareket bağlantıları.
3. İkinci Kalp, Demir Deri, Yaşam Filizi: maksimum can, hasar azaltma ve iyileşme.
4. Çekim Çekirdeği, Hafıza Kristali: pickup ve XP.
5. Kırık Kum Saati: [Kırık Kum Saati](Broken-Hourglass-Upgrade.md) kapsamındaki bekleme azaltımı. Akıcı Mekanizma reload hareket cezası tasarlanıncaya kadar kimliği korunarak ertelendi; şarjör hızına çevrilmez.
6. Son Direniş, Demir Yemin, Kan Ahdi, Cam Kalp: koşullu ve bedelli etkiler.
7. Kızıl Diş, Hileli Zar: öldürme sahipliği/iyileşme ve kapsamı belirlenmiş olasılık etkileri.
8. Ödül/F1 entegrasyonu ve birlikte çalışma değerlendirmesi, ardından kullanıcı kabulü ve dal kapanışı.

Her grubun mekaniği uygulama öncesinde konuşulur. Koddan önce EN/TR TDD kaydı alınır. Her dosyayı parçalamadan docs ve anlamlı uygulama checkpoint'leri ayrılır. EditMode/PlayMode ve elle testleri kullanıcı yapar; asistan test yazar, yalnızca özellikle istenen eksik/başarısız çalıştırmalarla ilgilenir.

## Ortak kurallar ve açık kararlar

- Tüm 16 eşya mevcut ortak güçlendirme slot havuzundadır; stat/özellik ayrımı yeni kategori veya ayrı kapasite değildir.
- Onaylanan ilerleme: dokuz stat güçlendirmesi beş, yedi özellik güçlendirmesi üç seviye. Normal yüzdeler toplanır; yeni seviye önceki tam etki grubunun yerini alır. İlk grubun değerleri ve kuralları [Temel Stat Güçlendirmeleri](Core-Stat-Upgrades.md) belgesindedir; diğer eşya değerleri açıktır.
- Her seviye üst üste eklenen farkı değil tam etki grubunu tanımlar. Mevcut etki sahipliği, değiştirme ve maksimum seviyede havuzdan çıkarma korunur.
- Silah/yetenek isabetleri, parçacıklar ve zamanlı hasar kapsamı doğrulanır; atış anında alınan değerler ile canlı hesaplama belgelenir.
- Maksimum can artışı/azalışı, hasar azaltma formülü/sınırı ve yenilenme [Hayatta Kalma Stat Güçlendirmeleri](Survival-Stat-Upgrades.md) doğrultusunda uygulandı; kullanıcı doğrulaması bekleniyor.
- Onaylanan cooldown kapsamı normal yeniden kullanım ve Shuriken yörünge-sonu beklemesidir; Drone sıklığı ve aktif etkiler hızlandırılmaz. Bkz. [Kırık Kum Saati](Broken-Hourglass-Upgrade.md). Önce uygulanabilir güçlendirmeler tamamlanır; ek sistem gerektirenler sonra tek tek ele alınır.
- Son Direniş, Demir Yemin, Kan Ahdi ve Cam Kalp mekanikleri [Koşullu ve Bedelli Güçlendirmeler](Conditional-Tradeoff-Upgrades.md) içinde onaylandı. Tek tek uygulanıp her öğeden sonra kabul alınır; Cam Kalp zırhtan önce gelen hasarı artırır.
- Akıcı Mekanizma reload hareket cezası kimliği değişmeden ertelenmiş kalır. Eşyayı işe yarar yapmak için onaysız yeni ceza icat edilmez.
- Kızıl Diş uygulandı: zamanlı hasar dahil oyuncuya ait öldürmede garantili 2/5/7 can; maksimumu aşma veya diriltme yok. Bkz. [Kızıl Diş](Crimson-Fang-Upgrade.md). Kullanıcı doğrulaması bekleniyor.
- Hileli Zar'ın belgelenen davranışı Yeşil/Mor/Efsanevi seçim ağırlıklarını %20/40/60 artırır; sandık adedi ve oluşturma ihtimali değişmez. Kapsam, olasılık hesabı ve denge ölçütleri için bkz. [Hileli Zar](Loaded-Dice-Upgrade.md). Uygulama bekliyor; evolution, altın/meta ve şanssızlık koruması kapsam dışıdır.
- Run sahipliği, sıfırlama ve haritalar arası davranış korunur; yalnızca tekil edinme değil kombinasyonlar da test edilir.
- Geliştirme hasar öğesinin değiştirilmesi/geçişi açıkça kararlaştırılır; havuzlarda yanlışlıkla aynı rolü taşıyan gerçek hasar öğeleri çoğaltılmaz.

## Özel ödül kartı sunumu

Yedi özellik güçlendirmesi ileride parlak çerçeveyle ayrıştırılacak; ışıltı çerçevenin dışına taşmayıp içine doğru yayılacak. Kullanıcı görsel referans sağlayabilir. Referans/tasarım bekleyen sunum işi olarak kaydedilir; şimdi genel dış bloom uygulanmaz. Bu görünüm daha yüksek nadirlik veya ayrı slot havuzu anlamına gelmez.

Altın/meta, yeni evolution içeriği, nihai tema/görseller ve Windows build kapsam dışıdır. Önceki yetenek doğrulama açıkları (Shuriken kanamasının oynanış doğrulaması ve bildirilmeyen otomatik sonuçlar dahil) bu aşamaya geçince kapanmış sayılmaz.
