# Eşya seviyesi temeli

Devam adımı: [Silah Seviyesi Etkileri](Weapon-Level-Effects.md), aynı branch'te seviye verisini silah hasarı ve süre/kapasite profillerine bağlar. Aşağıdaki bölümler önceki yalnızca veri adımını anlatır; yetenek/geliştirme etkileri ve seviye ödülleri sonraki işlerdir.

## Başlangıç ve sıra

Üç Common sandığın kullanıcı onayı ve merge'i sonrasında birleşmiş Plastic `/main/dev` **cs:160**, Git `main` **80699d2** tabanından başlar. Branch'ler `/main/dev/item-level-foundation` ve `codex/item-level-foundation`dır; önceki feature branch taban alınmaz.

Bağımlılık sırası:

1. Run'a ait seviye verisi, yapılandırılabilir maksimum, doğrulama ve uyumluluk (bu adım).
2. Mevcut silah/yetenek/geliştirme örneklerinde seviyeye özel gerçek etkiler; mermi, şarjör doldurma/bekleme süresi ve slot sahipliğinin korunması.
3. Açık edinme/seviye artırma ödülleri, atomik claim, mevcut/sonraki seviye gösterimi ve maksimum seviye uygunluğu.

Bu adım yalnızca 1'i uygular. Sandıklar hâlâ yeni eşya verir; seviye artırmaz. Sadece veri temeli bitince eşya seviyesi ödüllerinin tamamlandığı söylenmez veya bütün özellik bitmiş gibi merge yapılmaz.

## Veri sözleşmesi

`ItemDefinition.MaximumLevel` içerik verisidir; gerçek seviye etkileri tanımlanana kadar mevcut içerik için varsayılanı 1'dir. Sıfır/negatif maksimum reddedilir, sessizce düzeltilmez. GDD temel silah ve yetenekler için sekiz seviye hedefler; henüz uygulanmayan yedi ödülü açmak anlamına gelmez. Geliştirmelerin seviye sayısı tasarımda açıktır; ortak bir sayı uydurulmaz.

`PlayerBuild`, (kategori, büyük/küçük harfe duyarlı stable ID) başına bir `PlayerBuildItem` tutar. Edinmede maksimumun kopyası alınır, seviye 1'den başlar. Değişken run seviyesi asset'e yazılmaz. Farklı build'ler bağımsızdır; asset'in sonradan değiştirilmesi edinilmiş eşyanın maksimumunu değiştirmez. Mevcut string listeleri ve edinme sonuçları korunur. Yalnızca ID alan eski eklemeler tek seviyeli kabul edilir.

Salt okunur kayıt özellikleri kategori, ID, mevcut/maksimum seviye ve maksimuma ulaşma bilgisini sunar. Sahip olunmayan eşya sorgusu false (seviye sorgusu 0) döner. Modelin `TryLevelUp` işlemi tam bir seviye artırır; sahiplik yoksa NotOwned, maksimumdaysa MaximumLevelReached döndürür ve veriyi değiştirmez. Yeni slot harcamaz. Tekrar edinme AlreadyOwned döndürür; seviyeyi artırmaz, sıfırlamaz veya maksimumu değiştirmez. Geçersiz kategori/ID/maksimum değişiklikten önce reddedilir.

`PlayerBuildController.TryAdd(ItemDefinition)` tanımdaki maksimumu doğrulayıp kopyalar. Seviye değiştirme API'si şimdilik saf build modelindedir; gerçek etkiler atomik güncellenmeden sandık/claim/debug üzerinden çağrılmaz.

## Doğrulama ve kapsam dışı

2026-09-14 doğrulaması: izole Unity 6000.3.9f1 projesinde **655 EditMode / 332 PlayMode geçti**. Yeni 29 EditMode senaryosu bu veri temelini kapsar; mevcut tam PlayMode paketi değiştirilmeden geçti. Yeni/değişen dokuz Unity kod, test ve meta dosyası SHA-256 ile test kopyasıyla eşleşti. Bu veri adımının otomatik doğrulamasıdır; sandıktan seviye artırmanın oynanış onayı değildir.

Üç kategori, başlangıç/sahiplik yokluğu, maksimum sınırı, dolu slot, tekrar edinme, run bağımsızlığı, kayıtların dışarıdan değiştirilememesi, salt okunur sahiplik listeleri, tanım doğrulaması ve maksimum kopyası test edilir. Edinme, silah değiştirme, yetenek, geliştirme ve sandık regresyonları için iki tam test paketi çalıştırılır.

Gerçek seviye etkileri, sekiz seviyelik örnek dengelemesi, seviye UI'ı, ödül uygunluğu değişikliği, evrim, altın telafisi, kayıt göçü ve debug seviye düğmesi kapsam dışıdır. Henüz oynanış değişikliği beklenmez. Ödül olarak seviye artışı açılmadan önce gerçek etkilerin uygulanmasıyla devam edilir.
