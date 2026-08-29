using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class Rideable : MonoBehaviour
{
    private Rigidbody _rb;
    private ModifierAffectedObject _modifierObject;
    public Vector3 Velocity { get; private set; }

    private Vector3 _lastPosition;

    void Awake()
    {
        _rb = GetComponent<Rigidbody>();
        _modifierObject = GetComponent<ModifierAffectedObject>();

    }

    private void Start()
    {
        _lastPosition = _rb.position;
    }

    void FixedUpdate()
    {
        if (_modifierObject != null)
        {
            Velocity = _modifierObject.CurrentVelocity;
        }
        else { 
            Vector3 current = _rb.position;
            Velocity = (current - _lastPosition) / Time.fixedDeltaTime;
            _lastPosition = current;
        }
    }

    public void RegisterPassenger(Collider passengerCollider)
    {
        _modifierObject.RegisterPassenger(passengerCollider);
    }

    public void UnregisterPassenger(Collider passengerCollider)
    {
        _modifierObject.UnregisterPassenger(passengerCollider);
    }
}

