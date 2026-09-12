# Oyuncu Ölüm Sistemi

## 1. Amaç

Bu modül, Project First Run içerisinde oyuncunun ölüm kararına verdiği gameplay
tepkisini yönetir.

Can ve ölüm kararı mevcut HealthComponent tarafından verilir. Oyuncuya özgü
davranışlar ayrı bir PlayerDeathController tarafından uygulanır.

İlk sürüm aşağıdaki özellikleri kapsar:

- Oyuncu ölümünü dinleme
- Hareket ve kamera kontrolünü kapatma
- Silah kullanımını durdurma
- Cursor kilidini kaldırma
- Ölüm durumunu yalnızca bir kez işleme
- HealthReset sonrasında oyuncu kontrolünü geri açma
- Yerel ölüm ve yeniden canlanma bildirimleri

Ölüm ekranı, respawn, arena yeniden başlatma ve run sonlandırma bu sürümün
kapsamı dışındadır.

---

## 2. Tasarım Hedefleri

- HealthComponent oyuncuya özgü davranışları bilmemelidir.
- PlayerController ölüm kararının sahibi olmamalıdır.
- PlayerWeaponController can sistemine doğrudan bağımlı olmamalıdır.
- Oyuncu ölümü yalnızca bir kez işlenmelidir.
- Kontrol kapatma ve geri açma açık metotlarla yapılmalıdır.
- Sistem HealthReset ile test edilebilir şekilde geri döndürülebilmelidir.
- Global event bus veya GameManager kullanılmamalıdır.

---

## 3. PlayerDeathController

PlayerDeathController, HealthComponent event'leri ile oyuncu gameplay bileşenleri
arasındaki koordinatördür.

Sorumlulukları:

- HealthComponent.Died event'ini dinlemek
- Ölümün yalnızca bir kez işlenmesini sağlamak
- PlayerController kontrolünü kapatmak
- PlayerWeaponController kullanımını durdurmak
- PlayerDied event'ini yayınlamak
- HealthComponent.HealthReset event'ini dinlemek
- Oyuncu kontrolünü yeniden etkinleştirmek
- PlayerRevived event'ini yayınlamak

Sorumlu olmadığı alanlar:

- Can azaltmak
- Hasar hesaplamak
- Enemy saldırıları
- Ölüm ekranı çizmek
- Respawn konumu seçmek
- Sahneyi yeniden yüklemek
- Run sonucunu belirlemek
- Animasyon ve ses oynatmak

---

## 4. Ölüm Akışı

1. HealthComponent öldürücü hasarı uygular.
2. HealthComponent Died event'ini yayınlar.
3. PlayerDeathController ölüm durumuna geçer.
4. PlayerController kontrolü kapatılır.
5. PlayerWeaponController silah kullanımını durdurur.
6. Cursor serbest bırakılır.
7. PlayerDied event'i yayınlanır.

Aynı hedefe gelen sonraki hasarlar yeni ölüm işlemi oluşturmamalıdır.

---

## 5. Kontrol Davranışı

Oyuncu öldüğünde:

- Move girdisi uygulanmaz.
- Sprint uygulanmaz.
- Look girdisi uygulanmaz.
- Fire ve Reload uygulanmaz.
- Aktif reload ilerlemesi durdurulabilir.
- CharacterController yer çekimini işlemeye devam edebilir.
- Oyuncu yatay olarak hareket edemez.

PlayerController içindeki mevcut SetControlEnabled metodu kullanılacaktır.

Silah sistemi için açık bir kontrol metodu kullanılacaktır:

`SetWeaponControlEnabled(bool isEnabled)`

Bu metot WeaponRuntimeState verisini sıfırlamaz. Yalnızca giriş ve runtime
güncellemesini durdurur.

---

## 6. Reset ve Yeniden Etkinleştirme

HealthComponent.ResetHealth çağrıldığında:

1. Oyuncu maksimum cana döner.
2. HealthReset event'i yayınlanır.
3. PlayerDeathController ölüm durumunu temizler.
4. PlayerController yeniden etkinleştirilir.
5. PlayerWeaponController yeniden etkinleştirilir.
6. PlayerRevived event'i yayınlanır.

Silah mühimmatı reset sırasında otomatik olarak yenilenmez. Bu karar respawn
sistemi içinde ayrıca verilecektir.

---

## 7. İlk Sürüm Kabul Kriterleri

- Oyuncu öldüğünde IsDead true olmalıdır.
- Ölüm yalnızca bir kez işlenmelidir.
- Oyuncu ölümden sonra hareket edememelidir.
- Oyuncu ölümden sonra kamerayı çevirememelidir.
- Oyuncu ölümden sonra ateş edememelidir.
- Oyuncu ölümden sonra reload başlatamamalıdır.
- Cursor kilidi kaldırılmalıdır.
- HealthReset oyuncuyu yeniden kontrol edilebilir hâle getirmelidir.
- Reset oyuncunun silah mühimmatını kendiliğinden değiştirmemelidir.
- Console'da error veya sürekli warning oluşmamalıdır.

---

## 8. Ertelenen Konular

- Ölüm ekranı
- Respawn
- Arena yeniden başlatma
- Run başarısızlığı
- Checkpoint
- Ölüm animasyonu
- Kamera düşme efekti
- Ses efektleri
- Gamepad titreşimi
- Yenilmezlik süresi
- Düşmanların ölü oyuncuya saldırmayı bırakması