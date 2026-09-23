# İçerik test arenası

## Durum ve amaç

Tasarım checkpoint'i — 24 Eylül 2026. Branch: Plastic `/main/dev/content-test-arena`, Git `feature/content-test-arena`; birleştirilmiş harita temelinden açıldı. Bu kayıt yalnızca sözleşmedir: sahne ve test paneli henüz uygulanmadı.

Amaç, her yeni silah/yeteneği gerçek oyuncu, düşman ve ilerleme sistemleriyle tekrar edilebilir şekilde denemektir. Bu sahne final demo haritası veya yeni bir run yöneticisi değildir. Her silah/yetenek uygulamasından sonra kullanıcı oynanış değerlendirmesi için durulur.

## Sahne sözleşmesi

- Hedef: `Assets/_Project/Scenes/Tests/Test_ContentArena.unity`. Mevcut PlayableMapFixture, PlayableMapDestination ve Test_Waves değiştirilmez. Mevcut prefab ve servisler kullanılır; paralel bir combat/ödül sistemi yazılmaz.
- Zemin, sınır duvarları, yakın/orta/uzak mesafe işaretleri, spawn noktaları, NavMesh ve component bağlantıları Editor'de hazırlanıp sahnede kaydedilir. Play sırasında geometri, servis hiyerarşisi veya eksik Inspector referansları otomatik üretilmez. Düşman, mermi, XP ve sandık gibi dinamik oyun nesneleri normal runtime akışında oluşur.
- Bir açık mücadele alanı ve ona bağlı, düşman doğmayan bir başlangıç/test bölmesi yeterlidir. İlk sürümde bölge geçidi, dungeon veya sahne çıkışı gerekmez. Başlangıç bölmesi dokunulmazlık sağlamaz.
- Tek oyuncu, tek düşman registry'si, tek XP başlangıcı ve tek ödül UI/servis grubu bulunur. Geçiş fixture'ındaki bağlantı yaklaşımı kullanılır; servisler iki kez başlatılmaz.
- Chaser, Charger ve Ranger prefab/tanımları atanır. Inspector'dan ayarlanabilen küçük bir başlangıç grubu gerçek hedef bulma, navigasyon ve saldırıyla çalışır. Tek aile veya karışık grup için tekrar üretme komutları bulunur. Gruplar sınırsız üst üste birikmez.
- Oyuncu mevcut başlangıç loadout'u ile başlar. Başlangıçta ücretsiz yetenek, upgrade veya otomatik ödül seçimi verilmez. Başlangıç düşman sayıları ve mesafeler test verisidir; denge kararı değildir.

## Gerçek ilerleme ve ödül akışı

- Düşman ölümü mevcut hasar/ölüm hattını kullanır; XP ve atanmış düşman sandık drop profilleri normal kurallarla çalışır. Rastgele drop'un her ölümde gerçekleşmesi beklenmez.
- XP nesneleri mevcut mesafeden çekim/toplama sistemiyle alınır; mevcut eğri ile level atlanır ve level-up sandığı oluşur.
- Sandık etkileşimi gerçek ödül seçimi ekranını açar. Yeni eşya edinimi, sahip olunan eşyanın level-up'ı, slot sınırı, maksimum seviye, kalan seçim hakkı ve duraklatma davranışı korunur.
- Mevcut silah/yetenek/upgrade ve gelişmiş sandık tanımları atanabilir. Gold/evolution kapsamı açılmaz; bu sahne yeni evolution asset'i veya gold fallback üretmez.

## Geliştirme paneli

- Panel normal oyun HUD'ından ayrı, açılıp kapatılabilir bir test aracıdır. Açıldığında imleç serbest kalır ve hareket/ateş/yetenek kontrolü durur; kapanışta ölüm, ödül UI'ı ve önceki kontrol durumu gözetilir. Panelin girdi veya duraklatma sahipliği diğer UI'ı yanlışlıkla açmamalıdır.
- Panel/ödül ekranı aynı anda yönetilmez. Ödül seçimi açıkken test mutasyonları ve düşman grubu yenileme reddedilir. Ölü veya henüz başlatılmamış oyuncuyla eşya/XP/grup komutları çalışmaz.
- Açıkça atanmış eşya kataloğundan edinme ve sahip olunan eşyanın seviyesini bir artırma yapılabilir. Bunlar gerçek edinim/seviye controller'larını kullanır; build listelerini veya asset'leri doğrudan değiştirmez. Dolu slot ve maksimum seviye sonuçları panelde açıklanır. Yeni içerik paketi kendi katalog kaydını ve gerekiyorsa runtime factory bağlantısını ekler; henüz uygulanmamış eşya listelenmez.
- XP ekleme normal `GainExperience` hattını kullanır; level-up sandığı üretmesi beklenen davranıştır. Belirli mevcut sandık türünü üretme gerçek ChestSpawner hattını kullanır; böylece rastgele drop beklemeden ödül akışı denenebilir.
- Düşman grubunu yenilemek yalnızca bu test grubunun yaşayan örneklerini kaldırıp seçili grubu kurar. Test amaçlı kaldırma kill/XP/drop ödülü üretmez. Daha önce kazanılmış XP/sandıklar, oyuncu canı, mermisi ve build'i sıfırlanmaz.
- Son komut sonucu ve test yönergeleri görünürdür. Can, mermi, tur seviyesi/XP ve eşya seviyesi okunabilir şekilde gösterilir; gereksiz üst üste debug panelleri eklenmez.
- Bu panel tam run reset, dirilme veya save/load aracı değildir. Baştan temiz deneme için Editor'de Play durdurulup yeniden başlatılır.

## Doğrulama ve teslim sınırı

1. EditMode: kayıtlı sahne/prefab/asset referansları, tekil servisler, düşman tanımları, spawn noktaları, baked NavMesh ve katalog geçerliliği.
2. PlayMode: kayıtlı sahneden başlatma; başlangıç loadout'u ve gereksiz ödül olmaması; üç düşman ailesinin doğru kurulması ve navigasyonu.
3. Gerçek düşman ölümü → pickup → XP → level → sandık → ödül edinimi/level-up zinciri; seçimin duraklatma/kontrol davranışı.
4. Panel edinimi/seviye artırma, dolu slot/maksimum seviye, geçersiz istekler ve canlı runtime durumunun korunması.
5. Grup yenilemede registry/nesne temizliği ve ücretsiz kill ödülü olmaması; mevcut drop/build'in korunması.
6. Panel ile ödül UI'ının çakışmaması, ölüm sırasında kontrolün yanlışlıkla açılmaması ve tekrarlı aç/kapat güvenliği.
7. Kullanıcı sahnede hareket/savaş, drop toplama, sandık seçimi ve doğrudan içerik deneme kontrollerini değerlendirir. Bu kabul gelmeden Shotgun geliştirmesine geçilmez.

Editor odaklı bu teslim Windows build incelemesini yeniden başlatmaz ve standalone kabul iddiası taşımaz. Normal geliştirme için EditMode/PlayMode testleri agent tarafından çalıştırılıp sonuçları raporlanır; bu tasarım checkpoint'i için yeni test çalıştırılmaz.

## Kayıt düzeni

Önce bu EN/TR sözleşme ve indeksler için docs check-in/commit; ardından sahne/panel/testleri kapsayan anlamlı uygulama checkpoint'i. Kullanıcı merge işlemlerini yapar; GitHub'da bu branch tamamlandığında ilk PR üzerinden inceleme/merge akışı önerilir. PR açılması ayrı bir işlemdir ve henüz yapılmadı. Sonraki silah/yetenek branch'i güncel dev/main'den açılır.
