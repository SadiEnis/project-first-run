# Ödül seviye artırma seçenekleri

Ödül teklifleri yeni edinim ve seviye artırma için aynı `ItemDefinition` kimliğini kullanır. `RewardCandidateFilter`, sahip olunan öğeyi yalnızca runtime kopyası maksimumun altındaysa dahil eder; kategorinin dolu olması böyle bir seviye artırmayı engellemez. Maksimum seviyedeki öğeler çıkarılır.

Claim handler'ları önce seviye artırmayı dener; yalnızca öğe sahipli değilse edinime geri döner. Başarılı seviye artırma mevcut `Claimed` sonucunu döndürür, oturumu bir kez tüketir, slot kullanmaz ve edinim olayı yayınlamaz. UI, build kopyasına göre seçenekleri `NEW`, `LEVEL UP` veya `MAX` olarak etiketler.

Reroll, altın fallback'i, çoklu seçim, kayıt migrasyonu veya otomatik seviye artırma eklenmez. Silah/yetenek/geliştirme runtime controller'ları etkileri atomik uygulamaktan sorumludur.

## Doğrulama — 2026-09-14

İzole Unity 6000.3.9f1 ortamında tam paketler geçti: 666 EditMode ve 351 PlayMode testi. Güncellenen showcase, üç kategorinin her birinde yeni edinimin ardından seviye artırma seçimini kapsar.
