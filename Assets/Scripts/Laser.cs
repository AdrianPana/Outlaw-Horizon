using UnityEngine;

[RequireComponent(typeof(LineRenderer))]
public class Laser : MonoBehaviour
{
    public float maxDistance = 20f;
    LineRenderer line;
    LayerMask layerMask;
    Vector3 laserOrigin;

    void Awake()
    {
        laserOrigin = transform.position + Vector3.up * 1f; // Adjust the offset as needed
        line = GetComponent<LineRenderer>();
        line.positionCount = 2;
        line.startWidth = line.endWidth = 0.05f;
    }

    private void OnValidate()
    {
        laserOrigin = transform.position + Vector3.up * 1f;
        Vector3 start = laserOrigin;
        Vector3 end = start + transform.forward * maxDistance;

        if (line == null)
        {
            line = GetComponent<LineRenderer>();
            line.positionCount = 2;
            line.startWidth = line.endWidth = 0.05f;
        }
        line.SetPosition(0, start);
        line.SetPosition(1, end);
    }

    private void Start()
    {
        layerMask = ~LayerMask.GetMask("Laser");
    }

    void Update()
    {
        Vector3 start = laserOrigin;
        Vector3 end = start + transform.forward * maxDistance;

        if (Physics.Raycast(laserOrigin, transform.forward, out RaycastHit hit, maxDistance, layerMask))
        {
            end = hit.point;
            
            if (hit.collider.gameObject.TryGetComponent<GloveScript>(out var gloveScript))
            {
                gloveScript.ResetScene();
            }
        }

        line.SetPosition(0, start);
        line.SetPosition(1, end);
    }
}
