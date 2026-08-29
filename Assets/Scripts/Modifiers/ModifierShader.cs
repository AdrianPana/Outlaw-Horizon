using System;
using UnityEngine;
using Game.Modifiers;

[RequireComponent(typeof(Renderer))]
[RequireComponent(typeof(ModifierAffectedObject))]
public class ModifierShader : MonoBehaviour
{
    private Color windColor = new Color32(0, 193, 255, 255);
    private Color gravityColor = new Color32(253, 42, 0, 255);
    private Color thirdColor = new Color32(150, 0, 180, 255);
    private Color _fallbackColor = Color.black;

    [HideInInspector, Min(0f)] public float _intensity = 1f;

    private static readonly int ColorID = Shader.PropertyToID("_Color");
    private static readonly int IntensityID = Shader.PropertyToID("_Intensity");
    private static readonly int MainTextureID = Shader.PropertyToID("_Main_Texture");

    [SerializeField] private Material _modifiableMaterial;
    private Material _material;

    private ModifierAffectedObject modifierObject => GetComponent<ModifierAffectedObject>();

    public float Intensity
    {
        get => _intensity;
        set { _intensity = Mathf.Max(0f, value); ApplyIntensity(); }
    }

    private void Awake()
    {
        SwapToModifiableMaterial();
    }

    private void Start()
    {
        ApplyIntensity();
        CheckForModifiers();
    }

    private void Update()
    {
        _intensity = modifierObject.CheckIfInfluenced() ? 1.2f : 0.6f;

        ApplyIntensity();
    }

    private void SwapToModifiableMaterial()
    {
        var renderer = GetComponent<Renderer>();

        // Grab the texture from the current material before we replace it.
        Texture originalTexture = renderer.sharedMaterial != null
            ? renderer.sharedMaterial.mainTexture
            : null;

        if (_modifiableMaterial == null)
        {
            Debug.LogError("[ShaderController] No Modifiable Material assigned! " +
                           "Drag your material asset into the 'Modifiable Material' field.", this);
            _material = renderer.material; // fall back to whatever is there
            return;
        }

        // Instantiate so we don't modify the shared asset.
        _material = Instantiate(_modifiableMaterial);
        renderer.material = _material;

        // Push the original texture into the shader's _Main_Texture slot.
        if (originalTexture != null)
            _material.SetTexture(MainTextureID, originalTexture);
        else
            Debug.LogWarning("[ShaderController] Original material had no mainTexture — " +
                             "_Main_Texture will be empty.", this);
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
        if (_material == null) return;
        ApplyIntensity();
        CheckForModifiers();
    }
#endif

    private void OnDestroy()
    {
        if (_material != null)
            Destroy(_material);
    }

    private void ApplyColor(Color color)
    {
        if (_material != null)
            _material.SetColor(ColorID, color);
    }

    private void ApplyIntensity()
    {
        if (_material != null)
            _material.SetFloat(IntensityID, _intensity);
    }
}