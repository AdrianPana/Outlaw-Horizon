using UnityEngine;

namespace StarterAssets
{
    [RequireComponent(typeof(Rigidbody))]
    public class Rideable : MonoBehaviour
    {
        private Rigidbody _rb;
        public Vector3 Velocity { get; private set; }

        private Vector3 _lastPosition;

        void Awake()
        {
            _rb = GetComponent<Rigidbody>();
        }

        private void Start()
        {
            _lastPosition = _rb.position;
        }

        void FixedUpdate()
        {
            Vector3 current = _rb.position;
            Velocity = (current - _lastPosition) / Time.fixedDeltaTime;
            _lastPosition = current;
        }

        //private void OnTriggerEnter(Collider other)
        //{
        //    if (other.CompareTag("Player"))
        //    {
        //        other.gameObject.GetComponent<ThirdPersonController>()._ridable = this;
        //    }
        //}

        //private void OnTriggerExit(Collider other)
        //{
        //    if (other.CompareTag("Player"))
        //    {
        //        other.gameObject.GetComponent<ThirdPersonController>()._ridable = null;
        //    }
        //}
    }
}
