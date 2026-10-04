# Enchanted Staff — içerik tasarımı

## Durum ve kapsam

abilities-content üzerindeki yedinci yetenek; GDD 12.7. Kullanıcı uygulanan Runetracer referanslı davranışı onayladı: rastgele atış yönü, düşman delme ve gerçek dünya engellerinden yansıma. Uçuş sırasında rastgele dönüş, ekran kenarından sekme ve güdüm yoktur. Runtime, sekiz seviyeli asset, arena/F1 ve ödül entegrasyonu uygulandı. Kullanıcı oynanış davranışı kabulü tamamlandı; asistan Unity derlemesi/testleri çalıştırmadı, otomatik test sonucu bildirilmedi.

Denge takibi: kullanıcı 3–4 düşmana karşı etkinliği düşük buldu ve mekaniği değiştirmek yerine değerlendirmeyi erteledi. Kalabalık ve kapalı alanlarda daha fazla isabet fırsatı beklenebilir, ancak bu beklenti doğrulanmadı. Rastgele başlangıç yönü korunur; düşük düşman yoğunluğunda isabet ve kalabalık performansı dengelemede yeniden ele alınır.

## Kararlaştırılan mekanik

- Rastgele yatay yönlere otomatik olarak kısa, hareketli enerji ışınları çıkar; hedef gerekmez. Oyuncuya bağlı kesintisiz lazer değildir.
- Düşmanlardan yön değiştirmeden ve delme hakkı tüketmeden geçer. Her ışının düşman başına ayrı isabet süresi vardır: aynı düşmana iki hasar arasında en az 0,5 s gerekir. Sekmek veya başka düşmana vurmak bu korumayı sıfırlamaz. Birden fazla collider tek hedef sayılır.
- Katı dünya yüzeyinden çarpışma normaline göre yansır. Sekmeyi kamera/ekran sınırı değil gerçek harita geometrisi belirler. Çarpışmalar arasında rastgele yön değiştirmez.
- Tekrar tekrar sekse de yaşam süresi dolunca kaybolur. Yeni atış eski ışınların bitmesini beklemez; her ışın kendi süresini ve isabet kaydını tutar.
- L5 bağımsız yönlü iki ışın çıkarır. Hasar, süre ve diğer ayarlar atışta alınır; seviye değişikliği sonraki atışları etkiler.
- Hasar silah statlarından değil AbilityDamage üzerinden ölçeklenir. Pause hareketi, yaşam süresini, cooldown ve isabet sayaçlarını dondurur. Oyuncu ölümü sahip olduğu ışınları temizler; harita bağlama eski ışınları kaldırıp yeni registry'yi kullanır.
- Bir parçada ilk engelin arkasındaki düşmana hasar verilmemesi için çarpışmalar sıralanır. Hızlı hareketi süpürerek kontrol et; sekmeden sonra karenin kalan yolunu işle.
- Temas sonrası yüzeyden küçük bir ayrılma ve köşeler için sınırlı çarpışma işleme bütçesi kullanılır. Bütçe içinde güvenli yol çözülemezse o karenin hareketi durur; yaşam süresi ilerler. Kalan yolu tüketmek için engeller atlanmaz.
- Katı geometri içinde doğan ışın duvardan kaçamaz; güvenli biçimde iptal edilir. Kaynak oyuncu, pickup, trigger ve sunum nesneleri dünya çarpışmalarından dışlanır.
- İlk prototip sabit atış düzleminde yatay ilerler. Duvar normalinden yatay yansıma türetilir; geçerli yatay yansıma vermeyen zemin/tavan temasında sonlandırılır. Zemini takip etmez; merdiven/eğimlerde elle değerlendirme gerekir.
- Kısa parlak gövde/iz ve sekme geri bildirimi geçici sunumdur. Evolution, patlama, durum etkisi, nihai görsel veya build işi yoktur.

## Geçici başlangıç değerleri

Önceden önerilen 8 m/s hız, 2/3/4 s yaşam süresi ve 5/4/3 s cooldown başlangıç denge değerleri olarak korunur. 0,15 m temas yarıçapı ve 20/30/40 hasar geçici uygulama varsayımlarıdır. Cooldown Shuriken'in dönüş sonrası beklemesinden farklı olarak atıştan itibaren ölçülür.

| Seviye | Hasar | Yaşam süresi s | Cooldown s | Işın |
| --- | --- | --- | --- | --- |
| 1 | 20 | 2 | 5 | 1 |
| 2 | 30 | 2 | 5 | 1 |
| 3 | 30 | 3 | 5 | 1 |
| 4 | 30 | 3 | 4 | 1 |
| 5 | 30 | 3 | 4 | 2 |
| 6 | 40 | 3 | 4 | 2 |
| 7 | 40 | 4 | 4 | 2 |
| 8 | 40 | 4 | 3 | 2 |

## Uygulama ve doğrulama planı

1. Doğrulanan seviye definition ve runtime factory; seviye artışında mevcut cooldown ilerlemesini koruma.
2. Süpürmeli ışın hareketi, sıralı duvar/düşman temasları, yansıma ve ışın/düşman başına isabet koruması. Yeniden kullanılan hedefi eski yaşamından ayırma.
3. Geçici görseller; asset ve kayıtlı içerik arenası/F1 ile ability/karma ödül havuzları. Slot sınırları ve başlangıç ekipmanı korunur.
4. Seviye, yansıma, delme, collider tekilleştirme, bağımsız ışınlar, sekmeler arası tekrar isabet süresi, süre bitişi, köşeler/başlangıç örtüşmesi, uzun kare, pause/ölüm/harita temizleme ve seviye testlerini yazma.
5. EditMode/PlayMode ve oynanış kabulünü kullanıcı yapar. Asistan yalnızca özellikle istenen eksik/başarısız testleri çalıştırır. Otomatik VCS işlemi yoktur.

Sniper Bomb öncesinde kullanıcı kabulü için durulur. Docs ve uygulama ayrı check-in noktalarıdır.

## Uygulama kaydı

Görsel güncelleme: mor ışın, ömrü boyunca geçtiği bütün yolu daha ince bir world-space çizgi olarak bırakır. Tarama parçalarının uçları kare içindeki sekme noktalarını da içerir; iz köşeyi kesmek yerine sekmeyi takip eder. İz ışının alt nesnesidir; süre bitişi, ölüm veya harita temizliğinde ışınla birlikte anında kaybolur. Yalnızca görseldir, hasar vermez. Yol/sekme/pause/temizleme testi eklendi ancak çalıştırılmadı.

Işınlar 0,025 s alt adımlar ve alt adım başına en fazla sekiz çarpışma çözümü kullanır. Her ışın düşman ve SpawnVersion anahtarıyla ayrı isabet zamanları tutar. Kaynak gövdesinden başlatma, öne taşmış namludan duvarın ötesine atış yapılmasını önler. Kısa mor çizgi her sekmede yön değiştirir; ayrı sekme parçacık efekti henüz yoktur.

Yazılan testler seviye/geçersiz ayar, çoklu hedef delme, collider tekilleştirme, duvar sırası/yansıma, sekmede tekrar isabet koruması, gecikmiş tekrar isabet, bağımsız ışınlar, başlangıç katı örtüşmesi, pause/süre bitişi, kaynak ölümü ve registry/cooldown korunmasını kapsar. Testler çalıştırılmadı. Köşe stresi, eğimler ve tam sahne boşaltma elle/ek doğrulama bekler.
