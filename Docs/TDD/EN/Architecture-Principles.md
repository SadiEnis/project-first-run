# Architecture-Principles

# Technical Goals, Non-Goals, and Architecture Principles

## 1. Purpose

This section defines the technical quality standards and architectural rules of the project.

These rules exist to keep the codebase maintainable, testable, performant, and easy to extend without introducing unnecessary abstraction.

The architecture is not considered successful merely because the game functions. It should also make responsibilities understandable, dependencies visible, content creation practical, and future changes reasonably safe.

---

## 2. Technical Goals

### 2.1 Maintainable and Readable Code

The codebase should be understandable without requiring extensive knowledge of hidden conventions.

Each system should have:

- A clear responsibility.
- A clear owner for its state.
- Explicit inputs and outputs.
- Limited and visible dependencies.
- Names that describe intent rather than implementation details.

A class should not be considered well designed only because it is small. Its responsibility and reason to change must also be clear.

### 2.2 Data-Driven Content Creation

Weapons, abilities, upgrades, evolutions, enemies, waves, arenas, and rewards should be defined through data where this improves content production.

ScriptableObjects may store:

- Stable identifiers.
- Display information.
- Level data.
- Numerical configuration.
- Prefab and presentation references.
- Content relationships.
- Designer-controlled settings.

ScriptableObjects must not be used as shared mutable runtime state.

Data-driven design does not mean that every behavior must be represented through generic data structures. Unique gameplay behavior may use dedicated runtime code when that solution is clearer.

### 2.3 Explicit Dependency Management

Dependencies should be visible through:

- Constructor parameters for pure C# objects.
- Serialized references for local Unity scene and prefab dependencies.
- Explicit initialization methods when runtime construction is required.
- Context objects at scene boundaries.

The project should avoid hidden dependency access such as global singleton lookups, static service access, and repeated runtime object searches.

### 2.4 Clear Lifetime and Ownership Boundaries

Application, run, and scene state must remain separated.

Every important piece of state should have one authoritative owner.

Examples:

- Permanent progression belongs to `PlayerProfile`.
- Current run progression belongs to `RunSession`.
- Current arena enemies belong to the arena scene.
- Current weapon ammunition belongs to the weapon runtime instance.

Two different systems must not independently maintain authoritative copies of the same state.

### 2.5 Testable Gameplay Rules

Important gameplay rules should be implementable as pure C# where practical.

Examples include:

- Damage calculation.
- Armor calculation.
- Experience thresholds.
- Chest reward filtering.
- Evolution eligibility.
- Slot validation.
- Gold retention.
- Stat modifier calculation.

Unity-specific presentation and scene behavior may remain in MonoBehaviours, but core rules should not require a loaded scene when this can reasonably be avoided.

### 2.6 Extendable Content Systems

Adding a new item should normally require:

1. Creating its definition data.
2. Providing its presentation assets.
3. Selecting or implementing its runtime behavior.
4. Registering it in the appropriate catalog.

Existing systems should not require large switch statements or modification whenever a new weapon, ability, or upgrade is added.

However, the project will not attempt to make every possible future item configurable without code.

### 2.7 Editor-Friendly Workflow

Content creation should be practical inside the Unity Editor.

The project should support:

- Clear Inspector fields.
- Validation of missing or duplicate identifiers.
- Useful default values.
- Organized ScriptableObject creation menus.
- Warnings for invalid content relationships.
- Test scenes for isolated gameplay systems.

Custom editors should be created only when the default Inspector becomes insufficient.

### 2.8 Performance-Aware Architecture

The project targets smooth play on Steam Deck-class hardware.

The provisional target is:

- 1280 × 800 resolution.
- 60 FPS during normal gameplay.
- Full gamepad support.
- Readable UI on a handheld display.

Performance-sensitive systems should be designed with:

- Object pooling for frequently created objects.
- Controlled update frequency.
- Efficient target queries.
- Limited runtime allocations.
- Scalable VFX and quality settings.
- Profiling on actual builds.

Optimizations must be based on measurements rather than assumptions.

### 2.9 Platform-Independent Gameplay

Gameplay code should not directly depend on a specific input device or desktop-only API.

The architecture should support:

- Keyboard and mouse.
- Gamepad.
- Steam Deck controls.
- Resolution-independent UI.
- Platform-specific services behind clear boundaries.

Possible future console support should remain feasible without making the initial PC project unnecessarily complex.

### 2.10 Safe Persistence

Saved data should be versioned and separated from runtime scene objects.

The save system should support:

- Profile versioning.
- Migration between supported save versions.
- Backup or recovery from incomplete writes.
- Separation of profile progression and device settings.
- Stable content identifiers instead of display names.

---

## 3. Technical Non-Goals

The following are intentionally not goals of the initial architecture.

### 3.1 Building a General-Purpose Roguelite Framework

The project may produce reusable systems, but it is being designed for this game first.

The architecture will not be generalized for imaginary future projects unless a real requirement appears.

### 3.2 Creating an Interface for Every Class

Interfaces will not be added automatically.

An interface is justified when it provides a real boundary, supports multiple implementations, enables meaningful testing, or separates high-level rules from infrastructure.

A single implementation alone is not sufficient justification.

### 3.3 Making Every Interaction Event-Driven

Events will not replace clear method calls.

Events are appropriate when multiple independent listeners need to react to an occurrence. Direct calls are preferred when one object explicitly requests work from another object.

A global event bus will not be used initially.

### 3.4 Introducing a Dependency Injection Framework at Project Start

The initial project will use explicit dependency construction and injection.

A DI framework may be reconsidered only if manual composition becomes a demonstrated maintenance problem.

### 3.5 Creating a Fully Generic Effect or Ability Graph

The initial architecture will not attempt to represent every weapon, ability, trait, and evolution through one universal effect system.

Shared patterns will be extracted after real repetition appears.

Unique behavior may remain in dedicated classes.

### 3.6 Using Addressables for Every Asset

Addressables will be introduced where asynchronous loading, memory control, content grouping, or platform-specific delivery provides a clear benefit.

Small and permanently loaded assets do not need to become Addressable by default.

### 3.7 Premature Micro-Optimization

Code clarity will not be sacrificed for unmeasured performance gains.

Hot paths will be profiled before complex optimization is introduced.

### 3.8 Persisting Gameplay GameObjects Between Arenas

Player, camera, weapon, ability, enemy, projectile, and arena objects will not persist between arena scenes.

Persistent run data will be reconstructed into new scene objects.

### 3.9 Supporting Mid-Run Save and Resume Initially

The initial release does not require restoring an unfinished run after closing the game.

The possibility may be revisited later through a serializable run snapshot.

### 3.10 Supporting Multiplayer

Networking, authority, replication, and multiplayer synchronization are outside the current project scope.

---

## 4. Architecture Principles

### 4.1 Gameplay Requirements Drive Architecture

Architecture exists to support the game.

A technically elegant system that makes content creation or gameplay iteration difficult is not considered successful.

Before introducing a pattern, the project should identify the concrete problem it solves.

### 4.2 Prefer the Simplest Correct Solution

The preferred solution is the simplest one that:

- Meets current requirements.
- Keeps responsibilities clear.
- Can be safely modified.
- Does not create obvious future blockers.

“Simplest” does not mean placing all logic in one class. It means avoiding complexity that does not provide current value.

### 4.3 One Authoritative Owner per State

Every important state value must have one source of truth.

Other systems may observe, display, or temporarily cache values, but they must not become competing owners.

### 4.4 Separate Definition, Runtime State, and Presentation

The project distinguishes three concepts:

```
Definition
    Static design data

Runtime State
    Mutable state of the current object or run

Presentation
    Visual, audio, animation, and UI representation
```

Example:

```
ShotgunDefinition
    Damage, magazine size, level configuration, prefab references

ShotgunRuntime
    Current ammunition, reload state, temporary modifiers

ShotgunView
    Model, animation, muzzle flash, sound
```

Not every item must use three separate classes. The separation is conceptual and should be implemented only as far as the actual complexity requires.

### 4.5 Prefer Composition over Deep Inheritance

Shared capabilities should generally be composed from focused components or services.

Deep inheritance trees that combine identity, behavior, state, and presentation should be avoided.

Inheritance is acceptable when:

- The relationship is genuinely substitutable.
- Shared behavior is stable.
- Derived classes do not need to disable or contradict base behavior.

### 4.6 Keep Unity at the Boundaries Where Practical

MonoBehaviours should be used for Unity-specific responsibilities such as:

- Transforms.
- Physics.
- Collision.
- Animation.
- Scene references.
- Unity lifecycle methods.
- Visual and audio presentation.

Pure gameplay calculations should remain outside MonoBehaviours where this improves clarity and testing.

This is a preference, not a requirement to wrap every Unity API behind an interface.

### 4.7 Make Dependencies Explicit

A system should receive only what it needs.

A weapon should not receive the complete `RunSession` when it only requires a stat source and ammunition configuration.

Large context objects must not become disguised service locators.

### 4.8 Avoid Global Mutable State

The project will avoid:

- `GameManager.Instance`
- Static mutable gameplay data
- Global service locators
- Shared mutable ScriptableObjects
- Global event buses

Static classes may be used for stateless utility functions or constants when appropriate.

### 4.9 Choose Communication Based on Intent

Use a direct method call when:

- The caller knows which system should perform the work.
- An immediate result is required.
- The relationship is explicit and local.

Use a C# event when:

- Something has already happened.
- Multiple independent systems may react.
- The sender should not know the receivers.

Examples:

```
Direct call:
ChestRewardGenerator.GenerateChoices()

Event:
EnemyDied
PlayerLevelledUp
RunEnded
```

### 4.10 Use Interfaces at Real Boundaries

Interfaces are appropriate for boundaries such as:

- Damageable targets with multiple implementations.
- Random-number sources used by deterministic tests.
- Save-storage implementations.
- Target-selection strategies.
- Time sources when time-dependent rules require testing.

Interfaces should remain focused and small.

### 4.11 Treat ScriptableObjects as Immutable Definitions

Runtime code must not modify persistent ScriptableObject asset data.

A ScriptableObject may be read during gameplay, but mutable values belong to runtime objects.

Editor validation should detect:

- Empty stable identifiers.
- Duplicate identifiers.
- Missing level data.
- Invalid evolution references.
- Invalid maximum levels.
- Missing required assets.

### 4.12 Avoid Manager Classes Without Specific Meaning

The word `Manager` should not be used as a replacement for an unclear responsibility.

Names should describe the actual role:

```
RunCoordinator
SceneFlowController
ChestRewardGenerator
EnemyRegistry
LoadoutController
```

A `Manager` name is acceptable only when the managed responsibility is narrow and well defined.

### 4.13 Build for Iteration

The first implementation is allowed to be simple.

Refactoring should occur when:

- Responsibility boundaries become unclear.
- Real duplication appears.
- A second implementation requires a boundary.
- Testing is unnecessarily difficult.
- Performance measurements identify a bottleneck.

Refactoring should not occur only to introduce patterns.

### 4.14 Validate Data Early

Invalid content should fail during development rather than produce silent runtime errors.

Validation should happen through:

- `OnValidate` where appropriate.
- Catalog validation.
- Automated tests.
- Editor warnings.
- Development-build assertions.

Player-facing builds should handle recoverable failures gracefully and record meaningful logs.

### 4.15 Measure Performance on Target Builds

Editor performance is not the final performance reference.

Profiling should be performed using development builds on representative hardware.

Performance work should focus on measured costs such as:

- CPU frame time.
- GPU frame time.
- Garbage collection allocations.
- Memory usage.
- Draw calls.
- Active enemy and projectile counts.
- Physics and navigation cost.
- VFX overdraw.

---

## 5. SOLID Application Guidelines

### Single Responsibility Principle

A class should have one clear reason to change.

A class may coordinate multiple collaborators if coordination itself is its defined responsibility.

### Open/Closed Principle

Systems should support new content through new definitions and focused behaviors where real variation exists.

The project will not force all future variation into abstractions before it is understood.

### Liskov Substitution Principle

Inheritance will only be used when derived implementations can safely replace their base type without changing expected behavior.

If derived classes need to disable large parts of the base class, composition should be preferred.

### Interface Segregation Principle

Interfaces should contain only the operations required by their consumers.

Large interfaces that expose unrelated gameplay functionality should be divided or avoided.

### Dependency Inversion Principle

High-level gameplay rules should not depend directly on replaceable infrastructure when doing so harms testing or maintainability.

Examples include random generation, save storage, and platform services.

Dependency inversion will not be applied mechanically to every concrete class.

---

## 6. Architectural Quality Checks

Before accepting an important system, the following questions should be answered:

1. What is the system’s exact responsibility?
2. Which state does it own?
3. Which dependencies does it require?
4. Are those dependencies visible?
5. Is there another authoritative copy of the same state?
6. Can the important rules be tested without loading a full gameplay scene?
7. Can new content be added without modifying unrelated systems?
8. Is an abstraction solving a current problem?
9. Does the system allocate or perform expensive work in a frequent update path?
10. Will the design remain understandable six months later?

A system that works but fails several of these checks should be reviewed before being considered complete.

---

## 7. Confirmed Technical Direction

| Area | Direction |
| --- | --- |
| Input | Unity Input System |
| Player movement | CharacterController |
| Render pipeline | URP |
| Primary handheld target | Steam Deck-class hardware |
| Scene structure | Separate arena scenes |
| Scene loading | Asynchronous Single loading |
| Run state | Pure C# runtime model |
| Persistent scene root | One restricted ApplicationRoot |
| Content definitions | ScriptableObject where appropriate |
| Runtime data | Separate mutable runtime models |
| Dependency access | Explicit injection and references |
| Global service locator | Not used |
| Global event bus | Not used |
| DI framework | Not used initially |
| Save format | Versioned local profile data |
| Addressables | Selective use after vertical slice |
| Optimization | Profiling-driven |
| Architecture style | Simple, explicit, data-driven, and testable |