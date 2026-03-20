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
    }
}
