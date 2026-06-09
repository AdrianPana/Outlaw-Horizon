using UnityEngine;
using Game.Resources;

namespace Game.Modifiers
{
    public class GravityProvider : MonoBehaviour, IModifierProvider
    {
        [SerializeField] private UniversalStateManagerScriptableObject stateManager;
        public float gravityStrength = 9.81f / 2f;
        private bool isGravityInverted;

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
            return Vector3.up * gravityStrength;
        }

        public bool IsActiveOnObject(Vector3 currentPos)
        {
            return isGravityInverted;
        }
    }
}
