# Silah seviyesi etkileri

## Doğrulama — 2026-09-14

İzole Unity 6000.3.9f1 projesinde 666 EditMode ve 340 PlayMode testi geçti; bu adım 11 yeni EditMode ve sekiz yeni PlayMode testi içerir. Aktif/pasif silahlar, hasar geliştirmeleri, edinme anındaki kopyalar, hatalı ilerleme, maksimum seviye, tutarsız sahiplik ve mermi/süre korunması kapsanır. Bu otomatik runtime doğrulamasıdır; henüz uygulanmayan sandıktan seviye ödülleri veya UI için manuel kabul değildir.

Veri temeli cs162 / fd6fd4b sonrasında `/main/dev/item-level-foundation` / `codex/item-level-foundation` üzerinde devam eder. Bu adım önce silah etkilerini uygular; yetenek/geliştirme etkileri ve sandıktan seviye artırma seçenekleri aynı branch'te sonraki işlerdir.

## Sözleşme

- Seviye 1 mevcut silah alanlarını kullanır. Sonraki her seviye mutlak temel hasar, şarjör kapasitesi, saniyedeki atış ve doldurma süresi içerir. Liste tam MaximumLevel eksi bir giriş içermeli; tüm girişler geçerli olmalı ve kapasite azalmamalıdır. Edinmeden önce bütün ilerleme doğrulanır.
- Runtime kaydı edinmede bütün seviyelerin kopyasını alır. Ortak asset'in sonradan değiştirilmesi edinilmiş ilerlemeyi değiştirmez. Oyuncu statları mevcut seviyenin temel hasarı üzerine uygulanır.
- Canlı silahlarda yetkili işlem `PlayerWeaponAcquisitionController.TryLevelUp`tır: build/loadout sahipliği, tam tanım referansı, mevcut/maksimum seviye eşleşmelidir. İki veri de değiştirilmeden doğrulama yapılır. Sahiplik yoksa NotOwned, maksimumdaysa MaximumLevelReached döner; tutarsızlıkta değişiklik yapmadan hata verir.
- Aktif/pasif silahlarda aynı runtime ve mermi durumu korunur. Şarjör ve yedek mermi sayısı değişmez. Kapasite artışı mermi değil boş alan ekler. Devam eden atış/doldurma sürelerinin kalan saniyeleri korunur; yeni hız/süre sonraki işlemde kullanılır. Devam eden doldurma, özgün bitiş zamanında mevcut yedek mermiden yeni kapasiteye doğru doldurur. Reset ayrı ve açık işlemdir.
- Seviye artırma silahı otomatik kuşandırmaz, tekrar edinmez veya edinme olayı yayınlamaz. Silah değiştirince geliştirilmiş aynı kayıt korunur. Canlı silahlarda doğrudan saf build seviye metodu çağrılmaz.

## Örnek içerik ve sınırlar

Tarihsel checkpoint: aşağıdaki geçici Plasma Rifle tablosunun yerini GDD'deki fiziksel mermi/delme/yanma sırasını uygulayan [Plasma Rifle İçeriği](Plasma-Rifle-Content.md) aldı. Yukarıdaki genel mühimmat/süre koruma sözleşmesi geçerliliğini korur.

Plasma Rifle ve Development Secondary için sekiz geçici birikimli test seviyesi: 2'de hasar, 3'te kapasite, 4'te atış hızı, 5'te hasar, 6'da doldurma süresi, 7'de kapasite, 8'de hasar. Değerler geliştirme dengesidir; GDD silah tasarımlarının veya evrim uygunluğunun tamamlandığı anlamına gelmez. Tetik türü, menzil ve hasar maskesi değişmez. Fireball ve geliştirmeler kendi etkileri eklenene kadar tek seviyede kalır.

Bu adım yeni tuş, seviye ödülü veya UI eklemez. Otomatik testler runtime controller'ını çağırır; sandıkların mevcut edinme davranışı korunur. Tam test paketlerine ek olarak aktif/pasif silah geliştirme, stat etkileri, mermi/süre korunması, hatalı içerik, maksimum sınırı ve sahiplik tutarsızlığı doğrulanır. Sonra yetenek/geliştirme etkileri ve açık ödül seçenekleriyle devam edilir; eşya seviyesi aşaması henüz merge edilmez.
