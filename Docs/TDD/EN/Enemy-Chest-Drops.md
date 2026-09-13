# Enemy chest drops

## Scope and baseline

Branch baseline: Plastic `/main/dev` **cs:151**, Git `main` **6e42508**, after the player validated level-up chests. Enemy death drops are additional to XP and guaranteed level-up chests.

This stage adds configurable death-drop probability and weighted chest-definition selection. Each `EnemyDefinition` optionally references its own `EnemyChestDropProfile`, allowing normal and elite content to use independent chances and tables without introducing an enemy-class enum. It does not add elite combat behavior, boss rewards, new chest rarity content, Luck scaling, gold fallback, or change the level-up source's development definition.

## Contracts

- A profile has a chance in basis points: 0 = never, 10000 = always. A successful chance roll is followed by an independent weighted choice using positive integer weights. Probabilities are not balanced yet.
- A missing profile means no chest. Invalid enabled profiles fail explicitly; null definitions, invalid weights and overflowing totals are rejected. Zero-chance profiles need no entries. Randomness uses the existing injectable `IRandomSource`; no seed or roll state is stored in assets.
- `EnemySpawner` explicitly injects the scene's `EnemyChestDropSource` into the prefab's `EnemyChestDrop` before enemy initialization. Enabled profiles require both dependencies; unrelated enemy spawners remain valid with profile-free definitions.
- `EnemyChestDrop` listens to the enemy's actual lethal death once. Registry removal, disable and cleanup are not death signals. It reserves the attempt before rolling and isolates drop errors from other death observers (including XP). Pooling/reinitialization of the same enemy is not supported in this stage.
- A successful roll queues the selected definition and a value snapshot of the death position/facing. It does not instantiate during the death event, retain the corpse, open reward UI, or reroll on placement retries.
- The scene source owns pending drops independently of enemy lifetime. It tries the death point first, then nearby rings using shared flat-ground/footprint/obstruction/line-of-sight checks. Blocked requests remain pending and rotate to the back so another drop can proceed. At most one request is attempted per update; failed placement is throttled to four attempts/second. No relocation toward the player or cross-scene persistence is provided.
- Pause, player death, disabled processing, and a not-yet-ready chest spawner defer processing without consuming pending drops. A spawn exception keeps the request and disables automatic processing after logging; fix the configuration before reenabling.

## Development composition and manual verification

The two test waves use the dedicated `ED_ChaserChestDropTest` definition. `ED_ChaserBasic` remains profile-free for older attack-only scenes without chest services; combat and XP values are unchanged.

`Test_Waves` shares the existing chest spawner, placement and player health. Chaser content uses `CDP_DevelopmentNormal` with a provisional **25%** chance and one development-chest entry of weight 1. `CDP_DevelopmentElite` is a separate **50%** example for future elite definitions, not an enemy currently spawned by waves. Both currently select the same chest because additional chest content is deferred.

1. Kill chasers without collecting their XP: some deaths should produce a chest near the death position, independently of level-up. A short streak without chests is valid.
2. For deterministic manual testing, temporarily set the normal profile chance to 10000 before Play; every kill should give one chest. Set it to 0 to disable only enemy chests. Restore 2500 after testing.
3. XP still drops, attracts and levels the player; each level still gives its own guaranteed chest. Opening any chest still requires E, and a claim consumes it only once.
4. Multiple deaths, pause/resume and corpse cleanup must not duplicate or lose queued drops. Blocked death locations retry while the scene remains alive.
5. The small existing reward pool can exhaust eligible choices. This is not a drop failure; item-level rewards and gold fallback remain separate work.

Automated verification covers chance boundaries, weighted intervals, invalid data, death lifecycle, injection, queue retry/snapshots, gating and real scene/prefab wiring. Record executed suite results in the repository README after validation.
