# Gelişmiş sandık türleri

## Kapsam

Bu aşama mevcut Common kategori sandıklarını karma gelişmiş sandık tanımlarıyla genişletir. Evolution oynanışı, altın ödülleri, Golden Chest ve boss spawn kaynakları bu aşamanın dışındadır.

Uncommon (Green), Rare (Purple çalışma adı), Legendary ve Boss Chest tanımları eklenir. Mevcut Common Weapon, Ability ve Upgrade sandıkları değişmeden korunur.

## Ödül politikası

Her sandık tanımı teklif seçenek sayısını ve izin verilen seçim sayısını açıkça taşır. Nadirlik metadata ve sunum kimliğidir; düşme ihtimalini, eşya seviyesini veya aday uygunluğunu gizlice değiştirmez.

| Sandık | Nadirlik | Teklif seçeneği | Seçim | Havuz |
| --- | --- | ---: | ---: | --- |
| Green | Uncommon | 3 | 1 | Mixed |
| Purple (çalışma adı) | Rare | 4 | 1 | Mixed |
| Legendary | Legendary | 5 | 2 | Mixed |
| Boss | Legendary | 6 | 2 | Mixed |

Uygun eşya sayısı yapılandırılmış teklif sayısından azsa teklif mevcut benzersiz adaylardan oluşur. Etkin seçim sayısı, yapılandırılmış seçim sayısı ile üretilen teklif boyutunun küçüğüdür. Teklif boşsa mevcut `NoEligibleRewards` davranışı korunur: sandık kullanılabilir kalır ve modal açılmaz.

Mevcut uygunluk filtresi sahip olunan maksimum seviyedeki eşyaları, dolu kategorileri ve diğer geçersiz adayları çıkarmaya devam eder. Başarılı seçim mevcut kategori acquisition/level-up handler'larına yönlendirilir. Sandık yalnızca etkin seçimlerin tamamı yapıldıktan sonra tüketilir.

## Çoklu seçim oturumu

Reward claim session seçim durumunun yetkili sahibi olmaya devam eder. Seçilen tanımları kaydeder, aynı seçimin tekrarlanmasını reddeder, kalan seçim sayısını sunar ve yalnızca etkin seçim sayısına ulaşıldığında kapanır. Tek seçimli Common, Green ve Purple sandıklarının mevcut davranışı korunur.

Seçim UI'ı kalan seçim sayısını gösterir, başarılı seçimden sonra ilgili seçeneği devre dışı bırakır ve tamamlanmamış çoklu seçim oturumunda açık kalır. Oyuncu kontrolü ve zaman ölçeği yalnızca oturum kapandığında geri yüklenir.

## İçerik ve kaynaklar

Gelişmiş tanımlar mevcut sandık prefabını ve mevcut geliştirme item asset'lerini kullanır. Karma ödül havuzu Weapon, Ability ve Upgrade tanımlarını içerebilir; kategori sınırı `ChestDefinition` ve `RewardItemPool` tarafından doğrulanır.

Gelişmiş tanımlar deterministik içerik testleri ve showcase bağlantısı için ayrı bir geliştirme kaynak tablosuna eklenir. Daha önce kabul edilen run akışını sabit tutmak için mevcut level-up ve düşman kaynak tablolarının ağırlıkları bu aşamada Common-only kalır. Gelişmiş tanımların gerçek düşme olasılıklarına bağlanması sonraki dengeleme adımıdır.

Boss Chest tanımı doğrulanır ve içerik olarak kullanılabilir; ancak bu aşamada hiçbir enemy veya boss kaynağı onu spawn etmez. Evolution seçeneği/olasılığı, gold fallback, Golden Chest ve kalıcı currency uygulanmaz.

## Sunum

Mevcut placeholder chest view tanım adını ve nadirlik etiketini gösterir. Renkler yalnızca geliştirme sunumudur; gameplay kuralları display rengine veya çalışma adına değil serialized rarity/policy alanlarına bakar.

## Doğrulama

EditMode testleri gelişmiş tanım doğrulamasını, karma havuz sınırlarını, açık rarity/policy değerlerini, teklif/seçim sayısı kısıtlarını ve Common uyumluluğunu kapsar. PlayMode testleri dört, beş ve altı seçenekli teklifleri; çoklu seçim tamamlanmasını; aynı seçimin reddini; sandığın yalnızca son seçimden sonra tüketilmesini ve tek seçimli davranış regresyonunu kapsar.

Manuel doğrulamada geliştirme showcase'ından Green, Purple, Legendary ve Boss tanımları açılır. İlk ikisi geçerli bir seçimden sonra kapanır; Legendary ve Boss ilk seçimden sonra açık kalır, ikinci seçimden sonra kapanır. Evolution veya gold seçeneği beklenmez.

2026-09-14 doğrulaması: izole Unity 6000.3.9f1 projesinde **681 EditMode / 355 PlayMode** testi geçti. Test_Waves düzeninde üç Common başlangıç sandığına ek olarak dört gelişmiş tanım bulunur; kabul edilmiş run akışının rastgeleliği değişmesin diye kaynak tabloları bu aşamada Common-only kalır.
