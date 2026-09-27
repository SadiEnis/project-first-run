# Sniper Bomb — içerik tasarımı

## Durum

abilities-content üzerindeki sekizinci ve son planlı temel yetenek; GDD 12.8. Kullanıcı aşağıdaki hedefleme, güdüm ve yeniden seçim kurallarını onayladı; oynanış testinden sonra revize edilebilir. Bu checkpoint yalnızca dokümantasyondur. Henüz uygulama veya test çalıştırma yoktur; evolution kapsam dışıdır.

## Kararlaştırılan davranış

- Oyuncunun 20 m yakınındaki görülebilen canlı mevcut-harita düşmanı seçilir. Öncelik Boss, ardından Elite, ardından Normal'dir; mevcut en yüksek öncelikli grup içinden en yakın seçilir. Uygun hedef yoksa cooldown tüketilmez.
- 10 m/s hızla kilitli hedefe sürekli yönelir. Engellerin etrafından navigasyon/yol bulma yapmaz. Hareket, hızlı geçişlerde hedef/duvar atlamamak için süpürülür.
- Düşman veya katı dünya engeliyle temas, o temas noktasında patlatır. Düşmana temas ayrıca doğrudan isabet hasarı eklemez; uygun düşman başına bir kez patlama hasarı uygulanır.
- Patlama yarıçapı gerçek patlama konumundan ölçülür; dünya engellerine karşı görüş kontrolü yapılır. Birden fazla collider hasarı çoğaltmaz; duvar arkasındaki düşmana hasar geçmez.
- L5 aynı aktivasyon/cooldown içinde iki bomba çıkarır; başlangıçta ikisi de aynı öncelikli hedefe kilitlenir. Her bombanın uçuşu, patlaması ve hedef yaşam takibi ayrıdır; ikisi de aynı düşmana hasar verebilir.
- L7 öncesinde hedef ölür veya geçersiz olursa son bilinen konumuna devam edip orada patlar.
- L7'den itibaren hedef geçersizleştiğinde bombanın güncel konumunun 20 m yakınında aynı rank önceliği ve en yakın görülebilen hedef kuralıyla yeni seçim denenir. Daha yüksek rank ortaya çıktı diye yaşayan hedef bırakılmaz. Aday yoksa son bilinen konuma gitmeye karar verir; sonsuz tarama yapmaz. Yeni hedef de ölürse daha sonra yeniden seçim yapılabilir.
- İlk seçim oyuncu merkezli, yeniden seçim bomba merkezlidir. Farklı registry/haritalara hedef geçişi yapılmaz; yeniden kullanılan düşman örneği eski hedef sanılmaz.
- Geçici uçuş ömrü atıştan itibaren 5 s'dir; hedef değişiminde sıfırlanmaz. Süre dolunca bulunduğu konumda patlar; hedefe ışınlanmaz.
- Seviye ayarları ve AbilityDamage ile ölçeklenen hasar atışta alınır. Sonraki seviye artışı uçan bombaları değiştirmez. Mevcut cooldown ilerlemesi korunur.
- Pause uçuşu, yaşam süresini, cooldown ve patlama görselini dondurur. Oyuncu ölümü/harita değişimi sahip olunan bombaları patlatmadan temizler. Kaynak oyuncu, pickup, yalnızca trigger ve sunum collider'ları dışlanır.
- Katı engel içinde doğan bomba duvardan kaçamaz. Geçersiz atış, engel arkasına hasar üretmek yerine güvenle iptal edilir.
- Okunur geçici bomba ve genişleyen patlama görselleri; nihai görsel/ses, evolution ve build kapsam dışıdır.

## Geçici denge

Konuşulan 20 m hedef menzili, 10 m/s hız, 6/4 s cooldown ve 2/3/4 m patlama yarıçapı başlangıç değerleridir. 50/70/100 hasar, 0,2 m bomba yarıçapı ve 5 s uçuş ömrü ek uygulama varsayımlarıdır; nihai denge değildir.

| Seviye | Patlama hasarı | Yarıçap m | Cooldown s | Bomba | Yeniden hedefleme |
| --- | --- | --- | --- | --- | --- |
| 1 | 50 | 2 | 6 | 1 | Yok |
| 2 | 70 | 2 | 6 | 1 | Yok |
| 3 | 70 | 3 | 6 | 1 | Yok |
| 4 | 70 | 3 | 4 | 1 | Yok |
| 5 | 70 | 3 | 4 | 2 | Yok |
| 6 | 100 | 3 | 4 | 2 | Yok |
| 7 | 100 | 3 | 4 | 2 | Var |
| 8 | 100 | 4 | 4 | 2 | Var |

## Uygulama ve doğrulama planı

1. Doğrulanan seviye verileri, öncelikli hedef seçici ve runtime factory.
2. Engelli süpürmeli güdümlü uçuş, son bilinen konuma devam, hedef yeniden kullanım kontrolü ve L7 yeniden seçim.
3. Düşman başına bir kez engel kontrollü patlama, bağımsız çift bomba, geçici görseller ve yaşam döngüsü temizliği.
4. Asset, kayıtlı içerik arenası/F1 ve ability/karma ödül havuzları; slot kapasitesi ve başlangıç ekipmanı korunur.
5. Rank/mesafe/görüş, hedefsiz atış, takip, hedef ölümü/yeniden kullanım, son konum/yeniden seçim, duvar sıralaması, patlama tekilleştirme/engel, süre bitişi, çift bomba, seviye anlık görüntüsü ve pause/ölüm/harita temizleme testlerini yazma.

EditMode/PlayMode ve oynanış kabulünü kullanıcı yapar; asistan yalnızca özellikle istenen eksik/başarısız testleri çalıştırır. Otomatik commit/merge yoktur. Docs ve uygulama ayrı check-in noktalarıdır. abilities-content aşamasının tamamlanmasını değerlendirmeden önce kullanıcı kabulü için durulur.
