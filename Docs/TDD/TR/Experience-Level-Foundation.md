# XP ve Seviye Temeli

Aşağıdaki ilk temel kapsamı, aynı branch'te [XP Düşürme ve Toplama](Experience-Pickups.md) ile genişletildi. Güncel sahne bağlantıları o dokümanda anlatılır.

## Kapsam ve sahiplik

Run XP'si; eşya seviyelerinden, build sahipliğinden ve kalıcı ilerlemeden ayrıdır. Asıl veri sahibi saf C# `ExperienceState` nesnesidir. `PlayerExperienceController` sahne adaptörüdür; ileride run yöneticisi oyuncuyu yeniden oluşturduğunda mevcut state'e bağlanabilir. Singleton veya `DontDestroyOnLoad` eklenmez.

Bu aşama XP eğrisini, state'i, kazanım sonucunu, controller bildirimlerini ve testleri kapsar. Düşman XP değerleri, pickup, HUD, sandık üretimi ve kayıt sonraki aşamalardadır. Mevcut prefab/sahneler değiştirilmez.

## Kurallar

- Oyuncu seviye 1 ve 0 XP ile başlar. XP tam sayıdır.
- L seviyesinden sonraki seviyeye geçiş maliyeti: `ilk maliyet + (L - 1) * seviye başına artış`.
- İki parametre de pozitiftir. Başlangıç varsayılanları 100 ve 50'dir; bunlar geçici dengeleme değerleridir. Bir oyun tasarımı maksimum seviyesi eklenmez.
- `ExperienceDefinition`, değişmez `ExperienceCurve` üretir. Asset düzenlemeleri devam eden run'ın eğrisini değiştirmez.
- Negatif kazanım reddedilir; sıfır kazanım veri değiştirmez ve bildirim üretmez.
- Artan XP korunur; tek kazanımda birden fazla seviye atlanabilir.
- Seviye içi XP ve run boyunca toplam XP ayrı tutulur. Eşikler ve toplam `long` kullanır; aritmetik taşma state değiştirilmeden reddedilir.
- `ExperienceGainResult` kazanılan XP'yi, önceki/sonraki seviyeyi, artan XP'yi, toplamı, yeni eşiği ve kazanılan seviye sayısını değişmez olarak taşır.

## Bildirimler

Controller açıkça initialize edilir; null veya tekrar initialize işlemi mevcut state'i değiştirmez. İlerleme salt okunur özelliklerle sunulur.

Her pozitif kazanımda state güncellendikten sonra bir `ExperienceChanged` bildirimi yayınlanır. Seviye artmışsa aynı sonuçla bir `LevelChanged` bildirimi yayınlanır. Örneğin 100, 150, 200 maliyetleriyle 500 XP kazanmak seviye 4 ve 50 XP sonucunu verir; `LevelsGained` 3 olur. Sandık kaynağı bu sayıyı kullanacak, tek bildirimden yalnızca bir sandık çıkarıldığı varsayılmayacak.

Bildirim sırasında iç içe XP kazanımı sıra tutarlılığı için reddedilir. Dinleyiciler exception fırlatmamalıdır. Ölüm/kontrol/pause uygunluğu sonraki toplama sınırında ele alınır; reward ekranı açılması XP'yi sıfırlamaz.

## Doğrulama ve devam

EditMode testleri eşik/doğrulama, asset snapshot'ı, artan XP, çoklu seviye, sıfır/negatif/büyük kazanım, bağımsız run'lar, sonuç snapshot'ı, initialization, bildirimler, iç içe çağrı ve mevcut state'e bağlanmayı kapsar.

Devam sırası: düşman XP'si ve pickup toplama + basit HUD; level-up sandık kaynağı; diğer düşman/elite/boss sandık kaynakları.
