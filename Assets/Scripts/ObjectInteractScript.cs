using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;

public class CollectorScript : MonoBehaviour
{
    public float interactDistance = 2f;
    public LayerMask interactableLayer;

    private Renderer currentRenderer;
    private InteractorScript selectedInteractor;
    private Material originalMaterial;
    public Material selectedMaterial;

    public GameObject tooltip;

    private InputSystem_Actions playerInputActions;

    private void Awake()
    {
        playerInputActions = new InputSystem_Actions();
    }
    private void OnEnable()
    {
        playerInputActions.Enable();
        playerInputActions.Player.Interact.started += Interact;
    }

    private void OnDisable()
    {
        playerInputActions.Disable();
        playerInputActions.Player.Interact.started -= Interact;
    }

    void Update()
    {
        float radius = 0.8f;

        Vector3 origin = transform.position;
        Vector3 direction = transform.forward;

        if (Physics.SphereCast(origin, radius, direction, out RaycastHit hit, interactDistance, interactableLayer))
        {
            Debug.DrawLine(origin, hit.point, Color.green);
            selectedInteractor = hit.transform.GetComponentInChildren<InteractorScript>();
            Renderer rend = hit.collider.GetComponentInChildren<Renderer>();

            if (rend != null && rend != currentRenderer)
            {
                ClearHighlight();

                currentRenderer = rend;
                originalMaterial = rend.material;
                rend.material = selectedMaterial;

                if (tooltip != null)
                {
                    tooltip.SetActive(true);
                    tooltip.transform.position = hit.transform.position + new Vector3(0, 1, 0);
                }
            }
        }
        else
        {
            ClearHighlight();
            Debug.DrawRay(origin, direction * interactDistance, Color.red);
        }

        RotateTooltip();
    }

    void ClearHighlight()
    {
        if (currentRenderer != null)
        {
            currentRenderer.material = originalMaterial;
            currentRenderer = null;
            selectedInteractor = null;
        }

        if (tooltip != null)
            tooltip.SetActive(false);
    }

    void RotateTooltip()
    {
        if (tooltip != null && tooltip.activeSelf)
        {
            tooltip.transform.LookAt(transform);
            tooltip.transform.Rotate(0, 180, 0); // flip because LookAt faces backward by default
        }
    }

    void Interact(InputAction.CallbackContext ctx)
    {
        if (selectedInteractor != null) 
        {
            selectedInteractor.ToggleModifier();
        }
    }
}
