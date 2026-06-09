using UnityEngine;
using Unity.Cinemachine;
using UnityEngine.InputSystem;

public class CameraSwitcher : MonoBehaviour
{
    public enum ViewMode { Main, Cannon1, Cannon2 }
    public ViewMode CurrentView => (ViewMode)_currentIndex;

    [Header("Cameras")]
    public CinemachineCamera mainCamera;
    public CinemachineCamera cannon1Camera;
    public CinemachineCamera cannon2Camera;

    [Header("Line Renderers")]
    public LineRenderer cannon1Line;
    public LineRenderer cannon2Line;

    private InputSystem_Actions _playerInputActions;
    private int _currentIndex = 0; // 0: Main, 1: Cannon 1, 2: Cannon 2

    private void Awake()
    {
        _playerInputActions = new InputSystem_Actions();
    }

    private void OnEnable()
    {
        _playerInputActions.Enable();
        _playerInputActions.Player.Interact.started += OnInteract;
    }

    private void OnDisable()
    {
        _playerInputActions.Player.Interact.started -= OnInteract;
        _playerInputActions.Disable();
    }

    private void Start()
    {
        UpdateCameraSelection();
    }

    private void OnInteract(InputAction.CallbackContext context)
    {
        _currentIndex = (_currentIndex + 1) % 3;
        UpdateCameraSelection();
    }

    private void UpdateCameraSelection()
    {
        // Set priorities: Active camera gets higher priority
        if (mainCamera != null) mainCamera.Priority = (_currentIndex == 0) ? 20 : 10;
        if (cannon1Camera != null) cannon1Camera.Priority = (_currentIndex == 1) ? 20 : 10;
        if (cannon2Camera != null) cannon2Camera.Priority = (_currentIndex == 2) ? 20 : 10;

        // Toggle LineRenderers
        if (cannon1Line != null) cannon1Line.enabled = (_currentIndex == 1);
        if (cannon2Line != null) cannon2Line.enabled = (_currentIndex == 2);
    }
}
