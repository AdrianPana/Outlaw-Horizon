using UnityEngine;
using Game.Resources;

public class ShipGloveScript : GloveScript
{
    public GameObject latestShotCannonball;
    public CameraSwitcher cameraSwitcher;

    public override void ToggleModifier(Modifier modifier)
{
        if (cameraSwitcher != null && cameraSwitcher.CurrentView == CameraSwitcher.ViewMode.Main)
        {
            base.ToggleModifier(modifier);
            return;
        }

        if (latestShotCannonball == null)
        {
            return;
        }

        switch (modifier)
        {
            case Modifier.GRAVITY_INVERTED:
                universalStateManagerScriptableObject.ToggleGravity(latestShotCannonball.transform.position, range, latestShotCannonball);
                break;
            case Modifier.NONE:
                universalStateManagerScriptableObject.ToggleWind(latestShotCannonball.transform.position, range, WindDirection.NONE, latestShotCannonball);
                break;
            default:
                universalStateManagerScriptableObject.ToggleWind(latestShotCannonball.transform.position, range, ResourceHelper.ModifierToWindDirection(modifier), latestShotCannonball);
                break;
        }
    }
}
