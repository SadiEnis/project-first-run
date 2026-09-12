# Combat Foundation

## 1. Purpose

This module provides the shared health, damage, and death foundation for
Project First Run.

Weapons, automatic abilities, enemy attacks, and environmental hazards must use
the same damage contract.

The first version covers:

- Creating damage requests
- Representing damageable targets
- Managing maximum and current health
- Detecting death
- Returning damage results to the caller
- Local health and death notifications
- Validation through a target dummy

---

## 2. Design Goals

- Health rules should remain independent from Unity components where practical.
- Current health must have one source of truth.
- Weapons must not depend on specific enemy classes.
- Death decisions must remain separate from death animation and destruction.
- Invalid damage values must be rejected safely.
- The system must support object pooling later.
- A death notification must be raised only once per death.

---

## 3. System Boundaries

### DamageInfo

Carries the Unity runtime context of a damage request.

The first version contains:

- Amount
- Source
- Hit Point
- Hit Direction

DamageInfo is not persistent data and is not stored between scenes.

### DamageResult

Returns the outcome of a damage operation to the calling system.

The first version contains:

- Requested damage
- Applied damage
- Previous health
- Current health
- Whether damage was applied
- Whether the damage was lethal

### IDamageable

Defines the boundary between damage-dealing systems and damageable targets.

Weapons and abilities must not depend directly on Enemy, Player, or Prop classes.

Contract:

`DamageResult ApplyDamage(in DamageInfo damageInfo)`

### HealthState

A pure C# runtime model.

Responsibilities:

- Store maximum health
- Store current health
- Apply damage
- Clamp health between zero and maximum health
- Determine the dead state
- Reset health for reuse

It does not know about GameObjects, MonoBehaviours, animation, audio, or effects.

### HealthComponent

The bridge between HealthState and the Unity scene.

Responsibilities:

- Receive the initial maximum health
- Create HealthState
- Implement IDamageable
- Forward damage from DamageInfo to HealthState
- Publish health and death notifications
- Support explicit reset after pooling

Not responsible for:

- Destroying the object
- Playing death animations
- Producing loot
- Granting experience
- Updating score or objectives
- Displaying damage numbers

---

## 4. Damage Rules

- Zero or negative damage is not applied.
- A dead target does not accept additional damage.
- Current health never falls below zero.
- Applied damage cannot exceed the target's remaining health.
- Death occurs when current health reaches zero for the first time.
- The death notification is raised only once.
- Health values use `float`.
- Armor, resistance, and critical damage are outside the first version.

Example:

```text
Current Health: 30
Requested Damage: 50
Applied Damage: 30
New Health: 0
Lethal: Yes