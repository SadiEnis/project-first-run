# Oynanabilir harita fixture'ı — 9. aşama

## Yerleşim sözleşmesi

Prototip haritası placeholder geometri kullanan tek bir elle hazırlanmış sahne ve küçük bir hedef sahneden oluşur. Ana alanda oyuncu başlar ve iki isteğe bağlı geri dönüşlü yan yol bulunur. Ön hazırlık koridoru sonraki grubu etkinleştirme alanından önce hazırlar. İkinci ana alana yeniden kullanılabilir tek yönlü geçitle gidilir; ters bağlantı sözleşme gereği yoktur. Son harita çıkışı 7. aşamadaki sahne yükleme adaptörünü kullanarak teknik hedef sahneye gider.

Harita wave'leri temizleme koridoru değildir: oyuncu karşılaşmadan ayrılıp düşmanlar yaşarken serbest çıkışı kullanabilir. Gelecekte karşılaşma şartlı çıkışlar eklenebilir; sıradan yan yollarda kullanılmaz. Yan yola dönüş aynı karşılaşma nesnelerini ve drop'ları korur. Tek yönlü bağlantı checkpoint değildir.

## Fixture bileşimi

- Deterministik placeholder zemin/duvar/etiket ve trigger'lar tek içerik kökü altında sahnede bulunur. `PlayableMapFixtureBootstrap` yalnızca debug sunucusudur; runtime'da gameplay nesnesi üretmez.
- Tüm component referansları sahnede serialize edilmiştir: tek oyuncu, tek map session, yerel registry'ler, iki dönüşlü yan geçit, bir hazırlama/etkinleştirme çifti, tek yönlü geçit ve bir sahne çıkışı.
- Kullanılabilir referanslarda mevcut düşman, XP ve sandık servisleri kullanılır; hazırlama akışını sandık drop kurulumuna bağlamamak için yalnızca fixture'a özel, drop'suz tek wave asset'i eklenmiştir.
- Hedef sahne yalnızca metadata, kapalı içerik kökü, bilinen giriş ve registry taşır; ikinci arena değildir.
- Çıkış/trigger collider'ları oyuncu gövdesine uygun boyuttadır ve oyuncunun üzerinde spawn olmaz. Etiketler kaynak/hedef kimliğini ve dönüş/tek yön bilgisini gösterir.

Geçici görünümde gri/mavi ana zemini, camgöbeği kuzey dönüşlü yan alanı, yeşil güney dönüşlü yan alanı, sarı ise ikinci ana alanı gösterir. Gri bağlantı zeminleri görünen boşlukları yürünebilir hale getirir. Renkli alanlar kilitli duvar değil zemindir; her bağlantının ince geçit kısmı geçiş trigger'ı/bariyeridir.

## Kabul testleri

Fixture'ı Unity Editor'den açıp çalıştır: oyuncu ana alanda başlar, iki yan yolu ziyaret edip dönebilir, hazırlama/etkinleştirme rotasını görür ve tek yönlü rotayı ters geçit olmadan kullanır. Son çıkış hedef sahneyi yükler ve aynı oyuncuyu girişe yerleştirir. Kısa ve keşif rotalarını dene. İkinci oyuncu, varıştan önce görünen hedef spawn'ı veya geçişte otomatik XP/sandık ödülü olmadığını doğrula.

Fixture küçük ve deterministiktir. Nihai sanat, procedural üretim, düşman çeşitliliği, performans bütçesi veya restart UI'sını temsil etmez; bunlar sonraki polish/doğrulama işleridir.

Bu prototipte hedef sahne asset path üzerinden Editor'de yüklenebilir. Oyuncu build'i alınmadan önce iki sahne de aktif Build Profile'a eklenmelidir.
