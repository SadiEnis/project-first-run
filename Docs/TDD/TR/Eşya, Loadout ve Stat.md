# Eşya, Loadout ve Stat

# Eşya, Loadout ve Stat Mimarisi

## 1. Amaç

Bu doküman; eşyaların teknik olarak nasıl temsil edileceğini, oyuncunun tur içindeki loadout verisinin nasıl saklanacağını, ekipmanların arena geçişlerinden sonra nasıl yeniden oluşturulacağını, oyuncu statlarının nasıl hesaplanacağını ve pasif güçlendirmelerle trait’lerin nasıl uygulanacağını tanımlar.

Mimari şunları desteklemelidir:

- Oyuncu tarafından kontrol edilen silahlar.
- Otomatik kullanılan yetenekler.
- Pasif stat güçlendirmeleri.
- Oynanışı değiştiren trait’ler.
- Eşya seviyeleri.
- Ekipman slot sınırları.
- Eşya evrimleri.
- Sandık ödülü filtreleme.
- Arenalar arasında çalışma zamanı nesnelerinin yeniden oluşturulması.
- Kalıcı slot geliştirmeleri.

Sistem, her şeyi kapsayan genel bir eşya veya effect framework’üne dönüşmeden açık ve tip güvenli kalmalıdır.

Bu dokümandaki sınıf adları çalışma adlarıdır ve geliştirme sırasında değişebilir. Kesin isimlerinden daha önemli olan sorumluluklarıdır.

---

## 2. Tasarım Hedefleri

Eşya, loadout ve stat mimarisi:

- ScriptableObject tanımlarını değişebilir çalışma zamanı durumundan ayırmalıdır.
- Tur boyunca korunacak ekipmanları sabit eşya kimlikleriyle saklamalıdır.
- Oyuncunun çalışma zamanı nesnelerinin arena geçişlerinden sonra güvenli biçimde yeniden kurulmasını sağlamalıdır.
- Sandık ödülü uygunluğunun kolayca sorgulanmasını sağlamalıdır.
- Tekrarlanan veya geçersiz ekipman durumlarını engellemelidir.
- Evrimlerin aynı slottaki eşyayı değiştirmesine izin vermelidir.
- Sayısal ve davranışsal gelişimleri desteklemelidir.
- Stat değişikliklerini davranış değiştiren trait’lerden ayırmalıdır.
- Derin kalıtım hiyerarşilerinden kaçınmalıdır.
- Her eşya için tek bir evrensel effect sistemi oluşturmamalıdır.
- Tam bir oynanış sahnesi yüklenmeden test edilebilir olmalıdır.

---

## 3. Temel Kavramlar

Mimari beş kavramı birbirinden ayırır:

```
Eşya Tanımı
    ScriptableObject asset’lerinde saklanan değişmeyen içerik verisi

Loadout Durumu
    Eşya kimliklerini, seviyeleri ve slot konumlarını içeren tur verisi

Eşya Çalışma Zamanı
    Sahneye ait silah, yetenek veya trait davranışı

Stat Koleksiyonu
    Sahneye ait hesaplanmış oyuncu statları

Sunum
    Model, animasyon, ses, VFX ve kullanıcı arayüzü
```

Örnek:

```
ShotgunDefinition
    Değişmeyen silah ayarları
        ↓
LoadoutEntry
    Item ID: weapon.shotgun
    Level: 5
        ↓
ShotgunRuntime
    Mevcut mühimmat ve reload durumu
        ↓
ShotgunView
    Model, animasyon, ses ve VFX
```

Bu kavramların her zaman ayrı sınıflara dönüşmesi gerekmez. Ancak sahiplik sınırları açık kalmalıdır.

---

## 4. Eşya Kategorileri

Başlangıçtaki eşya kategorileri:

```
public enum ItemCategory
{
    Weapon,
    Ability,
    Upgrade
}
```

Kategori şunları belirler:

- Eşyanın hangi slot havuzunu kullandığını.
- Hangi sandık türlerinde sunulabileceğini.
- Hangi çalışma zamanı oluşturma yolunun kullanılacağını.
- Hangi seviye ve evrim kurallarının geçerli olduğunu.

Stat güçlendirmeleri ve trait’ler `Upgrade` kategorisinin üyeleridir. Oyuncu tarafında ayrı slot havuzları kullanmazlar.

---

## 5. Eşya Tanım Modeli

Eşya tanımları değişmeyen ScriptableObject asset’leridir.

Bütün eşya kategorileri ortak kimlik ve sunum verilerine ihtiyaç duyduğu için sığ bir ortak tanım hiyerarşisi anlamlıdır.

Kavramsal yapı:

```
ItemDefinition
├── WeaponDefinition
├── AbilityDefinition
└── UpgradeDefinition
```

### 5.1 Ortak Eşya Verileri

`ItemDefinition` şunları içerebilir:

```
Sabit Kimlik
Görünen Ad
Açıklama
İkon
Eşya Kategorisi
Normal Ödül Havuzu Uygunluğu
Seviye Sayısı
```

Sabit kimlik şunlar tarafından kullanılır:

- Tur durumu.
- Kayıt verileri.
- Eşya katalogları.
- Evrim tanımları.
- Sandık ödülleri.
- Hata ayıklama ve analiz.

Örnek kimlikler:

```
weapon.shotgun
weapon.minigun
weapon.rocket_launcher
ability.force_wave
ability.fireball
upgrade.damage
```

Oyuncuya gösterilen isimler kalıcı kimlik olarak kullanılmamalıdır.

### 5.2 Kategoriye Özgü Veriler

`WeaponDefinition` şunları içerebilir:

- Silah çalışma zamanı prefab’ı.
- Silah seviye verileri.
- Şarjör ayarları.
- Ateş etme ayarları.
- Silah sunum referansları.

`AbilityDefinition` şunları içerebilir:

- Yetenek çalışma zamanı prefab’ı.
- Yetenek seviye verileri.
- Bekleme süresi ayarları.
- Ortak olduğu durumlarda hedefleme ayarları.
- Yetenek sunum referansları.

`UpgradeDefinition` şunları içerebilir:

- Güçlendirme türü.
- Güçlendirme seviye verileri.
- Stat modifier tanımları.
- Gerekirse trait çalışma zamanı bilgileri.

Tip güvenli, benzersiz ayarlar gerektiğinde daha odaklanmış tanım alt sınıfları kullanılabilir.

Örnekler:

```
ShotgunDefinition : WeaponDefinition
RocketLauncherDefinition : WeaponDefinition
DroneDefinition : AbilityDefinition
```

Bir eşyaya özel veriler ortak kategori tanımına ait değilse özel bir alt sınıf oluşturulmalıdır.

---

## 6. Eşya Seviye Verileri

### 6.1 Seviye Temsili

Eşya seviye verileri, art arda uygulanan değişiklikler yerine kümülatif anlık görüntüler kullanmalıdır.

Bir seviye anlık görüntüsü, eşyanın o seviyedeki nihai ayarlarını tanımlar.

Örnek:

```
Shotgun Seviye 5
├── Hasar: beşinci seviyedeki nihai hasar
├── Şarjör: beşinci seviyedeki nihai kapasite
├── Saçma Sayısı: beşinci seviyedeki nihai sayı
├── Knockback: beşinci seviyedeki nihai değer
└── Reload Süresi: beşinci seviyedeki nihai süre
```

Bu yaklaşım önceki bütün seviye değişikliklerini yeniden uygulamaya tercih edilir.

Faydaları:

- Eşya herhangi bir seviyede doğrudan yeniden oluşturulabilir.
- Arena geçişlerinde eski geliştirmelerin tekrar uygulanması gerekmez.
- Save migration daha kolay olur.
- Seviye davranışları daha kolay test edilir.
- Nihai değer geliştirmelerin uygulanma sırasına bağlı kalmaz.

İleride editör araçları, tekrarlanan veri girişini azaltmak için önceki seviyenin değerlerini kopyalayabilir. Çalışma zamanı verileri yine kümülatif kalır.

### 6.2 Seviye Kuralları

- Temel eşyalar birinci seviyede başlar.
- Normal bir eşya tanımındaki maksimum seviyeye kadar geliştirilebilir.
- Maksimum seviye, geçerli seviye anlık görüntülerinin sayısından elde edilir.
- Sıfırıncı seviye sahip olunan bir eşya için geçerli değildir.
- Evrimleşmiş bir eşyanın şu anda tek seviyesi vardır ve daha fazla geliştirilemez.
- Maksimum seviyedeki eşyalar normal seviye yükseltme ödüllerinden çıkarılır.

### 6.3 Ödül Açıklamaları

Her seviye, oyuncuya gösterilecek bir geliştirme açıklaması içerebilir.

Örnek:

```
Seviye 8:
“Ek saçmalar ekler.”
```

Açıklama teknik uygulamayı değil, oynanış değişikliğini anlatmalıdır.

---

## 7. Normal Ödül Uygunluğu

Her eşya tanımı normal sandık ödül havuzunda bulunmamalıdır.

Eşya tanımı basit bir normal ödül uygunluğu ayarına sahip olmalıdır.

Örnekler:

| Eşya | Normal Ödül Havuzu |
| --- | --- |
| Shotgun | Dahil |
| Alev Topu | Dahil |
| Hasar Güçlendirmesi | Dahil |
| Meteor Evrimi | Hariç |
| Evrimleşmiş Shotgun | Hariç |

Evrim sonucu olan eşyalar yalnızca evrim sistemiyle edinilir.

Başlangıçta karmaşık bir edinme kuralı grafiği oluşturulmayacaktır. Daha ayrıntılı açılma koşulları ileride profil gelişimi üzerinden filtrelenebilir.

---

## 8. Eşya Kataloğu

`ItemCatalog`, bütün kayıtlı eşya tanımlarını içeren uygulama yaşam süreli bir katalogdur.

Sorumlulukları:

- Sabit eşya kimliklerini tanımlara dönüştürmek.
- Kategoriye göre eşya sorguları sağlamak.
- Tekrarlanan eşya kimliklerini tespit etmek.
- Eksik tanımları tespit etmek.
- Ödül havuzu filtrelemesini desteklemek.
- Gelecekte Addressables entegrasyonuna imkân sağlamak.

Kavramsal sorgu:

```
"weapon.shotgun"
        ↓
ItemCatalog
        ↓
ShotgunDefinition
```

İlk katalog doğrudan ScriptableObject referansları tutabilir.

Kanıtlanmış bir yükleme veya bellek yönetimi ihtiyacı oluşmadan Addressables katalog sistemine eklenmemelidir.

### 8.1 Katalog Kuralları

- Her eşya kimliği benzersiz olmalıdır.
- Her kayıtlı tanım boş olmayan bir kimliğe sahip olmalıdır.
- Tanımın kategorisi çalışma zamanı türüyle uyuşmalıdır.
- Evrim sonucu tanımları katalogda bulunmalıdır.
- Geçersiz katalog verileri geliştirme sırasında doğrulama hatası oluşturmalıdır.

---

## 9. Loadout Durumu

`LoadoutState`, `RunSession` içinde bulunur.

Saf C# veri modelidir ve sahne nesnesi referansları içermez.

Kavramsal yapı:

```
LoadoutState
├── Kapasiteler
│   ├── Silah Slotları
│   ├── Yetenek Slotları
│   └── Güçlendirme Slotları
│
├── Silahlar
├── Yetenekler
└── Güçlendirmeler
```

Sahip olunan her eşya bir `LoadoutEntry` ile temsil edilir.

```
LoadoutEntry
├── Item ID
└── Level
```

Kayıtların sırası oyuncunun slot sırasını temsil eder.

Silah değiştirme sistemi slot sırasına bağlı olabileceği için bu özellikle silahlarda önemlidir.

Evrim, aynı liste konumundaki kaydı değiştirir.

---

## 10. Slot Kapasitesi

Slot kapasiteleri tur başladığında belirlenir.

Şu şekilde hesaplanır:

```
Varsayılan Slot Kapasitesi
        +
Kalıcı Profil Slot Geliştirmeleri
        ↓
Tur Slot Kapasitesi Anlık Görüntüsü
```

Bu anlık görüntü `LoadoutState` içinde saklanır.

Mevcut geçici değerler:

| Kategori | Başlangıç Kapasitesi | Önerilen Maksimum |
| --- | --- | --- |
| Silah | 1 | 2 |
| Yetenek | 3 | 5 |
| Güçlendirme | 5 | 8 |

Bu değerler oynanış testlerinden sonra değişebilir.

İleride geçici slot değişiklikleri sağlayan bir oynanış özelliği eklenmediği sürece slot kapasitesi tur sırasında değişmez.

---

## 11. Run Loadout Domain Nesnesi

Çalışma adı `RunLoadout` olan saf C# domain nesnesi, loadout kurallarının ve değişikliklerinin sahibidir.

`LoadoutState` üzerinde çalışır.

Sorumlulukları:

- Bir eşyanın sahip olunup olunmadığını sorgulamak.
- Mevcut eşya seviyesini sorgulamak.
- Kategoride kalan kapasiteyi sorgulamak.
- Yeni bir eşyanın alınıp alınamayacağını doğrulamak.
- Bir eşyanın seviye atlayıp atlayamayacağını doğrulamak.
- Yeni eşya eklemek.
- Eşya seviyesini artırmak.
- Evrim aracılığıyla eşyayı değiştirmek.
- Slot sırasını korumak.
- Geçersiz loadout değişikliklerini reddetmek.

Şunları yapmamalıdır:

- GameObject oluşturmak.
- Kullanıcı arayüzünü güncellemek.
- Efekt oynatmak.
- Sandık ödülleri üretmek.
- Eşya asset’lerini yüklemek.
- Silah değiştirmeyi kontrol etmek.

---

## 12. Loadout Değişmez Kuralları

Aşağıdaki kurallar her zaman doğru kalmalıdır:

1. Bir eşya kimliği oyuncunun loadout’unda yalnızca bir kez bulunabilir.
2. Eşya, kategorisine uygun koleksiyonda saklanmalıdır.
3. Kayıt sayısı kategori kapasitesini aşamaz.
4. Eşya seviyesi 1 ile maksimum seviyesi arasında kalmalıdır.
5. Evrim sonucu kaynak eşyanın aynı slotuna yerleşmelidir.
6. Bir silah yalnızca başka bir silaha evrilebilir.
7. Bir yetenek yalnızca başka bir yeteneğe evrilebilir.
8. Güçlendirmeler şu anda evrim kaynağı değildir.
9. Evrim sonucu loadout’un başka bir yerinde zaten bulunmamalıdır.
10. Geçersiz işlemler loadout’u kısmen değiştirmemelidir.

Evrim değişimi atomik olmalıdır: İşlem tamamen başarılı olur veya loadout hiç değişmeden kalır.

---

## 13. Eşya Edinme Akışı

Sandık ödülleri doğrudan sahne çalışma zamanı nesnelerini değiştirmez.

Normal edinme akışı:

```
Oyuncu ödülü seçer
        ↓
Ödül seçimi doğrulanır
        ↓
RunLoadout değişikliği yapılır
        ↓
Loadout değişiklik sonucu oluşturulur
        ↓
Sahne çalışma zamanı senkronize edilir
        ↓
Oyuncu arayüzü güncellenir
        ↓
Geri bildirim oynatılır
```

Olası değişiklik sonuçları:

```
ItemAdded
ItemLevelIncreased
ItemEvolved
RejectedNoCapacity
RejectedMaximumLevel
RejectedDuplicate
RejectedInvalidCategory
RejectedInvalidDefinition
```

Kesin uygulama bir sonuç enum’u ve isteğe bağlı sonuç verisi kullanabilir.

---

## 14. Ödül Uygunluğu Sorguları

Sandık sistemi loadout’u değiştirmeden inceleyebilmelidir.

Gerekli sorgular:

```
CanAcquireNewItem(definition)
CanLevelUpItem(itemId)
IsItemOwned(itemId)
IsItemAtMaximumLevel(itemId)
HasAvailableSlot(category)
```

Sandık ödül sistemi bu sorguları şunlar için kullanır:

- Maksimum seviyedeki eşyaları çıkarmak.
- Yeni eşya adaylarından zaten sahip olunan eşyaları çıkarmak.
- Kategori slotları doluyken yeni eşyaları çıkarmak.
- Altın ödülüne geçilmesi gereken durumu belirlemek.
- Geçerli evrim sonuçlarını belirlemek.

Ödül filtreleme ödül sistemine; sahiplik ve kapasite kuralları ise `RunLoadout` sistemine aittir.

---

## 15. Eşyaların Çalışma Zamanında Yeniden Kurulması

Arena sahnesi yüklendiğinde oyuncunun çalışma zamanı ekipmanları `LoadoutState` üzerinden yeniden oluşturulur.

Kavramsal sıra:

```
Arena yüklenir
    ↓
PlayerLoadoutController, LoadoutState verisini alır
    ↓
Her Item ID, ItemCatalog üzerinden çözümlenir
    ↓
Silah ve yetenek çalışma zamanı nesneleri oluşturulur
    ↓
Mevcut eşya seviye anlık görüntüleri uygulanır
    ↓
Güçlendirme stat modifier’ları uygulanır
    ↓
Aktif trait çalışma zamanları oluşturulur
    ↓
Sunum ve HUD güncellenir
```

`PlayerLoadoutController`, sahne yaşam süreli bir koordinatördür.

Sorumlulukları:

- Loadout durumundan sahne çalışma zamanı ekipmanlarını kurmak.
- Eşya değişikliklerinden sonra çalışma zamanı nesnelerini senkronize etmek.
- Silah slot sırasını korumak.
- Eşya çalışma zamanı nesnelerini oluşturmak ve kaldırmak.
- Çalışma zamanı eşyalarını oyuncu bağımlılıklarına bağlamak.

Eşya kimlikleri ve seviyelerinin yetkili sahibi olmamalıdır. Bu sahiplik `LoadoutState` içinde kalır.

---

## 16. Sahneye Ait Eşya Çalışma Zamanı Durumu

Aşağıdaki değerler sahneye ait eşya çalışma zamanı nesnelerine aittir:

### Silah Çalışma Zamanı Durumu

- Mevcut şarjör mühimmatı.
- Reload ilerlemesi.
- Ateş bekleme süresi.
- Tepme durumu.
- Geçici silah etkileri.
- Mevcut silah sunum durumu.

### Yetenek Çalışma Zamanı Durumu

- Mevcut bekleme süresi.
- Aktif etki süresi.
- Mevcut hedefler.
- Oluşturulmuş kalıcı yetenek nesneleri.
- Geçici davranış durumu.

### Trait Çalışma Zamanı Durumu

- Aktif koşul durumu.
- Kayıtlı event abonelikleri.
- Geçici modifier handle’ları.
- Gerekiyorsa iç bekleme süreleri.

Bu değerler ScriptableObject tanımlarında saklanmaz.

Daha sonraki bir tasarım kararı bunu değiştirmediği sürece mühimmat, bekleme süresi ilerlemesi ve geçici eşya durumları yeni arenaya girildiğinde sıfırlanır.

---

## 17. Evrim Tanımları

Evrim tarifleri eşya tanımlarından ayrı saklanır.

Kavramsal yapı:

```
EvolutionDefinition
├── Sabit Evrim Kimliği
├── Kaynak Eşya
├── Gereksinimler
└── Sonuç Eşyası
```

Her gereksinim şunları içerir:

```
Gerekli Item ID
Gerekli Seviye
```

Bu yapı aşağıdaki gibi koşulları destekler:

```
Alev Topu Seviye 8
+
Cooldown Güçlendirmesi Seviye 6
        ↓
Meteor
```

Birden fazla gereksinimi desteklemenin ilk içerik için gerekli olup olmadığı yeniden değerlendirilecektir. Ancak birden fazla gereksinim zaten olası bir oyun tasarımı ihtiyacı olduğu için veri modeli küçük bir liste kullanabilir.

### 17.1 Evrim Doğrulaması

Evrim yalnızca şu koşullarda uygundur:

- Kaynak eşyaya sahip olunması.
- Kaynak eşyanın gerekli seviyeye ulaşması.
- Bütün ek gereksinimlerin karşılanması.
- Sonuç eşyasının bulunması.
- Sonuç kategorisinin kaynak kategorisiyle uyuşması.
- Sonuç eşyasına zaten sahip olunmaması.
- Kaynak eşyanın geçerli bir slotta bulunması.

### 17.2 Evrim Değişikliği

Evrim seçildiğinde:

```
Kaynak slotu bul
    ↓
Kaynak kaydı kaldır
    ↓
Sonuç kaydını aynı konuma ekle
    ↓
Sonuç seviyesini 1 yap
    ↓
Etkilenen çalışma zamanı nesnesini yeniden kur
```

Evrimleşmiş tanımın mevcut maksimum seviyesi 1’dir.

---

## 18. Stat Sistemine Genel Bakış

Stat sistemi, birden fazla oynanış sistemi tarafından kullanılan sayısal oyuncu özelliklerini yönetir.

Örnekler:

- Maksimum can.
- Zırh.
- Hareket hızı.
- Genel hasar.
- Silah ateş hızı.
- Reload hızı.
- Yetenek cooldown yenilenme hızı.
- Toplama yarıçapı.
- Deneyim kazancı.
- Şans.

Stat sistemi şunları içermemelidir:

- Mevcut can.
- Mevcut mühimmat.
- Mevcut deneyim.
- Eşya seviyeleri.
- Mermi sayısı.
- Shotgun saçma sayısı.
- Düşmanlardaki aktif durum etkileri.

Bu değerler kendi çalışma zamanı sistemlerine veya eşyaya özel seviye ayarlarına aittir.

---

## 19. Başlangıç Stat Kimlikleri

İlk tip güvenli stat listesi:

```
public enum StatId
{
    MaxHealth,
    Armor,
    MovementSpeed,
    GlobalDamageMultiplier,
    WeaponFireRateMultiplier,
    ReloadSpeedMultiplier,
    AbilityCooldownRate,
    PickupRadius,
    ExperienceGainMultiplier,
    Luck
}
```

Yeni statlar yalnızca gerçek bir oynanış ihtiyacı ortaya çıktığında eklenmelidir.

Kritik ihtimali, şarjör boyutu, mermi hızı veya patlama yarıçapı gibi silaha özgü değerler gerçekten ortak oyuncu statlarına dönüşmedikleri sürece silah seviye verilerinde kalmalıdır.

---

## 20. Stat Anlamları

Statların açık anlamları ve birimleri bulunmalıdır.

| Stat | Anlamı |
| --- | --- |
| MaxHealth | Oyuncunun maksimum canı |
| Armor | Hasar azaltma formülünün kullandığı değer |
| MovementSpeed | Oyuncunun saniyedeki dünya birimi cinsinden hareket hızı |
| GlobalDamageMultiplier | Uygun oyuncu hasarlarına uygulanan çarpan |
| WeaponFireRateMultiplier | Silah ateş hızına uygulanan çarpan |
| ReloadSpeedMultiplier | Reload hızı çarpanı; yüksek değer daha hızlı reload anlamına gelir |
| AbilityCooldownRate | Cooldown yenilenme çarpanı; yüksek değer daha kısa cooldown anlamına gelir |
| PickupRadius | Toplanabilir nesnelerin çekildiği veya toplandığı yarıçap |
| ExperienceGainMultiplier | Kazanılan deneyime uygulanan çarpan |
| Luck | Açıkça desteklenen olasılık sistemlerinin kullandığı değer |

Cooldown yenilenme hızı kullanmak negatif veya geçersiz cooldown sürelerini önler.

Örnek:

```
Temel Cooldown: 10 saniye
Cooldown Rate: 1.25
Etkili Cooldown: 10 / 1.25 = 8 saniye
```

---

## 21. Temel Stat Anlık Görüntüsü

Tur başladığında:

```
Varsayılan Oyuncu Statları
        +
Kalıcı Profil Geliştirmeleri
        ↓
Tur Temel Stat Anlık Görüntüsü
```

Bu anlık görüntü `RunSession` içinde saklanır.

Tur süresince değişmeden kalır.

Sahne çalışma zamanı, oyuncu oluşturulurken bu anlık görüntüden bir `StatCollection` oluşturur.

Kalıcı profil verileri çatışma sırasında sürekli sorgulanmaz.

---

## 22. Stat Modifier’ları

Bir stat modifier’ı şunları içerir:

```
Stat ID
İşlem
Değer
Kaynak ID
Modifier Anahtarı
```

Başlangıçtaki modifier işlemleri:

```
public enum StatModifierOperation
{
    Flat,
    AdditivePercent
}
```

Başlangıç hesaplaması:

```
Nihai Değer =
    (Temel Değer + Flat Modifier Toplamı)
    ×
    (1 + Toplamsal Yüzde Modifier’ları)
```

Örnek:

```
Temel Maksimum Can: 100
Flat Modifier: +20
Yüzde Modifier: +%15

Nihai Maksimum Can:
(100 + 20) × 1.15 = 138
```

Dengeleme gerçek bir ihtiyaç göstermeden ayrı bir çarpımsal modifier aşaması eklenmemelidir.

---

## 23. Modifier Kaynakları

Her modifier tanımlanabilir bir kaynağa sahip olmalıdır.

Örnekler:

```
upgrade.damage
upgrade.heavy_armor
trait.berserker.active
temporary.arena_blessing
```

Kaynak ID ile modifier anahtarının birleşimi sistemin şunları yapmasını sağlar:

- Eşya seviye atladığında eski modifier’ı değiştirmek.
- Koşul sona erdiğinde modifier’ı kaldırmak.
- Yanlışlıkla tekrar uygulanmayı engellemek.
- Hata ayıklarken nihai değeri açıklamak.
- Arenalar arasında eşyadan gelen modifier’ları yeniden oluşturmak.

Modifier kaydı, hızlı kaldırma için bir handle döndürebilir.

---

## 24. Stat Collection

`StatCollection`, saf C# sahne çalışma zamanı nesnesidir.

Sorumlulukları:

- Temel stat değerlerini saklamak.
- Modifier eklemek.
- Modifier kaldırmak.
- Aynı kaynaktan gelen modifier’ları değiştirmek.
- Nihai stat değerlerini hesaplamak.
- Değerleri geçerli aralıklara sınırlamak.
- İlgili statlar değiştiğinde yerel kullanıcıları bilgilendirmek.
- Modifier kaynakları için hata ayıklama bilgisi sağlamak.

Koleksiyon dirty-value cache kullanmalıdır.

Bir stat yalnızca şu durumlarda yeniden hesaplanır:

- Temel değeri değişirse.
- Onu etkileyen bir modifier eklenirse.
- Onu etkileyen bir modifier kaldırılırsa.
- Onu etkileyen bir modifier değiştirilirse.

Bütün statlar her frame yeniden hesaplanmamalıdır.

---

## 25. Stat Değer Sınırları

Geçersiz nihai değerler engellenmelidir.

Örnekler:

- Maksimum can sıfırın üzerinde kalmalıdır.
- Hareket hızı negatif olmamalıdır.
- Ateş hızı çarpanı güvenli bir minimumun üzerinde kalmalıdır.
- Reload hızı çarpanı güvenli bir minimumun üzerinde kalmalıdır.
- Cooldown rate güvenli bir minimumun üzerinde kalmalıdır.
- Toplama yarıçapı negatif olmamalıdır.
- Deneyim kazanım çarpanı negatif olmamalıdır.

Kesin minimum ve maksimum değerler dengeleme sırasında belirlenecektir.

Sınırlama kuralları farklı tüketicilerde tekrar edilmek yerine merkezî tutulmalıdır.

---

## 26. Stat Güçlendirmelerinin Uygulanması

Bir stat güçlendirmesi seviyesi, o seviyedeki nihai modifier tanımını içerir.

Örnek:

```
Hasar Güçlendirmesi
Seviye 1: +%10
Seviye 2: +%20
Seviye 3: +%30
```

Güçlendirme ikinci seviyeden üçüncü seviyeye çıktığında:

```
upgrade.damage kaynağındaki modifier’ı kaldır veya değiştir
        ↓
Üçüncü seviye anlık görüntüsünü uygula: +%30
```

Sistem önceki +%20 değerinin üzerine ayrıca +%30 eklememelidir.

Bu yaklaşım eşya seviye verilerini kümülatif tutar ve modifier birikme hatalarını engeller.

---

## 27. Trait Güçlendirmeleri

Trait’ler davranışı değiştirir ve yalnızca stat değerleriyle temsil edilmez.

Örnekler:

- Berserker.
- Vampire.
- Ammo Expert.
- Lucky.
- Blood Pact.
- Glass Cannon.
- Heavy Armor.

Bir trait şunların ikisini de içerebilir:

- Kalıcı stat modifier’ları.
- Kendine ait çalışma zamanı davranışı.

Örnek:

```
Heavy Armor
├── Zırh modifier’ı
└── Hareket hızı cezası
```

Örnek:

```
Berserker
├── Mevcut can oranını gözlemler
├── Belirli bir sınırın altında etkinleşir
├── Geçici hasar modifier’ı ekler
└── Koşul sona erdiğinde modifier’ı kaldırır
```

Örnek:

```
Vampire
├── Uygun düşman ölümlerini gözlemler
├── Kurallarına göre can yeniler
└── Gerekirse iç cooldown veya olasılık kullanır
```

---

## 28. Trait Çalışma Zamanı Sınırı

Trait’lerin kesin çalışma zamanı oluşturma yöntemi, Güçlendirme ve Trait Sistemi dokümanında tanımlanacaktır.

Bu modül şu kuralları belirler:

- Trait tanımları değişmeden kalır.
- Trait çalışma zamanı durumu sahneye aittir.
- Trait çalışma zamanı nesneleri yalnızca açık yerel event’lere abone olabilir.
- Trait yok edildiğinde event abonelikleri kaldırılmalıdır.
- Trait kaynaklı modifier’lar tanımlanabilir kaynaklar kullanmalıdır.
- Trait davranışı ScriptableObject asset’lerini değiştirmemelidir.
- Trait çalışma zamanı nesneleri arena geçişlerinden sonra loadout durumundan yeniden oluşturulmalıdır.

Başlangıçta tek bir evrensel trait yorumlayıcısı oluşturulmayacaktır.

Davranış gerçekten farklıysa odaklanmış trait implementasyonlarına izin verilir.

---

## 29. Eşyadan Gelen ve Geçici Modifier’lar

Sahip olunan güçlendirmelerden doğrudan gelen modifier’lar ayrı ve yetkili tur verisi olarak saklanmaz.

Şunlardan yeniden oluşturulabilirler:

```
Loadout Kaydı
+
Upgrade Definition
+
Mevcut Güçlendirme Seviyesi
```

`RunSession` yalnızca şu modifier’ları saklayabilir:

- Sahip olunan bir eşyayla temsil edilmeyen.
- Arena geçişlerinde korunması gereken.
- Tur genelindeki geçici bir etkiden oluşan modifier’lar.

Bu yaklaşım aynı modifier’ın hem loadout içinde hem de ayrı bir modifier listesinde saklanmasını engeller.

Bu bölüm önceki `RunSession.StatState` önerisini daha kesin hâle getirir.

---

## 30. Çalışma Zamanı Senkronizasyonu

Arena içinde bir loadout değişikliği gerçekleştiğinde:

```
RunLoadout durumu değişir
        ↓
PlayerLoadoutController değişiklik sonucunu alır
        ↓
Etkilenen çalışma zamanı nesnesi eklenir, güncellenir, kaldırılır veya değiştirilir
        ↓
Gerekli stat modifier’ları yenilenir
        ↓
Gerekli trait çalışma zamanları yenilenir
        ↓
HUD güncellenir
```

Sistem her ödül sonrasında bütün çalışma zamanı nesnelerini yeniden oluşturmak yerine yalnızca etkilenen kategori veya eşyayı güncellemelidir.

Tam yeniden oluşturma yeni bir arenaya girerken kullanılır.

---

## 31. Yerel Event’ler

Loadout sistemi şu gibi yerel C# event’leri sunabilir:

```
ItemAdded
ItemLevelChanged
ItemEvolved
LoadoutRebuilt
```

Bunlar global event değildir.

Olası dinleyiciler:

- Oyuncu loadout çalışma zamanı senkronizasyonu.
- HUD.
- Geri bildirim sunumu.
- Geliştirme hata ayıklama araçları.

Değişiklik event gönderilmeden önce tamamlanmalıdır.

Event’ler gerçekleşmiş durum değişikliklerini bildirir; değişikliği talep etmez.

---

## 32. Editör Doğrulaması

Proje geliştirme sırasında eşya içeriklerini doğrulamalıdır.

Doğrulamalar:

- Boş eşya kimlikleri.
- Tekrarlanan eşya kimlikleri.
- Gerektiği yerde eksik ikon veya çalışma zamanı prefab’ı.
- Boş seviye listeleri.
- Geçersiz seviye sayıları.
- Eksik seviye açıklamaları.
- Geçersiz kategori atamaları.
- Evrim kategori uyuşmazlıkları.
- Eksik evrim gereksinimleri.
- Normal ödül havuzuna yanlışlıkla dahil edilmiş evrim sonuçları.
- Geçersiz stat modifier değerleri.
- Aynı seviyede tekrarlanan modifier anahtarları.

Doğrulama yöntemleri:

- `OnValidate`.
- Katalog doğrulama araçları.
- Edit Mode testleri.
- Gerektiğinde özel editör uyarıları.

---

## 33. Test Stratejisi

Saf C# Edit Mode testleri şunları kapsamalıdır:

### Loadout Testleri

- Boş slot varken eşya eklemek.
- Slotlar doluyken yeni eşyayı reddetmek.
- Tekrarlanan eşyayı reddetmek.
- Sahip olunan eşyanın seviyesini artırmak.
- Maksimum seviyedeki geliştirmeyi reddetmek.
- Slot sırasını korumak.
- Evrim aracılığıyla eşyayı değiştirmek.
- Geçersiz evrim kategorisini reddetmek.
- Başarısız işlemlerin durumu değiştirmediğini doğrulamak.

### Stat Testleri

- Flat modifier hesabı.
- Toplamsal yüzde modifier hesabı.
- Birleşik modifier hesabı.
- Aynı kaynaktan gelen modifier’ı değiştirmek.
- Koşullu trait modifier’ını kaldırmak.
- Stat sınırlarını uygulamak.
- Tekrarlanan modifier uygulamasını engellemek.
- Loadout durumundan statları yeniden oluşturmak.

### Katalog Testleri

- Bilinen eşya kimliklerini çözümlemek.
- Tekrarlanan kimlikleri reddetmek.
- Eksik tanımları tespit etmek.
- Evrim referanslarını doğrulamak.

Play Mode testleri şunları doğrulamalıdır:

- Çalışma zamanı eşya oluşturma.
- Silah slot sırası.
- Evrim sonrasında çalışma zamanı nesnesinin değiştirilmesi.
- Sahne yok edilirken trait temizliği.
- Arena yüklemesinden sonra yeniden oluşturma.

---

## 34. Performans Konuları

Loadout ve stat mimarisi sık allocation oluşturmaktan kaçınmalıdır.

Kurallar:

- Sık tekrarlanan oynanış yollarında LINQ kullanılmamalıdır.
- Katalog dictionary’leri başlangıçtan sonra cache edilmelidir.
- Statlar yalnızca modifier değiştiğinde yeniden hesaplanmalıdır.
- Eşya çalışma zamanı nesnelerini bulmak için sahne araması yapılmamalıdır.
- Sıradan bir stat değişiminden sonra tüm loadout yeniden oluşturulmamalıdır.
- Reflection tabanlı çalışma zamanı eşya oluşturma kullanılmamalıdır.
- Mermiler ve geçici eşya efektleri, eşya tanımlarından ayrı olarak pool edilmelidir.
- Ödül filtrelemesi frame update döngülerinde yapılmamalıdır.

Aynı anda kuşanılan eşya sayısı düşük olduğu için karmaşık veri yapılarından önce açıklık tercih edilmelidir.

---

## 35. Açık Teknik Sorular

Aşağıdaki sorular henüz cevaplanmamıştır:

- Oyuncu yeni bir tura hangi eşyayla başlayacak?
- Oyuncu başlangıç silahını seçebilecek mi?
- Maksimum can arttığında mevcut can da artmalı mı?
- Maksimum can azaldığında mevcut can oransal olarak değişmeli mi?
- Oyuncu evrimleşmemiş bir eşyayı isteyerek kaldırabilecek veya değiştirebilecek mi?
- Bir eşyanın birden fazla olası evrimi bulunabilir mi?
- Her evrim gereksiniminin maksimum seviyede olması zorunlu mu?
- Kalıcı ilerleme başlangıçta kapalı olan eşyaları açabilecek mi?
- Güçlendirme seviye verileri her zaman tek bir kümülatif modifier anlık görüntüsü mü içermeli?
- Luck statını hangi olasılık sistemleri kullanacak?
- Geçici tur geneli modifier’ları bütün arena geçişlerinde korunmalı mı?
- Silah mühimmatı veya yetenek cooldown’ları gelecekte arenalar arasında korunacak mı?
- Development build’ler için özel bir oyuncu stat hata ayıklama paneli gerekli mi?

---

## 36. Kesinleşen Teknik Kararlar

| Konu | Karar |
| --- | --- |
| Tanım depolama | Değişmeyen ScriptableObject asset’leri |
| Çalışma zamanı kalıcılığı | Sabit eşya kimlikleri ve seviyeleri |
| Tanım hiyerarşisi | Sığ, kategori tabanlı hiyerarşi |
| Benzersiz davranış verisi | Yalnızca gerektiğinde odaklanmış alt sınıflar |
| Seviye verileri | Kümülatif seviye anlık görüntüleri |
| Loadout sahipliği | `RunSession` içindeki `LoadoutState` |
| Loadout kuralları | Saf C# `RunLoadout` domain nesnesi |
| Slot sırası | Sıralı kategori koleksiyonlarıyla korunacak |
| Tekrarlanan eşyalar | İzin verilmeyecek |
| Çalışma zamanı yeniden kurma | Arena girişinde loadout üzerinden |
| Evrim | Aynı slotta atomik eşya değişimi |
| Evrimleşmiş eşya seviyesi | Daha fazla seviyesi olmayan Seviye 1 |
| Güçlendirme slotları | Stat güçlendirmeleri ve trait’ler için ortak |
| Stat kimlikleri | Tip güvenli enum |
| Stat işlemleri | Başlangıçta Flat ve Additive Percent |
| Stat yeniden hesaplama | Dirty-value cache |
| Trait davranışı | Gerektiğinde kendine ait çalışma zamanı davranışı |
| Evrensel effect graph | Başlangıçta kullanılmayacak |
| Normal ödül uygunluğu | Eşya bazında açıkça tanımlanacak |
| Eşyadan gelen modifier’lar | Loadout üzerinden yeniden oluşturulacak |
| Global loadout event’leri | Kullanılmayacak |
| Doğrulama | Editör ve otomatik test doğrulaması |