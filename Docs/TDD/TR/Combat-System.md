# Combat Foundation

## 1. Amaç

Bu modül, Project First Run içerisindeki can, hasar ve ölüm kurallarının ortak
temelini oluşturur.

Silahlar, otomatik yetenekler, düşman saldırıları ve çevresel tehlikeler aynı
hasar sözleşmesini kullanmalıdır.

İlk sürüm aşağıdaki özellikleri kapsar:

- Hasar isteği oluşturma
- Hasar alabilen hedefleri temsil etme
- Maksimum ve mevcut can yönetimi
- Ölüm durumunun belirlenmesi
- Hasar sonucunun çağıran sisteme döndürülmesi
- Yerel can ve ölüm bildirimleri
- Hedef kuklası üzerinde doğrulama

---

## 2. Tasarım Hedefleri

- Can kuralları Unity bileşenlerinden mümkün olduğunca bağımsız olmalıdır.
- Mevcut can için tek bir doğruluk kaynağı bulunmalıdır.
- Silahlar belirli bir düşman sınıfına bağımlı olmamalıdır.
- Ölüm kararı ile ölüm animasyonu veya nesne yok etme davranışı ayrılmalıdır.
- Geçersiz hasar değerleri güvenli şekilde reddedilmelidir.
- Sistem ileride object pooling ile kullanılabilmelidir.
- Aynı ölüm için ölüm bildirimi yalnızca bir kez gönderilmelidir.

---

## 3. Sistem Sınırları

### DamageInfo

Bir hasar isteğinin Unity çalışma zamanı bağlamını taşır.

İlk sürümde:

- Amount
- Source
- Hit Point
- Hit Direction

alanlarını içerir.

DamageInfo kalıcı kayıt verisi değildir ve sahneler arasında saklanmaz.

### DamageResult

Hasar işleminin sonucunu çağıran sisteme döndürür.

İlk sürümde:

- İstenen hasar
- Gerçekte uygulanan hasar
- Önceki can
- Yeni can
- Hasarın uygulanıp uygulanmadığı
- Hasarın öldürücü olup olmadığı

bilgilerini içerir.

### IDamageable

Hasar verebilen sistemler ile hasar alan hedefler arasındaki sınırdır.

Silah ve yetenek sistemleri doğrudan Enemy, Player veya Prop sınıflarına bağımlı
olmamalıdır.

Sözleşme:

`DamageResult ApplyDamage(in DamageInfo damageInfo)`

### HealthState

Saf C# çalışma zamanı modelidir.

Sorumlulukları:

- Maksimum canı saklamak
- Mevcut canı saklamak
- Hasarı uygulamak
- Canı sıfır ile maksimum can arasında sınırlamak
- Ölüm durumunu belirlemek
- Yeniden kullanım için canı sıfırlamak

Unity GameObject, MonoBehaviour, animasyon, ses veya efekt sistemlerini bilmez.

### HealthComponent

HealthState ile Unity sahnesi arasındaki köprüdür.

Sorumlulukları:

- Başlangıç maksimum canını almak
- HealthState oluşturmak
- IDamageable sözleşmesini uygulamak
- DamageInfo içindeki hasarı HealthState'e aktarmak
- Can değişimi ve ölüm bildirimlerini yayınlamak
- Pooling sonrasında açıkça sıfırlanabilmek

Sorumlu olmadığı alanlar:

- Nesneyi yok etmek
- Ölüm animasyonu oynatmak
- Loot üretmek
- Deneyim vermek
- Skor veya görev ilerlemesi
- Hasar sayısı göstermek

---

## 4. Hasar Kuralları

- Sıfır veya negatif hasar uygulanmaz.
- Ölü bir hedef yeni hasarı kabul etmez.
- Mevcut can sıfırın altına düşmez.
- Uygulanan hasar, hedefin kalan canından büyük olamaz.
- Ölüm, mevcut can ilk kez sıfıra ulaştığında gerçekleşir.
- Ölüm bildirimi yalnızca bir kez gönderilir.
- Can değerleri `float` olarak tutulur.
- Zırh, direnç ve kritik hasar ilk sürümün dışında tutulur.

Örnek:

```text
Mevcut Can: 30
İstenen Hasar: 50
Uygulanan Hasar: 30
Yeni Can: 0
Öldürücü: Evet