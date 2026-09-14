# Geliştirme seviyesi etkileri

## Doğrulama — 2026-09-14

İzole Unity 6000.3.9f1 projesinde 666 EditMode ve 348 PlayMode testi geçti. Sekiz yeni PlayMode testi; etki değiştirme, dolu slotta ilerleme, maksimum/tekrar edinme, kopyalanmış seviyeler, aynı kaynak kimliğindeki bağımsız etkiler, eksik etkiler, tanım/build tutarsızlığı, hatalı gelecek seviyeler ve hata veren aboneleri kapsar. Değişen/yeni sekiz Unity dosyası test kopyasıyla SHA-256 üzerinden eşleşti. Bu otomatik doğrulamadır; gelecekteki ödül UI'ının manuel kabulü değildir.

Silah etkilerinden sonra item-level-foundation branch'inde devam eder. Sonraki her seviye ilave farkı değil, etkilerin tamamını tanımlar. Edinmeden önce bütün seviyeler doğrulanıp kopyalanır. Varsayılan tek seviyedir; geliştirme amaçlı hasar öğesi üç geçici seviye kullanır (silah ve yetenek hasarı +%20, +%30, +%40). Bu, tüm geliştirmeler için belirlenmiş maksimum değildir.

`PlayerUpgradeController.TryLevelUp`, tanım referansı, build seviyesi/maksimumu ve kurulmuş etki nesnelerinin eşleşmesini gerektirir. Sahiplik yokluğu ve maksimum seviye normal sonuçtur; tutarsızlık değişiklikten önce hata verir. Yalnızca ilgili geliştirmenin tam etki nesneleri değiştirilir; aynı kaynak kimliğine sahip olanlar dahil diğer etkiler korunur. Ek slot veya edinme olayı üretilmez.

Sahiplik, runtime seviyesi ve bütün etkiler tamamlandıktan sonra tek stat değişim bildirimi yayınlanır. Abone hatası işlem tamamlandıktan sonra oluşur ve yarım uygulanmış etki bırakmaz. Asset'in sonradan değişmesi kopyalanmış seviyeleri etkilemez. Yetenek seviyeleri ve sandık ödülü/UI bağlantısı sonraki adımlardır; agent merge veya sürüm kontrolü yazma işlemi yapmaz.
