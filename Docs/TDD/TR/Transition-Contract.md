# Genel geçiş sözleşmesi

## Uygulama sözleşmesi — 15 Eylül 2026

Bu adım geçiş kararını dekor ve fiziksel kapıdan ayırır. `ArenaTransitionController` mevcut prototipte çalışmaya devam eder; bu sözleşme yeni bölge/harita geçişlerinin karar çekirdeğidir.

Her yönlü bağlantı bağımsız bir `RegionTransition` tanımıdır:

- `SourceId` ve `DestinationId` sahne nesnesi adından bağımsız, sabit kimliklerdir.
- `Free` uygunluk kaynağın karşılaşma sonucunu beklemez.
- `EncounterCompleted` yalnızca ilgili bölge karşılaşması tamamlandıysa geçişe izin verir; haritadaki tüm düşmanları sorgulamaz.
- `Walk`, `Relocate` ve `SceneLoad` traversal yöntemini tanımlar. Bunlar aynı karar sözleşmesini kullanır.
- `Returnable` tekrar kullanılabilir. `OneWay` başarılı kullanımdan sonra tüketilir ve aynı yönde tekrar çalışmaz.
- `TryUse` başarısız olduğunda tüketim yapmaz. Geçiş isteği atomik olarak tek kez kabul edilir.
- Hedefin hazır/uygun olmadığı durum geçişi reddeder; bu sınıf düşman üretmez, sahne yüklemez veya oyuncuyu taşımaz.

Bu aşamada `RegionEncounterSession` ile bağlantı olay aboneliği kurulmaz; çağıran katman `Status == Completed` bilgisini açıkça aktarır. Böylece serbest çıkışlar encounter yaşam döngüsüne gizlice bağlanmaz.

## Kabul testleri

EditMode testleri: kimlik doğrulama, serbest geçiş, tamamlanmamış/tamamlanmış karşılaşma şartı, hedef uygunluğu, returnable tekrar kullanım, one-way tüketim, başarısız denemede state korunması ve traversal bilgisinin korunması.

Fiziksel trigger'ın yalnızca gerçek oyuncu koliderini kabul etmesi mevcut `ArenaTransitionController` testlerinde korunur. Trigger, bariyer animasyonu ve sahne hedefi sonraki artımlarda bu sözleşmeye bağlanır.
