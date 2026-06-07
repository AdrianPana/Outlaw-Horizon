using StarterAssets;
using UnityEngine;
using UnityEngine.InputSystem;

public class CannonScript : MonoBehaviour
{
    public GameObject cannonBallPrefab;
    public Transform shootOrigin;

    [Header("Trajectory")]
    [SerializeField] private LineRenderer trajectoryLine;
    [SerializeField] private float speed = 20f;
    [SerializeField] private float arcAngle = 30f;
    [SerializeField] private int trajectoryPoints = 50;
    [SerializeField] private float timeStep = 0.1f;

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
        playerInputActions.Player.Jump.started -= Shoot;
        playerInputActions.Disable();
    }

    private void Update()
    {
        DrawTrajectory();
    }

    private void Shoot(InputAction.CallbackContext ctx)
    {
        GameObject ball =
            Instantiate(cannonBallPrefab, shootOrigin.position, transform.rotation);

        Rigidbody shipRb = transform.parent.GetComponent<Rigidbody>();

        ball.GetComponent<CannonBallScript>()
            .Initialize(speed, arcAngle, shipRb.linearVelocity);
    }

    private void DrawTrajectory()
    {
        Vector3 startPos = shootOrigin.position;

        Vector3 direction =
            Quaternion.Euler(-arcAngle, 0f, 0f) * transform.forward;

        Vector3 velocity = direction * speed;

        trajectoryLine.positionCount = trajectoryPoints;

        for (int i = 0; i < trajectoryPoints; i++)
        {
            float t = i * timeStep;

            Vector3 point =
                startPos +
                velocity * t +
                0.5f * Physics.gravity * t * t;

            trajectoryLine.SetPosition(i, point);
        }
    }
}