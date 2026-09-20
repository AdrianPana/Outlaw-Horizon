using System;
using UnityEngine;
using Game.Modifiers;

[RequireComponent(typeof(Renderer))]
public class ModifierShader : MonoBehaviour
{
    private Color windColor = new Color32(0, 193, 255, 255);
    private Color gravityColor = new Color32(252, 110, 81, 255);
    private Color thirdColor = new Color32(150, 0, 180, 255);
    private Color _fallbackColor = Color.black;

    [HideInInspector, Min(0f)] public float _intensity = 1f;

    private static readonly int ColorID = Shader.PropertyToID("_Color");
    private static readonly int IntensityID = Shader.PropertyToID("_Intensity");
    private static readonly int MainTextureID = Shader.PropertyToID("_Main_Texture");

    [SerializeField] private Material _modifiableMaterial;
    [SerializeField] private static float _idleIntensity = 0.3f;
    [SerializeField] private static float _inRangeIntensity = 0.8f;
    [SerializeField] private static float _influencedIntensity = 1.5f;
    private Material[] _materials;

    private AbstractModifierObject modifierObject => GetComponent<AbstractModifierObject>();

    public float Intensity
    {
        get => _intensity;
        set { _intensity = Mathf.Max(0f, value); ApplyIntensity(); }
    }

    private int modifierSourcesInRange;

    public void MarkSourceInRange()
    {
        modifierSourcesInRange++;
    }

    public void MarkSourceOutOfRange()
    {
        modifierSourcesInRange--;
        if (modifierSourcesInRange < 0)
        {
            modifierSourcesInRange = 0;
        }
    }

    private void Awake()
    {
        SwapToModifiableMaterial();
    }

    private void Start()
    {
        ApplyIntensity();
        CheckForModifiers();
        modifierSourcesInRange = 0;
    }

    private void Update()
    {
        _intensity = modifierObject.CheckIfInfluenced() ? _influencedIntensity :
            (modifierSourcesInRange > 0 ? _inRangeIntensity : _idleIntensity);

        ApplyIntensity();
    }

    private void SwapToModifiableMaterial()
    {
        var renderer = GetComponent<Renderer>();
        Material[] sharedMaterials = renderer.sharedMaterials;

        if (_modifiableMaterial == null)
        {
            Debug.LogError("[ShaderController] No Modifiable Material assigned! " +
                           "Drag your material asset into the 'Modifiable Material' field.", this);
            return;
        }

        // Instantiate so we don't modify the shared asset.
        _materials = new Material[sharedMaterials.Length];

        for (int i = 0; i < sharedMaterials.Length; i++)
        {
            Texture originalMatTexture = sharedMaterials[i] != null ? sharedMaterials[i].mainTexture : null;
            Material instance = Instantiate(_modifiableMaterial);

            if (originalMatTexture != null)
                instance.SetTexture(MainTextureID, originalMatTexture);
            else
                Debug.LogWarning("[ShaderController] Original material had no mainTexture — " +
                                 "_Main_Texture will be empty.", this);

            _materials[i] = instance;
        }

        renderer.materials = _materials;
    }

    private void CheckForModifiers()
    {
        Color effectColor = Color.black;

        if (GetComponent<WindProvider>() != null && GetComponent<GravityProvider>() != null)
        {
            ApplyColor(thirdColor);
        }
        else if (GetComponent<WindProvider>() != null)
        {
            ApplyColor(windColor);
        }
        else if (GetComponent<GravityProvider>() != null)
        {
            ApplyColor(gravityColor);
        }
        else
        {
            ApplyColor(_fallbackColor);
        }
    }

#if UNITY_EDITOR
    private void OnValidate()
    {
        // Keep the Inspector preview live in Play Mode.
        if (_materials == null || _materials.Length == 0) return;
        ApplyIntensity();
        CheckForModifiers();
    }
#endif

    private void OnDestroy()
    {
        if (_materials == null) return;

        foreach (var mat in _materials)
        {
            if (mat != null)
                Destroy(mat);
        }
    }

    private void ApplyColor(Color color)
    {
        if (_materials == null) return;

        foreach (var mat in _materials)
        {
            if (mat != null)
                mat.SetColor(ColorID, color);
        }
    }

    private void ApplyIntensity()
    {
        if (_materials == null) return;

        foreach (var mat in _materials)
        {
            if (mat != null)
                mat.SetFloat(IntensityID, _intensity);
        }
    }
}