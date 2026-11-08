# Run sahnesinin hazırlanması

## Sorumluluk

`RunSceneStartup`, kayıtlı sahnedeki başlangıç loadout'unu mevcut `PlayerStartingLoadoutInitializer` üzerinden hazırlar. Başlangıç silahı ve slot kapasitesi aynı ScriptableObject ayarından okunmaya devam eder. Loadout zaten hazırlanmışsa yeniden kurulmaz.

XP, sandık ve yetenek servislerinin mevcut kurulum sahipleri korunur. Yeni bileşen bunların hazır olmasını bekler; fabrika kaydı veya servis kurulumunu tekrar etmez. Bu adım bir servis bulucu ya da genel bağımlılık yönetimi sistemi eklemez.

`DemoOpeningController`, ortak hazırlık tamamlandıktan sonra koridor encounter'ının hazırlanmasını ister. İlk sandık, oda ve ölüm ekranı davranışları bu adımda taşınmaz. Oyun akışının izin ve duraklatma kararları `GameFlowController` sorumluluğunda kalır.

## Bağlantılar ve hata davranışı

- Opening ve WideCombat sahnelerinde `Demo opening services` nesnesinde bir `RunSceneStartup` bulunur.
- Inspector referansları: loadout, oyuncunun XP servisi, sandık spawner'ı, yetenek runtime installer'ı.
- Hazırlık beklemesi gerçek zaman kullanır; giriş ekranının zamanı durdurması başlangıcı engellemez.
- Varsayılan 10 saniyede hazır olmayan servisler hata mesajında listelenir. `IsReady` false kalır ve giriş ekranı hatayı gösterir.
- Sahne üreticisi aynı bağlantıları oluşturur; mevcut sahneler yeniden üretilmemiştir.

## Doğrulama

EditMode sahne testi iki sahnedeki başlangıç bağlantılarını denetler. PlayMode açılış kabul testi, oynanış hazır olduğunda loadout, XP, sandık ve yetenek servislerinin hazır olduğunu da doğrular. Unity testlerinin ve oyun içi kabulün kullanıcı tarafından çalıştırılması beklenir.

Elle kontrol: Start ile oyuna giriş, ilk yeteneği alma, pause/ölüm/final üzerinden restart sonrası temiz başlangıç. Meta kayıt/yükleme ve ölüm ekranının ayrılması bu adımın kapsamı dışındadır.
