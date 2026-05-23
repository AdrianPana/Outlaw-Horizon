using UnityEngine;
using UnityEngine.InputSystem;
using Game.Resources;
using System;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using StarterAssets;
using UnityEngine.Windows;

public class GloveScript : MonoBehaviour
{
    private StarterAssetsInputs starterInputs;
    private InputSystem_Actions playerInputActions;
    private Camera cam;

    public UniversalStateManagerScriptableObject universalStateManagerScriptableObject;
    public SectionManager currentSection;
    public GameObject windMenuUI;
    private WindWheelController windMenuUIController;
    public Volume postProcessingVolume;
    public float menuTimeScale = 0.0f;
    public float range = 5.0f;
    [SerializeField] GameObject rangeIndicator;

    private Modifier menuSelectedModifier = Modifier.NONE;

    private ThirdPersonController thirdPersonController;

    private ColorAdjustments colorAdjustments;

    public GameObject goalPrefab;
    private GameObject currentGoalInstance;

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
        playerInputActions.Player.Gravity.started += ToggleGravity;
        playerInputActions.Player.Clear.started += ToggleWindNone;
        playerInputActions.Player.Ability.started += ShowAbilityMenu;
        playerInputActions.Player.Ability.canceled += HideAbilityMenu;
        playerInputActions.Player.Reset.started += ResetSceneCallback;

        universalStateManagerScriptableObject.modifierButtonSelectedEvent.AddListener(MenuModifierSelected);

    }

    private void OnDisable()
    {
        playerInputActions.Disable();
        playerInputActions.Player.Gravity.started -= ToggleGravity;
        playerInputActions.Player.Clear.started -= ToggleWindNone;
        playerInputActions.Player.Ability.started -= ShowAbilityMenu;
        playerInputActions.Player.Ability.canceled -= HideAbilityMenu;
        playerInputActions.Player.Reset.started -= ResetSceneCallback;

        universalStateManagerScriptableObject.modifierButtonSelectedEvent.RemoveListener(MenuModifierSelected);
    }

    private void ToggleGravity(InputAction.CallbackContext ctx)
    {
        universalStateManagerScriptableObject.ToggleGravity(transform.position, range);
    }

    private void ToggleWindNone(InputAction.CallbackContext ctx) 
    { 
        universalStateManagerScriptableObject.ToggleWind(transform.position, range, WindDirection.NONE);
    }

    private void ToggleModifier(Modifier modifier)
    {
        switch (modifier)
        {
            case Modifier.GRAVITY_INVERTED:
                universalStateManagerScriptableObject.ToggleGravity(transform.position, range);
                break;
            case Modifier.WIND_NORTH:
                universalStateManagerScriptableObject.ToggleWind(transform.position, range, WindDirection.NORTH);
                break;
            case Modifier.WIND_EAST:
                universalStateManagerScriptableObject.ToggleWind(transform.position, range, WindDirection.EAST);
                break;
            case Modifier.WIND_SOUTH:
                universalStateManagerScriptableObject.ToggleWind(transform.position, range, WindDirection.SOUTH);
                break;
            case Modifier.WIND_WEST:
                universalStateManagerScriptableObject.ToggleWind(transform.position, range, WindDirection.WEST);
                break;
            default:
                universalStateManagerScriptableObject.ToggleWind(transform.position, range, WindDirection.NONE);
                break;
        }
    }

    private void ShowAbilityMenu(InputAction.CallbackContext context)
    {
        Time.timeScale = menuTimeScale; // Pause the game
        colorAdjustments.saturation.value = -100f; // Desaturate the screen to indicate ability menu is open

        if (thirdPersonController != null)
        {
            thirdPersonController.enabled = false; // Disable player movement
        }

        starterInputs.SetCursorState(false); // Show the cursor for the ability menu
        
        windMenuUI.gameObject.SetActive(true); // Show the ability menu UI
        windMenuUIController.Rotate(cam.transform.eulerAngles.y);
    }
    private void HideAbilityMenu(InputAction.CallbackContext context)
    {
        Time.timeScale = 1f; // Resume the game
        colorAdjustments.saturation.value = 0f; // Reset saturation to normal

        if (thirdPersonController != null)
        {
            thirdPersonController.enabled = true; // Enable player movement
        }

        starterInputs.SetCursorState(true); // Hide the cursor when closing the ability menu

        windMenuUI.gameObject.SetActive(false); // Hide the ability menu UI

        ToggleModifier(menuSelectedModifier);
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
    public void Update()
    {
        //if (currentGoalInstance == null)
        //{
        //    currentGoalInstance = Instantiate(goalPrefab, transform.position + Vector3.up, Quaternion.identity, transform.parent);
        //}
    }
}
