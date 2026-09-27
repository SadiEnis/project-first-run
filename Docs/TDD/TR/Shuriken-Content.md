# Shuriken — içerik tasarımı

## Durum ve kapsam

abilities-content üzerindeki altıncı yetenek; GDD 12.5 temel alınır. Kullanıcı dönüş/temas/kanama mekaniğini onayladı. Runtime, seviye asset'i, kayıtlı arena/F1 ve ödül entegrasyonu uygulandı. Unity derlemesi, test çalıştırma ve oynanış kabulü henüz doğrulanmadı. Evolution oynanışı ve nihai görseller kapsam dışıdır.

## Kararlaştırılan davranış

- Oyuncu canlı ve yetenek kontrolü açıkken otomatik bir dönüş grubu başlar. Hedef gerekmez; hedef seçen saldırı değil, yakın çevre korumasıdır.
- Her aktivasyon oyuncunun hareket eden konumu çevresinde tam iki tur tamamlar, sonra shuriken'ler kaldırılır. Oyuncunun nişan yönünü değiştirmesi dönüş açısını sıfırlamaz.
- Her shuriken aynı düşmana tur başına bir kez vurabilir (aktivasyonda en fazla iki kez). İkinci turda kendi isabet kaydı sıfırlanır. Birden fazla collider tek isabeti çoğaltmaz.
- L6 karşılıklı açılarda ikinci shuriken ekler; her birinin isabet kaydı bağımsızdır.
- Temas kontrolü dönüş yolunu süpürür; açısal adımlar ve tur sınırları gerektiğinde bölünür. Hızlı dönüş, oyuncu hareketi ve uzun karelerde hedef atlanmamalıdır. Işınlanma/harita geçişi dönüşü iptal eder; ışınlanma yolu boyunca hasar taranmaz.
- Dünya geometrisi hasarı engeller: oyuncudan temas noktasına ve yerel taramadan hedefe engelsiz yol gerekir. Duvar arkasına hasar verilmez. Geçici dönüş görseli geometriye girebilir; oyuncuyu iten veya dönüşü durduran katı bir fiziksel mermi değildir.
- Yeni aktivasyon iki tur bitip dönüş sonrası cooldown dolunca başlar. Cooldown atış başlangıcından ölçülmez; aynı oyuncu için eşzamanlı dönüş grupları birikmez.
- Seviye ayarları ve AbilityDamage ölçeklemesi grup başlarken alınır; dönüş sırasında seviye artışı sonraki grubu etkiler. Harita değişimi/kontrol kaybı görselleri ve temas kayıtlarını kaldırır; anlık yeniden atış açığı oluşmaması için normal dönüş sonrası cooldown korunur. Oyuncu ölümünde dönüş kalmaz.
- Pause açı, dönüş süresi, cooldown, görseller ve kanama sayaçlarını dondurur. Harita bağlama yalnızca güncel registry'yi kullanır. Düşman ölümü/yeniden kullanımında isabet/durum kayıtları uygun şekilde temizlenir.

## L8 kanama

Başarılı temas, düşman dönüş alanından çıktıktan sonra da süren zamanlı hasar uygular. İlk tick 0,5 s sonra; 2 s boyunca her 0,5 s'de bir devam eder. Yeniden isabet süreyi yeniler; hasarı katlamaz veya sıradaki tick'i ertelemez. Fire/plasma yanmasından ayrı, kaynak oyuncuya ait kanama kimliği kullanılır; iki durum birlikte çalışabilir. Kanama hasarı aktivasyonda alınır. Hedef ölümü/devre dışı bırakma/yeniden kullanım veya harita boşaltmada durur; önceden uygulanmış kanama kaynak öldükten sonra süresini tamamlayabilir.

## Geçici denge

Önceden konuşulmayan hasarlar, L7 yarıçapı, kanama miktarı ve temas yarıçapı başlangıç uygulama varsayımlarıdır; nihai denge değildir. Temas yarıçapı 0,3 m; dönüş gövde yüksekliğine yakın yatay düzlemdedir. Her seviyede iki tur vardır.

| Seviye | Temas hasarı | Tur başına saniye | Dönüş sonrası cooldown s | Dönüş yarıçapı m | Shuriken | Kanama |
| --- | --- | --- | --- | --- | --- | --- |
| 1 | 15 | 1.5 | 3 | 2.5 | 1 | Yok |
| 2 | 25 | 1.5 | 3 | 2.5 | 1 | Yok |
| 3 | 25 | 1 | 3 | 2.5 | 1 | Yok |
| 4 | 25 | 1 | 2 | 2.5 | 1 | Yok |
| 5 | 35 | 1 | 2 | 2.5 | 1 | Yok |
| 6 | 35 | 1 | 2 | 2.5 | 2 | Yok |
| 7 | 35 | 1 | 2 | 3.5 | 2 | Yok |
| 8 | 35 | 1 | 2 | 3.5 | 2 | 2 s boyunca 0,5 s başına 5 hasar |

## Uygulama ve doğrulama planı

1. Doğrulanan sekiz seviyeli definition, runtime/factory ve açık dönüş/dönüş sonrası cooldown yaşam döngüsü. Diğer yeteneklerin cooldown anlamını değiştirmeden mevcut yetenek bağlantılarını uyarlama.
2. Hareketli merkezli temas taraması, shuriken/tur başına isabet kayıtları, engel kontrolü ve okunur geçici görseller.
3. Ayrı kanama kimliği; süre yenileme, pause ve temizleme kuralları uyduğu ölçüde mevcut zamanlı hasar altyapısını kullanma.
4. Asset, kayıtlı içerik arenası/F1, ability/karma havuzları ve katalog beklentileri. Başlangıç ekipmanı ve yetenek slot sınırı korunur.
5. Seviye, tam iki tur, bağımsız isabet sınırları, uzun kare/tur sınırları, hareket/duvar, dönüş sonrası cooldown, seviye anlık görüntüsü, kanama yenileme/birlikte çalışma ve pause/ölüm/yeniden kullanım/harita temizleme EditMode/PlayMode testlerini yazma.

Testleri ve oynanış kabulünü kullanıcı yapar. Asistan yalnızca özellikle istenen eksik/başarısız testleri çalıştırır. Docs ve uygulama ayrı check-in noktalarıdır; Enchanted Staff öncesi kabul için durulur.

## Uygulama notları

- Mevcut sürekli yetenek bağlantısı kullanılır; dönüş/dönüş sonrası cooldown kendi içinde yönetilir. Diğer yeteneklerin cooldown davranışı değişmez. Doğru cooldown kaynağı genel entry sayacı değil, ShurikenRuntime.CooldownRemaining değeridir.
- Tarama her turu en az 72 parçaya, oyuncu hareketini en fazla 0,1 m adımlara böler. Tek tick'te merkez 6 m'den fazla yer değiştirirse ışınlanma sayılıp iptal edilir; registry yeniden bağlama da iptal eder. Çok büyük normal hareketler bu nedenle tarama yerine iptal oluşturabilir. Uzun takılmada dahi tick başına en fazla bir yeni grup başlar.
- Enemy SpawnVersion, yeniden başlatılan düşmanları isabet kaydında ve kanama temizliğinde ayırt eder. Kanama TimedBurnState sayacını kullanır; kaynak ölümünden sonra sürdüğü için ayrı component/yaşam döngüsüne sahiptir.
- Yazılan kontroller seviye/geçersiz veri, iki tur isabet sınırı, çift shuriken, dönüş sırasında seviye artışı, duvar, pause/kontrol iptali, ışınlanma/registry, kanama yenileme/yanmayla birliktelik/kaynak ölümü, hedef yeniden kullanımı ve L8 uygulamasını kapsar. Testler çalıştırılmadı. Küçük adım ile uzun kare eşdeğerliği, hareketli merkez teması ve tam sahne boşaltma ek doğrulama hedefleridir.
