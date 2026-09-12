# Wave Foundation

## 1. Amaç

Bu modül, bir arena içerisindeki sıralı düşman dalgalarının oluşturulmasını ve
ilerlemesinin takip edilmesini yönetir.

EnemySpawner tek bir düşmanın oluşturulmasından sorumludur. WaveController ise
hangi düşmanların, kaç adet ve hangi sırayla oluşturulacağını koordine eder.

İlk sürüm aşağıdaki davranışları kapsar:

- Birden fazla wave tanımlama
- Her wave içerisinde bir veya daha fazla düşman girdisi tanımlama
- Her düşman girdisi için prefab, EnemyDefinition ve adet belirleme
- Mevcut wave içerisindeki düşmanları oluşturma
- Spawn noktalarını sırayla kullanma
- Yalnızca ilgili wave tarafından oluşturulan düşmanları takip etme
- Tüm düşmanlar öldüğünde sonraki wave'e geçme
- Son wave tamamlandığında sequence tamamlanma olayı yayınlama
- Başarısız wave başlangıcında oluşturulan kısmi düşmanları temizleme

Spawn zamanlayıcıları, wave arası bekleme ve zorluk ölçeklendirme bu sürümün
kapsamı dışındadır.

---

## 2. Tasarım Hedefleri

- WaveController düşman yapay zekâsını yönetmemelidir.
- Düşman oluşturmak için mevcut EnemySpawner kullanılmalıdır.
- WaveController sahneden otomatik bağımlılık aramamalıdır.
- Player, EnemyRegistry ve spawn noktaları açıkça verilmelidir.
- ScriptableObject tanımları sahne Transform referansı tutmamalıdır.
- Wave tamamlanması global EnemyRegistry sayısına bağlı olmamalıdır.
- WaveController yalnızca kendi oluşturduğu düşmanları takip etmelidir.
- Global singleton, service locator veya global event bus kullanılmamalıdır.
- Runtime ilerleme verisi ScriptableObject assetlerini değiştirmemelidir.
- Wave tamamlanma event'leri yalnızca bir kez yayınlanmalıdır.

---

## 3. Temel Bileşenler

### EnemyWaveEntry

Bir wave içerisindeki tek düşman grubunu temsil eder.

İçerdiği bilgiler:

- EnemyController prefabı
- EnemyDefinition
- Oluşturulacak düşman adedi

Bir wave içerisinde birden fazla EnemyWaveEntry bulunabilir.

Örnek:

- 3 Basic Chaser
- 2 Fast Chaser
- 1 Heavy Enemy

### EnemyWaveDefinition

Tek bir wave'in içeriğini tanımlayan ScriptableObject'tir.

İçerdiği bilgiler:

- Kalıcı kimlik
- Görünen isim
- Sıralı EnemyWaveEntry listesi

Sahne nesnesi veya runtime ilerleme bilgisi içermez.

### ArenaWaveDefinition

Bir arena içerisindeki sıralı wave listesini tanımlayan ScriptableObject'tir.

İçerdiği bilgiler:

- Kalıcı kimlik
- Sıralı EnemyWaveDefinition listesi

WaveController wave'leri bu sıraya göre çalıştırır.

### WaveProgressState

Tek bir wave'in runtime ilerlemesini temsil eden saf C# sınıfıdır.

Takip ettiği bilgiler:

- Toplam planlanan düşman sayısı
- Oluşturulan düşman sayısı
- Hayatta olan düşman sayısı
- Ölen düşman sayısı
- Spawn işleminin tamamlanıp tamamlanmadığı
- Wave'in tamamlanıp tamamlanmadığı

ScriptableObject verisini değiştirmez ve MonoBehaviour bağımlılığı içermez.

### WaveController

Wave akışını yöneten runtime MonoBehaviour'dur.

Sorumlulukları:

- Açık bağımlılıklarla initialize edilmek
- İlk wave'i başlatmak
- EnemySpawner üzerinden düşman oluşturmak
- Spawn noktalarını sırayla kullanmak
- Oluşturduğu EnemyController instance'larını takip etmek
- EnemyController.Died event'lerini dinlemek
- Wave tamamlanınca sonraki wave'e geçmek
- Son wave tamamlandığında sequence tamamlanma olayı yayınlamak
- Event aboneliklerini temizlemek

Sorumlu olmadığı alanlar:

- Düşman hareketi ve saldırısı
- Düşman sağlığı
- Rastgele düşman seçimi
- Wave arası süre
- Spawn efektleri
- Ödül hesaplama
- Arena başarı veya başarısızlık kararı
- Player ölümünü yönetmek
- Object pooling

---

## 4. Bağımlılıklar

WaveController aşağıdaki bağımlılıkları açıkça alır:

- ArenaWaveDefinition
- EnemySpawner
- EnemyRegistry
- Target Transform
- Target IDamageable
- Bir veya daha fazla spawn noktası

Spawn noktaları sahneye ait oldukları için ScriptableObject içerisinde tutulmaz.

Target Transform ile Target IDamageable aynı nesneye ait olmak zorunda değildir.

---

## 5. Wave Başlatma Akışı

1. WaveController initialize edilir.
2. Begin çağrısı yapılır.
3. İlk EnemyWaveDefinition seçilir.
4. WaveStarted olayı yayınlanır.
5. Wave içerisindeki EnemyWaveEntry kayıtları sırayla işlenir.
6. Her düşman için EnemySpawnRequest oluşturulur.
7. EnemySpawner.Spawn çağrılır.
8. Oluşturulan düşmanın Died event'ine abone olunur.
9. Spawn noktaları round-robin yöntemiyle sırayla kullanılır.
10. Tüm düşmanlar oluşturulduğunda wave spawn işlemi tamamlanır.

İlk sürümde wave içerisindeki tüm düşmanlar bekleme olmadan oluşturulur.

---

## 6. Wave Tamamlanma Akışı

1. Wave tarafından oluşturulan bir düşman ölür.
2. WaveController ilgili Died event'ini işler.
3. Düşmanın event aboneliği kaldırılır.
4. Hayatta olan düşman sayısı azaltılır.
5. Wave içerisindeki tüm düşmanlar oluşturulmuş ve hayatta kalan düşman sayısı
   sıfır olmuşsa wave tamamlanır.
6. WaveCompleted olayı yayınlanır.
7. Sonraki wave varsa otomatik olarak başlatılır.
8. Sonraki wave yoksa SequenceCompleted olayı yayınlanır.

Aynı düşmanın ölüm olayı birden fazla kez işlenmemelidir.

---

## 7. EnemyRegistry Kullanımı

EnemyRegistry, oluşturulan düşmanların global runtime kaydını tutmaya devam eder ve
EnemySpawnRequest için gerekli bir bağımlılıktır.

Ancak aşağıdaki kontrol kullanılmaz:

`EnemyRegistry.ActiveCount == 0`

Bu değer wave dışında oluşturulan düşmanlardan etkilenebilir.

Wave tamamlanması yalnızca WaveController'ın kendi oluşturduğu ve takip ettiği
EnemyController instance'ları üzerinden belirlenir.

---

## 8. Spawn Noktası Seçimi

Spawn noktaları verilen sıraya göre kullanılır.

Örnek olarak üç spawn noktası varsa:

- Birinci düşman: Spawn Point 0
- İkinci düşman: Spawn Point 1
- Üçüncü düşman: Spawn Point 2
- Dördüncü düşman: Spawn Point 0

İlk sürümde rastgele seçim yapılmaz.

Null veya boş spawn noktası listesi kabul edilmez. Liste içerisindeki null
Transform kayıtları da reddedilir.

---

## 9. Başarısızlık Davranışı

Bir wave oluşturulurken herhangi bir spawn işlemi başarısız olursa:

1. Mevcut wave sırasında başarıyla oluşturulan düşmanların event abonelikleri
   kaldırılır.
2. Bu düşmanlar devre dışı bırakılır ve temizlenir.
3. Wave ilerleme durumu temizlenir.
4. WaveController başarısız durumda kalır.
5. Orijinal exception çağıran sisteme iletilir.

Kısmen başlatılmış bir wave çalışmaya devam etmemelidir.

---

## 10. İlk Sürüm Event'leri

WaveController aşağıdaki yerel event'leri yayınlar:

- WaveStarted
- EnemySpawned
- EnemyDefeated
- WaveCompleted
- SequenceCompleted

Event'ler yalnızca WaveController instance'ı üzerinden dinlenir. Global event bus
kullanılmaz.

---

## 11. İlk Sürüm Kabul Kriterleri

- Geçerli bir arena tanımı ilk wave'i başlatmalıdır.
- Wave içerisindeki tüm düşmanlar oluşturulmalıdır.
- Birden fazla düşman girdisi desteklenmelidir.
- Spawn noktaları sırayla kullanılmalıdır.
- Oluşturulan düşmanlar EnemyRegistry içine kaydolmalıdır.
- WaveController yalnızca kendi oluşturduğu düşmanları takip etmelidir.
- Bir düşman öldüğünde hayatta olan düşman sayısı azalmalıdır.
- Tüm wave düşmanları öldüğünde sonraki wave başlamalıdır.
- Son wave tamamlandığında SequenceCompleted yalnızca bir kez yayınlanmalıdır.
- Wave dışındaki bir düşman wave tamamlanma kararını etkilememelidir.
- Geçersiz tanımlar ve bağımlılıklar açık exception üretmelidir.
- Başarısız spawn sonrasında kısmi wave instance'ları kalmamalıdır.
- Mevcut enemy ve spawn testleri geçmeye devam etmelidir.

---

## 12. Ertelenen Konular

- Wave arası bekleme
- Düşmanların belirli aralıklarla oluşturulması
- Rastgele spawn noktası seçimi
- Ağırlıklı düşman seçimi
- Zorluk ölçeklendirme
- Elite ve boss wave kuralları
- Object pooling
- Spawn animasyonu ve efektleri
- Wave kullanıcı arayüzü
- Arena ödülü
- Arena tamamlanma kararı
- Run başarı ve başarısızlık akışı
- Düşman cesedi temizleme