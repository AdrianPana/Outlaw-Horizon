using Game.Resources;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class WheelButtonController : MonoBehaviour
{
    private Animator anim;
    public Modifier modifier;
    public UniversalStateManagerScriptableObject universalStateManagerScriptableObject;
    
    private float alphaThreshold = 0.1f;

    void Start()
    {
        anim = GetComponent<Animator>();
        this.GetComponent<Image>().alphaHitTestMinimumThreshold = alphaThreshold;
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
