# Project Overview
- **Game Title**: Outlaw Horizon
- **High-Level Concept**: A third-person action game with gravity and wind manipulation mechanics.
- **Players**: Single player
- **Target Platform**: PC (Windows)
- **Render Pipeline**: URP

# Game Mechanics
## Core Gameplay Loop
- Players can switch between a third-person character and ship-mounted cannons.
- Cannons fire cannonballs that can be influenced by wind and gravity abilities.
- Abilities are selected via a UI wheel and apply to either the environment (range-based) or specific targeted objects (like cannonballs).

## Targeted Ability System (Refined)
- Extend the state management to support targeted events.
- Specialized "Ship Glove" logic for cannon-to-cannonball interaction.

# Key Assets & Context
- `UniversalStateManagerScriptableObject.cs`: Manages global/targeted modifier state and events.
- `GloveScript.cs`: Base class for player ability interaction.
- `ShipGloveScript.cs`: Specialized glove for ship context.
- `CameraSwitcher.cs`: Provides information about the active camera view.
- `CannonScript.cs`: Fires cannonballs and registers them with the glove.
- `IModifierProvider` (Implementations: `GravityProvider`, `WindProvider`): Reacts to events.
- `Modifier` (Implementations: `GravityModifier`, `WindModifier`): Traditional movement-based modifiers.

# Implementation Steps

## 1. Update State Management
- **File**: `Assets/Scripts/UniversalStateManagerScriptableObject.cs`
- **Changes**:
    - Update `gravityInvertedEvent` to `UnityEvent<(Vector3 origin, float range, bool inverted, GameObject target)>`.
    - Update `windChangedEvent` to `UnityEvent<(Vector3 origin, float range, WindDirection direction, GameObject target)>`.
    - Update methods (`ToggleGravity`, `ToggleWind`, `Clear...`) to accept an optional `GameObject target`.
    - Remove the temporary `targetedObject` and `isTargetedMode` fields from previous implementation.

## 2. Refactor Base Glove
- **File**: `Assets/Scripts/GloveScript.cs`
- **Changes**:
    - Make `ToggleModifier` virtual to allow specialized behavior in child classes.

## 3. Implement Ship Glove
- **File**: `Assets/Scripts/ShipGloveScript.cs` (New)
- **Description**:
    - Inherit from `GloveScript`.
    - Add references to `latestShotCannonball` and `CameraSwitcher`.
    - Override `ToggleModifier`:
        - **If `cameraSwitcher.CurrentView == Main`**: Call `base.ToggleModifier(modifier)` to apply global/range effects.
        - **If `cameraSwitcher.CurrentView == Cannon1` or `Cannon2`**:
            - If `latestShotCannonball` exists, invoke targeted gravity/wind events on that object.
            - If no cannonball exists, do nothing.

## 4. Update Cannon Logic
- **File**: `Assets/Scripts/CannonScript.cs`
- **Changes**:
    - Remove `stateManager.isTargetedMode` logic.
    - Add a reference to `ShipGloveScript`.
    - When shooting, set `shipGlove.latestShotCannonball = ball`.

## 5. Update Event Listeners
- **Files**:
    - `Assets/Scripts/Modifiers/GravityProvider.cs`
    - `Assets/Scripts/Modifiers/WindProvider.cs`
    - `Assets/Scripts/GravityModifier.cs`
    - `Assets/Scripts/WindModifier.cs`
    - `Assets/Scripts/ModifierTimer.cs`
- **Changes**:
    - Update event handler signatures to match new tuple format.
    - Logic:
        - If `target != null`: Apply only if `gameObject == target`.
        - If `target == null`: Fall back to `OH_Helpers.isInRangeNoHeight` check.

## 6. Scene Integration
- Replace `GloveScript` with `ShipGloveScript` on the `PlayerArmature` (or target object) in the `Playground` and `Sea` scenes if appropriate.
- Wire the `ShipGloveScript` reference in `CannonScript` instances.

# Verification & Testing
1. **Shooting**: Verify cannons only shoot when active camera matches.
2. **Targeted Ability**: 
    - Shoot a cannonball.
    - Open UI wheel.
    - Change gravity/wind.
    - Observe that **only** the cannonball reacts.
    - Observe that the ship and other objects remain unaffected.
3. **Global Ability (Regression)**:
    - Ensure regular `GloveScript` (if used elsewhere) still works with range-based logic (passing `null` target).
4. **Lifecycle**: Ensure cannonball destruction doesn't cause null references in `ShipGloveScript`.
