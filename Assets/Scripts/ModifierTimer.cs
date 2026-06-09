using UnityEngine;
using System.Collections;
using Game.Resources;

public class ModifierTimer : MonoBehaviour
{
    [SerializeField] private UniversalStateManagerScriptableObject universalStateManager;
    public bool oceanScene = false;

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

    private void OnGravityTriggered((Vector3 origin, float range, bool inverted, GameObject target) data)
    {
        if (data.inverted)
            StartGravityTimer(data.target);
        else
            StopGravityTimer();
    }

    private void OnWindTriggered((Vector3 origin, float range, WindDirection direction, GameObject target) data)
    {
        if (oceanScene) return; // Wind modifiers don't apply in the ocean scene, so we don't need to start a timer for them
        if (data.direction != WindDirection.NONE)
            StartWindTimer(data.target);
        else
            StopWindTimer();
    }

    private void StartGravityTimer(GameObject target = null)
    {
        if (activeGravityRoutine != null)
            StopCoroutine(activeGravityRoutine);
        activeGravityRoutine = StartCoroutine(ClearGravityAfterTime(target));
    }

    private void StopGravityTimer()
    {
        if (activeGravityRoutine == null) return;
        StopCoroutine(activeGravityRoutine);
        activeGravityRoutine = null;
    }

    private void StartWindTimer(GameObject target = null)
    {
        if (activeWindRoutine != null)
            StopCoroutine(activeWindRoutine);
        activeWindRoutine = StartCoroutine(ClearWindAfterTime(target));
    }

    private void StopWindTimer()
    {
        if (activeWindRoutine == null) return;
        StopCoroutine(activeWindRoutine);
        activeWindRoutine = null;
    }

    private IEnumerator ClearGravityAfterTime(GameObject target = null)
    {
        yield return new WaitForSeconds(universalStateManager.modifierDurationSeconds);
        universalStateManager.ClearGravityModifier(Vector3.zero, -1, target);
        activeGravityRoutine = null;
    }

    private IEnumerator ClearWindAfterTime(GameObject target = null)
    {
        yield return new WaitForSeconds(universalStateManager.modifierDurationSeconds);
        universalStateManager.ClearWindModifier(Vector3.zero, -1, target);
        activeWindRoutine = null;
    }
}