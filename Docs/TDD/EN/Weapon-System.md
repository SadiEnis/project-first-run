# Weapon System

## 1. Purpose

This module defines the shared runtime foundation for player weapons in
Project First Run.

The first version covers:

- ScriptableObject-based weapon definitions
- Pure C# runtime state
- Semi-automatic and automatic trigger modes
- Magazine and reserve ammunition
- Fire-rate limiting
- Manual reloading
- Hitscan shot resolution
- Integration with the Combat Foundation
- Local weapon notifications
- A Plasma Rifle prototype

Shotgun spread, Minigun spin-up, Rocket Launcher projectiles, and weapon switching
are outside the scope of the first version.

---

## 2. Design Goals

- Weapon configuration must remain separate from runtime state.
- ScriptableObject assets must not be modified at runtime.
- Ammunition and reload rules must remain independent from Unity components.
- Weapons must not depend on specific enemy classes.
- Damage must be applied through the IDamageable contract.
- Input reading must remain separate from weapon rules.
- Audio, visual, and camera effects must not own weapon state.
- The design should support two weapon slots and switching later.
- The first version must not become an unnecessary generic weapon framework.

---

## 3. System Boundaries

### ItemDefinition

Provides the minimal shared item identity required by future loadout and reward
systems.

The first version contains only:

- Stable ID
- Display Name
- Item Category

Loadout, reward selection, and item-level progression are not implemented by this
module.

### WeaponDefinition

A ScriptableObject containing immutable weapon configuration.

The first version contains:

- Stable ID and display name
- Trigger mode
- Base damage
- Magazine capacity
- Starting reserve ammunition
- Shots per second
- Reload duration
- Range
- Damage LayerMask

WeaponDefinition does not store current ammunition, cooldown, or reload progress.

### WeaponRuntimeState

A pure C# runtime model.

Responsibilities:

- Store magazine ammunition
- Store reserve ammunition
- Track fire cooldown
- Track reload state and remaining duration
- Determine whether a shot is allowed
- Consume ammunition after a valid shot
- Transfer ammunition when reload completes
- Reset state for reuse

It does not know about GameObjects, MonoBehaviours, the Input System, raycasts,
audio, or effects.

### PlayerWeaponController

The Unity component coordinating the player's active weapon.

Responsibilities:

- Read Fire and Reload values from PlayerInputReader
- Tick WeaponRuntimeState every frame
- Validate fire requests through runtime rules
- Invoke hitscan resolution for valid shots
- Publish local weapon events
- Expose weapon state to UI and presentation components

Not responsible for:

- Player movement
- Health and death rules
- Drawing ammunition UI
- Weapon animation
- Audio playback
- Camera recoil
- Loadout reward selection

### HitscanShotResolver

Resolves a hitscan shot against the Unity physics world.

Responsibilities:

- Determine the aim point from the camera centre
- Validate line of fire between the muzzle and aim point
- Prevent firing through nearby cover
- Resolve IDamageable from the hit target
- Create DamageInfo and apply damage
- Return the shot result to the caller

It does not manage ammunition, cooldown, or reload rules.

---

## 4. Input Action Structure

Two actions are added to the existing `Gameplay` action map.

| Action | Type | Control Type | Description |
|---|---|---|---|
| Fire | Button | Button | Active weapon trigger |
| Reload | Button | Button | Manual reload request |

### Bindings

#### Fire

- Mouse Left Button
- Gamepad Right Trigger

#### Reload

- Keyboard R
- Gamepad Button West / Xbox X

PlayerInputReader exposes:

- IsFireHeld
- WasFirePressedThisFrame
- WasReloadPressedThisFrame

The weapon system does not access InputAction instances directly.

---

## 5. Trigger Modes

The first version supports two trigger modes.

### SemiAutomatic

At most one shot is produced for each physical button press.

Holding the trigger does not repeatedly fire.

### Automatic

Shots are produced while the trigger remains held, subject to the configured fire
rate.

The initial Plasma Rifle prototype uses Automatic mode.

Burst and charge-based trigger modes are deferred.

---

## 6. Fire Rules

A weapon may fire only when:

- It has been initialized with a valid WeaponDefinition.
- It is not reloading.
- Its fire cooldown has completed.
- At least one round remains in the magazine.
- The required input has been received for its trigger mode.

A valid shot:

1. Consumes one magazine round.
2. Starts a new fire cooldown.
3. Resolves the hitscan shot.
4. Publishes ShotFired.

A round is consumed even when the hitscan shot misses.

A fire request with an empty magazine does not reduce ammunition below zero and may
publish DryFired.

The first version does not automatically reload an empty weapon.

---

## 7. Fire Rate

WeaponDefinition stores shots per second.

The shot interval is:

`fire interval = 1 / shots per second`

Cooldown is reduced using delta time each frame.

The first version creates at most one shot per frame and does not generate an
unlimited number of catch-up shots after a long frame.

---

## 8. Ammunition Model

WeaponRuntimeState is the single source of truth for:

- MagazineAmmo
- ReserveAmmo

Rules:

- Magazine ammunition never falls below zero.
- Magazine ammunition never exceeds capacity.
- Reserve ammunition never falls below zero.
- Reload requires an incomplete magazine and positive reserve ammunition.
- Firing is blocked while reloading.
- A new reload request is rejected while already reloading.
- A full magazine cannot begin reloading.
- Reload transfers only the required amount from reserve to magazine.

Example:

```text
Magazine Capacity: 12
Magazine Ammo: 5
Reserve Ammo: 4

After Reload:
Magazine Ammo: 9
Reserve Ammo: 0