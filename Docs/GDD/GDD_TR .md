# GDD_TR

# Oyun Tasarım Dokümanı

> **Yaşayan Doküman Notu:** Bu belge yaşayan bir tasarım dokümanıdır. Oyuncuya sunulacak deneyimi önemli ölçüde değiştiren her karar bu belgeye yansıtılmalıdır. Kesinleşmemiş fikirler açıkça geçici karar veya açık soru olarak işaretlenmelidir.
> 

---

## 1. Doküman Bilgileri

| Alan | Değer |
| --- | --- |
| Proje Adı | Project First Run |
| Doküman Türü | Oyun Tasarım Dokümanı |
| Doküman Sürümü | v0.1 |
| Durum | Ön Yapım |
| Tür | Birinci Şahıs Roguelite / Arena Shooter |
| İlk Hedef Platform | PC |
| Oyun Motoru | Unity 6.3.9 |
| Sürüm Kontrolü | Unity Version Control / Plastic SCM |
| Son Güncelleme | 31.07.2026 |
> Project First Run mevcut geliştirme kod adıdır. Oyunun nihai ticari adı daha sonra değişebilir.
---

## 2. Vizyon

Her turun anlamlı build seçimleri, agresif çatışmalar ve hissedilebilir güçlenme aracılığıyla farklı geliştiği, hızlı tempolu bir birinci şahıs roguelite geliştirmek.

Oyun, oyuncunun doğrudan kontrol ettiği silahları otomatik kullanılan yeteneklerle birleştirir. Oyuncu hareket etmeli, doğru nişan almalı, silahlarını yönetmeli ve silahlar, yetenekler, güçlendirmeler ile evrimler arasında uyumlu kombinasyonlar oluşturmalıdır.

Her sistem, çatışmayı okunamaz veya gereksiz derecede karmaşık hale getirmeden oyuncunun giderek güçlendiğini hissettirmelidir.

---

## 3. Üst Düzey Oyun Tanımı

Agresif FPS çatışmalarını, otomatik hayatta kalma oyunu yeteneklerini, arena tabanlı ilerlemeyi, build oluşturmayı ve turlar arasında kalıcı gelişimi bir araya getiren hızlı tempolu bir birinci şahıs roguelite.

---

## 4. Oyunun Genel Tanımı

Oyuncu; düşman dalgaları, elit düşmanlar ve boss'larla dolu, birbirini takip eden savaş arenalarında mücadele eder. Bir turun temel amacı hayatta kalmak, mevcut build'i geliştirmek, tüm arenaları geçmek ve son boss'u yenmektir.

Oyuncu şarjörlü silahları aktif olarak kullanırken sahip olduğu yetenekler otomatik biçimde çalışır. Silahlar nişan alma, pozisyon alma, mühimmat yönetimi ve zamanlamayı ödüllendirir. Yetenekler ise ek hasar, kalabalık kontrolü, alan hâkimiyeti, güvenli alan oluşturma ve otomatik hedef baskısı sağlar.

Düşmanlar deneyim puanı düşürür ve ayrıca altın veya ödül sandığı düşürebilir. Yeterli deneyim toplandığında oyuncunun tur içindeki seviyesi artar. Her seviye atlamada kesin olarak bir sandık elde edilir. Normal ve elit düşmanlar da kendilerine ait olasılıklar üzerinden ek sandıklar düşürebilir.

Sandıklardan yeni silahlar, yetenekler ve güçlendirmeler alınabilir veya sahip olunan eşyaların seviyeleri yükseltilebilir. Daha yüksek nadirlikteki bazı sandıklar, gerekli şartlar sağlandığında evrim seçeneği de sunabilir.

Tur sona erdiğinde toplanan altının belirli bir kısmı kalıcı ilerlemeye aktarılır. Başarılı turlar ve son boss ödülleriyle ek ekipman slotları ve diğer kalıcı geliştirmeler açılabilir.

---

## 5. Temel İlham Kaynakları

| Oyun | Alınan Temel İlham |
| --- | --- |
| DOOM | Agresif birinci şahıs çatışmaları, hareket, silah hissi ve savaş yoğunluğu |
| Vampire Survivors | Otomatik yetenekler, tur içi seviye gelişimi, build oluşturma ve ekipman evrimleri |
| Dead Cells | Arena tabanlı ilerleme, giderek zorlaşan karşılaşmalar, boss'lar ve tur yapısı |

Bu oyunlar doğrudan şablon değil, referans noktalarıdır. Proje; aktif FPS çatışmasını, otomatik yetenekleri, sandık tabanlı ilerlemeyi ve değişen arenaları birleştirerek kendi kimliğini oluşturmalıdır.

---

## 6. Tasarım İlkeleri

### 6.1 Agresif Çatışma

Oyuncu sürekli hareket etmeye, nişan almaya, konum değiştirmeye ve düşmanlarla çatışmaya teşvik edilmelidir. Sabit durmak veya aşırı pasif oynamak çoğu durumda tehlikeyi artırmalıdır.

### 6.2 Anlamlı Build Seçimleri

Sandık ödülleri, tek bir bariz doğru seçenek yerine gerçek kararlar oluşturmalıdır. Silahlar, yetenekler ve güçlendirmeler farklı oyun tarzlarını ve build yönelimlerini desteklemelidir.

### 6.3 Tatmin Edici Güçlenme

Oyuncunun gücü tur boyunca görsel ve mekanik olarak artmalıdır. Geliştirmeler uygun olduğunda yalnızca görünmeyen sayısal değerleri değil; mermi sayısını, alan büyüklüğünü, hedefleme biçimini, kalabalık kontrolünü veya davranışı değiştirmelidir.

### 6.4 Build Çeşitliliği

Farklı turlar; yakın mesafe saldırganlığı, uzak mesafeli alan hasarı, otomatik yetenek baskısı, tank odaklı hayatta kalma, yüksek riskli hasar, durum etkileri ve boss hasarı gibi anlamlı biçimde farklı build'leri desteklemelidir.

### 6.5 Kontrollü Kaos

Çatışmada çok sayıda düşman, patlama, mermi, drone, yıldırım ve alan saldırısı bulunabilir. Bu yoğunluğa rağmen oyuncu tehditleri, hasar kaynaklarını, hedefleri, toplanabilir nesneleri ve önemli etkileri anlayabilmelidir.

### 6.6 Tekrar Oynanabilirlik

Rastgele sandık ödülleri, farklı ekipman kombinasyonları, evrimler, arena şartları ve kalıcı ilerleme, tekrar oynanan turları değerli hissettirmelidir.

### 6.7 Öğrenmesi Kolay, Ustalaşması Zor

Temel kontroller ve ilerleme sistemleri anlaşılır olmalı; hareket, nişan alma, pozisyon, kaynak yönetimi ve build optimizasyonu uzun vadeli ustalık alanları oluşturmalıdır.

### 6.8 Önce Oyuncu Geri Bildirimi

Geliştirmeler hissedilebilir olmalıdır. Mümkün olduğunda bir gelişme görsel, işitsel veya mekanik geri bildirim oluşturmalıdır.

Örnekler:

- Yalnızca hasar artışı yerine ek Shotgun saçmaları.
- Yalnızca hasar artışı yerine ek yıldırım zinciri.
- Anlaşılması zor ikinci patlama yerine şarapnel mermileri.
- Yalnızca pasif hasar çarpanı yerine ek drone.

---

## 7. Temel Oyuncu Fantezisi

Oyuncu tura sınırlı ekipmanla başlar ve zamanla son derece yıkıcı bir savaş gücüne dönüşür.

Turun başında düşmanlar tehlikeli, kaynaklar ise sınırlı hissettirmelidir. Oyuncu seviye kazandıkça, sandık açtıkça, eşyalarını geliştirdikçe ve evrimleri açtıkça savaş alanındaki kaos giderek oyuncunun lehine dönmelidir.

Hedeflenen duygusal gelişim:

> Savunmasız hayatta kalan → yetkin savaşçı → uzmanlaşmış build → ezici savaş gücü
> 

---

## 8. Temel Oynanış Döngüsü

Güncel ilerleme (15 Eylül 2026): Tura başla → haritaya gir → keşfet ve isteğe bağlı savaş/güçlen → uygun çıkışa ulaş → sonraki haritaya geç → nihai hedefi tamamla. Savaş, XP ve sandık ödülleri yerel güçlenme döngüsüdür; her çıkışın zorunlu adımları değildir. Aşağıdaki eski ayrıntılı sıra zorunlu ilerleme zinciri değil, içerik/tempo referansıdır; altın ve meta geliştirmeler ertelenmiştir.

```
Tura Başla
    ↓
Arenaya Gir
    ↓
Düşman Dalgalarıyla Savaş
    ↓
Deneyim ve Altın Topla
    ↓
Seviye Atla
    ↓
Sandık Kazan ve Aç
    ↓
Yeni Bir Eşya Al veya Mevcut Eşyayı Geliştir
    ↓
Mevcut Build'i Güçlendir
    ↓
Elit Düşmanlarla Savaş
    ↓
Arena Boss'unu Yen
    ↓
Sonraki Arenaya İlerle
    ↓
Artan Zorlukla Döngüyü Tekrarla
    ↓
Son Boss'u Yen
    ↓
Son Ödülü Al
    ↓
Üsse Dön
    ↓
Kalıcı Geliştirmeler Satın Al
    ↓
Yeni Bir Tura Başla
```

---

## 9. Tur Yapısı

Bir tur, birbirine bağlı keşfedilebilir haritalardan oluşur. İlk prototipte her harita, ana ve isteğe bağlı yan bölgeler içeren bir Unity sahnesidir.

Bölgelerde düşman grupları veya wave tabanlı karşılaşmalar bulunur. Oyuncu isteğe bağlı savaşları atlayıp daha az güçlenerek çıkışa ulaşabilir. Tüm düşmanları temizlemek genel ilerleme şartı değildir; yalnızca açıkça karşılaşma şartlı tasarlanan geçişler tamamlanmayı bekler.

Aynı sahnedeki bağlantılar geri dönülebilir veya tek yönlü olabilir; oyuncuyu teleport etmek zorunda değildir. Girişten önce hazırlık, düşmanların kuruluşunu görüş dışında tutmalı ve performans ölçülmelidir. Harita çıkışı başka sahne yükleyebilir. Tek yönlü geçiş checkpoint anlamına gelmez. Prosedürel üretim veya zorunlu varyantlar yerine harita başına tek el yapımı düzenle başlanır.

Boss'lar planlanan aralıklarda ortaya çıkar. Nihai amaç son boss'a ulaşmak ve onu yenmektir.

Arena sistemi, deneyimin görsel ve mekanik olarak tekrara düşmesini engellemek için tasarlanmıştır. Farklı arenalar zamanla kendilerine ait yerleşimlere, tehlikelere, savaş koşullarına ve karşılaşma kimliklerine sahip olmalıdır.

Kesin arena, boss ve karşılaşma aşaması sayıları henüz belirlenmemiştir.

---

## 10. Oyuncu Ekipman Yapısı

Oyunda üç ana ekipman kategorisi bulunur:

1. **Silahlar:** Oyuncunun nişan aldığı ve manuel olarak ateşlediği şarjörlü ateşli silahlar.
2. **Yetenekler:** Otomatik olarak kullanılan savaş yetenekleri.
3. **Güçlendirmeler:** Pasif istatistik değişiklikleri veya oynanış davranışını etkileyen özellikler.

Güçlendirmeler tasarım tarafında istatistik güçlendirmeleri ve özellikler gibi alt sınıflara ayrılabilir. Ancak oyuncu açısından tek bir ortak güçlendirme slot havuzu kullanılacaktır. Ayrı istatistik ve özellik slotları oluşturulmayacaktır.

### 10.1 İçerik Havuzu

Şu anda planlanan içerik havuzu:

| Kategori | Planlanan Toplam Eşya |
| --- | --- |
| Silah | 4 |
| Yetenek | 8 |
| Güçlendirme | 10 |

Son on güçlendirme henüz kesin olarak seçilmemiştir.

### 10.2 Ekipman Slotları

Şu anda önerilen başlangıç slotları:

| Kategori | Başlangıç Slotu | Önerilen Maksimum |
| --- | --- | --- |
| Silah | 1 | 2 |
| Yetenek | 3 | 5 |
| Güçlendirme | 5 | 8 |

Ek slotlar kalıcı ilerleme sistemiyle açılabilir.

Bu değerler geçicidir ve oynanış testleriyle doğrulanmalıdır. Slot sınırları, oyuncunun yeterli çeşitliliği deneyimlemesini engellemeden anlamlı build kararları oluşturmalıdır.

---

## 11. Silahlar

Silahlar oyuncu tarafından aktif olarak kontrol edilir. Şarjör, mühimmat, yeniden doldurma süresi, isabet davranışı ve farklı etkili menzillere sahiptir.

Her temel silah şu anda sekiz seviyeden oluşmaktadır. Son seviyeye ulaşmak, diğer evrim şartları da sağlanmışsa silahı evrime uygun hâle getirir.

Kesin sayısal değerler dengeleme aşamasında belirlenecektir.

### 11.1 Shotgun

**Rol:** Yakın mesafe ani hasarı ve kalabalık kontrolü.

Shotgun geniş bir saçılım içinde birden fazla saçma ateşler. Yakındaki düşman gruplarına karşı çok etkilidir, ancak uzun mesafede güvenilirliği düşer.

| Seviye | Mevcut Tasarım |
| --- | --- |
| 1 | Temel Shotgun açılır |
| 2 | Hasar artar |
| 3 | Şarjör kapasitesi artar |
| 4 | Knockback artar |
| 5 | Hasar artar |
| 6 | Yeniden doldurma süresi azalır |
| 7 | Şarjör kapasitesi artar |
| 8 | Saçma sayısı artar ve evrime uygun hâle gelir |

İkinci knockback gelişiminin konumu dengeleme aşamasında yeniden değerlendirilebilir.

### 11.2 Minigun

**Rol:** Sürekli hasar ve nişan becerisine dayalı baskı.

Minigun yüksek hızla ateş eder. Nişanını hedef üzerinde tutabilen, tepme ve mühimmat yönetimini başarabilen oyuncuyu ödüllendirir.

| Seviye | Mevcut Tasarım |
| --- | --- |
| 1 | Temel Minigun açılır |
| 2 | Hasar artar |
| 3 | Kritik vuruş ihtimali artar |
| 4 | Şarjör kapasitesi artar |
| 5 | Ateş hızı artar |
| 6 | Hasar artar |
| 7 | Yeniden doldurma süresi azalır |
| 8 | Silah tepmesi azalır ve evrime uygun hâle gelir |

İsabet, saçılım toparlanması ve tepme gelişimi, ateş etme modeli prototiplendiğinde yeniden değerlendirilecektir.

### 11.3 Rocket Launcher

**Rol:** Uzak mesafeli alan hasarı ve kalabalık temizleme.

Rocket Launcher küçük bir şarjöre ve yavaş bir ateş döngüsüne sahiptir, ancak gruplanmış düşmanlara karşı güçlü patlamalar oluşturur.

| Seviye | Mevcut Tasarım |
| --- | --- |
| 1 | Temel Rocket Launcher açılır |
| 2 | Hasar artar |
| 3 | Roket hızı artar |
| 4 | Şarjör kapasitesi artar |
| 5 | Yeniden doldurma süresi azalır |
| 6 | Hasar artar |
| 7 | Patlama yarıçapı artar |
| 8 | Patlama şarapnel mermileri çıkarır ve silah evrime uygun hâle gelir |

Başlangıçtaki çift patlama fikri, daha belirgin görsel ve mekanik geri bildirim sağlamak için şarapnel sistemiyle değiştirilmiştir.

### 11.4 Plasma Rifle

**Rol:** Orta mesafeli sürekli hasar, delme ve yakma etkisi.

Plasma Rifle, düşmanların içinden geçebilen ve ilerleyen seviyelerde yakma hasarı uygulayan enerji mermileri ateşler.

| Seviye | Mevcut Tasarım |
| --- | --- |
| 1 | Temel Plasma Rifle açılır |
| 2 | Hasar artar |
| 3 | Ateş hızı artar |
| 4 | Mermiler bir düşmanın içinden geçebilir |
| 5 | Hasar artar |
| 6 | Şarjör kapasitesi artar |
| 7 | Mermiler iki düşmanın içinden geçebilir |
| 8 | Mermiler yakma uygular ve silah evrime uygun hâle gelir |

---

## 12. Yetenekler

Yetenekler kendilerine ait hedefleme, bekleme süresi, etki süresi ve davranış kurallarına göre otomatik olarak çalışır.

Her temel yetenek şu anda sekiz seviyeden oluşur. Son seviyeye ulaşmak, diğer şartlar da sağlanmışsa yeteneği evrime uygun hâle getirir.

### 12.1 Güç Dalgası

**Rol:** Kısa mesafeli kalabalık kontrolü ve acil güvenli alan oluşturma.

Oyuncunun baktığı yönde kısa menzilli bir güç dalgası çıkarır.

| Seviye | Mevcut Tasarım |
| --- | --- |
| 1 | Öne doğru bir güç dalgası açılır |
| 2 | Hasar artar |
| 3 | Etki alanı genişler |
| 4 | Bekleme süresi azalır |
| 5 | Knockback artar |
| 6 | Hasar artar |
| 7 | Etki alanı genişler |
| 8 | Bekleme süresi azalır ve evrime uygun hâle gelir |

### 12.2 Alev Topu

**Rol:** Otomatik uzak mesafe hasarı ve yakma baskısı.

Belirli menzil içindeki rastgele düşmanları hedefleyen alev topları oluşturur.

| Seviye | Mevcut Tasarım |
| --- | --- |
| 1 | Her kullanımda iki alev topu atılır |
| 2 | Hasar artar |
| 3 | Bekleme süresi azalır |
| 4 | Üç alev topu atılır |
| 5 | Hasar artar |
| 6 | Mermi hızı artar |
| 7 | Dört alev topu atılır |
| 8 | Yakma süresi artar ve evrime uygun hâle gelir |

### 12.3 Drone

**Rol:** Sürekli otomatik hedef baskısı.

Drone'lar oyuncunun yakınında kalır ve düşmanlara otomatik olarak ateş eder.

| Seviye | Mevcut Tasarım |
| --- | --- |
| 1 | Bir drone açılır |
| 2 | Hasar artar |
| 3 | Ateş hızı artar |
| 4 | Hedefleme menzili artar |
| 5 | Hasar artar |
| 6 | İkinci drone açılır |
| 7 | Ateş hızı artar |
| 8 | Boss hedeflerken ateş hızı artar ve evrime uygun hâle gelir |

### 12.4 Yıldırım Asası

**Rol:** Rastgele alan hasarı, sersemletme ve zincirleme hasar.

Belirli menzil içindeki rastgele düşmanlara veya noktalara yıldırım düşürür.

| Seviye | Mevcut Tasarım |
| --- | --- |
| 1 | Bir yıldırım düşer |
| 2 | Hasar artar |
| 3 | Bekleme süresi azalır |
| 4 | İki yıldırım düşer |
| 5 | Hasar artar |
| 6 | Yıldırım düşmanları sersemletir |
| 7 | Üç yıldırım düşer |
| 8 | Yıldırım bir ek düşmana zincirlenir ve evrime uygun hâle gelir |

### 12.5 Şuriken

**Rol:** Yakın çevre koruması ve tekrarlı temas hasarı.

Şuriken etkinleştiğinde oyuncunun çevresinde iki tam tur döner.

| Seviye | Mevcut Tasarım |
| --- | --- |
| 1 | Bir dönen şuriken açılır |
| 2 | Hasar artar |
| 3 | Dönüş hızı artar |
| 4 | Bekleme süresi azalır |
| 5 | Hasar artar |
| 6 | İkinci şuriken açılır |
| 7 | Etkili yarıçap artar |
| 8 | Kanama uygular ve evrime uygun hâle gelir |

### 12.6 Asit Şişesi

**Rol:** Alan kontrolü, zamanla hasar ve yavaşlatma.

Asit şişeleri rastgele hedeflerin üzerine düşer ve hasar veren alanlar oluşturur.

| Seviye | Mevcut Tasarım |
| --- | --- |
| 1 | Bir asit şişesi atılır |
| 2 | Asit hasarı artar |
| 3 | Alan süresi artar |
| 4 | Yarıçap artar |
| 5 | İki şişe atılır |
| 6 | Hasar artar |
| 7 | Alan süresi artar |
| 8 | Asit alanı düşmanları yavaşlatır ve evrime uygun hâle gelir |

### 12.7 Efsun Asası

**Rol:** Tahmin edilemeyen delici ışınlar ve savaş alanı kapsaması.

Rastgele yönlere ışınlar gönderir. Işınlar sınırlı bir süre hareket eder, düşmanların içinden geçer ve rastgele noktalarda yön değiştirir.

| Seviye | Mevcut Tasarım |
| --- | --- |
| 1 | Bir ışın oluşturulur |
| 2 | Hasar artar |
| 3 | Işın süresi artar |
| 4 | Bekleme süresi azalır |
| 5 | İki ışın oluşturulur |
| 6 | Hasar artar |
| 7 | Işın süresi artar |
| 8 | Bekleme süresi azalır ve evrime uygun hâle gelir |

### 12.8 Sniper Bomb

**Rol:** Öncelikli hedef hasarı ve hassas patlayıcı baskısı.

Sniper Bomb menzil içindeki bir hedefe kesin olarak güdümlenir. Elit düşmanlar ve boss'lar hedeflemede önceliklidir.

| Seviye | Mevcut Tasarım |
| --- | --- |
| 1 | Bir bomba fırlatılır |
| 2 | Hasar artar |
| 3 | Patlama yarıçapı artar |
| 4 | Bekleme süresi azalır |
| 5 | İki bomba fırlatılır |
| 6 | Hasar artar |
| 7 | İlk hedef ölürse bomba yeni bir hedefe yönelebilir |
| 8 | Patlama yarıçapı artar ve evrime uygun hâle gelir |

---

## 13. Güçlendirmeler

Güçlendirmeler oyuncunun genel istatistiklerini destekler veya çatışmaya yaklaşımını değiştiren özellikler sağlar.

Tüm güçlendirmeler aynı slot havuzunu kullanır. İstatistik güçlendirmesi ve özellik ayrımı yalnızca tasarım organizasyonu için kullanılır.

Hedef toplam on güçlendirmedir, ancak kesin liste henüz tamamlanmamıştır.

### 13.1 İstatistik Güçlendirmesi Adayları

| Güçlendirme | Mevcut Fikir |
| --- | --- |
| Hasar Artışı | Genel hasarı artırır |
| Zırh Artışı | Alınan hasarı azaltır |
| Maksimum Can | Maksimum canı artırır |
| Can Yenileme | Zaman içinde can yeniler |
| Toplama Mıknatısı | Toplanabilir nesnelerin çekim yarıçapını artırır |
| Deneyim Kazancı | Kazanılan deneyimi artırır |
| Bekleme Süresi Azaltma | Yetenek bekleme sürelerini azaltır; etkilenen sistemler kesinleşmemiştir |
| Hareket Hızı | Oyuncunun hareket hızını artırır |
| Saldırı Hızı | Uygun silahların ateş hızını artırır |

Yalnızca yakma hasarını artıran özel bir güçlendirme, çok fazla build için kullanışsız olabileceği için şu anda tercih edilmemektedir.

### 13.2 Özellik Güçlendirmesi Adayları

| Güçlendirme | Mevcut Fikir |
| --- | --- |
| Berserker | Can belirli bir sınırın altındayken hasarı artırır |
| Heavy Armor | Zırhı büyük ölçüde artırır ancak hareket hızını düşürür |
| Blood Pact | Maksimum can karşılığında hasar kazandırır |
| Glass Cannon | Dayanıklılık karşılığında büyük saldırı gücü sağlar |
| Lucky | Değerli sandık, nadirlik ve muhtemelen evrim ihtimallerini etkiler |
| Ammo Expert | Yeniden doldurma sırasındaki hareket cezasını kaldırır veya azaltır |
| Vampire | Düşman öldürerek can kazanma ihtimali veya yöntemi sağlar |

Bu özelliklerin sayısal değerleri, seviyeleri, birbirleriyle etkileşimleri ve kesin listeye girip girmeyecekleri henüz belirlenmemiştir.

---

## 14. Evrim Sistemi

Evrim, orijinal eşyanın ek bir seviyesi olarak değerlendirilmez. Orijinal eşyanın yerine aynı slota yerleşen yeni bir silah veya yetenektir.

Bu yaklaşım, evrimin orijinal fikri korumasına veya savaş davranışını tamamen değiştirmesine imkân verir.

Örnek:

```
Alev Topu
    +
Gerekli Güçlendirme
    ↓
Meteor Yeteneği
```

Temel Alev Topu oyuncudan mermi fırlatırken, evrim sonucu havadan meteorlar düşebilir. Evrim sonucu ayrı bir eşya olduğu için tamamen farklı hedefleme, görsel, hasar aktarımı, zamanlama ve davranış kullanabilir.

### 14.1 Mevcut Evrim Kuralları

- Kaynak silah veya yetenek maksimum seviyeye ulaşmalıdır.
- Bir veya daha fazla gerekli güçlendirme ya da koşul bulunabilir.
- Gerekli güçlendirmenin de maksimum seviyeye ulaşmasının gerekip gerekmediği kesinleşmemiştir.
- Evrimleşmiş eşya kaynak eşyanın aynı slotuna yerleşir.
- Evrimleşmiş eşyaların şu anda ek seviyeleri bulunmaz.
- Evrimler uygun sandıklardan sunulur.
- Birden fazla eşya evrime uygunsa evrim ödülü oluşturulurken uygun sonuçlardan biri rastgele seçilir.

---

## 15. Sandık Sistemi

Oyunda şu anda sekiz sandık türü bulunmaktadır.

Sandık ödülleri şunları içerebilir:

- Uygun bir slot varsa yeni silah, yetenek veya güçlendirme.
- Maksimum seviyeye ulaşmamış sahip olunan bir eşyanın seviye gelişimi.
- Sandık destekliyorsa uygun bir evrim.
- Yalnızca anlamlı bir ekipman ödülü oluşturulamıyorsa altın.

Maksimum seviyedeki eşyalar normal ödül havuzundan çıkarılır.

Geçerli bir ödül varken altın onun yerine verilmez. Altın yalnızca ilgili tüm slotlar doluysa ve ilgili havuzdaki tüm sahip olunan eşyalar maksimum seviyedeyse kullanılır.

### 15.1 Silah Sandığı

- Üç seçenek sunar.
- Silah veya silah seviyesi içerir.
- Sıradan sandıktır.
- Gri renkle temsil edilir.

### 15.2 Yetenek Sandığı

- Üç seçenek sunar.
- Yetenek veya yetenek seviyesi içerir.
- Sıradan sandıktır.
- Gri renkle temsil edilir.

### 15.3 Güçlendirme Sandığı

- Üç seçenek sunar.
- Güçlendirme veya güçlendirme seviyesi içerir.
- Sıradan sandıktır.
- Gri renkle temsil edilir.

### 15.4 Yeşil Sandık

- Üç seçenek sunar.
- Oyuncu birini seçer.
- Silah, yetenek veya güçlendirme içerebilir.
- Uygun evrim varsa seçeneklerden birinin evrim olma ihtimali %25'tir.
- Sıradan olmayan nadirliktedir.
- Yeşil renkle temsil edilir.

### 15.5 Mor Sandık

- Dört seçenek sunar.
- Oyuncu birini seçer.
- Silah, yetenek veya güçlendirme içerebilir.
- Uygun evrim varsa seçeneklerden birinin evrim olma ihtimali %50'dir.
- Ender nadirliktedir.
- Çalışma adı “Mor Sandık” olmasına rağmen mevcut görsel rengi mavidir. İsimlendirme yeniden değerlendirilecektir.

### 15.6 Efsanevi Sandık

- Beş seçenek sunar.
- Oyuncu ikisini seçer.
- Silah, yetenek veya güçlendirme içerebilir.
- Uygun evrim varsa seçeneklerden birinin evrim olma ihtimali %75'tir.
- Efsanevi nadirliktedir.
- Mor renkle temsil edilir.

### 15.7 Boss Sandığı

- Yalnızca öldürülen boss'lardan düşer.
- Altı seçenek sunar.
- Oyuncu ikisini seçer.
- Silah, yetenek veya güçlendirme içerebilir.
- Uygun bir evrim varsa en az bir seçenek kesinlikle evrim olur.
- Efsanevi seviyede bir ödüldür.
- Mor renkle temsil edilir.

### 15.8 Altın Sandık

- Yalnızca son boss yenildiğinde düşer.
- Büyük miktarda altın içerir.
- Normal çoklu seçim ekranını kullanmaz.
- Ödül alındıktan sonra oyuncuya üsse dönme seçeneği sunulur.
- Kesin altın miktarı henüz dengelenmemiştir.

---

## 16. Deneyim ve Seviye Sistemi

Düşmanlar öldürüldüğünde deneyim nesneleri düşürür.

Yeterli deneyim toplandığında oyuncunun mevcut tur seviyesi artar. Tur seviyesi, tur bittiğinde sıfırlanır.

Her seviye atlamada kesin olarak bir sandık kazanılır. Sandık türü ağırlıklı bir olasılık tablosuyla belirlenir.

Normal ve elit düşmanlar da sandık düşürebilir. Sandık düşürme ihtimalleri ve sandık türü ağırlıkları iki düşman türü için ayrı hesaplanır.

Boss'lar kendilerine ait Boss Sandığı ödülünü kullanır. Son boss ayrıca Altın Sandık sağlar.

Deneyim sistemi kısa vadeli ilerlemenin ana kaynaklarından biridir ve oyuncuya sık sık bir sonraki ödüle yaklaştığını hissettirmelidir.

---

## 17. Düşmanlar ve Karşılaşmalar

Arenalarda farklı hareket, saldırı, menzil ve dayanıklılık kalıplarıyla oyuncuya baskı uygulayan düşman dalgaları bulunur.

Projede şunlar yer alacaktır:

- Normal düşmanlar.
- Elit düşmanlar.
- Arena boss'ları.
- Son boss.

Düşman aileleri, kesin sayılar, doğma kuralları ve boss mekanikleri henüz tasarlanmamıştır.

Düşman çeşitliliği yalnızca daha fazla can ve hasar yerine taktiksel farklılıklar oluşturmalıdır.

---

## 18. Altın ve Tur Ekonomisi

Altın, ikinci bir ilerleme çubuğu yerine basit bir UI sayacıyla gösterilmelidir.

Düşmanlar tur sırasında altın düşürebilir. Oyuncu tur bitene kadar tur altını biriktirir.

Oyuncu öldüğünde toplanan altının yalnızca bir kısmı korunur.

Şu anda önerilen kalıcı geliştirme yapısı:

| Geliştirme Seviyesi | Ölümde Korunan Altın |
| --- | --- |
| Varsayılan | %40 |
| Seviye 2 | %50 |
| Seviye 3 | %60 |
| Seviye 4 | %70 |

Planlanan maksimum koruma oranı şu anda %70'tir.

Son boss'un başarıyla yenilmesi durumunda tur ödüllerinin korunması ve ek Altın Sandık ödülü verilmesi beklenmektedir. Ara boss'lardan sonra altının bir kısmının güvenceye alınıp alınmayacağı açık sorudur.

---

## 19. Kalıcı İlerleme

Tur sona erdikten sonra oyuncu üs veya ana ilerleme alanına döner.

Kalıcı altın uzun vadeli geliştirmeler için harcanabilir.

Kesinleşen veya şu anda önerilen geliştirmeler:

- Ek silah slotları.
- Ek yetenek slotları.
- Ek güçlendirme slotları.
- Ölüm sonrasında korunan altın yüzdesinin artırılması.

Daha sonra ek kalıcı geliştirmeler oluşturulabilir. Ancak kalıcı ilerleme, oyuncunun becerisini veya build seçimlerinin önemini ortadan kaldırmamalıdır.

---

## 20. Kullanıcı Arayüzü İlkeleri

Arayüz, birinci şahıs görüşünü gereğinden fazla kapatmadan kritik bilgileri aktarmalıdır.

Beklenen temel HUD öğeleri:

- Can.
- Mühimmat ve şarjör durumu.
- Deneyim çubuğu ve tur seviyesi.
- Altın sayacı.
- Gerekiyorsa yetenek durumu veya bekleme süresi geri bildirimi.
- Uygun karşılaşmalarda boss canı.
- Etkileşim ve sandık bildirimleri.

Gelecekteki testler zorunlu olduğunu göstermedikçe ayrı bir altın ilerleme çubuğu bulunmayacaktır.

Sandık ödül ekranında şunlar açıkça gösterilmelidir:

- Eşya adı.
- Eşya kategorisi.
- Mevcut veya yeni seviye.
- Oynanış etkisi.
- Ödülün yeni eşya, seviye gelişimi veya evrim olup olmadığı.
- Kalan seçim hakkı.

---

## 21. Ses ve Görsel Geri Bildirim

Çatışma geri bildirimi deneyimin temel parçalarından biridir.

Silahlar, yetenekler, güçlendirmeler, evrimler, hasar, öldürmeler, seviye atlama, sandık açma, elit karşılaşmalar ve boss'lar birbirinden ayırt edilebilir geri bildirimlere sahip olmalıdır.

Görsel efektler gücü desteklemeli, ancak düşmanları veya tehlikeleri gizlememelidir. Önemli düşman saldırıları ve tehlikeli zemin etkileri yoğun çatışma sırasında okunabilir kalmalıdır.

---

## 22. Mevcut Kapsam

Şu anda planlanan kapsam:

- Tek oyunculu birinci şahıs çatışma.
- Şarjörlü silahlar.
- Otomatik yetenekler.
- Pasif ve özellik tabanlı güçlendirmeler.
- Ekipman seviyeleri.
- Ekipman evrimleri.
- Deneyim ve tur içi seviye.
- Sekiz sandık türü.
- Normal düşmanlar, elit düşmanlar, boss'lar ve son boss.
- Arena tabanlı ilerleme.
- Tur altını ve kalıcı gelişim.
- Açılabilir ekipman slotları.
- Turlar arasında üs veya gelişim alanı.

---

## 23. Kapsam Dışı

Şu anda planlanmayan özellikler:

- Çevrimiçi çok oyunculu oyun.
- Rekabetçi çok oyunculu oyun.
- İş birliğine dayalı çok oyunculu oyun.
- Açık dünya keşfi.
- Geleneksel üretim sistemi.
- Oyuncular arası ticaret.
- Prosedürel olarak oluşturulmuş açık dünya.
- Ayrı istatistik ve özellik güçlendirmesi slotları.

Özellikler yalnızca temel tasarım ilkelerini açıkça destekliyor ve projenin gerçekleştirilebilir kapsamını tehlikeye atmıyorsa yeniden değerlendirilebilir.

---

## 24. Tasarımla İlgili Teknik Kısıtlar

- Proje Unity 6.3.9 ile geliştirilecektir.
- Unity Version Control / Plastic SCM kullanılacaktır.
- Proje veri odaklı içerik üretimini desteklemelidir.
- Uygun içerik verilerinin tanımlanmasında ScriptableObject kullanılması beklenmektedir.
- Oynanış tasarımı gereksiz teknik soyutlamalara bağlı olmamalıdır.
- Sistemler açık sorumlulukları, sürdürülebilirliği, genişletilebilirliği ve test edilebilirliği desteklemelidir.
- Mimari kararların esas yeri bu belge değil, Teknik Tasarım Dokümanı olacaktır.

---

## 25. Açık Sorular

- Projenin kesin adı ne olacak?
- Başarılı bir tur ortalama ne kadar sürmeli?
- Tam bir tur kaç arena içermeli?
- Boss'lar hangi sıklıkta ortaya çıkmalı?
- Kaç farklı düşman ailesi bulunmalı?
- Son güçlendirme havuzunu hangi on güçlendirme oluşturmalı?
- Her güçlendirme kaç seviyeden oluşmalı?
- Hangi silah, yetenek ve güçlendirmeler evrim kombinasyonları oluşturmalı?
- Gerekli evrim güçlendirmesinin de maksimum seviyeye ulaşması zorunlu olmalı mı?
- Bir eşyanın birden fazla olası evrimi bulunabilir mi?
- Ara boss'lar mevcut tur altınının bir kısmını kalıcı olarak güvenceye almalı mı?
- Son sandık düşürme ihtimalleri ne olacak?
- Şans etkileri sonrasında evrim ihtimalleri nasıl hesaplanacak?
- Lucky güçlendirmesi farklı olasılık sistemlerini nasıl etkileyecek?
- Slotlar ve altın koruması haricinde hangi kalıcı geliştirmeler bulunacak?
- Arenalar elle mi hazırlanacak, rastgele mi seçilecek veya dallanan bir yol üzerinden mi sıralanacak?
- Arena değiştiricileri ilk sürümde bulunacak mı?
- Bir sandık yeterli sayıda benzersiz ve geçerli seçenek oluşturamazsa ne olacak?
- Zorluk arenalar arasında ve tur boyunca nasıl artacak?
- Kesin ölüm, zafer ve üsse dönüş akışı nasıl olacak?
- Efektler, kamera hareketi ve savaş okunabilirliği için hangi erişilebilirlik ayarları gerekli olacak?

---

## 26. İlk Karar Kaydı

### GDD-001 — Aktif Silahlar ve Otomatik Yetenekler

**Karar:** Silahlar oyuncu tarafından manuel kontrol edilirken yetenekler otomatik çalışacaktır.

**Gerekçe:** Bu birleşim FPS becerisini korurken build'in giderek daha güçlü otomatik savaş etkileri oluşturmasını sağlar.

### GDD-002 — Arena Tabanlı Tur Yapısı

**Karar (15 Eylül 2026 güncellemesi):** Turlar, isteğe bağlı bölgeler ve seçili karşılaşma kilitleri içeren keşfedilebilir haritalarda ilerler. Rotayı ilerleten genel koşul wave temizlemek değil uygun çıkışa ulaşmaktır. Bkz. [aşamalı teknik plan](../TDD/TR/Map-Region-Encounter-Plan.md).

**Gerekçe:** Özel kilitli karşılaşmaları ve harita kimliklerini korurken keşif ile daha hızlı fakat daha az güçlenilen rotalar arasında seçim sunmak.

### GDD-003 — Ortak Güçlendirme Slotları

**Karar:** İstatistik güçlendirmeleri ve özellikler aynı güçlendirme slot havuzunu kullanacaktır.

**Gerekçe:** Ayrı slot kategorileri gereksiz arayüz ve envanter karmaşıklığı oluşturacaktır.

### GDD-004 — Evrimin Eşya Değişimi Olması

**Karar:** Evrimleşmiş bir silah veya yetenek, kaynak eşyanın aynı slotuna yerleşen ayrı bir eşya olacaktır.

**Gerekçe:** Evrimler hedeflemeyi, hasar biçimini, sunumu ve davranışı tamamen değiştirebilir.

### GDD-005 — Anlamlı Ödül Filtreleme

**Karar:** Maksimum seviyedeki eşyalar sandık ödül havuzundan çıkarılacaktır. Altın yalnızca geçerli ekipman ödülü kalmadığında sunulacaktır.

**Gerekçe:** Maksimum seviyeye ulaşmak sonraki ödüllerin kalitesini düşürmemeli veya oyuncuyu cezalandırmamalıdır.

### GDD-006 — Olasılıklı Evrimler

**Karar:** Yeşil, Mor ve Efsanevi Sandıklar evrimi garanti etmek yerine farklı evrim ihtimalleri kullanacaktır.

**Gerekçe:** Evrimler her değerli sandıktan gelen tahmin edilebilir ödüller yerine heyecan verici ve değerli kalmalıdır.

### GDD-007 — Basit Altın Arayüzü

**Karar:** Altın ayrı bir ilerleme çubuğu yerine sayaçla gösterilecektir.

**Gerekçe:** HUD zaten can, mühimmat, deneyim ve çatışma geri bildirimi içermektedir. İkinci bir ilerleme çubuğu gereksiz görsel yük oluşturacaktır.

### GDD-008 — Profesyonel Sadelik

**Karar:** Proje açık ve sürdürülebilir çözümleri önceliklendirecek; gereksiz soyutlamalardan ve referanslardan kaçınacaktır.

**Gerekçe:** Bir sistem yalnızca çalışmamalı; aynı zamanda anlaşılır, sürdürülebilir ve projenin gerçek ihtiyaçlarına uygun olmalıdır.

---

## 27. Sürüm Geçmişi

| Sürüm | Tarih | Değişiklikler |
| --- | --- | --- |
| v0.1 | 31.07.2026 | İlk temel GDD oluşturuldu. Vizyon, tasarım ilkeleri, temel döngü, ekipman yapısı, mevcut silah ve yetenek tasarımları, güçlendirme adayları, evrim kuralları, sandık sistemi, ekonomi, kalıcı ilerleme, kapsam, açık sorular ve ilk tasarım kararları belgelendi. |
