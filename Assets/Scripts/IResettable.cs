using UnityEngine;

public interface IResettable
{
    public void SaveSnapshot();

    public void Reset();
}
