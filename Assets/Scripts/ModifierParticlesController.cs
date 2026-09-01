using Game.Resources;
using UnityEngine;

public class ModifierParticlesController : MonoBehaviour
{
    public UniversalStateManagerScriptableObject stateManager;
    public Transform player;
    public float orbitRadius = 3f;
    private ParticleSystem modifierParticles;
    private Vector3 latestDisplacementToTarget;
    public ModifierParticleProperties windParticleProperties;
    public ModifierParticleProperties gravityParticleProperties;

    private void Awake()
    {
        modifierParticles = GetComponent<ParticleSystem>();
    }

    private void Start()
    {
        modifierParticles.Stop();
    }

    private void OnEnable()
    {
        stateManager.gravityInvertedEvent.AddListener(OnGravityChanged);
        stateManager.windChangedEvent.AddListener(OnWindChanged);
    }

    private void OnDisable()
    {
        stateManager.gravityInvertedEvent.RemoveListener(OnGravityChanged);
        stateManager.windChangedEvent.RemoveListener(OnWindChanged);
    }

    private void Update()
    {
        modifierParticles.transform.position = player.transform.position + latestDisplacementToTarget;
    }

    private void OnGravityChanged((Vector3 origin, float range, bool inverted, GameObject target) data)
    {
        if (!data.inverted) {
            ClearParticles();
            return;
        }

        latestDisplacementToTarget = Vector3.zero;
        modifierParticles.transform.rotation = Quaternion.Euler(Vector3.up);

        UpdateParticleState(gravityParticleProperties);
    }

    private void OnWindChanged((Vector3 origin, float range, WindDirection direction, GameObject target) data)
    {
        if (data.direction == WindDirection.NONE)
        {
            ClearParticles();
            return;
        }

        Vector3 dir = WindDirectionToPositionDisplacement(data.direction);
        latestDisplacementToTarget = dir * orbitRadius;

        var rotationTowardsTarget = WindDirectionToSourceRotation(data.direction) * 90;
        modifierParticles.transform.rotation = Quaternion.Euler(rotationTowardsTarget);

        UpdateParticleState(windParticleProperties);
    }

    private Vector3 WindDirectionToPositionDisplacement(WindDirection direction)
    {
        switch (direction)
        {
            case WindDirection.NORTH: return Vector3.back;
            case WindDirection.EAST: return Vector3.left;
            case WindDirection.SOUTH: return Vector3.forward;
            case WindDirection.WEST: return Vector3.right;
            default: return Vector3.forward;
        }
    }

    private Vector3 WindDirectionToSourceRotation(WindDirection direction)
    {
        switch (direction)
        {
            case WindDirection.NORTH: return Vector3.right;
            case WindDirection.EAST: return Vector3.back;
            case WindDirection.SOUTH: return Vector3.left;
            case WindDirection.WEST: return Vector3.forward;
            default: return Vector3.forward;
        }
    }

    private void UpdateParticleState(ModifierParticleProperties properties)
    {
        modifierParticles.Stop();
        SetNewParticleEffectProperties(properties);
        modifierParticles.Play();
    }

    private void ClearParticles()
    {
        modifierParticles.Stop();
    }

    private void SetNewParticleEffectProperties(ModifierParticleProperties props)
    {
        var main = modifierParticles.main;
        main.startColor = props.startColor;

        var emission = modifierParticles.emission;
        emission.rateOverTime = props.rateOverTime;

        var vel = modifierParticles.velocityOverLifetime;
        vel.enabled = true;
        vel.speedModifier = props.speedModifier;

        var noise = modifierParticles.noise;
        noise.enabled = props.noiseEnabled;

        var trails = modifierParticles.trails;
        trails.enabled = true;
        trails.lifetime = props.trailLifetime;
    }
}