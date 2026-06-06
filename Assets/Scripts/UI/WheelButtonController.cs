using Game.Resources;
using TMPro;
using UnityEngine;

public class WheelButtonController : MonoBehaviour
{
    private Animator anim;
    public Modifier modifier;
    public UniversalStateManagerScriptableObject universalStateManagerScriptableObject;

    void Start()
    {
        anim = GetComponent<Animator>();
    }

    public void HoverEnter()
    {
        anim.SetBool("Hover", true);
        universalStateManagerScriptableObject.SelectModifier(modifier);
    }

    public void HoverExit()
    {
        anim.SetBool("Hover", false);
        universalStateManagerScriptableObject.SelectModifier(Modifier.NONE);
    }
}
