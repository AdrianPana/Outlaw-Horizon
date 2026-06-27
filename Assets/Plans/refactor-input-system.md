# Project Overview
- Game Title: Outlaw Horizon
- High-Level Concept: A third-person action game featuring a player character, ships, and special abilities (Glove).
- Players: Single player
- Target Platform: Standalone Windows (PC)
- Render Pipeline: URP
- Input System: New Input System (using generated C# wrapper `InputSystem_Actions`)

# Game Mechanics
## Core Gameplay Loop
The player explores the world, controls a character or a ship, and uses special abilities to interact with the environment (wind/gravity modifiers).
## Controls and Input Methods
- Character Movement: WASD / Left Stick
- Camera Rotation: Mouse / Right Stick
- Jump: Space / South Button
- Ability Cycle/Use: Keyboard/Gamepad actions

# UI
- Ability Menu: Pauses game and allows selecting modifiers using the cursor.
- Mobile Input (if applicable): Virtual sticks and buttons (currently using `VirtualInput`).

# Key Asset & Context
- `StarterAssetsInputs.cs`: The script to be removed. It currently acts as a data bridge for input values.
- `InputSystem_Actions.cs`: The generated C# class for the Input Action Asset. This will be the primary source of input.
- `ThirdPersonController.cs`: Main movement script.
- `ShipMoveScript.cs`: Ship camera and movement script.
- `GloveScript.cs`: Ability management and cursor control.

# Implementation Steps

## Step 1: Refactor ThirdPersonController
- **Description**: Remove dependency on `StarterAssetsInputs` and use `InputSystem_Actions` directly.
- **Assigned role**: developer
- **Dependencies**: None
- **Parallelizable**: Yes
- **Details**:
    - Add `private InputSystem_Actions _inputActions;`
    - Initialize `_inputActions = new InputSystem_Actions();` in `Awake`.
    - Enable/Disable in `OnEnable`/`OnDisable`.
    - Update `CameraRotation()` to read `_inputActions.Player.Look.ReadValue<Vector2>()`.
    - Update `Move()` to read `_inputActions.Player.Move.ReadValue<Vector2>()`.
    - Implement a `_jumpInput` flag updated by `_inputActions.Player.Jump.WasPressedThisFrame()` (or event) to maintain jump buffering logic.
    - Implement a `_sprintInput` flag updated by `_inputActions.Player.Sprint.IsPressed()`.

## Step 2: Refactor ShipMoveScript
- **Description**: Replace `StarterAssetsInputs` with `InputSystem_Actions`.
- **Assigned role**: developer
- **Dependencies**: None
- **Parallelizable**: Yes
- **Details**:
    - Similar to Step 1, update camera rotation logic to read directly from the generated input class.

## Step 3: Refactor GloveScript
- **Description**: Remove `StarterAssetsInputs` dependency and update cursor logic.
- **Assigned role**: developer
- **Dependencies**: None
- **Parallelizable**: Yes
- **Details**:
    - Replace `starterInputs.SetCursorState(bool)` calls with a direct implementation or a shared utility call to `Cursor.lockState` and `Cursor.visible`.

## Step 4: Cleanup CannonScript
- **Description**: Remove the unused `starterInputs` variable and `GetComponent` call.
- **Assigned role**: developer
- **Dependencies**: None
- **Parallelizable**: Yes

## Step 5: Scene and Prefab Cleanup
- **Description**: Remove the `StarterAssetsInputs` component from all GameObjects and prefabs.
- **Assigned role**: developer
- **Dependencies**: Steps 1-4
- **Parallelizable**: No
- **Details**:
    - Update `Player.prefab`, `PlayerArmature.prefab`, `PlayerCapsule.prefab`.
    - Remove the component from scenes where applicable.
    - Update `PlayerInput` components to not send messages (optional, as they will be ignored anyway).

## Step 6: Final Deletion
- **Description**: Delete `StarterAssetsInputs.cs` and potentially `VirtualInput.cs` if mobile support is not required or is migrated to `OnScreen` components.
- **Assigned role**: developer
- **Dependencies**: Step 5
- **Parallelizable**: No

# Verification & Testing
- **Movement**: Verify character moves correctly in all directions.
- **Camera**: Verify camera rotation works with mouse.
- **Jump**: Verify jump and jump buffering still work.
- **Ability Menu**: Verify the cursor appears/disappears correctly when opening/closing the ability menu.
- **Ship**: Verify ship camera rotation works.
- **Console**: Ensure no "Missing Component" or "NullReferenceException" errors appear.
