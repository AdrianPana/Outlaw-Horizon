using System.Linq;
using UnityEngine;
using UnityEngine.XR;

public class FloatScript : MonoBehaviour
{
    public float depthBeforeSubmerged = 1.0f;
    public float displacementAmount = 3.0f;
    public float waterDrag = 0.99f;
    public float waterAngularDrag = 0.5f;

    private Rigidbody rb;
    private int floatingPointsCount = 1;

    private void Start()
    {
        rb = GetComponent<Rigidbody>();
        if (rb == null)
        {
            rb = GetComponentInParent<Rigidbody>();
        }

        floatingPointsCount = transform.parent.GetComponentsInChildren<FloatScript>().Count();
    }

    private void FixedUpdate()
    {
        rb.AddForceAtPosition(Physics.gravity / floatingPointsCount, transform.position, ForceMode.Acceleration);

        float waveHeight = WaveManager.Instance.GetWaveHeight(transform.position);
        if (transform.position.y < waveHeight)
        {
            float displacementMultiplier = Mathf.Clamp01(waveHeight - transform.position.y / depthBeforeSubmerged) * displacementAmount;
            rb.AddForceAtPosition(new Vector3(0f, Mathf.Abs(Physics.gravity.y) * displacementMultiplier, 0f), transform.position, ForceMode.Acceleration);
            rb.AddForce(displacementMultiplier * -rb.linearVelocity * waterDrag * Time.deltaTime, ForceMode.VelocityChange);
            rb.AddTorque(displacementMultiplier * -rb.angularVelocity * waterAngularDrag * Time.deltaTime, ForceMode.VelocityChange);
        }
    }
}
