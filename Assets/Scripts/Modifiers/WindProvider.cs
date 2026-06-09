using UnityEngine;
using Game.Resources;

namespace Game.Modifiers
{
    public class WindProvider : MonoBehaviour, IModifierProvider
    {
        [SerializeField] private UniversalStateManagerScriptableObject stateManager;
        public float windStrength = 10f;
        private WindDirection currentWind = WindDirection.NONE;

        private void OnEnable()
        {
            if (stateManager != null)
                stateManager.windChangedEvent.AddListener(OnWindChanged);
        }

        private void OnDisable()
        {
            if (stateManager != null)
                stateManager.windChangedEvent.RemoveListener(OnWindChanged);
        }

        private void OnWindChanged((Vector3 origin, float range, WindDirection direction, GameObject target) data)
        {
            if (data.target != null)
            {
                if (this.gameObject == data.target)
                {
                    currentWind = data.direction;
                }
            }
            else if (OH_Helpers.isInRangeNoHeight(transform.position, data.origin, data.range))
            {
                currentWind = data.direction;
            }
        }

        public Vector3 GetForceContribution(Vector3 currentPos)
        {
            switch (currentWind)
            {
                case WindDirection.NORTH: return Vector3.forward * windStrength;
                case WindDirection.EAST: return Vector3.right * windStrength;
                case WindDirection.SOUTH: return Vector3.back * windStrength;
                case WindDirection.WEST: return Vector3.left * windStrength;
                case WindDirection.NORTH_EAST: return (Vector3.forward + Vector3.right).normalized * windStrength;
                case WindDirection.SOUTH_EAST: return (Vector3.back + Vector3.right).normalized * windStrength;
                case WindDirection.SOUTH_WEST: return (Vector3.back + Vector3.left).normalized * windStrength;
                case WindDirection.NORTH_WEST: return (Vector3.forward + Vector3.left).normalized * windStrength;   
                default: return Vector3.zero;
            }
        }

        public bool IsActiveOnObject(Vector3 currentPos)
        {
            return currentWind != WindDirection.NONE;
        }
    }
}
