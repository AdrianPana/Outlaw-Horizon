using UnityEngine;

public class ResettableModifierObject : MonoBehaviour, IResettable
{
    private Snapshot snapshot;
    public void SaveSnapshot()
    {
        snapshot = new Snapshot
        {
            position = transform.position
        };
    }

    public void Reset()
    {
        transform.position = snapshot.position;
        if (TryGetComponent<Rigidbody>(out var rb))
        {
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
        }
    }
}
