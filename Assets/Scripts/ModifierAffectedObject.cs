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

    protected Rigidbody rb;
    private Collider physicalCollider;
    private List<IModifierProvider> providers = new List<IModifierProvider>();
    private Vector3 momentumVelocity;
    private Vector3 currentModifierVelocity;
    private Vector3 passiveGravityVelocity;
    private bool wasInfluenced;

    public virtual void Awake()
    {
        rb = GetComponent<Rigidbody>();
        rb.interpolation = RigidbodyInterpolation.Interpolate;

        // Identify the physical (non-trigger) collider for collision detection
        foreach (var col in GetComponents<Collider>())
        {
            if (!col.isTrigger)
            {
                physicalCollider = col;
                break;
            }
        }

        providers.AddRange(GetComponents<IModifierProvider>());

        if (behaviorType == BehaviorType.Controlled)
        {
            rb.isKinematic = true;
            rb.useGravity = false;
            rb.freezeRotation = true;

            obstacleMask = ~LayerMask.GetMask("Modifiable");
        }
        else
        {
            rb.isKinematic = false;
            rb.useGravity = true;
            rb.freezeRotation = false;

            obstacleMask = ~0;
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
        if (float.IsNaN(momentumVelocity.x) || float.IsNaN(momentumVelocity.y) || float.IsNaN(momentumVelocity.z))
        {
            Debug.LogError($"[NaN] momentumVelocity is NaN on {name}", this);
            momentumVelocity = Vector3.zero;
        }
        if (float.IsNaN(currentModifierVelocity.x) || float.IsNaN(currentModifierVelocity.y) || float.IsNaN(currentModifierVelocity.z))
        {
            Debug.LogError($"[NaN] currentModifierVelocity is NaN on {name}", this);
            currentModifierVelocity = Vector3.zero;
        }
        if (float.IsNaN(passiveGravityVelocity.x) || float.IsNaN(passiveGravityVelocity.y) || float.IsNaN(passiveGravityVelocity.z))
        {
            Debug.LogError($"[NaN] passiveGravityVelocity is NaN on {name}", this);
            passiveGravityVelocity = Vector3.zero;
        }

        if (isInfluenced && !wasInfluenced)
        {
            if (!rb.isKinematic)
            {
                momentumVelocity = rb.linearVelocity;
                rb.isKinematic = true;
                rb.useGravity = false;
            }
        }

        momentumVelocity = Vector3.Lerp(momentumVelocity, Vector3.zero, kinematicDrag * Time.fixedDeltaTime);

        Vector3 modifierForce = Vector3.zero;
        if (isInfluenced)
        {
            foreach (var provider in providers)
            {
                Vector3 contribution = provider.GetForceContribution(transform.position);
                if (float.IsNaN(contribution.x) || float.IsNaN(contribution.y) || float.IsNaN(contribution.z))
                {
                    Debug.LogError($"[NaN] provider {provider} returned NaN force on {name}", this);
                    contribution = Vector3.zero;
                }
                modifierForce += contribution;
            }
            passiveGravityVelocity = Vector3.zero;
        }
        else
        {
            passiveGravityVelocity += Vector3.down * passiveGravityStrength * Time.fixedDeltaTime;
        }

        Vector3 targetModifierVelocity = modifierForce * speed;
        currentModifierVelocity = Vector3.Lerp(currentModifierVelocity, targetModifierVelocity, acceleration * Time.fixedDeltaTime);

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
        if (delta.sqrMagnitude < 0.00001f || physicalCollider == null)
        {
            if (delta.sqrMagnitude >= 0.00001f) rb.MovePosition(rb.position + delta);
            return;
        }

        if (rb.SweepTest(delta.normalized, out RaycastHit hit, delta.magnitude, QueryTriggerInteraction.Ignore))
        {
            // Check if it's a static obstacle (wall, floor, etc.)
            if (((1 << hit.collider.gameObject.layer) & obstacleMask) != 0)
            {
                float safeDist = Mathf.Max(0, hit.distance - 0.01f);
                rb.MovePosition(rb.position + delta.normalized * safeDist);

                Vector3 normal = hit.normal;
                currentModifierVelocity = Vector3.ProjectOnPlane(currentModifierVelocity, normal);
                momentumVelocity = Vector3.ProjectOnPlane(momentumVelocity, normal);
                passiveGravityVelocity = Vector3.ProjectOnPlane(passiveGravityVelocity, normal);
                return;
            }

            // Check if it's a pushable non-kinematic rigidbody (e.g. ambient crate)
            Rigidbody hitRb = hit.collider.attachedRigidbody;
            if (hitRb != null && !hitRb.isKinematic)
            {
                // Transfer velocity to the pushed object
                Vector3 pushVelocity = Vector3.Project(delta / Time.fixedDeltaTime, delta.normalized);
                hitRb.linearVelocity = Vector3.Lerp(hitRb.linearVelocity, pushVelocity, acceleration * Time.fixedDeltaTime);
            }
        }

        // In both the "no hit" and "pushable hit" cases, move fully
        rb.MovePosition(rb.position + delta);
    }
}