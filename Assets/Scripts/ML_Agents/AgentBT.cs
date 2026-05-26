// AgentBehaviorTree.cs
using UnityEngine;
using System.Collections;
using StarterAssets;

public enum EnemyType { MELEE, RANGED }

public class AgentBT : MonoBehaviour
{
    [Header("Stats")]
    public EnemyType type = EnemyType.MELEE;
    [Range(0, 10)] public int hp = 10;

    [Header("References")]
    public Transform player;
    public Transform exit;

    [Header("Range Settings")]
    public float meleeRange = 1.5f;
    public float rangedRange = 5.0f;

    private MoveToGoalAgent _agent;
    private AIController _controller;

    public enum BehaviorState { FOLLOWING, ATTACKING, RETREATING }
    public BehaviorState currentState = BehaviorState.FOLLOWING;

    private bool _isAttacking = false;

    private void Awake()
    {
        _agent = GetComponent<MoveToGoalAgent>();
        _controller = GetComponent<AIController>();
    }

    private void Update()
    {
        if (player == null || exit == null) return;
        if (_isAttacking) return; // don't interrupt an ongoing attack

        float attackRange = type == EnemyType.MELEE ? meleeRange : rangedRange;
        float distanceToPlayer = Vector3.Distance(transform.position, player.position);

        // Priority 1: retreat if low hp
        if (hp < 5)
        {
            SetState(BehaviorState.RETREATING);
            _agent.SetTarget(exit);
            _agent.EnableInference(true);
            return;
        }

        // Priority 2: attack if in range
        if (distanceToPlayer <= attackRange)
        {
            SetState(BehaviorState.ATTACKING);
            StartCoroutine(AttackRoutine());
            return;
        }

        // Priority 3: follow player
        SetState(BehaviorState.FOLLOWING);
        _agent.SetTarget(player);
        _agent.EnableInference(true);
    }

    private IEnumerator AttackRoutine()
    {
        _isAttacking = true;
        _agent.EnableInference(false);

        if (type == EnemyType.MELEE)
            Debug.Log("MELEE ATTACK");
        else
            Debug.Log("RANGED ATTACK");

        yield return new WaitForSeconds(2f);

        _isAttacking = false;
        // inference will be re-enabled on the next Update cycle
    }

    private void SetState(BehaviorState newState)
    {
        if (currentState != newState)
        {
            currentState = newState;
            Debug.Log("State changed to: " + newState);
        }
    }
}