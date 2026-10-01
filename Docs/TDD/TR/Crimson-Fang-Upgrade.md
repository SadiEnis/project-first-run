# Kızıl Diş

## Durum ve kapsam

upgrades-content üzerinde mekanikler onaylandı. Uygulama öncesi doküman checkpoint'idir; runtime uygulaması veya test çalıştırıldığı iddia edilmez.

Kızıl Diş (Crimson Fang) mevcut ortak güçlendirme slotlarını ve üç seviyeyi kullanır. Her seviye önceki tam etkiyi değiştirir; önceki seviyelerin iyileştirmesi birikmez.

| Seviye | Oyuncuya yazılan düşman öldürmesi başına can |
| --- | --- |
| 1 | 2 |
| 2 | 5 |
| 3 | 7 |

Bunlar başlangıç oynanış testi değerleridir. Son seviyede on uygun öldürme en fazla 70 can yenileyebilir; kalabalık karşılaşmalar sonraki dengelemede değerlendirilir. Şimdiden onaysız şans, bekleme süresi veya rank çarpanı eklenmez.

## Öldürme ve iyileştirme sözleşmesi

- Oyuncu hayattayken oyuncunun öldürdüğü düşman için garantili iyileştirme; rastgele seçim yok.
- Öldürme son ölümcül hasarın sahibine yazılır; daha önce hasar veren herkese değil.
- Oyuncunun silah, yetenek, yanma ve kanama öldürmeleri sayılır. Bağlantı noktası seçilmeden mermi/drone/etki kaynakları dahil mevcut hasar sahipliği incelenir. Eksik veya ilgisiz sahip oyuncuya yazılmaz.
- Her düşman ölümü en fazla bir iyileştirme verir. Havuzdan yeniden doğan düşman yeni spawn olarak ele alınır; eski ölüm sayılmaz. Despawn, devre dışı bırakma, registry'den çıkarma ve sahne temizliği öldürme değildir.
- Şimdilik normal, elite ve boss aynı miktarda iyileştirir.
- Öldürme anında sahip olunan güncel güçlendirme seviyesi kullanılır. Önceden başlayan etkiler daha sonra öldürebilir; hasar anlık görüntüsünü korumak iyileştirme seviyesini fırlatma anına sabitlemez.
- İyileştirme maksimum canı aşmaz; fazlalık atılır. Tam candaki öldürmeler iyileştirme biriktirmez.
- Oyuncu ölünce kalan etkiler iyileştiremez/diriltemez. Öldürme işlendiği anda oyuncunun zaten ölü olduğu karşılıklı öldürme durumu oyuncuyu diriltmez.
- Mevcut iyileştirme API'si kullanılır; reset/tam doldurma kullanılmaz. Alınan hasar çarpanı ve zırh iyileştirmeyi değiştirmez.
- Can bildirimleri Son Direniş'i yeniden değerlendirir: iyileşmeyle %30'un üstüne çıkınca koşullu hasar bonusu kalkar.

## Entegrasyon planı

Uygulamadan önce EnemyController ölüm sırası, EnemyRegistry'den çıkarma ve DamageInfo.Source incelenir. Ölüm callback'i işlenmeden unregister yüzünden aboneliğin kaldırılması engellenir. Global düşman sayısındaki azalmadan ödül vermek yerine sahiplik açık tutulur.

Mevcut ve yeni doğan düşmanlara tekrarsız bağlanılır; harita değişince eski bağlantılar kaldırılır ve oyuncunun güçlendirme sahipliği korunur. Yeni run önceki run'ın sahipliğini veya tekrar-engelleme durumunu taşımaz. Öldürme sahipliği desteği dar kapsamlı kalır; altın veya başka öldürme ödülleri eklenmez.

Üç seviyeli tek gerçek asset kayıtlı içerik arenası, sahne oluşturucu, F1 kataloğu ve upgrade/karışık havuzlara eklenir. Slot sınırları ve maksimum-seviye dışlaması korunur. Özellik kartı ışıltısı veya diğer bekleyen güçlendirmeler bu adımda yapılmaz.

## Doğrulama planı

Üç seviye değerleri/değiştirme, tekrar edinme ve maksimum seviye, doğrudan silah/yetenek ve zamanlı etki sahipliği, ilgisiz/eksik kaynak reddi, ölüm başına tek iyileştirme, havuzdan yeniden kullanım, despawn/temizlik dışlaması, tam can sınırı, oyuncu ölümü, öldürme anındaki seviye, Son Direniş etkileşimi ve harita bağlantısı/temizliği için testler yazılır.

Unity derlemesi, EditMode/PlayMode ve manuel kabul kullanıcıya aittir. Asistan testleri yazar ve yalnızca açıkça istenen eksik/başarısız testleri çalıştırır. Uygulama sonrası oynanış kabulü için durulur; nihai denge ertelenir.
