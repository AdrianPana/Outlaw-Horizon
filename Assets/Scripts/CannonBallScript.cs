using UnityEngine;

public class CannonBallScript : MonoBehaviour
{
    [SerializeField] float speed = 20f;
    [SerializeField] float arcAngle = 30f;
    [SerializeField] float lifetime = 5f;

    private Rigidbody rb;
    private Collider col;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        col = GetComponent<Collider>();
        col.enabled = false;

        Vector3 direction = Quaternion.Euler(-arcAngle, 0f, 0f) * transform.forward;
        rb.linearVelocity = direction * speed;

        Invoke(nameof(EnableCollider), 1f);
        Destroy(gameObject, lifetime);
    }

    void EnableCollider() => col.enabled = true;
}