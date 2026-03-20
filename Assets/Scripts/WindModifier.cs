using UnityEngine;
using Game.Resources;
using Unity.VisualScripting;

[RequireComponent(typeof(ResettableModifierObject))]
public class WindModifier : MonoBehaviour
{
    private Rigidbody rb;

    public WindDirection windDirection;
    private float windStrength;
    private Vector3 currentVelocity;
    public LayerMask obstacleMask;
    public float speed = 3f;
    public float acceleration = 6f;


    public enum WindObjectType
    {
        Controlled,
        Ambient
    }
    public WindObjectType windObjectType = WindObjectType.Controlled;
    
    [Header("Ambient settings")]
    public float turbulence = 2f;
    public float torqueStrength = 5f;

    [SerializeField]
    private UniversalStateManagerScriptableObject universalStateManagerScriptableObject;

    private void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.isKinematic = windObjectType == WindObjectType.Controlled; // only controlled objects are kinematic
        rb.interpolation = RigidbodyInterpolation.Interpolate; // smooth movement for both types
        rb.useGravity = windObjectType == WindObjectType.Ambient; // disable Unity's built-in gravity
        windDirection = WindDirection.NONE;
        rb.freezeRotation = windObjectType == WindObjectType.Controlled; // only freeze rotation for controlled objects

        if (windObjectType == WindObjectType.Ambient)
        {
            windStrength = 10f;
        }
        else
        {
            windStrength = 1f;
        }
    }

    private void OnEnable()
    {
        universalStateManagerScriptableObject.windChangedEvent.AddListener(ChangeWindDirection);
    }

    private void OnDisable()
    {
        universalStateManagerScriptableObject.windChangedEvent.RemoveListener(ChangeWindDirection);
    }

    private void ChangeWindDirection((Vector3, float, WindDirection) data)
    {
        if (OH_Helpers.isInRangeNoHeight(transform.position, data.Item1, data.Item2)) {
            windDirection = data.Item3;
        }
    }

    private void FixedUpdate()
    {
        Vector3 windVector = GetWindVector();
        //Debug.Log($"Applying wind: {windVector} to {gameObject.name} of type {windObjectType}");
        if (windObjectType == WindObjectType.Controlled)
        {
            ApplyControlledWind(windVector);
        }
        else
        {
            ApplyAmbientWind(windVector);
        }
    }

    private void ApplyControlledWind(Vector3 windVector)
    {

        Vector3 targetVelocity = windVector * speed;

        currentVelocity = Vector3.Lerp(
            currentVelocity,
            targetVelocity,
            acceleration * Time.fixedDeltaTime
        );

        MoveWithCollision(currentVelocity * Time.fixedDeltaTime);
    }

    private void MoveWithCollision(Vector3 delta)
    {
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
            rb.MovePosition(rb.position + dir * hit.distance);
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
        return col.bounds.extents * 0.95f;
    }

    private void ApplyAmbientWind(Vector3 windVector)
    {
        rb.AddForce(windVector, ForceMode.Acceleration);

        Vector3 offset = transform.forward * 0.3f; // sau orice axa locala
        rb.AddForceAtPosition(windVector.magnitude * 1.0f * Vector3.up, transform.position, ForceMode.Acceleration);


        // variatie aleatoare (turbulenta)
        Vector3 random = Random.insideUnitSphere * turbulence;
        rb.AddForce(random, ForceMode.Acceleration);

        Vector3 forward = transform.forward;
        float alignment = Mathf.Abs(Vector3.Dot(forward.normalized, windVector.normalized));

        Vector3 torqueAxis = Vector3.Cross(forward, windVector).normalized;
        rb.AddTorque(torqueAxis * alignment * torqueStrength, ForceMode.Acceleration);
    }

    private Vector3 GetWindVector()
    {
        switch (windDirection)
        {
            case WindDirection.NORTH:
                return Vector3.forward * windStrength;
            case WindDirection.EAST:
                return Vector3.right * windStrength;
            case WindDirection.SOUTH:
                return Vector3.back * windStrength;
            case WindDirection.WEST:
                return Vector3.left * windStrength;
            default:
                return Vector3.zero;
        }
    }
}
