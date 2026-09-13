# Düşman kaynaklı sandık drop'ları

Güncel seçim verisi: profiller artık ortak `ChestDropTable` yapısına referans verir; eski gömülü girişler [Sandık Seviyesi Temeli](Chest-Tier-Foundation.md) aşamasında ayrı normal/elit asset'lerine taşınmıştır. Aşağıdaki ihtimal değerleri ve ölüm/kuyruk sözleşmeleri değişmez. Nadirlik bir etikettir; ihtimal çekilişini değiştirmez.

## Kapsam ve başlangıç

Branch tabanı: oyuncunun level-up sandıklarını doğrulamasından sonra Plastic `/main/dev` **cs:151**, Git `main` **6e42508**. Düşman ölümü sandıkları, XP ve garantili level-up sandıklarına ektir.

Bu aşama ölümde sandık düşme ihtimalini ve ağırlıklı sandık tanımı seçimini ekler. Her `EnemyDefinition` isteğe bağlı bir `EnemyChestDropProfile` kullanır; normal ve elit içerikler ayrı ihtimal ve tablolar kullanabilir. Yeni düşman sınıfı enum'u, elit savaş davranışı, boss ödülü, yeni sandık nadirlik içeriği, Luck etkisi ve altın telafisi eklenmez. Level-up kaynağının geliştirme sandığı seçimi değişmez.

## Sözleşmeler

- İhtimal on binde bir birimiyle tanımlanır: 0 = asla, 10000 = kesin. Başarılı ihtimal çekilişinden sonra pozitif tam sayı ağırlıklarla ayrı bir sandık seçimi yapılır. Sayılar henüz denge kararı değildir.
- Profil yoksa sandık yoktur. Etkin profilde eksik sandık, geçersiz ağırlık ve taşan toplam açıkça reddedilir. Sıfır ihtimalde tablo gerekmez. Rastgelelik mevcut `IRandomSource` ile enjekte edilir; asset üzerinde değişken çalışma durumu tutulmaz.
- `EnemySpawner`, sahnedeki `EnemyChestDropSource` bağımlılığını prefabın `EnemyChestDrop` bileşenine düşman başlatılmadan önce verir. Etkin profil bu iki bağımlılığı gerektirir; profilsiz düşman kullanan diğer spawner'lar etkilenmez.
- `EnemyChestDrop` gerçek ölümcül ölüm olayını bir kez dinler. Registry'den çıkış, disable ve temizlik ölüm değildir. Çekilişten önce deneme tüketilir; drop hataları XP dahil diğer ölüm dinleyicilerine yayılmaz. Aynı düşmanın pooling/yeniden başlatılması bu aşamanın dışında kalır.
- Başarılı çekilişte seçilen tanım ile ölüm konumu/yönü değer olarak kuyruğa alınır. Ölüm callback'inde nesne oluşturulmaz, ceset referansı tutulmaz, UI açılmaz ve konum tekrarlarında yeniden çekiliş yapılmaz.
- Sahne kaynağı kuyruğu düşman ömründen bağımsız tutar. Önce ölüm noktası, sonra yakın halkalar ortak düz zemin/ayak izi/engel/görüş hattı kontrollerinden geçirilir. Engellenen kayıt silinmez, kuyruğun sonuna alınır; diğer drop'ları kilitlemez. Update başına en fazla bir deneme yapılır; başarısız konum denemeleri saniyede dörde sınırlandırılır. Oyuncuya doğru taşıma ve sahneler arası kayıt yoktur.
- Pause, oyuncu ölümü, devre dışı işleme ve henüz hazır olmayan chest spawner kuyruğu tüketmez. Spawn istisnasında kayıt korunur; hata bir kez yazılıp otomatik işleme kapatılır. Yapılandırma düzeltildikten sonra yeniden açılmalıdır.

## Geliştirme bağlantıları ve elle doğrulama

İki test dalgası ayrı `ED_ChaserChestDropTest` tanımını kullanır. Chest servislerini içermeyen eski saldırı test sahneleri için `ED_ChaserBasic` profilsiz kalır; savaş ve XP değerleri değişmez.

`Test_Waves` mevcut chest spawner, placement ve oyuncu sağlığını paylaşır. Chaser, geçici **%25** ihtimalli ve tek geliştirme sandığına ağırlık 1 veren `CDP_DevelopmentNormal` kullanır. `CDP_DevelopmentElite`, gelecekteki elit tanımları için bağımsız **%50** örneğidir; dalgalarda şu anda elit düşman yoktur. Yeni sandık içerikleri ertelendiğinden iki profil de aynı sandığı seçer.

1. XP toplamadan chaser öldür: bazı ölümler, level-up'tan bağımsız olarak ölüm yerine yakın sandık üretmelidir. Kısa bir seride hiç sandık çıkmaması mümkündür.
2. Kesin elle test için Play öncesinde normal profil ihtimalini geçici olarak 10000 yap: her ölüm bir sandık üretmelidir. 0 yalnızca düşman sandıklarını kapatır. Testten sonra 2500 değerini geri yükle.
3. XP düşmesi, çekim ve level atlama korunur; her level ayrıca garantili sandık verir. Sandık E ile açılır ve başarılı seçimden sonra bir kez tüketilir.
4. Çoklu ölüm, pause/devam ve ceset temizliği kuyruktaki ödülü çoğaltmamalı veya silmemelidir. Engelli ölüm konumları sahne yaşadığı sürece tekrar denenir.
5. Küçük mevcut ödül havuzunun uygun seçenekleri tükenebilir. Bu drop hatası değildir; eşya seviyesi ödülleri ve altın telafisi ayrı iştir.

Otomatik doğrulama ihtimal sınırları, ağırlık aralıkları, geçersiz veri, ölüm yaşam döngüsü, bağımlılık enjeksiyonu, kuyruk tekrarları/anlık değerleri, işleme koşulları ve gerçek sahne/prefab bağlantılarını kapsar. Çalıştırılmış test sonuçları doğrulama sonrasında kök README'ye kaydedilir.
