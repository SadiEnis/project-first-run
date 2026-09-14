# Menzilli düşman temeli

## Durum ve kapsam

Kullanıcının onayladığı Ranger uygulama sözleşmesidir. Charger commit'i `6f087ae` sonrasında `feature/enemy-variety-foundation` üzerinde düşman çeşitliliği aşamasını sürdürür. Bu branch başlangıçta birleştirilmiş main `b8aa45f` / Plastic dev `cs:173` üzerinden açılmıştır; bu adım ayrı bir geliştirme aşaması başlatmaz.

Kullanıcı Ranger'a geçmeden önce gelişmiş sandıkları, Chaser temas hasarını ve Charger atılmasını oyunda doğruladı. Ranger bu düşmanların yanında yana hareket etmeyi ve hedef önceliklendirmeyi gerektirir.

## Kabul edilen davranış

- Tercih edilen mesafe bandının dışındayken yaklaşır, bandın içinde durur. Oyuncu fazla yaklaşınca navigasyona uygun hedeflere geri çekilir. Sürekli ileri-geri kararsızlık oluşmaması için giriş/çıkış eşikleri arasında pay bulunur.
- Yalnızca görüş hattı açıksa ve atış menzilindeyse saldırır. Görünür bir hazırlıktan sonra tek, görülebilir mermi atar ve bekleme süresine girer.
- Merminin yönü atış anında sabitlenir. Düz ilerler, hedefi takip etmez ve kaçılabilir. Hasar yalnızca çarpışmada, mermi başına bir kez uygulanır; duvarlar mermiyi durdurur ve ömrü sınırlıdır.
- Geri çekilme yolu garanti değildir. Uygun navigasyon konumu bulunamazsa yürünebilir alanda kalır ve daha sonra yeniden dener; engelden geçmez veya arena dışına ışınlanmaz.
- Rank davranıştan bağımsız kalır. İlk örnek Normal Ranged olur; statları, saldırı zamanlaması, mermi hızı ve tercih edilen mesafe bandı açık yapılandırmadır. Sayısal denge oynanış değerlendirmesine kadar geçicidir.

## Yaşam döngüsü ve bağlantılar

Hedef/hasar alıcı açıkça aktarılır; mevcut registry, can, XP ve drop yaşam döngüsü kullanılır. Pause hareketi, hazırlığı, beklemeyi ve mermi ömrünü dondurur. Düşmanın ölümü veya disable olması henüz yapılmamış atışı iptal eder. Önceden atılmış mermi çarpışana veya ömrü bitene kadar bağımsız yaşar; atıcısının hâlâ var olmasına bağlı olmamalıdır. Oyuncunun ölümü yeni hasarı engeller; mermiler arenaya aittir ve sahne kapanırken temizlenir.

İlk dalga ve toplam gösterim büyüklüğü kontrollü tutulur. İkinci dalgaya mevcut Chaser/Charger/Elite Charger grubuna bir Normal Ranger eklenmiştir (toplam dört düşman). Elite gelişmiş sandık ağırlıkları ve boss karşılaşmaları, genel düşman aşamasında ayrıca görüşülecek mekaniklerdir.

## Planlanan doğrulama

EditMode: mesafe bandı eşikleri, hazırlık/bekleme geçişleri, geçersiz yapılandırma, sınırlı ömür ve tek hasar hakkı.

PlayMode: navigasyonda yaklaşma/durma/geri çekilme, engelli kaçış, duvarın görüşü kesmesi, kaçılabilir mermi, atıcıdan bağımsız mermi ömrü, pause, iptal, temizlik ve mevcut Charger/drop/wave regresyonları.

## Uygulama değerleri ve kararları

İlk Ranger: 75 can, 10 hasar, 25 XP ve mevcut Normal sandık profili. Tercih edilen mesafe 5–9 m, eşik payı 1 m; atış menzili 12 m; hazırlık 0.6 sn; bekleme 1.5 sn; mermi hızı 7 m/sn, yarıçapı 0.15 m, ömrü 4 sn. Bunlar değiştirilebilir prototip değerleridir. Geri çekilme, yeni atış başlatmaya göre önceliklidir. Görüş hattı kesilirse hazırlık iptal olur ve atış yapılmaz. Örtü arkasında duran düşman bekleyebilir; bilinçli yandan dolaşma sonraya bırakılır.

Yön, atış anında hedef collider merkezine göre belirlenir (açık hedefte collider yoksa kök + 1 m). Mermiler hareketin tamamını tarar, başlangıç çakışmasını kontrol eder, en yakın katı engel veya aktarılmış hedefle çarpışır; atıcı/düşman collider'larını ve trigger'ları yok sayar, en fazla bir kez sonuçlanır. Uzun karede de hareket mermi ömrüyle sınırlanır. Mermi hasarı atış olayından ayrıdır; ateş anında anlık hasar verilmez. Atılmış mermiler atıcının sahnesinde kök nesne olarak yaşar ve atıcının temizlenmesinden bağımsızdır.

Mevcut düşman prefabı bileşimi sağlar. Ranger davranışı EnemyAttackController'ın yönlendirdiği ayrı bir runtime içinde yürütülür. EnemyMotor navigasyon komutlarını ve geri çekilme yolu doğrulamasını yönetir. Ranger mavi kimlik rengi ve sarı hazırlık uyarısıyla ayrılır. Otomatik doğrulama 14.09.2026 tarihinde izole Unity 6000.3.9f1 projesinde 700 EditMode ve 378 PlayMode testiyle başarılıdır. Ranger oynanış kabulü ve denge ayarı beklenir.
