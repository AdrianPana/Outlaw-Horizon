using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Collider))]
public class SectionManager : MonoBehaviour
{
    private List<IResettable> resettableObjects = new List<IResettable>();
    public Vector3 respawnPoint { get; private set; }
    public Vector3 respawnRotation { get; private set; }

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
        if (other.CompareTag("Player"))
        {
            SaveSnapshot();
            SetSectionAsCurrent(other.gameObject.GetComponent<GloveScript>());
        }
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
        foreach (var resettable in resettableObjects)
        {
            resettable.Reset();
        }
    }

    private void SetSectionAsCurrent(GloveScript playerScript)
    {
        playerScript.SetCurrentSection(this);
    }
}
