using UnityEngine;

[CreateAssetMenu(fileName = "ModifierParticleProperties", menuName = "Scriptable Objects/ModifierParticleProperties")]
public class ModifierParticleProperties : ScriptableObject
{
    public Color startColor;
    public float rateOverTime;
    public float speedModifier;
    public bool noiseEnabled;
    public float trailLifetime;
}
