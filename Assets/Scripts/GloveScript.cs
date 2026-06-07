using UnityEngine;
using UnityEngine.InputSystem;
using Game.Resources;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using StarterAssets;

public class GloveScript : MonoBehaviour
{
    [SerializeField]
    private GloveUI gloveUI;

    private StarterAssetsInputs starterInputs;
    private InputSystem_Actions playerInputActions;
    private ThirdPersonController thirdPersonController;
    private Camera cam;

    public UniversalStateManagerScriptableObject universalStateManagerScriptableObject;
    public SectionManager currentSection;

    public GameObject windMenuUI;
    public GameObject gravityMenuUI;
    private WindWheelController windMenuUIController;

    public Volume postProcessingVolume;
    private ColorAdjustments colorAdjustments;

    public float menuTimeScale = 0.0f;
    private Modifier menuSelectedModifier = Modifier.NONE;

    public float range = 5.0f;
    [SerializeField] GameObject rangeIndicator;

    [HideInInspector] public bool hasWindAbility = false;
    [HideInInspector] public bool hasGravityAbility = false;

    private void Awake()
    {
        playerInputActions = new InputSystem_Actions();
        starterInputs = GetComponent<StarterAssetsInputs>();
    }

    private void Start()
    {
        rangeIndicator.transform.localScale = new Vector3(range, 0.025f, range);

        postProcessingVolume.profile.TryGet<ColorAdjustments>(out colorAdjustments);

        thirdPersonController = GetComponent<ThirdPersonController>();

        cam = Camera.main;

        windMenuUIController = windMenuUI.GetComponent<WindWheelController>();

    }

    private void OnEnable()
    {
        playerInputActions.Enable();
        playerInputActions.Player.Cycle.started += CycleAbility;
        playerInputActions.Player.Ability.started += ShowAbilityMenu;
        playerInputActions.Player.Ability.canceled += HideAbilityMenu;
        playerInputActions.Player.Reset.started += ResetSceneCallback;

        universalStateManagerScriptableObject.modifierButtonSelectedEvent.AddListener(MenuModifierSelected);

    }

    private void OnDisable()
    {
        playerInputActions.Disable();
        playerInputActions.Player.Cycle.started -= CycleAbility;
        playerInputActions.Player.Ability.started -= ShowAbilityMenu;
        playerInputActions.Player.Ability.canceled -= HideAbilityMenu;
        playerInputActions.Player.Reset.started -= ResetSceneCallback;

        universalStateManagerScriptableObject.modifierButtonSelectedEvent.RemoveListener(MenuModifierSelected);
    }

    public void ToggleModifier(Modifier modifier)
    {
        switch (modifier)
        {
            case Modifier.GRAVITY_INVERTED:
                universalStateManagerScriptableObject.ToggleGravity(transform.position, range);
                break;
            case Modifier.NONE:
                //universalStateManagerScriptableObject.ClearModifier(transform.position, range);
                universalStateManagerScriptableObject.ToggleWind(transform.position, range, WindDirection.NONE);
                break;
            default:
                universalStateManagerScriptableObject.ToggleWind(transform.position, range, ResourceHelper.ModifierToWindDirection(modifier));
                break;
        }
    }

    private void CycleAbility(InputAction.CallbackContext ctx)
    {
        gloveUI.CycleNext();
    }

    private void ShowAbilityMenu(InputAction.CallbackContext context)
    {
        if (gloveUI.ActiveUpgrade == null)
        {
            return;
        }

        EnterMenuScreen();
        
        switch (gloveUI.ActiveUpgrade.Type)
        {
            case UpgradeType.WindRune:
                windMenuUI.gameObject.SetActive(true);
                windMenuUIController.Rotate(cam.transform.eulerAngles.y);
                break;
            default:
                gravityMenuUI.gameObject.SetActive(true);
                break;
        }
    }

    private void EnterMenuScreen()
    {
        Time.timeScale = menuTimeScale; // Pause the game
        colorAdjustments.saturation.value = -100f; // Desaturate the screen to indicate ability menu is open

        if (thirdPersonController != null)
        {
            thirdPersonController.enabled = false; // Disable player movement
        }

        starterInputs.SetCursorState(false); // Show the cursor for the ability menu
    }

    private void HideAbilityMenu(InputAction.CallbackContext context)
    {
        ExitMenuScreen();

        switch (gloveUI.ActiveUpgrade.Type)
        {
            case UpgradeType.WindRune:
                windMenuUI.gameObject.SetActive(false);
                break;
            default:
                gravityMenuUI.gameObject.SetActive(false);
                break;
        }

        ToggleModifier(menuSelectedModifier);
    }

    private void ExitMenuScreen()
    {
        Time.timeScale = 1f; // Resume the game
        colorAdjustments.saturation.value = 0f; // Reset saturation to normal

        if (thirdPersonController != null)
        {
            thirdPersonController.enabled = true; // Enable player movement
        }

        starterInputs.SetCursorState(true); // Hide the cursor when closing the ability menu
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
        universalStateManagerScriptableObject.ClearModifier(Vector3.zero, -int.MaxValue); // Clear all modifiers with infinite range

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
}
