# Mimari İlkeler

# Teknik Hedefler, Hedef Dışı Konular ve Mimari İlkeler

## 1. Amaç

Bu bölüm projenin teknik kalite standartlarını ve mimari kurallarını tanımlar.

Bu kurallar; kod tabanını sürdürülebilir, test edilebilir, performanslı ve gereksiz soyutlamalar oluşturmadan genişletilebilir tutmak için kullanılacaktır.

Bir mimari yalnızca oyun çalıştığı için başarılı kabul edilmez. Sorumlulukları anlaşılır, bağımlılıkları görünür, içerik üretimi pratik ve gelecekteki değişiklikleri makul ölçüde güvenli hâle getirmelidir.

---

## 2. Teknik Hedefler

### 2.1 Sürdürülebilir ve Okunabilir Kod

Kod tabanı, gizli kuralların tamamını bilmeden de anlaşılabilir olmalıdır.

Her sistem şunlara sahip olmalıdır:

- Açık bir sorumluluk.
- Sahip olduğu durum için açık bir sahiplik.
- Belirli girdiler ve çıktılar.
- Sınırlı ve görünür bağımlılıklar.
- Uygulama ayrıntısı yerine amacı anlatan isimler.

Bir sınıf yalnızca küçük olduğu için iyi tasarlanmış kabul edilmez. Sorumluluğu ve değişme nedeni de açık olmalıdır.

### 2.2 Veri Odaklı İçerik Üretimi

Silahlar, yetenekler, güçlendirmeler, evrimler, düşmanlar, dalgalar, arenalar ve ödüller; içerik üretimini kolaylaştırdığı yerlerde verilerle tanımlanmalıdır.

ScriptableObject’ler şunları tutabilir:

- Değişmeyen kimlikler.
- Oyuncuya gösterilen bilgiler.
- Seviye verileri.
- Sayısal ayarlar.
- Prefab ve sunum referansları.
- İçerikler arasındaki ilişkiler.
- Tasarımcı tarafından düzenlenen ayarlar.

ScriptableObject’ler ortak ve değiştirilebilir çalışma zamanı durumu olarak kullanılmamalıdır.

Veri odaklı tasarım, bütün davranışların genel veri yapılarıyla temsil edilmesi gerektiği anlamına gelmez. Benzersiz oynanış davranışları, daha anlaşılır olması durumunda kendilerine ait çalışma zamanı kodlarını kullanabilir.

### 2.3 Açık Bağımlılık Yönetimi

Bağımlılıklar şu yollarla görünür olmalıdır:

- Saf C# nesnelerinde constructor parametreleri.
- Yerel Unity sahne ve prefab bağımlılıklarında serialized referanslar.
- Çalışma zamanında kurulum gereken durumlarda açık initialization metotları.
- Sahne sınırlarında context nesneleri.

Proje; global singleton aramaları, statik servis erişimleri ve tekrarlanan çalışma zamanı nesne aramaları gibi gizli bağımlılıklardan kaçınmalıdır.

### 2.4 Açık Yaşam Süresi ve Sahiplik Sınırları

Uygulama, tur ve sahne durumları birbirinden ayrı kalmalıdır.

Her önemli durumun tek bir yetkili sahibi bulunmalıdır.

Örnekler:

- Kalıcı ilerleme `PlayerProfile` sistemine aittir.
- Mevcut tur gelişimi `RunSession` sistemine aittir.
- Mevcut arenadaki düşmanlar arena sahnesine aittir.
- Mevcut silah mühimmatı silahın çalışma zamanı nesnesine aittir.

İki farklı sistem aynı durumun birbirinden bağımsız ve yetkili kopyalarını tutmamalıdır.

### 2.5 Test Edilebilir Oynanış Kuralları

Önemli oynanış kuralları mümkün olduğu ölçüde saf C# olarak uygulanmalıdır.

Örnekler:

- Hasar hesaplama.
- Zırh hesaplama.
- Deneyim eşikleri.
- Sandık ödülü filtreleme.
- Evrim uygunluğu.
- Slot doğrulama.
- Korunan altın hesabı.
- Stat modifier hesaplaması.

Unity’ye özgü sunum ve sahne davranışları MonoBehaviour içinde kalabilir. Ancak temel kurallar makul ölçüde mümkünse yüklü bir sahneye ihtiyaç duymamalıdır.

### 2.6 Genişletilebilir İçerik Sistemleri

Yeni bir eşya eklemek normalde şu adımları gerektirmelidir:

1. Tanım verisini oluşturmak.
2. Sunum asset’lerini sağlamak.
3. Uygun çalışma zamanı davranışını seçmek veya geliştirmek.
4. Eşyayı ilgili kataloğa kaydetmek.

Yeni silah, yetenek veya güçlendirme eklendiğinde mevcut sistemlerde büyük switch bloklarının değiştirilmesi gerekmemelidir.

Ancak proje gelecekte oluşturulabilecek her eşyanın kod yazılmadan hazırlanmasını sağlamaya çalışmayacaktır.

### 2.7 Editör Dostu Çalışma Akışı

İçerik üretimi Unity Editor içinde pratik olmalıdır.

Proje şunları desteklemelidir:

- Açık Inspector alanları.
- Eksik veya tekrarlanan kimliklerin doğrulanması.
- Kullanışlı varsayılan değerler.
- Düzenli ScriptableObject oluşturma menüleri.
- Geçersiz içerik ilişkileri için uyarılar.
- Sistemlerin bağımsız denenebileceği test sahneleri.

Özel editörler yalnızca varsayılan Inspector yetersiz kaldığında oluşturulmalıdır.

### 2.8 Performansı Gözeten Mimari

Proje Steam Deck sınıfı donanımlarda akıcı çalışmayı hedefler.

Geçici performans hedefi:

- 1280 × 800 çözünürlük.
- Normal oynanış sırasında 60 FPS.
- Tam gamepad desteği.
- El konsolu ekranında okunabilir kullanıcı arayüzü.

Performansa duyarlı sistemler şu yaklaşımlarla tasarlanmalıdır:

- Sık oluşturulan nesnelerde object pooling.
- Kontrollü güncelleme sıklığı.
- Verimli hedef sorguları.
- Sınırlı çalışma zamanı allocation’ı.
- Ölçeklenebilir VFX ve kalite ayarları.
- Gerçek build’ler üzerinde profiling.

Optimizasyonlar tahminlere değil ölçümlere dayanmalıdır.

### 2.9 Platformdan Bağımsız Oynanış

Oynanış kodu belirli bir input cihazına veya yalnızca masaüstünde bulunan API’lere doğrudan bağlı olmamalıdır.

Mimari şunları desteklemelidir:

- Klavye ve fare.
- Gamepad.
- Steam Deck kontrolleri.
- Çözünürlükten bağımsız kullanıcı arayüzü.
- Açık sınırlar arkasında platforma özel servisler.

Gelecekteki olası konsol desteği mümkün kalmalıdır. Ancak ilk PC projesi bu ihtimal nedeniyle gereksiz şekilde karmaşıklaştırılmamalıdır.

### 2.10 Güvenli Kayıt Sistemi

Kaydedilen veriler sürümlendirilmeli ve çalışma zamanı sahne nesnelerinden ayrı tutulmalıdır.

Kayıt sistemi şunları desteklemelidir:

- Profil sürümlendirmesi.
- Desteklenen kayıt sürümleri arasında migration.
- Eksik yazma durumlarına karşı yedek veya kurtarma.
- Profil ilerlemesiyle cihaz ayarlarının ayrılması.
- Oyuncuya gösterilen isimler yerine değişmeyen içerik kimlikleri.

---

## 3. Teknik Hedef Dışı Konular

Aşağıdakiler ilk mimarinin bilinçli olarak hedeflemediği konulardır.

### 3.1 Genel Amaçlı Bir Roguelite Framework Geliştirmek

Projeden tekrar kullanılabilir sistemler çıkabilir. Ancak sistemler öncelikle bu oyun için tasarlanacaktır.

Gerçek bir ihtiyaç ortaya çıkmadıkça mimari, varsayımsal gelecek projeler için genelleştirilmeyecektir.

### 3.2 Her Sınıf İçin Interface Oluşturmak

Interface’ler otomatik olarak eklenmeyecektir.

Interface şu durumlarda anlamlıdır:

- Gerçek bir sistem sınırı oluşturuyorsa.
- Birden fazla implementasyonu destekliyorsa.
- Anlamlı test kolaylığı sağlıyorsa.
- Üst düzey kuralları altyapıdan ayırıyorsa.

Yalnızca tek bir implementasyon bulunması interface oluşturmak için yeterli bir gerekçe değildir.

### 3.3 Her İletişimi Event Tabanlı Yapmak

Event’ler açık metot çağrılarının yerini almayacaktır.

Birden fazla bağımsız dinleyicinin gerçekleşmiş bir olaya tepki vermesi gerektiğinde event uygundur. Bir nesne başka bir nesneden açıkça iş yapmasını istiyorsa doğrudan çağrı tercih edilir.

Başlangıçta global event bus kullanılmayacaktır.

### 3.4 Projenin Başında Dependency Injection Framework Kullanmak

İlk projede bağımlılıklar açık biçimde oluşturulacak ve aktarılacaktır.

DI framework yalnızca manuel kurulumun gerçekten sürdürülebilirlik sorunu oluşturduğu kanıtlanırsa yeniden değerlendirilecektir.

### 3.5 Tamamen Genel Bir Effect veya Ability Graph Oluşturmak

İlk mimari, bütün silahları, yetenekleri, trait’leri ve evrimleri tek bir genel effect sistemiyle temsil etmeye çalışmayacaktır.

Gerçek tekrarlar ortaya çıktıktan sonra ortak yapılar ayrıştırılacaktır.

Benzersiz davranışlar kendilerine ait sınıflarda kalabilir.

### 3.6 Her Asset İçin Addressables Kullanmak

Addressables; asenkron yükleme, bellek kontrolü, içerik gruplama veya platforma özel dağıtım açık bir fayda sağladığında kullanılacaktır.

Küçük ve uygulama boyunca kalıcı olarak yüklü asset’lerin varsayılan olarak Addressable yapılması gerekmez.

### 3.7 Erken Mikro Optimizasyon

Ölçülmemiş performans kazanımları için kod okunabilirliğinden vazgeçilmeyecektir.

Karmaşık optimizasyonlardan önce yoğun kullanılan kod yolları profile edilecektir.

### 3.8 Oynanış GameObject’lerini Arenalar Arasında Korumak

Oyuncu, kamera, silah, yetenek, düşman, mermi ve arena nesneleri sahneler arasında korunmayacaktır.

Kalıcı tur verileri yeni sahne nesnelerine yeniden uygulanacaktır.

### 3.9 İlk Sürümde Tur Ortasında Kaydetme ve Devam Etme

İlk sürüm, oyun kapatıldıktan sonra tamamlanmamış bir turun geri yüklenmesini gerektirmez.

Bu ihtimal daha sonra serialize edilebilir bir tur snapshot’ı aracılığıyla değerlendirilebilir.

### 3.10 Çok Oyunculu Oyun Desteği

Networking, authority, replication ve çok oyunculu senkronizasyon mevcut proje kapsamının dışındadır.

---

## 4. Mimari İlkeler

### 4.1 Mimariyi Oynanış İhtiyaçları Belirler

Mimari oyuna hizmet etmek için vardır.

Teknik olarak şık görünen ancak içerik üretimini veya oynanış üzerinde tekrar çalışmayı zorlaştıran bir sistem başarılı kabul edilmez.

Bir tasarım deseni eklenmeden önce çözdüğü somut problem belirlenmelidir.

### 4.2 En Basit Doğru Çözümü Tercih Et

Tercih edilen çözüm şu koşulları karşılayan en basit çözümdür:

- Mevcut gereksinimleri karşılar.
- Sorumlulukları açık tutar.
- Güvenli biçimde değiştirilebilir.
- Belirgin gelecek engelleri oluşturmaz.

“Basit”, bütün mantığın tek bir sınıfa yerleştirilmesi anlamına gelmez. Güncel bir fayda sağlamayan karmaşıklıktan kaçınmak anlamına gelir.

### 4.3 Her Durum İçin Tek Yetkili Sahip

Her önemli durum değerinin tek bir doğru kaynağı bulunmalıdır.

Diğer sistemler değerleri gözlemleyebilir, gösterebilir veya geçici olarak önbelleğe alabilir. Ancak birbirleriyle yarışan sahipler hâline gelmemelidir.

### 4.4 Tanım, Çalışma Zamanı Durumu ve Sunumu Ayır

Projede üç kavram birbirinden ayrılır:

```
Tanım
    Değişmeyen tasarım verileri

Çalışma Zamanı Durumu
    Mevcut nesneye veya tura ait değişebilir veriler

Sunum
    Görsel, ses, animasyon ve kullanıcı arayüzü
```

Örnek:

```
ShotgunDefinition
    Hasar, şarjör büyüklüğü, seviye ayarları ve prefab referansları

ShotgunRuntime
    Mevcut mühimmat, reload durumu ve geçici modifier’lar

ShotgunView
    Model, animasyon, namlu efekti ve ses
```

Her eşya için mutlaka üç ayrı sınıf oluşturulması gerekmez. Ayrım kavramsaldır ve yalnızca gerçek karmaşıklığın gerektirdiği ölçüde uygulanmalıdır.

### 4.5 Derin Kalıtım Yerine Composition Tercih Et

Ortak yetenekler genellikle odaklanmış bileşenlerden veya servislerden birleştirilmelidir.

Kimlik, davranış, durum ve sunumu tek kalıtım ağacında birleştiren derin yapılar kullanılmamalıdır.

Kalıtım şu koşullarda kabul edilebilir:

- İlişki gerçekten birbirinin yerine kullanılabilir durumdaysa.
- Ortak davranış kararlıysa.
- Alt sınıfların temel davranışları kapatması veya tersine çevirmesi gerekmiyorsa.

### 4.6 Mümkün Olduğunda Unity’yi Sistem Sınırlarında Tut

MonoBehaviour şu Unity’ye özgü sorumluluklar için kullanılmalıdır:

- Transform.
- Fizik.
- Çarpışma.
- Animasyon.
- Sahne referansları.
- Unity yaşam döngüsü.
- Görsel ve işitsel sunum.

Saf oynanış hesaplamaları, açıklığı ve test edilebilirliği artırıyorsa MonoBehaviour dışında tutulmalıdır.

Bu tercih, bütün Unity API’lerinin zorunlu olarak interface arkasına alınması anlamına gelmez.

### 4.7 Bağımlılıkları Açık Tut

Bir sistem yalnızca ihtiyaç duyduğu bağımlılıkları almalıdır.

Bir silah yalnızca stat kaynağına ve mühimmat ayarlarına ihtiyaç duyuyorsa tüm `RunSession` nesnesini almamalıdır.

Büyük context nesneleri gizlenmiş service locator’lara dönüşmemelidir.

### 4.8 Global Değişebilir Durumdan Kaçın

Projede şunlardan kaçınılacaktır:

- `GameManager.Instance`
- Statik ve değişebilir oynanış verileri
- Global service locator
- Ortak değişebilir ScriptableObject’ler
- Global event bus

Statik sınıflar uygun durumlarda durumsuz yardımcı fonksiyonlar veya sabit değerler için kullanılabilir.

### 4.9 İletişim Yöntemini Amaca Göre Seç

Şu durumlarda doğrudan metot çağrısı kullan:

- Çağıran sistem işi hangi sistemin yapacağını biliyorsa.
- Anında bir sonuç gerekiyorsa.
- İlişki açık ve yerelse.

Şu durumlarda C# event kullan:

- Bir olay zaten gerçekleşmişse.
- Birden fazla bağımsız sistem tepki verebilecekse.
- Gönderen sistem alıcıları bilmemeliyse.

Örnekler:

```
Doğrudan çağrı:
ChestRewardGenerator.GenerateChoices()

Event:
EnemyDied
PlayerLevelledUp
RunEnded
```

### 4.10 Interface’leri Gerçek Sınırlarda Kullan

Interface kullanılabilecek anlamlı sınırlar:

- Birden fazla implementasyona sahip hasar alabilen hedefler.
- Deterministik testlerde kullanılan rastgele sayı kaynakları.
- Kayıt depolama implementasyonları.
- Hedef seçme stratejileri.
- Zamana bağlı kuralların test edilmesi gerektiğinde zaman kaynakları.

Interface’ler odaklanmış ve küçük kalmalıdır.

### 4.11 ScriptableObject’leri Değişmeyen Tanımlar Olarak Kullan

Çalışma zamanı kodu, kalıcı ScriptableObject asset verilerini değiştirmemelidir.

ScriptableObject oynanış sırasında okunabilir. Değişebilir değerler ise çalışma zamanı nesnelerine aittir.

Editör doğrulamaları şunları kontrol etmelidir:

- Boş değişmeyen kimlikler.
- Tekrarlanan kimlikler.
- Eksik seviye verileri.
- Geçersiz evrim referansları.
- Geçersiz maksimum seviyeler.
- Eksik zorunlu asset’ler.

### 4.12 Belirsiz Sorumluluklar İçin Manager İsmi Kullanma

`Manager` kelimesi açık olmayan bir sorumluluğun yerine kullanılmamalıdır.

İsimler gerçek rolü anlatmalıdır:

```
RunCoordinator
SceneFlowController
ChestRewardGenerator
EnemyRegistry
LoadoutController
```

`Manager` ismi yalnızca yönetilen sorumluluk dar ve açık biçimde tanımlanmışsa kabul edilebilir.

### 4.13 Tekrar Geliştirmeye Uygun Tasarla

İlk uygulama basit olabilir.

Şu durumlarda refactor yapılmalıdır:

- Sorumluluk sınırları belirsizleşirse.
- Gerçek kod tekrarı ortaya çıkarsa.
- İkinci bir implementasyon bir sınır gerektirirse.
- Test etmek gereksiz derecede zorlaşırsa.
- Performans ölçümleri bir darboğaz gösterirse.

Yalnızca tasarım deseni eklemek için refactor yapılmamalıdır.

### 4.14 Verileri Erken Doğrula

Geçersiz içerikler sessiz çalışma zamanı hataları oluşturmak yerine geliştirme sırasında fark edilmelidir.

Doğrulamalar şu yöntemlerle gerçekleştirilebilir:

- Uygun durumlarda `OnValidate`.
- Katalog doğrulaması.
- Otomatik testler.
- Editör uyarıları.
- Development build assertion’ları.

Oyuncuya sunulan build’ler kurtarılabilir hataları güvenli biçimde ele almalı ve anlamlı loglar oluşturmalıdır.

### 4.15 Performansı Hedef Build’lerde Ölç

Editor performansı nihai performans referansı değildir.

Profiling, temsilî donanımlarda çalışan development build’lerde gerçekleştirilmelidir.

Performans çalışmaları şu ölçülen maliyetlere odaklanmalıdır:

- CPU frame süresi.
- GPU frame süresi.
- Garbage collection allocation’ları.
- Bellek kullanımı.
- Draw call sayısı.
- Aktif düşman ve mermi sayısı.
- Fizik ve navigation maliyeti.
- VFX overdraw.

---

## 5. SOLID Uygulama Kuralları

### Single Responsibility Principle

Bir sınıfın tek ve açık bir değişme nedeni olmalıdır.

Bir sınıf, farklı iş birlikçilerini koordine edebilir. Ancak bu koordinasyon onun tanımlanmış sorumluluğu olmalıdır.

### Open/Closed Principle

Sistemler, gerçek çeşitlilik bulunan yerlerde yeni tanımlar ve odaklanmış davranışlar aracılığıyla yeni içeriği desteklemelidir.

Henüz anlaşılmamış gelecek değişiklikleri için önceden soyutlama oluşturulmayacaktır.

### Liskov Substitution Principle

Kalıtım yalnızca türetilmiş implementasyonlar temel türün yerine beklenen davranışı değiştirmeden kullanılabiliyorsa uygulanmalıdır.

Alt sınıflar temel sınıfın büyük bölümünü devre dışı bırakıyorsa composition tercih edilmelidir.

### Interface Segregation Principle

Interface’ler yalnızca kullanıcılarının ihtiyaç duyduğu işlemleri içermelidir.

Birbiriyle ilgisiz oynanış işlevlerini sunan büyük interface’ler bölünmeli veya kullanılmamalıdır.

### Dependency Inversion Principle

Üst düzey oynanış kuralları, test edilebilirlik veya sürdürülebilirliği olumsuz etkileyen değiştirilebilir altyapılara doğrudan bağımlı olmamalıdır.

Örnekler:

- Rastgelelik üretimi.
- Kayıt depolama.
- Platform servisleri.

Dependency inversion her somut sınıfa mekanik olarak uygulanmayacaktır.

---

## 6. Mimari Kalite Kontrolü

Önemli bir sistem kabul edilmeden önce şu sorular cevaplanmalıdır:

1. Sistemin kesin sorumluluğu nedir?
2. Hangi durumun sahibidir?
3. Hangi bağımlılıklara ihtiyaç duyar?
4. Bu bağımlılıklar görünür müdür?
5. Aynı durumun başka bir yetkili kopyası var mı?
6. Önemli kurallar tam bir oynanış sahnesi yüklemeden test edilebilir mi?
7. İlgisiz sistemler değiştirilmeden yeni içerik eklenebilir mi?
8. Kullanılan soyutlama mevcut bir problemi çözüyor mu?
9. Sistem sık çalışan bir update yolunda allocation veya pahalı işlem yapıyor mu?
10. Tasarım altı ay sonra da anlaşılabilir kalacak mı?

Çalışan ancak bu kontrollerin birkaçında başarısız olan bir sistem tamamlanmış kabul edilmeden önce yeniden değerlendirilmelidir.

---

## 7. Kesinleşen Teknik Yön

| Alan | Karar |
| --- | --- |
| Input | Unity Input System |
| Oyuncu hareketi | CharacterController |
| Render Pipeline | URP |
| Temel el konsolu hedefi | Steam Deck sınıfı donanım |
| Sahne yapısı | Ayrı arena sahneleri |
| Sahne yükleme | Asenkron Single yükleme |
| Tur durumu | Saf C# çalışma zamanı modeli |
| Kalıcı sahne kökü | Sınırlandırılmış tek `ApplicationRoot` |
| İçerik tanımları | Uygun yerlerde ScriptableObject |
| Çalışma zamanı verileri | Ayrı değişebilir modeller |
| Bağımlılık erişimi | Açık injection ve referanslar |
| Global service locator | Kullanılmayacak |
| Global event bus | Kullanılmayacak |
| DI framework | Başlangıçta kullanılmayacak |
| Kayıt biçimi | Sürümlendirilmiş yerel profil verisi |
| Addressables | Vertical slice sonrasında seçici kullanım |
| Optimizasyon | Profiling odaklı |
| Mimari yaklaşım | Basit, açık, veri odaklı ve test edilebilir |