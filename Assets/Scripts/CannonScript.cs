using StarterAssets;
using UnityEngine;
using UnityEngine.InputSystem;

public class CannonScript : MonoBehaviour
{
    public GameObject cannonBallPrefab;
    public Transform shootOrigin;

    private StarterAssetsInputs starterInputs;
    private InputSystem_Actions playerInputActions;

    private void Awake()
    {
        playerInputActions = new InputSystem_Actions();
        starterInputs = GetComponent<StarterAssetsInputs>();
    }

    private void OnEnable()
    {
        playerInputActions.Enable();
        playerInputActions.Player.Jump.started += Shoot;
    }

    private void OnDisable()
    {
        playerInputActions.Disable();
        playerInputActions.Player.Jump.started -= Shoot;
    }

    private void Shoot(InputAction.CallbackContext ctx)
    {
        Instantiate(cannonBallPrefab, shootOrigin.position, transform.rotation);
    }
}
