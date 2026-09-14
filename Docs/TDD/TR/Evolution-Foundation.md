# Evolution temeli

Evolution ilave eşya seviyesi değil, kaynak öğenin değiştirilmesidir. `EvolutionDefinition`; bir kaynak öğe, sonuç öğe ve isteğe bağlı öğe gereksinimleri içerir. Kaynak maksimum seviyede sahip olunmalı; sonuç sahip olunmamalı ve kaynakla aynı kategoriye ait olmalıdır. `PlayerEvolutionController`, bütün koşulları doğruladıktan sonra kaynak stable ID'sini aynı `PlayerBuild` slotunda sonuç stable ID'siyle atomik biçimde değiştirir.

Sonuç öğe tanımlı maksimumuyla seviye 1'den başlar. Bu temelde yeni gerçek gameplay asset'i oluşturulmaz. Silah/yetenek runtime davranışının değiştirilmesi, evolution ödül sandıkları, nihai içerik dengesi ve görseller sonraki işlerdir. İsteğe bağlı gereksinimler, ileride “gerekli geliştirme maksimum seviyede” kuralını tüm sisteme zorunlu kılmadan temsil edebilir.

## Doğrulama — 2026-09-14

İzole Unity 6000.3.9f1 projesinde 668 EditMode ve 354 PlayMode testi geçti. Geçici ScriptableObject'ler maksimum seviye koşulunu, isteğe bağlı gereksinimleri, aynı kategori doğrulamasını, sonuç sahipliği çakışmasını, aynı slotta değiştirmeyi ve hata durumunda atomikliği doğrular.
