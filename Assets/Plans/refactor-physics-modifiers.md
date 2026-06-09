# Project Overview
- Game Title: Outlaw Horizon
- High-Level Concept: A physics-based game with environmental modifiers like wind and gravity inversion.
- Players: Single player
- Inspiration / Reference Games: Action-puzzlers with gravity manipulation.
- Tone / Art Direction: TBD
- Target Platform: Standalone Windows 64
- Screen Orientation / Resolution: Landscape
- Render Pipeline: PC_RPAsset (URP)

# Game Mechanics
## Core Gameplay Loop
The player interacts with objects that are affected by environmental modifiers (Wind, Gravity). Objects can be "Controlled" (simulated kinematic movement) or "Ambient" (regular Rigidbody physics).

## Controls and Input Methods
The environment state changes (e.g., wind direction, gravity inversion) via a state manager, which affects all objects with `ModifierAffectedObject`.

# UI
N/A (Refactoring internal physics logic).

# Key Asset & Context
- `ModifierAffectedObject.cs`: Main script handling object movement and modifier influence.
- `GravityProvider.cs`: Implementation of `IModifierProvider` for inverted gravity.
- `WindProvider.cs`: Implementation of `IModifierProvider` for wind forces.

# Implementation Steps
## Step 1: Separate Passive Gravity from Modifier Influence
**Description**: Modify `ModifierAffectedObject.cs` to ensure `passiveGravityVelocity` is not instantly zeroed out when any modifier is active. Instead, it should continue to act unless explicitly overridden by a gravity-specific modifier.
**Assigned role**: developer
**Dependencies**: None
**Parallelizable**: No

## Step 2: Implement Gravity Override Detection
**Description**: Update the provider iteration loop in `HandleControlledMovement` to detect if a `GravityProvider` (or any provider designated as a gravity override) is active.
**Assigned role**: developer
**Dependencies**: Step 1
**Parallelizable**: No

## Step 3: Refactor Velocity Summation logic
**Description**: 
- Update `HandleControlledMovement` to:
    1. Calculate `passiveGravityVelocity` independently.
    2. Loop through all providers and sum their forces into `modifierForce`.
    3. If a gravity override is active, set `passiveGravityVelocity` to zero or allow the modifier to negate it.
    4. Combine `momentumVelocity + currentModifierVelocity + passiveGravityVelocity` for the final movement.
**Assigned role**: developer
**Dependencies**: Step 2
**Parallelizable**: No

## Step 4: Verification
**Description**: 
- Test with only Wind active: Object should move horizontally and fall normally (passive gravity).
- Test with only Inverted Gravity active: Object should move UP (if the provider returns UP force and passive gravity is overridden).
- Test with BOTH active: Object should move diagonally (Forward + UP).
**Assigned role**: developer
**Dependencies**: Step 3
**Parallelizable**: No

# Verification & Testing
- **Unit Test/Scene Test**: Use a test scene with a `WindProvider` and a `GravityProvider` on a `CONTROLLED` object.
- **Edge Case 1**: Entering a wind zone while falling. (Object should keep falling while moving sideways).
- **Edge Case 2**: Exiting a wind zone. (Wind velocity should decay while gravity continues to act).
- **Edge Case 3**: Toggling gravity inversion while in wind. (Vertical direction should change while maintaining horizontal wind velocity).
