using UnityEngine;

namespace Game.Modifiers
{
    public interface IModifierProvider
    {
        Vector3 GetForceContribution(Vector3 currentPos);
        bool IsActiveOnObject(Vector3 currentPos);
    }
}
