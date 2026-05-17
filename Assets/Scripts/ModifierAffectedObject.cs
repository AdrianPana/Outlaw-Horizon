using UnityEngine;
using System.Collections.Generic;
using Game.Modifiers;

[RequireComponent(typeof(Rigidbody))]
public class ModifierAffectedObject : MonoBehaviour
{
    public enum BehaviorType { Controlled, Ambient }

    [Header("Settings")]
    public BehaviorType behaviorType = BehaviorType.Controlled;
    public LayerMask obstacleMask;
    public float speed = 3f;
    public float acceleration = 6f;
    public float kinematicDrag = 2f;
    public float passiveGravityStrength = 9.81f;

    private Rigidbody rb;
    private List<IModifierProvider> providers = new List<IModifierProvider>();
    private Vector3 momentumVelocity;
    private Vector3 currentModifierVelocity;
    private Vector3 passiveGravityVelocity;
    private bool wasInfluenced;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        rb.interpolation = RigidbodyInterpolation.Interpolate;
        
        // Find all providers on this object
        providers.AddRange(GetComponents<IModifierProvider>());

        if (behaviorType == BehaviorType.Controlled)
        {
            rb.isKinematic = true;
            rb.useGravity = false;
            rb.freezeRotation = true;
        }
        else
        {
            rb.isKinematic = false;
            rb.useGravity = true;
            rb.freezeRotation = false;
        }
    }

    private void FixedUpdate()
    {
        bool isInfluenced = CheckIfInfluenced();

        if (behaviorType == BehaviorType.Controlled)
        {
            HandleControlledMovement(isInfluenced);
        }
        else
        {
            HandleAmbientMovement(isInfluenced);
        }

        wasInfluenced = isInfluenced;
    }

    private bool CheckIfInfluenced()
    {
        foreach (var provider in providers)
        {
            if (provider.IsActiveOnObject(transform.position)) return true;
        }
        return false;
    }

    private void HandleControlledMovement(bool isInfluenced)
    {
        // 1. Momentum and Drag
        if (isInfluenced && !wasInfluenced)
        {
            // If we were dynamic before (unlikely now but for robustness), capture it
            if (!rb.isKinematic)
            {
                momentumVelocity = rb.linearVelocity;
                rb.isKinematic = true;
                rb.useGravity = false;
            }
        }

        // Apply drag to captured momentum
        momentumVelocity = Vector3.Lerp(momentumVelocity, Vector3.zero, kinematicDrag * Time.fixedDeltaTime);

        // 2. Modifier Velocity
        Vector3 modifierForce = Vector3.zero;
        if (isInfluenced)
        {
            foreach (var provider in providers)
            {
                modifierForce += provider.GetForceContribution(transform.position);
            }
            passiveGravityVelocity = Vector3.zero; // Modifiers override passive gravity
        }
        else
        {
            // 3. Passive Gravity (Manual simulation to keep kinematic objects stable against player)
            // We only apply this if not influenced by modifiers
            passiveGravityVelocity += Vector3.down * passiveGravityStrength * Time.fixedDeltaTime;
        }

        Vector3 targetModifierVelocity = modifierForce * speed;
        currentModifierVelocity = Vector3.Lerp(currentModifierVelocity, targetModifierVelocity, acceleration * Time.fixedDeltaTime);

        // 4. Final Movement
        Vector3 totalVelocity = momentumVelocity + currentModifierVelocity + passiveGravityVelocity;
        MoveKinematicWithCollision(totalVelocity * Time.fixedDeltaTime);
    }

    private void HandleAmbientMovement(bool isInfluenced)
    {
        if (isInfluenced)
        {
            foreach (var provider in providers)
            {
                rb.AddForce(provider.GetForceContribution(transform.position), ForceMode.Acceleration);
            }
        }
    }

    private void MoveKinematicWithCollision(Vector3 delta)
    {
        if (delta == Vector3.zero) return;

        // Predictive collision detection using BoxCast (assuming BoxCollider for crates)
        BoxCollider box = GetComponent<BoxCollider>();
        if (box != null)
        {
            Vector3 center = transform.TransformPoint(box.center);
            Vector3 halfExtents = Vector3.Scale(box.size, transform.lossyScale) * 0.5f;

            if (Physics.BoxCast(center, halfExtents, delta.normalized, out RaycastHit hit, transform.rotation, delta.magnitude, obstacleMask, QueryTriggerInteraction.Ignore))
            {
                float safeDist = Mathf.Max(0, hit.distance - 0.01f);
                rb.MovePosition(rb.position + delta.normalized * safeDist);
                
                // Zero out velocity component hitting the wall
                Vector3 normal = hit.normal;
                currentModifierVelocity = Vector3.ProjectOnPlane(currentModifierVelocity, normal);
                momentumVelocity = Vector3.ProjectOnPlane(momentumVelocity, normal);
                passiveGravityVelocity = Vector3.ProjectOnPlane(passiveGravityVelocity, normal);
                
                return;
            }
        }

        rb.MovePosition(rb.position + delta);
    }
}

