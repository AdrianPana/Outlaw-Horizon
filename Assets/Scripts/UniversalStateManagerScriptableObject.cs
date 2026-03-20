using UnityEngine;
using UnityEngine.Events;
using System;
using System.Collections;
using Game.Resources;

[CreateAssetMenu(fileName = "UniversalStateManagerScriptableObject", menuName = "Scriptable Objects/Universal State Manager")]
public class UniversalStateManagerScriptableObject : ScriptableObject
{
    public Modifier currentModifier;
    public float modifierDurationSeconds;

    [System.NonSerialized]
    public UnityEvent<(Vector3, float, bool)> gravityInvertedEvent;
    [System.NonSerialized]
    public UnityEvent<(Vector3, float, WindDirection)> windChangedEvent;
    [System.NonSerialized]
    public UnityEvent<Modifier> modifierButtonSelectedEvent;
    [System.NonSerialized]
    public UnityEvent<bool> savingStateEvent;

    private void OnEnable()
    {
        currentModifier = Modifier.NONE;
        if (gravityInvertedEvent == null) 
        {
            gravityInvertedEvent = new UnityEvent<(Vector3, float, bool)>();
        }
        if (windChangedEvent == null) 
        {
            windChangedEvent = new UnityEvent<(Vector3, float, WindDirection)>();
        }
        if (modifierButtonSelectedEvent == null)
        {
            modifierButtonSelectedEvent = new UnityEvent<Modifier>();
        }
        if (savingStateEvent == null)
        {
            savingStateEvent = new UnityEvent<bool>();
        }
    }

    public void ToggleGravity(Vector3 originPosition, float range)
    {
        if (currentModifier != Modifier.GRAVITY_INVERTED) 
        {
            currentModifier = Modifier.GRAVITY_INVERTED;
            gravityInvertedEvent.Invoke((originPosition, range, true));
        }
        else
        {
            currentModifier = Modifier.NONE;
            gravityInvertedEvent.Invoke((originPosition, range, false));
        }
        windChangedEvent.Invoke((originPosition, range, WindDirection.NONE));
    }

    public void ToggleWind(Vector3 originPosition, float range, WindDirection direction)
    {
        currentModifier = ResourceHelper.WindDirectionToModifier(direction);
        windChangedEvent.Invoke((originPosition, range, direction));
        gravityInvertedEvent.Invoke((originPosition, range, false));
    }

    public void ClearModifier(Vector3 originPosition, float range)
    {
        currentModifier = Modifier.NONE;

        gravityInvertedEvent.Invoke((originPosition, range, false));
        windChangedEvent.Invoke((originPosition, range, WindDirection.NONE));
    }

    // Used by the Ability Wheel buttons
    public void SelectModifier(Modifier modifier)
    {
        modifierButtonSelectedEvent.Invoke(modifier);
    }

    // Used to trigger saving state when player enters a new section
    public void SetSavingState(bool isSaving)
    {
        savingStateEvent.Invoke(isSaving);
    }
}
