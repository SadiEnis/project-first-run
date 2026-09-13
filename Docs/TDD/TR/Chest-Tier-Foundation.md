# Sandık seviyesi temeli

## Başlangıç ve kapsam

Oyuncunun düşman ölümü drop'larını doğrulamasından sonra birleştirilmiş Plastic `/main/dev` **cs:154**, Git `main` **204745f** tabanından başlar. Bu aşama GDD bölüm 15'teki sekiz sandık türünün uygulanmasından önceki temeldir; türlerin tamamlandığı anlamına gelmez.

`ChestRarity`, `ChestDefinition` üzerinde **Common, Uncommon, Rare, Legendary** bilgisini açık 0–3 değerleriyle tutar. Eski geliştirme sandığında alanın yokluğu Common kabul edilir; bilinmeyen enum değerleri reddedilir. Nadirlik bir veri etiketidir; sayısal eşya seviyesi, düşme ihtimali, ödül kategorisi veya otomatik kalite çarpanı değildir. Weapon/Ability/Upgrade sandıkları aynı Common seviyesini paylaşabilir. Boss, legendary-tier olsa da ayrı ödül politikasına sahiptir. Golden Chest için uydurma beşinci nadirlik yerine özel altın ödülü akışı gerekir.

## Ortak ağırlıklı seçim

`ChestDropTable`, pozitif tam sayı ağırlıklarla `ChestDefinition` referanslarını tutan ScriptableObject'tir. **Hangi sandık?** sorusunu yanıtlar; **sandık düşecek mi?** kararını vermez. Seçim, toplam ağırlık üzerinden tek `IRandomSource.Next(0, totalWeight)` çağrısı ve üst sınırı hariç aralıklarla yapılır. Ağırlıkların toplamı 100 olmak zorunda değildir; nadirlik ağırlıkları gizlice değiştirmez. Boş/eksik tablo, eksik tanım, pozitif olmayan ağırlık, geçersiz sandık, taşan toplam ve aralık dışı rastgele değer açıkça reddedilir. Asset üzerinde değişken rastgelelik durumu tutulmaz.

`EnemyChestDropProfile`, on binde bir ihtimalini korur ve tabloya referans verir. Sıfır ihtimalde tablo gerekmez; pozitif ihtimalde geçerli tablo zorunludur. Ölüm ihtimali ve sandık seçimi ayrı çekilişlerdir. Seçilen tanım mevcut cesetten bağımsız kuyrukta saklanır; tekrar denemelerinde yeniden çekilmez.

`LevelUpChestSource` artık tek tanım yerine tablo kullanır. Her level yine bir sandığı garantiler; level-up için düşme ihtimali çekilişi yoktur. Seçim XP callback'i dışında, ilk uygun işleme denemesinde yapılır. Engelli konum, pause/ölüm, disable/enable ve spawn hatasında seçilen tanım korunur. Yalnızca başarılı spawn sonrasında temizlenir; sıradaki level hakkı kendi çekilişini yapar. Seçilen tanımın yok edilmesi yeniden çekiliş izni değil, hatadır. Başlatma sırasında tablodaki bütün prefabların placement sözleşmesi doğrulanır.

Mevcut tracker sandık haklarının sayısını yönetmeye devam eder. Kaynak yalnızca sıradaki seçimi tutar. Oyuncuya göre konumlandırma, sınırlı işleme, modal davranışı ve tek seçimle tamamlanma değişmez.

## Veri geçişi ve korunan oynanış

- `CDT_DevelopmentLevelUp`, `CDT_DevelopmentNormal`, `CDT_DevelopmentElite` gelecekte ağırlıklarının bağımsız ayarlanabilmesi için **ayrı asset'lerdir**. Şimdilik hepsi yalnızca `CD_DevelopmentChest` ve ağırlık 1 içerir.
- İki düşman profilinin eski gömülü girişleri ilgili tablo referanslarına taşınır. Geçici **%25 / %50** ihtimalleri korunur; elit örneği hâlâ sahnede üretilen elit içerik değildir.
- `Test_Waves` level-up kaynağının tek sandık referansı level-up tablosuna taşınır. Başlangıç sandığı doğrudan geliştirme tanımını kullanmaya devam eder.
- Takip edilen profiller, sahne ve test bağlantıları birlikte güncellenir. Eski gömülü alanı kullanan takip dışı özel profiller için elle tablo oluşturulup atanmalıdır; eksik yapılandırmayı gizleyen çalışma zamanı telafisi yoktur.
- Renkli yeni prefablar, ödül havuzları, düşme sıklığı dengesi ve rank bazlı düşman davranışı eklenmez. Mevcut sandıkların görünümü ve davranışı bu aşamada aynıdır.

## Sonraki sandık türü aşaması

Önce üç Common kategori sandığı (Weapon, Ability, Upgrade), ardından gerekli ödül altyapıları hazır oldukça karma Green/Purple ve Legendary/Boss politikaları uygulanır. Nadirlik, kategori filtresi, sunulan seçenek sayısı, yapılabilen seçim sayısı, evrim politikası ve sunum ayrı tutulur. GDD'deki “Purple Chest” çalışma adı şu anda Rare ve planlanan mavi görsel anlamındadır; Legendary mordur. Renk veya addan kural çıkarılmaz.

Eşya seviyesi ödülleri, evrim, iki seçimli oturumlar, altın telafisi ve Golden Chest henüz uygulanmamıştır. Nadirlik etiketinin eklenmesi bu davranışların çalıştığı anlamına gelmez.

## Doğrulama

Otomatik testler nadirlik doğrulaması, ortak tablo aralıkları/geçersiz veriler, korunan düşman ihtimali sınırları, bağımsız kaynak tabloları, çoklu level için ayrı çekiliş, konum hatası/disable sonrasında seçim korunması ve mevcut XP/ölüm/spawn/sandık etkileşim regresyonlarını kapsar.

Elle `Test_Waves` testi aynı görünmelidir: XP çekilir, level sandığı garantidir, bazı düşman ölümleri ek sandık üretir, E ile uygun ödül açılıp seçilir. Inspector'da geliştirme tanımının Rarity alanı ve üç bağımsız tablo incelenebilir. Yalnızca nadirliği değiştirmek düşme ihtimalini veya ödül davranışını değiştirmemelidir. Gerçek test sonuçları README'ye kaydedilir; bu temelin elle onayı, doğrulanmış düşman drop aşamasından ayrıdır.
