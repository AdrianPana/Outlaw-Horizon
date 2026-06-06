# Project Overview
- Game Title: Outlaw Horizon
- High-Level Concept: Puzzle-platformer with environmental manipulation.
- Refactor Goal: Create a robust, hybrid physics system for objects affected by Gravity and Wind.

# Game Mechanics Refinement

## 1. Behavior Types (Ambient vs. Controlled)
Objects will have a `BehaviorType` setting to define how they respond to modifiers:
- **Controlled (e.g., Large Crates, Platforms):**
    - **Purpose:** Used for precision puzzles and player transport.
    - **Physics:** Switches to `isKinematic = true` when influenced by a modifier.
    - **Constraints:** `freezeRotation` is enabled to ensure a flat, rideable surface.
    - **Movement:** Uses `rb.MovePosition` with `SweepTest` collision detection.
- **Ambient (e.g., Books, Debris, Cannonballs):**
    - **Purpose:** Visual flavor and physics-heavy interactions.
    - **Physics:** Always `isKinematic = false` (Dynamic).
    - **Constraints:** Free rotation allowed.
    - **Movement:** Receives forces via `rb.AddForce` and `rb.AddTorque`.

## 2. Momentum Preservation (The "Cannonball" Rule)
To ensure objects don't lose their speed when a modifier activates:
- **Transition to Active (Controlled):** The script captures the current `rb.velocity` and stores it in a `momentumVelocity` variable.
- **Kinematic Simulation:** While active, the object moves by `TotalVelocity = momentumVelocity + modifierVelocity`. The `momentumVelocity` is gradually reduced by a configurable drag value.
- **Transition to Passive:** When the modifier ends, the final `TotalVelocity` is applied back to `rb.velocity`, allowing the object to "fly" naturally out of the controlled state.

## 3. Extendable Design
The system will use a **Provider Pattern**:
- **Core Script:** `ModifierAffectedObject.cs` (Handles state, momentum, and movement).
- **Interface:** `IModifierProvider` (New modifiers like "Magnetism" or "Time-Dilation" can be added without touching the core script).
- **Current Providers:** `GravityProvider.cs` and `WindProvider.cs` will implement the interface.

## 4. Jitter-Free Riding
- **Interpolation:** `rb.interpolation` will be set to `Interpolate` on all affected objects.
- **Consistency:** All kinematic movement will occur in `FixedUpdate` using `rb.MovePosition`, which is the correct way to update "Moving Platforms" so that the player’s physics (Grounded/Parenting) stays stable.

# Implementation Steps

## Step 1: Interface and Providers
- Define `IModifierProvider` interface.
- Refactor Gravity and Wind logic into separate "Provider" components.

## Step 2: The Core Controller (`ModifierAffectedObject`)
- Implement the `BehaviorType` enum.
- Implement momentum capture and drag simulation.
- Implement the hybrid `FixedUpdate` (Dynamic vs. Kinematic paths).

## Step 3: Collision & Clamping
- Use `rb.SweepTest` in the `Controlled` path to ensure objects never clip through walls or floors.

## Step 4: Asset Setup
- Replace existing `GravityModifier` scripts with `ModifierAffectedObject`.
- Set crates to **Controlled** and small objects/cannonballs to **Ambient**.

# Verification & Testing
1. **The Cannonball Test:** Verify a cannonball shot from a cannon keeps its forward arc while gravity is inverted.
2. **The Crate Ride Test:** Stand on a kinematic crate while it moves. Verify the player does not jitter or fall through.
3. **The Precision Test:** Verify a crate stops exactly at a wall when blown by wind.
