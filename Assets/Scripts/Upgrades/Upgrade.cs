using UnityEngine;
using Game.Resources;

[CreateAssetMenu(menuName = "Progression/Upgrade")]
public abstract class Upgrade : ScriptableObject
{
    public UpgradeType Type;
    public abstract void ApplyUpgrade(GameObject player);

    public abstract void RemoveUpgrade(GameObject player);
}
