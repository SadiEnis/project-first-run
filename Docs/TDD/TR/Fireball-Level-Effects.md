# Fireball seviye etkileri

Fireball, silah ve geliştirme etkilerinden sonra item-level-foundation branch'inde devam eder. Edinilen runtime sekiz geçici konfigürasyonu kopyalar. Bu adım temel hasar, cooldown ve projectile hızını uygular; tek runtime kaydını korur ve mevcut otomatik hedefleme/çalıştırma akışını kullanır.

Seviye 1 mevcut asset değerlerini korur. 2–8 seviyeleri edinmeden önce doğrulanan tam mutlak değerlerdir. Edinimden sonra asset'i değiştirmek runtime'ı yeniden ayarlamaz. Seviye artırma PlayerBuild sahipliği, tam tanım referansı ve runtime seviyesi eşleşmesini gerektirir; sahiplik yokluğu ve maksimum seviye normal sonuç, tutarsızlıklar ise değişikliksiz hatadır.

Projectile sayısı, burn süresi, evolution uygunluğu, UI ve sandık ödülü seçimi bu adıma bağlanmadı. Bunlar ayrı davranış/evolution kararları gerektirir.

## Doğrulama — 2026-09-14

İzole Unity 6000.3.9f1 projesinde 666 EditMode ve 351 PlayMode testi geçti. Üç yeni PlayMode testi cooldown ilerlemesini, edinim anı kopyasını, tekrar/tanım alias korumasını ve tek kayıt sahipliğini kapsar. Agent sürüm kontrolü işlemi yapmaz.
