# Çoklu arena run temeli

## Genel geçiş adımı (uygulama sözleşmesi)

`ArenaTransitionTrigger` mekanın görselinden bağımsızdır. Herhangi bir trigger collider şekli çıkışı temsil edebilir. Bir `ArenaTransitionController` ve sıfır tabanlı kaynak arena index'i alır. Yalnızca açıkça atanmış oyuncunun collider'ları geçiş isteyebilir. Çıkış sadece kendi arenası tamamlanıp run geçiş beklerken kullanılabilir. Final Victory, Defeat, pause, devre dışı bileşenler ve tekrarlanan girişler run'ı ilerletemez. Kilitli çıkışın içinde bekleyen oyuncu, kilit açıldıktan sonra çıkıp tekrar girmelidir.

Geçiş yöneticisi sıralı giriş noktalarını tutar; sonraki arenayı başlatmadan önce mevcut oyuncuyu taşır. CharacterController etkinlik durumu korunur, düşme hızı sıfırlanır ve giriş noktasının yatay yönü uygulanır. Can, XP, ekipman ve eşya seviyeleri aynı oyuncuda korunur. Arena başlangıcı hata verirse run sahte bir Running durumunda kalmak yerine Defeat olur. Çıkışta beklerken ölüm de run'ı bitirir.

Bu adım aynı sahnedeki hedefleri kullanır; asenkron sahne yükleme ve görsele özgü efektler sonraya aittir. Test_Waves mevcut yerleşimde iki mantıksal arena oturumu, işaretli genel çıkış ve ikinci giriş noktasıyla akışı gösterir. İki bitmiş mekan tasarımı değildir. Mevcut drop'lar sahnede kalır; sahneler arası drop temizliği kararı sonraya bırakılır. Kod, test ve sahne bağlantıları tek anlamlı uygulama kaydı oluşturur.

### Sahneye yerleştirme

1. Arena oturumlarını initialize et; RunSessionController'a sıralı oturumları ve oyuncunun ölüm kaynağını aktar. Geçişte beklerken ölümün işlenmesi için ölüm kaynağı da verilmelidir.
2. ArenaTransitionController'a run, oyuncu kökü, oyuncu can bileşeni ve sıralı giriş Transform'larını ata (Inspector veya Configure). Giriş sayısı arena sayısıyla eşleşir. Giriş noktasının yatay yönü kullanılır; kameranın dikey bakışı korunur.
3. `Is Trigger` collider ile aynı nesneye ArenaTransitionTrigger ekle. Yönetici ve kaynak arena index'ini ata. Box, Sphere, Capsule veya Unity'nin desteklediği trigger mesh kullanılabilir. Unity fizik trigger koşulları geçerlidir; trigger üzerindeki kinematic Rigidbody örnekteki CharacterController oyuncusuyla çalışır.
4. Giriş noktasını engellerin ve diğer çıkış alanlarının dışında tut. Çıkış yalnızca kaynak index'ini iletir; hedefi run sırası belirler. Aynı arenanın birden fazla çıkışı aynı sırayı kullanır, arena atlamaz.

Test_Waves, ArenaSessionDevelopmentBootstrap üzerindeki `_secondArenaDefinition` ile örneği etkinleştirir. Dünya (0, 1, 7) konumundaki küp iki wave bitince yeşile döner; giriş oyuncuyu (0, 0.1, -5) konumuna taşır ve ikinci oturumu başlatır. Run paneli arena index'ini ve final sonucu gösterir. Bu örnek Editor development bootstrap'lerini kullanır; production composition ayrı bir iştir.

14–15.09.2026 geçiş doğrulaması: izole Unity 6000.3.9f1 projesinde 706/706 EditMode ve 395/395 PlayMode başarılıdır. Dokuz yeni PlayMode testi gerçek fizik Sphere trigger'ını, gerçek Test_Waves iki oturum akışını, oyuncu kimliğini ve taşıma sırasını, can korumasını, tekrar/yanlış kaynak reddini, pause ve eksik hedefi, geçişte ölümü, arena başlatma hatasını, CharacterController düşme hızı sıfırlamasını ve hatalı run kurulumundan sonra yeniden kurulumu kapsar. Kullanıcı oynanış kabulü beklenir.

## Durum ve kapsam

Bu increment mevcut tek arena oturumunun üzerine run seviyesinde orkestrasyon ekler. Sahne yükleme, arena modifier'ları, boss'lar, gold ve meta progression bu adıma dahil değildir.

## Kabul edilen davranış

- Run, önceden initialize edilmiş ve sıralı, boş olmayan arena oturumları listesini alır.
- Run başladığında yalnızca sıfırıncı arena başlar ve run/oturum başlangıç olayları yayınlanır.
- Aktif arena Victory olduğunda run Transition durumuna geçer ve tamamlanan arena index'ini yayınlar.
- Sonraki arena yalnızca açık bir `AdvanceToNextArena()` çağrısından sonra başlar.
- Son arena Victory olduğunda run tam olarak bir kez tamamlanır.
- Aktif arena Defeat olduğunda run Defeat ile tamamlanır; pasif veya bitmiş arenaların geç olayları yok sayılır.

## Teknik sınırlar

`RunSessionState` saf run ilerlemesini ve arena index'ini yönetir. `RunSessionController` aktarılmış `IArenaSession` nesnelerini ve geçiş olaylarını yönetir. Arena implementasyonları kendi wave yaşam döngülerinden sorumlu kalır. Sahne yükleme, geçiş sunumu ve production composition sonraki increment'lardır.

## Doğrulama

EditMode run state geçişlerini ve geçersiz sıralamayı kapsar. PlayMode aktif arena yönlendirmesini, açık geçişi, final Victory'yi, Defeat korumasını ve mevcut gameplay regresyonlarını kapsar.

14.09.2026 doğrulamasında izole Unity 6000.3.9f1 projesinde 706/706 EditMode ve 386/386 PlayMode testi başarılıdır.
