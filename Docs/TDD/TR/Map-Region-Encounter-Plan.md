# Harita, bölge ve karşılaşma ilerleme planı

## Durum ve kapsam

15 Eylül 2026: Kabul edilen tasarım yönü ve aşamalı geliştirme planı. Bu belge yeni sistemin uygulandığı anlamına gelmez. Mevcut çoklu arena prototipi wave tamamlanmasıyla açılan, aynı sahne içinde oyuncuyu taşıyan geçişi kullanmaya devam eder.

Bu plan, eski belgelerdeki her arenanın ayrı sahne olması ve her ilerlemenin tüm wave'leri temizlemeyi gerektirmesi varsayımlarının yerini alır. Altın, meta geliştirmeler, yeni evolution içeriği ve prosedürel üretim kapsam dışıdır.

## Kavramlar ve kabul edilen mekanikler

| Kavram | Sorumluluk |
| --- | --- |
| Harita | İlk prototipte bir Unity sahnesindeki gezilebilir alan; birden fazla bölge ve çıkış içerebilir. |
| Bölge | Haritanın ana alanı, yan odası veya mağarası; bağımsız sahne olmak zorunda değildir. |
| Karşılaşma | Bölgedeki düşman grubu veya wave dizisi; tamamlanması haritayı otomatik bitirmez. |
| Geçiş | Kaynak ve hedef arasında ilerleme isteği; uygunluk, hareket yöntemi ve görsel kapı birbirinden ayrılır. |
| Run | Haritalar boyunca oyuncunun ilerlemesini ve nihai sonucunu kapsar. |

- Oyuncu isteğe bağlı savaşları atlayıp çıkışa ilerleyebilir. Bunun bedeli kaçırılan XP ve gelişim fırsatlarıdır; denge daha sonra ölçülecek.
- Karşılaşma temizleme şartı yalnızca açıkça tanımlanmış özel geçişlerde kullanılır. Haritadaki tüm düşmanlar genel bir çıkış kilidi değildir.
- Yan alanlara dönüş ve tek yönlü bağlantılar desteklenecek. Tek yönlü geçiş otomatik checkpoint veya kayıt anlamına gelmez.
- Aynı sahnede merdiven/koridor boyunca yürümek de geçiştir; teleport zorunlu değildir. Sahne yüklemesi ayrı bir geçiş yöntemidir.
- `GateTrigger` / `PitTrigger` gibi dekor türüne bağlı sınıflar yerine genel geçiş davranışı kullanılır.
- Hazırlama ve savaşı etkinleştirme ayrılacak. Oyuncu yeni bölgeyi görmeden düşmanlar hazırlanabilecek; takılmama hedefi ölçümle doğrulanacak, yalnızca async çağrı kullanmak yeterli sayılmayacak.
- Bölge değişimi oyuncu sağlığını, ekipmanını, mermisini, XP'sini veya cooldown'larını sıfırlamaz. Sahne sınırında bu verileri taşıma mekanizması ayrıca tasarlanacak.
- Sıradan karşılaşma veya harita çıkışı run zaferi değildir. Nihai hedef ayrı bir koşuldur.
- İlk oynanabilir örnek el yapımı tek bir harita düzeni kullanacak; varyantlar zorunlu değil.

## Mevcut sistemden geçiş envanteri

| Parça | Korunacak / değiştirilecek yön |
| --- | --- |
| `WaveController`, `WaveEnemyTracker` | Yerel karşılaşmaların spawn ve bitiş takibi korunur; harita ilerlemesinin tek otoritesi olmaz. |
| `ArenaSessionController` | Wave tabanlı yaşam döngüsü karşılaşma kapsamında değerlendirilecek; otomatik harita tamamlama bağlantısı ayrılacak. İsim değişikliği henüz kararlaştırılmadı. |
| `RunSessionController` | Tüm arena oturumlarını baştan hazırlayan sıralı liste ve victory üzerinden ilerleme varsayımı gözden geçirilecek; harita çıkışı ve karşılaşma bitişi ayrılacak. |
| `ArenaTransitionTrigger` | Genel collider ve açık oyuncu referansı korunur; kaynak arena indeksi bağımlılığı yeni hedef modeline uyarlanır. |
| `ArenaTransitionController` | Uygunluk kontrolü, geçiş yöntemi ve hedef çözümleme ayrılacak; serbest çıkış için zorunlu arena victory şartı kaldırılacak. |
| `PlayerMotor.Teleport` | Gereken yerel taşımalarda kalır; yürüyüş bağlantıları bunu kullanmak zorunda değildir. |
| Development bootstrap / `Test_Waves` | Mevcut doğrulama örneği korunur; dolaşılabilir örnek sahnenin yerine geçtiği varsayılmaz. |

## Aşamalı geliştirme sırası

Her adımın mekanikleri uygulamadan önce konuşulup TDD'ye işlenir. 1 tasarım olarak, 2 yaşam döngüsü çekirdeği olarak uygulanmıştır. 3–4 için düzeltilmiş geçiş sözleşmesi ve yürüyüş/bariyer entegrasyonu, 5 için tek grup ön hazırlığı, 6 için ortak harita içi geçiş ve kaynak sahipliği vardır; doğrulama kapsamı ilgili belgelerde tutulur. 7–10 planlıdır. Tekrar giriş ve sürekli aktif karşılaşma kararları aşağıda güncellenmiştir.

1. **Tasarım ayrımı — bu belge:** GDD yönünü, terimleri, korunacak parçaları ve sırayı kaydet. Runtime değişikliği yok.
2. **Bölge ve karşılaşma yaşam döngüsü:** Hazırlama, etkinleştirme, tamamlama ve tekrar giriş kurallarını tasarla. Karşılaşma bitişinin harita bitişi olmadığını test et.
3. **Genel geçiş sözleşmesi:** Serbest / karşılaşma şartlı uygunluk, hedef, tek yön / dönüş ve yürüyüş / taşıma / sahne yükleme ayrımını kur. Yanlış oyuncu ve yinelenen istekleri test et.
4. **Geçiş bariyeri:** Görselden bağımsız açılma/kapanma sinyalleri; oyuncu tamamen geçmeden kapatmama, arada sıkışmama ve tekrar tetiklemeyi test et.
5. **Ön hazırlık — tek grup entegrasyonu uygulandı:** Ayrı hazırlama/aktivasyon hacimleri, adet ve süre bütçesi, hazır olmayan hedef için bekleyen giriş ve geçit kilidi eklendi. Ölçümler ve doğrulama Region-Preparation belgesindedir. Wave dizilerini ön hazırlama ve havuzlama henüz eklenmedi.
6. **Kaynak sürekliliği — uygulandı:** Ortak bölge konumu ve harita genelinde geçiş kilidi; oyuncu durumu yerinde korunur. Drop kaynak haritanın sahnesine aittir ve bölge/düşman temizliğinde kalır. Geri dönüş, tek yön ve savaş atlama kapsamı Map-Progression-Resources belgesindedir.
7. **Sahneler arası geçiş:** Inspector üzerinden hedef sahne ve giriş kimliği yapılandırması, run verisi aktarımı, eski sahne temizliği ve yükleme hatası davranışı. Bölge geçişinden ayrı doğrula.
8. **Run sonucu ve yeniden başlatma:** Gezinme/geçiş sırasında ölüm, nihai zafer ve temiz restart. Checkpoint ve kayıt kararı bu adıma otomatik dahil değildir.
9. **Oynanabilir örnek sahne:** Ana alan, iki geri dönülebilen yan alan, hazırlama bağlantısı, tek yönlü ikinci ana alan ve küçük ikinci sahneye çıkış. Placeholder geometri yeterli.
10. **Oynanış ve performans doğrulaması:** Kısa rota ile keşif rotasını karşılaştır; geçişte frame sıçraması, görünür spawn ve kaynak kayıplarını ölç.

İlk tasarım ve uyarlamalar mevcut `feature/multi-arena-run` üzerinde ilerler. Her madde ayrı branch veya commit gerektirmez. Yeni bağımsız aşama gerektiğinde Plastic dev / Git main tabanından açılır; merge'i kullanıcı yapar.

## Kararlar ve kalan tasarım konuları

- Karar: tekrar giriş düşmanları yeniden üretmez; yaşayanların canı ve mevcut karşılaşma durumu korunur.
- Düşmanlar bölge dışına oyuncuyu takip edebilecek mi; sahiplik ve takip sınırı nasıl ayrılacak?
- Karar: oyuncu ayrılınca aktif karşılaşma çalışmaya devam eder; askıya alma optimizasyonu ertelendi.
- Prototip kararı: hazırlanıp ziyaret edilmeyen gruplar, sahipleri açıkça kapatılana/yok edilene veya harita sahnesi kaldırılana kadar tutulur; mesafeye göre boşaltma yoktur.

Takip sınırları açık konudur. Kararlaştırılmış yaşam döngüsü ve kaynak kuralları Region-Encounter-Lifecycle ve Map-Progression-Resources belgelerinde yer alır.

## Doğrulama ve kayıt düzeni

Önce anlamlı tasarım paketi için docs check-in/commit; sonra ilgili runtime ve testler için anlamlı uygulama paketleri. Her küçük dosya değişimi ayrı kayıt gerektirmez. Bu adım yalnızca dokümandır; önceki Unity test sonuçları yeni sistemin doğrulandığı anlamına gelmez.
