# Bölge ön hazırlığı

## Sözleşme — roadmap 5

PreparedRegionEncounter bir bölgenin tek düşman grubunu mevcut EnemySpawnRequest/EnemySpawner altyapısıyla hazırlar. Dalgalı özel karşılaşmalar WaveController kullanmaya devam eder. Bu artım hazır gruplar içindir; WaveController dizileri önceden hazırlanmış sayılmaz.

- Yaklaşma trigger'ı RequestPreparation çağırır; tekrar çağrılar aynı işi çoğaltmaz.
- Her Update en fazla MaxEnemiesPerFrame düşman oluşturur; her düşmandan sonra ölçülen süre bütçesine bakılır. Bir Instantiate çağrısı bölünemediği için milisaniye bütçesi kesin üst sınır değildir.
- Düşmanlar etkin olmayan bir parent altında instantiate/initialize edilir. Motor, saldırı ve EnemyController kapalı; fiziksel collider'lar devre dışı tutulur. Sonra kök nesne etkin alana alınır: Awake ve hazırlık maliyeti karelere yayılır. Düşmanlar registry'ye katılmaz, hareket/saldırı yapmaz.
- Hazır modellerin görünmemesi level design sorumluluğudur: hazırlık hacmini görüşü kesen koridor/kapıdan önce yerleştir. Sistem occlusion veya shader ön derlemesi vaat etmez.
- Giriş trigger'ı hazırsa RegionEncounterSession.Enter çağırır. Henüz hazır değilse giriş niyeti bekler; oyuncu alandan ayrılırsa niyet iptal olur. Giriş alanı yaklaşma trigger'ı atlanmışsa hazırlığı da başlatır.
- RegionPassageController'a hedef hazırlık referansı bağlanır. Hazırlık bitene kadar mevcut bariyer hedefi kapalı tutar; Ready/Running/Victory durumlarında geçiş hazır kabul edilir. Hazır olma, karşılaşmayı bitirme şartından ayrıdır.
- Aktivasyon mevcut nesnelerde AI/collider/registry katılımını açar; yeniden instantiate etmez. Aktivasyon süresi ayrıca ölçülür. Çok sayıda AI'ı aynı anda açmanın maliyeti profil verisiyle değerlendirilecektir.
- Oyuncu ölürse veya hazırlık sahibi kapatılır/yok edilirse bekleyen iş iptal edilir, sahip olduğu düşmanlar temizlenir; ölüm/drop olayı taklit edilmez. İptal/hata terminaldir; yeni çalışma için yeni oturum kurulur.
- Geri giriş mevcut düşmanları ve canlarını korur. Aktif alandan normal çıkış karşılaşmayı durdurmaz. Unvisited kaynak boşaltma politikası hâlâ sonraki karar kapsamındadır.

## Durumlar ve yapılandırma

Hazırlık: Idle → Preparing → Ready; hata/iptal → Failed/Cancelled. IArenaSession Ready/Running/Victory/Defeat karşılaşma durumudur. Hazırlık Ready olmadan bölge oturumu bağlanmaz.

Inspector'da mevcut bir EnemyWaveDefinition (tek grup olarak), spawn noktaları, EnemySpawner, EnemyRegistry, hedef oyuncu/health ve bölge kimliği atanabilir. Kodla Initialize da aynı yolu kullanır. RegionPreparationTrigger için generic trigger Collider, oyuncu root'u ve amaç (Prepare/Activate) atanır. Bölge başına tek aktivasyon hacmi kullanılır; birden fazla hazırlık hacmi olabilir. Başlangıçta içerdeki oyuncu ve çoklu collider'lar tarama ile ele alınır. Hacmi bir fizik adımında tamamen atlamayı önlemek için boyut oyuncu hızına uygun olmalıdır.

## Kabul ve ölçüm

Gerçek düşmanlarla kare başına adet sınırı, dormancy, registry ve hasar, erken giriş/geri çıkış, farklı collider, readiness bariyeri, hazırlık hatasında kısmi temizlik, ölüm, iptal ve yeniden giriş test edilir. Profiler işaretleri Region.PrepareStep ve Region.Activate kullanılır. Last/MaxPreparationStepMilliseconds ve LastActivationMilliseconds geliştirme ölçümleridir; headless testler hedef cihaz FPS garantisi değildir. Nesne havuzu bu adımda eklenmez; ölçümler ve oynanabilir sahne sonrasında karar verilir.

## Doğrulama — 15 Eylül 2026

- EditMode: 733/733; PlayMode: 420/420 geçti. Ön hazırlık için eklenen 12 PlayMode testi gerçek NavMesh kullanır.
- Son çalıştırmanın dört düşmanlı örneğinde hazırlık adımı en fazla 0,4034 ms, aktivasyon 0,1689 ms ölçüldü. Önceki çalıştırmada sırasıyla 0,3451 ms ve 0,0674 ms ölçülmesi bu değerlerin ortam/yük bağımlı olduğunu gösterir; performans hedefi değildir.
- Küresel trigger'da yalnızca AABB kesişmesi yeterli sayılmaz; gerçek şekil kesişimi doğrulanır. Hazırlık kaynağının yok edilmesi bağlı geçidi hazır kabul ettirmez.
- Sonuç dosyaları yerel .codex-temp/xp-attraction/preparation-edit-final.xml ve preparation-play-verified.xml içindedir.
- Mevcut Test_Waves sahnesi değiştirilmedi. Oynanabilir koridor/odalar roadmap 9'da kurulacak; bu aşama kod ve otomatik fixture entegrasyonudur.
