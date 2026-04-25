using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;
using StarterAssets;

public class MoveToGoalAgent : Agent
{
    private AIController _controller;
    private StarterAssetsInputs _input;
    [SerializeField] private Transform targetTransform;

    [Header("Win/Lose state")]
    [SerializeField] private Material winMaterial;
    [SerializeField] private Material loseMaterial;
    [SerializeField] private MeshRenderer floorMeshRenderer;

    public override void Initialize()
    {
        _controller = GetComponent<AIController>();
        _input = GetComponent<StarterAssetsInputs>();
    }

    public override void OnEpisodeBegin()
    {
        // Reset agent and target positions
        transform.localPosition = new Vector3(3 , 0.3f, -6);
        //transform.localPosition = new Vector3(68, 0.3f, -13);
        transform.localRotation = Quaternion.Euler(0, 90, 0);
        //targetTransform.position = new Vector3(74, 0f, -14);
    }

    public override void CollectObservations(VectorSensor sensor)
    {
        sensor.AddObservation(transform.localPosition);
        sensor.AddObservation(targetTransform.localPosition);
    }

    public override void OnActionReceived(ActionBuffers action)
    {
        float moveX = action.ContinuousActions[0];
        float moveZ = action.ContinuousActions[1];

        _controller.aiMoveInput = new Vector2(moveX, moveZ);
        _controller.aiJump = action.DiscreteActions[0] > 0;
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
            SetReward(1f);
            floorMeshRenderer.material = winMaterial;
            EndEpisode();
        }
        else if (other.TryGetComponent<DamageObject>(out _))
        {
            SetReward(-1f);
            floorMeshRenderer.material = loseMaterial;
            EndEpisode();
        }
    }
}
