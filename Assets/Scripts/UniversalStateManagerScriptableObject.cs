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

    [System.NonSerialized] public UnityEvent<(Vector3, float, bool)> gravityInvertedEvent;
    [System.NonSerialized] public UnityEvent<(Vector3, float, WindDirection)> windChangedEvent;
    [System.NonSerialized] public UnityEvent<Modifier> modifierButtonSelectedEvent;
    [System.NonSerialized] public UnityEvent<bool> savingStateEvent;

    private void OnEnable()
    {
        currentGravityModifier = Modifier.NONE;
        currentWindModifier = Modifier.NONE;

        if (gravityInvertedEvent == null)
            gravityInvertedEvent = new UnityEvent<(Vector3, float, bool)>();
        if (windChangedEvent == null)
            windChangedEvent = new UnityEvent<(Vector3, float, WindDirection)>();
        if (modifierButtonSelectedEvent == null)
            modifierButtonSelectedEvent = new UnityEvent<Modifier>();
        if (savingStateEvent == null)
            savingStateEvent = new UnityEvent<bool>();
    }

    public void ToggleGravity(Vector3 originPosition, float range)
    {
        if (currentGravityModifier != Modifier.GRAVITY_INVERTED)
        {
            currentGravityModifier = Modifier.GRAVITY_INVERTED;
            gravityInvertedEvent.Invoke((originPosition, range, true));
        }
        else
        {
            currentGravityModifier = Modifier.NONE;
            gravityInvertedEvent.Invoke((originPosition, range, false));
        }
    }

    public void ToggleWind(Vector3 originPosition, float range, WindDirection direction)
    {
        currentWindModifier = ResourceHelper.WindDirectionToModifier(direction);
        windChangedEvent.Invoke((originPosition, range, direction));
    }

    public void ClearGravityModifier(Vector3 originPosition, float range)
    {
        currentGravityModifier = Modifier.NONE;
        gravityInvertedEvent.Invoke((originPosition, range, false));
    }

    public void ClearWindModifier(Vector3 originPosition, float range)
    {
        currentWindModifier = Modifier.NONE;
        windChangedEvent.Invoke((originPosition, range, WindDirection.NONE));
    }

    public void ClearAllModifiers(Vector3 originPosition, float range)
    {
        ClearGravityModifier(originPosition, range);
        ClearWindModifier(originPosition, range);
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