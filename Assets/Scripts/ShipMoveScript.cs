using Game.Resources;
using UnityEngine;
using UnityEngine.InputSystem;

public class ShipMoveScript : ModifierAffectedObject
{
    [SerializeField] private float rotationSpeed = 5.0f;

    [Header("Cinemachine")]
    [Tooltip("The follow target set in the Cinemachine Virtual Camera that the camera will follow")]
    public GameObject CinemachineCameraTarget;

    [Tooltip("How far in degrees can you move the camera up")]
    public float TopClamp = 70.0f;

    [Tooltip("How far in degrees can you move the camera down")]
    public float BottomClamp = -30.0f;

    [Tooltip("Additional degress to override the camera. Useful for fine tuning camera position when locked")]
    public float CameraAngleOverride = 0.0f;

    private float _cinemachineTargetYaw;
    private float _cinemachineTargetPitch;

    private InputSystem_Actions _inputSystemActions;
    private const float _threshold = 0.01f;

    private bool IsCurrentDeviceMouse
    {

        get
        {
            return true;
        }
    }

    private GloveScript gloveScript;

    public override void Awake()
    {
        base.Awake();
        _inputSystemActions = new InputSystem_Actions();
        _cinemachineTargetYaw = CinemachineCameraTarget.transform.rotation.eulerAngles.y;
        gloveScript = GetComponentInChildren<GloveScript>();
    }

    private void OnEnable()
    {
        _inputSystemActions.Enable();
    }

    private void OnDisable()
    {
        _inputSystemActions.Disable();
    }

    private void Update()
    {
        Vector3 moveDirection = new Vector3(rb.linearVelocity.x, 0, rb.linearVelocity.z);

        if (moveDirection.magnitude > 0.1f)
        {
            var targetRotation = Quaternion.LookRotation(moveDirection, Vector3.up);
            transform.rotation = Quaternion.Slerp(rb.rotation, targetRotation, rotationSpeed * Time.deltaTime);
        }
    }

    private void LateUpdate()
    {
        CameraRotation();
    }

    private void CameraRotation()
    {
        Vector2 look = _inputSystemActions.Player.Look.ReadValue<Vector2>();

        // if there is an input and camera position is not fixed
        if (look.sqrMagnitude >= _threshold)
        {
            //Don't multiply mouse input by Time.deltaTime;
            float deltaTimeMultiplier = IsCurrentDeviceMouse ? 1.0f : Time.deltaTime;

            _cinemachineTargetYaw += look.x * deltaTimeMultiplier;
            _cinemachineTargetPitch += look.y * deltaTimeMultiplier;
        }

        // clamp our rotations so our values are limited 360 degrees
        _cinemachineTargetYaw = OH_Helpers.ClampAngle(_cinemachineTargetYaw, float.MinValue, float.MaxValue);
        _cinemachineTargetPitch = OH_Helpers.ClampAngle(_cinemachineTargetPitch, BottomClamp, TopClamp);

        // Cinemachine will follow this target
        CinemachineCameraTarget.transform.rotation = Quaternion.Euler(_cinemachineTargetPitch + CameraAngleOverride,
            _cinemachineTargetYaw, 0.0f);
    }
}
