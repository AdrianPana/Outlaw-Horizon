using Game.Modifiers;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(ModifierShader))]
[RequireComponent(typeof(ResettableModifierObject))]
[RequireComponent(typeof(Collider))]
public class AbstractModifierObject : MonoBehaviour
{
    protected List<IModifierProvider> providers = new List<IModifierProvider>();

    public virtual void Awake()
    {
        providers.AddRange(GetComponents<IModifierProvider>());
        //gameObject.layer = LayerMask.NameToLayer("Modifiable");
    }

    public virtual bool CheckIfInfluenced()
    {
        foreach (var provider in providers)
        {
            if (provider.IsActiveOnObject(transform.position)) return true;
        }
        return false;
    }
}
