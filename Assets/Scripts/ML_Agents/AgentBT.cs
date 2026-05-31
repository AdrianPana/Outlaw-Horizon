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
    private bool _isInvulnerable = false;

    private void Awake()
    {
        _agent = GetComponent<MoveToGoalAgent>();
        _controller = GetComponent<AIController>();
    }

    private void Update()
    {
        if (player == null || exit == null) return;
        if (_isAttacking) return;

        float attackRange = type == EnemyType.MELEE ? meleeRange : rangedRange;
        float distanceToPlayer = Vector3.Distance(transform.position, player.position);

        if (hp < 5)
        {
            SetState(BehaviorState.RETREATING);
            _agent.SetTarget(exit);
            _agent.EnableInference(true);
            return;
        }

        if (distanceToPlayer <= attackRange)
        {
            SetState(BehaviorState.ATTACKING);
            StartCoroutine(AttackRoutine());
            return;
        }

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
    }

    private IEnumerator TakeDamageRoutine(int damage)
    {
        _isInvulnerable = true;
        hp = Mathf.Max(0, hp - damage);
        Debug.Log("Took " + damage + " damage. HP is now " + hp);

        yield return new WaitForSeconds(2f);

        _isInvulnerable = false;
        Debug.Log("Invulnerability ended");
    }

    private void SetState(BehaviorState newState)
    {
        if (currentState != newState)
        {
            currentState = newState;
            Debug.Log("State changed to: " + newState);
        }
    }

    private void OnCollisionEnter(Collision collision)

    {
        if (collision.gameObject.TryGetComponent<DamageObject>(out _))
        {
            if (!_isInvulnerable)
                StartCoroutine(TakeDamageRoutine(3));
        }
    }
}