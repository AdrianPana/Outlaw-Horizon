using UnityEngine;
using System.Collections;
using Game.Resources;

public class ModifierTimer : MonoBehaviour
{
    [SerializeField] private UniversalStateManagerScriptableObject universalStateManager;

    private Coroutine activeRoutine;

    private void OnEnable()
    {
        universalStateManager.gravityInvertedEvent.AddListener(OnModifierTriggered);
        universalStateManager.windChangedEvent.AddListener(OnWindTriggered);
    }

    private void OnDisable()
    {
        universalStateManager.gravityInvertedEvent.RemoveListener(OnModifierTriggered);
        universalStateManager.windChangedEvent.RemoveListener(OnWindTriggered);
    }

    private void OnModifierTriggered((Vector3, float, bool) data)
    {
        if (data.Item3)
            StartTimer();
    }

    private void OnWindTriggered((Vector3, float, WindDirection) data)
    {
        if (data.Item3 != WindDirection.NONE)
            StartTimer();
    }

    private void StartTimer()
    {
        if (activeRoutine != null)
            StopCoroutine(activeRoutine);

        activeRoutine = StartCoroutine(ClearAfterTime());
    }

    private IEnumerator ClearAfterTime()
    {
        yield return new WaitForSeconds(universalStateManager.modifierDurationSeconds);
        universalStateManager.ClearModifier(Vector3.zero, -1);
    }
}
