using UnityEngine;

public class ActivatableGate : MonoBehaviour, IActivatable
{
    public Vector3 activatedDisplacement;
    public float activationDuration = 1.0f;

    private Vector3 originalPosition;
    private Vector3 activatedPosition;
    private float activationSpeed;

    private bool isActive = false;

    public void Start()
    {
        originalPosition = transform.position;
        activatedPosition = originalPosition + activatedDisplacement;
        activationSpeed = Vector3.Distance(originalPosition, activatedPosition) / activationDuration;
    }

    public void Update()
    {
        if (isActive)
        {
            transform.position = Vector3.MoveTowards(transform.position, activatedPosition, activationSpeed * Time.deltaTime);
        }
        else
        {
            transform.position = Vector3.MoveTowards(transform.position, originalPosition, activationSpeed * Time.deltaTime);
        }
    }

    public void Activate()
    {
        isActive = true;
    }

    public void Deactivate()
    {
        isActive = false;
    }
}
