# Player Death System

## 1. Purpose

This module manages the gameplay response to player death in Project First Run.

HealthComponent remains responsible for health and death decisions.
Player-specific behaviour is handled by a separate PlayerDeathController.

The first version covers:

- Listening for player death
- Disabling movement and camera control
- Disabling weapon use
- Releasing the cursor
- Processing death only once
- Restoring player control after HealthReset
- Publishing local death and revival notifications

Death UI, respawning, arena restart, and run termination are outside the scope of
this version.

---

## 2. Design Goals

- HealthComponent must not know about player-specific behaviour.
- PlayerController must not own the death decision.
- PlayerWeaponController must not depend directly on the health system.
- Player death must be processed only once.
- Disabling and restoring control must use explicit methods.
- The system must support reversible validation through HealthReset.
- A global event bus or GameManager must not be used.

---

## 3. PlayerDeathController

PlayerDeathController coordinates HealthComponent events and player gameplay
components.

Responsibilities:

- Listen to HealthComponent.Died
- Ensure death is processed only once
- Disable PlayerController control
- Stop PlayerWeaponController usage
- Publish PlayerDied
- Listen to HealthComponent.HealthReset
- Restore player control
- Publish PlayerRevived

Not responsible for:

- Reducing health
- Calculating damage
- Enemy attacks
- Drawing death UI
- Selecting a respawn position
- Reloading the scene
- Determining the run result
- Playing animation or audio

---

## 4. Death Flow

1. HealthComponent applies lethal damage.
2. HealthComponent publishes Died.
3. PlayerDeathController enters the dead state.
4. PlayerController control is disabled.
5. PlayerWeaponController weapon control is disabled.
6. The cursor is released.
7. PlayerDied is published.

Additional damage against the dead player must not create another death response.

---

## 5. Control Behaviour

When the player dies:

- Move input is not applied.
- Sprint input is not applied.
- Look input is not applied.
- Fire and Reload are not applied.
- Active reload progress may be paused.
- CharacterController may continue processing gravity.
- The player cannot move horizontally.

The existing PlayerController.SetControlEnabled method is used.

Weapon input is controlled through:

`SetWeaponControlEnabled(bool isEnabled)`

This method does not reset WeaponRuntimeState. It only pauses input and runtime
updates.

---

## 6. Reset and Reactivation

When HealthComponent.ResetHealth is called:

1. The player returns to maximum health.
2. HealthReset is published.
3. PlayerDeathController clears its dead state.
4. PlayerController is enabled again.
5. PlayerWeaponController is enabled again.
6. PlayerRevived is published.

Weapon ammunition is not automatically restored. That policy belongs to the future
respawn system.

---

## 7. First-Version Acceptance Criteria

- IsDead becomes true after player death.
- Death is processed only once.
- The player cannot move after death.
- The player cannot rotate the camera after death.
- The player cannot fire after death.
- The player cannot begin reloading after death.
- The cursor is released.
- HealthReset restores player control.
- HealthReset does not automatically modify weapon ammunition.
- The Console produces no errors or continuous warnings.

---

## 8. Deferred Topics

- Death screen
- Respawning
- Arena restart
- Run failure
- Checkpoints
- Death animation
- Camera collapse effect
- Audio effects
- Controller vibration
- Invulnerability after revival
- Enemies stopping attacks against a dead player