using UnityEngine;

public class Button : MonoBehaviour
{
    public Material idleMaterial;
    public Material pressedMaterial;
    public GameObject activatableObject;

    private IActivatable activatable;
    private int objectsOnButton = 0;

    private void OnValidate()
    {
        if (activatableObject == null)
        {
            Debug.LogWarning("Activatable object is not assigned in the Button script.");
            return;
        }
        else if (!activatableObject.TryGetComponent<IActivatable>(out _))
        {
            Debug.LogWarning("The assigned activatable object does not implement the IActivatable interface.");
            return;
        }

        activatable = activatableObject.GetComponent<IActivatable>();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (objectsOnButton == 0)
        {
            GetComponent<MeshRenderer>().material = pressedMaterial;
            activatable.Activate();
        }
        objectsOnButton++;
    }

    private void OnTriggerExit(Collider other)
    {
        objectsOnButton--;
        if (objectsOnButton == 0)
        {
            GetComponent<MeshRenderer>().material = idleMaterial;
            activatable.Deactivate();
        }
    }
}
