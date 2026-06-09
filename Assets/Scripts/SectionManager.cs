using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Collider))]
public class SectionManager : MonoBehaviour
{
    private List<IResettable> resettableObjects = new List<IResettable>();
    public Vector3 respawnPoint { get; private set; }
    public Vector3 respawnRotation { get; private set; }

    public UniversalStateManagerScriptableObject universalStateManagerScriptableObject;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        foreach (var child in GetComponentsInChildren<IResettable>())
        {
            resettableObjects.Add(child);
        }

        respawnPoint = transform.TransformPoint(GetComponent<BoxCollider>().center);
        respawnRotation = transform.eulerAngles;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") && SetSectionAsCurrent(other.gameObject.GetComponent<GloveScript>()))
        {
            StartCoroutine(EnterSectionRoutine());
        }

    }

    IEnumerator EnterSectionRoutine()
    {
        universalStateManagerScriptableObject.SetSavingState(true);
        Debug.Log("Entered new section, saving state...");

        SaveSnapshot();

        yield return new WaitForSeconds(1.0f);

        universalStateManagerScriptableObject.SetSavingState(false);
    }

    private void SaveSnapshot()
    {
        foreach (var resettable in resettableObjects)
        {
            resettable.SaveSnapshot();
        }
    }

    public void ResetSection()
    {
        universalStateManagerScriptableObject.ClearAllModifiers(Vector3.zero, -1);
        foreach (var resettable in resettableObjects)
        {
            resettable.Reset();
        }
    }

    private bool SetSectionAsCurrent(GloveScript playerScript)
    {
        return playerScript.SetNewCurrentSection(this);
    }
}
