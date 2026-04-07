using UnityEngine;

public class Level3Script : MonoBehaviour
{
    public UniversalStateManagerScriptableObject stateManager;

    private void Start()
    {
        InvokeRepeating(nameof(ToggleWind), 3f, 5f);
    }

    private void ToggleWind()
    {
        stateManager.ToggleWind(Vector3.zero, -1, Game.Resources.WindDirection.SOUTH);
    }
}