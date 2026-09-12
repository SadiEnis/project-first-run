# Silah Sistemi

## 1. Amaç

Bu modül, Project First Run içerisindeki oyuncu silahlarının ortak çalışma
temelini tanımlar.

İlk sürüm aşağıdaki özellikleri kapsar:

- ScriptableObject tabanlı silah tanımı
- Saf C# çalışma zamanı durumu
- Yarı otomatik ve otomatik tetik modu
- Şarjör ve yedek mühimmat
- Ateş hızı sınırı
- Manuel doldurma
- Hitscan atış çözümleme
- Combat Foundation ile hasar entegrasyonu
- Yerel silah bildirimleri
- Plasma Rifle prototipi

Shotgun saçılımı, Minigun hızlanması, Rocket Launcher mermisi ve silah değiştirme
ilk sürümün kapsamı dışındadır.

---

## 2. Tasarım Hedefleri

- Silah verileri çalışma zamanı durumundan ayrılmalıdır.
- ScriptableObject asset'leri çalışma sırasında değiştirilmemelidir.
- Şarjör ve doldurma kuralları Unity bileşenlerinden bağımsız olmalıdır.
- Silah sistemi belirli bir düşman sınıfına bağımlı olmamalıdır.
- Hasar, Combat Foundation içindeki IDamageable sözleşmesi üzerinden uygulanmalıdır.
- Giriş okuma ile silah kuralları birbirinden ayrılmalıdır.
- Görsel, ses ve kamera efektleri silah kurallarının sahibi olmamalıdır.
- Sistem ileride iki silah slotunu ve silah değiştirmeyi destekleyebilmelidir.
- İlk sürüm gereksiz genel bir silah framework'üne dönüştürülmemelidir.

---

## 3. Sistem Sınırları

### ItemDefinition

Silahların ileride loadout ve ödül sistemleriyle ortak kullanacağı temel eşya
tanımıdır.

İlk sürümde yalnızca aşağıdaki ortak alanları taşır:

- Stable ID
- Display Name
- Item Category

Loadout, ödül seçimi ve seviye ilerlemesi bu modülde uygulanmaz.

### WeaponDefinition

Değişmeyen silah yapılandırmasını taşıyan ScriptableObject'tir.

İlk sürümde aşağıdaki verileri içerir:

- Stable ID ve görünen ad
- Tetik modu
- Temel hasar
- Şarjör kapasitesi
- Başlangıç yedek mühimmatı
- Saniyedeki atış sayısı
- Doldurma süresi
- Atış menzili
- Hasar uygulanabilecek LayerMask

WeaponDefinition çalışma sırasında mevcut mermi, cooldown veya reload zamanı
saklamaz.

### WeaponRuntimeState

Saf C# çalışma zamanı modelidir.

Sorumlulukları:

- Şarjördeki mermiyi saklamak
- Yedek mühimmatı saklamak
- Ateş cooldown süresini yönetmek
- Reload durumunu ve kalan süresini yönetmek
- Bir atışın yapılıp yapılamayacağına karar vermek
- Atış sonrasında mermi tüketmek
- Reload tamamlandığında mühimmat aktarmak
- Yeniden kullanım için durumu sıfırlamak

Unity GameObject, MonoBehaviour, Input System, raycast, ses veya efekt sistemlerini
bilmez.

### PlayerWeaponController

Oyuncunun aktif silahını yöneten Unity bileşenidir.

Sorumlulukları:

- Fire ve Reload girişlerini PlayerInputReader üzerinden almak
- WeaponRuntimeState'i her frame güncellemek
- Atış isteğini çalışma zamanı kurallarıyla doğrulamak
- Geçerli atışta hitscan çözümleyicisini çağırmak
- Yerel silah event'lerini yayınlamak
- Silah durumunu UI ve presentation bileşenlerine sunmak

Sorumlu olmadığı alanlar:

- Oyuncu hareketi
- Can ve ölüm kuralları
- Mermi UI çizimi
- Silah animasyonu
- Ses üretimi
- Kamera geri tepmesi
- Loadout ödül seçimi

### HitscanShotResolver

Bir hitscan atışının Unity fizik dünyasındaki sonucunu belirler.

Sorumlulukları:

- Kameranın merkezinden nişan noktasını belirlemek
- Muzzle ile nişan noktası arasındaki görüş hattını doğrulamak
- Yakındaki engellerin arkasına ateş edilmesini önlemek
- Vurulan hedefte IDamageable sözleşmesini çözümlemek
- DamageInfo oluşturmak ve hasarı uygulamak
- Atış sonucunu çağıran sisteme döndürmek

Silahın mermi, cooldown veya reload kurallarını yönetmez.

---

## 4. Input Action Yapısı

Mevcut `Gameplay` action map'ine iki action eklenecektir.

| Action | Type | Control Type | Açıklama |
|---|---|---|---|
| Fire | Button | Button | Aktif silahın tetiği |
| Reload | Button | Button | Manuel şarjör doldurma isteği |

### Binding'ler

#### Fire

- Mouse Left Button
- Gamepad Right Trigger

#### Reload

- Keyboard R
- Gamepad Button West / Xbox X

PlayerInputReader aşağıdaki değerleri sunacaktır:

- IsFireHeld
- WasFirePressedThisFrame
- WasReloadPressedThisFrame

Silah sistemi InputAction nesnelerine doğrudan erişmeyecektir.

---

## 5. Tetik Modları

İlk sürüm iki tetik modunu destekler:

### SemiAutomatic

Her fiziksel basış için en fazla bir atış yapılır.

Tetik basılı tutulduğunda art arda ateş edilmez.

### Automatic

Tetik basılı tutulduğu sürece fire rate sınırları içinde ateş edilir.

Plasma Rifle prototipi ilk olarak Automatic tetik modunu kullanacaktır.

Burst ve charge tabanlı ateş modları ertelenmiştir.

---

## 6. Ateş Kuralları

Bir silah yalnızca aşağıdaki koşullarda ateş edebilir:

- Silah geçerli bir WeaponDefinition ile başlatılmıştır.
- Reload yapılmıyordur.
- Ateş cooldown süresi tamamlanmıştır.
- Şarjörde en az bir mermi vardır.
- Tetik modu için uygun giriş alınmıştır.

Başarılı bir atış:

1. Şarjörden bir mermi azaltır.
2. Yeni ateş cooldown süresini başlatır.
3. Hitscan atışını çözümler.
4. ShotFired bildirimini yayınlar.

Hitscan atışı bir hedefe değmese bile mermi tüketilmiş sayılır.

Boş şarjörle ateş isteği mermi değerini sıfırın altına düşürmez ve DryFired
bildirimi üretebilir.

İlk sürümde boş şarjör otomatik olarak doldurulmaz.

---

## 7. Ateş Hızı

WeaponDefinition saniyedeki atış sayısını taşır.

Atış aralığı:

`fire interval = 1 / shots per second`

Cooldown her frame delta time kullanılarak azaltılır.

Bir frame içerisindeki aşırı delta time nedeniyle sınırsız sayıda telafi atışı
yapılmaz. İlk sürümde bir frame içerisinde en fazla bir atış oluşturulur.

---

## 8. Mühimmat Modeli

WeaponRuntimeState aşağıdaki değerlerin tek doğruluk kaynağıdır:

- MagazineAmmo
- ReserveAmmo

Kurallar:

- Şarjör mühimmatı sıfırın altına düşmez.
- Şarjör mühimmatı kapasiteyi aşmaz.
- Yedek mühimmat sıfırın altına düşmez.
- Reload yalnızca eksik şarjör ve pozitif yedek mühimmat bulunduğunda başlayabilir.
- Reload sırasında ateş edilemez.
- Zaten reload yapılırken yeni reload isteği reddedilir.
- Dolu şarjörle reload başlatılmaz.
- Reload tamamlandığında yalnızca gereken miktar yedekten şarjöre aktarılır.

Örnek:

```text
Magazine Capacity: 12
Magazine Ammo: 5
Reserve Ammo: 4

Reload Sonrası:
Magazine Ammo: 9
Reserve Ammo: 0