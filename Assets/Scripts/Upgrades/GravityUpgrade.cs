using UnityEngine;

[CreateAssetMenu(fileName = "GravityUpgrade", menuName = "Scriptable Objects/GravityUpgrade")]
public class GravityUpgrade : Upgrade, IModifierUpgrade
{
    public Sprite Icon => icon;
    [SerializeField] Sprite icon;

    public override void ApplyUpgrade(GameObject player)
    {
        if (player.TryGetComponent<GloveScript>(out var gloveScript))
        {
            gloveScript.hasGravityAbility = true;
        }
    }

    public override void RemoveUpgrade(GameObject player)
    {
        if (player.TryGetComponent<GloveScript>(out var gloveScript))
        {
            gloveScript.hasGravityAbility = false;
        }
    }
}
