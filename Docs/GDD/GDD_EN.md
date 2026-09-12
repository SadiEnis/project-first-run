# GDD_EN

# Game Design Document

> **Living Document Notice:** This is a living design document. Any decision that significantly changes the intended player experience should be reflected in this document. Unconfirmed ideas must be clearly marked as provisional or open questions.
> 

---

## 1. Document Information

| Field | Value |
| --- | --- |
| Project Name | Project First Run |
| Document Type | Game Design Document |
| Document Version | v0.1 |
| Status | Pre-Production |
| Genre | First-Person Roguelite / Arena Shooter |
| Initial Platform | PC |
| Game Engine | Unity 6.3.9 |
| Version Control | Unity Version Control / Plastic SCM |
| Last Updated | 2026-07-31 |
> Project First Run is the current development codename. The final commercial title may change.
---

## 2. Vision Statement

Create a fast-paced first-person roguelite in which every run develops differently through meaningful build choices, aggressive combat, and visible power growth.

The game combines manually controlled firearms with automatically activated abilities. Players must remain mobile, aim effectively, manage their weapons, and build synergies between weapons, abilities, upgrades, and evolutions.

Every system should strengthen the player's feeling of becoming more powerful without making combat unreadable or unnecessarily complicated.

---

## 3. High Concept

A fast-paced first-person roguelite that combines aggressive FPS gunplay, automated survival-game abilities, arena-based progression, build crafting, and permanent progression between runs.

---

## 4. Game Overview

The player fights through a sequence of combat arenas filled with waves of enemies, elite enemies, and bosses. The main objective of a run is to survive, improve the current build, progress through every arena, and eventually defeat the final boss.

The player actively uses magazine-based firearms while a collection of abilities activates automatically. Weapons reward aiming, positioning, ammunition management, and timing. Abilities provide additional damage, crowd control, area denial, defensive space, and automatic target pressure.

Enemies drop experience points and may also provide gold or reward chests. Gaining enough experience increases the player's run level. Each level-up guarantees a chest, while enemies and elite enemies may drop additional chests according to their own drop probabilities.

Chests allow the player to acquire new weapons, abilities, and upgrades or increase the level of already-owned items. Certain higher-rarity chests may also offer an evolution when the required conditions have been met.

After a run ends, part of the collected gold is transferred to permanent progression. Successful runs and final-boss rewards allow players to unlock additional equipment slots and other long-term improvements.

---

## 5. Main Inspirations

| Game | Main Inspiration |
| --- | --- |
| DOOM | Aggressive first-person combat, movement, weapon impact, and combat intensity |
| Vampire Survivors | Automatic abilities, run-based leveling, build progression, and equipment evolutions |
| Dead Cells | Arena-to-arena progression, escalating encounters, bosses, and run structure |

These games are reference points rather than exact templates. The project should develop its own identity through the combination of active FPS combat, automated abilities, chest-based progression, and changing arenas.

---

## 6. Design Pillars

### 6.1 Aggressive Combat

The player should be encouraged to move, aim, reposition, and engage enemies. Remaining stationary or playing too passively should usually create additional danger.

### 6.2 Meaningful Build Choices

Chest rewards should create decisions rather than merely present one obviously superior option. Weapons, abilities, and upgrades should support different playstyles and build directions.

### 6.3 Satisfying Power Growth

The player's power should visibly and mechanically increase throughout a run. Improvements should affect combat feedback, projectile count, area size, targeting, crowd control, or behavior whenever appropriate—not only hidden numerical values.

### 6.4 Build Diversity

Different runs should support meaningfully different builds, such as close-range aggression, ranged area damage, automated ability pressure, tank-focused survival, high-risk damage, status-effect builds, and boss-focused damage.

### 6.5 Controlled Chaos

Combat may include numerous enemies, explosions, projectiles, drones, lightning effects, and area attacks. Despite this intensity, the player must still understand threats, damage sources, targets, pickups, and important effects.

### 6.6 Replayability

Random chest rewards, different equipment combinations, evolutions, arena conditions, and long-term progression should make repeated runs feel worthwhile.

### 6.7 Easy to Learn, Hard to Master

The basic controls and progression systems should be understandable, while movement, aim, positioning, resource management, and build optimization provide long-term mastery.

### 6.8 Player Feedback First

Upgrades should feel noticeable. When possible, an improvement should produce visible, audible, or mechanical feedback.

Examples:

- Additional shotgun pellets instead of only increased damage.
- Additional lightning chains instead of only increased damage.
- Fragmentation projectiles instead of an unclear second explosion.
- Additional drones instead of only a passive damage multiplier.

---

## 7. Core Player Fantasy

The player begins a run with limited equipment and gradually becomes a highly destructive combat force.

At the beginning, enemies should feel dangerous and resources should be limited. As the player gains levels, opens chests, improves items, and unlocks evolutions, the battlefield should become increasingly chaotic in the player's favor.

The desired emotional progression is:

> Vulnerable survivor → capable fighter → specialized build → overwhelming combat force
> 

---

## 8. Core Gameplay Loop

```
Start Run
    ↓
Enter Arena
    ↓
Fight Enemy Waves
    ↓
Collect Experience and Gold
    ↓
Level Up
    ↓
Receive and Open a Chest
    ↓
Acquire or Improve an Item
    ↓
Strengthen the Current Build
    ↓
Fight Elite Enemies
    ↓
Defeat Arena Boss
    ↓
Advance to the Next Arena
    ↓
Repeat with Increased Difficulty
    ↓
Defeat Final Boss
    ↓
Receive Final Reward
    ↓
Return to Base
    ↓
Purchase Permanent Improvements
    ↓
Start a New Run
```

---

## 9. Run Structure

A run consists of multiple connected combat arenas.

Each arena contains waves of enemies and may include elite encounters, environmental mechanics, or arena-specific modifiers. Completing the required encounters allows the player to progress.

Bosses appear at planned intervals. The final objective is to reach and defeat the final boss.

The arena system is intended to prevent the experience from becoming visually and mechanically repetitive. Different arenas should eventually contain their own layouts, hazards, combat conditions, and encounter identities.

The exact number of arenas, bosses, and encounter stages has not yet been determined.

---

## 10. Player Equipment Structure

The game contains three main equipment categories:

1. **Weapons:** Manually aimed and fired magazine-based firearms.
2. **Abilities:** Automatically activated combat abilities.
3. **Upgrades:** Passive stat modifiers or gameplay-changing traits.

The upgrade category may internally contain different design classifications, such as stat upgrades and traits, but the player uses one shared upgrade slot pool. Separate stat and trait slot systems will not be created.

### 10.1 Content Pool

The currently planned content pool contains:

| Category | Total Planned Items |
| --- | --- |
| Weapons | 4 |
| Abilities | 8 |
| Upgrades | 10 |

The final ten upgrades have not yet been selected.

### 10.2 Equipment Slots

The currently proposed starting slot limits are:

| Category | Starting Slots | Proposed Maximum |
| --- | --- | --- |
| Weapons | 1 | 2 |
| Abilities | 3 | 5 |
| Upgrades | 5 | 8 |

Additional slots may be unlocked through permanent progression.

These values are provisional and must be validated through playtesting. Slot limits should create meaningful build decisions without preventing the player from experiencing enough variety during a run.

---

## 11. Weapons

Weapons are actively controlled by the player. They use magazines, ammunition, reload times, accuracy behavior, and different combat ranges.

Each base weapon currently has eight levels. Reaching the final level makes the weapon eligible for evolution if the remaining evolution requirements are also satisfied.

Final numerical values will be determined during balancing.

### 11.1 Shotgun

**Role:** Close-range burst damage and crowd control.

The shotgun fires multiple pellets in a wide spread. It is highly effective against nearby groups but less reliable at long range.

| Level | Current Design |
| --- | --- |
| 1 | Unlock the base Shotgun |
| 2 | Increase damage |
| 3 | Increase magazine capacity |
| 4 | Increase knockback |
| 5 | Increase damage |
| 6 | Reduce reload duration |
| 7 | Increase magazine capacity |
| 8 | Increase pellet count and enable evolution eligibility |

The exact placement of the second knockback improvement may be reconsidered during balancing.

### 11.2 Minigun

**Role:** Sustained damage and aim-based pressure.

The minigun fires rapidly and rewards players who can keep their aim on targets while managing recoil and ammunition.

| Level | Current Design |
| --- | --- |
| 1 | Unlock the base Minigun |
| 2 | Increase damage |
| 3 | Increase critical-hit chance |
| 4 | Increase magazine capacity |
| 5 | Increase fire rate |
| 6 | Increase damage |
| 7 | Reduce reload duration |
| 8 | Reduce recoil and enable evolution eligibility |

Accuracy, spread recovery, and recoil progression will be reviewed when the firing model is prototyped.

### 11.3 Rocket Launcher

**Role:** Long-range area damage and crowd clearing.

The Rocket Launcher has a small magazine and a slower firing cycle but creates powerful explosions against grouped enemies.

| Level | Current Design |
| --- | --- |
| 1 | Unlock the base Rocket Launcher |
| 2 | Increase damage |
| 3 | Increase rocket speed |
| 4 | Increase magazine capacity |
| 5 | Reduce reload duration |
| 6 | Increase damage |
| 7 | Increase explosion radius |
| 8 | Explosion releases fragmentation projectiles and enables evolution eligibility |

The original double-explosion concept was replaced with fragmentation to provide clearer visual and mechanical feedback.

### 11.4 Plasma Rifle

**Role:** Medium-range sustained damage, piercing, and burn application.

The Plasma Rifle fires energy projectiles that can pass through enemies and eventually apply burn damage.

| Level | Current Design |
| --- | --- |
| 1 | Unlock the base Plasma Rifle |
| 2 | Increase damage |
| 3 | Increase fire rate |
| 4 | Projectiles can pierce one enemy |
| 5 | Increase damage |
| 6 | Increase magazine capacity |
| 7 | Projectiles can pierce two enemies |
| 8 | Projectiles apply burn and enable evolution eligibility |

---

## 12. Abilities

Abilities activate automatically according to their own targeting, cooldown, duration, and behavior rules.

Each base ability currently has eight levels. Reaching the final level makes the ability eligible for evolution if the remaining requirements are satisfied.

### 12.1 Force Wave

**Role:** Short-range crowd control and emergency space creation.

The ability releases a short-range force wave in the direction the player is facing.

| Level | Current Design |
| --- | --- |
| 1 | Unlock one frontal force wave |
| 2 | Increase damage |
| 3 | Increase area |
| 4 | Reduce cooldown |
| 5 | Increase knockback |
| 6 | Increase damage |
| 7 | Increase area |
| 8 | Reduce cooldown and enable evolution eligibility |

### 12.2 Fireball

**Role:** Automatic ranged damage and burn pressure.

Fireballs target random enemies within a defined range.

| Level | Current Design |
| --- | --- |
| 1 | Fire two fireballs per activation |
| 2 | Increase damage |
| 3 | Reduce cooldown |
| 4 | Fire three fireballs |
| 5 | Increase damage |
| 6 | Increase projectile speed |
| 7 | Fire four fireballs |
| 8 | Increase burn duration and enable evolution eligibility |

### 12.3 Drone

**Role:** Continuous automatic target pressure.

Drones remain near the player and automatically fire at enemies.

| Level | Current Design |
| --- | --- |
| 1 | Deploy one drone |
| 2 | Increase damage |
| 3 | Increase fire rate |
| 4 | Increase targeting range |
| 5 | Increase damage |
| 6 | Deploy a second drone |
| 7 | Increase fire rate |
| 8 | Increase fire rate when targeting bosses and enable evolution eligibility |

### 12.4 Lightning Staff

**Role:** Random area damage, stun, and chain damage.

Lightning strikes random enemies or positions within range.

| Level | Current Design |
| --- | --- |
| 1 | Create one lightning strike |
| 2 | Increase damage |
| 3 | Reduce cooldown |
| 4 | Create two lightning strikes |
| 5 | Increase damage |
| 6 | Lightning stuns affected enemies |
| 7 | Create three lightning strikes |
| 8 | Lightning chains to one additional enemy and enables evolution eligibility |

### 12.5 Shuriken

**Role:** Close-range circular protection and repeated contact damage.

A shuriken rotates around the player for two complete rotations when activated.

| Level | Current Design |
| --- | --- |
| 1 | Create one rotating shuriken |
| 2 | Increase damage |
| 3 | Increase rotation speed |
| 4 | Reduce cooldown |
| 5 | Increase damage |
| 6 | Create a second shuriken |
| 7 | Increase effective radius |
| 8 | Apply bleeding and enable evolution eligibility |

### 12.6 Acid Bottle

**Role:** Area denial, damage over time, and slowing.

Acid bottles fall onto random targets and create damaging areas.

| Level | Current Design |
| --- | --- |
| 1 | Throw one acid bottle |
| 2 | Increase acid damage |
| 3 | Increase area duration |
| 4 | Increase radius |
| 5 | Throw two acid bottles |
| 6 | Increase damage |
| 7 | Increase area duration |
| 8 | Acid areas slow enemies and enable evolution eligibility |

### 12.7 Enchanted Staff

**Role:** Unpredictable piercing beams and battlefield coverage.

The ability fires beams in random directions. Beams travel for a limited duration, pass through enemies, and change direction at random points.

| Level | Current Design |
| --- | --- |
| 1 | Create one beam |
| 2 | Increase damage |
| 3 | Increase beam duration |
| 4 | Reduce cooldown |
| 5 | Create two beams |
| 6 | Increase damage |
| 7 | Increase beam duration |
| 8 | Reduce cooldown and enable evolution eligibility |

### 12.8 Sniper Bomb

**Role:** High-priority target damage and precision explosive pressure.

The Sniper Bomb homes toward a target within range. Elite enemies and bosses receive targeting priority.

| Level | Current Design |
| --- | --- |
| 1 | Launch one bomb |
| 2 | Increase damage |
| 3 | Increase explosion radius |
| 4 | Reduce cooldown |
| 5 | Launch two bombs |
| 6 | Increase damage |
| 7 | Retarget if the original target dies before impact |
| 8 | Increase explosion radius and enable evolution eligibility |

---

## 13. Upgrades

Upgrades support the player's general statistics or provide traits that change how the player approaches combat.

All upgrades use the same slot pool. The distinction between stat upgrades and traits exists for design organization only.

The target is currently ten upgrades, but the final selection has not been completed.

### 13.1 Stat Upgrade Candidates

| Upgrade | Current Concept |
| --- | --- |
| Damage Buff | Increases general damage |
| Armor Buff | Increases damage mitigation |
| Maximum Health | Increases maximum health |
| Health Regeneration | Restores health over time |
| Loot Magnet | Increases pickup collection radius |
| Experience Gain | Increases gained experience |
| Cooldown Reduction | Reduces ability cooldowns; exact affected systems are TBD |
| Movement Speed | Increases player movement speed |
| Attack Speed | Increases weapon fire rate where applicable |

A dedicated burn-damage upgrade is currently not preferred because it may be unusable for too many builds.

### 13.2 Trait Upgrade Candidates

| Upgrade | Current Concept |
| --- | --- |
| Berserker | Grants increased damage while health is below a threshold |
| Heavy Armor | Greatly increases armor but reduces movement speed |
| Blood Pact | Increases damage while reducing maximum health |
| Glass Cannon | Provides a large offensive bonus at the cost of survivability |
| Lucky | Improves valuable-chest, rarity, and possibly evolution-related chances |
| Ammo Expert | Removes or reduces the movement penalty while reloading |
| Vampire | Provides a chance or method to recover health by killing enemies |

Trait values, level structures, interactions, and final inclusion are not yet confirmed.

---

## 14. Evolution System

An evolution is not treated as an additional level of the original item. It is a new weapon or ability that replaces the original item in the same slot.

This allows an evolution to retain the original concept or completely change its combat behavior.

Example:

```
Fireball
    +
Required Upgrade
    ↓
Meteor Ability
```

The base Fireball may launch projectiles from the player, while its evolved result may create meteors that fall from above. Because the evolved result is a separate item, it may use entirely different targeting, visuals, damage delivery, timing, and behavior.

### 14.1 Current Evolution Rules

- The source weapon or ability must reach its maximum level.
- One or more required upgrades or conditions may be needed.
- Whether the required upgrade must also reach maximum level is not yet confirmed.
- The evolved item replaces the source item in the same slot.
- Evolved items currently have no additional levels.
- Evolutions are offered through eligible chests.
- If multiple items can evolve, one eligible result is selected randomly when an evolution reward is generated.

---

## 15. Chest System

The game currently contains eight chest types.

Chest rewards may contain:

- A new weapon, ability, or upgrade when an appropriate slot is available.
- A level increase for an owned item that has not reached maximum level.
- An eligible evolution when supported by the chest.
- Gold only when no meaningful equipment reward can be generated.

Items already at maximum level are removed from the normal reward pool.

Gold is not intended to replace a valid reward. It is used only when every relevant slot is full and every owned item in the applicable pool has reached maximum level.

### 15.1 Weapon Chest

- Offers three choices.
- Contains weapons or weapon-level improvements.
- Common chest.
- Represented by grey.

### 15.2 Ability Chest

- Offers three choices.
- Contains abilities or ability-level improvements.
- Common chest.
- Represented by grey.

### 15.3 Upgrade Chest

- Offers three choices.
- Contains upgrades or upgrade-level improvements.
- Common chest.
- Represented by grey.

### 15.4 Green Chest

- Offers three choices.
- The player selects one.
- May contain weapons, abilities, or upgrades.
- If an evolution is currently available, one option has a 25% chance to become an evolution.
- Uncommon rarity.
- Represented by green.

### 15.5 Purple Chest

- Offers four choices.
- The player selects one.
- May contain weapons, abilities, or upgrades.
- If an evolution is currently available, one option has a 50% chance to become an evolution.
- Rare rarity.
- Its current planned visual color is blue, despite the working name “Purple Chest.” Naming will be reviewed.

### 15.6 Legendary Chest

- Offers five choices.
- The player selects two.
- May contain weapons, abilities, or upgrades.
- If an evolution is currently available, one option has a 75% chance to become an evolution.
- Legendary rarity.
- Represented by purple.

### 15.7 Boss Chest

- Drops only from defeated bosses.
- Offers six choices.
- The player selects two.
- May contain weapons, abilities, or upgrades.
- If an evolution is available, at least one option is guaranteed to be an evolution.
- Legendary-tier reward.
- Represented by purple.

### 15.8 Golden Chest

- Drops only after defeating the final boss.
- Contains a large gold reward.
- Does not use the normal multi-choice reward screen.
- After collecting the reward, the player receives an option to return to the base.
- The exact gold value is not yet balanced.

---

## 16. Experience and Leveling

Enemies drop experience pickups when defeated.

Collecting enough experience increases the player's current run level. Run level resets when the run ends.

Every level-up guarantees a chest. The chest type is determined by a weighted probability table.

Normal enemies and elite enemies may also drop chests. Their chest-drop chances and chest-type weightings are calculated separately.

Bosses use their dedicated Boss Chest reward, while the final boss also provides the Golden Chest.

Experience is one of the main sources of short-term progression and should create a frequent sense of approaching the next reward.

---

## 17. Enemies and Encounters

Arenas contain waves of enemies that pressure the player through different movement, attack, range, and durability patterns.

The project will include:

- Normal enemies.
- Elite enemies.
- Arena bosses.
- A final boss.

Enemy families, exact counts, spawn rules, and boss mechanics have not yet been designed.

Enemy variety should create tactical differences rather than only larger health and damage values.

---

## 18. Gold and Run Economy

Gold should be represented by a simple UI counter rather than a second progression bar.

Enemies may drop gold during a run. The player accumulates run gold until the run ends.

If the player dies, only part of the collected gold is retained.

The current provisional permanent-upgrade progression is:

| Upgrade Level | Gold Retained on Death |
| --- | --- |
| Default | 40% |
| Level 2 | 50% |
| Level 3 | 60% |
| Level 4 | 70% |

The maximum planned retention rate is currently 70%.

Successfully defeating the final boss is expected to preserve the run's reward and provide an additional Golden Chest reward. The exact rules for banking gold after intermediate bosses remain an open question.

---

## 19. Meta Progression

After a run, the player returns to a base or main progression area.

Permanent gold may be spent on long-term improvements.

Confirmed or currently proposed improvements include:

- Additional weapon slots.
- Additional ability slots.
- Additional upgrade slots.
- Increased percentage of gold retained after death.

Further permanent improvements may be added later, but the meta-progression system should not eliminate the importance of skill or build choices.

---

## 20. User Interface Principles

The interface should communicate critical information without covering too much of the first-person view.

Expected core HUD elements include:

- Health.
- Ammunition and magazine state.
- Experience bar and run level.
- Gold counter.
- Ability status or cooldown feedback where necessary.
- Boss health when applicable.
- Interaction and chest prompts.

There will not be a separate gold-progression bar unless future testing proves it necessary.

Chest reward screens should clearly show:

- Item name.
- Item category.
- Current or new level.
- Gameplay effect.
- Whether the reward is new, an improvement, or an evolution.
- The number of remaining selections.

---

## 21. Audio and Visual Feedback

Combat feedback is a core part of the experience.

Weapons, abilities, upgrades, evolutions, damage, kills, level-ups, chest openings, elite encounters, and bosses should have distinct feedback.

Visual effects should reinforce power without hiding enemies or hazards. Important enemy attacks and dangerous ground effects must remain readable during intense combat.

---

## 22. Current Scope

The current intended scope includes:

- Single-player first-person combat.
- Magazine-based weapons.
- Automated abilities.
- Passive and trait-based upgrades.
- Equipment levels.
- Equipment evolutions.
- Experience and run leveling.
- Eight chest types.
- Normal enemies, elite enemies, bosses, and a final boss.
- Arena-based progression.
- Run gold and permanent progression.
- Unlockable equipment slots.
- A base or progression area between runs.

---

## 23. Out of Scope

The following features are not currently planned:

- Online multiplayer.
- Competitive multiplayer.
- Cooperative multiplayer.
- Open-world exploration.
- Traditional crafting.
- Player trading.
- Procedurally generated open worlds.
- Separate stat-upgrade and trait-upgrade slot systems.

Features may be reconsidered only if they clearly support the core design pillars and do not threaten the project's achievable scope.

---

## 24. Technical Constraints Relevant to Design

- The project will be developed with Unity 6.3.9.
- Unity Version Control / Plastic SCM will be used.
- The project should support data-driven content creation.
- ScriptableObjects are expected to define content data where appropriate.
- Gameplay design should not depend on unnecessary technical abstractions.
- Systems should favor clear responsibilities, maintainability, extensibility, and testability.
- Architecture decisions belong primarily in the Technical Design Document rather than this document.

---

## 25. Open Questions

- What will the final project name be?
- How long should an average successful run last?
- How many arenas will a complete run contain?
- How frequently will bosses appear?
- How many enemy families will be created?
- Which ten upgrades will form the final upgrade pool?
- How many levels will each upgrade have?
- Which weapons, abilities, and upgrades combine into evolutions?
- Must the required evolution upgrade also be at maximum level?
- Can an item have more than one possible evolution?
- Will intermediate bosses permanently bank part of the current run gold?
- What are the final chest-drop probabilities?
- What are the final evolution probabilities after luck modifiers?
- How will the Lucky upgrade affect separate probability systems?
- What permanent upgrades will exist beyond slots and gold retention?
- Will arenas be handcrafted, randomly selected, or arranged through a branching path?
- Will arena modifiers be present in the initial release?
- What happens when a chest cannot generate enough unique valid choices?
- How will difficulty scale between arenas and throughout a run?
- What is the final death, victory, and return-to-base flow?
- What accessibility options will be required for effects, camera motion, and combat readability?

---

## 26. Initial Decision Log

### GDD-001 — Active Weapons and Automatic Abilities

**Decision:** Weapons are manually controlled, while abilities activate automatically.

**Reason:** This combination preserves FPS skill expression while allowing the build to create increasingly powerful automated combat effects.

### GDD-002 — Arena-Based Run Structure

**Decision:** Runs progress through changing combat arenas rather than one continuous static map.

**Reason:** Arena changes improve pacing, variety, encounter identity, and long-term engagement.

### GDD-003 — Shared Upgrade Slots

**Decision:** Stat upgrades and traits use the same upgrade slot pool.

**Reason:** Separate slot categories would add unnecessary UI and inventory complexity.

### GDD-004 — Evolution as Item Replacement

**Decision:** An evolved weapon or ability is a separate item that replaces its source in the same slot.

**Reason:** Evolutions may completely change targeting, damage delivery, presentation, and behavior.

### GDD-005 — Meaningful Reward Filtering

**Decision:** Maximum-level items are removed from chest reward pools. Gold is offered only when no valid equipment reward remains.

**Reason:** Reaching maximum level should not reduce the quality of future rewards or punish the player.

### GDD-006 — Probabilistic Evolutions

**Decision:** Green, Purple, and Legendary Chests use different evolution probabilities rather than guaranteeing an evolution.

**Reason:** Evolutions should remain exciting and valuable rather than becoming predictable rewards from every rare chest.

### GDD-007 — Simple Gold Interface

**Decision:** Gold uses a counter rather than a separate progression bar.

**Reason:** The HUD already contains health, ammunition, experience, and combat feedback. A second progression bar would create unnecessary visual load.

### GDD-008 — Professional Simplicity

**Decision:** The project will prioritize clear, maintainable solutions and avoid unnecessary abstractions or references.

**Reason:** A system must not only function; it must also be understandable, maintainable, and appropriate for the actual project requirements.

---

## 27. Revision History

| Version | Date | Changes |
| --- | --- | --- |
| v0.1 | 2026-07-31 | Created the initial foundation GDD. Documented the vision, design pillars, core loop, equipment concepts, current weapon and ability designs, upgrade candidates, evolution rules, chest system, economy, meta progression, scope, open questions, and initial decisions. |