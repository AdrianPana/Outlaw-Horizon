using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;
using StarterAssets;

public class MoveToGoalAgent : Agent
{
    private AIController _controller;
    private StarterAssetsInputs _input;
    [SerializeField] private GameObject target;
    private Transform targetTransform;

    [SerializeField] private bool isTraining = false;

    [Header("Win/Lose state")]
    [SerializeField] private Material winMaterial;
    [SerializeField] private Material loseMaterial;
    [SerializeField] private MeshRenderer floorMeshRenderer;

    private float _previousDistance;

    public override void Initialize()
    {
        _controller = GetComponent<AIController>();
        _input = GetComponent<StarterAssetsInputs>();
    }

    private void Update()
    {
        if (target == null)
        {
            target = GameObject.FindGameObjectWithTag("Goal");
            if (target != null)
                targetTransform = target.transform;
        }
    }

    public override void OnEpisodeBegin()
    {
        if (!isTraining) return;

        //if (Random.value > 0.5f)
            transform.localPosition = new Vector3(Random.Range(1.8f, 4.6f) , 0.3f, Random.Range(-3f, -9f));
        //else
        //    transform.localPosition = new Vector3(Random.Range(-2f, 0f), 1.5f, Random.Range(-13f, 1f));

        // Reset agent and target positions
        transform.localRotation = Quaternion.Euler(0, 90, 0);

        //transform.localPosition = new Vector3(68, 0.3f, -13);
        int version = Random.Range(0, 4);
        switch (version)
        {
            case 0:
                targetTransform.localPosition = new Vector3(Random.Range(10f, 13f), 1.5f, Random.Range(-13f, 1f));
                break;
            case 1:
                targetTransform.localPosition = new Vector3(Random.Range(-2f, 0f), 1.5f, Random.Range(-13f, 1f));
                break;
            case 2:
                targetTransform.localPosition = new Vector3(Random.Range(0f, 11f), 1.5f, Random.Range(-2f, 1f));
                break;
            case 3:
                targetTransform.localPosition = new Vector3(Random.Range(0f, 11f), 1.5f, Random.Range(-10f, -13f));
                break;
        }

        _previousDistance = Vector3.Distance(transform.localPosition, targetTransform.localPosition);
    }

    public override void CollectObservations(VectorSensor sensor)
    {
        Vector3 directionToGoal = targetTransform.position - transform.position;
        sensor.AddObservation(directionToGoal);
        sensor.AddObservation(directionToGoal.magnitude);
        sensor.AddObservation(_controller.OnLedge);
        sensor.AddObservation(_controller.Grounded);
        sensor.AddObservation(_controller._verticalVelocity);
    }

    public override void OnActionReceived(ActionBuffers action)
    {
        float moveX = action.ContinuousActions[0];
        float moveZ = action.ContinuousActions[1];

        _controller.aiMoveInput = new Vector2(moveX, moveZ);
        _controller.aiJump = action.DiscreteActions[0] > 0;

        // reward for getting closer
        //float currentDistance = Vector3.Distance(transform.position, targetTransform.position);
        //float distanceDelta = _previousDistance - currentDistance;
        //AddReward(distanceDelta * 0.1f);
        //_previousDistance = currentDistance;

        AddReward(-0.001f); // small time penalty
    }

    public override void Heuristic(in ActionBuffers actionsOut)
    {
        actionsOut.ContinuousActions.Array[0] = _input.move.x;
        actionsOut.ContinuousActions.Array[1] = _input.move.y;
        Debug.Log(_input.jump);
        actionsOut.DiscreteActions.Array[0] = _input.jump ? 1 : 0;
        _input.jump = false;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Goal"))
        {
            if (!isTraining)
            {
                var player = GameObject.FindGameObjectWithTag("Player");
                other.transform.position = player.transform.position + new Vector3(0, 0.5f, 0);
                return;
            }

            SetReward(1f);
            floorMeshRenderer.material = winMaterial;
            EndEpisode();
        }
        else if (other.TryGetComponent<DamageObject>(out _))
        {
            if (!isTraining)
            {
                return;
            }

            SetReward(-1f);
            floorMeshRenderer.material = loseMaterial;
            EndEpisode();
        }
    }
}
