using UnityEngine;

[RequireComponent(typeof(Collider))]
public class DamageObject : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        GloveScript glove = other.GetComponent<GloveScript>();
        if (glove != null)
        {
            glove.ResetScene();
        }
    }
}
