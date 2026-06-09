using UnityEngine;
using UnityEngine.Events;
using System;
using System.Collections;
using Game.Resources;

[CreateAssetMenu(fileName = "UniversalStateManagerScriptableObject", menuName = "Scriptable Objects/Universal State Manager")]
public class UniversalStateManagerScriptableObject : ScriptableObject
{
    // TODO: Replace with a HashSet<Modifier> if multiple simultaneous modifiers are needed
    public Modifier currentGravityModifier;
    public Modifier currentWindModifier;

    public float modifierDurationSeconds;
    
    [System.NonSerialized] public UnityEvent<(Vector3 origin, float range, bool inverted, GameObject target)> gravityInvertedEvent;
    [System.NonSerialized] public UnityEvent<(Vector3 origin, float range, WindDirection direction, GameObject target)> windChangedEvent;
    [System.NonSerialized] public UnityEvent<Modifier> modifierButtonSelectedEvent;
    [System.NonSerialized] public UnityEvent<bool> savingStateEvent;

    private void OnEnable()
    {
        currentGravityModifier = Modifier.NONE;
        currentWindModifier = Modifier.NONE;

        if (gravityInvertedEvent == null)
            gravityInvertedEvent = new UnityEvent<(Vector3, float, bool, GameObject)>();
        if (windChangedEvent == null)
            windChangedEvent = new UnityEvent<(Vector3, float, WindDirection, GameObject)>();
        if (modifierButtonSelectedEvent == null)
            modifierButtonSelectedEvent = new UnityEvent<Modifier>();
        if (savingStateEvent == null)
            savingStateEvent = new UnityEvent<bool>();
    }

    public void ToggleGravity(Vector3 originPosition, float range, GameObject target = null)
    {
        if (currentGravityModifier != Modifier.GRAVITY_INVERTED)
        {
            currentGravityModifier = Modifier.GRAVITY_INVERTED;
            gravityInvertedEvent.Invoke((originPosition, range, true, target));
        }
        else
        {
            currentGravityModifier = Modifier.NONE;
            gravityInvertedEvent.Invoke((originPosition, range, false, target));
        }
    }

    public void ToggleWind(Vector3 originPosition, float range, WindDirection direction, GameObject target = null)
    {
        currentWindModifier = ResourceHelper.WindDirectionToModifier(direction);
        windChangedEvent.Invoke((originPosition, range, direction, target));
    }

    public void ClearGravityModifier(Vector3 originPosition, float range, GameObject target = null)
    {
        currentGravityModifier = Modifier.NONE;
        gravityInvertedEvent.Invoke((originPosition, range, false, target));
    }

    public void ClearWindModifier(Vector3 originPosition, float range, GameObject target = null)
    {
        currentWindModifier = Modifier.NONE;
        windChangedEvent.Invoke((originPosition, range, WindDirection.NONE, target));
    }

    public void ClearAllModifiers(Vector3 originPosition, float range, GameObject target = null)
    {
        ClearGravityModifier(originPosition, range, target);
        ClearWindModifier(originPosition, range, target);
    }

    public void SelectModifier(Modifier modifier)
    {
        modifierButtonSelectedEvent.Invoke(modifier);
    }

    public void SetSavingState(bool isSaving)
    {
        savingStateEvent.Invoke(isSaving);
    }
}