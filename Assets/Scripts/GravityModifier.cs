using Game.Resources;
using UnityEngine;

public class GravityModifier : MonoBehaviour
{
    private Rigidbody rb;

    public bool gravityInverted;
    public float gravityStrength = Physics.gravity.magnitude / 2;
    private Vector3 currentVelocity;
    public LayerMask obstacleMask;
    public float speed = 3f;
    public float acceleration = 6f;

    [SerializeField]
    private UniversalStateManagerScriptableObject universalStateManagerScriptableObject;


    private void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.useGravity = false; // disable Unity's built-in gravity

        gravityInverted = false;
    }

    private void OnEnable()
    {
        universalStateManagerScriptableObject.gravityInvertedEvent.AddListener(ToggleObjectGravity);
    }

    private void OnDisable()
    {
        universalStateManagerScriptableObject.gravityInvertedEvent.RemoveListener(ToggleObjectGravity);
    }

    private void ToggleObjectGravity((Vector3 origin, float range, bool inverted, GameObject target) data)
    {
        if (data.target != null)
        {
            if (this.gameObject == data.target)
            {
                gravityInverted = data.inverted;
            }
        }
        else if (OH_Helpers.isInRangeNoHeight(transform.position, data.origin, data.range))
        {
            gravityInverted = data.inverted;
        }
    }

    private void FixedUpdate()
    {
        Vector3 gravityVector = GetGravityVector();
        ApplyControlledGravity(gravityVector);
    }

    private void ApplyControlledGravity(Vector3 gravityVector)
    {
        Vector3 targetVelocity = gravityVector * speed;

        currentVelocity = Vector3.Lerp(
            currentVelocity,
            targetVelocity,
            acceleration * Time.fixedDeltaTime
        );

        MoveWithCollision(currentVelocity * Time.fixedDeltaTime);
    }

    private void MoveWithCollision(Vector3 delta)
    {
        Debug.Log(rb.position);

        // resolve any existing penetration first
        Collider[] overlaps = Physics.OverlapBox(
            rb.position,
            GetHalfExtents(),
            transform.rotation,
            obstacleMask,
            QueryTriggerInteraction.Ignore);

        foreach (Collider overlap in overlaps)
        {
            if (overlap.gameObject == gameObject) continue;

            if (Physics.ComputePenetration(
                GetComponent<Collider>(), rb.position, transform.rotation,
                overlap, overlap.transform.position, overlap.transform.rotation,
                out Vector3 separationDir, out float separationDist))
            {
                rb.MovePosition(rb.position + separationDir * (separationDist + 0.01f));
            }
        }

        if (delta == Vector3.zero)
            return;

        float distance = delta.magnitude;
        Vector3 dir = delta.normalized;

        if (Physics.BoxCast(
            rb.position,
            GetHalfExtents(),
            dir,
            out RaycastHit hit,
            transform.rotation,
            distance,
            obstacleMask,
            QueryTriggerInteraction.Ignore))
        {
            float safeDistance = Mathf.Max(0f, hit.distance - 0.01f);
            rb.MovePosition(rb.position + dir * safeDistance);
            currentVelocity = Vector3.zero;
        }
        else
        {
            rb.MovePosition(rb.position + delta);
        }
    }

    private Vector3 GetHalfExtents()
    {
        Collider col = GetComponent<Collider>();
        return col.bounds.extents * 1.5f;
    }

    private Vector3 GetGravityVector()
    {
        switch (gravityInverted)
        {
            case true:
                return Vector3.up * gravityStrength;
            default:
                return Vector3.down * gravityStrength;
        }
    }
}
