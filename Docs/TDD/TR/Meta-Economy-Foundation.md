# Meta ekonomi temeli

Durum: 9 Kasım 2026. Kalıcı cüzdanın ilk kod adımı hazır; Unity test kabulü ve oyun akışı bağlantısı bekleniyor.

## Amaç ve önkoşul

Run kazancını kalıcı cüzdandan ayırmak; ölüm/zafer sonucunu hesaplamak ve aynı run için yalnızca bir kez güvenli aktarım yapmak. Tasarım kuralları GDD 18.1'de bulunur.

Branch incelemesinde `feature/meta-economy`, `02a1383` içerik commit'inden başlamaktadır. Son başlangıç ve ortak akış çalışmaları `feature/runtime-foundation` üzerindedir (`979a8b9`). Kodlamadan önce bu temel entegre edilmelidir. Mevcut doküman değişiklikleri bu entegrasyonu gerçekleştirmez.

## Sorumluluklar

- Run kazancı: yalnızca mevcut run'a ait kazanımları tutar; yeni run'da sıfırlanır.
- Sonuç hesabı: kaynak, sonuç, koruma oranı ve boss ödülünden aktarılacak miktarı üretir. UI veya dosya erişimi yapmaz.
- Kalıcı kayıt: cüzdan bakiyesi ve ödeme tekrarını önleyecek run kimliğini tutar. Bakiye ve sonuç kimliği birlikte kaydedilir.
- Akış bağlantısı: mevcut ölüm/final akışından sonucu alır; başarılı kayıt öncesinde run'ı temizleyerek sonucu kaybetmez.
- Sunum: hesaplanan sonuç verisini gösterir; UI'yı açmak veya yeniden çizmek ödeme yapmaz.

Bunlar sorumluluk sınırlarıdır, her satır ayrı sınıf veya interface zorunluluğu değildir. Gerekli ayrımlar mevcut kod incelenerek yapılacaktır. Genel servis bulucu veya geniş bir ekonomi framework'ü kurulmayacaktır.

## Kayıt ve aktarım sözleşmesi

- Kayıt kullanıcı verisi konumunda tutulur; SO asset'i oyuncu kaydı olarak değiştirilmez.
- İlk açılış sıfır bakiyeyle başlar. Kayıt sürümü açıkça tutulur.
- Bozuk/okunamayan veya desteklenmeyen sürümlü kayıt sessizce sıfırlanıp üzerine yazılmaz; hata ve kurtarma yolu sunulur.
- Yazma yarıda kesildiğinde son geçerli kaydı koruyan geçici dosya/değiştirme ve yedek yaklaşımı uygulanıp doğrulanır.
- Kaynak tam sayı olarak tutulur; negatif miktar, geçersiz oran ve sayı taşması reddedilir. Yuvarlama davranışı onaylandıktan sonra sabitlenir.
- Aynı run kimliğiyle yinelenen sonuç çağrısı ikinci ödeme üretmez. Kimlik ve bakiye aynı kayıt işleminde korunur.
- Kayıt hatası başarı olarak gösterilmez. Yeniden deneme aynı sonuç kimliğiyle yapılır; yeniden hesaplayıp ikinci ödül yaratmaz.
- Sonuç kaydı sırasında kapanma ile henüz tamamlanmamış run'ın kapanması ayrı durumlardır. Tamamlanmamış run kazancı için GDD'deki açık karar çözülmelidir.

## Uygulama sırası ve kabul

| İş | Kabul ölçütü |
| --- | --- |
| Kurallar | Ölüm/zafer kabulü ve bırakma/kapanma/yuvarlama kararları yazılı |
| Kalıcı cüzdan | İlk açılış, tekrar açılış, bozuk kayıt ve yazma hatası ele alınmış |
| Run kazancı | Kazanç birikir; yeni run'da sıfırlanır; eski cüzdan etkilenmez |
| Sonuç aktarımı | Ölümde kesinti, zaferde tam aktarım + boss ödülü; yinelenen çağrı ödeme yapmaz |
| Sonuç ekranı | Toplanan, kayıp, boss ödülü, aktarım ve toplam; klavye/gamepad ile erişim |
| Oynanış kaynağı | Kazanım miktarları veri üzerinden ayarlanır; aynı düşmanın ölümü iki kez ödül vermez |

Her anlamlı adımda kullanıcı oynanış kabulü ve check-in yapılır. Unity EditMode/PlayMode çalıştırmalarını kullanıcı yürütür; gerektiğinde yalnızca bildirilen başarısız testler ele alınır. Kritik hesap/kayıt davranışları için odaklı testler yazılır. Windows build kontrolü kayıt sistemi bağlandıktan sonra ayrıca planlanır.

## Araştırma ve sonraki kapsam

Meta öğeleri henüz seçilmedi. Oyun araştırması kaynak kazanımı, ölüm/zafer farkı, satın alma temposu, seviye sınırları ve run içi güçlendirmelerle ilişkiyi karşılaştıracaktır. Ardından tek bir gelişim için satın alma → kayıt → yeni run'da uygulama tamamlanacaktır.

İlk kapsam dışında: gelişim ağacı, ayrıntılı üs, zorluk/hikâye ilerlemesi, anı kayıtları, run ortasında devam kaydı ve bulut kayıt.

## İlk uygulama adımı — kalıcı cüzdan

Runtime temeli feature/meta-economy branch'ine birleştirildi. PersistentMetaWallet, Progression altında tek sınıf olarak eklendi. OpenDefault kullanıcı veri dizinini; Open(directory) ise izole test dizinini kullanır. Sürüm 1 kaydı bakiye tutar. Run kimliği ve tek seferlik sonuç aktarımı sonraki adımda aynı kayda eklenecektir; Credit doğrudan bir run ödülü API'si değildir.

İlk açılış sıfır bakiye dosyasını oluşturur. Artış önce geçici dosyaya yazılıp diske aktarılır; mevcut dosya yedeklenerek değiştirilir. Bellekteki bakiye yalnızca başarılı yazmadan sonra değişir. Ayrı örnekler eski dosya içeriğini ezemez; dosya kilidi eşzamanlı yazmayı engeller.

Bozuk veya bilinmeyen sürümlü kayıt korunarak hata bildirilir. Ana dosya yokken kurtarma dosyası varsa sıfırdan kayıt açılmaz. Otomatik yedeğe dönüş yapılmaz; kurtarma UI'sı sonraki sahne bağlantısı işidir. Sahnelere henüz bağlanmadığı için bu adım oynanışta görünür değişiklik üretmez.

Odaklı EditMode testleri: ilk açılış/yeniden açılış, önceki kayıt yedeği, bozuk/eksik/yeni sürümlü kayıt, kayıp ana dosya, başarısız yazma ve yeniden deneme, eski örnekten yazma, negatif tutar ve taşma. Test yürütme kullanıcıdadır; derleme kontrolü testlerin geçtiği anlamına gelmez.
