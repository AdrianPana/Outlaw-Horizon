using UnityEngine;

public class SavingUIController : MonoBehaviour
{
    public UniversalStateManagerScriptableObject universalStateManagerScriptableObject;

    public bool isSaving = false;

    private Animator anim;

    private void Start()
    {
        anim = GetComponent<Animator>();
    }

    private void OnEnable()
    {
        universalStateManagerScriptableObject.savingStateEvent.AddListener(HandleSavingStateChanged);
    }

    private void OnDisable()
    {
        universalStateManagerScriptableObject.savingStateEvent.RemoveListener(HandleSavingStateChanged);
    }

    private void HandleSavingStateChanged(bool savingState)
    {
        Debug.Log("Received " + savingState);
        isSaving = savingState;
        anim.SetBool("SavingInProgress", isSaving);
    }
}
