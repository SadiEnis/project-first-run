# Boş şarjörde otomatik doldurma

## Düzeltme sözleşmesi

Content üzerinde aktif silahın şarjöründeki mermi sıfır ve yedeği pozitifse R veya ikinci ateş denemesi gerektirmeden mevcut reload başlar. Kısmen boş şarjörde elle R kullanımı korunur. Mermi kaynağı, sınırsız mermi, denge değişimi veya yeni animasyon eklenmez.

- Kontrolün açık olduğu normal güncellemelerde (kuşanma/devam sonrası dahil) boş şarjör kontrol edilir; son başarılı atışın işlemleri biter bitmez de kontrol yapılır.
- Mevcut reload doğrulaması, süresi ve aktarım mantığı kullanılır. ReloadStarted bir kez yayınlanır, Minigun hazırlığı sıfırlanır; süre tamamlandığında mevcut tamamlanma/mühimmat bildirimleri yayınlanır.
- Yedek yoksa reload veya tekrarlanan başlangıç bildirimi yoktur. Eksik yedekte yalnız eldeki mermi aktarılır. Süre dolmadan doldurma yapılmaz.
- Silah kontrolü kapalıyken veya pause sırasında reload başlamaz/ilerlemez; ölüm mevcut kontrol engelini izler. Pasif silahlar durumunu korur; arka planda reload eklenmez.
- Otomatik silahta basılı ateş reload sonrası devam eder. Minigun yeniden hazırlanır; yarı otomatik silah yeni basış ister. Silah değiştirmek kalan reload süresini korur.
- Mevcut davranışa ek olarak PlasmaRifle, Shotgun ve Minigun'da gerçek son mermi atışı, inputsuz boş şarjör, tekrarlanan R, basılı Minigun, sıfır/eksik yedek, pause/kontrol/ölüm ve silah değişimi doğrulanır.
- Commit niyeti: `fix: auto-reload empty magazines when reserve ammo is available`. Content üzerinde check-in/commit kullanıcıya aittir; bu artışta merge/PR gerekmez.

## Doğrulama ve kullanıcı gözlemi

- PlayerWeaponController kontrollü işlemeden önce ve atıştan sonra otomatik reload'u kontrol eder; elle R ile ortak StartReload kullanılır. WeaponRuntimeState mermi aktarımı ve reload süreleri değişmedi.
- Unity 6000.3.9f1, izole kopya: **824/824 EditMode ve 489/489 PlayMode geçti**. Raporlar: `.codex-temp/xp-attraction/auto-reload-EditMode.xml` ve `auto-reload-PlayMode-v2.xml`.
- Dört PlayMode senaryosu eklendi: inputsuz doldurma/eksik yedek/tekrarlanan R; PlasmaRifle/Shotgun/Minigun'da gerçek son mermi atışı; basılı Minigun'un yeniden hazırlanması ve yedeğin bitmesi; pause/kontrol/ölüm engelleri. Mevcut elle reload ve silah değişimi regresyonları da geçti. Çoklu silah testinde basışların arasına yeni input kareleri gerekti; bu test düzeltmesi için runtime kodunda ek değişiklik gerekmedi.
- Kullanıcı mermi sıfır olduğunda reload'un aktif olduğunu gözlemledi. Bu, istenen başlangıç davranışını doğrular; her uç durumun veya tüm mermi aktarım döngüsünün ayrıca elle doğrulandığı şeklinde yorumlanmaz.
- Değişen kod/testler hash ile test kopyasıyla eşleşti; `git diff --check` temiz. Mermi düşürme sistemi, build, commit/check-in veya merge yapılmadı.
