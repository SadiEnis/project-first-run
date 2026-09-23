# Genel geçiş sözleşmesi

## Düzeltme sözleşmesi — 15 Eylül 2026

RegionTransition iki uçlu bir bağlantıdır. OneWay yalnızca SourceId → DestinationId yönünü kabul eder; Returnable iki yönde çalışır. Tek kullanımlılık ayrı singleUse ayarıdır ve varsayılan olarak kapalıdır.

- CanBegin mevcut bölge kimliğini, ilgili karşılaşma şartını, hedef hazırlığını ve devam eden işlemi denetler.
- TryBegin her başarılı isteğe ayrı bir Attempt verir; işlem sürerken iki yönden de yinelenen istek reddedilir.
- Complete(attempt) yalnızca güncel işlemi tamamlar. Tek kullanımlılık burada tüketilir.
- Cancel(attempt) tüketmeden yeniden denemeye izin verir. Eski/yabancı Attempt yeni işlemi tamamlayamaz veya iptal edemez.
- Taşıma/yükleme hatasında yürütücü Cancel çağırmalıdır. Walk, Relocate ve SceneLoad yöntem bilgisidir; gerçek sahne yüklemesi roadmap 7 kapsamındadır.
- EncounterCompleted açıkça bağlanan karşılaşmayı kontrol eder. Çift yönlü bağlantıda aynı şart iki yönde geçerlidir.

## Yürüyüş entegrasyonu

RegionPassageController sözleşmeyi oyuncu, karşılaşma, genel trigger hacmi ve isteğe bağlı fiziksel engelle bağlar. Yerel +Z hedef, -Z kaynak tarafıdır. Girişte işlem başlar; oyuncunun tüm fiziksel collider'ları güvenlik hacminden çıkınca çıkış tarafı kontrol edilir. Hedefe çıkmak Complete, geldiği tarafa dönmek Cancel üretir. Başarı CurrentRegionId bilgisini değiştirir; harita/run zaferi üretmez.

Bileşen yürüyüş içindir. Diğer traversal yöntemleri aynı Attempt protokolünü kullanacak ayrı yürütücülere bağlanacaktır. Mevcut Test_Waves eski adapter ile kalır; yeni bileşen PlayMode fixture'ında kurulur.

## Kabul testleri

Yön, bağımsız tek kullanımlılık, başarısız geçişte retry, yinelenen istek, eski completion, yanlış kaynak ve enum doğrulama; gerçek Unity collider'larıyla çift yönlü yürüyüş, geri çekilme, şartlı geçiş ve tek yönlü kapanma.
