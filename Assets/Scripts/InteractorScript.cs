using UnityEngine;
using UnityEngine.InputSystem;
using Game.Resources;

public class InteractorScript : MonoBehaviour
{
    public UniversalStateManagerScriptableObject universalStateManagerScriptableObject;
    public Modifier modifier;
    [SerializeField] GameObject rangeIndicator;
    public float range = 3.0f;
    [SerializeField] Material[] materials = new Material[2];

    private void Start()
    {

    }

    private void OnValidate()
    {
        if (rangeIndicator != null)
        {
            rangeIndicator.transform.localScale = new Vector3(range, 0.025f, range);
        }

        switch (modifier)
        {
            case Modifier.WIND_NORTH:
            case Modifier.WIND_EAST:
            case Modifier.WIND_SOUTH:
            case Modifier.WIND_WEST:
                transform.rotation = Quaternion.Euler(0, 90 * ((int)ResourceHelper.ModifierToWindDirection(modifier) - 1), 0);
                break;
            default:
                break;
        }
    }

    public void ToggleModifier()
    {
        switch(modifier)
        {
            case Modifier.WIND_NORTH:
            case Modifier.WIND_EAST:
            case Modifier.WIND_SOUTH:
            case Modifier.WIND_WEST:
                universalStateManagerScriptableObject.ToggleWind(transform.position, range, ResourceHelper.ModifierToWindDirection(modifier));
                break;
            default:
                universalStateManagerScriptableObject.ToggleGravity(transform.position, range);
                break;
        }
    }
}
