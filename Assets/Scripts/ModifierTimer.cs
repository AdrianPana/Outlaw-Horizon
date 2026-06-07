using UnityEngine;
using System.Collections;
using Game.Resources;

public class ModifierTimer : MonoBehaviour
{
    [SerializeField] private UniversalStateManagerScriptableObject universalStateManager;

    private Coroutine activeGravityRoutine;
    private Coroutine activeWindRoutine;

    private void OnEnable()
    {
        universalStateManager.gravityInvertedEvent.AddListener(OnGravityTriggered);
        universalStateManager.windChangedEvent.AddListener(OnWindTriggered);
    }

    private void OnDisable()
    {
        universalStateManager.gravityInvertedEvent.RemoveListener(OnGravityTriggered);
        universalStateManager.windChangedEvent.RemoveListener(OnWindTriggered);
    }

    private void OnGravityTriggered((Vector3, float, bool) data)
    {
        if (data.Item3)
            StartGravityTimer();
        else
            StopGravityTimer();
    }

    private void OnWindTriggered((Vector3, float, WindDirection) data)
    {
        if (data.Item3 != WindDirection.NONE)
            StartWindTimer();
        else
            StopWindTimer();
    }

    private void StartGravityTimer()
    {
        if (activeGravityRoutine != null)
            StopCoroutine(activeGravityRoutine);
        activeGravityRoutine = StartCoroutine(ClearGravityAfterTime());
    }

    private void StopGravityTimer()
    {
        if (activeGravityRoutine == null) return;
        StopCoroutine(activeGravityRoutine);
        activeGravityRoutine = null;
    }

    private void StartWindTimer()
    {
        if (activeWindRoutine != null)
            StopCoroutine(activeWindRoutine);
        activeWindRoutine = StartCoroutine(ClearWindAfterTime());
    }

    private void StopWindTimer()
    {
        if (activeWindRoutine == null) return;
        StopCoroutine(activeWindRoutine);
        activeWindRoutine = null;
    }

    private IEnumerator ClearGravityAfterTime()
    {
        yield return new WaitForSeconds(universalStateManager.modifierDurationSeconds);
        universalStateManager.ClearGravityModifier(Vector3.zero, -1);
        activeGravityRoutine = null;
    }

    private IEnumerator ClearWindAfterTime()
    {
        yield return new WaitForSeconds(universalStateManager.modifierDurationSeconds);
        universalStateManager.ClearWindModifier(Vector3.zero, -1);
        activeWindRoutine = null;
    }
}