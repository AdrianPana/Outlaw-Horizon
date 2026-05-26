// AgentTestController.cs
using StarterAssets;
using UnityEngine;
using UnityEngine.InputSystem;

public class AgentTestController : MonoBehaviour
{
    [Header("References")]
    public AgentBT behaviorTree;

    private InputSystem_Actions playerInputActions;

    private void Awake()
    {
        playerInputActions = new InputSystem_Actions();
    }

    private void OnEnable()
    {
        playerInputActions.Enable();
        playerInputActions.Player.Gravity.performed += ToggleType;
        playerInputActions.Player.WindNorth.performed += DealDamage;
        playerInputActions.Player.WindEast.performed += HealDamage;
    }

    private void OnDisable()
    {
        playerInputActions.Disable();
    }

    private void ToggleType(InputAction.CallbackContext ctx)
    {
        behaviorTree.type = behaviorTree.type == EnemyType.MELEE ? EnemyType.RANGED : EnemyType.MELEE;
        Debug.Log("TEST: " + behaviorTree.type);
    }

    private void DealDamage(InputAction.CallbackContext ctx)
    {
        behaviorTree.hp = Mathf.Max(0, behaviorTree.hp - 1);
        Debug.Log("TEST: HP is now " + behaviorTree.hp);
    }

    private void HealDamage(InputAction.CallbackContext ctx)
    {
        behaviorTree.hp = Mathf.Min(10, behaviorTree.hp + 1);
        Debug.Log("TEST: HP is now " + behaviorTree.hp);
    }
}