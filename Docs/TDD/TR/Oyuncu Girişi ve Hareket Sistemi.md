# Oyuncu Girişi ve Hareket Sistemi

## 1. Amaç

Bu modül, Project First Run içerisindeki oyuncu girişlerini, karakter hareketini ve
birinci şahıs kamera kontrolünü yönetir.

Sistemin ilk sürümü aşağıdaki özellikleri destekler:

- Klavye ve fare
- Gamepad
- Yürüme
- Koşma
- Birinci şahıs kamera kontrolü
- Yer çekimi
- Zemin kontrolü

Jump, dash, crouch, head bob ve hareket tabanlı kamera efektleri ilk sürümün
kapsamında değildir.

---

## 2. Tasarım Hedefleri

- Giriş okuma ile fiziksel hareket birbirinden ayrılmalıdır.
- Hareket sistemi belirli bir giriş cihazına bağlı olmamalıdır.
- Input System yalnızca giriş katmanında kullanılmalıdır.
- Hareket kodu mümkün olduğunca test edilebilir ve okunabilir olmalıdır.
- Oyuncu prefab'ı arena sahneleri arasında taşınmamalıdır.
- Oyuncu her arena sahnesinde yeniden oluşturulmalıdır.
- Kontrol ayarları daha sonra yeniden atanabilir olmalıdır.
- Gamepad desteği Steam Deck hedefi göz önünde bulundurularak baştan kurulmalıdır.

---

## 3. Sistem Sınırları

### PlayerInputReader

Sorumlulukları:

- Unity Input System ile iletişim kurmak
- Move, Look ve Sprint girişlerini okumak
- Input Action Map yaşam döngüsünü yönetmek
- Giriş değerlerini gameplay bileşenlerine sunmak

Sorumlu olmadığı alanlar:

- CharacterController hareketi
- Yer çekimi
- Kamera dönüşü
- Oyuncu stat değerleri
- Animasyon

### PlayerMotor

Sorumlulukları:

- Yatay hareket yönünü hesaplamak
- Yürüme ve koşma hızlarını uygulamak
- Yer çekimini hesaplamak
- CharacterController.Move çağrısını yapmak
- Zeminde bulunma durumunu takip etmek

Sorumlu olmadığı alanlar:

- Input System
- Kamera dönüşü
- Oyuncu saldırıları
- Kamera efektleri
- Kalıcı stat yönetimi

### PlayerLook

Sorumlulukları:

- Oyuncu gövdesinin yatay dönüşünü yönetmek
- Kamera pivotunun dikey dönüşünü yönetmek
- Dikey kamera açısını sınırlandırmak
- Fare ve gamepad hassasiyetlerini uygulamak

Sorumlu olmadığı alanlar:

- Hareket
- CharacterController
- Silah geri tepmesi
- Kamera sallanması
- FOV efektleri

### PlayerController

Sorumlulukları:

- PlayerInputReader, PlayerMotor ve PlayerLook bileşenlerini koordine etmek
- Her frame giriş değerlerini ilgili gameplay bileşenlerine aktarmak
- Oyuncu kontrolünün etkinleştirilmesini ve devre dışı bırakılmasını yönetmek

PlayerController gameplay kurallarının veya giriş cihazlarının sahibi değildir.

---

## 4. Input Action Yapısı

Input Action Asset:

`ProjectFirstRunInputActions`

### Gameplay Action Map

| Action | Type | Control Type | Açıklama |
|---|---|---|---|
| Move | Value | Vector2 | Oyuncunun yatay hareket yönü |
| Look | Value | Vector2 | Kamera dönüş girdisi |
| Sprint | Button | Button | Koşma girdisi |
| Pause | Button | Button | Pause menüsünü açma isteği |

### Kontrol Şemaları

#### KeyboardMouse

- Keyboard
- Mouse

#### Gamepad

- Gamepad

Steam Deck, gameplay açısından Gamepad kontrol şeması üzerinden desteklenecektir.

---

## 5. Varsayılan Binding'ler

### Move

KeyboardMouse:

- W: İleri
- S: Geri
- A: Sol
- D: Sağ

Gamepad:

- Left Stick

### Look

KeyboardMouse:

- Mouse Delta

Gamepad:

- Right Stick

### Sprint

KeyboardMouse:

- Left Shift

Gamepad:

- Left Stick Press

### Pause

KeyboardMouse:

- Escape

Gamepad:

- Start

---

## 6. Hareket Modeli

Hareket yönü, oyuncunun yatay yöneliminden hesaplanır:

`movement = forward * inputY + right * inputX`

Hareket vektörü normalleştirilerek çapraz hareketin düz hareketten daha hızlı
olması engellenir.

Yürüme ve koşma hızı PlayerMotor tarafından seçilir. İlerleyen aşamada bu hızlar
StatCollection üzerinden sağlanacaktır.

İlk sürümde hızlanma ve yavaşlama anlıktır. Acceleration ve deceleration yalnızca
oynanış testlerinde ihtiyaç görülürse eklenecektir.

---

## 7. Yer Çekimi

CharacterController otomatik yer çekimi uygulamaz. Bu nedenle dikey hız PlayerMotor
tarafından saklanır ve her frame güncellenir.

Oyuncu zemindeyken küçük bir negatif dikey hız korunur. Bu, CharacterController'ın
zeminle temasını daha kararlı hâle getirir.

İlk sürümde oyuncu zıplayamaz.

---

## 8. Kamera Modeli

Yatay kamera girdisi oyuncu gövdesini Y ekseninde döndürür.

Dikey kamera girdisi yalnızca kamera pivotunu X ekseninde döndürür.

Dikey dönüş aşağıdaki aralıkla sınırlandırılır:

- Minimum pitch: -85 derece
- Maximum pitch: 85 derece

Fare girdisi delta tabanlıdır. Gamepad girdisi ise zaman tabanlı dönüş hızı olarak
işlenir. Bu nedenle fare ve gamepad için ayrı hassasiyet değerleri kullanılır.

---

## 9. Güncelleme Akışı

Her frame:

1. PlayerController giriş değerlerini PlayerInputReader'dan okur.
2. Look girdisi PlayerLook'a aktarılır.
3. Move ve Sprint girdileri PlayerMotor'a aktarılır.
4. PlayerMotor yatay ve dikey hareketi birleştirir.
5. CharacterController.Move bir kez çağrılır.

Fizik tabanlı Rigidbody hareketi kullanılmadığı için hareket işlemi Update içerisinde
çalıştırılır.

---

## 10. Yaşam Döngüsü

PlayerInputReader:

- OnEnable sırasında Gameplay Action Map'i etkinleştirir.
- OnDisable sırasında Gameplay Action Map'i devre dışı bırakır.

Oyuncu kontrolü pause, ödül seçimi veya arena geçişi sırasında devre dışı
bırakılabilir.

Giriş kapatıldığında hareket ve bakış girdileri sıfırlanmalıdır.

---

## 11. İlk Ayar Değerleri

| Ayar | Başlangıç Değeri |
|---|---:|
| Walk Speed | 5 |
| Sprint Speed | 8 |
| Gravity | -25 |
| Grounded Vertical Speed | -2 |
| Mouse Sensitivity | 0.1 |
| Gamepad Look Speed | 160 |
| Minimum Pitch | -85 |
| Maximum Pitch | 85 |

Bu değerler nihai denge değerleri değildir. Oynanış testleri sırasında
değiştirilecektir.

---

## 12. İlk Sürüm Kabul Kriterleri

- Oyuncu WASD ile hareket edebilmelidir.
- Oyuncu sol analog çubukla hareket edebilmelidir.
- Fare ile kamera kontrol edilebilmelidir.
- Sağ analog çubukla kamera kontrol edilebilmelidir.
- Çapraz hareket ek hız üretmemelidir.
- Koşma yalnızca Sprint girdisi aktifken uygulanmalıdır.
- Kamera dikey olarak ters dönmemelidir.
- Oyuncu yer çekimiyle zemine düşmelidir.
- Oyuncu eğimli yüzeylerde CharacterController sınırları içinde hareket etmelidir.
- Input devre dışı bırakıldığında oyuncu hareket etmemelidir.
- Console'da hata veya sürekli warning oluşmamalıdır.

---

## 13. Ertelenen Konular

- Jump
- Dash
- Crouch
- Head bob
- Footstep sistemi
- Kamera sallanması
- Silah geri tepmesi
- Kontrol yeniden atama
- Hassasiyet ayar ekranı
- Gamepad aim acceleration
- Aim assist
- Harici kuvvetler ve knockback

Bu özellikler ilk hareket prototipi doğrulandıktan sonra ayrı kararlarla ele alınacaktır.