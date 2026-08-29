# Project Overview
- **Game Title**: Outlaw Horizon
- **High-Level Concept**: A third-person action platformer where the environment is influenced by global modifiers like wind and gravity. The player must navigate these dynamic environments by riding and grabbing onto moving physical objects.
- **Players**: Single player.
- **Inspiration / Reference Games**: Modern 3D platformers with physics-based environmental puzzles.
- **Tone / Art Direction**: Stylized action.
- **Target Platform**: PC (StandaloneWindows64).
- **Screen Orientation / Resolution**: Landscape 1920x1080.
- **Render Pipeline**: URP (PC_RPAsset).
- **Input System**: New Input System.

# Game Mechanics
## Core Gameplay Loop
- Exploration and platforming using a responsive third-person character.
- Interacting with "Rideable" objects that are moved by environmental forces (Wind, Gravity).
- Ledge grabbing and hanging to navigate vertical environments.
- Solving puzzles by timing movements with dynamic platforms.

## Controls and Input Methods
- **WASD/Joystick**: Movement relative to camera.
- **Space/Button South**: Jump / Ledge Jump.
- **Mouse/Joystick**: Camera control.
- **Automatic Ledge Grab**: Triggered by falling near a ledge.

# UI
- Standard HUD for game state (not the focus of this refactor).
- Debug indicators for grounding and ledge detection.

# Key Asset & Context
- `ThirdPersonController.cs`: The main player controller script. Needs refactoring to a Kinematic Character Controller (KCC) with parenting support.
- `Rideable.cs`: Attached to objects that can be ridden. Acts as a hook for the parenting system.
- `ModifierAffectedObject.cs`: The base class for moving platforms, which uses `rb.MovePosition` and can be kinematic.

# Implementation Steps

## Step 1: Refactor ThirdPersonController to Kinematic
- **Description**: Convert the `ThirdPersonController` from a pseudo-dynamic Rigidbody to a proper Kinematic Character Controller.
- **Files**: `Assets/Starter Assets/Runtime/ThirdPersonController/Scripts/ThirdPersonController.cs`
- **Changes**:
    - In `Start()`, set `rb.isKinematic = true`.
    - Remove code that manually zeros `linearVelocity` and `angularVelocity` in `FixedUpdate` (Lines 232-233).
    - Ensure all movement uses `rb.MovePosition()` and rotation uses `rb.MoveRotation()`.
- **Assigned role**: developer
- **Dependencies**: None
- **Parallelizable**: No

## Step 2: Implement the Parenting System
- **Description**: Implement a system to parent the player to `Rideable` objects when grounded or hanging.
- **Files**: `Assets/Starter Assets/Runtime/ThirdPersonController/Scripts/ThirdPersonController.cs`
- **Changes**:
    - Add a private `Transform _currentPlatform` field.
    - Create a `SetPlatform(Transform newPlatform)` method that handles `transform.SetParent(newPlatform, true)`.
    - Update `GroundedCheck()` to detect if the ground has a `Rideable` component and call `SetPlatform`.
    - Update `LedgeCheck()` to call `SetPlatform` when a ledge is grabbed.
    - Update `JumpAndGravity()` and `Move()` to call `SetPlatform(null)` when the player jumps or leaves a platform.
- **Assigned role**: developer
- **Dependencies**: Step 1
- **Parallelizable**: No

## Step 3: Clean up Movement and Ledge Logic
- **Description**: Remove legacy manual displacement logic and fix synchronization.
- **Files**: `Assets/Starter Assets/Runtime/ThirdPersonController/Scripts/ThirdPersonController.cs`
- **Changes**:
    - Remove the `platformVelocity` calculation and addition in `Move()` (Lines 410-418).
    - Remove manual `_ridable.Velocity` offset in `LedgeCheck()` (Lines 460-461).
    - Simplify `SnapToGround()` to work with the new kinematic setup.
- **Assigned role**: developer
- **Dependencies**: Step 2
- **Parallelizable**: No

## Step 4: Script Execution Order Optimization
- **Description**: Ensure platforms update their positions before the player processes movement.
- **Files**: Project Settings
- **Changes**:
    - Set `ModifierAffectedObject` and `Rideable` execution order to `-10`.
    - Set `ThirdPersonController` execution order to `0`.
- **Assigned role**: developer
- **Dependencies**: Step 3
- **Parallelizable**: Yes

# Verification & Testing
- **Test Riding**: Place a `ModifierAffectedObject` with `Rideable` in a scene, apply wind to move it, and verify the player stays perfectly centered on it without jitter.
- **Test Ledge Grab**: Grab the ledge of a moving platform. The player should stay attached and move with the ledge.
- **Test Jumping**: Ensure jumping off a moving platform works correctly and clears the parent.
- **Edge Case: Scale**: Verify that riding a scaled platform does not distort the player (the `SetParent(newPlatform, true)` with `worldPositionStays` should handle this, but might need `localScale` resetting if issues occur).
- **Edge Case: Rapid Movement**: Verify that platforms moving at high speeds (due to strong wind) don't cause the player to clip through or fly off.