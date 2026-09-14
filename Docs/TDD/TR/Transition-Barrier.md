# Geçit bariyeri yaşam döngüsü

## Uygulama sözleşmesi — 15 Eylül 2026

Bu adım, genel geçiş kararının fiziksel/ görsel sunumdan bağımsız bariyer durumunu tanımlar. `TransitionBarrier` kapının modelini, animasyonunu veya collider şeklini bilmez; bir görünüm katmanı `Opened` ve `Closed` olaylarını dinler.

- Bariyer başlangıçta `Closed` durumundadır; `Open` ilk çağrıda bir kez yayınlanır.
- Kapalı bariyer oyuncu girişini kabul etmez. Açık bariyere aynı oyuncunun ikinci collider girişi yeni işlem üretmez.
- Oyuncu içerideyken kapanma istenirse bariyer açık kalır ve kapanma isteğini bekletir.
- Oyuncu tamamen temizlendiğinde bekleyen kapanma bir kez uygulanır. Böylece oyuncu kapının içinde sıkışmaz.
- Oyuncu yokken kapanma hemen uygulanır. Kapalı bariyere yinelenen kapanma çağrısı olay yayınlamaz.
- Yeniden açma, bekleyen kapanmayı iptal eder; yeni bir açılma olayı yayınlamaz.
- Bu sınıf geçiş uygunluğunu, oyuncu teleportunu, sahne yüklemesini veya animasyon süresini yönetmez.

## Kabul testleri

EditMode: idempotent açma, kapalı/tekrarlı giriş, oyuncu içerideyken kapanmayı erteleme, tamamen çıkıştan sonra kapanma, oyuncu yokken anında kapanma, yinelenen kapanma ve bekleyen kapanmayı iptal eden yeniden açma.
