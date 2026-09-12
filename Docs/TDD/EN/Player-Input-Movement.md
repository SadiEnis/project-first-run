# Player Input and Movement System

## 1. Purpose

This module manages player input, character movement, and first-person camera
control in Project First Run.

The first version supports:

- Keyboard and mouse
- Gamepad
- Walking
- Sprinting
- First-person camera control
- Gravity
- Ground detection

Jumping, dashing, crouching, head bob, and movement-based camera effects are outside
the scope of the first version.

---

## 2. Design Goals

- Input reading and physical movement must remain separate.
- The movement system must not depend on a specific input device.
- The Unity Input System must remain inside the input layer.
- Movement code should remain readable and testable where practical.
- The player prefab must not persist between arena scenes.
- The player must be reconstructed for every arena scene.
- Controls should support rebinding in a later version.
- Gamepad support must be included from the beginning for the Steam Deck target.

---

## 3. System Boundaries

### PlayerInputReader

Responsibilities:

- Communicate with the Unity Input System
- Read Move, Look, and Sprint input
- Manage the Input Action Map lifecycle
- Expose input values to gameplay components

Not responsible for:

- CharacterController movement
- Gravity
- Camera rotation
- Player stats
- Animation

### PlayerMotor

Responsibilities:

- Calculate horizontal movement
- Apply walking and sprinting speeds
- Calculate gravity
- Call CharacterController.Move
- Track the grounded state

Not responsible for:

- The Input System
- Camera rotation
- Player attacks
- Camera effects
- Persistent stat management

### PlayerLook

Responsibilities:

- Rotate the player body horizontally
- Rotate the camera pivot vertically
- Clamp vertical camera rotation
- Apply mouse and gamepad sensitivity

Not responsible for:

- Movement
- CharacterController behavior
- Weapon recoil
- Camera shake
- FOV effects

### PlayerController

Responsibilities:

- Coordinate PlayerInputReader, PlayerMotor, and PlayerLook
- Forward input values to the appropriate gameplay components each frame
- Enable and disable player control

PlayerController does not own gameplay rules or input devices.

---

## 4. Input Action Structure

Input Action Asset:

`ProjectFirstRunInputActions`

### Gameplay Action Map

| Action | Type | Control Type | Description |
|---|---|---|---|
| Move | Value | Vector2 | Horizontal player movement |
| Look | Value | Vector2 | Camera rotation input |
| Sprint | Button | Button | Sprint input |
| Pause | Button | Button | Pause menu request |

### Control Schemes

#### KeyboardMouse

- Keyboard
- Mouse

#### Gamepad

- Gamepad

The Steam Deck will use the Gamepad control scheme for gameplay.

---

## 5. Default Bindings

### Move

KeyboardMouse:

- W: Forward
- S: Backward
- A: Left
- D: Right

Gamepad:

- Left Stick

### Look

KeyboardMouse:

- Mouse Delta

Gamepad:

- Right Stick

### Sprint

KeyboardMouse:

- Left Shift

Gamepad:

- Left Stick Press

### Pause

KeyboardMouse:

- Escape

Gamepad:

- Start

---

## 6. Movement Model

Movement is calculated relative to the horizontal orientation of the player:

`movement = forward * inputY + right * inputX`

The movement vector is normalized to prevent diagonal movement from becoming faster
than movement along a single axis.

PlayerMotor selects either walking or sprinting speed. These values will later be
provided by StatCollection.

Acceleration and deceleration are instantaneous in the first version. Smoothing will
only be added when playtesting demonstrates a clear need.

---

## 7. Gravity

CharacterController does not apply gravity automatically. PlayerMotor therefore owns
and updates vertical velocity every frame.

A small negative vertical velocity is retained while grounded to keep the controller
in stable contact with the ground.

The player cannot jump in the first version.

---

## 8. Camera Model

Horizontal look input rotates the player body around the Y axis.

Vertical look input rotates only the camera pivot around the X axis.

Vertical rotation is clamped to:

- Minimum pitch: -85 degrees
- Maximum pitch: 85 degrees

Mouse input is delta-based. Gamepad input is processed as a time-based rotation
speed. Separate sensitivity settings are therefore used for mouse and gamepad.

---

## 9. Update Flow

Every frame:

1. PlayerController reads input from PlayerInputReader.
2. Look input is forwarded to PlayerLook.
3. Move and Sprint input are forwarded to PlayerMotor.
4. PlayerMotor combines horizontal and vertical movement.
5. CharacterController.Move is called once.

Movement runs in Update because the system does not use Rigidbody-based physics.

---

## 10. Lifecycle

PlayerInputReader:

- Enables the Gameplay Action Map in OnEnable.
- Disables the Gameplay Action Map in OnDisable.

Player control may be disabled during pause, reward selection, or arena transitions.

Movement and look input must be reset when input is disabled.

---

## 11. Initial Configuration

| Setting | Initial Value |
|---|---:|
| Walk Speed | 5 |
| Sprint Speed | 8 |
| Gravity | -25 |
| Grounded Vertical Speed | -2 |
| Mouse Sensitivity | 0.1 |
| Gamepad Look Speed | 160 |
| Minimum Pitch | -85 |
| Maximum Pitch | 85 |

These are starting values and will be adjusted through playtesting.

---

## 12. First-Version Acceptance Criteria

- The player can move with WASD.
- The player can move with the left analog stick.
- The camera can be controlled with the mouse.
- The camera can be controlled with the right analog stick.
- Diagonal movement does not provide additional speed.
- Sprinting applies only while Sprint input is active.
- Vertical camera rotation cannot flip.
- Gravity pulls the player toward the ground.
- The player can move across slopes within CharacterController limits.
- The player stops responding when input is disabled.
- The Console produces no errors or continuous warnings.

---

## 13. Deferred Topics

- Jumping
- Dashing
- Crouching
- Head bob
- Footsteps
- Camera shake
- Weapon recoil
- Control rebinding
- Sensitivity settings UI
- Gamepad aim acceleration
- Aim assist
- External forces and knockback

These features will be handled through separate decisions after the initial movement
prototype has been validated.