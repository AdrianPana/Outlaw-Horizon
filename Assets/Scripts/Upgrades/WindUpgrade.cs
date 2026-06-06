using UnityEngine;

[CreateAssetMenu(fileName = "WindUpgrade", menuName = "Scriptable Objects/WindUpgrade")]
public class WindUpgrade : Upgrade, IModifierUpgrade
{
    public Sprite Icon => icon;
    [SerializeField] Sprite icon;

    public override void ApplyUpgrade(GameObject player)
    {
        if (player.TryGetComponent<GloveScript>(out var gloveScript))
        {
            gloveScript.hasWindAbility = true;
        }
    }

    public override void RemoveUpgrade(GameObject player)
    {
        if (player.TryGetComponent<GloveScript>(out var gloveScript))
        {
            gloveScript.hasWindAbility = false;
        }
    }
}
