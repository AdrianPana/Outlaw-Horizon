using System.Collections.Generic;
using Game.Modifiers;
using Game.Resources;
using UnityEngine;
using static ModifierAffectedObject;

public class ModifierAnimatedObject : MonoBehaviour
{
    // Can be extended with multiple modifiers/states
    public Animator animator;
    public Modifier requiredModifier = Modifier.NONE;
    public float gaugeSeconds;
    private float _influencedSeconds;
    private bool _hasAnimated;

    private List<IModifierProvider> providers = new List<IModifierProvider>();

    public virtual void Awake()
    {
        providers.AddRange(GetComponents<IModifierProvider>());
    }

    void Start()
    {
        _hasAnimated = false;
    }

    void Update()
    {
        if (_hasAnimated)
            return;

        if (CheckIfInfluenced()) 
        {
            _influencedSeconds += Time.deltaTime;
            if (_influencedSeconds >= gaugeSeconds) {
                Animate();
            }
        }
        else
        {
            _influencedSeconds = 0;
        }
    }

    public bool CheckIfInfluenced()
    {
        foreach (var provider in providers)
        {
            if (provider.IsActiveOnObject(transform.position) &&
                provider.GetModifier() == requiredModifier) return true;
        }
        return false;
    }

    private void Animate()
    {
        _hasAnimated = true;
        animator.SetTrigger("Modify");
    }
}
