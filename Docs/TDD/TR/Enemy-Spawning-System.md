# Düşman Oluşturma Sistemi

## 1. Amaç

Bu modül, Project First Run içerisinde düşman prefablarının runtime sırasında
oluşturulmasını ve gerekli gameplay bağımlılıklarıyla başlatılmasını yönetir.

Şu anda düşman oluşturma işlemi yalnızca editör test bootstrap'ı tarafından
gerçekleştirilmektedir. Enemy Spawning System bu işlemi tekrar kullanılabilir bir
runtime servisine dönüştürür.

İlk sürüm aşağıdaki davranışları kapsar:

- Düşman prefabını belirli konum ve rotasyonda oluşturma
- EnemyController bileşenini initialize etme
- EnemyAttackController bileşenini initialize etme
- Oyuncu hedefi, hedef canı ve EnemyRegistry bağımlılıklarını aktarma
- Oluşturulan düşmanı çağıran sisteme döndürme
- Tek bir spawn noktası veya birden fazla spawn noktasıyla kullanılabilme
- Geçersiz bağımlılıkları açık hatalarla reddetme
- Yarım kalan spawn işlemlerinde oluşturulan nesneyi temizleme

Wave yönetimi, zamanlanmış spawn ve object pooling bu sürümün kapsamı dışındadır.

---

## 2. Tasarım Hedefleri

- EnemySpawner düşman yapay zekâsını yönetmemelidir.
- EnemySpawner düşman sağlığı veya saldırı kararlarının sahibi olmamalıdır.
- Spawn için gereken bağımlılıklar açıkça verilmelidir.
- EnemySpawner sahnedeki Player nesnesini otomatik olarak aramamalıdır.
- `FindObjectOfType`, global singleton veya service locator kullanılmamalıdır.
- Spawn edilen düşman eksik component içeriyorsa işlem açıkça başarısız olmalıdır.
- Başarısız initialization sahnede yarım oluşturulmuş düşman bırakmamalıdır.
- EnemyController ve EnemyAttackController mevcut sorumluluklarını korumalıdır.
- Object pooling daha sonra aynı spawn sözleşmesini kullanabilmelidir.

---

## 3. Temel Bileşenler

### EnemySpawnRequest

Bir düşmanın oluşturulması için gereken verileri temsil eder.

İçerdiği bilgiler:

- EnemyController prefabı
- EnemyDefinition
- Hedef Transform
- Hedef IDamageable
- EnemyRegistry
- Spawn konumu
- Spawn rotasyonu

Request kendi verilerini doğrular ve geçersiz bağımlılıkların spawn işlemine
ulaşmasını engeller.

### EnemySpawnResult

Başarılı spawn işleminin sonucunu temsil eder.

İçerdiği bilgiler:

- Oluşturulan EnemyController
- Oluşturulan EnemyAttackController
- Oluşturulan GameObject

Sonuç nesnesi, wave veya arena sistemlerinin düşman instance'ına açık biçimde
erişebilmesini sağlar.

### EnemySpawner

EnemySpawner, spawn işlemini gerçekleştiren runtime MonoBehaviour servisidir.

Sorumlulukları:

- EnemySpawnRequest kabul etmek
- Prefabı oluşturmak
- EnemyAttackController varlığını doğrulamak
- EnemyController.Initialize çağırmak
- EnemyAttackController.Initialize çağırmak
- Başarılı sonucu döndürmek
- Başarısızlık hâlinde oluşturulan nesneyi temizlemek

Sorumlu olmadığı alanlar:

- Spawn zamanı seçmek
- Spawn noktası seçmek
- Düşman türünü rastgele belirlemek
- Maksimum düşman sayısını yönetmek
- Wave ilerlemesini hesaplamak
- Düşman zorluğunu ölçeklendirmek
- Ölü düşmanı yok etmek
- Object pooling yapmak

---

## 4. Spawn Akışı

1. Çağıran sistem bir EnemySpawnRequest oluşturur.
2. Request gerekli bağımlılıkları doğrular.
3. EnemySpawner düşman prefabını verilen konum ve rotasyonda oluşturur.
4. Spawn edilen nesnede EnemyAttackController aranır.
5. EnemyController, tanım, hedef ve registry ile initialize edilir.
6. EnemyAttackController, tanım, hedef ve hedef IDamageable ile initialize edilir.
7. EnemySpawnResult çağıran sisteme döndürülür.

Başlatma sırasında exception oluşursa oluşturulan düşman nesnesi temizlenir ve
hata çağıran sisteme yeniden iletilir.

---

## 5. Bağımlılık Kuralları

Spawn işlemi aşağıdaki bağımlılıkları zorunlu kabul eder:

- EnemyController prefab
- EnemyDefinition
- Target Transform
- Target IDamageable
- EnemyRegistry

Spawn için konum ve rotasyon doğrudan request içerisinde bulunur.

Target Transform ile Target IDamageable aynı nesneye ait olmak zorunda değildir.
Bu sayede ileride hedefleme noktası ile hasar alan collider veya health nesnesi
ayrılabilir.

Target IDamageable, mevcut EnemyAttackController sözleşmesi nedeniyle bir Unity
Object olmalıdır.

---

## 6. Çoklu Spawn Noktaları

EnemySpawner tek bir request üzerinden tek bir düşman oluşturur.

Birden fazla spawn noktası kullanmak isteyen üst seviye sistem:

1. Spawn noktalarını seçer.
2. Her nokta için ayrı EnemySpawnRequest oluşturur.
3. EnemySpawner.Spawn çağrısını tekrarlar.
4. Dönen sonuçları kendi koleksiyonunda saklar.

Bu karar EnemySpawner'ı wave ve arena kurallarından bağımsız tutar.

---

## 7. Hata Davranışı

Aşağıdaki durumlarda spawn işlemi başarısız olmalıdır:

- Prefab null ise
- EnemyDefinition null ise
- Target null ise
- Target IDamageable null ise
- EnemyRegistry null ise
- Target IDamageable bir Unity Object değilse
- Spawn edilen prefab EnemyAttackController içermiyorsa
- EnemyController initialization başarısız olursa
- EnemyAttackController initialization başarısız olursa

Prefab oluşturulduktan sonra hata oluşursa instance sahnede bırakılmamalıdır.

---

## 8. İlk Sürüm Kabul Kriterleri

- Geçerli request bir düşman oluşturmalıdır.
- Düşman verilen konum ve rotasyonda oluşturulmalıdır.
- EnemyController initialize edilmiş olmalıdır.
- EnemyAttackController initialize edilmiş olmalıdır.
- Düşman EnemyRegistry içine kaydolmalıdır.
- Düşman verilen hedefe doğru hareket etmelidir.
- Düşman menzile geldiğinde hedefe hasar vermelidir.
- Spawn sonucu oluşturulan controller'lara erişim sağlamalıdır.
- Null bağımlılıklar açık exception üretmelidir.
- Attack component'i eksik prefab reddedilmelidir.
- Başarısız spawn işleminden sonra sahnede yarım instance kalmamalıdır.
- Mevcut Enemy Foundation ve Enemy Attack testleri geçmeye devam etmelidir.

---

## 9. Ertelenen Konular

- Wave sistemi
- Spawn zamanlayıcısı
- Rastgele spawn noktası seçimi
- Ağırlıklı düşman türü seçimi
- Zorluk ölçeklendirme
- Maksimum aktif düşman sınırı
- Object pooling
- Spawn efektleri
- Spawn animasyonları
- Spawn güvenlik mesafesi
- Oyuncunun görüş alanı dışında spawn
- NavMesh üzerinde geçerli pozisyon arama
- Düşman cesedi temizleme
- Arena tamamlanma kararı