# XP düşürme ve toplama

## Kapsam

Bu çalışma aynı XP branch'inde [XP ve Seviye Temeli](Experience-Level-Foundation.md) aşamasını genişletir. Düşman ölümü dünyaya XP bırakır, oyuncu yakına giderek bunu bir kez toplar ve Test_Waves run ilerlemesini gösterir. Level-up kaynaklı sandık üretimi sonraki aşamadır; bu katman sandık üretmez.

## Sorumluluklar ve akış

- `EnemyDefinition.ExperienceReward` negatif olamaz; sıfır, XP düşürülmemesi demektir. Geliştirme chaser'ı 25 XP verir; geçici denge değeridir.
- `EnemyExperienceDrop`, initialize edilmiş düşmanın gerçek `Died` olayında ölüm konumunun biraz üzerinde tek pickup oluşturur ve ödül miktarını ona kopyalar. Yalnızca disable/destroy edilmek XP vermez. Mevcut ölüm sonrası yok etme akışına uygun olarak pooling/yeniden initialize etme henüz desteklenmez.
- `ExperiencePickup`, miktarı ve tek kullanımlık tüketim durumunu tutar. Trigger yalnızca `PlayerExperienceCollector` kabul eder. Kinematic Rigidbody oyuncunun CharacterController'ıyla teması sağlar; Ignore Raycast katmanı ateş ve sandık etkileşimini engellemez.
- `PlayerExperienceCollector`; XP initialization, component etkinliği, hayatta olma ve pozitif time scale koşullarını denetler. Reddedilen toplama pickup'ı tüketmez. Trigger stay, temas sürerken yeniden uygun hale gelen oyuncunun toplamasını sağlar. Tuşla toplama, zaman aşımı, pooling ve sahneler arası kalıcılık kapsam dışıdır.
- XP bildirimlerinden önce tüketim rezerve edilerek tekrar/reentrant toplama engellenir. XP işlenmeden hata oluşursa rezervasyon bırakılır; XP işlendikten sonra dinleyici hatası pickup'ı yeniden kullanılabilir yapmaz. Temel katmandaki dinleyicilerin hata fırlatmama sözleşmesi geçerlidir.
- `ExperienceRunBootstrap`, Awake sırasında açık referanslı eğri asset'inden yeni run state oluşturup sahnedeki oyuncuya bağlar. Gelecekte run koordinatörü bu bootstrap'ı değiştirir; controller mevcut bir state'i kabul etmeye devam eder.
- `ExperienceDebugPresenter`, Editor-only salt okunur level/mevcut/gereken/toplam XP göstergesi ve ilerleme çubuğudur. Diğer geliştirme panelleriyle tutarlıdır; nihai HUD tasarımı değildir.

## Bağlantılar ve doğrulama

### Çekim genişletmesi

Collector her fizik adımında yakın pickup collider'larını tekrar kullanılan overlap tamponuyla tarar. Tampon dolarsa sonuçları atlamak yerine büyütür. Varsayılan dünya ölçeğindeki çekim yarıçapı **3 metre**, merkezi oyuncudaki yerel `(0, 0.75, 0)` offset'idir. Yalnızca collider teması değil, pickup merkezinin menzilin içinde olması aranır. Ignore Raycast pickup katmanı açıkça taranır; ilgisiz collider'lar yok sayılır.

Menzildeki küreler kinematic Rigidbody ile oyuncunun güncel toplama noktasına **8 metre/saniye** hızla ilerler. Hedefi aşmamak için hareket sınırlandırılır. Menzile girmek anında XP vermez; hedefe varış veya normal temas mevcut tek seferlik toplama sözleşmesini kullanır. Kalıcı hedef kilidi yoktur: menzilden çıkınca çekim durur, geri girince devam eder. Ölüm, devre dışı component'ler, XP'nin initialize edilmemiş olması ve duraklatma çekimi de engeller. Fiziksel engel oluşturmayan XP küreleri için görüş hattı/engel aşma hesabı yapılmaz.

`PlayerExperienceCollector.SetAttractionRadius`, sonlu ve negatif olmayan değer kabul eder; değişiklik mevcut drop'lara bir sonraki fizik adımında uygulanır. Sıfır çekimi kapatır, normal temasla toplamayı korur. Menzil ve toplama noktası Inspector'dan ayarlanır; collector seçildiğinde menzil gizmo'su görünür. Uçuş hızı pickup prefab'ında tutulur. Gelecekte geliştirmeler hesaplanan menzili bu sınıra aktarabilir; bu adımda yeni stat türü, modifier veya upgrade eklenmez.

Çekim testleri: tam menzil/collider kenarı sınırı, çalışma zamanında menzil değişimi, sıfır/geçersiz değerler, XP verilmeden önce uçuş, menzile yaklaşma/çıkma/yeniden girme, uçuşta ölüm, pause/disable engeli ve başlangıç tamponundan büyük XP yığınları.

Player prefab'ı: XP controller ve collector. Chaser prefab'ı: pickup prefab referanslı drop adapter. Test_Waves: run bootstrap ve geliştirme göstergesi. Eğri asset'i: başlangıçta 100, sonraki her level için +50 XP. Diğer sahneler kendiliğinden XP run'ı başlatmaz.

Otomatik testler: geçersiz değerler, tek seferlik toplama, ilgisiz collider, ölü/initialize edilmemiş/duraklatılmış/etkin olmayan oyuncu, temas sırasında devam etme, çoklu level, dinleyici hatası, ölüm ile disable/destroy ayrımı ve asset/sahne bağlantıları. Bağlantı değişikliklerinden sonra tüm EditMode ve PlayMode testleri çalıştırılır.

Manuel Test_Waves kontrolü: toplama menzili dışında chaser öldür, sabit duran camgöbeği küreyi gör, merkezine 3 metre yaklaştığında sana doğru uçmasını ve ulaştığında 25 XP vermesini doğrula. Dört küreyle level 2 olur; sonraki level 150 XP ister. Sandık ödülleri ve silah değiştirmek XP'yi korur. Ölümden sonra uçuş ve toplama durur. Level atlamak henüz sandık oluşturmaz.
