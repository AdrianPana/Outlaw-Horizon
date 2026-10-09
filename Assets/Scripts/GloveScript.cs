using UnityEngine;
using UnityEngine.InputSystem;
using Game.Resources;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using StarterAssets;

public class GloveScript : MonoBehaviour
{
    public GloveUI gloveUI;

    private InputSystem_Actions playerInputActions;
    private Camera cam;

    public UniversalStateManagerScriptableObject universalStateManagerScriptableObject;
    public SectionManager currentSection;

    public GameObject abilityMenuUI;
    private WindWheelController windMenuUIController;

    public Volume postProcessingVolume;
    private ColorAdjustments colorAdjustments;

    public float menuTimeScale = 0.0f;
    private Modifier menuSelectedModifier = Modifier.NONE;

    public float range = 5.0f;
    public GameObject rangeIndicator;

    [HideInInspector] public bool hasWindAbility = false;
    [HideInInspector] public bool hasGravityAbility = false;

    private void Awake()
    {
        playerInputActions = new InputSystem_Actions();
    }

    private void Start()
    {
        rangeIndicator.transform.localScale = new Vector3(range, 0.025f, range);
        rangeIndicator.SetActive(hasWindAbility || hasGravityAbility);

        postProcessingVolume.profile.TryGet<ColorAdjustments>(out colorAdjustments);
        abilityMenuUI.SetActive(false);

        cam = Camera.main;

        windMenuUIController = abilityMenuUI.GetComponentInChildren<WindWheelController>();

        GetComponent<SphereCollider>().radius = range;

    }

    private void Update()
    {
        rangeIndicator.SetActive(hasWindAbility || hasGravityAbility);
    }

    private void OnEnable()
    {
        playerInputActions.Enable();
        playerInputActions.Player.Ability.started += ShowAbilityMenu;
        playerInputActions.Player.Ability.canceled += HideAbilityMenu;
        playerInputActions.Player.Reset.started += ResetSceneCallback;

        universalStateManagerScriptableObject.modifierButtonSelectedEvent.AddListener(MenuModifierSelected);

    }

    private void OnDisable()
    {
        playerInputActions.Disable();
        playerInputActions.Player.Ability.started -= ShowAbilityMenu;
        playerInputActions.Player.Ability.canceled -= HideAbilityMenu;
        playerInputActions.Player.Reset.started -= ResetSceneCallback;

        universalStateManagerScriptableObject.modifierButtonSelectedEvent.RemoveListener(MenuModifierSelected);
    }

    public virtual void ToggleModifier(Modifier modifier)
    {
        switch (modifier)
        {
            case Modifier.GRAVITY_INVERTED:
                universalStateManagerScriptableObject.ToggleGravity(transform.position, range);
                break;
            case Modifier.NONE:
                universalStateManagerScriptableObject.ClearAllModifiers(transform.position, range);
                //universalStateManagerScriptableObject.ToggleWind(transform.position, range, WindDirection.NONE);
                break;
            default:
                universalStateManagerScriptableObject.ToggleWind(transform.position, range, ResourceHelper.ModifierToWindDirection(modifier));
                break;
        }
    }

    private void ShowAbilityMenu(InputAction.CallbackContext context)
    {
        if (gloveUI.ActiveUpgrade == null)
        {
            return;
        }

        EnterMenuScreen();
        
        abilityMenuUI.SetActive(true);
        if (windMenuUIController != null)
            windMenuUIController.Rotate(cam.transform.eulerAngles.y);
    }

    private void EnterMenuScreen()
    {
        Time.timeScale = menuTimeScale; // Pause the game
        colorAdjustments.saturation.value = -100f; // Desaturate the screen to indicate ability menu is open

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    private void HideAbilityMenu(InputAction.CallbackContext context)
    {
        ExitMenuScreen();

        abilityMenuUI.SetActive(false);

        ToggleModifier(menuSelectedModifier);
    }

    private void ExitMenuScreen()
    {
        Time.timeScale = 1f; // Resume the game
        colorAdjustments.saturation.value = 0f; // Reset saturation to normal

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    private void MenuModifierSelected(Modifier modifier)
    {
        menuSelectedModifier = modifier;
    }

    private void ResetSceneCallback(InputAction.CallbackContext ctx)
    {
        ResetScene();
    }

    public void ResetScene()
    {
        universalStateManagerScriptableObject.ClearAllModifiers(Vector3.zero, -int.MaxValue); // Clear all modifiers with infinite range

        // tp back to checkpoint
        if (currentSection.respawnPoint != null)
        {
            transform.SetPositionAndRotation(currentSection.respawnPoint, Quaternion.Euler(currentSection.respawnRotation));
        }

        // reload snapshot
        currentSection.ResetSection();
    }

    public bool SetNewCurrentSection(SectionManager section)
    {
        if (currentSection == section)
        {
            return false;
        }

        currentSection = section;
        return true;
    }

    private void OnTriggerEnter(Collider other)
    {
        Debug.Log($"GloveScript: OnTriggerEnter with {other.name}");
        if (other.TryGetComponent<ModifierShader> (out var modifierObject))
        {
            modifierObject.MarkSourceInRange();
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.TryGetComponent<ModifierShader> (out var modifierObject))
        {
            modifierObject.MarkSourceOutOfRange();
        }
    }

    public void ToggleUpgrade(Upgrade upgrade, bool isActive)
    {
        if (upgrade is WindUpgrade)
        {
            Debug.Log("WE got wind upgrade");
            hasWindAbility = isActive;
            abilityMenuUI.transform.GetChild(0).gameObject.SetActive(isActive);
            windMenuUIController = abilityMenuUI.GetComponentInChildren<WindWheelController>();
            return;
        }
        if (upgrade is GravityUpgrade)
        {
            hasGravityAbility = isActive;
            abilityMenuUI.transform.GetChild(1).gameObject.SetActive(isActive);
            return;
        }

        Debug.Log("Unknown upgrade type: " + upgrade.GetType().Name);
    }
}
