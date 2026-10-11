# Weapon audio - first increment

2026-10-11. Optional direct AudioClip references on WeaponDefinition for four weapons. Packs remain under Assets/_Project/ThirdParty; no Resources scan or scene regeneration. Successful shots play once per volley, not per shotgun pellet; minigun follows actual shots, plasma/rocket follow successful launches. Gameplay values remain unchanged.

Initial candidates: three Free Weapon SFX shotgun variants, three assault rifle variants for minigun, grenade launcher fire for rocket; Free Laser Weapons light_blast_1 for plasma. Variants cycle without touching gameplay randomness. User accepted the sounds as basic firing feedback for the initial demo, not final sound design; minigun has no dedicated spin/motor sound yet.

Inspector Audio settings expose Fire Sounds, Reload Start/End Sound and volumes. Reload is two short cues at actual start/completion, not a stretched full animation recording. Manual/automatic reload share the same path; faster reload completion remains synchronized. Failed attempts produce no cue. Dry fire, equip, rocket explosions, abilities, enemies and ambience are deferred.

Local player audio is 2D with eight bounded shot voices and a separate reload voice, created on demand. The ninth overlapping shot steals the oldest voice. Time pause/control blocks pause audio; equip, reset and disable stop previous cues. The audio root is a player child and leaves with the scene. No new AudioListener. Mixer/settings UI are outside this increment.

Validation: WeaponAudioContentTests covers four assets and optional empty settings. Runtime and EditMode code compiled statically; no Unity test result was reported for this increment. User runs Unity tests. Manual checklist: all weapons, sustained/released minigun fire, one cue per shotgun volley, manual/automatic reload, Q during reload, reward/pause, death, ending and restart. Initial listening acceptance does not claim every regression above was individually verified. Crosshair presentation is the next task.
