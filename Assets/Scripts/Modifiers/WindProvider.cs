using UnityEngine;
using Game.Resources;
#if UNITY_EDITOR
using UnityEditor;
#endif


namespace Game.Modifiers
{
    public class WindProvider : MonoBehaviour, IModifierProvider
    {
        [SerializeField] private UniversalStateManagerScriptableObject stateManager;
        private WindDirection currentWind = WindDirection.NONE;

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
                case WindDirection.NORTH: return Vector3.forward;
                case WindDirection.EAST: return Vector3.right;
                case WindDirection.SOUTH: return Vector3.back;
                case WindDirection.WEST: return Vector3.left;
                case WindDirection.NORTH_EAST: return (Vector3.forward + Vector3.right).normalized;
                case WindDirection.SOUTH_EAST: return (Vector3.back + Vector3.right).normalized;
                case WindDirection.SOUTH_WEST: return (Vector3.back + Vector3.left).normalized;
                case WindDirection.NORTH_WEST: return (Vector3.forward + Vector3.left).normalized;   
                default: return Vector3.zero;
            }
        }

        public bool IsActiveOnObject(Vector3 currentPos)
        {
            return currentWind != WindDirection.NONE;
        }

        public Modifier GetModifier() 
        {
            return ResourceHelper.WindDirectionToModifier(currentWind);
        }
    }
}
