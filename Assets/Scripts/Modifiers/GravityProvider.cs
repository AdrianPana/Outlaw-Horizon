using UnityEngine;
using Game.Resources;
using UnityEditor;

namespace Game.Modifiers
{
    public class GravityProvider : MonoBehaviour, IModifierProvider
    {
        [SerializeField] private UniversalStateManagerScriptableObject stateManager;
        private bool isGravityInverted;

        private void Reset()
        {
#if UNITY_EDITOR
            if (stateManager == null)
            {
                stateManager = AssetDatabase.LoadAssetAtPath<UniversalStateManagerScriptableObject>("Assets/Scripts/UniversalStateManagerScriptableObject.asset");
            }
#endif
        }
        private void OnEnable()
        {
            if (stateManager != null)
                stateManager.gravityInvertedEvent.AddListener(OnGravityChanged);
        }

        private void OnDisable()
        {
            if (stateManager != null)
                stateManager.gravityInvertedEvent.RemoveListener(OnGravityChanged);
        }

        private void OnGravityChanged((Vector3 origin, float range, bool inverted, GameObject target) data)
        {
            if (data.target != null)
            {
                if (this.gameObject == data.target)
                {
                    isGravityInverted = data.inverted;
                }
            }
            else if (OH_Helpers.isInRangeNoHeight(transform.position, data.origin, data.range))
            {
                isGravityInverted = data.inverted;
            }
        }

        public Vector3 GetForceContribution(Vector3 currentPos)
        {
            if (!isGravityInverted) return Vector3.zero;
            // When inverted, we want to fly UP. 
            // Standard gravity is down, so we return an upward force that overrides or negates it.
            // Since we are overriding gravity in the controller, we just return the desired direction.
            return Vector3.up;
        }

        public bool IsActiveOnObject(Vector3 currentPos)
        {
            return isGravityInverted;
        }

        public Modifier GetModifier()
        {
            return isGravityInverted ? Modifier.GRAVITY_INVERTED : Modifier.NONE;
        }
    }
}
