# Level atlama kaynaklı sandık

Güncel içerik: [Common Kategori Sandıkları](Common-Chest-Types.md) tablonun tek geliştirme seçimini eşit ağırlıklı Weapon/Ability/Upgrade tanımlarıyla, tek başlangıç düzeneğini üç etiketli örnekle değiştirir. Aşağıdaki hak, seçimin korunması ve konum sözleşmeleri değişmez; tek geliştirme sandığı örnekleri önceki adımı anlatır.

## Kapsam

GDD'ye uygun olarak kazanılan her run level'ı bir dünya sandığı garanti eder. İlk aşama XP ve sandık temellerini tek tanımla bağlamıştı. [Sandık Seviyesi Temeli](Chest-Tier-Foundation.md) artık `ChestDropTable` üzerinden ağırlıklı seçim sağlar: ilk uygun işleme denemesinde bir kez çekilir, seçilen tanım o hak başarıyla spawn edilene kadar korunur. Test_Waves hâlâ yalnızca mevcut geliştirme sandığını ve ödül havuzunu seçer. Eşya seviyesi ödülleri, altın telafisi ve otomatik ödül popup'ı bu kaynağın dışındadır.

Branch tabanı: Plastic `/main/dev` **cs:148**, Git `main` **cad6518**. Bu yeni bir aşamadır; chest-foundation altından devam edilmez.

## Sorumluluklar

- `LevelUpChestTracker`, gözlenen en yüksek run level'ını, bekleyen sandık sayısını ve başarılı üretim sayısını tutar. Level 1 sandık vermez. Tekrarlanan/eski bildirim ödülü çoğaltmaz; 1'den 4'e geçiş üç sandık kazandırır.
- `LevelUpChestSource`, XP bildiriminin içinde sandık üretmeden `PlayerExperienceController.LevelChanged` olayını izler. İşleme/devam sırasında güncel level ile de eşleşir; disable dönemlerinde kazanılan level kaybolmaz. İlk bağlanma oyuncunun o anki level'ından (XP henüz başlamamışsa 1'den) başlar; kaynak oluşturulmadan önce kazanılan level'lara geriye dönük ödül ödemez.
- Kaynak her Update'te en fazla bir sandık işler. XP/spawner initialization, hayatta olma ve pozitif time scale koşullarını bekler. Yalnızca başarılı üretim bir bekleyen hakkı tüketir; uygun yer yoksa hak bekler. Konum araması sınırlı aralıklarla yeniden denenir. Konfigürasyon/üretim hatası bir kez loglanır ve hakkı tüketmeden otomatik işlemeyi durdurur.
- Disable/re-enable tracker'ı sıfırlamaz; ömür run kapsamındadır. Kaynağı yok edip yeniden oluşturma, save/load ve sahneler arası kalıcılık bu adımda desteklenmez.
- `ChestSpawnerBootstrap`, build initialization sonrasında ortak spawner'ı hazırlar. Level-up üretimi Editor-only başlangıç sandığı bootstrap'ına bağımlı değildir; başlangıç sandığı ayrı test fixture'ı olarak kalır.

## Konumlandırma

`ChestSpawnPlacement`, oyuncunun güncel konumu çevresindeki üç halkayı (2.5, 4.25, 6 metre) ön taraftan başlayarak tarar. Halka başına sekiz adayla çalışma sınırlandırılır. Mevcut dik duran, kökünde BoxCollider bulunan sandık prefab'ının ölçekli boyutları kullanılır. Merkez ve taban köşelerinde zemin aranır; eksik/düzensiz destek, dolu hacim ve katı collider ile kesilen doğrudan yol reddedilir. Zemin Environment katmanından seçilir; engel kontrolü trigger/XP kürelerini yok sayar. Çoklu level'da üst üste üretimi önlemek için yeni collider bir sonraki sandıktan önce fiziğe yansıtılır.

Bu, düz arena için konumlandırmadır; genel navigasyon/pathfinding sistemi değildir. Bütün adaylar doluysa oyuncu hareket edebilir veya mevcut sandıkları açabilir; hak korunur ve yeni oyuncu konumu çevresinde tekrar denenir. Ölüm/duraklatma ödülleri silmez, işlemeyi erteler.

## Mevcut ödül davranışı

Sandığın oluşması ödül ekranını açmaz. Oyuncu sandığa bakıp E / gamepad South kullanır. Uygun ödül kalmadığında sandık, temeldeki davranış gibi kullanılabilir kalır; kaynak altın fallback uydurmaz. Küçük geliştirme ödül havuzu tükendiğinde ödül ilerlemesi genişletilene kadar sonraki sandıkların sunacak ödülü kalmayabilir.

## Doğrulama

Otomatik testler: çoklu level ve yinelenen gözlem, initialization ve pause/death/disable, spawner'ın geç hazır olması, başarılı/başarısız konumlandırma ve üretim, hareket veya boşalan alan sonrası yeniden deneme, zemin/engel/taban kontrolleri. Sahne testleri ortak spawner, XP/oyuncu referansları ve tanım bağlantılarını doğrular.

Manuel Test_Waves: başlangıç geliştirme sandığı değişmez. Dört adet 25 XP küresiyle level 2 ol; yakındaki boş zeminde bir ek sandık oluşmalı, UI kendiliğinden açılmamalı. E ile aç. Sonraki 150 XP'de bir sandık daha gelmeli. Ölüm/duraklatma, arena kenarı ve mevcut sandık yakınında konumlandırmayı kontrol et. Çoklu level kazanımı callback başına değil level başına ayrı sandık üretmeli.
