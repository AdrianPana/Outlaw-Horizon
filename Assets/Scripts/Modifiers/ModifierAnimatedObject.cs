using System.Collections.Generic;
using Game.Modifiers;
using Game.Resources;
using UnityEngine;

[RequireComponent(typeof(Animator))]
public class ModifierAnimatedObject : AbstractModifierObject
{
    // Can be extended with multiple modifiers/states
    private Animator animator;
    public Modifier requiredModifier = Modifier.NONE;
    public float gaugeSeconds;
    private float _influencedSeconds;
    private bool _hasAnimated;

    void Start()
    {
        animator = GetComponent<Animator>();
        _hasAnimated = false;
    }

    void Update()
    {
        if (_hasAnimated)
            return;

        if (CheckIfInfluenced()) 
        {
            Debug.Log("Influenced by " + requiredModifier.ToString());
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

    public override bool CheckIfInfluenced()
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
