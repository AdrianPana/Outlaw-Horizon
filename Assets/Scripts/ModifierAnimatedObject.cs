using System.Collections.Generic;
using Game.Modifiers;
using UnityEngine;
using static ModifierAffectedObject;

public class ModifierAnimatedObject : MonoBehaviour
{
    public Animator animator;
    public float gaugeSeconds;
    private float _influencedSeconds;
    private bool _hasAnimated;

    //protected Rigidbody rb;
    private List<IModifierProvider> providers = new List<IModifierProvider>();

    public virtual void Awake()
    {
        //rb = GetComponent<Rigidbody>();
        //rb.interpolation = RigidbodyInterpolation.Interpolate;

        providers.AddRange(GetComponents<IModifierProvider>());

        //rb.isKinematic = true;
        //rb.useGravity = false;
        //rb.freezeRotation = true;
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
            if (provider.IsActiveOnObject(transform.position)) return true;
        }
        return false;
    }

    private void Animate()
    {
        _hasAnimated = true;
    }
}
