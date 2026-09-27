# Force Wave — onaylanan tasarım ve uygulama

Yetenek İçerik Planı'nın ikinci yeteneği; mevcut abilities-content branch'inde ilerlenir. Kullanıcı 2026-09-26'da sektörü, koşullu tetiklemeyi ve başlangıç değerlerini onayladı; oynanışa göre revize edilebilir. GDD 12.1 öne doğru kalabalık kontrolünü ve yükseltme sırasını tanımlar; aşağıdaki sayıları tanımlamaz.

## Önerilen mekanik

- Otomatik, anlık bir ön sektör: toplam 100 derece açı ve başlangıçta 4 m menzil. En yakın düşmana değil, oyuncunun yataydaki baktığı yöne gider. Dikey bakış dalgayı zemine yatırmaz.
- Yalnızca mevcut harita registry'sindeki canlı ve aktif bir düşman sektör içinde ve dünya görüş hattındaysa tetiklenir. Hedef yoksa cooldown tüketilmez. Başka kattaki düşmanları vurmamak için başlangıçta 2 m dikey tolerans kullanılır.
- Menzil, açı ve siper kontrolünde tutarlı biçimde collider merkezi değerlendirilir. Kaynak collider'ları, aktörler ve trigger'lar siper sayılmaz; katı dünya geometrisi etkiyi engeller.
- Bir düşmanın birden fazla collider'ı olsa da atış başına tek hasar uygulanır. Temel hasara AbilityDamage bir kez uygulanır. Kabul edilen öldürücü olmayan isabet, IKnockbackReceiver üzerinden oyuncudan uzağa yatay itme dener; itmenin reddedilmesi uygulanmış hasarı iptal etmez. Hedefler arasında kaynak sağlığı ve hedef yaşamı tekrar kontrol edilir.
- EnemyMotor'un çarpışmayı gözeten mevcut itme davranışı kullanılır. Duvar içinden ışınlama veya ayrı hareket sistemi eklenmez. Bu aşamada boss bağışıklığı ya da rank çarpanı uydurulmaz.
- Gerçekleşen dalga başına tek cooldown. Seviye artışında kalan cooldown korunur. Pause, oyuncu ölümü ve harita geçişi yeni atışı engeller; registry yeniden bağlama korunur.
- Geçici açılan yay görseli isabetten sonra animasyon oynatabilir; tekrar tekrar hasar vermez. Mermi, stun, yanma, evolution oynanışı, altın veya nihai VFX/ses yoktur.

## Önerilen ilk test değerleri

| Seviye | Hasar | Menzil m | Cooldown s | İtme m |
| --- | --- | --- | --- | --- |
| 1 | 20 | 4 | 5 | 2 |
| 2 | 30 | 4 | 5 | 2 |
| 3 | 30 | 5 | 5 | 2 |
| 4 | 30 | 5 | 3 | 2 |
| 5 | 30 | 5 | 3 | 3 |
| 6 | 40 | 5 | 3 | 3 |
| 7 | 40 | 6 | 3 | 3 |
| 8 | 40 | 6 | 1.5 | 3 |

## Oynanış kabulü — 2026-09-26

Kullanıcı mekaniği kabul etti ve değiştirdiği cooldown değerlerini kendisi test etti: L1–3 5 s, L4–7 3 s, L8 1.5 s. Yukarıdaki tablo ve otomatik test beklentileri kullanıcının kaydettiği asset ile eşleştirildi. Kullanıcının isteğiyle bu değişiklikler için otomatik test tekrarı yapılmadı. Önceki test sayıları yalnızca ayar değişikliğinden önceki koşuya aittir; tam pakette kalan sorunlar sonraki regresyon çalışmaları için korunur. Tüm yeteneklerin dengesi ve güçlenmenin daha belirgin hissettirilmesi içerik genişletme sonrasında ele alınacak.

Açı sabit kalır; alan yükseltmeleri menzili büyütür. Sayılar onaylanan ilk oynanış testi değerleridir; nihai denge değildir.

## Uygulama checkpoint'leri

1. Mekaniği netleştirme ve EN/TR docs kaydı.
2. Sektör/siper seçimi, hasar, itme ve sekiz seviyelik runtime yapılandırması; odaklı testler.
3. Kayıtlı ability asset'i, edinme/ödül/F1 kataloğu entegrasyonu ve geçici sunum. Hazır arena korunur; runtime'da yeniden sahne kurulmaz.
4. Tam EditMode/PlayMode regresyonları; ardından Drone öncesinde kullanıcı oynanış kabulü için durulur.

Doğrulama kapsamı: açı/menzil/yükseklik sınırları, hedefsiz cooldown tüketmeme, bakış yönü ile en yakın hedef ayrımı, siper, çoklu collider, öldürücü hasar, reddedilen itme, duvar güvenliği, AbilityDamage hesabı, seviye geçişleri, pause/ölüm, registry yeniden bağlama, maksimum seviye ödülleri ve kayıtlı arenada edinme.

## Uygulama

ForceWaveDefinition sekiz kayıtlı seviyeyi doğrular. ForceWaveRuntime uygunluk ve çalıştırma için aynı hedef seçimini kullanır; hedef başına tek hasar uygular ve hayatta kalanları IKnockbackReceiver üzerinden iter. ForceWaveRuntimeFactory standart edinme/seviye hattında cooldown'ı korur. Mevcut geliştirme bootstrap'ı Fireball ve Force Wave factory'lerini birlikte kaydeder.

AD_ForceWave kimliği ability.force-wave olarak kaydedildi. Kayıtlı içerik arenası/F1 kataloğunun sonuna, ability ve karma ödül havuzlarına eklendi; mevcut katalog indeksleri, başlangıç silahı ve sahne geometrisi korundu. Geçici görsel olarak mevcut plasma mermi materyaliyle haritaya ait kısa bir LineRenderer yayı kullanılır. Collider veya hasar mantığı yoktur; .25 ölçekli saniye sonunda veya kaynak ölünce temizlenir. Evolution uygulaması ve Windows build kapsam dışıdır.

## Doğrulama durumu — 2026-09-26

EditMode: 889/889 geçti (force-wave-final-EditMode.xml). Üç yeni ForceWaveArenaTests testi de geçti: gerçek hasar/itme, duvar engeli, cooldown koruyan seviyeler ve sandık uygunluğu. Tam PlayMode: 534/536 geçti (force-wave-final-PlayMode.xml). Eski common-chest testi genişleyen havuzdaki iki ability yerine bir ability bekliyordu; beklenti düzeltildi fakat yeniden çalıştırılmadı. MinigunArenaTests.HoldPreparesThenConsumesOneRoundPerShotAndRecoilAccumulates testinin bırakılmış giriş kontrolü başarısız oldu; sebebi henüz kesinleşmedi. Son tam tekrar çalıştırmasına izin verilmediğinden bütün regresyonların geçtiği sonucu henüz alınmadı. Raporlar .codex-temp/xp-attraction altındadır. Force Wave kullanıcı oynanış kabulü bekliyor.
