using UnityEngine;

public class ResettableModifierObject : MonoBehaviour, IResettable
{
    private Snapshot snapshot;
    public void SaveSnapshot()
    {
        snapshot = new Snapshot
        {
            position = transform.localPosition,
            rotation = transform.localRotation
        };
    }

    public void Reset()
    {
        transform.SetLocalPositionAndRotation(snapshot.position, snapshot.rotation);
        if (TryGetComponent<Rigidbody>(out var rb))
        {
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
        }
        if (TryGetComponent<Animator>(out var anim))
        {
            anim.Rebind();
            anim.Update(0f);
        }
    }
}
