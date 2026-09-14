# Düşman çeşitliliği temeli

## Durum ve başlangıç

Kullanıcının kabul ettiği mekanikler doğrultusunda Charger adımının uygulama sözleşmesidir. Oynanış kabulü testten sonra ayrıca kaydedilecektir.

Plastic `/main/dev/enemy-variety-foundation`, birleştirilmiş dev `cs:173` üzerinden açıkça oluşturuldu. Git `feature/enemy-variety-foundation`, birleştirilmiş main `b8aa45f` üzerinden oluşturuldu. Gelişmiş sandıklar otomatik doğrulama ile merge edildi; kullanıcının oynanış değerlendirmesi bekleniyor.

GDD bölüm 17 taktiksel düşman çeşitliliğini ister; aileleri ve boss mekaniklerini henüz belirlemez. Bu adım kararlaştırılan Charger ailesini uygular.

## Kabul edilen mekanikler

- Mevcut yakın mesafe takipçisi Chaser temel davranış olarak korunur.
- Charger yaklaşır, görünür hazırlığın başında yönünü sabitler, o yönde atılır ve yeniden takibe başlamadan önce toparlanır. Oyuncu uyarı sırasında yana hareket ederek kaçabilir. Hazırlık süresi, atılma hızı, mesafesi ve toparlanması ayarlanabilir; ilk sayılar oynanış testi gerektirir.
- Rank (`Normal`, `Elite`, `Boss`) davranıştan ayrılır. Bir Elite, Chaser veya Charger ailesinden olabilir. Rank tek başına stat çarpanı uygulamaz veya ganimet seçmez; can, hasar, hareket, XP ve drop profili tanımda açıkça ayarlanır.
- İlk oynanabilir elite örneği, geçici sunumu ve ayarlanabilir zamanlamalarıyla ayırt edilen Elite Charger olur. Saldırı kodunu çoğaltmadan aynı davranış sözleşmesini kullanır.
- Temelde Boss sınıflandırması ayrılır. Oynanabilir boss eklenmeden önce karşılaşması ve saldırıları ayrıca tasarlanır.

## Teknik sınırlar

Mevcut EnemyDefinition, açık hedef aktarımı, EnemyRegistry, HealthComponent, EnemyMotor ve ölüm yaşam döngüsü kullanılır. Navigasyon motor içinde kalır; atılma navigasyon sınırlarına uyar ve engelde durur. Bir atılma aynı hedefe tekrar tekrar hasar veremez. Pause, ölüm, disable ve geçersiz hedef saldırı işlemeyi güvenli biçimde durdurmalı veya askıya almalı; iptal sonrasında gecikmiş hasar oluşmamalıdır.

Mevcut EnemyChestDropProfile ve ağırlıklı ChestDropTable kullanılır. Elite içeriğine gelişmiş sandıkları içeren ayrı bir tablo atanabilir; ihtimal ve ağırlıklar rank'tan çıkarılmaz, kullanıcıyla belirlenir. Tek ölüm bildirimi, tek XP ödülü ve en fazla tek sandık düşürme denemesi korunur. Boss ödül bağlantısı boss karşılaşmasının tasarımını izler.

## Uygulama

EnemyChargeState zamanlamayı ve tek hasar hakkını yönetir; EnemyAttackController tanımdan Chaser veya Charger davranışını seçer. EnemyMotor düz ve navigasyona uygun hareketi yürütür. EnemyChargeView uyarı çizgisini, kameraya dönen etiketi ve geçici rengi yönetir. Mevcut EnemySpawner iki davranışı da var olan başlatma akışından kurar. EnemyDefinition ve EnemySpawnRequest bilinmeyen rank/davranış verilerini kayıt veya üretim öncesinde reddeder.

ED_Charger ve ED_EliteCharger mevcut düşman prefabını ve açık drop profillerini kullanır. Eski Common-only elite tablosu gelişmiş nadirlik dengelemesi kararlaştırılana kadar korunur.

## Doğrulama kapsamı

EditMode: varsayılan/geçersiz rank, yapılandırma sınırları, saldırı geçişleri, sabitlenen yön, bekleme süresi ve atılma başına tek hasar.

PlayMode: hedef aktarımı, engeller, pause/ölüm/disable iptali, registry temizliği, normal ve elite üretimi, tek XP/drop davranışı ve mevcut wave/sandık regresyonları.

## Charger uygulama sözleşmesi

Yön hazırlığın BAŞINDA sabitlenir; böylece uyarının tamamı kaçış fırsatıdır. Ölçekli zaman pause sırasında hazırlığı, hareketi ve toparlanmayı askıya alır. Ölüm, disable, Stop ve eksik/pasif/ölü hedef saldırıyı iptal eder; yeniden etkinleşme yeni hazırlık gerektirir. Hasar gerçek hareket parçası boyunca taranır; büyük kare adımlarında da atılma başına en fazla bir kez uygulanır. NavMesh sınırları ve katı engeller hareketi sınırlar; sabitlenmiş saldırı sırasında engelin etrafından yol bulunmaz.

Geçici normal değerler: 100 can, 15 hasar, 25 XP, 7 başlatma mesafesi, 0.8 sn hazırlık, 10 atılma hızı, 8 mesafe, 0.7 temas yarıçapı, 1.2 sn toparlanma. Elite örneği: 200 can, 20 hasar, 50 XP, 0.65 sn hazırlık, 12 hız, 1.0 sn toparlanma. Rank gizli çarpan uygulamaz. Mevcut normal/elite drop profilleri (%25/%50, Common tabloları) ilk açık atamalar olarak korunur; gelişmiş nadirlik ağırlıkları ayrı denge kararı gerektirir.

Test_Waves ilk dalgayı korur; ikinci dalgada bir Chaser, bir Charger ve bir Elite Charger bulunur. Ayrı sunum bileşeni rank/renk ve yerde yön uyarısı gösterir; collider ölçeklenmez. Boss yalnızca sınıflandırmadır. Evolution oynanışı, altın, meta ilerleme ve nihai denge sonraki işlerdir.

2026-09-14 doğrulaması: izole Unity 6000.3.9f1 projesinde **695/695 EditMode**, **371/371 PlayMode** testi geçti. Kapsam kameraya dönen uyarı sunumu, hareket boyunca hasar taraması, yana kaçarak kurtulma, katı engeller, NavMesh sınırları, pause/iptal, normal/elite üretimi ve tek XP/sandık drop akışını içerir. Eski yalnızca-Chaser sahne kontrolü rank'a özel drop bağlantısını doğrulayacak şekilde güncellendi. Önceki adımdan AdvancedChestContentTests içinde eksik kalan generic koleksiyon namespace import'u geri eklendi. Elle oynanış kabulü ve nihai denge bekleniyor.
