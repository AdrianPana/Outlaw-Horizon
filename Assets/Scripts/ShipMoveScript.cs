using Game.Resources;
using StarterAssets;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Windows;

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

#if ENABLE_INPUT_SYSTEM
    private PlayerInput _playerInput;
#endif
    private StarterAssetsInputs _input;
    private const float _threshold = 0.01f;

    private bool IsCurrentDeviceMouse
    {

        get
        {
            //#if ENABLE_INPUT_SYSTEM
            //            return _playerInput.currentControlScheme == "KeyboardMouse";
            //#else
            //				return false;
            //#endif
            return true;
        }
    }

    private GloveScript gloveScript;

    public override void Awake()
    {
        base.Awake();
        _input = GetComponent<StarterAssetsInputs>();
        _cinemachineTargetYaw = CinemachineCameraTarget.transform.rotation.eulerAngles.y;
        gloveScript = GetComponentInChildren<GloveScript>();
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
        // if there is an input and camera position is not fixed
        if (_input.look.sqrMagnitude >= _threshold)
        {
            //Don't multiply mouse input by Time.deltaTime;
            float deltaTimeMultiplier = IsCurrentDeviceMouse ? 1.0f : Time.deltaTime;

            _cinemachineTargetYaw += _input.look.x * deltaTimeMultiplier;
            _cinemachineTargetPitch += _input.look.y * deltaTimeMultiplier;
        }

        // clamp our rotations so our values are limited 360 degrees
        _cinemachineTargetYaw = OH_Helpers.ClampAngle(_cinemachineTargetYaw, float.MinValue, float.MaxValue);
        _cinemachineTargetPitch = OH_Helpers.ClampAngle(_cinemachineTargetPitch, BottomClamp, TopClamp);

        // Cinemachine will follow this target
        CinemachineCameraTarget.transform.rotation = Quaternion.Euler(_cinemachineTargetPitch + CameraAngleOverride,
            _cinemachineTargetYaw, 0.0f);
    }

    //private void OnCollisionEnter(Collision collision)
    //{
    //    //if (gloveScript != null)
    //    //{
    //    //    gloveScript.ToggleModifier(Modifier.NONE);
    //    //}
    //}
}
