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
    [SerializeField] private Material progressMaterial;
    [SerializeField] private Material loseMaterial;
    [SerializeField] private MeshRenderer floorMeshRenderer;

    private float _previousDistance;
    private int goalMoves;
    private bool _goalReached;
    private bool _inferenceEnabled = true;
    public override void Initialize()
    {
        _controller = GetComponent<AIController>();
        _controller.OnLedge = false;
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
        goalMoves = 0;
        _goalReached = false;
        targetTransform = target.transform;

        if (!isTraining) return;

        transform.localPosition = new Vector3(Random.Range(1.8f, 4.6f), 0.3f, Random.Range(-3f, -9f));
        transform.localRotation = Quaternion.Euler(0, 90, 0);

        PlaceGoal();

        _previousDistance = Vector3.Distance(transform.localPosition, targetTransform.localPosition);
    }

    public override void CollectObservations(VectorSensor sensor)
    {
        Vector3 directionToGoal = targetTransform.localPosition - transform.localPosition;
        sensor.AddObservation(directionToGoal);
        sensor.AddObservation(directionToGoal.magnitude);
        sensor.AddObservation(_controller.OnLedge);
        sensor.AddObservation(_controller.Grounded);
        sensor.AddObservation(_controller._verticalVelocity);
    }

    public override void OnActionReceived(ActionBuffers action)
    {
        if (!_inferenceEnabled)
        {
            _controller.aiMoveInput = Vector2.zero;
            _controller.aiJump = false;
            return;
        }

        float moveX = action.ContinuousActions[0];
        float moveZ = action.ContinuousActions[1];

        _controller.aiMoveInput = new Vector2(moveX, moveZ);
        _controller.aiJump = action.DiscreteActions[0] > 0;
        if (_controller.aiJump)
        {
            AddReward(-0.05f);
        }

        float currentDistance = Vector3.Distance(transform.localPosition, targetTransform.localPosition);

        if (currentDistance < 1.5f)
        {
            ReachGoal();
        }

        float distanceDelta = _previousDistance - currentDistance;
        AddReward(distanceDelta * 0.1f);
        _previousDistance = currentDistance;

        AddReward(-0.001f);
    }

    public override void Heuristic(in ActionBuffers actionsOut)
    {
        actionsOut.ContinuousActions.Array[0] = _input.move.x;
        actionsOut.ContinuousActions.Array[1] = _input.move.y;
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
                other.transform.localPosition = player.transform.localPosition + new Vector3(0, 0.5f, 0);
                targetTransform.localPosition = player.transform.localPosition + new Vector3(0, 0.5f, 0);
                //EndEpisode();
                return;
            }

            ReachGoal();
        }
        else if (other.TryGetComponent<DamageObject>(out _))
        {
            if (!isTraining) return;

            AddReward(-1f);
            floorMeshRenderer.material = loseMaterial;
            EndEpisode();
        }
    }

    private void ReachGoal()
    {
        if (!isTraining)
        {
            var player = GameObject.FindGameObjectWithTag("Player");
            targetTransform.localPosition = player.transform.localPosition + new Vector3(0, 0.5f, 0);
            //EndEpisode();
            return;
        }

        if (_goalReached) return;
        _goalReached = true;

        goalMoves++;
        Debug.Log("Goal reached! Total moves: " + goalMoves);
        AddReward(1f);

        if (goalMoves >= 5)
        {
            floorMeshRenderer.material = winMaterial;
            EndEpisode();
            return;
        }

        floorMeshRenderer.material = progressMaterial;
        PlaceGoal();
        _previousDistance = Vector3.Distance(transform.localPosition, targetTransform.localPosition);
        _goalReached = false;
    }

    //private void PlaceGoal()
    //{
    //    int version = Random.Range(0, 4);
    //    switch (version)
    //    {
    //        case 0:
    //            targetTransform.localPosition = new Vector3(Random.Range(10f, 13f), 1.5f, Random.Range(-13f, 1f));
    //            break;
    //        case 1:
    //            targetTransform.localPosition = new Vector3(Random.Range(-2f, 0f), 1.5f, Random.Range(-13f, 1f));
    //            break;
    //        case 2:
    //            targetTransform.localPosition = new Vector3(Random.Range(0f, 11f), 1.5f, Random.Range(-2f, 1f));
    //            break;
    //        case 3:
    //            targetTransform.localPosition = new Vector3(Random.Range(0f, 11f), 1.5f, Random.Range(-10f, -13f));
    //            break;
    //    }
    //}
    
    private void PlaceGoal()
    {
        int version = Random.Range(0, 4);
        switch (version)
        {
            case 0:
                targetTransform.localPosition = new Vector3(Random.Range(10f, 13f), 3.2f, Random.Range(-13f, 1f));
                break;
            case 1:
                targetTransform.localPosition = new Vector3(Random.Range(-2f, 0f), 5f, Random.Range(-13f, 1f));
                break;
            case 2:
                targetTransform.localPosition = new Vector3(Random.Range(2f, 9f), 3.2f, Random.Range(-2f, 1f));
                break;
            case 3:
                targetTransform.localPosition = new Vector3(Random.Range(2f, 9f), 1.5f, Random.Range(-10f, -13f));
                break;
        }
    }

    public void SetTarget(Transform newTarget)
    {
        targetTransform = newTarget;
    }

    public void EnableInference(bool enable)
    {
        _inferenceEnabled = enable;
        if (!enable)
        {
            _controller.aiMoveInput = Vector2.zero;
            _controller.aiJump = false;
        }
    }
}